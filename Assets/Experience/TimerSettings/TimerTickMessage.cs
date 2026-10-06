// Assets/Scripts/Core/TimerTickMessage.cs
namespace Game.Core
{
    /// <summary>倒计时tick消息，UI订阅后更新倒计时显示</summary>
    public class TimerTickMessage : GameMessage
    {
        public float RemainingSeconds;
    }
}