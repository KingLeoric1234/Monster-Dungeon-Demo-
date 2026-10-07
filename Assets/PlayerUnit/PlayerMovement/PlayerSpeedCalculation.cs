// Assets/PlayerUnit/PlayerMovement/PlayerSpeedCalculation.cs
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// 速度计算单元：控制 PlayerSpeed 的唯一端口（only port to control playerSpeed）。
    /// 只算数据写 MaintenanceData，不碰物理。
    ///
    /// 职责（对应原 PlayerMovement.Update 中的速度部分，Step 4）：
    ///   1. 按状态选目标速度（奔跑/走路）；
    ///   2. 指数趋近平滑（内部保存当前速度向量 currentVelocity：起跑/转向快、停下滑行慢）；
    ///   3. 应用外部强制速度（攻击减速 forcedSpeed，>0 时覆盖平滑结果）；
    ///   4. 把实际速度大小写入 MaintenanceData.PlayerSpeed，并返回实际速度向量。
    /// </summary>
    public class PlayerSpeedCalculation
    {
        private Vector2 currentVelocity = Vector2.zero;   // 当前实际速度向量（滑行衰减状态）

        /// <summary>当前实际速度向量（供 PlayerMovement 读取：急刹判定 / IsMoving / IsRunning / 相机预判）</summary>
        public Vector2 CurrentVelocity => currentVelocity;

        /// <summary>
        /// 计算并写入速度（PlayerSpeed 的唯一写入口）。
        /// </summary>
        /// <param name="data">核心数据维护器（MaintenanceData）</param>
        /// <param name="isMoving">是否有移动输入</param>
        /// <param name="intentDirection">意图方向（PlayerDirectionCalculation 的产出）</param>
        /// <param name="state">当前移动状态（走路/奔跑）</param>
        /// <param name="moveSpeed">最终走路速度（PlayerMovement 传入，含属性加成）</param>
        /// <param name="runSpeed">最终奔跑速度（PlayerMovement 传入，含属性加成）</param>
        /// <param name="forcedSpeed">外部强制速度（攻击减速用，-1 表示不强制）</param>
        /// <returns>实际速度向量（用于写入方向与移动执行）</returns>
        public Vector2 ComputeSpeed(MaintenanceData data, bool isMoving, Vector2 intentDirection,
            PlayerMoveState state, float moveSpeed, float runSpeed, float forcedSpeed)
        {
            // 目标速度：奔跑且移动 = 奔跑速度，否则走路速度
            float targetSpeed = (state == PlayerMoveState.Run && isMoving) ? runSpeed : moveSpeed;
            Vector2 targetVelocity = isMoving ? intentDirection * targetSpeed : Vector2.zero;

            // 输入时用加速系数，没输入时用减速系数（滑行更远）
            float k = isMoving ? PlayerConfig.MoveAccelK : PlayerConfig.MoveDecelK;
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, Time.deltaTime * k);

            // 实际移动向量：有强制速度则覆盖，否则用平滑后的速度
            Vector2 moveVec = forcedSpeed > 0f ? intentDirection * forcedSpeed : currentVelocity;

            data.SetSpeed(moveVec.magnitude);
            return moveVec;
        }

        /// <summary>场景重置：清空平滑状态（速度向量归零）</summary>
        public void ResetState()
        {
            currentVelocity = Vector2.zero;
        }
    }
}
