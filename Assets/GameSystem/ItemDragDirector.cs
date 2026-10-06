using UnityEngine;
using Game.Items;

namespace Game.Items
{
    /// <summary>
    /// 物品拖拽Director（单例）。
    /// 管"当前正在拖拽的物品"这个临时状态。
    /// 跟ItemDirector的区别：ItemDirector管永久数据，这个管临时拖拽状态。
    /// </summary>
    public class ItemDragDirector : MonoBehaviour
    {
        public static ItemDragDirector Instance { get; private set; }

        // 当前正在拖拽的物品
        private bool isDragging = false;
        private FixedItemType draggedItemType;
        private string draggedItemID;
        private Sprite draggedIcon;
        private Color draggedColor;
        private int draggedCount = 1;  // 拖拽数量（背包物品可能 >1，固定槽/装备槽=1）
        private FixedSlotType sourceSlot;  // 来源槽（固定槽）
        private EquipmentSlotType sourceEquipSlot;  // 来源槽（装备槽）
        private bool isFromFixedSlot = false;  // 是否来自固定槽
        private bool isFromInventory = false;  // 是否来自背包槽
        private BagSlotType sourceInventoryType;   // 来源背包区域
        private int sourceInventoryIndex;          // 来源背包索引

        /// <summary>是否正在拖拽物品</summary>
        public bool IsDragging => isDragging;

        /// <summary>是否正在放置物品（从商店买的东西放置时，防止槽口点击事件冲突）</summary>
        public static bool IsPlacing { get; private set; }

        /// <summary>开始放置状态，0.4秒后自动结束（防止点击事件冲突）</summary>
        public void StartPlacing()
        {
            IsPlacing = true;
            CancelInvoke(nameof(EndPlacing));
            Invoke(nameof(EndPlacing), 0.4f);
        }

