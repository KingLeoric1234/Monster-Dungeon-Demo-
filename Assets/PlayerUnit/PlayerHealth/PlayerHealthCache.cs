using UnityEngine;
using Player; // PlayerHealthChangedMessage + PlayerMessageBus（UI 通知走 Player 域总线）

namespace Game.Player
{
    /// <summary>
    /// 血量账本（数据层）：currentHealth + maxHealth 的唯一权威来源。
    /// 铁律：谁都不许直接碰字段——加减走 ChangeHealth，上限走 SetMaxHealth，初始化走 SetCurrentHealth。
    /// 所有变更统一在这里钳制到 [0, maxHealth]，并只发一次 PlayerHealthChangedMessage（唯一通知出口）。
    /// 挂在 Player 物体上，被 PlayerHealthAction / MaxHealthManager / PlayerHealthInit 单向调用。
    /// </summary>
    public class PlayerHealthCache : MonoBehaviour
    {
        private int currentHealth;
        private int maxHealth;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        /// <summary>加减血量（唯一写入口）：钳制到 [0, maxHealth] 并广播一次</summary>
        public void ChangeHealth(int delta)
        {
            currentHealth += delta;
            ClampAndNotify();
        }

        /// <summary>直接设置当前血量（初始化/读档/复活用，不走加减语义）</summary>
        public void SetCurrentHealth(int value)
        {
            currentHealth = value;
            ClampAndNotify();
        }

        /// <summary>更新血量上限：上限增→当前血跟随；上限减→当前血钳到新上限</summary>
        public void SetMaxHealth(int newMax)
        {
            int oldMax = maxHealth;
            maxHealth = newMax;

            // 上限增加，当前血量也跟着增加（比如兴奋剂）
            if (newMax > oldMax)
                currentHealth += (newMax - oldMax);

            ClampAndNotify();
        }

        /// <summary>钳制 + 广播（所有变更的唯一出口）</summary>
        private void ClampAndNotify()
        {
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            if (currentHealth < 0) currentHealth = 0;

            PlayerMessageBus.Publish(new PlayerHealthChangedMessage
            {
                CurrentHealth = currentHealth,
                MaxHealth = maxHealth
            });
        }
    }
}
