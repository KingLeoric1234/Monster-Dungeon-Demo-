using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.UI;

namespace Game.Items
{
    /// <summary>检测落点是哪种槽，返回槽对象</summary>
    public static class ShopSlotDetector
    {
        public enum HitType { None, Fixed, Equip, Inventory }

        public struct HitResult
        {
            public HitType type;
            public FixedSlotUI fixedSlot;
            public InventorySlotUI inventorySlot;
            public EquipmentSlotUI equipSlot;
        }

        public static HitResult Detect(Canvas canvas)
        {
            // 全局射线：EventSystem.RaycastAll 覆盖所有 Canvas（商店/状态面板/背包）
            // 不能只用单个 Canvas 的 GraphicRaycaster——它只能命中自己 Canvas 下的 UI
            var ev = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(ev, results);
            // Debug.Log($"[Detect] 检测成功！命中 {results.Count} 个UI");

            // 遍历所有UI物体
            foreach (var r in results)
            {
                var go = r.gameObject;
                var fixedSlot = go.GetComponentInParent<FixedSlotUI>();
                if (fixedSlot != null)
                    return new HitResult { type = HitType.Fixed, fixedSlot = fixedSlot };

                var invSlot = go.GetComponentInParent<InventorySlotUI>();
                if (invSlot != null)
                    return new HitResult { type = HitType.Inventory, inventorySlot = invSlot };

                var equipSlot = go.GetComponentInParent<EquipmentSlotUI>();
                if (equipSlot != null)
                {
                    // Debug.Log($"[Detect] 命中装备槽 equipSlot={equipSlot.name}");
                    return new HitResult { type = HitType.Equip, equipSlot = equipSlot };
                }
                else
                {
                    // Debug.Log($"[Detect] go={go.name} 的 equipSlot 是空值！");
                }
            }
            return new HitResult { type = HitType.None };
        }
    }
}
