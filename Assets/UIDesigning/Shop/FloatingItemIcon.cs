using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// 从槽口拿出来的跟随鼠标图标。
    /// 三角结构规则：
    ///   · 固定槽 ↔ 背包槽：可放置/交换（各自槽规则校验，双向）
    ///   · 拖拽物品 → 装备槽：无效（装备只能商店购买直接放置）
    ///   · 装备拖拽（来源装备槽）：点任意槽无效，只能点空白丢弃 / 右键取消
    ///   · 点空白：丢弃
    /// </summary>
    public class FloatingItemIcon : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Image iconImage;

        private Canvas parentCanvas;
        private FixedItemType itemType;
        private string itemID;
        private int itemCount = 1;
        private bool isFromEquipSlot = false;  // 装备拖拽：只能丢弃/取消，禁止放任何槽
        private float clickDelay = 0.1f;  // 生成后延迟响应点击，防止同一帧触发

        private void Awake()
        {
            // 预制体上没拖iconImage引用时自动查找（挂在有Image的物体上）
            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
            }
        }

        /// <summary>初始化</summary>
        public void Initialize(FixedItemType type, string id, Sprite icon, Color color, Canvas canvas, int count = 1, bool fromEquipSlot = false)
        {
            itemType = type;
            itemID = id;
            parentCanvas = canvas;
            itemCount = count;
            isFromEquipSlot = fromEquipSlot;

            // 兜底：没传Canvas时向上找最近的Canvas
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }

            // 兜底：icon没传到（传参链路缺口/配置图标未配）→ 从配置查图标，保证不显示白块
            if (icon == null)
            {
                icon = ItemFactory.GetItemIcon(id);
            }

            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.color = color;
                iconImage.raycastTarget = false;  // 不阻挡射线
            }

            transform.SetAsLastSibling();
        }

        /// <summary>防闪现：先隐藏，等一帧定位到鼠标后再显示（与商店版FloatingIconFollow同款已验证模式）</summary>
        private void Start()
        {
            if (iconImage != null) iconImage.enabled = false;
            StartCoroutine(DelaySnap());
        }

        private System.Collections.IEnumerator DelaySnap()
        {
            yield return null;  // 等一帧
            FollowMouse();      // 定位到鼠标
            if (iconImage != null) iconImage.enabled = true;
        }

        private void Update()
        {
            // 延迟响应点击，防止生成同一帧就触发
            if (clickDelay > 0)
            {
                clickDelay -= Time.deltaTime;
            }

            FollowMouse();

            // 左键：尝试放置（延迟结束后才响应）
            if (clickDelay <= 0 && Input.GetMouseButtonDown(0))
            {
                if (ItemDragDirector.Instance != null)
                {
                    ItemDragDirector.Instance.StartPlacing();
                }
                TryPlaceOrThrow();
            }

            // 右键：取消，物品回到原槽
            if (Input.GetMouseButtonDown(1))
            {
                if (ItemDragDirector.Instance != null)
                {
                    ItemDragDirector.Instance.CancelDrag();
                }
                Destroy(gameObject);
            }
        }

        /// <summary>跟随鼠标：把图标摆到鼠标在父Canvas坐标系下的位置</summary>
        private void FollowMouse()
        {
            // 每帧兜底查找Canvas，防止实例化时父层级未就绪
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }
            if (parentCanvas != null)
            {
                RectTransform rect = parentCanvas.transform as RectTransform;
                Vector2 mousePos;
                if (rect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rect, Input.mousePosition, parentCanvas.worldCamera, out mousePos))
                {
                    transform.localPosition = mousePos;
                }
            }
        }

        /// <summary>尝试放置到槽口，或者扔掉</summary>
        private void TryPlaceOrThrow()
        {
            // 全局射线检测（覆盖所有Canvas：商店/状态面板/背包）
            var hit = ShopSlotDetector.Detect(parentCanvas);

            // 装备拖拽：点在任意槽上 → 无效（物品留在手上）；点空白 → 丢弃
            if (isFromEquipSlot)
            {
                if (hit.type != ShopSlotDetector.HitType.None) return;
                if (ItemDragDirector.Instance != null) ItemDragDirector.Instance.ThrowAway();
                Destroy(gameObject);
                return;
            }

            if (hit.type == ShopSlotDetector.HitType.Fixed && hit.fixedSlot != null)
            {
                TryPlaceToFixedSlot(hit.fixedSlot);
                return;
            }
            if (hit.type == ShopSlotDetector.HitType.Inventory && hit.inventorySlot != null)
            {
                TryPlaceToInventorySlot(hit.inventorySlot);
                return;
            }
            if (hit.type == ShopSlotDetector.HitType.Equip && hit.equipSlot != null)
            {
                return;  // 拖拽物品不能放装备槽（装备只能商店购买直接放置）：无效，留在手上
            }

            // 空白：丢弃
            if (ItemDragDirector.Instance != null)
            {
                ItemDragDirector.Instance.ThrowAway();
            }
            Destroy(gameObject);
        }

        /// <summary>放置到固定槽（空放 / 交换）</summary>
        private void TryPlaceToFixedSlot(FixedSlotUI slot)
        {
            // 槽不接受这个类型：无效，物品留在手上
            if (!slot.CanAcceptItem(itemType)) return;

            // 目标槽有物品 → 交换：目标物品回填来源槽（双向校验），拖拽物品放入目标槽
            var (targetType, targetID) = ItemDirector.Instance.GetFixedSlot(slot.GetSlotType());
            if (targetType != FixedItemType.None && !string.IsNullOrEmpty(targetID))
            {
                // 目标物品能否回填来源槽？不能则交换失败，留在手上
                if (ItemDragDirector.Instance == null || !ItemDragDirector.Instance.CanRestoreToSource(targetID)) return;

                var (targetIcon, targetColor) = ItemDirector.Instance.GetFixedSlotIcon(slot.GetSlotType());
                ItemDragDirector.Instance.RestoreToSourceSlot(targetID, targetIcon, targetColor, 1);
                slot.PlaceItem(itemType, itemID, iconImage.sprite, iconImage.color);
                Destroy(gameObject);
                return;
            }

            // 空槽：放置
            slot.PlaceItem(itemType, itemID, iconImage.sprite, iconImage.color);
            if (ItemDragDirector.Instance != null)
            {
                ItemDragDirector.Instance.EndDrag();
            }
            Destroy(gameObject);
        }

        /// <summary>放置到背包槽（空放 / 交换）</summary>
        private void TryPlaceToInventorySlot(InventorySlotUI slot)
        {
            var bagType = slot.GetSlotType();
            var bagItemType = InventoryDirector.GetItemType(itemID);

            // 区域不接受这个类型：无效，物品留在手上
            if (!SlotRules.CanHold(bagType, bagItemType)) return;

            var target = InventoryDirector.Instance.GetSlot(bagType, slot.GetIndex());

            // 目标槽有物品 → 交换：目标物品回填来源槽（双向校验），拖拽物品放入目标槽
            if (target != null && !target.IsEmpty && target.data != null)
            {
                // 目标物品能否回填来源槽？不能则交换失败，留在手上
                if (ItemDragDirector.Instance == null || !ItemDragDirector.Instance.CanRestoreToSource(target.data.id)) return;

                ItemDragDirector.Instance.RestoreToSourceSlot(target.data.id, target.data.icon, Color.white, target.count);
                var data = ItemFactory.CreateItemData(itemID, iconImage.sprite);
                InventoryDirector.Instance.PlaceAt(bagType, slot.GetIndex(), data, itemCount);
                Destroy(gameObject);
                return;
            }

            // 空槽：放置（失败则取消回原槽）
            var newData = ItemFactory.CreateItemData(itemID, iconImage.sprite);
            if (InventoryDirector.Instance.PlaceAt(bagType, slot.GetIndex(), newData, itemCount))
            {
                if (ItemDragDirector.Instance != null)
                {
                    ItemDragDirector.Instance.EndDrag();
                }
            }
            else
            {
                if (ItemDragDirector.Instance != null)
                {
                    ItemDragDirector.Instance.CancelDrag();
                }
            }
            Destroy(gameObject);
        }
    }
}
