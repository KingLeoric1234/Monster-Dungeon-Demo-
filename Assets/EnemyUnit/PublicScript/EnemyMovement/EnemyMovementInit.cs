using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// 初始化模块：把外部输入落地并播种台帐。
    /// 只负责"开局撒种子"，不算任何每帧逻辑。
    /// </summary>
    public class EnemyMovementInit
    {
        /// <summary>
        /// 首次播种：巡逻方向 + 换向计时 + 偏移预判首启。
        /// 由调用方在拿到模板后调用一次（当前无管线入口，模块孤零零待命）。
        /// </summary>
        public void Seed(MonsterTemplate template, EnemyMovementData data, EnemyPathfindingApplier pathfinding)
        {
            // 初始巡逻方向和时间
            data.patrolDirection = Random.insideUnitCircle.normalized;
            data.patrolTimer = Random.Range(template.patrolMinTime, template.patrolMaxTime);

            // 初始化偏移预判
            pathfinding.RandomizeOffset(data, template);
        }
    }
}
