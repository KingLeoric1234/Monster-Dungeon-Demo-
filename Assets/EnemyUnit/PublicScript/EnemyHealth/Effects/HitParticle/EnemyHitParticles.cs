using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// 受击粒子：按伤害量喷粒子（数量=0.5×伤害，上限100）。订阅账本 OnDamaged 自动触发。
    /// 预制体不拖引用：运行时从 Resources\Enemy\Effects\HitParticlePrefab 抓取；
    /// 找不到就打 LogWarning，粒子不播（重构完成后请补建该预制体）。
    /// </summary>
    public class EnemyHitParticles : MonoBehaviour
    {
        private const string PrefabPath = "Enemy/Effects/HitParticlePrefab";

        private EnemyHealthData data;
        private GameObject hitParticlePrefab;   // 运行时抓取，不拖引用

        private void Awake()
        {
            data = GetComponent<EnemyHealthData>();

            hitParticlePrefab = Resources.Load<GameObject>(PrefabPath);
            if (hitParticlePrefab == null)
            {
                Debug.LogWarning("[EnemyHitParticles] 受击粒子预制体缺失: Resources/" + PrefabPath +
                                 "，受击粒子将不播放。重构完成后请补建该预制体。");
            }
        }

        private void OnEnable()  { if (data != null) data.OnDamaged += HandleDamaged; }
        private void OnDisable() { if (data != null) data.OnDamaged -= HandleDamaged; }

        private void HandleDamaged(int damage, int current, int max)
        {
            if (hitParticlePrefab == null) return;   // 预制体缺失：不播，已在 Awake 打过警告
            SpawnHitParticles(damage);
        }

        /// <summary>用预制体实例化粒子</summary>
        private void SpawnHitParticles(int damage)
        {
            GameObject particle = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            int count = Mathf.Min(100, Mathf.RoundToInt(0.5f * damage));
            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, count) });
        }
    }
}
