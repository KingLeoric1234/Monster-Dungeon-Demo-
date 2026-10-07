// Assets/PlayerUnit/PlayerMovement/PlayerDirectionCalculation.cs
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// 方向计算单元：控制 PlayerDirection 的唯一端口（only port to control playerDirection）。
    /// 只算数据写 MaintenanceData，不碰物理。
    ///
    /// 两步：
    ///   1. ComputeIntentDirection —— 由原始输入算出"意图方向"（归一化，无输入为零向量）；
    ///   2. WriteActualDirection —— 由速度单元产出的实际速度向量写 PlayerDirection，
    ///      保证滑行/转向时方向跟随实际运动（无输入、速度归零时写零向量）。
    /// </summary>
    public class PlayerDirectionCalculation
    {
        /// <summary>
        /// 计算意图方向：原始输入（Horizontal/Vertical）归一化，无输入为零向量。
        /// </summary>
        /// <param name="rawInput">原始输入向量（Input.GetAxisRaw 的 Horizontal/Vertical）</param>
        /// <returns>意图方向（单位向量或零向量）</returns>
        public Vector2 ComputeIntentDirection(Vector2 rawInput)
        {
            return rawInput.sqrMagnitude > 0.01f ? rawInput.normalized : Vector2.zero;
        }

        /// <summary>
        /// 写入实际方向（PlayerDirection 的唯一写入口）：
        /// 取速度单元产出的实际速度向量，归一化后写入；速度≈0 时写零向量。
        /// </summary>
        /// <param name="data">核心数据维护器（MaintenanceData）</param>
        /// <param name="actualVelocity">实际速度向量（PlayerSpeedCalculation 的产出）</param>
        public void WriteActualDirection(MaintenanceData data, Vector2 actualVelocity)
        {
            data.SetDirection(actualVelocity.sqrMagnitude > 0.0001f ? actualVelocity.normalized : Vector2.zero);
        }
    }
}
