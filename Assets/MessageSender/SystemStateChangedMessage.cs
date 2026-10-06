namespace Game.Core
{
    /// <summary>系统状态切换播报：刻度变了才发</summary>
    public class SystemStateChangedMessage : GameMessage
    {
        public SystemState OldState;
        public SystemState NewState;
        public int Layer;      // 地牢模式时：第几层
    }
}
