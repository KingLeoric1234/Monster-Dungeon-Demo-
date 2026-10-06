using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Items;
using Game.Core;

namespace Game.UI
{
    /// <summary>
    /// 固定槽UI：挂在每个固定槽上（HandUse/Pocket/Pouch）。
    /// 自动订阅FixedSlotChangedMessage，所有Panel的固定槽同步显示。
    /// 支持槽位类型限制：HandUse放所有，Pocket/Pouch只放Potion/Bullet。
    /// 点击槽口里的物品可以拿出来拖拽。
    /// </summary>
    public class FixedSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("设置")]
        [SerializeField] private FixedSlotType slotType;   // 这个槽是什么类型
        [SerializeField] private Image iconImage;           // 图标显示（自动找如果没设）
        [SerializeField] private GameObject floatingItemIconPrefab;  // 拖拽图标预制体
        [SerializeField] private Canvas parentCanvas;                // 父Canvas

        private void Awake()
        {
            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
            }
            // 预制体实例不拖Canvas引用：向上找最近的Canvas（已拖则优先用拖的）
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
            // string path = GetHierarchyPath();
            // bool isStatusPanel = path.Contains("Status");
            // Debug.Log($"[FixedSlotUI] OnEnable, 槽={slotType}, 物体={gameObject.name}, 路径={path}, IsStatus={isStatusPanel}");
            MessageBus.Subscribe<FixedSlotChangedMessage>(OnFixedSlotChanged);
        }

        /// <summary>获取完整的父物体路径，用于判断属于哪个Panel</summary>
        private string GetHierarchyPath()
        {
            string path = gameObject.name;
            Transform parent = transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        private void OnDisable()
        {
            // string path = GetHierarchyPath();
            // if (path.Contains("Status"))
            // {
            //     Debug.Log($"[StatusPanel槽] OnDisable取消订阅, 槽={slotType}, 路径={path}");
            // }
            MessageBus.Unsubscribe<FixedSlotChangedMessage>(OnFixedSlotChanged);
        }

        /// <summary>固定槽变化时自动刷新</summary>
        private void OnFixedSlotChanged(FixedSlotChangedMessage msg)
        {
            // string path = GetHierarchyPath();
            // if (path.Contains("Status"))
            // {
            //     Debug.Log($"[StatusPanel槽] 收到消息, 消息槽={msg.Slot}, 本槽={slotType}, 匹配={msg.Slot == slotType}, 路径={path}");
            // }
            if (msg.Slot == slotType)
            {
                RefreshDisplay();
            }
        }

        /// <summary>从ItemDirector读取当前固定槽物品并刷新显示（public，供StatusPanelDirector打开面板时调用）</summary>
        public void RefreshDisplay()
        {
            // string path = GetHierarchyPath();
            // bool isStatusPanel = path.Contains("Status");
            // if (isStatusPanel) Debug.Log($"[StatusPanel槽] RefreshDisplay, 槽={slotType}, 路径={path}");

            if (ItemDirector.Instance == null)
            {
                // if (isStatusPanel) Debug.LogError("[StatusPanel槽] ItemDirector.Instance为null！");
                return;
            }

            var (itemType, itemID) = ItemDirector.Instance.GetFixedSlot(slotType);
            // if (isStatusPanel) Debug.Log($"[StatusPanel槽] 读取到物品, 类型={itemType}, ID={itemID}");

            if (itemType == FixedItemType.None || string.IsNullOrEmpty(itemID))
            {
                ClearIcon();
                return;
            }

            // 优先从缓存读图标+颜色，缓存没有再从Config读
            var (icon, color) = ItemDirector.Instance.GetFixedSlotIcon(slotType);
            if (icon == null)
            {
                icon = GetItemIcon(itemType, itemID);
                color = Color.white;
            }
            // if (isStatusPanel) Debug.Log($"[StatusPanel槽] 获取到图标={(icon != null ? icon.name : "null")}, 颜色={color}");

            if (icon != null)
            {
                SetIcon(icon, color);
            }
            else
            {
                // if (isStatusPanel) Debug.LogWarning("[StatusPanel槽] 图标为null，清空");
                ClearIcon();
            }
        }

        /// <summary>根据物品类型获取图标</summary>
        private Sprite GetItemIcon(FixedItemType type, string id)
        {
            switch (type)
            {
                case FixedItemType.Equipment:
                    var equipData = EquipmentConfig.GetEquipment(id);
                    return equipData != null ? equipData.Icon : null;
                case FixedItemType.Weapon:
                    var weaponData = WeaponConfig.GetWeapon(id);
                    return weaponData != null ? weaponData.icon : null;
                case FixedItemType.Potion:
                    var potionData = PotionConfig.GetPotion(id);
                    return potionData != null ? potionData.icon : null;
                case FixedItemType.Bullet:
                    var bulletData = BulletConfig.GetBullet(id);
                    return bulletData != null ? bulletData.icon : null;
                default:
                    return null;
            }
        }

        /// <summary>检查这个槽能不能放某类型的物品</summary>
        public bool CanAcceptItem(FixedItemType itemType)
        {
            // HandUse能放所有
            if (slotType == FixedSlotType.HandUse) return true;

            // Pocket/Pouch只能放Potion和Bullet
            if (slotType == FixedSlotType.Pocket || slotType == FixedSlotType.Pouch)
            {
                return itemType == FixedItemType.Potion || itemType == FixedItemType.Bullet;
            }

            return false;
        }

        /// <summary>放置物品到这个槽（由FloatingShopIcon调用，直接传图标+颜色）</summary>
        public void PlaceItem(FixedItemType itemType, string itemID, Sprite icon, Color color)
        {
            // Debug.Log($"[FixedSlotUI] PlaceItem开始，槽={slotType}, 类型={itemType}, ID={itemID}");

            if (!CanAcceptItem(itemType))
            {
                // Debug.LogWarning($"[FixedSlotUI] 槽{slotType}不能接受类型{itemType}");
                return;
            }

            if (ItemDirector.Instance == null)
            {
                // Debug.LogError("[FixedSlotUI] ItemDirector.Instance为null！");
                return;
            }

            // Debug.Log($"[FixedSlotUI] 调用ItemDirector.SetFixedSlot");
            ItemDirector.Instance.SetFixedSlot(slotType, itemType, itemID, icon, color);

            // 直接用传过来的图标+颜色显示，不等消息回调
            Debug.Log($"[Slot] icon={icon}, color={color}, iconImage={iconImage}");
            if (icon != null)
            {
                SetIcon(icon, color);
            }
            // Debug.Log("[FixedSlotUI] SetFixedSlot完成");
        }

        /// <summary>设置图标+颜色</summary>
        public void SetIcon(Sprite icon, Color color)
        {
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.color = color;
                iconImage.enabled = true;
            }
        }

        /// <summary>设置图标（默认白色）</summary>
        public void SetIcon(Sprite icon)
        {
            SetIcon(icon, Color.white);
        }

        /// <summary>清空图标</summary>
        public void ClearIcon()
        {
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.color = new Color(1, 1, 1, 0);
            }
        }

        /// <summary>获取槽类型</summary>
        public FixedSlotType GetSlotType()
        {
            return slotType;
        }

        /// <summary>点击槽口：左键拿出来拖拽，右键使用药水</summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // 如果正在放置物品（从商店买的东西放置中），不处理
            if (ItemDragDirector.IsPlacing) return;

            // 如果正在拖拽物品，不处理（交给FloatingItemIcon处理放置）
            if (ItemDragDirector.Instance != null && ItemDragDirector.Instance.IsDragging) return;

            // 如果槽里没有物品，不处理
            if (ItemDirector.Instance == null) return;
            var (itemType, itemID) = ItemDirector.Instance.GetFixedSlot(slotType);
            if (itemType == FixedItemType.None || string.IsNullOrEmpty(itemID)) return;

            // 右键：使用药水
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (itemType == FixedItemType.Potion)
                {
                    UsePotion(itemID);
                }
                return;
            }

            // 左键：拿出来拖拽
            if (iconImage != null && iconImage.sprite != null)
            {
                // 先缓存图标和颜色（因为StartDragFromFixedSlot会清空槽口，导致iconImage被重置）
                Sprite cachedIcon = iconImage.sprite;
                Color cachedColor = iconImage.color;

                ItemDragDirector.Instance.StartDragFromFixedSlot(
                    slotType, itemType, itemID, cachedIcon, cachedColor);

                // 生成跟随鼠标的图标
                if (floatingItemIconPrefab != null && parentCanvas != null)
                {
                    GameObject iconObj = Instantiate(floatingItemIconPrefab, parentCanvas.transform);
                    FloatingItemIcon floatingIcon = iconObj.GetComponent<FloatingItemIcon>();
                    if (floatingIcon != null)
                    {
                        floatingIcon.Initialize(itemType, itemID, cachedIcon, cachedColor, parentCanvas);
                    }
                }
            }
        }

        /// <summary>使用药水</summary>
        private void UsePotion(string potionID)
        {
            var potionData = PotionConfig.GetPotion(potionID);
            if (potionData == null) return;

            // 发布药水使用消息（让PotionBuffDirector处理）
            MessageBus.Publish(new PotionUsedMessage { PotionID = potionID });

            // 清空槽口
            ItemDirector.Instance.ClearFixedSlot(slotType);
        }
    }
}
