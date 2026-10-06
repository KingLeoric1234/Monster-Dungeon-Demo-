// Assets/Scripts/Core/EnemyAttackMessage.cs
using UnityEngine;

namespace Game.Core
{
    /// <summary>怪物攻击命中消息，CombatDirector 订阅后处理伤害和击退</summary>
    public class EnemyAttackMessage : GameMessage
    {
        public GameObject Target;    // 命中的玩家
        public Vector2 Direction;    // 攻击方向（怪物移动方向）
        public int Damage;           // 伤害值
    }
}
