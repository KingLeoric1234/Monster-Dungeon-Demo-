using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// 方向计算与输出（数据链接层）：
    /// 综合所有信息（状态 / 追击或巡逻方向 / 同伴排斥）→ 归一化 → 写 currentDirection。
    /// 不碰寻路、不碰排斥、不碰避障——方向源由寻路应用层提供（追击时）、排斥由排斥层提供，从参数进。
    /// 预计调用顺序（由未来入口驱动）：状态判定 → 寻路应用(追击时) → 排斥 → 本层计算 → 避障执行。
    /// </summary>
    public class EnemyDirectionCalculator
    {
        /// <summary>
        /// 综合所有信息，计算并输出 currentDirection。
        /// </summary>
        /// <param name="chaseDir">寻路应用层产出：寻路单位方向，或（直追+偏移）的原始和（巡逻时传 zero）</param>
        /// <param name="sepDir">排斥层产出：同伴推开方向（可能为 zero）</param>
        public void ComputeDirection(MonsterTemplate template, EnemyMovementData data,
            Vector2 chaseDir, Vector2 sepDir)
        {
            Vector2 targetDir;
            if (data.currentState == EnemyState.Chase)
            {
                targetDir = (chaseDir + sepDir * MonsterTemplate.separationForce).normalized;
            }
            else
            {
                // 巡逻：随机方向，定时更换
                data.patrolTimer -= Time.deltaTime;
                if (data.patrolTimer <= 0)
                {
                    data.patrolDirection = Random.insideUnitCircle.normalized;
                    data.patrolTimer = Random.Range(template.patrolMinTime, template.patrolMaxTime);
                }
                targetDir = (data.patrolDirection + sepDir * MonsterTemplate.separationForce).normalized;
            }

            data.currentDirection = targetDir;
        }
    }
}
