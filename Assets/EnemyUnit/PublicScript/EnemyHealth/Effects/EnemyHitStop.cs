using System.Collections;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>顿帧：打中瞬间全屏慢动作（timeScale 压低），由 TimeScaleGuard 兜底恢复</summary>
    public class EnemyHitStop : MonoBehaviour
    {
        private EnemyHealthData data;
        private bool isHitStopping;

        private void Awake()
        {
            data = GetComponent<EnemyHealthData>();
        }

        private void OnEnable()  { if (data != null) data.OnDamaged += HandleDamaged; }
        private void OnDisable() { if (data != null) data.OnDamaged -= HandleDamaged; }

        private void HandleDamaged(int damage, int current, int max)
        {
            if (isHitStopping) return;   // 正在顿帧就不重复触发
            StartCoroutine(HitStopRoutine());
        }

        private IEnumerator HitStopRoutine()
        {
            isHitStopping = true;
            Time.timeScale = 0.03f;
            float duration = data != null && data.Template != null ? data.Template.hitStopDuration : 0.01f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
            isHitStopping = false;
        }
    }
}
