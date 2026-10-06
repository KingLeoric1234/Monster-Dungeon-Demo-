using UnityEngine;
using Game.Core;

namespace Game.Enemy
{
    public enum ChargerState { Patrol, Windup, Charging, Recovering }

    public class ChargingEnemyAI : MonoBehaviour
    {
        [Header("冲锋参数（从MonsterConfig读）")]
        private float chargeSpeed;
        private float windupTime;
        private float chargeDuration;
        private float recoverTime;
        [SerializeField] private float wallStopDistance = 0.3f;
        [SerializeField] private float stuckCooldown = 0.5f;
        [SerializeField] private float stuckMoveThreshold = 0.2f;

        private MonsterTemplate template;
        private Transform player;
        private ChargerState state;
        private float timer;
        private Vector2 chargeDir;
        private float lastPos;
        private float stuckCooldownTimer;
        private float chargeCooldownTimer;  // 冲锋CD倒计时
        private KnockBackHandler knockBack;
        private Vector2 patrolDir;      // 巡逻方向（数据流封在本类内，不再依赖 EnemyMovement）
        private float patrolTimer;      // 巡逻换向倒计时
        private int obstacleMask;
        private SpriteRenderer sprite;
        private Color originalColor;

        public void Initialize(MonsterTemplate t, Transform p)
        {
            template = t;
            player = p;
            state = ChargerState.Patrol;
            // 从模板读冲锋参数
            chargeSpeed = t.chargeSpeed;
            windupTime = t.windupTime;
            chargeDuration = t.chargeDuration;
            recoverTime = t.recoverTime;
            // 出生自带CD，不会一上来就冲锋
            chargeCooldownTimer = t.chargeCooldown;
            obstacleMask = LayerMask.GetMask("Obstacle");
            knockBack = GetComponent<KnockBackHandler>();
            sprite = GetComponent<SpriteRenderer>();
            if (sprite != null) originalColor = sprite.color;
        }

        private void Update()
        {
            if (template == null || player == null) return;
            if (knockBack != null && knockBack.IsKnockedBack) return;

            if (stuckCooldownTimer > 0) stuckCooldownTimer -= Time.deltaTime;
            if (chargeCooldownTimer > 0) chargeCooldownTimer -= Time.deltaTime;

            switch (state)
            {
                case ChargerState.Patrol: UpdatePatrol(); break;
                case ChargerState.Windup: UpdateWindup(); break;
                case ChargerState.Charging: UpdateCharging(); break;
                case ChargerState.Recovering: UpdateRecovering(); break;
            }
        }

        private void UpdatePatrol()
        {
            // 巡逻：数据流封在本类内（随机方向 + 避障移动），不再依赖 EnemyMovement
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0f)
            {
                patrolDir = Random.insideUnitCircle.normalized;
                patrolTimer = Random.Range(template.patrolMinTime, template.patrolMaxTime);
            }

            float patrolSpeed = template.moveSpeed * template.patrolSpeedRatio;
            Vector2 nextPos = (Vector2)transform.position + patrolDir * patrolSpeed * Time.deltaTime;
            if (Physics2D.OverlapCircle(nextPos, 0.6f, obstacleMask) == null)
            {
                transform.position = nextPos;
            }
            else
            {
                // 撞墙：立刻换方向
                patrolDir = Random.insideUnitCircle.normalized;
                patrolTimer = Random.Range(template.patrolMinTime, template.patrolMaxTime);
            }

            float dist = Vector2.Distance(transform.position, player.position);
            // CD中或玩家不在范围内，继续巡逻
            if (dist >= template.aggroRange || chargeCooldownTimer > 0f || stuckCooldownTimer > 0f)
                return;

            // 玩家在范围内，先锁定坐标+射线检测
            chargeDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            float chargeDistance = chargeSpeed * chargeDuration;
            bool wallInPath = Physics2D.Raycast(
                transform.position, chargeDir, chargeDistance + 1f, obstacleMask).collider != null;

            if (wallInPath)
            {
                // 路径有墙，进CD，继续巡逻
                chargeCooldownTimer = template.chargeCooldown;
                return;
            }

            // 路径没墙，进蓄力
            state = ChargerState.Windup;
            timer = windupTime;
        }

        private void UpdateWindup()
        {
            // 蓄力时停住，不动（移动只在 Patrol 状态发生，进入 Windup 自然停住）
            timer -= Time.deltaTime;

            // 蓄力三阶段颜色提示
            if (sprite != null)
            {
                float progress = 1f - (timer / windupTime);
                Color warnColor;
                float flashSpeed;

                if (progress < 0.5f)       // 前50%：蓝色慢闪
                {
                    warnColor = Color.blue;
                    flashSpeed = 4f;
                }
                else if (progress < 0.9f)  // 中间40%：黄色中闪
                {
                    warnColor = Color.yellow;
                    flashSpeed = 8f;
                }
                else                        // 最后10%：红色快闪
                {
                    warnColor = Color.red;
                    flashSpeed = 16f;
                }

                float pulse = Mathf.PingPong(Time.time * flashSpeed, 1f);
                sprite.color = Color.Lerp(warnColor, originalColor, pulse);
            }

            if (timer <= 0)
            {
                state = ChargerState.Charging;
                timer = chargeDuration;
                if (sprite != null) sprite.color = originalColor;
            }
        }

        private void UpdateCharging()
        {
            timer -= Time.deltaTime;
            Vector2 nextPos = (Vector2)transform.position + chargeDir * chargeSpeed * Time.deltaTime;
            // 雷达：下一步落点有墙就停
            if (Physics2D.OverlapCircle(nextPos, 0.6f, obstacleMask) == null)
            {
                transform.position = nextPos;
            }
            else
            {
                // 撞墙，立刻进硬直
                state = ChargerState.Recovering;
                timer = recoverTime;
                lastPos = transform.position.x;
                if (sprite != null) sprite.color = originalColor;
                return;
            }

            if (timer <= 0)
            {
                state = ChargerState.Recovering;
                timer = recoverTime;
                lastPos = transform.position.x;
            }
        }

        private void UpdateRecovering()
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                float moved = Mathf.Abs(transform.position.x - lastPos);
                bool stuck = moved < stuckMoveThreshold;
                if (stuck) stuckCooldownTimer = stuckCooldown;
                // 进CD，CD期间巡逻
                chargeCooldownTimer = template.chargeCooldown;
                state = ChargerState.Patrol;
            }
        }
    }
}
