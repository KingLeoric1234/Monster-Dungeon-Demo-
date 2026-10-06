using UnityEngine;
using System.Collections.Generic;

namespace Game.Items
{
    /// <summary>物品种类</summary>
    public enum ItemType
    {
        Armor,        // 装备（护甲店卖的）
        Weapon,       // 武器
        Bullet,       // 子弹
        Potion,       // 药水
        Accessory,    // 饰品（任务奖品，放Head/Hand）
        Collectible    // 收集物
    }

    /// <summary>槽位类型</summary>
    public enum BagSlotType
    {
        Head,         // 饰品槽
        Hand,         // 饰品槽
        Armor,        // 护甲槽
        BackPack,     // 背包
        Safe          // 保险箱
    }

    /// <summary>
    /// 物品定义（说明书）。所有同款物品共享一份。
    /// 以后改用ScriptableObject，现在先静态写。
    /// </summary>
    public class ItemData
    {
        public string id;
        public string displayName;
        public ItemType type;
        public Sprite icon;
        public int maxStack = 1;
        public string description;
    }

    /// <summary>背包里的实物：说明书+数量</summary>
    public class ItemInstance
    {
        public ItemData data;
        public int count;

        public bool IsEmpty => data == null;
        public void Clear() { data = null; count = 0; }
    }

    /// <summary>槽位规则：哪些类型能放哪些槽</summary>
    public static class SlotRules
    {
        public static bool CanHold(BagSlotType slot, ItemType type)
        {
            switch (slot)
            {
                case BagSlotType.Head:
                case BagSlotType.Hand:
                    return type == ItemType.Accessory;

                case BagSlotType.Armor:
                case BagSlotType.BackPack:
                    return true; // 任意

                case BagSlotType.Safe:
                    // 装备和武器（除子弹）不能放
                    if (type == ItemType.Armor) return false;
                    if (type == ItemType.Weapon) return false;
                    return true;

                default: return false;
            }
        }
    }
}
