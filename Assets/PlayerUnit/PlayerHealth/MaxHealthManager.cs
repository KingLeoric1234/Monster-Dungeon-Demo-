using UnityEngine;
using Game.Core; // MessageBus + PlayerStatsChangedMessage

namespace Game.Player
{
    /// <summary>
    /// 血量上限来源管理（只管"上限是谁"）：收集属性/Buff/饰品/道具对上限的贡献，
    /// 算出最终 maxHealth 写入 PlayerHealthCache。
    /// 铁律：永远不碰 currentHealth——上限变化如何联动当前血，由 Cache 的 SetMaxHealth 内部处理。
    /// 挂在 Player 物体上。
    /// </summary>
    public class MaxHealthManager : MonoBehaviour
    {
        private PlayerHealthCache cache;

        private void Awake()
        {
            cache = GetComponent<PlayerHealthCache>();
        }

        private void OnEnable()
        {
            MessageBus.Subscribe<PlayerStatsChangedMessage>(OnStatsChanged);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<PlayerStatsChangedMessage>(OnStatsChanged);
        }

        /// <summary>属性变化 → 更新上限（Cache 内部联动当前血并广播）</summary>
        private void OnStatsChanged(PlayerStatsChangedMessage msg)
        {
            if (cache != null) cache.SetMaxHealth(msg.MaxHealth);
        }

        /// <summary>初始化初始上限（从配置读，供 PlayerHealthInit 编排）</summary>
        public void Initialize()
        {
            if (cache != null) cache.SetMaxHealth(PlayerConfig.MaxHealth);
        }
    }
}
