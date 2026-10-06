using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Items;
using Game.Core;

namespace Game.UI
{
    /// <summary>
    /// Equipment slot UI: displays equipped item icon.
    /// Attach to each equipment slot (Head/Hand/Body/Back).
    /// Auto-subscribes EquipmentEquippedMessage, updates display automatically.
    /// Default image is transparent, when equipped it shows the item icon.
    /// 左键点击：拿出装备拖拽（装备拖拽只能丢弃/右键取消，不能放任何槽）。
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Settings")]
        [SerializeField] private EquipmentSlotType slotType;   // Which slot this is
        [SerializeField] private Image iconImage;              // Icon display (auto-finds if null)
        [Header("拖拽（拿出装备）")]
        [SerializeField] private GameObject floatingItemIconPrefab;  // 拖拽图标预制体
        [SerializeField] private Canvas parentCanvas;                // 父Canvas

        private EquipmentData equippedItem;

        private void Awake()
        {
            // Auto-find icon image if not set
            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
            }
            // 预制体实例不拖Canvas引用：向上找最近的Canvas
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }
        }

        private void Start()
        {
            RefreshDisplay();
        }

        private void OnEnable()
        {
            MessageBus.Subscribe<EquipmentEquippedMessage>(OnEquipmentChanged);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<EquipmentEquippedMessage>(OnEquipmentChanged);
        }

        /// <summary>装备变化时自动刷新</summary>
        private void OnEquipmentChanged(EquipmentEquippedMessage msg)
        {
            if (msg.Slot == slotType)
            {
                RefreshDisplay();
            }
        }

        /// <summary>从ItemDirector读取当前穿戴的装备并刷新显示</summary>
        private void RefreshDisplay()
        {
            if (ItemDirector.Instance == null) return;

            string equippedID = ItemDirector.Instance.GetEquipped(slotType);
            if (string.IsNullOrEmpty(equippedID))
            {
                UnequipItem();
            }
            else
            {
                EquipmentData data = EquipmentConfig.GetEquipment(equippedID);
                if (data != null)
                {
                    EquipItem(data);
                    // 图标来源链：商店缓存 → 配置 → Resources（装备ID.png）
                    // 商店缓存只在打开过商店后才有；GameCheat等非商店链路靠配置/Resources兜底
                    Sprite icon = EquipmentIconCache.GetIcon(equippedID);
                    if (icon == null) icon = ItemFactory.GetItemIcon(equippedID);
                    if (icon == null) icon = Resources.Load<Sprite>(equippedID);
                    if (icon != null)
                    {
                        SetIcon(icon, EquipmentIconCache.GetIconColor(equippedID));
                    }
                }
            }
        }

        /// <summary>Equip an item to this slot</summary>
        public void EquipItem(EquipmentData item)
        {
            equippedItem = item;

            if (iconImage != null && item != null)
            {
                iconImage.gameObject.SetActive(true);   // 装备时启用图标（用代码控制，替代手动禁用）
                // 强制读取Inspector配置的颜色（引擎默认白色），只保证Alpha=1
                Color c = iconImage.color;
                c.a = 1f;
                iconImage.color = c;
            }
        }

        /// <summary>Unequip the item from this slot</summary>
        public void UnequipItem()
        {
            equippedItem = null;

            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);  // 未装备时隐藏图标（防止默认内容显示）
                iconImage.sprite = null;
            }
        }

        /// <summary>Set the icon sprite using the slot's own Inspector color (refresh fallback)</summary>
        public void SetIcon(Sprite icon)
        {
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                // 刷新场景（无携带颜色）：用槽自己在Inspector配置的颜色，Alpha强制1
                Color c = iconImage.color;
                c.a = 1f;
                iconImage.color = c;
            }
        }

        /// <summary>Set the icon sprite and color (purchase: color comes from Cursor/button Image)</summary>
        public void SetIcon(Sprite icon, Color iconColor)
        {
            // Debug.Log($"[SetIcon]功能正常 image={(iconImage == null ? "NULL" : iconImage.gameObject.name)} active={iconImage?.gameObject.activeSelf}");
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                // 用Cursor携带的按钮Image颜色（不是槽自己的颜色），Alpha强制1
                Color c = iconColor;
                c.a = 1f;
                iconImage.color = c;
            }
        }

        /// <summary>Get the slot type</summary>
        public EquipmentSlotType GetSlotType()
        {
            return slotType;
        }

        /// <summary>Check if slot has item equipped</summary>
        public bool HasItem()
        {
            return equippedItem != null;
        }

        /// <summary>左键点击：拿出装备拖拽（装备拖拽只能丢弃/右键取消，不能放任何槽）</summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // 商店放置中不处理
            if (ItemDragDirector.IsPlacing) return;
            // 拖拽中不处理
            if (ItemDragDirector.Instance != null && ItemDragDirector.Instance.IsDragging) return;
            // 左键才处理
            if (eventData.button != PointerEventData.InputButton.Left) return;
            // 槽里没有装备
            if (equippedItem == null || iconImage == null || iconImage.sprite == null) return;

            // 缓存图标、颜色、ID（StartDragFromEquipmentSlot会卸下装备清空槽口，之后equippedItem变null）
            Sprite cachedIcon = iconImage.sprite;
            Color cachedColor = iconImage.color;
            string cachedID = equippedItem.ID;

            ItemDragDirector.Instance.StartDragFromEquipmentSlot(slotType, cachedID, cachedIcon, cachedColor);

            // 生成跟随鼠标的图标（fromEquipSlot=true：只能丢弃/取消）
            if (floatingItemIconPrefab != null && parentCanvas != null)
            {
                GameObject iconObj = Instantiate(floatingItemIconPrefab, parentCanvas.transform);
                FloatingItemIcon floatingIcon = iconObj.GetComponent<FloatingItemIcon>();
                if (floatingIcon != null)
                {
                    floatingIcon.Initialize(FixedItemType.Equipment, cachedID, cachedIcon, cachedColor, parentCanvas, 1, true);
                }
            }
        }
    }
}
