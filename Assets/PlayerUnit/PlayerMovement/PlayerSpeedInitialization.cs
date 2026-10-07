// Assets/PlayerUnit/PlayerMovement/PlayerSpeedInitialization.cs
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// 速度初始化模块：只负责"开局撒种子"，不算任何每帧逻辑。
    ///
    /// 职责（Step 1 - Initialize the Data）：
    ///   1. 决定初始速度的默认值（走路速度），方向静止；
    ///   2. 把初始化结果送入 MaintenanceData 维护（Step 2 - Send the Initialization）；
    ///   3. 同时作为场景重置（复活/读档/ResetPosition）的入口。
    ///
    /// 由 PlayerMovement 在 Awake / ResetPosition 时调用。
    /// 不依赖 MonoBehaviour/场景对象，可单独测试。
    /// </summary>
    public class PlayerSpeedInitialization
    {
        /// <summary>
        /// 首次初始化（Awake 调用）：方向静止、速度为默认走路速度。
        /// </summary>
        /// <param name="data">核心数据维护器（MaintenanceData）</param>
        /// <param name="defaultSpeed">初始速度（通常为走路速度）</param>
        public void Initialize(MaintenanceData data, float defaultSpeed)
        {
            data.Initialize(Vector2.zero, defaultSpeed);
        }

        /// <summary>
        /// 场景重置（ResetPosition 调用）：核心数据回到默认值。
        /// </summary>
        /// <param name="data">核心数据维护器（MaintenanceData）</param>
        /// <param name="defaultSpeed">重置后的默认速度（通常为走路速度）</param>
        public void Reset(MaintenanceData data, float defaultSpeed)
        {
            data.Reset(defaultSpeed);
        }
    }
}
