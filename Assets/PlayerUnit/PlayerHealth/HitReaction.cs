using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// 受击演出（粒子特效）：被 PlayerHealthAction 触发，不碰血量数据。
    /// 挂在 Player 物体上。
    /// </summary>
    public class HitReaction : MonoBehaviour
    {
        // 代码驱动：不拖引用。Prefab 从 Resources 懒加载缓存；
        // 也可由外部注入（SetParticlePrefab）覆盖。
        private const string HitParticlePath = "HitParticlePrefab/HitParticlePrefab";
        private GameObject hitParticlePrefab;

        /// <summary>注入受击粒子Prefab（代码驱动，替代 Inspector 拖引用；不注入则走 Resources 加载）</summary>
        public void SetParticlePrefab(GameObject prefab) => hitParticlePrefab = prefab;

        /// <summary>播放受击粒子（数量随伤害正比例，上限100）</summary>
        public void Play(int actualDamage)
        {
            if (hitParticlePrefab == null)
                hitParticlePrefab = Resources.Load<GameObject>(HitParticlePath);

            if (hitParticlePrefab == null)
            {
                Debug.LogWarning($"[HitReaction] 受击粒子 Prefab 加载失败：Resources 下找不到 {HitParticlePath}");
                return;
            }

            GameObject particle = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            if (ps == null) return;

            int count = Mathf.Min(100, Mathf.RoundToInt(0.5f * actualDamage));
            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, count) });
        }
    }
}
