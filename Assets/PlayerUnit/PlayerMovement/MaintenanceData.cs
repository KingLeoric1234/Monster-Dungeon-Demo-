// Assets/PlayerUnit/PlayerMovement/MaintenanceData.cs
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// 玩家移动状态（核心中的核心，由台账维护）。
    /// 原定义于 PlayerMovement.cs，随其删除迁入台账（与 EnemyMovementData 持有 EnemyState 同构）。
    /// </summary>
    public enum PlayerMoveState
    {
        Walk,   // 走路
        Run     // 奔跑（Shift加速）
    }

    /// <summary>
    /// 玩家移动核心数据维护器。
    ///
    /// 职责：只负责"维护"玩家移动的两个核心参数——
    ///   • PlayerDirection —— 玩家移动方向（单位向量，静止时为零向量）
    ///   • PlayerSpeed     —— 玩家移动速度（标量，单位/秒）
    ///
    /// 数据流（与 PlayerMovement 配合）：
    ///   Step 1  PlayerMovement 每帧"初始化数据"（读取输入 → 计算方向与速度）；
    ///   Step 2  初始化结果通过 Initialize() 发送到本文件维护，
    ///           此后移动执行与外部读取统一走本类的 PlayerDirection / PlayerSpeed。
    ///   Step 3/4  PlayerDirectionCalculation / PlayerSpeedCalculation 分别通过
    ///           SetDirection() / SetSpeed() 写入各自的核心参数（各自唯一写入口）。
    ///
    /// 本类不自行计算，只做存储与维护；也不依赖任何 MonoBehaviour/场景对象，可单独测试。
    /// </summary>
    public class MaintenanceData
    {
        /// <summary>玩家移动方向（单位向量；无输入/完全静止时为零向量）</summary>
        public Vector2 PlayerDirection { get; private set; } = Vector2.zero;

        /// <summary>玩家移动速度（标量，单位/秒）</summary>
        public float PlayerSpeed { get; private set; } = 0f;

        /// <summary>核心数据是否已被初始化（PlayerMovement 在 Awake 写入默认值后为 true）</summary>
        public bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// 初始化 / 更新核心数据（Step 2 的入口：PlayerMovement 把初始化结果发送到这里维护）。
        /// </summary>
        /// <param name="direction">移动方向向量；传入原始速度向量会自动归一化，零向量表示无移动</param>
        /// <param name="speed">移动速度（单位/秒），负值会被钳制为 0</param>
        public void Initialize(Vector2 direction, float speed)
        {
            PlayerDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.zero;
            PlayerSpeed = Mathf.Max(0f, speed);
            IsInitialized = true;
        }

        /// <summary>
        /// 写入方向（PlayerDirectionCalculation 的唯一写入口）。
        /// 传入实际速度向量会自动归一化，零向量表示无移动。
        /// </summary>
        public void SetDirection(Vector2 direction)
        {
            PlayerDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.zero;
        }

        /// <summary>
        /// 写入速度（PlayerSpeedCalculation 的唯一写入口）。负值会被钳制为 0。
        /// </summary>
        public void SetSpeed(float speed)
        {
            PlayerSpeed = Mathf.Max(0f, speed);
        }

        /// <summary>
        /// 重置核心数据为默认值（场景重置/玩家复活时使用）。
        /// </summary>
        /// <param name="defaultSpeed">重置后的默认速度，不传则归零</param>
        public void Reset(float defaultSpeed = 0f)
        {
            PlayerDirection = Vector2.zero;
            PlayerSpeed = Mathf.Max(0f, defaultSpeed);
            IsInitialized = true;
        }
    }
}
