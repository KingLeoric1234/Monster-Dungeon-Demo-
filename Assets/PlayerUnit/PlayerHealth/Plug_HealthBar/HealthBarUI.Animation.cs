using UnityEngine;
using System.Collections;

namespace HealthBar
{
    /// <summary>
    /// 玩家血条 UI 芯片——动画件：血条自己的淡入淡出/滑动。
    /// 焊死在血条芯片内部自管，不做全局共用（每个 UI 芯片带自己的动画，避免收容所）。
    /// </summary>
    public partial class HealthBarUI : MonoBehaviour
    {
        [Header("Animation")]
        private Coroutine animCoroutine;

        private void ShowBarsWithAnimation()
        {
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(ShowBarsCoroutine());
        }

        private void HideBarsWithAnimation()
        {
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(HideBarsCoroutine());
        }

        private IEnumerator ShowBarsCoroutine()
        {
            if (hpGroup != null) hpGroup.SetActive(true);

            // 淡入 + 滑动（easeOutExpo）
            float t = 0f;
            while (t < HealthBarConfig.FadeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / HealthBarConfig.FadeDuration);
                float eased = 1f - Mathf.Pow(2f, -HealthBarConfig.EaseOutExpoK * p);

                SetGroupAlpha(hpGroup, eased);
                SetGroupSlide(hpGroup, HealthBarConfig.SlideDistance * (1f - eased));

                yield return null;
            }

            SetGroupAlpha(hpGroup, 1f);
            SetGroupSlide(hpGroup, 0f);
        }

        private IEnumerator HideBarsCoroutine()
        {
            // 慢半拍再隐藏
            yield return new WaitForSeconds(HealthBarConfig.HideDelay);

            float t = 0f;
            while (t < HealthBarConfig.FadeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / HealthBarConfig.FadeDuration);
                float eased = 1f - Mathf.Pow(2f, -HealthBarConfig.EaseOutExpoK * p);

                SetGroupAlpha(hpGroup, 1f - eased);
                SetGroupSlide(hpGroup, HealthBarConfig.SlideDistance * eased);

                yield return null;
            }

            // 主世界：面板关了就隐藏；地牢：血条常驻保留
            if (hpGroup != null && !isInDungeon) hpGroup.SetActive(false);
            if (hpGroup != null && isInDungeon)
            {
                hpGroup.SetActive(true);
                SetGroupAlpha(hpGroup, 1f);
            }

            SetGroupAlpha(hpGroup, 1f);
            SetGroupSlide(hpGroup, 0f);
        }

        private void SetGroupAlpha(GameObject group, float alpha)
        {
            if (group == null) return;
            CanvasGroup cg = group.GetComponent<CanvasGroup>();
            if (cg == null) cg = group.AddComponent<CanvasGroup>();
            cg.alpha = alpha;
        }

        private void SetGroupSlide(GameObject group, float offset)
        {
            if (group == null) return;
            RectTransform rt = group.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 pos = rt.anchoredPosition;
                pos.x = hpGroupBasePos.x - offset; // 以自举时的基准位置滑动，不丢初始边距
                rt.anchoredPosition = pos;
            }
        }
    }
}
