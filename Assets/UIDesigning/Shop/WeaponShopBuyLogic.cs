using UnityEngine;
using UnityEngine.UI;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// 武器商店购买脚本（独立，只卖武器和子弹）。
    /// 挂在武器商店的每个商品条目上。
    /// 点击按钮 → 检查金币 → 扣钱 → 生成跟随鼠标的Icon → 点击QuickSlot放进去。
    /// UI显示（名称/价格/图标）由用户手动在Inspector设置，脚本不管。
    /// </summary>
    public class WeaponShopBuyLogic : MonoBehaviour
    {
        public enum WeaponShopItemType
        {
            Weapon,     // 武器
            Bullet      // 子弹
        }

        [Header("物品配置")]
        [SerializeField] private WeaponShopItemType itemType;   // 物品类型
        [SerializeField] private string itemID;                  // 物品ID（如 "long_sword"）
        [SerializeField] private int price;                      // 价格（手动填，跟Config一致）

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

            // 拿起时不扣钱，放到槽时再扣
            switch (itemType)
            {
                case WeaponShopItemType.Weapon:
                    BuyWeapon();
                    break;
                case WeaponShopItemType.Bullet:
                    BuyBullet();
                    break;
            }
        }

        /// <summary>购买武器：生成跟随鼠标的Icon</summary>
        private void BuyWeapon()
        {
            WeaponData data = WeaponConfig.GetWeapon(itemID);
            if (data == null) return;
            SpawnFloatingIcon(data.displayName, data.icon, FixedItemType.Weapon);
        }

        /// <summary>购买子弹：生成跟随鼠标的Icon</summary>
        private void BuyBullet()
        {
            BulletData data = BulletConfig.GetBullet(itemID);
            if (data == null) return;
            SpawnFloatingIcon(data.displayName, data.icon, FixedItemType.Bullet);
        }

        /// <summary>生成跟随鼠标的Icon</summary>
        private void SpawnFloatingIcon(string itemName, Sprite icon, FixedItemType fixedType)
        {
            if (floatingIconPrefab == null || parentCanvas == null) return;

            GameObject iconObj = Instantiate(floatingIconPrefab, parentCanvas.transform);
            FloatingIconFollow floatingIcon = iconObj.GetComponent<FloatingIconFollow>();

            if (floatingIcon != null)
            {
                Sprite iconSprite = (itemIcon != null) ? itemIcon.sprite : icon;
                Color iconColor = (itemIcon != null) ? itemIcon.color : Color.white;
                // Debug.Log($"[WeaponShop] itemIcon={itemIcon}, icon={icon}, iconSprite={iconSprite}");
                floatingIcon.Init(itemID, iconSprite, iconColor, parentCanvas, fixedType);
            }
        }
    }
}
