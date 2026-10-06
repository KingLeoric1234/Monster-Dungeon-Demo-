using UnityEngine;
using Game.Core;

namespace Game.Enemy
{
    /// <summary>
    /// 避障（执行层）：前方有障碍就尝试偏转，最终写 transform.position。
    /// 直行 → ±60° → ±91° → 90°/120°/180° 逐级试，每次 OverlapCircle 探测 ObstacleLayer。
    /// 这是将来 2D→3D 唯一要换的执行段。
    /// </summary>
    public class EnemyAvoidance
    {
        /// <summary>从台帐读 currentDirection + currentSpeed，执行带避障的位移。</summary>
        public void Execute(Transform self, EnemyMovementData data)
        {
            MoveWithAvoidance(self, data, data.currentDirection, data.currentSpeed);
        }

        /// <summary>带避障的移动：直行 → ±60° → ±91° → 90°/120°/180° 逐级试</summary>
        private void MoveWithAvoidance(Transform self, EnemyMovementData data, Vector2 direction, float speed)
        {
            float checkRadius = 0.5f;
            float moveDist = speed * Time.deltaTime;

            // ① 先试正前方
            Vector2 newPos = (Vector2)self.position + direction * moveDist;
            if (Physics2D.OverlapCircle(newPos, checkRadius, CombatConfig.ObstacleLayer) == null)
            {
                self.position = newPos;
                return;
            }

            // ② 前方有障碍，试左转60度
            Vector2 leftDir = Quaternion.Euler(0, 0, 60) * direction;
            Vector2 leftPos = (Vector2)self.position + leftDir * moveDist;
            if (Physics2D.OverlapCircle(leftPos, checkRadius, CombatConfig.ObstacleLayer) == null)
            {
                self.position = leftPos;
                data.currentDirection = leftDir;
                return;
            }

            // ③ 试右转60度
            Vector2 rightDir = Quaternion.Euler(0, 0, -60) * direction;
            Vector2 rightPos = (Vector2)self.position + rightDir * moveDist;
            if (Physics2D.OverlapCircle(rightPos, checkRadius, CombatConfig.ObstacleLayer) == null)
            {
                self.position = rightPos;
                data.currentDirection = rightDir;
                return;
            }

            // ④ 试更大偏转（91度）
            Vector2 farLeftDir = Quaternion.Euler(0, 0, 91) * direction;
            Vector2 farLeftPos = (Vector2)self.position + farLeftDir * moveDist;
            if (Physics2D.OverlapCircle(farLeftPos, checkRadius, CombatConfig.ObstacleLayer) == null)
            {
                self.position = farLeftPos;
                data.currentDirection = farLeftDir;
                return;
            }

            Vector2 farRightDir = Quaternion.Euler(0, 0, -91) * direction;
            Vector2 farRightPos = (Vector2)self.position + farRightDir * moveDist;
            if (Physics2D.OverlapCircle(farRightPos, checkRadius, CombatConfig.ObstacleLayer) == null)
            {
                self.position = farRightPos;
                data.currentDirection = farRightDir;
                return;
            }

            // ⑤ 实在走不了，尝试大角度偏转（90°、120°、180°）
            float[] bigAngles = { 90f, -90f, 120f, -120f, 180f };
            foreach (float angle in bigAngles)
            {
                Vector2 bigDir = Quaternion.Euler(0, 0, angle) * direction;
                Vector2 bigPos = (Vector2)self.position + bigDir * moveDist;
                if (Physics2D.OverlapCircle(bigPos, checkRadius, CombatConfig.ObstacleLayer) == null)
                {
                    self.position = bigPos;
                    data.currentDirection = bigDir;
                    return;
                }
            }
        }
    }
}
