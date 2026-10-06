using UnityEngine;
using Game.Player;

namespace Game.Weapons
{
    /// <summary>
    /// 伤害计算器：统一处理伤害随机浮动、向上取整、暴击判定。
    /// 所有武器都用这个，避免重复代码。
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// 计算最终伤害。
        /// 公式：基础伤害 × 随机系数(0.7~1.3) → 向上取整 → 暴击×倍率
        /// </summary>
        /// <param name="baseDamage">基础伤害（枪伤害+子弹伤害，或武器伤害）</param>
        /// <param name="critChance">暴击率（0~1）</param>
        /// <param name="critMultiplier">暴击伤害倍率（默认1.5）</param>
        /// <returns>最终伤害，是否暴击</returns>
        public static (int damage, bool isCrit) Calculate(float baseDamage, float critChance, float critMultiplier = 1.5f)
        {
            // 随机浮动（区间从 PlayerConfig 读取，统一手感）
            float alpha = Random.Range(PlayerConfig.DamageMinMultiplier, PlayerConfig.DamageMaxMultiplier);
            float floatedDamage = baseDamage * alpha;

            // 向上取整
            int finalDamage = Mathf.CeilToInt(floatedDamage);

            // 暴击判定（在浮动基础上计算）
            bool isCrit = Random.value < critChance;
            if (isCrit)
            {
                finalDamage = Mathf.CeilToInt(finalDamage * critMultiplier);
            }

            return (finalDamage, isCrit);
        }

        /// <summary>
        /// 伤害输出接口（封死中）：计算好的伤害从这里传给下游（伤害应用系统）。
        /// ⚠ 尚未链接数据接口 —— 请在此处接入 PlayerAttackMessage 发布（CombatDirector 订阅后扣血）。
        /// </summary>
        /// <param name="target">被命中的目标（Enemy）</param>
        /// <param name="direction">攻击方向（击退/特效用）</param>
        /// <param name="damage">最终伤害（由 Calculate 得出）</param>
        /// <param name="isCrit">是否暴击（飘字/音效表现用）</param>
        public static void OutputDamage(GameObject target, Vector2 direction, int damage, bool isCrit)
        {
            Debug.LogWarning($"[DamageCalculator] 伤害输出接口未链接！damage={damage}, isCrit={isCrit}, target={target?.name}。请链接数据接口（如发布 PlayerAttackMessage 给 CombatDirector）。");
            // TODO: 链接数据接口 —— 在此处发布 PlayerAttackMessage
        }
    }
}
