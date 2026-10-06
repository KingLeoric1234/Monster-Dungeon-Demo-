using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// 状态判定（执行层）：判断应该是巡逻还是追击。
    /// 每帧按距离重判，只写 data.currentState，别的什么都不管。
    /// </summary>
    public class EnemyStateDecider
    {
        public void DecideState(Transform self, Transform player, MonsterTemplate template, EnemyMovementData data)
        {
            float distToPlayer = Vector2.Distance(self.position, player.position);
            data.currentState = (distToPlayer < template.aggroRange) ? EnemyState.Chase : EnemyState.Patrol;
        }
    }
}
