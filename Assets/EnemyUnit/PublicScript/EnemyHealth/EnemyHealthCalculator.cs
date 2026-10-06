using UnityEngine;
using Game.Core;
using Enemy;

namespace Game.Enemy
{
    /// <summary>
    /// 维护血量（对外门面/结算计算器）：EnemyDirector / CombatDirector 只认这个类。
    /// 初始化转发给 EnemyInit；扣血先过 Buff 链，再交给账本；演出由特效脚本订阅事件自动播放。
    /// </summary>
    public class EnemyHealthCalculator : MonoBehaviour
    {
        private EnemyInit init;
        private EnemyHealthData data;
        private EnemyBuffSystem buffSystem;

        private void Awake()
        {
            init = GetComponent<EnemyInit>();
            data = GetComponent<EnemyHealthData>();
            buffSystem = GetComponent<EnemyBuffSystem>();
        }

        /// <summary>对外暴露怪物模板（CombatDirector 读击退抗性用）</summary>
        public MonsterTemplate Template => data != null ? data.Template : null;

        /// <summary>初始化（EnemyDirector 生成怪物时调用）</summary>
        public void Initialize(MonsterTemplate template)
        {
            if (init != null) init.Initialize(template);
            else if (data != null) data.Initialize(template);
        }

        /// <summary>结算入口（CombatDirector 调用）：Buff 先改伤害，再动账本，最后广播伤害已结算</summary>
        public void TakeDamage(int damage, bool isCrit)
        {
            if (data == null) return;

            // ① Buff 链：让插件先改"进门伤害"
            int finalDamage = buffSystem != null ? buffSystem.ApplyIncomingDamage(damage) : damage;
            if (finalDamage <= 0) return;   // 完全格挡/无敌：不扣血、不演出

            // ② 动账本（内部广播 OnDamaged / OnHealthChanged / OnDied）
            data.ApplyDamage(finalDamage);

            // ③ 广播"伤害已结算"：DamagePopup 跳字、HealthBarBinder 刷血条（走 Enemy 域总线，不惊动全局）
            EnemyMessageBus.Publish(new EnemyDamageAppliedMessage
            {
                Source = gameObject,
                Damage = finalDamage,
                IsCrit = isCrit,
                CurrentHealth = data.CurrentHealth,
                MaxHealth = data.MaxHealth
            });
        }
    }
}
