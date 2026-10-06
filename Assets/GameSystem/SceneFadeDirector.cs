using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Core
{
    /// <summary>全局转场：跨场景存活，所有场景共用一个FadeImage</summary>
    public class SceneFadeDirector : MonoBehaviour
    {
        public static SceneFadeDirector Instance { get; private set; }

        private Image fadeImage;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 自动找fadeImage
            fadeImage = GetComponentInChildren<Image>();
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = 0;
                fadeImage.color = c;
            }
        }

        /// <summary>渐暗/渐白后切换场景</summary>
        public void FadeToScene(string sceneName, Color fadeColor, float fadeOutDuration, float fadeInDuration)
        {
            StartCoroutine(FadeRoutine(sceneName, fadeColor, fadeOutDuration, fadeInDuration));
        }

        private System.Collections.IEnumerator FadeRoutine(string sceneName, Color fadeColor, float fadeOutDur, float fadeInDur)
        {
            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
                float t = 0;
                while (t < fadeOutDur)
                {
                    t += Time.deltaTime;
                    fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, t / fadeOutDur);
                    yield return null;
                }
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
            }

            SceneManager.LoadScene(sceneName);

            yield return null; // 等新场景加载完

            if (fadeImage != null)
            {
                float t = 0;
                while (t < fadeInDur)
                {
                    t += Time.deltaTime;
                    fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1 - t / fadeInDur);
                    yield return null;
                }
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
            }
        }
    }
}

