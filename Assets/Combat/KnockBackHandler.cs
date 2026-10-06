using UnityEngine;
using Game.Core;
using Game.Enemy;

namespace Game.Core
{
    public class KnockBackHandler : MonoBehaviour
    {
        private bool isKnockedBack;
        private float knockBackTimer;
        private Vector2 knockBackDirection;
        private float currentSpeed;
        private float checkRadius;

        public bool IsKnockedBack => isKnockedBack;

        /// <summary>由 Director 调用，设置击退检测半径</summary>
        public void SetCheckRadius(float radius)
        {
            checkRadius = radius;
        }

        private void Update()
        {
            if (!isKnockedBack) return;

            knockBackTimer -= Time.deltaTime;
            float elapsed = CombatConfig.KnockBackDuration - knockBackTimer;
            currentSpeed = CombatConfig.KnockBackBaseSpeed * Mathf.Exp(-CombatConfig.KnockBackDecayRate * elapsed);

            Vector2 newPos = (Vector2)transform.position + knockBackDirection * currentSpeed * Time.deltaTime;

            Collider2D hit = Physics2D.OverlapCircle(newPos, checkRadius, CombatConfig.ObstacleLayer);
            if (hit == null)
            {
                transform.position = newPos;
            }
            else
            {
                // 碰到墙，往击退反方向弹0.5f，确保不贴墙
                transform.position -= (Vector3)(knockBackDirection * 0.5f);
                isKnockedBack = false;
                currentSpeed = 0;
            }

            if (knockBackTimer <= 0)
            {
                isKnockedBack = false;
                currentSpeed = 0;
            }

            // 击退结束后，检测是不是卡在墙里，如果是就微调出来
            if (!isKnockedBack)
            {
                ResolveIfStuck();
            }
        }

        /// <summary>防卡死：如果当前位置在障碍里，往8个方向小幅度微调，找到安全位置</summary>
        private void ResolveIfStuck()
        {
            // 先检测当前位置是不是在障碍里
            Collider2D hit = Physics2D.OverlapCircle(
                transform.position, checkRadius, CombatConfig.ObstacleLayer);
            if (hit == null) return;  // 没卡住，正常

            // 卡住了，尝试往8个方向微调
            Vector2[] directions = {
                Vector2.up, Vector2.down, Vector2.left, Vector2.right,
                new Vector2(1, 1).normalized, new Vector2(1, -1).normalized,
                new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
            };

            float step = 0.1f;   // 每次微调0.08单位，幅度小，观感好
            int maxSteps = 8;     // 每个方向最多试8次（总共0.64单位，足够离开墙）

            foreach (Vector2 dir in directions)
            {
                for (int i = 1; i <= maxSteps; i++)
                {
                    Vector2 testPos = (Vector2)transform.position + dir * step * i;
                    Collider2D testHit = Physics2D.OverlapCircle(
                        testPos, checkRadius, CombatConfig.ObstacleLayer);
                    if (testHit == null)
                    {
                        transform.position = testPos;  // 找到安全位置，移过去
                        return;
                    }
                }
            }

            // 8个方向都试了还是不行，兜底往上挪一点
            transform.position += Vector3.up * step * maxSteps;
        }

        public void ApplyKnockBack(Vector2 direction, float resistance)
        {
            knockBackDirection = direction.normalized;
            currentSpeed = CombatConfig.KnockBackBaseSpeed * (1f - resistance);
            knockBackTimer = CombatConfig.KnockBackDuration;
            isKnockedBack = true;
        }

    }
}


