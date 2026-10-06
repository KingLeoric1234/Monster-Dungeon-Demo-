using System.Collections.Generic;
using UnityEngine;

namespace Game.Items
{
    /// <summary>饰品配置：头部2个，手部3个，护甲3个</summary>
    public static class AccessoryConfig
    {
        public class AccessoryData
        {
            public string id;
            public string displayName;
            public BagSlotType slot;          // 能戴在哪
            public string description;
            public Sprite icon;
            // 加成
            public int bonusArmor;
            public float bonusSpeed;
            public int bonusMaxHp;
            public float bonusDamagePct;
        }

        private static readonly Dictionary<string, AccessoryData> items = new Dictionary<string, AccessoryData>
        {
            // ===== 头部饰品（2个）=====
            { "antique_lorgnette", new AccessoryData {
                id = "antique_lorgnette", displayName = "Antique Lorgnette", slot = BagSlotType.Head,
                description = "Previous owner survived three assassinations. +5% damage.",
                bonusDamagePct = 0.05f } },
            { "witch_hairpin", new AccessoryData {
                id = "witch_hairpin", displayName = "Witch's Broken Pin", slot = BagSlotType.Head,
                description = "Jingles softly when walking. +10 max HP.",
                bonusMaxHp = 10 } },

            // ===== Hand (3) =====
            { "gambler_dice", new AccessoryData {
                id = "gambler_dice", displayName = "Gambler's Dice", slot = BagSlotType.Hand,
                description = "Always rolls what you want. +5% move speed.",
                bonusSpeed = 0.05f } },
            { "blacksmith_bracer", new AccessoryData {
                id = "blacksmith_bracer", displayName = "Blacksmith's Bracer", slot = BagSlotType.Hand,
                description = "Battered flat by countless hammers. +2 armor.",
                bonusArmor = 2 } },
            { "gambling_ring", new AccessoryData {
                id = "gambling_ring", displayName = "Wager Ring", slot = BagSlotType.Hand,
                description = "Left behind by a broke gambler. +3% damage.",
                bonusDamagePct = 0.03f } },

            // ===== Armor (3) =====
            { "merchant_pouch", new AccessoryData {
                id = "merchant_pouch", displayName = "Trader's Pouch", slot = BagSlotType.Armor,
                description = "Bulging with pebbles. +3 max HP.",
                bonusMaxHp = 3 } },
            { "hunter_feather", new AccessoryData {
                id = "hunter_feather", displayName = "Hunter's Feather", slot = BagSlotType.Armor,
                description = "Worn on the lapel. +3% move speed.",
                bonusSpeed = 0.03f } },
            { "broken_medal", new AccessoryData {
                id = "broken_medal", displayName = "Chipped Medal", slot = BagSlotType.Armor,
                description = "Honor gone, iron remains. +1 armor.",
                bonusArmor = 1 } },
        };

        public static AccessoryData Get(string id)
        {
            return items.TryGetValue(id, out var d) ? d : null;
        }

        public static IEnumerable<AccessoryData> All => items.Values;
    }
}
