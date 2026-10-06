namespace Game.Core
{
    /// <summary>
    /// 回血请求消息：外部模块（如药水）请求给玩家回血，PlayerHealth订阅后执行。
    /// 外部不再直接摸 PlayerHealth，回血走消息请求。
    /// </summary>
    public class PlayerHealRequestMessage : GameMessage
    {
        public int Amount;  // 回血量
    }
}
