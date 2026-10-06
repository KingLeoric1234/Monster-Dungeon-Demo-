using UnityEngine;
using Game.UI;

namespace Game.Items
{
    /// <summary>放置到固定槽</summary>
    public static class FixedSlotPlacer
    {
        public static void Place(FixedSlotUI slot, string id, Sprite icon, Color color, FixedItemType type)
        {
            // Debug.Log($"[Placer] Place id={id}, type={type}, slot={slot.name}");
            // 先问槽位收不收：不收直接 return，避免"钱扣了但物品没放进去"
            if (slot != null && !slot.CanAcceptItem(type)) { return; }
            // 扣钱（不入包：放固定槽只登记固定槽，避免重复入包到背包）
            if (!ItemDirector.Instance.BuyItem(id, 1, false)) { /* Debug.Log("[Placer] 扣钱失败") */ return; }
            slot.PlaceItem(type, id, icon, color);
        }
    }
}
