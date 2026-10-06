using UnityEngine;
using UnityEngine.UI;

namespace Game.Items
{
    /// <summary>浮动图标跟随鼠标，右键取消</summary>
    public class FloatingIconFollow : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        private Canvas parentCanvas;
        private string itemID;
        private FixedItemType fixedType;

        public void Init(string id, Sprite icon, Color color, Canvas canvas, FixedItemType fixedType = FixedItemType.None)
        {
            itemID = id;
            parentCanvas = canvas;
            this.fixedType = fixedType;
            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
                iconImage.color = color;
                iconImage.raycastTarget = false;
            }
            transform.SetAsLastSibling();
        }

        private void Start()
        {
            if (iconImage != null) iconImage.enabled = false;
            StartCoroutine(DelaySnap());
        }

        private System.Collections.IEnumerator DelaySnap()
        {
            yield return null;
            SnapToMouse();
            if (iconImage != null) iconImage.enabled = true;
        }

        private void SnapToMouse()
        {
            if (parentCanvas != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.transform as RectTransform,
                    Input.mousePosition, parentCanvas.worldCamera, out var mousePos);
                transform.localPosition = mousePos;
            }
        }

        private void Update()
        {
            SnapToMouse();
            if (Input.GetMouseButtonDown(1)) Destroy(gameObject);
            if (Input.GetMouseButtonDown(0))
            {
                var hit = ShopSlotDetector.Detect(parentCanvas);
                // 放置前升起 IsPlacing 守卫：同一次点击的"抬起"事件会被槽位 OnPointerClick 收到，
                // 守卫让它在 IsPlacing 期间直接 return，避免把刚放入的物品误判为"从槽里拿出拖拽"
                if (ItemDragDirector.Instance != null) ItemDragDirector.Instance.StartPlacing();
                // Debug.Log($"[Follow] hit={hit.type}, slot={hit.fixedSlot}");
                if (hit.type == ShopSlotDetector.HitType.None)
                {
                    Destroy(gameObject);
                    return;
                }
                if (hit.type == ShopSlotDetector.HitType.Fixed && hit.fixedSlot != null)
                {
                    FixedSlotPlacer.Place(hit.fixedSlot, itemID, iconImage.sprite, iconImage.color, fixedType);
                    Destroy(gameObject);
                }
                else if (hit.type == ShopSlotDetector.HitType.Equip && hit.equipSlot != null)
                {
                    // Debug.Log("[购买确认]功能正常");
                    EquipSlotPlacer.Place(hit.equipSlot, itemID, iconImage.sprite, iconImage.color);
                    Destroy(gameObject);
                }
                else if (hit.type == ShopSlotDetector.HitType.Inventory && hit.inventorySlot != null)
                {
                    InventoryPlacer.Buy(hit.inventorySlot, itemID, iconImage.sprite);
                    Destroy(gameObject);
                }
            }
        }
    }
}
