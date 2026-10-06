using UnityEngine;

namespace Player
{
    /// <summary>
    /// 玩家请求攻击（PlayerAttackAction 发布 → WeaponDirector 订阅）。
    /// Player 域单向消息：Player 侧只负责发（"我朝某个方向打了一下"），Weapon 侧自己决定怎么打。
    /// 走 PlayerMessageBus 状态通道，不惊动全局总线。
    /// </summary>
    public class PlayerAttackRequestMessage : PlayerMessage
    {
        public Vector2 Direction;      // 攻击方向（玩家位置朝向鼠标，归一化）
        public Vector2 PlayerPosition; // 玩家位置
        public Vector2 MousePosition;  // 鼠标世界坐标（远程武器从发射点重新算方向用）
    }
}
