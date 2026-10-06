using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// 怪物"伤害已结算"消息（Enemy 域状态通道，不惊动全局总线）。
    /// 发布方：EnemyHealthCalculator（结算后）；EnemyHealthData 建档时也发一条 Damage=0 的，用于血条初始化。
    /// 订阅方：DamagePopup（跳字）、HealthBarBinder（血条）。
    /// Source 是发布方物体——多只怪时订阅方只认自己的来源，别人的消息不动。
    /// Damage 是 Buff 链结算后的最终伤害（建档广播时为 0），IsCrit 用于暴击样式；
    /// CurrentHealth / MaxHealth 是结算后的血量，血条用。
    /// </summary>
    public class EnemyDamageAppliedMessage : EnemyMessage
    {
        public GameObject Source;       // 发布方（结算器/账本所在怪物物体）
        public int Damage;              // 结算后的最终伤害（建档广播为 0，仅刷血条）
        public bool IsCrit;             // 是否暴击
        public int CurrentHealth;       // 结算后的当前血量
        public int MaxHealth;           // 血量上限
    }
}
