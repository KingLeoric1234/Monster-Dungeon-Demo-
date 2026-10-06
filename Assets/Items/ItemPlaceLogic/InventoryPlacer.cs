using UnityEngine;
using Game.Core;

namespace Game.Items
{
    /// <summary>
    /// 背包业务编排：物品入库的唯一入口（购买放置 + 自动入包）。
    /// 校验规则 → 组装数据（ItemFactory）→ 写入数据层（PlaceAt/StackAt）。
    /// </summary>
    public static class InventoryPlacer
    {
        /// <summary>购买放置：校验 → 扣钱 → 组装 → 写入玩家点击的指定槽</summary>
        public static bool Buy(InventorySlotUI slot, string id, Sprite icon)
        {
            if (slot == null) return false;
            // 防御：数据层单例未就绪（未挂载）时不扣钱，避免"钱扣了物品没进"
            if (ItemDirector.Instance == null || InventoryDirector.Instance == null) return false;

            // ① 先检查：这个槽收不收这类物品（不支持的槽 → 直接 return，不扣钱）
            var type = InventoryDirector.GetItemType(id);
            if (!SlotRules.CanHold(slot.GetSlotType(), type)) return false;

            // ② 再扣钱（跳过自动入包，避免"自动塞一次 + 指定槽再放一次"重复）
            if (!ItemDirector.Instance.BuyItem(id, 1, false)) return false;

            // ③ 组装数据 → 写入玩家点击的指定槽
            var data = ItemFactory.CreateItemData(id, icon);
            return InventoryDirector.Instance.PlaceAt(slot.GetSlotType(), slot.GetIndex(), data, 1);
        }

        /// <summary>自动入包：找空槽放物品（子弹先堆叠），成功返回true</summary>
        public static bool AddItem(string itemID, int count = 1)
        {
            if (InventoryDirector.Instance == null) return false;
            var type = InventoryDirector.GetItemType(itemID);

            // 子弹先堆叠
            if (type == ItemType.Bullet)
            {
                foreach (BagSlotType s in new[] { BagSlotType.BackPack, BagSlotType.Safe })
                    if (TryStack(s, itemID, count)) return true;
            }

            // 找空槽
            BagSlotType[] priority = { BagSlotType.BackPack, BagSlotType.Safe };
            foreach (var slot in priority)
            {
                if (!SlotRules.CanHold(slot, type)) continue;
                int listCount = InventoryDirector.Instance.GetSlotCount(slot);
                for (int i = 0; i < listCount; i++)
                {
                    var s = InventoryDirector.Instance.GetSlot(slot, i);
                    if (s == null || !s.IsEmpty) continue;
                    var data = ItemFactory.CreateItemData(itemID);
                    int put = Mathf.Min(count, data.maxStack);
                    InventoryDirector.Instance.PlaceAt(slot, i, data, put);
                    return true;
                }
            }

            MessageBus.Publish(new InventoryFullMessage { ItemID = itemID });
            return false;
        }

        /// <summary>同区域堆叠同ID物品，返回是否已全部放下</summary>
        private static bool TryStack(BagSlotType slot, string itemID, int count)
        {
            int listCount = InventoryDirector.Instance.GetSlotCount(slot);
            for (int i = 0; i < listCount; i++)
            {
                var s = InventoryDirector.Instance.GetSlot(slot, i);
                if (s == null || s.data == null || s.data.id != itemID) continue;
                int added = InventoryDirector.Instance.StackAt(slot, i, count);
                count -= added;
                if (count <= 0) return true;
            }
            return count <= 0;
        }
    }
}
