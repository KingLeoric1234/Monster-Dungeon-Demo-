using UnityEngine;
using Game.Player;
using Game.Enemy;

namespace Game.Core
{
    /// <summary>
    /// 战斗秘书：统一处理所有伤害和击退。
    /// 待重写：之后加入装备护甲减伤、装备增益等。
    /// </summary>
    public class CombatDirector : MonoBehaviour
    {
        private void OnEnable()
        {
            MessageBus.Subscribe<PlayerAttackMessage>(OnPlayerAttack);
            MessageBus.Subscribe<EnemyAttackMessage>(OnEnemyAttack);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<PlayerAttackMessage>(OnPlayerAttack);
            MessageBus.Unsubscribe<EnemyAttackMessage>(OnEnemyAttack);
        }

        /// <summary>玩家攻击：对怪物造成伤害+击退</summary>
        private void OnPlayerAttack(PlayerAttackMessage msg)
        {
            int finalDamage = msg.Damage;

            // 对怪物造成伤害
            EnemyHealthCalculator enemyHealth = msg.Target.GetComponent<EnemyHealthCalculator>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(finalDamage, msg.IsCrit);
            }

            // 对怪物应用击退（抗性从怪物模板读）
            KnockBackHandler knockBack = msg.Target.GetComponent<KnockBackHandler>();
            if (knockBack != null && enemyHealth != null && enemyHealth.Template != null)
            {
                knockBack.ApplyKnockBack(msg.Direction, enemyHealth.Template.knockBackResistance);
            }
        }

        /// <summary>怪物攻击：对玩家造成伤害+击退（之后加入护甲减伤）</summary>
        private void OnEnemyAttack(EnemyAttackMessage msg)
        {
            int finalDamage = msg.Damage;
            finalDamage = Mathf.Max(1, finalDamage);

            // 扣血
            PlayerHealthAction playerHealth = msg.Target.GetComponent<PlayerHealthAction>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(finalDamage);
            }

            // 对玩家应用击退（抗性从玩家配置读）
            KnockBackHandler knockBack = msg.Target.GetComponent<KnockBackHandler>();
            if (knockBack != null)
            {
                knockBack.ApplyKnockBack(msg.Direction, PlayerConfig.KnockBackResistance);
            }
        }
    }
}
