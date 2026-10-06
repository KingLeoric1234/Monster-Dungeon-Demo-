using UnityEngine;

namespace Game.Core
{
    /// <summary>玩家攻击命中消息，CombatDirector订阅后处理伤害和击退</summary>
    public class PlayerAttackMessage : GameMessage
    {
        public GameObject Target;    // 命中的敌人
        public Vector2 Direction;    // 攻击方向（鼠标指向）
        public int Damage;             // 伤害值（整数）
        public bool IsCrit;          // 是否暴击
    }
}
