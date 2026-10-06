using UnityEngine;
using UnityEngine.UI;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// Shop buy: click button, icon follows mouse, click on slot to equip.
    /// No cost for now (testing). Attach to each EquipmentItem with a Button.
    /// </summary>
    public class ShopBuyAndEquip : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button buyButton;
        [SerializeField] private Image itemIcon;           // The icon on this shop item
        [SerializeField] private string equipmentID;        // Equipment ID (e.g. "leather_gloves")
        [SerializeField] private GameObject floatingIconPrefab; // Prefab for floating icon
        [SerializeField] private Canvas parentCanvas;       // Canvas to spawn icon under

        private EquipmentData data;

        private void Start()
        {
            // Load equipment data
            if (!string.IsNullOrEmpty(equipmentID))
            {
                data = EquipmentConfig.GetEquipment(equipmentID);

                // Cache icon for status panel to use
                if (itemIcon != null && itemIcon.sprite != null)
                {
                    EquipmentIconCache.SetIcon(equipmentID, itemIcon.sprite, itemIcon.color);
                }
            }

            // Register button click
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(OnBuyClicked);
            }
        }

        /// <summary>Buy button clicked: spawn floating icon that follows mouse</summary>
        private void OnBuyClicked()
        {
            if (data == null)
            {
                // Debug.LogWarning("[ShopBuyAndEquip] Missing equipment data");
                return;
            }

            if (floatingIconPrefab == null || parentCanvas == null)
            {
                // Debug.LogWarning("[ShopBuyAndEquip] Missing floatingIconPrefab or parentCanvas");
                return;
            }

            // Spawn floating icon
            GameObject iconObj = Instantiate(floatingIconPrefab, parentCanvas.transform);
            FloatingIconFollow floatingIcon = iconObj.GetComponent<FloatingIconFollow>();

            if (floatingIcon != null)
            {
                Sprite iconSprite = (itemIcon != null) ? itemIcon.sprite : null;
                Color iconColor = (itemIcon != null) ? itemIcon.color : Color.white;
                floatingIcon.Init(data.ID, iconSprite, iconColor, parentCanvas);
            }
            // Debug.Log("[ShopBuyAndEquip] Purchased: " + data.Name + ", icon follows mouse");
        }
    }
}
