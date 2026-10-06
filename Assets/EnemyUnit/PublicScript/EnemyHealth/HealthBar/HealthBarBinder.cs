using UnityEngine;
using Enemy;

namespace Game.Enemy
{
    /// <summary>
    /// HealthBarBinder 插件：订阅 EnemyMessageBus 的 EnemyDamageAppliedMessage，刷新血条本体。
    /// 消息驱动，不直接接触账本事件——账本发消息，血条听消息。
    /// 多只怪时只响应 Source 是自己的消息，别人的血不刷到自己屏幕上。
    /// </summary>
    public class HealthBarBinder : MonoBehaviour
    {
        private EnemyHealthData data;   // 自己的账本（用于对比消息来源）
        private HealthBarBody healthBar;    // 血条本体组件在子物体上，GetComponentInChildren 直取
        private bool barInitialized;    // 首次收到消息时初始化上限，之后只刷新

        private void Awake()
        {
            data = GetComponent<EnemyHealthData>();
            healthBar = GetComponentInChildren<HealthBarBody>();
        }

        private void OnEnable()
        {
            EnemyMessageBus.Subscribe<EnemyDamageAppliedMessage>(OnEnemyDamageApplied);
        }

        private void OnDisable()
        {
            EnemyMessageBus.Unsubscribe<EnemyDamageAppliedMessage>(OnEnemyDamageApplied);
        }

        private void OnEnemyDamageApplied(EnemyDamageAppliedMessage msg)
        {
            // 来源过滤：只刷自己的血条（消息是全局广播，别的怪掉血别动我）
            if (data == null || msg.Source != data.gameObject) return;
            if (healthBar == null) return;

            if (!barInitialized)
            {
                healthBar.Initialize(msg.MaxHealth);
                barInitialized = true;
            }
            healthBar.UpdateHealth(msg.CurrentHealth);
        }
    }
}