        private void EndPlacing()
        {
            IsPlacing = false;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>开始拖拽（从固定槽拿出来）</summary>
        public void StartDragFromFixedSlot(FixedSlotType slot, FixedItemType itemType, string itemID, Sprite icon, Color color)
        {
            isDragging = true;
            isFromFixedSlot = true;
            isFromInventory = false;
            sourceSlot = slot;
            draggedItemType = itemType;
            draggedItemID = itemID;
            draggedIcon = icon;
            draggedColor = color;
            draggedCount = 1;

            // 清空原槽（但不重新计算属性，等拖拽结束再算）
            if (ItemDirector.Instance != null)
            {
                ItemDirector.Instance.ClearFixedSlot(slot);
            }
        }

        /// <summary>开始拖拽（从装备槽拿出来）</summary>
        public void StartDragFromEquipmentSlot(EquipmentSlotType slot, string equipmentID, Sprite icon, Color color)
        {
            isDragging = true;
            isFromFixedSlot = false;
            isFromInventory = false;
            sourceEquipSlot = slot;
            draggedItemType = FixedItemType.Equipment;
            draggedItemID = equipmentID;
            draggedIcon = icon;
            draggedColor = color;
            draggedCount = 1;

            // 卸下原装备（但不重新计算属性，等拖拽结束再算）
            if (ItemDirector.Instance != null)
            {
                ItemDirector.Instance.Unequip(slot);
            }
        }

        /// <summary>开始拖拽（从背包槽拿出来）</summary>
        public void StartDragFromInventorySlot(BagSlotType slotType, int index, string itemID, Sprite icon, Color color, int count)
        {
            isDragging = true;
            isFromFixedSlot = false;
            isFromInventory = true;
            sourceInventoryType = slotType;
            sourceInventoryIndex = index;
            draggedItemType = ToFixedItemType(InventoryDirector.GetItemType(itemID));
            draggedItemID = itemID;
            draggedIcon = icon;
            draggedColor = color;
            draggedCount = count;

            // 清空原背包槽（取消时放回）
            if (InventoryDirector.Instance != null)
            {
                InventoryDirector.Instance.ClearAt(slotType, index);
            }
        }

        /// <summary>获取当前拖拽的物品信息</summary>
        public (FixedItemType type, string id, Sprite icon, Color color) GetDraggedItem()
        {
            return (draggedItemType, draggedItemID, draggedIcon, draggedColor);
        }

        /// <summary>拖拽结束（放置/扔掉/卖掉后调用）</summary>
        public void EndDrag()
        {
            isDragging = false;
            isFromFixedSlot = false;
            isFromInventory = false;
            draggedItemType = FixedItemType.None;
            draggedItemID = null;
            draggedIcon = null;
            draggedColor = Color.white;
            draggedCount = 1;

            // TODO: 在这里重新计算玩家属性
        }

        /// <summary>取消拖拽（物品回到原槽）</summary>
        public void CancelDrag()
        {
            if (!isDragging) return;

            if (isFromFixedSlot)
            {
                // 回到固定槽
                if (ItemDirector.Instance != null)
                {
                    ItemDirector.Instance.SetFixedSlot(sourceSlot, draggedItemType, draggedItemID, draggedIcon, draggedColor);
                }
            }
            else if (isFromInventory)
            {
                // 回到背包槽
                if (InventoryDirector.Instance != null)
                {
                    var data = ItemFactory.CreateItemData(draggedItemID, draggedIcon);
                    InventoryDirector.Instance.PlaceAt(sourceInventoryType, sourceInventoryIndex, data, draggedCount);
                }
            }
            else
            {
                // 回到装备槽
                if (ItemDirector.Instance != null)
                {
                    ItemDirector.Instance.Equip(draggedItemID);
                }
            }

            EndDrag();
        }

        /// <summary>
        /// 交换回填：把目标槽拿出的物品放回拖拽来源槽（来源槽已空）。
        /// 装备来源不会走到这（装备拖拽禁止放到任何槽）。
        /// </summary>
        public void RestoreToSourceSlot(string id, Sprite icon, Color color, int count)
        {
            if (isFromFixedSlot)
            {
                if (ItemDirector.Instance != null)
                {
                    ItemDirector.Instance.SetFixedSlot(sourceSlot, ToFixedItemType(InventoryDirector.GetItemType(id)), id, icon, color);
                }
            }
            else if (isFromInventory)
            {
                if (InventoryDirector.Instance != null)
                {
                    var data = ItemFactory.CreateItemData(id, icon);
                    InventoryDirector.Instance.PlaceAt(sourceInventoryType, sourceInventoryIndex, data, count);
                }
            }
            EndDrag();
        }

        /// <summary>
        /// 交换前校验：某物品能否回填到拖拽来源槽（防止目标物品违反来源槽规则）。
        /// 例：Safe 拿起子弹拖到固定槽（有武器）→ 武器回填 Safe → Safe 不收武器 → false。
        /// </summary>
        public bool CanRestoreToSource(string id)
        {
            if (!isDragging) return false;
            var itemType = InventoryDirector.GetItemType(id);

            if (isFromFixedSlot)
            {
                var fixedType = ToFixedItemType(itemType);
                if (fixedType == FixedItemType.None) return false;   // Collectible 不能放固定槽
                if (sourceSlot == FixedSlotType.HandUse) return true; // 手槽全收
                return fixedType == FixedItemType.Potion || fixedType == FixedItemType.Bullet; // Pocket/Pouch只收Potion/Bullet
            }

            if (isFromInventory)
            {
                return SlotRules.CanHold(sourceInventoryType, itemType);
            }

            return false;  // 装备来源不交换
        }

        /// <summary>背包物品类型 → 固定槽类型（Collectible 无法放固定槽 → None）</summary>
        public static FixedItemType ToFixedItemType(ItemType type)
        {
            switch (type)
            {
                case ItemType.Armor: return FixedItemType.Equipment;
                case ItemType.Weapon: return FixedItemType.Weapon;
                case ItemType.Potion: return FixedItemType.Potion;
                case ItemType.Bullet: return FixedItemType.Bullet;
                case ItemType.Accessory: return FixedItemType.Equipment;  // 饰品按装备类（HandUse可放）
                default: return FixedItemType.None;   // Collectible 不能放固定槽
            }
        }

        /// <summary>固定槽类型 → 背包物品类型</summary>
        public static ItemType ToItemType(FixedItemType type)
        {
            switch (type)
            {
                case FixedItemType.Equipment: return ItemType.Armor;
                case FixedItemType.Weapon: return ItemType.Weapon;
                case FixedItemType.Potion: return ItemType.Potion;
                case FixedItemType.Bullet: return ItemType.Bullet;
                default: return ItemType.Collectible;
            }
        }

        /// <summary>扔掉当前拖拽的物品</summary>
        public void ThrowAway()
        {
            // 装备丢弃：从拥有列表移除，允许再次购买（否则"已拥有"拦截新购买）
            if (draggedItemType == FixedItemType.Equipment && ItemDirector.Instance != null)
            {
                ItemDirector.Instance.RemoveOwnedEquipment(draggedItemID);
            }
            // 直接清空，不做地面箱子逻辑（之后做）
            EndDrag();
        }

        /// <summary>卖掉当前拖拽的物品（返回卖了多少钱）</summary>
        public int Sell()
        {
            int price = 0;

            // 根据物品类型获取售价（暂时按原价的50%算，之后调）
            switch (draggedItemType)
            {
                case FixedItemType.Equipment:
                    var equip = EquipmentConfig.GetEquipment(draggedItemID);
                    price = equip != null ? Mathf.FloorToInt(equip.Price * 0.5f) : 0;
                    break;
                case FixedItemType.Weapon:
                    var weapon = WeaponConfig.GetWeapon(draggedItemID);
                    price = weapon != null ? Mathf.FloorToInt(weapon.price * 0.5f) : 0;
                    break;
                case FixedItemType.Potion:
                    var potion = PotionConfig.GetPotion(draggedItemID);
                    price = potion != null ? Mathf.FloorToInt(potion.price * 0.5f) : 0;
                    break;
                case FixedItemType.Bullet:
                    var bullet = BulletConfig.GetBullet(draggedItemID);
                    price = bullet != null ? Mathf.FloorToInt(bullet.price * 0.5f) : 0;
                    break;
            }

            if (price > 0 && ItemDirector.Instance != null)
            {
                ItemDirector.Instance.AddGold(price);
            }

            EndDrag();
            return price;
        }
    }
}
