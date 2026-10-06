using UnityEngine;
using Game.UI;

namespace Game.Player
{
    /// <summary>
    /// 鼠标悬停玩家显示属性面板。
    /// 鼠标放到玩家身上0.5秒后，临时显示StatsPanel（ATK/DEF/CRT/SPD）。
    /// 鼠标移开自动关闭。
    /// 跟Q键的固定开关不冲突：Q键打开的面板不会被鼠标移开关闭。
    /// 挂在Player物体上，需要Collider2D组件。
    /// </summary>
    public class PlayerHoverStats : MonoBehaviour
    {
        [Header("设置")]
        [SerializeField] private float hoverDelay = 0.5f;  // 悬停多久后显示（秒）
        [SerializeField] private StatsPanel statsPanel;      // 属性面板引用

        private bool isMouseOver = false;
        private float hoverTimer = 0f;
        private bool isHoverOpen = false;  // 是不是鼠标悬停打开的（跟Q键区分）

        private void Update()
        {
            if (statsPanel == null) return;

            // 鼠标悬停计时
            if (isMouseOver && !isHoverOpen)
            {
                hoverTimer += Time.deltaTime;
                if (hoverTimer >= hoverDelay)
                {
                    // 只有当面板不是Q键打开的时候，才用鼠标悬停打开
                    if (!statsPanel.IsOpen)
                    {
                        statsPanel.Open();
                        isHoverOpen = true;
                    }
                    hoverTimer = 0f;
                }
            }
        }

        private void OnMouseEnter()
        {
            isMouseOver = true;
            hoverTimer = 0f;
        }

        private void OnMouseExit()
        {
            isMouseOver = false;
            hoverTimer = 0f;

            // 只有鼠标悬停打开的面板才会被关闭
            // Q键打开的面板不受影响
            if (isHoverOpen && statsPanel != null)
            {
                statsPanel.Close();
                isHoverOpen = false;
            }
        }

        /// <summary>通知鼠标悬停脚本：Q键切换了面板（用于同步状态）</summary>
        public void OnPanelToggledByKey()
        {
            // 如果Q键关闭了面板，重置鼠标悬停状态
            if (statsPanel != null && !statsPanel.IsOpen)
            {
                isHoverOpen = false;
                hoverTimer = 0f;
            }
        }
    }
}
