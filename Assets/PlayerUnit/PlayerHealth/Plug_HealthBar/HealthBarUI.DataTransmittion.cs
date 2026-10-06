using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Player; // Player 域：PlayerMessageBus + PlayerHealthChangedMessage（电话实物）

namespace HealthBar
{
    /// <summary>
    /// 玩家血条 UI 芯片——数据件（主文件）：只管数据流。
    /// 职责：零件引用字段 + 订阅/退订 PlayerHealthChangedMessage + 收消息更新数据 + 状态读取口。
    /// 分工：
    ///   HealthBarUI.DataTransmittion.cs               数据：字段 + 订阅 + 收消息（本件）
    ///   HealthBarUI.Assembly 装配：创建零件、锚点坐标、Resources 抓图抓字体
    ///   HealthBarUI.Display.cs       显示：默认显隐 + 地牢延迟渐显 + 面板联动 + 内容刷新
    ///   HealthBarUI.Animation.cs     动画：血条自己的淡入淡出/滑动（焊死，不做全局共用）
    /// 对外引脚：订阅 PlayerHealthChangedMessage 收血条数据。
    /// 只认识消息，不认识发布方；内部四件互相焊死。
    /// </summary>
    public partial class HealthBarUI : MonoBehaviour
    {
        // ── 三个零件引用（装配件创建并赋值，显示件读取）──
        private GameObject hpGroup;       // 托盘：空物体 + RectTransform + CanvasGroup（管整体显隐/淡入淡出）
        private Image hpFill;             // 填充条：宽度 = 血量比例（图来自 Resources/Fills/Fill1）
        private TextMeshProUGUI hpText;   // 标签："X / Y"（字体来自 Resources/TextMesh Pro）

        // 血条数据（消息写入，显示件读取，血量唯一数据源是 PlayerHealth）
        private int currentHP;
        private int maxHP;

        // 显示状态（由显示件/动画件读写）
        private bool isInDungeon = false;
        private bool isStatusPanelOpen = false;

        private void OnEnable()
        {
            PlayerMessageBus.Subscribe<PlayerHealthChangedMessage>(OnHealthChanged);
        }

        private void OnDisable()
        {
            PlayerMessageBus.Unsubscribe<PlayerHealthChangedMessage>(OnHealthChanged);
        }

        /// <summary>收血条消息：更新数据并触发显示刷新</summary>
        private void OnHealthChanged(PlayerHealthChangedMessage msg)
        {
            currentHP = msg.CurrentHealth;
            maxHP = msg.MaxHealth;
            UpdateHPDisplay();
        }

        // ── 状态读取口（显示/动画件直接内部读字段，外部只读）──
        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;
        public bool IsInDungeon => isInDungeon;
        public bool IsPanelOpen => isStatusPanelOpen;
    }
}
