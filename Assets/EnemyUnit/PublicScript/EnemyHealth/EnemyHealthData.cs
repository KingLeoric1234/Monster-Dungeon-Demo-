using System;
using UnityEngine;
using Game.Core;
using Enemy;

namespace Game.Enemy
{
    /// <summary>
    /// 血量账本：唯一持有 currentHealth / maxHealth 的地方。
    /// 对外只读 + 事件广播，是 Health 数据流的中枢。
    /// 加减血量只走 ApplyDamage，外部一律通过只读属性/事件消费。
    /// </summary>
    public class EnemyHealthData : MonoBehaviour
    {
        public MonsterTemplate Template { get; private set; }
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; private set; }

        // —— 事件（数据流出口）——
        // 血条信息改走 EnemyMessageBus 的 EnemyDamageAppliedMessage（建档广播 Damage=0，掉血由结算器广播）
        /// <summary>受伤（特效用：闪红/顿帧/粒子按伤害量）</summary>
        public event Action<int, int, int> OnDamaged;     // (damage, current, max)
        /// <summary>死亡（EnemyDeath 订阅）</summary>
        public event Action OnDied;

        /// <summary>建档：EnemyInit 调用，广播事件让特效就位、发布血条消息</summary>
        public void Initialize(MonsterTemplate template)
        {
            Template = template;
            MaxHealth = template.maxHealth;
            CurrentHealth = template.maxHealth;

            // 建档广播（Damage=0）：让血条初始化；之后每次掉血由结算器带血量广播，账本不再重复发
            EnemyMessageBus.Publish(new EnemyDamageAppliedMessage
            {
                Source = gameObject,
                Damage = 0,
                IsCrit = false,
                CurrentHealth = CurrentHealth,
                MaxHealth = MaxHealth
            });
        }

        /// <summary>动账本的唯一入口：扣血 + 钳制，然后广播</summary>
        public void ApplyDamage(int damage)
        {
            if (damage <= 0) return;

            CurrentHealth -= damage;
            if (CurrentHealth < 0) CurrentHealth = 0;

            OnDamaged?.Invoke(damage, CurrentHealth, MaxHealth);

            if (CurrentHealth <= 0)
                OnDied?.Invoke();
        }

        public int GetMaxHealth() => MaxHealth;

        public string TemplateName => Template != null ? Template.monsterName : "";
    }
}
