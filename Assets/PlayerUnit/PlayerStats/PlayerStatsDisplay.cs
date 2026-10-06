using UnityEngine;
using Game.Core;
using Game.Player;
using Game.UI;

namespace Game.PlayerStats
{
    /// <summary>
    /// 玩家属性显示：订阅PlayerStatsChangedMessage，更新StatsPanel的属性文本。
    /// 挂在StatsPanel物体上，或者单独挂一个物体引用StatsPanel。
    /// </summary>
    public class PlayerStatsDisplay : MonoBehaviour
    {
        [SerializeField] private StatsPanel statsPanel;  // 引用StatsPanel

        private void OnEnable()
        {
            MessageBus.Subscribe<PlayerStatsChangedMessage>(OnStatsChanged);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<PlayerStatsChangedMessage>(OnStatsChanged);
        }

        private void Start()
        {
            // 自动找StatsPanel（如果没拖引用）
            if (statsPanel == null)
            {
                statsPanel = FindObjectOfType<StatsPanel>();
            }
        }

        /// <summary>属性变化时更新显示</summary>
        private void OnStatsChanged(PlayerStatsChangedMessage msg)
        {
            if (statsPanel == null) return;

            // 直接更新StatsPanel的文本（通过反射或者public方法，这里先调用UpdateStats）
            // StatsPanel的UpdateStats是public的，但是它自己从ItemDirector读
            // 我们需要给StatsPanel加一个SetStats方法，或者直接改它的UpdateStats
            // 暂时先调用UpdateStats，之后再优化
            statsPanel.UpdateStats();
        }
    }
}
