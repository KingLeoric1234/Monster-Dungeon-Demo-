using Game.Core;

namespace Game.Items
{
    /// <summary>
    /// 固定槽类型：玩家身上的固定槽位，不随装备变化。
    /// HandUse：手槽，能放所有物品
    /// Pocket：口袋1，只能放Potion和Bullet
    /// Pouch：口袋2，只能放Potion和Bullet
    /// </summary>
    public enum FixedSlotType
    {
        HandUse,    // 手槽
        Pocket,     // 口袋1
        Pouch       // 口袋2
    }

    /// <summary>
    /// 固定槽里的物品类型
    /// </summary>
    public enum FixedItemType
    {
        None,       // 空
        Equipment,  // 装备
        Weapon,     // 武器
        Potion,     // 药水
        Bullet      // 子弹
    }

    /// <summary>
    /// 固定槽变化消息：玩家往固定槽放了/移除了物品时发布
    /// </summary>
    public class FixedSlotChangedMessage : GameMessage
    {
        public FixedSlotType Slot;       // 哪个槽
        public FixedItemType ItemType;   // 物品类型
        public string ItemID;            // 物品ID（空为null）
    }
}
