// 怪物攻击：碰到玩家就发伤害消息，不直接处理扣血和击退
using UnityEngine;
using Game.Core;

namespace Game.Enemy
{
    /// <summary>
    /// 怪物攻击组件：碰到玩家就发EnemyAttackMessage，CombatDirector订阅后处理扣血和击退。
    /// 挂在怪物Prefab上。
    /// </summary>
    public class EnemyAttack : MonoBehaviour
    {
        private MonsterTemplate template;           // 怪物数据（伤害、冷却等）
        private float attackCooldownTimer;           // 攻击冷却计时器
        private KnockBackHandler knockBack;          // 击退组件（被击退时不能攻击）

        private void Awake()
        {
            knockBack = GetComponent<KnockBackHandler>();
        }

        /// <summary>初始化怪物数据（生成时调用）</summary>
        public void Initialize(MonsterTemplate monsterTemplate)
        {
            template = monsterTemplate;
        }

        private void Update()
        {
            if (attackCooldownTimer > 0)
                attackCooldownTimer -= Time.deltaTime;
        }

        /// <summary>碰到任何Collider2D时自动触发。检查是不是玩家，是就发伤害消息。</summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            //Debug.Log("[EnemyAttack] OnTriggerEnter2D: " + other.gameObject.name);
            // 冷却中不攻击
            if (attackCooldownTimer > 0) return;
            // 被击退中不攻击
            if (knockBack.IsKnockedBack) return;

            //Debug.Log("[EnemyAttack] 碰到: " + other.gameObject.name + " 有PlayerHealthAction=" + (other.GetComponent<Player.PlayerHealthAction>() != null));

            // 碰到的物体有PlayerHealthAction组件，说明是玩家
            if (other.GetComponent<Player.PlayerHealthAction>() != null)
            {
                // 击退方向：从怪物指向玩家
                Vector2 dir = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
                int dmg = template != null ? template.attackPower : -1;

                // 播放受击音效
                MessageBus.Publish(new PlaySoundMessage { Type = SoundType.PlayerHurt });

                // 发布伤害消息，CombatDirector订阅后处理扣血和击退
                MessageBus.Publish(new EnemyAttackMessage
                {
                    Target = other.gameObject,
                    Direction = dir,
                    Damage = dmg
                });

                // 进入攻击冷却
                attackCooldownTimer = template.attackCooldown;
            }
        }

    }
}
