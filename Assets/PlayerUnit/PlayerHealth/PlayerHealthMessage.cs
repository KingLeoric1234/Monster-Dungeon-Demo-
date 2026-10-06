namespace Player
{
    /// <summary>
    /// 玩家血量变化消息（"电话实物"）：PlayerHealth 发布、血条等 UI 订阅刷新。
    /// 收进 Player 域，走 PlayerMessageBus，不再惊动全局总线。
    /// 为HealthBar_Plug留下接口，为血量UI插件发送信息。
    /// </summary>
    public class PlayerHealthChangedMessage : PlayerMessage
    {
        public int CurrentHealth;
        public int MaxHealth;
    }
}
