using UnityEngine;
using Game.Core;

namespace Game.Player
{
    /// <summary>
    /// 子弹：从玩家射出，朝鼠标方向飞行，碰到怪物扣血，超过射程自动销毁。
    /// 挂在子弹预制体上。
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [Header("子弹设置")]
        [SerializeField] private float speed = 15f;       // 子弹飞行速度
        [SerializeField] private float maxRange = 10f;    // 最大射程
        [SerializeField] private float maxLifetime = 3f;  // 最大存在时间（秒），防止无距离限制的子弹永远存在
        [SerializeField] private float critChance = 0.04f; // 暴击率
        [SerializeField] private float critMultiplier = 1.5f; // 暴击伤害倍率

        private Vector2 startPos;
        private Vector2 direction;
        private int totalDamage;  // 总伤害（枪伤害+子弹伤害）
        private bool isInitialized = false;
        private float lifetime = 0f;  // 已存在时间
        private SpriteRenderer spriteRenderer;  // 子弹图片渲染器
        private TrailRenderer trailRenderer;    // 彗尾渲染器

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            spriteRenderer.sortingOrder = 10;

            trailRenderer = GetComponent<TrailRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
            }
            // 默认隐藏彗尾，发射时再设置
            trailRenderer.enabled = false;
        }

        private void Update()
        {
            if (!isInitialized) return;

            // 累加存在时间
            lifetime += Time.deltaTime;

            // 朝方向飞行
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

            // 超过射程或超过最大存在时间，自动销毁
            if (Vector2.Distance(startPos, transform.position) >= maxRange || lifetime >= maxLifetime)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>初始化子弹（发射时调用）</summary>
        public void Initialize(Vector2 dir, int damage, float range, float bulletSpeed, 
            Sprite bulletSprite = null, Color bulletColor = default,
            float trailTime = 0f, float trailStartWidth = 0f)
        {
            direction = dir.normalized;
            totalDamage = damage;
            maxRange = range;
            speed = bulletSpeed;
            startPos = transform.position;
            isInitialized = true;

            // 设置子弹图片（如果传了的话）
            // Debug.Log("[Bullet] Initialize: bulletSprite=" + (bulletSprite != null ? bulletSprite.name : "null") + " spriteRenderer=" + (spriteRenderer != null));
            if (bulletSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = bulletSprite;
            }

            // 不管有没有传图片，都要设置颜色
            // Debug.Log("[Bullet] bulletColor=" + bulletColor + " spriteRenderer=" + (spriteRenderer != null));
            if (spriteRenderer != null)
            {
                spriteRenderer.color = bulletColor == default ? Color.white : bulletColor;
            }

            // 设置彗尾（如果传了参数就启用）
            // Debug.Log("[Bullet] trailRenderer=" + (trailRenderer != null) + " trailTime=" + trailTime + " trailStartWidth=" + trailStartWidth);
            if (trailRenderer != null && trailTime > 0 && trailStartWidth > 0)
            {
                trailRenderer.enabled = true;
                trailRenderer.time = trailTime;
                trailRenderer.startWidth = trailStartWidth;
                trailRenderer.endWidth = 0f;
                trailRenderer.startColor = bulletColor == default ? Color.white : bulletColor;
                trailRenderer.endColor = new Color(
                    (bulletColor == default ? Color.white : bulletColor).r,
                    (bulletColor == default ? Color.white : bulletColor).g,
                    (bulletColor == default ? Color.white : bulletColor).b,
                    0f);
                // 用Sprites/Default材质，支持透明
                trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
                // 排序层级跟SpriteRenderer一致
                trailRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                trailRenderer.sortingOrder = spriteRenderer.sortingOrder;
                // 清除上一颗子弹残留的彗尾
                trailRenderer.Clear();
                // Debug.Log($"[Bullet] 彗尾已启用: time={trailTime}, width={trailStartWidth}, color={spriteRenderer.color}");
            }
            else if (trailRenderer != null)
            {
                trailRenderer.enabled = false;
            }

            // 子弹朝向飞行方向
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        /// <summary>碰到任何Collider2D时自动触发。检查是不是Enemy，是就扣血+销毁子弹。</summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 子弹还没初始化完不响应
            if (!isInitialized) return;

            //Debug.Log("[Bullet] 碰到=" + other.gameObject.name + " Tag=" + other.tag);

            // 只有碰到Enemy才造成伤害
            if (other.CompareTag("Enemy"))
            {
                // 伤害随机浮动（从PlayerConfig读）
                float alpha = Random.Range(PlayerConfig.DamageMinMultiplier, PlayerConfig.DamageMaxMultiplier);
                float floatedDamage = totalDamage * alpha;
                int finalDamage = Mathf.CeilToInt(floatedDamage);

                // 判定暴击
                bool isCrit = Random.value < critChance;
                if (isCrit)
                {
                    finalDamage = Mathf.CeilToInt(finalDamage * critMultiplier);
                }

                // 发布伤害消息，CombatDirector订阅后执行扣血
                MessageBus.Publish(new PlayerAttackMessage
                {
                    Target = other.gameObject,
                    Direction = direction,
                    Damage = finalDamage,
                    IsCrit = isCrit
                });

                // 子弹碰到敌人后销毁
                Destroy(gameObject);
            }
            // 碰到障碍物也销毁
            else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                Destroy(gameObject);
            }
        }

        //private void OnDestroy()
        //{
        //    Debug.Log("[Bullet] 销毁!");
        //}
    }
}
