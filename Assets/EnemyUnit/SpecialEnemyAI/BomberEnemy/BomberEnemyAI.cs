using UnityEngine;
using Game.Core;

namespace Game.Enemy
{
    public enum BomberState { Patrol, Chase, Fuse, Explode }

    public class BomberEnemyAI : MonoBehaviour
    {
        [Header("自爆参数")]
        [SerializeField] private float fuseTime = 5f;
        [SerializeField] private float explodeRadius = 2f;
        [SerializeField] private int explodeDamage = 20;
        [SerializeField] private GameObject explosionParticlePrefab; // 爆炸粒子Prefab
        private LineRenderer circleLine;

        private MonsterTemplate template;
        private Transform player;
        private BomberState state;
        private float fuseTimer;
        private KnockBackHandler knockBack;
        private int obstacleMask;
        private SpriteRenderer sprite;
        private Color originalColor;

        public void Initialize(MonsterTemplate t, Transform p)
        {
            template = t;
            player = p;
            state = BomberState.Patrol;
            obstacleMask = LayerMask.GetMask("Obstacle");
            knockBack = GetComponent<KnockBackHandler>();
            sprite = GetComponent<SpriteRenderer>();
            if (sprite != null) originalColor = sprite.color;

            // 动态创建红色圆圈
            GameObject circleObj = new GameObject("ExplosionRange");
            circleObj.transform.SetParent(transform);
            circleObj.transform.localPosition = Vector3.zero;
            circleLine = circleObj.AddComponent<LineRenderer>();
            circleLine.useWorldSpace = false;
            circleLine.loop = true;
            circleLine.startWidth = 0.05f;
            circleLine.endWidth = 0.05f;
            circleLine.material = new Material(Shader.Find("Sprites/Default"));
            circleLine.startColor = new Color(1, 1, 0, 0.5f);
            circleLine.endColor = new Color(1, 1, 0, 0.5f);
            circleLine.positionCount = 32;
            for (int i = 0; i < 32; i++)
            {
                float angle = i * Mathf.PI * 2f / 32f;
                circleLine.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * explodeRadius);
            }
            circleLine.enabled = false;
        }

        private void Update()
        {
            if (template == null || player == null) return;
            if (knockBack != null && knockBack.IsKnockedBack) return;

            switch (state)
            {
                case BomberState.Patrol: UpdatePatrol(); break;
                case BomberState.Chase: UpdateChase(); break;
                case BomberState.Fuse: UpdateFuse(); break;
                case BomberState.Explode: /* 爆炸瞬间，下一帧销毁 */ break;
            }
        }

        private void UpdatePatrol()
        {
            if (circleLine != null) circleLine.enabled = false;

            float dist = Vector2.Distance(transform.position, player.position);
            if (dist < template.aggroRange)
            {
                state = BomberState.Chase;
            }
        }

        private void UpdateChase()
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist > template.aggroRange * 1.5f)
            {
                state = BomberState.Patrol;
                return;
            }

            // 查距离场选最短路方向，不可达回退直冲玩家
            Vector2 dir = GetPathDir();
            if (dir == Vector2.zero)
                dir = ((Vector2)player.position - (Vector2)transform.position).normalized;

            float moveDist = template.moveSpeed * Time.deltaTime;
            Vector2 nextPos = (Vector2)transform.position + dir * moveDist;

            if (Physics2D.OverlapCircle(nextPos, 0.7f, obstacleMask) == null)
            {
                transform.position = nextPos;
            }
            else
            {
                // 正面撞墙，试左右偏转
                Vector2 left = Quaternion.Euler(0, 0, 60) * dir;
                Vector2 leftPos = (Vector2)transform.position + left * moveDist;
                if (Physics2D.OverlapCircle(leftPos, 0.7f, obstacleMask) == null)
                {
                    transform.position = leftPos;
                }
                else
                {
                    Vector2 right = Quaternion.Euler(0, 0, -60) * dir;
                    Vector2 rightPos = (Vector2)transform.position + right * moveDist;
                    if (Physics2D.OverlapCircle(rightPos, 0.7f, obstacleMask) == null)
                        transform.position = rightPos;
                }
            }

            fuseTimer += Time.deltaTime;
            if (sprite != null) sprite.color = originalColor;
            if (fuseTimer >= fuseTime)
            {
                state = BomberState.Fuse;
                fuseTimer = fuseTime;
            }
        }

        private void UpdateFuse()
        {
            // 显示爆炸范围圈
            if (circleLine != null) circleLine.enabled = true;

            fuseTimer -= Time.deltaTime;

            // 引线阶段闪红，时间越短闪得越快
            if (sprite != null)
            {
                float progress = 1f - (fuseTimer / fuseTime); // 0~1
                float flashSpeed = Mathf.Lerp(1f, 4f, progress); // 1→4
                float pulse = Mathf.PingPong(Time.time * flashSpeed, 1f);
                sprite.color = Color.Lerp(new Color(1f, 0f, 0f), originalColor, pulse);
            }

            if (fuseTimer <= 0)
            {
                Explode();
            }
        }

        private void Explode()
        {
            // 爆炸粒子
            if (explosionParticlePrefab != null)
            {
                GameObject exp = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
                float scale = explodeRadius / 2f;
                exp.transform.localScale = Vector3.one * scale;

                // 代码设置颜色渐变：金黄→深黄→咸鸭蛋黄
                ParticleSystem ps = exp.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var col = ps.colorOverLifetime;
                    col.enabled = true;
                    Gradient g = new Gradient();
                    g.SetKeys(
                        new GradientColorKey[] {
                            new GradientColorKey(new Color(1f, 0.9f, 0.2f), 0f),    // 金黄
                            new GradientColorKey(new Color(1f, 0.3f, 0f), 0.5f),    // 橙红
                            new GradientColorKey(new Color(1f, 0f, 0f), 1f)         // 大红
                        },
                        new GradientAlphaKey[] {
                            new GradientAlphaKey(1f, 0f),
                            new GradientAlphaKey(1f, 0.7f),
                            new GradientAlphaKey(0f, 1f)
                        }
                    );
                    col.color = new ParticleSystem.MinMaxGradient(g);
                }
            }

            // 范围伤害
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
            foreach (Collider2D hit in hits)
            {
                if (!hit.CompareTag("Player")) continue;
                Player.PlayerHealthAction ph = hit.GetComponent<Player.PlayerHealthAction>();
                if (ph != null)
                {
                    ph.TakeDamage(explodeDamage);
                }
            }

            FindObjectOfType<EnemyDirector>()?.OnEnemyDied(gameObject);
            Destroy(gameObject);
        }

        private Vector2 GetPathDir()
        {
            if (PathfindingBFS.Instance == null || !PathfindingBFS.Instance.HasField) return Vector2.zero;
            if (GridWalkability.Instance == null) return Vector2.zero;

            Vector2Int g = GridWalkability.Instance.WorldToGrid(transform.position);
            int curD = PathfindingBFS.Instance.GetDistance(g.x, g.y);
            if (curD <= 0) return Vector2.zero;

            int bestX = 0, bestY = 0, bestD = curD;
            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };
            for (int i = 0; i < 4; i++)
            {
                int d = PathfindingBFS.Instance.GetDistance(g.x + dx[i], g.y + dy[i]);
                if (d >= 0 && d < bestD) { bestD = d; bestX = dx[i]; bestY = dy[i]; }
            }
            if (bestX == 0 && bestY == 0) return Vector2.zero;
            return new Vector2(bestX, bestY).normalized;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explodeRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}
