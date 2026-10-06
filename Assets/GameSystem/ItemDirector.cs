using UnityEngine;
using System.Collections.Generic;
using Game.Core;
using Game.Items;

namespace Game.Items
{
    /// <summary>
    /// 物品/装备全局管理：金币、购买、已拥有装备、穿戴装备。
    /// 背包格子数据由InventoryDirector单独管理。
    /// 数据从EquipmentConfig读取，不在这硬编码。
    /// </summary>
    public class ItemDirector : MonoBehaviour
    {
        public static ItemDirector Instance { get; private set; }

        [Header("玩家数据")]
        [SerializeField] private int currentGold;                          // 当前金币
        [SerializeField] private List<string> ownedEquipmentIDs = new List<string>();  // 已拥有的装备ID
        [SerializeField] private Dictionary<EquipmentSlotType, string> equipped = new Dictionary<EquipmentSlotType, string>();  // 当前穿戴的装备

        [Header("固定槽数据")]
        [SerializeField] private Dictionary<FixedSlotType, (FixedItemType type, string id)> fixedSlots = new Dictionary<FixedSlotType, (FixedItemType, string)>();  // 固定槽里的物品
        private Dictionary<FixedSlotType, (Sprite icon, Color color)> fixedSlotIcons = new Dictionary<FixedSlotType, (Sprite, Color)>();  // 固定槽图标+颜色缓存

        /// <summary>当前金币</summary>
        public int CurrentGold => currentGold;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // 先移到根层级，再DontDestroyOnLoad（否则会报警告）
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            // 初始化金币
            currentGold = EquipmentConfig.InitialGold;
        }

        // ── 金币管理 ──────────────────────────────────────

        /// <summary>增加金币</summary>
        public void AddGold(int amount)
        {
            currentGold += amount;
            MessageBus.Publish(new GoldChangedMessage { NewGold = currentGold });
        }

        /// <summary>减少金币（成功返回true）</summary>
        public bool SpendGold(int amount)
        {
            if (currentGold < amount) return false;
            currentGold -= amount;
            MessageBus.Publish(new GoldChangedMessage { NewGold = currentGold });
            return true;
        }

        // ── 购买 ──────────────────────────────────────

        /// <summary>购买装备（成功返回true）</summary>
        /// <summary>通用购买：护甲/武器/药水/子弹</summary>
        public bool BuyItem(string itemID, int count = 1, bool addToInventory = true)
        {
            int price = GetItemPrice(itemID);
            if (price < 0) return false;
            int totalCost = price * count;
            if (currentGold < totalCost) return false;

            currentGold -= totalCost;
            if (!ownedEquipmentIDs.Contains(itemID)) ownedEquipmentIDs.Add(itemID);

            MessageBus.Publish(new GoldChangedMessage { NewGold = currentGold });
            MessageBus.Publish(new EquipmentPurchasedMessage { EquipmentID = itemID });

            if (addToInventory && InventoryDirector.Instance != null)
                InventoryPlacer.AddItem(itemID, count);

            return true;
        }

        private int GetItemPrice(string itemID)
        {
            var eq = EquipmentConfig.GetEquipment(itemID);
            if (eq != null) return eq.Price;
            var w = WeaponConfig.GetWeapon(itemID);
            if (w != null) return w.price;
            var p = PotionConfig.GetPotion(itemID);
            if (p != null) return p.price;
            var b = BulletConfig.GetBullet(itemID);
            if (b != null) return b.price;
            return -1;
        }

        public bool BuyEquipment(string equipmentID, bool addToInventory = true)
        {
            var data = EquipmentConfig.GetEquipment(equipmentID);
            if (data == null)
            {
                //Debug.LogWarning($"[ItemDirector] 装备不存在: {equipmentID}");
                return false;
            }

            if (currentGold < data.Price)
            {
                //Debug.Log($"[ItemDirector] 金币不足，需要{data.Price}，当前{currentGold}");
                return false;
            }

            // 扣钱
            currentGold -= data.Price;

            // 加入已拥有
            if (!ownedEquipmentIDs.Contains(equipmentID))
                ownedEquipmentIDs.Add(equipmentID);
            //Debug.Log($"[ItemDirector] 购买成功: {data.Name}，剩余金币: {currentGold}");

            MessageBus.Publish(new GoldChangedMessage { NewGold = currentGold });
            MessageBus.Publish(new EquipmentPurchasedMessage { EquipmentID = equipmentID });

            // 同步进背包槽（拖拽放置装备时传false，避免"进背包+上槽"重复登记）
            if (addToInventory && InventoryDirector.Instance != null)
                InventoryPlacer.AddItem(equipmentID, 1);

            return true;
        }

