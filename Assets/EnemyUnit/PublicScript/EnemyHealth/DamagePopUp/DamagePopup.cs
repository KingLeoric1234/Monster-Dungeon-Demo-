using UnityEngine;
using Enemy;

namespace Game.Enemy
{
    /// <summary>
    /// 伤害跳字。挂在敌人身上，受到伤害时在头顶跳出伤害数字。
    /// 普通伤害白色，暴击伤害红色，抛物线运动+淡出。
    /// 需要配合DamageNumber预制体使用。
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        /// <summary>全部可调参数，集中在 DamageNumberArguments</summary>
        private readonly DamageNumberArguments arguments = new DamageNumberArguments();

        // 伤害数字预制体：不走 Inspector。
        // 由外部脚本通过 SetDamageNumberPrefab 注入；注入前 Awake 会按约定路径尝试 Resources.Load。
        private DamageNumber damageNumberPrefab;

        private float lastSpawnTime;

        // 提醒过一次就不再刷屏（每个敌人 Awake 都会尝试抓取，但警告只打一次）
        private static bool hasWarnedNoPrefab;

        private void Awake()
        {
            TryFetchDamageNumberPrefab();
        }

        private void OnEnable()
        {
            EnemyMessageBus.Subscribe<EnemyDamageAppliedMessage>(OnEnemyDamageApplied);
        }

        private void OnDisable()
        {
            EnemyMessageBus.Unsubscribe<EnemyDamageAppliedMessage>(OnEnemyDamageApplied);
        }

        /// <summary>怪物实际受到伤害时触发（结算器发布，走 Enemy 域总线）</summary>
        private void OnEnemyDamageApplied(EnemyDamageAppliedMessage msg)
        {
            // 只有打到自己时才跳字
            if (msg.Source != gameObject) return;

            // 建档广播（Damage=0）不跳字
            if (msg.Damage <= 0) return;

            // 防止数字重叠
            if (Time.time - lastSpawnTime < arguments.spawnInterval) return;
            lastSpawnTime = Time.time;

            SpawnDamageNumber(msg.Damage, msg.IsCrit);
        }

        /// <summary>生成伤害数字</summary>
        private void SpawnDamageNumber(int damage, bool isCrit)
        {
            // 预制体还没做（Awake 已提醒过一次）：不跳字，静默跳过
            if (damageNumberPrefab == null) return;

            // 在头顶生成
            Vector3 spawnPos = transform.position + arguments.offset;
            DamageNumber number = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity);

            // 初始化数字（参数一起传下去）
            number.Initialize(damage, isCrit, arguments);
        }

        /// <summary>外部脚本注入伤害数字预制体（不走 Inspector）</summary>
        public void SetDamageNumberPrefab(DamageNumber prefab)
        {
            damageNumberPrefab = prefab;
        }

        /// <summary>
        /// 抓取伤害数字预制体：没注入就按约定路径从 Resources 找；找不到就提醒一次（之后记得做预制体）。
        /// </summary>
        private void TryFetchDamageNumberPrefab()
        {
            if (damageNumberPrefab != null) return;

            damageNumberPrefab = Resources.Load<DamageNumber>(kDamageNumberPrefabPath);
            if (damageNumberPrefab == null && !hasWarnedNoPrefab)
            {
                hasWarnedNoPrefab = true;
                Debug.LogWarning(
                    $"[DamagePopup] 还没有伤害数字预制体，跳字暂不显示。之后记得做一个 DamageNumber 预制体：" +
                    $"放到 Resources/{kDamageNumberPrefabPath}（或改 kDamageNumberPrefabPath 常量），" +
                    "或用 SetDamageNumberPrefab 注入。", this);
            }
        }

        /// <summary>预制体约定路径（按项目惯例走 Resources）</summary>
        private const string kDamageNumberPrefabPath = "Enemy/Effects/DamageNumber";
    }
}
