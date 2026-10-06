using UnityEngine;
using System.Collections.Generic;

namespace Game.Items
{
    /// <summary>装备部位</summary>
    public enum EquipmentSlotType
    {
        Head,   // 头部
        Hand,   // 手部
        Body,   // 护甲
        Back    // 背包
    }

    /// <summary>装备数据</summary>
    public class EquipmentData
    {
        public string ID;
        public string Name;
        public EquipmentSlotType Slot;
        public int Price;
        public float Weight;
        public int Armor;
        public int accessorySlots = 0;  // 饰品槽数（头/手）
        public int backpackSlots = 0;   // 背包格数
        public int safeSlots = 0;       // 安全箱格数
        public float MoveSpeedPenalty;
        public string Description;
        public Sprite Icon;
    }

    /// <summary>
    /// 装备配置：所有装备数据和玩家基础属性都在这里，校准数值只改这个文件。
    /// </summary>
    public static class EquipmentConfig
    {
        // ── 玩家基础槽位（不带任何装备时） ──────────────────────
        public const int BaseAccessorySlots = 0;
        public const int BaseBackpackSlots = 0;
        public const int BaseSafeSlots = 1;

        /// <summary>基础护甲值</summary>
        public const int BaseArmor = 0;

        /// <summary>玩家初始金币</summary>
        public const int InitialGold = 1000000;

        // ── 所有装备数据 ────────────────────────────────────────

        private static Dictionary<string, EquipmentData> _allEquipment;

        /// <summary>所有装备（按ID索引）</summary>
        public static Dictionary<string, EquipmentData> AllEquipment
        {
            get
            {
                if (_allEquipment == null) InitEquipment();
                return _allEquipment;
            }
        }

        /// <summary>初始化所有装备数据</summary>
        private static void InitEquipment()
        {
            _allEquipment = new Dictionary<string, EquipmentData>
            {
                // ── 手部装备 ──
                {
                    "leather_gloves",
                    new EquipmentData
                    {
                        ID = "leather_gloves",
                        Name = "Leather Gloves",
                        Slot = EquipmentSlotType.Hand,
                        Price = 500,
                        Weight = 0.2f,
                        Armor = 0,
                        accessorySlots = 1,
                        safeSlots = 1,
                        MoveSpeedPenalty = 0f,
                        Description = "Leather gloves: +1 accessory slot, +1 safe slot."
                    }
                },
                {
                    "tactical_bracers",
                    new EquipmentData
                    {
                        ID = "tactical_bracers",
                        Name = "Tactical Bracers",
                        Slot = EquipmentSlotType.Hand,
                        Price = 5000,
                        Weight = 0.5f,
                        Armor = 0,
                        accessorySlots = 2,
                        safeSlots = 3,
                        MoveSpeedPenalty = 0f,
                        Description = "Tactical bracers: +2 accessory, +3 safe."
                    }
                },

                // ── 头部装备 ──
                {
                    "safety_helmet",
                    new EquipmentData
                    {
                        ID = "safety_helmet",
                        Name = "Safety Helmet",
                        Slot = EquipmentSlotType.Head,
                        Price = 500,
                        Weight = 0.5f,
                        Armor = 5,
                        accessorySlots = 1,
                        MoveSpeedPenalty = 0f,
                        Description = "Safety helmet: +5 armor, +1 accessory."
                    }
                },
                {
                    "tactical_helmet",
                    new EquipmentData
                    {
                        ID = "tactical_helmet",
                        Name = "Tactical Helmet",
                        Slot = EquipmentSlotType.Head,
                        Price = 5000,
                        Weight = 0.9f,
                        Armor = 15,
                        accessorySlots = 2,
                        MoveSpeedPenalty = 0.01f,
                        Description = "Tactical helmet: +15 armor, +2 accessory, -1% speed."
                    }
                },

                // ── 护甲 ──
                {
                    "light_vest",
                    new EquipmentData
                    {
                        ID = "light_vest",
                        Name = "Light Vest",
                        Slot = EquipmentSlotType.Body,
                        Price = 500,
                        Weight = 1f,
                        Armor = 10,
                        accessorySlots = 1,
                        MoveSpeedPenalty = 0f,
                        Description = "Light vest: +10 armor, +1 accessory."
                    }
                },
                {
                    "tactical_armor",
                    new EquipmentData
                    {
                        ID = "tactical_armor",
                        Name = "Tactical Armor",
                        Slot = EquipmentSlotType.Body,
                        Price = 5000,
                        Weight = 4f,
                        Armor = 50,
                        accessorySlots = 2,
                        safeSlots = 3,
                        MoveSpeedPenalty = 0.02f,
                        Description = "Tactical armor: +50 armor, +2 accessory, +3 safe, -2% speed."
                    }
                },

                // ── 背包 ──
                {
                    "light_bag",
                    new EquipmentData
                    {
                        ID = "light_bag",
                        Name = "Light Satchel",
                        Slot = EquipmentSlotType.Back,
                        Price = 500,
                        Weight = 0.5f,
                        Armor = 0,
                        backpackSlots = 3,
                        MoveSpeedPenalty = 0f,
                        Description = "Small backpack, 3 slots.",
                    }
                },
                {
                    "tactical_backpack",
                    new EquipmentData
                    {
                        ID = "tactical_backpack",
                        Name = "Tactical Backpack",
                        Slot = EquipmentSlotType.Back,
                        Price = 5000,
                        Weight = 3f,
                        Armor = 0,
                        backpackSlots = 8,
                        MoveSpeedPenalty = 0.01f,
                        Description = "Large backpack, 8 slots.",
                    }
                }
            };
        }

        /// <summary>按ID获取装备数据</summary>
        public static EquipmentData GetEquipment(string id)
        {
            if (AllEquipment.TryGetValue(id, out var data))
                return data;
            return null;
        }

        /// <summary>按部位获取所有装备</summary>
        public static List<EquipmentData> GetEquipmentBySlot(EquipmentSlotType slot)
        {
            var result = new List<EquipmentData>();
            foreach (var eq in AllEquipment.Values)
            {
                if (eq.Slot == slot) result.Add(eq);
            }
            return result;
        }
    }
}

