using System.Collections.Generic;
using UnityEngine;

namespace Game.Items
{
    /// <summary>收集物配置：10个任务物品</summary>
    public static class CollectibleConfig
    {
        public class CollectibleData
        {
            public string id;
            public string displayName;
            public string description;
            public Sprite icon;
        }

        private static readonly Dictionary<string, CollectibleData> items = new Dictionary<string, CollectibleData>
        {
            { "rusty_key", new CollectibleData {
                id = "rusty_key", displayName = "Rusty Key",
                description = "Who knows which lock it opens. Probably useful." } },
            { "old_letter", new CollectibleData {
                id = "old_letter", displayName = "Damp Letter",
                description = "Ink smudged, only \"...sorry\" is legible." } },
            { "slime_jelly", new CollectibleData {
                id = "slime_jelly", displayName = "Slime Jelly",
                description = "Slimy, clear, elastic. Alchemy material." } },
            { "broken_gem", new CollectibleData {
                id = "broken_gem", displayName = "Broken Gem",
                description = "There should be two. Now there is one and a half." } },
            { "hunter_tag", new CollectibleData {
                id = "hunter_tag", displayName = "Hunter's Tag",
                description = "Engraved with a name. Its owner isn't here." } },
            { "moldy_bread", new CollectibleData {
                id = "moldy_bread", displayName = "Moldy Bread",
                description = "Hard enough to use as a hammer. Don't eat it." } },
            { "cracked_mask", new CollectibleData {
                id = "cracked_mask", displayName = "Cracked Mask",
                description = "Split down the middle. Still looks angry." } },
            { "war_banner", new CollectibleData {
                id = "war_banner", displayName = "Banner Fragment",
                description = "Nobody recognizes the crest anymore." } },
            { "mysterious_coin", new CollectibleData {
                id = "mysterious_coin", displayName = "Strange Coin",
                description = "Same face on both sides." } },
            { "boss_horn", new CollectibleData {
                id = "boss_horn", displayName = "Boss's Broken Horn",
                description = "Still warm when you pried it off." } },
        };

        public static CollectibleData Get(string id)
        {
            return items.TryGetValue(id, out var d) ? d : null;
        }

        public static IEnumerable<CollectibleData> All => items.Values;
    }
}