        /// <summary>是否已拥有某装备</summary>
        public bool OwnsEquipment(string equipmentID)
        {
            return ownedEquipmentIDs.Contains(equipmentID);
        }

        /// <summary>移除已拥有（装备被丢弃后允许再次购买）</summary>
        public void RemoveOwnedEquipment(string equipmentID)
        {
            ownedEquipmentIDs.Remove(equipmentID);
        }

        /// <summary>测试用：直接获得装备</summary>
        public void CheatAddItem(string equipmentID)
        {
            if (!ownedEquipmentIDs.Contains(equipmentID))
                ownedEquipmentIDs.Add(equipmentID);
        }

        /// <summary>获取所有已拥有的装备</summary>
        public List<EquipmentData> GetOwnedEquipment()
        {
            var result = new List<EquipmentData>();
            foreach (var id in ownedEquipmentIDs)
            {
                var data = EquipmentConfig.GetEquipment(id);
                if (data != null) result.Add(data);
            }
            return result;
        }

        // ── 穿戴 ──────────────────────────────────────

        /// <summary>穿戴装备</summary>
        public void Equip(string equipmentID)
        {
            var data = EquipmentConfig.GetEquipment(equipmentID);
            if (data == null) return;
            if (!ownedEquipmentIDs.Contains(equipmentID)) return;

            equipped[data.Slot] = equipmentID;
            // Debug.Log($"[ItemDirector] 穿戴: {data.Name}");
            MessageBus.Publish(new EquipmentEquippedMessage { EquipmentID = equipmentID, Slot = data.Slot });
        }

        /// <summary>卸下某部位装备</summary>
        public void Unequip(EquipmentSlotType slot)
        {
            if (equipped.ContainsKey(slot))
            {
                equipped.Remove(slot);
                MessageBus.Publish(new EquipmentEquippedMessage { EquipmentID = null, Slot = slot });
            }
        }

        /// <summary>获取当前穿戴的装备</summary>
        public string GetEquipped(EquipmentSlotType slot)
        {
            return equipped.TryGetValue(slot, out var id) ? id : null;
        }

        // ── 固定槽管理 ──────────────────────────────────────

        /// <summary>往固定槽放物品（可选传图标+颜色，会缓存起来供其他Panel显示）</summary>
        public void SetFixedSlot(FixedSlotType slot, FixedItemType itemType, string itemID, Sprite icon = null, Color? color = null)
        {
            fixedSlots[slot] = (itemType, itemID);
            if (icon != null)
            {
                fixedSlotIcons[slot] = (icon, color ?? Color.white);
            }
            else if (itemType == FixedItemType.None)
            {
                fixedSlotIcons.Remove(slot);
            }
            MessageBus.Publish(new FixedSlotChangedMessage { Slot = slot, ItemType = itemType, ItemID = itemID });
        }

        /// <summary>获取固定槽的图标+颜色（从缓存读取）</summary>
        public (Sprite icon, Color color) GetFixedSlotIcon(FixedSlotType slot)
        {
            return fixedSlotIcons.TryGetValue(slot, out var data) ? data : (null, Color.white);
        }

        /// <summary>获取固定槽里的物品（类型+ID）</summary>
        public (FixedItemType type, string id) GetFixedSlot(FixedSlotType slot)
        {
            return fixedSlots.TryGetValue(slot, out var data) ? data : (FixedItemType.None, null);
        }

        /// <summary>清空固定槽</summary>
        public void ClearFixedSlot(FixedSlotType slot)
        {
            if (fixedSlots.ContainsKey(slot))
            {
                fixedSlots.Remove(slot);
            }
            MessageBus.Publish(new FixedSlotChangedMessage { Slot = slot, ItemType = FixedItemType.None, ItemID = null });
        }

        /// <summary>计算当前总护甲和移速惩罚</summary>
        public (int totalArmor, float totalSpeedPenalty) CalculateTotalStats()
        {
            int armor = EquipmentConfig.BaseArmor;
            float speedPenalty = 0f;

            foreach (var kvp in equipped)
            {
                var data = EquipmentConfig.GetEquipment(kvp.Value);
                if (data == null) continue;
                armor += data.Armor;
                speedPenalty += data.MoveSpeedPenalty;
            }

            return (armor, Mathf.Clamp01(speedPenalty));
        }
    }
}

