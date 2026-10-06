using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace Game.UI
{
    /// <summary>
    /// Hover effect: background fades in/out when mouse enters/leaves.
    /// Attach to any UI element with a background Image. Reusable.
    /// Uses exponential easing (fast start, slow end).
    /// </summary>
    public class HoverHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Settings")]
        [SerializeField] private Image background;           // Background image (auto-finds if null)
        [SerializeField] private float hoverAlpha = 0.5f;    // Alpha when hovering (0-1)
        [SerializeField] private float fadeSpeed = 8f;       // Fade speed (higher = faster)

        private Coroutine fadeCoroutine;
        private float currentAlpha = 0f;

        private void Awake()
        {
            // Auto-find background if not set
            if (background == null)
            {
                background = GetComponent<Image>();
            }

            // Start with alpha 0
            if (background != null)
            {
                Color c = background.color;
                c.a = 0f;
                background.color = c;
                currentAlpha = 0f;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StartFade(hoverAlpha);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StartFade(0f);
        }

        /// <summary>Start fading to target alpha with exponential easing</summary>
        private void StartFade(float targetAlpha)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeTo(targetAlpha));
        }

        /// <summary>Fade coroutine with exponential easing (fast start, slow end)</summary>
        private IEnumerator FadeTo(float targetAlpha)
        {
            while (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
            {
                // Exponential interpolation: fast start, slow end
                currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

                if (background != null)
                {
                    Color c = background.color;
                    c.a = currentAlpha;
                    background.color = c;
                }

                yield return null;
            }

            currentAlpha = targetAlpha;
            if (background != null)
            {
                Color c = background.color;
                c.a = currentAlpha;
                background.color = c;
            }
        }
    }
}
