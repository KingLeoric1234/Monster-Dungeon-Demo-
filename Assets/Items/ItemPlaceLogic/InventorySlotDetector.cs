using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.Items
{
    /// <summary>新背包槽具体检测</summary>
    public static class InventorySlotDetector
    {
        public static InventorySlotUI Detect(Canvas canvas)
        {
            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null) return null;
            var ev = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var results = new System.Collections.Generic.List<RaycastResult>();
            raycaster.Raycast(ev, results);
            foreach (var r in results)
                return r.gameObject.GetComponentInParent<InventorySlotUI>();
            return null;
        }
    }
}
