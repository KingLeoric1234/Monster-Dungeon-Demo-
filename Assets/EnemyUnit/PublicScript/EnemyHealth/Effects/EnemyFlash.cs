using System.Collections;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>闪红：受击瞬间变红再淡出。订阅账本 OnDamaged 自动触发</summary>
    public class EnemyFlash : MonoBehaviour
    {
        private EnemyHealthData data;
        private SpriteRenderer[] spriteRenderers;
        private Color[] originalColors;
        private Coroutine flashRoutine;

        private void Awake()
        {
            data = GetComponent<EnemyHealthData>();
            // 找所有SpriteRenderer（包括子物体），缓存原色供闪红还原
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            if (spriteRenderers.Length > 0)
            {
                originalColors = new Color[spriteRenderers.Length];
                for (int i = 0; i < spriteRenderers.Length; i++)
                    originalColors[i] = spriteRenderers[i].color;
            }
        }

        private void OnEnable()  { if (data != null) data.OnDamaged += HandleDamaged; }
        private void OnDisable() { if (data != null) data.OnDamaged -= HandleDamaged; }

        private void HandleDamaged(int damage, int current, int max) => Flash();

        private void Flash()
        {
            if (spriteRenderers == null || spriteRenderers.Length == 0) return;
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            float duration = data != null && data.Template != null ? data.Template.flashDuration : 0.1f;
            float inTime = duration * 0.2f;   // 快速变红
            float outTime = duration * 0.8f;  // 淡出恢复

            float t = 0;
            while (t < inTime)
            {
                t += Time.deltaTime;
                float lerp = t / inTime;
                for (int i = 0; i < spriteRenderers.Length; i++)
                    spriteRenderers[i].color = Color.Lerp(originalColors[i], Color.red, lerp);
                yield return null;
            }

            t = 0;
            while (t < outTime)
            {
                t += Time.deltaTime;
                float lerp = t / outTime;
                for (int i = 0; i < spriteRenderers.Length; i++)
                    spriteRenderers[i].color = Color.Lerp(Color.red, originalColors[i], lerp);
                yield return null;
            }

            for (int i = 0; i < spriteRenderers.Length; i++)
                spriteRenderers[i].color = originalColors[i];
        }
    }
}
