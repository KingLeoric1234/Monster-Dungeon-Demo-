using Game.Items;

namespace Game.Core
{
    /// <summary>金币变化消息</summary>
    public class GoldChangedMessage : GameMessage
    {
        public int NewGold;
    }

    /// <summary>装备购买成功消息</summary>
    public class EquipmentPurchasedMessage : GameMessage
    {
        public string EquipmentID;
    }

    /// <summary>装备穿戴/卸下消息</summary>
    public class EquipmentEquippedMessage : GameMessage
    {
        public string EquipmentID;   // null表示卸下
        public EquipmentSlotType Slot;
    }

    /// <summary>打开商店消息</summary>
    public class OpenShopMessage : GameMessage
    {
        public string ShopNPCID;
    }

    /// <summary>关闭商店消息</summary>
    public class CloseShopMessage : GameMessage
    {
        public string ShopNPCID;  // 要关闭的商店ID（null或空表示关闭所有）
    }
}
