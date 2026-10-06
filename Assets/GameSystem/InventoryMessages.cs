using Game.Core;

namespace Game.Items
{
    public class InventorySlotChangedMessage : GameMessage
    {
        public BagSlotType Slot;
    }

    public class InventoryFullMessage : GameMessage
    {
        public string ItemID;
    }
}
