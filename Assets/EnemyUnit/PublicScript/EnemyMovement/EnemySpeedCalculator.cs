using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// 速度计算单元：S2 思考节奏 + S6 速度目标 + S7 速度平滑。
    /// 只算数据写台帐，不碰物理。
    /// </summary>
    public class EnemySpeedCalculator
    {
        /// <summary>S2 思考节奏：追击时随机停下来"想事情"（产出 isThinking → 速度归零）</summary>
        public void UpdateThink(MonsterTemplate template, EnemyMovementData data)
        {
            if (data.currentState != EnemyState.Chase) return;

            if (data.isThinking)
            {
                data.thinkPauseLeft -= Time.deltaTime;
                if (data.thinkPauseLeft <= 0f) data.isThinking = false;
            }
            else
            {
                data.thinkTimer -= Time.deltaTime;
                if (data.thinkTimer <= 0f)
                {
                    data.isThinking = true;
                    data.thinkPauseLeft = Random.Range(MonsterTemplate.thinkPauseMin, MonsterTemplate.thinkPauseMax);
                    data.thinkTimer = Random.Range(MonsterTemplate.thinkIntervalMin, MonsterTemplate.thinkIntervalMax);
                }
            }
        }

        /// <summary>S6+S7：算目标速度（思考=0 / 追击=moveSpeed / 巡逻=×patrolSpeedRatio），Lerp 指数趋近写台帐</summary>
        public void ComputeSpeed(MonsterTemplate template, EnemyMovementData data)
        {
            float targetSpeed = 0f;
            if (!data.isThinking)
            {
                targetSpeed = (data.currentState == EnemyState.Chase)
                    ? template.moveSpeed
                    : template.moveSpeed * template.patrolSpeedRatio;
            }
            float k = (data.currentState == EnemyState.Chase) ? template.accelK : template.decelK;
            data.currentSpeed = Mathf.Lerp(data.currentSpeed, targetSpeed, Time.deltaTime * k);
        }
    }
}
