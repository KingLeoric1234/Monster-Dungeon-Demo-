using UnityEngine;
using Game.Core; // MessageBus + PlayerHealRequestMessage + PlayerStatsChangedMessage

namespace Game.Player
{
    /// <summary>
    /// 血量加减（业务层·对外接口）：所有伤害/治疗都从这里进。
    /// 不直接碰 currentHealth——算清楚后调 PlayerHealthCache.ChangeHealth。
    /// 防御数据归这里维护（订阅属性变化），扣血时算减免；
    /// 无敌帧/粒子/死亡侦测是受击流程的收尾，单向调用 Invincibility / HitReaction。
    /// 挂载 Player 物体上。
    /// </summary>
    public class PlayerHealthAction : MonoBehaviour
    {
        // 引用（自动获取，不依赖手动拖拽）
        private PlayerHealthCache cache;
        private Invincibility invincibility;
        private HitReaction hitReaction;
        private PlayerDirector director;

        private int defense; // 最终防御（订阅属性变化维护，初始从配置读）

        private void Awake()
        {
            cache = GetComponent<PlayerHealthCache>();
            invincibility = GetComponent<Invincibility>();
            hitReaction = GetComponent<HitReaction>();
            director = GetComponent<PlayerDirector>();

            defense = PlayerConfig.Defense; // 初始防御从配置读，后续由属性系统覆盖
        }

        private void OnEnable()
        {
            MessageBus.Subscribe<PlayerHealRequestMessage>(OnHealRequest);
            MessageBus.Subscribe<PlayerStatsChangedMessage>(OnStatsChanged);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<PlayerHealRequestMessage>(OnHealRequest);
            MessageBus.Unsubscribe<PlayerStatsChangedMessage>(OnStatsChanged);
        }

        /// <summary>属性变化：这里只维护防御（血量上限归 MaxHealthManager 管）</summary>
        private void OnStatsChanged(PlayerStatsChangedMessage msg)
        {
            defense = msg.Defense;
        }

        /// <summary>受到伤害（实际伤害 = 伤害 - 0.5×防御，最少1点）</summary>
        public void TakeDamage(int damage)
        {
            // 无敌帧内免疫
            if (invincibility != null && !invincibility.CanTakeDamage()) return;

            // 防御减免
            int actualDamage = Mathf.Max(1, damage - Mathf.RoundToInt(defense * 0.5f));
            cache.ChangeHealth(-actualDamage);

            // 受击反应：粒子 + 启动无敌帧（单向调用，不反向依赖）
            if (hitReaction != null) hitReaction.Play(actualDamage);
            if (invincibility != null) invincibility.StartInvincibility();

            // 死亡侦测
            if (cache.CurrentHealth <= 0)
                director?.OnPlayerDied();
        }

        /// <summary>回血（钳制上限由 Cache 内部完成）</summary>
        public void Heal(int amount)
        {
            cache.ChangeHealth(amount);
        }

        /// <summary>收到回血请求消息（药水等外部模块发来）</summary>
        private void OnHealRequest(PlayerHealRequestMessage msg)
        {
            Heal(msg.Amount);
        }

        /// <summary>重置为满血 + 清除无敌帧（登录/复活/PlayerDirector 调用）</summary>
        public void ResetHealth()
        {
            if (invincibility != null) invincibility.Reset();
            cache.SetCurrentHealth(cache.MaxHealth);
        }
    }
}
