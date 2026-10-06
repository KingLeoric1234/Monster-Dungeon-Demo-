using System;
using UnityEngine;
using Game.Core;
using Player;

namespace Game.Weapons
{
    /// <summary>
    /// 武器基类（原接口，现为 MonoBehaviour）：挂在武器物体上，由具体武器脚本继承。
    /// 订阅玩家攻击请求（PlayerAttackRequestMessage）——收到请求后，只有当前手持武器（IWeapon.Current）响应并执行 Attack。
    /// </summary>
    public abstract class IWeapon : MonoBehaviour
    {
        /// <summary>当前手持武器（由 SwitchWeapon 维护：切换时设置，攻击请求只响应当前武器）</summary>
        public static IWeapon Current { get; set; }

        /// <summary>是否远程武器（决定攻击后是否退出攻击模式）</summary>
        public abstract bool IsRanged { get; }

        /// <summary>执行攻击（由具体武器实现）</summary>
        /// <param name="playerPosition">玩家位置</param>
        /// <param name="direction">攻击方向（从玩家位置朝向鼠标，近战用）</param>
        /// <param name="mousePosition">鼠标世界坐标（远程武器用这个从发射点重新算方向）</param>
        public abstract void Attack(Vector2 playerPosition, Vector2 direction, Vector2 mousePosition);

        private void OnEnable()
        {
            PlayerMessageBus.Subscribe<PlayerAttackRequestMessage>(OnAttackRequest);
        }

        private void OnDisable()
        {
            PlayerMessageBus.Unsubscribe<PlayerAttackRequestMessage>(OnAttackRequest);
        }

        /// <summary>收到玩家攻击请求：只响应当前手持武器（其余武器忽略）</summary>
        private void OnAttackRequest(PlayerAttackRequestMessage msg)
        {
            if (Current != this) return;
            Attack(msg.PlayerPosition, msg.Direction, msg.MousePosition);
        }
    }

    /// <summary>
    /// 武器侧对 PlayerAttackMessage（命中消息）的订阅入口，与 IWeapon 同文件，作为武器契约的一部分。
    /// 武器脚本在 OnEnable / OnDisable 中调用 Subscribe / Unsubscribe。
    /// 消息携带三个信息（Target / Direction / Damage）和一个状态（IsCrit）。
    /// </summary>
    public static class WeaponHitSubscription
    {
        /// <summary>订阅命中消息（OnEnable 中调用）</summary>
        public static void Subscribe(Action<PlayerAttackMessage> handler) =>
            MessageBus.Subscribe<PlayerAttackMessage>(handler);

        /// <summary>取消订阅（OnDisable 中调用）</summary>
        public static void Unsubscribe(Action<PlayerAttackMessage> handler) =>
            MessageBus.Unsubscribe<PlayerAttackMessage>(handler);
    }
}
