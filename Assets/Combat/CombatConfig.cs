using UnityEngine;
using Game.World;

// Assets/Scripts/Core/CombatConfig.cs
namespace Game.Core
{
    /// <summary>
    /// 战斗公共配置，击退等通用数值
    /// </summary>
    public static class CombatConfig
    {
        /// <summary>击退基础速度（抗性为0时的初速度）</summary>
        public static float KnockBackBaseSpeed = 3f;

        /// <summary>击退持续时间（秒）</summary>
        public static float KnockBackDuration = 0.05f;

        /// <summary>指数衰减率（越大衰减越快）</summary>
        public static float KnockBackDecayRate = 12f;

        /// <summary>障碍物所在的层（所有脚本从这里读，不在Inspector设）</summary>
        public static LayerMask ObstacleLayer => LayerMask.GetMask("Obstacle");

    }
}
