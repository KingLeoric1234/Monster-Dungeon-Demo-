using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core;
using Game.UI;
using UnityEngine.EventSystems;

namespace Game.Items
{
    /// <summary>单个背包槽：显示物品，左键拿起拖拽（可与固定槽/背包槽交换），右键使用药水。</summary>
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private BagSlotType slotType;
        [SerializeField] private int index = -1;
        [SerializeField] private Image icon;
        [SerializeField] private Image slotBackground;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private GameObject floatingItemIconPrefab;  // 拖拽图标预制体
        [SerializeField] private Canvas parentCanvas;                // 父Canvas（预制体实例不拖引用，自动向上找）

        private void Awake()
        {
            // 预制体实例不能拖Canvas引用：向上找最近的Canvas（槽在哪个面板下就找到哪个面板）
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }
        }

        private void OnEnable()
        {
            MessageBus.Subscribe<InventorySlotChangedMessage>(OnMsg);
            Refresh();
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<InventorySlotChangedMessage>(OnMsg);
        }

        private void OnMsg(InventorySlotChangedMessage msg)
        {
            if (msg.Slot == slotType) Refresh();
        }

        public void Setup(BagSlotType type, int idx = -1)
        {
            slotType = type;
            index = idx;
            Refresh();
        }

        /// <summary>获取这个槽属于哪个区域（商店放置用）</summary>
        public BagSlotType GetSlotType() => slotType;

        /// <summary>获取这个槽是该区域的第几格（商店放置用）</summary>
        public int GetIndex() => index;

        public void Refresh()
        {
            if (InventoryDirector.Instance == null || index < 0) return;
            var s = InventoryDirector.Instance.GetSlot(slotType, index);
            if (s == null || s.IsEmpty)
            {
                icon.gameObject.SetActive(false);
                icon.enabled = false;
                countText.gameObject.SetActive(false);
                countText.text = "";
            }
            else
            {
                icon.gameObject.SetActive(true);
                icon.enabled = true;
                icon.sprite = s.data.icon;
                countText.gameObject.SetActive(s.count > 1);
                countText.text = s.count > 1 ? s.count.ToString() : "";
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // 商店放置中不处理（与FixedSlotUI一致），避免放置点击误触发拿起/右键使用
            if (ItemDragDirector.IsPlacing) return;
            // 拖拽中不处理（交给FloatingItemIcon处理放置）
            if (ItemDragDirector.Instance != null && ItemDragDirector.Instance.IsDragging) return;
            if (InventoryDirector.Instance == null || index < 0) return;

            // 右键：使用药水（无论哪个槽）
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                UsePotionAtSlot();
                return;
            }

            // 左键：拿起拖拽（与固定槽统一为拖拽式，可拖到固定槽/背包槽交换）
            var s = InventoryDirector.Instance.GetSlot(slotType, index);
            if (s == null || s.IsEmpty || s.data == null) return;

            // 先把数据全部缓存到局部变量（StartDragFromInventorySlot会清空来源槽，之后s.data会变null）
            string itemID = s.data.id;
            ItemType itemType = s.data.type;
            Sprite iconSprite = s.data.icon;
            if (iconSprite == null) iconSprite = ItemFactory.GetItemIcon(itemID);  // 数据icon缺失时从配置补（单点，可回档）
            Color iconColor = icon.color;   // 读槽内显示Image的当前颜色（Inspector预设色），不硬编码白色
            int count = s.count;

            ItemDragDirector.Instance.StartDragFromInventorySlot(slotType, index, itemID, iconSprite, iconColor, count);

            // 生成跟随鼠标的图标
            if (floatingItemIconPrefab != null && parentCanvas != null)
            {
                GameObject iconObj = Instantiate(floatingItemIconPrefab, parentCanvas.transform);
                FloatingItemIcon floatingIcon = iconObj.GetComponent<FloatingItemIcon>();
                if (floatingIcon == null)
                {
                    // 防御：拖错预制体（拖成商店版FloatingIconInShop）时立刻暴露
                    // Debug.LogError("[拿起] 预制体上没挂FloatingItemIcon脚本！拖错了，请拖 PickingSettings/FloatingItemIcon 预制体，当前=" + floatingItemIconPrefab.name);
                    return;
                }
                floatingIcon.Initialize(
                    ItemDragDirector.ToFixedItemType(itemType),
                    itemID, iconSprite, iconColor, parentCanvas, count);
            }
        }

        /// <summary>右键使用槽内药水：发效果消息 → 扣减数量（归零自动清空）</summary>
        private void UsePotionAtSlot()
        {
            var s = InventoryDirector.Instance.GetSlot(slotType, index);
            if (s == null || s.data == null || s.data.type != ItemType.Potion) return;

            MessageBus.Publish(new PotionUsedMessage { PotionID = s.data.id });  // PotionBuffDirector 应用效果
            InventoryDirector.Instance.ConsumeAt(slotType, index, 1);           // 扣减，归零自动清空
        }
    }
}
