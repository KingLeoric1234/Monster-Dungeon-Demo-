using UnityEngine;
using UnityEngine.UI;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// 道具商店购买脚本（独立，只卖药水）。
    /// 挂在道具商店的每个商品条目上。
    /// 点击按钮 → 检查金币 → 扣钱 → 生成跟随鼠标的Icon → 点击PocketSlot放进去。
    /// UI显示（名称/价格/图标）由用户手动在Inspector设置，脚本不管。
    /// 跟装备商店、武器商店的购买逻辑完全独立。
    /// </summary>
    public class ItemShopBuyLogic : MonoBehaviour
    {
        [Header("物品配置")]
        [SerializeField] private string potionID;    // 药水ID（如 "health_potion"）
        [SerializeField] private int price;           // 价格（手动填，跟PotionConfig一致）

        [Header("引用")]
        [SerializeField] private Button buyButton;               // 购买按钮
        [SerializeField] private Image itemIcon;                 // 商品图标
        [SerializeField] private GameObject floatingIconPrefab;  // 跟随鼠标的Icon预制体
        [SerializeField] private Canvas parentCanvas;            // 父Canvas

        private void Start()
        {
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(OnBuyClicked);
            }
        }

        /// <summary>购买按钮点击</summary>
        private void OnBuyClicked()
        {
            if (ItemDirector.Instance == null) return;
            if (ItemDirector.Instance.CurrentGold < price) return;

            PotionData data = PotionConfig.GetPotion(potionID);
            if (data == null) return;

            // 扣钱统一在物品放置到槽位时执行（FixedSlotPlacer.BuyItem / InventoryPlacer.Buy），
            // 这里不扣，避免"点击拿起 + 放入槽位"双重扣费。
            // ItemDirector.Instance.SpendGold(price);

            // 生成跟随鼠标的Icon（药水类型，放到PocketSlot）
            SpawnFloatingIcon(data.displayName, data.icon);
        }

        /// <summary>生成跟随鼠标的Icon</summary>
        private void SpawnFloatingIcon(string itemName, Sprite icon)
        {
            if (floatingIconPrefab == null || parentCanvas == null) return;

            GameObject iconObj = Instantiate(floatingIconPrefab, parentCanvas.transform);
            FloatingIconFollow floatingIcon = iconObj.GetComponent<FloatingIconFollow>();

            if (floatingIcon != null)
            {
                Sprite iconSprite = (itemIcon != null) ? itemIcon.sprite : icon;
                Color iconColor = (itemIcon != null) ? itemIcon.color : Color.white;
                floatingIcon.Init(potionID, iconSprite, iconColor, parentCanvas, FixedItemType.Potion);
            }
        }
    }
}
