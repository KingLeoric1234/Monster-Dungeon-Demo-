using UnityEngine;
using Game.Items;

namespace Game.Core
{
    /// <summary>请求切换武器（PlayerAttack发布，WeaponDirector订阅）</summary>
    public class WeaponSwitchRequestMessage : GameMessage
    {
        public string WeaponID;  // 要切换到的武器ID
    }

    /// <summary>武器已切换通知（WeaponDirector发布，所有需要的脚本订阅）</summary>
    public class WeaponSwitchedMessage : GameMessage
    {
        public string WeaponID;       // 当前武器ID
        public bool IsRanged;         // 是否是远程武器
        public float AttackSpeed;     // 攻速（每秒攻击次数）
        public float Range;           // 攻击范围
        public float Damage;          // 基础伤害
        public float CritChance;      // 暴击率
        public float AttackAngle;     // 攻击扇形角度（度）
        public float BackAttackCooldownMultiplier;  // 背身攻击冷却倍率
        public float BackAttackAngleThreshold;      // 背身攻击角度阈值
        public float AttackStopDuration;            // 移动中攻击强制停下时间
    }

    /// <summary>攻击模式变化通知（PlayerAttack发布，AttackRangeIndicator/CustomCursor订阅）</summary>
    public class AttackModeChangedMessage : GameMessage
    {
        public bool IsAttackMode;  // 是否进入攻击模式
    }
}
