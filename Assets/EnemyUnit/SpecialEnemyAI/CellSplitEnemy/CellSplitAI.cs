using UnityEngine;
using Game.Core;

namespace Game.Enemy
{
    /// <summary>细胞怪分裂：死亡时分裂成两个小怪</summary>
    public class CellSplit : MonoBehaviour
    {
        private int remainingSplits;
        private MonsterTemplate template;

        public void Initialize(MonsterTemplate t)
        {
            template = t;
            remainingSplits = t.maxSplits;
        }

        /// <summary>由EnemyHealth在死亡时调用。返回true表示已分裂（不需要真死）</summary>
        public bool OnDeathSplit()
        {
            if (remainingSplits <= 0) return false;

            remainingSplits--;
            SpawnChildren();
            return true;
        }

        private void SpawnChildren()
        {
            EnemyDirector director = FindObjectOfType<EnemyDirector>();
            if (director == null) return;

            // 两个小怪往左右偏移一点
            Vector2 pos = transform.position;
            SpawnChild(director, pos + new Vector2(-0.5f, 0));
            SpawnChild(director, pos + new Vector2(0.5f, 0));
        }

        private void SpawnChild(EnemyDirector director, Vector2 pos)
        {
            MonsterTemplate child = new MonsterTemplate
            {
                monsterType = template.monsterType,
                difficulty = template.difficulty,
                monsterName = template.monsterName + " (Mini)",
                maxHealth = Mathf.Max(5, template.maxHealth / 4),
                moveSpeed = template.moveSpeed * 1.5f,
                attackPower = Mathf.Max(3, template.attackPower / 2),
                attackCooldown = template.attackCooldown,
                size = template.size * 0.4f,
                knockBackResistance = 0f,
                knockBackForce = template.knockBackForce,
                aggroRange = template.aggroRange,
                patrolMinTime = template.patrolMinTime,
                patrolMaxTime = template.patrolMaxTime,
                minOffsetInterval = template.minOffsetInterval,
                maxOffsetInterval = template.maxOffsetInterval,
                maxOffset = template.maxOffset,
                maxSplits = remainingSplits
            };

            director.SpawnCellChild(child, pos);
        }
    }
}
