using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// 玩家最终属性变化消息：装备/武器/药水变化时发布。
    /// 所有需要玩家属性的脚本订阅这个消息，不要自己算。
    /// </summary>
    public class PlayerStatsChangedMessage : GameMessage
    {
        public int Attack;           // 最终攻击（武器伤害）
        public int Defense;          // 最终防御（基础+装备护甲）
        public float MoveSpeed;      // 最终移速（基础×(1-移速惩罚)）
        public float RunSpeed;       // 最终奔跑速度
        public int MaxHealth;        // 最终血量上限
        public float MaxStamina;     // 最终体力上限
        public float AttackSpeed;    // 最终攻速（武器攻速×冷却效果）
        public float AttackRange;    // 最终攻击范围
        public float CritChance;     // 最终暴击率
        public int Armor;            // 装备总护甲（跟Defense一样，方便理解）
        public float SpeedPenalty;   // 总移速惩罚
    }

    /// <summary>
    /// 药水使用消息：玩家右键使用药水时发布。
    /// PotionBuffDirector订阅，处理药水效果和持续时间。
    /// </summary>
    public class PotionUsedMessage : GameMessage
    {
        public string PotionID;  // 药水ID
    }

    /// <summary>
    /// Buff变化消息：药水Buff增加/消失时发布。
    /// BuffDisplay订阅，更新右上角显示。
    /// </summary>
    public class BuffChangedMessage : GameMessage
    {
        public string BuffName;     // Buff名称（简短，如Acceleration）
        public float Duration;      // 剩余时间（0表示消失）
        public Sprite Icon;         // Buff图标
        public bool IsAdded;        // true=增加，false=消失
        public string Description;  // Buff描述（悬停时显示）
    }
}
