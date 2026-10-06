using UnityEngine;

namespace Game.Items
{
    /// <summary>
    /// 物品工厂：模板查询 + 数据建立。
    /// 不属于 Director（Director 不负责模板建立与数据流转），供业务层（InventoryPlacer 等）调用。
    /// </summary>
    public static class ItemFactory
    {
        /// <summary>按ID从配置取物品图标（各Config的icon可能为null，后续补齐）</summary>
        public static Sprite GetItemIcon(string itemID)
        {
            var w = WeaponConfig.GetWeapon(itemID);      if (w != null) return w.icon;
            var e = EquipmentConfig.GetEquipment(itemID); if (e != null) return e.Icon;
            var a = AccessoryConfig.Get(itemID);         if (a != null) return a.icon;
            var c = CollectibleConfig.Get(itemID);       if (c != null) return c.icon;
            var b = BulletConfig.GetBullet(itemID);      if (b != null) return b.icon;
            var p = PotionConfig.GetPotion(itemID);      if (p != null) return p.icon;
            return null;
        }

        /// <summary>堆叠上限（子弹可堆叠，其余 1）</summary>
        public static int GetMaxStack(ItemType t) => t == ItemType.Bullet ? 60 : 1;

        /// <summary>组装物品数据（唯一创建 ItemData 的地方）。icon 为空时回退配置图标。</summary>
        public static ItemData CreateItemData(string itemID, Sprite icon = null)
        {
            var type = InventoryDirector.GetItemType(itemID);
            return new ItemData
            {
                id = itemID,
                type = type,
                icon = icon != null ? icon : GetItemIcon(itemID),
                maxStack = GetMaxStack(type)
            };
        }
    }
}
