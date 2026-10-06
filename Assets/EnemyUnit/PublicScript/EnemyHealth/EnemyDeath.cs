using UnityEngine;
using Game.Core;

namespace Game.Enemy
{
    /// <summary>死亡流程：订阅账本 OnDied → 分裂检查 → 死声 → 死亡粒子 → 通知导演 → 销毁</summary>
    public class EnemyDeath : MonoBehaviour
    {
        private EnemyHealthData data;
        private EnemyDeathParticles deathParticles;   // 死亡粒子由这显式触发（保证先播粒子再销毁）

        private void Awake()
        {
            data = GetComponent<EnemyHealthData>();
            deathParticles = GetComponent<EnemyDeathParticles>();
        }

        private void OnEnable()  { if (data != null) data.OnDied += HandleDied; }
        private void OnDisable() { if (data != null) data.OnDied -= HandleDied; }

        private void HandleDied()
        {
            // 细胞怪分裂检查：能分裂就分裂，不真死（CellSplit 逻辑保持原样）
            CellSplit cellSplit = GetComponent<CellSplit>();
            if (cellSplit != null && cellSplit.OnDeathSplit())
            {
                FindObjectOfType<EnemyDirector>()?.OnEnemyDied(gameObject);
                Destroy(gameObject);
                return;
            }

            // 死亡音效
            if (data != null && data.Template != null)
            {
                if (data.Template.difficulty == "Elite")
                    MessageBus.Publish(new PlaySoundMessage { Type = SoundType.EliteDie });
                else
                    MessageBus.Publish(new PlaySoundMessage { Type = SoundType.NormalDie });
            }

            // 死亡溶解粒子（独立物体，不随本怪销毁）
            if (deathParticles != null) deathParticles.SpawnDeathParticles();

            // 通知导演移除自己
            FindObjectOfType<EnemyDirector>()?.OnEnemyDied(gameObject);
            Destroy(gameObject);
        }
    }
}
