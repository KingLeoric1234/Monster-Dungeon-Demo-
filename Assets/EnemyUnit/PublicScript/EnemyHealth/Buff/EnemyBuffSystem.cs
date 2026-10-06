using System.Collections.Generic;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// Buff 插件：挂在 TakeDamage 结算链上，在动账本之前修改"进门伤害"。
    /// 增伤/减伤/无敌/吸血都实现 IEnemyBuff 后 AddBuff 进来即可。
    /// </summary>
    public class EnemyBuffSystem : MonoBehaviour
    {
        private readonly List<IEnemyBuff> buffs = new List<IEnemyBuff>();

        /// <summary>结算入口：EnemyHealthCalculator.TakeDamage 调它拿修正后的伤害</summary>
        public int ApplyIncomingDamage(int damage)
        {
            int result = damage;
            for (int i = 0; i < buffs.Count; i++)
                result = buffs[i].ModifyIncomingDamage(result);
            return Mathf.Max(0, result);
        }

        public void AddBuff(IEnemyBuff buff)
        {
            if (buff != null && !buffs.Contains(buff)) buffs.Add(buff);
        }

        public void RemoveBuff(IEnemyBuff buff)
        {
            if (buff != null) buffs.Remove(buff);
        }
    }

    /// <summary>Buff 接口：实现它就是一个可挂的 Buff</summary>
    public interface IEnemyBuff
    {
        /// <summary>修改即将造成的伤害，返回修正后的值</summary>
        int ModifyIncomingDamage(int damage);
    }

    /// <summary>示例：无敌 Buff（伤害归 0，连演出都不会触发）</summary>
    public class InvincibleBuff : IEnemyBuff
    {
        public int ModifyIncomingDamage(int damage) => 0;
    }
}
