using UnityEngine;
using System.Collections.Generic;
using Game.Core;

namespace Game.Items
{
    /// <summary>
    /// 背包数据总管：每个槽位类型存一个列表。
    /// Head/Hand/Armor各按装备决定槽数，BackPack/Safe同理。
    /// </summary>
    public class InventoryDirector : MonoBehaviour
    {
        public static InventoryDirector Instance { get; private set; }

        private readonly Dictionary<BagSlotType, List<ItemInstance>> slots = new()
        {
            { BagSlotType.Head,     new List<ItemInstance>() },
            { BagSlotType.Hand,     new List<ItemInstance>() },
            { BagSlotType.Armor,    new List<ItemInstance>() },
            { BagSlotType.BackPack, new List<ItemInstance>() },
            { BagSlotType.Safe,     new List<ItemInstance>() },
        };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        // ── 槽数管理 ──────────────────────────────────────
        public int GetSlotCount(BagSlotType type) => slots[type].Count;

        public void SetSlotCount(BagSlotType type, int count)
        {
            var list = slots[type];
            while (list.Count < count) list.Add(new ItemInstance());
            while (list.Count > count && list[list.Count - 1].IsEmpty) list.RemoveAt(list.Count - 1);
            Notify(type);
        }

        // ── 取/放 ──────────────────────────────────────
        public ItemInstance GetSlot(BagSlotType type, int index)
        {
            var list = slots[type];
            if (index < 0 || index >= list.Count) return null;
            return list[index];
        }

        public void Swap(BagSlotType fromType, int fromIdx, BagSlotType toType, int toIdx)
        {
            var a = GetSlot(fromType, fromIdx);
            var b = GetSlot(toType, toIdx);
            if (a == null || b == null) return;
            (a, b) = (b, a);
            slots[fromType][fromIdx] = a;
            slots[toType][toIdx] = b;
            Notify(fromType); Notify(toType);
        }

        /// <summary>原子写入：把组装好的物品放入指定格子并广播（数据层唯一写入入口）。只查数据完整性，不懂业务规则。</summary>
        public bool PlaceAt(BagSlotType type, int index, ItemData data, int count = 1)
        {
            if (!slots.TryGetValue(type, out var list)) return false;
            if (index < 0 || index >= list.Count) return false;        // 槽索引要正确
            if (!list[index].IsEmpty) return false;                    // 槽必须空

            list[index].data = data;
            list[index].count = count;
            Notify(type);
            return true;
        }

        /// <summary>往已有物品的格子叠加数量（数据层原子操作），返回实际叠加量。</summary>
        public int StackAt(BagSlotType type, int index, int addCount)
        {
            var s = GetSlot(type, index);
            if (s == null || s.data == null) return 0;
            int max = s.data.maxStack;
            int add = Mathf.Max(0, Mathf.Min(addCount, max - s.count));
            s.count += add;
            if (add > 0) Notify(type);
            return add;
        }

        /// <summary>消耗格内物品数量（药水使用等），归零则清空格子，返回剩余数量（-1=无效格）。</summary>
        public int ConsumeAt(BagSlotType type, int index, int amount = 1)
        {
            var s = GetSlot(type, index);
            if (s == null || s.data == null) return -1;   // 无效格
            s.count -= amount;
            if (s.count <= 0) { s.Clear(); s.count = 0; }
            Notify(type);
            return s.count;
        }

        /// <summary>清空格子（拖拽拿起物品时用，取消拖拽时放回）。</summary>
        public void ClearAt(BagSlotType type, int index)
        {
            var s = GetSlot(type, index);
            if (s == null || s.IsEmpty) return;
            s.Clear();
            Notify(type);
        }

        public bool HasItem(string itemID)
        {
            foreach (var list in slots.Values)
                foreach (var s in list)
                    if (s.data != null && s.data.id == itemID) return true;
            return false;
        }

        public static ItemType GetItemType(string itemID)
        {
            if (itemID == null) return ItemType.Collectible;
            if (WeaponConfig.GetWeapon(itemID) != null) return ItemType.Weapon;
            if (EquipmentConfig.GetEquipment(itemID) != null) return ItemType.Armor;
            if (AccessoryConfig.Get(itemID) != null) return ItemType.Accessory;
            if (CollectibleConfig.Get(itemID) != null) return ItemType.Collectible;
            if (BulletConfig.GetBullet(itemID) != null) return ItemType.Bullet;
            if (PotionConfig.GetPotion(itemID) != null) return ItemType.Potion;
            return ItemType.Collectible;
        }

        private void Notify(BagSlotType slot)
        {
            MessageBus.Publish(new InventorySlotChangedMessage { Slot = slot });
        }
    }
}
