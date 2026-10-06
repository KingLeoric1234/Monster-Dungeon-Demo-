using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Game.Core;
using Game.Player;

namespace Game.World
{
    public class ExitPortal : MonoBehaviour
    {
        [Header("交互设置")]
        [SerializeField] private float interactRange = 1.5f;
        [SerializeField] private string nextSceneName = "LargeWorld";
        [SerializeField] private string hubSceneName = "World1";

        [Header("UI引用")]
        [SerializeField] private RectTransform pressFPrompt;
        [SerializeField] private Vector2 promptHiddenOffset = new Vector2(-300f, 0f);
        [SerializeField] private float promptSlideSpeed = 10f;
        [SerializeField] private CanvasGroup exitPanelGroup;

        [Header("浮动文字")]
        [SerializeField] private Transform floatingText;
        [SerializeField] private float textOffsetY = 1f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.15f;

        private Transform player;
        private bool isPanelOpen;
        private Vector3 textStartPos;
        private Vector2 promptTargetPos;
        private bool promptVisible;

        private void Start()
        {
            if (PlayerDirector.Instance != null)
                player = PlayerDirector.Instance.transform;

            if (pressFPrompt != null)
            {
                promptTargetPos = pressFPrompt.anchoredPosition;
                pressFPrompt.anchoredPosition = promptTargetPos + promptHiddenOffset;
                pressFPrompt.SetAsLastSibling();
            }

            if (exitPanelGroup != null)
            {
                exitPanelGroup.alpha = 0;
                exitPanelGroup.blocksRaycasts = false;
            }

            if (floatingText != null)
            {
                floatingText.SetParent(transform);
                textStartPos = new Vector3(0, textOffsetY, 0);
                floatingText.localPosition = textStartPos;
                MeshRenderer mr = floatingText.GetComponent<MeshRenderer>();
                if (mr != null) mr.sortingOrder = 4;
            }
        }

        private void Update()
        {
            if (floatingText != null)
            {
                float y = textStartPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                floatingText.localPosition = new Vector3(0, y, 0);
            }

            if (pressFPrompt != null)
            {
                Vector2 goal = promptVisible ? promptTargetPos : promptTargetPos + promptHiddenOffset;
                pressFPrompt.anchoredPosition = Vector2.Lerp(
                    pressFPrompt.anchoredPosition, goal, promptSlideSpeed * Time.deltaTime);
            }

            if (player == null) return;

            float dist = Vector2.Distance(transform.position, player.position);
            bool isNear = dist < interactRange;
            promptVisible = isNear && !isPanelOpen;

            if (isNear && !isPanelOpen && Input.GetKeyDown(KeyCode.F))
                OpenPanel();

            if (!isNear && isPanelOpen)
                ClosePanel();
        }

        private void OpenPanel()
        {
            isPanelOpen = true;
            if (exitPanelGroup != null)
                StartCoroutine(FadePanel(1, 0.15f));
        }

        public void OnClickContinue()
        {
            if (SceneFadeDirector.Instance != null)
                SceneFadeDirector.Instance.FadeToScene(nextSceneName, Color.black, 0.3f, 0.15f);
        }

        public void OnClickExtract()
        {
            Debug.Log("[ExitPortal] OnClickExtract! SceneFadeDirector=" + (SceneFadeDirector.Instance != null) + " hubScene=" + hubSceneName);
            if (SceneFadeDirector.Instance != null)
                SceneFadeDirector.Instance.FadeToScene(hubSceneName, Color.white, 0.3f, 0.2f);
        }

        public void OnClickCancel()
        {
            ClosePanel();
        }

        private void ClosePanel()
        {
            isPanelOpen = false;
            if (exitPanelGroup != null)
                StartCoroutine(FadePanel(0, 0.15f));
        }

        private IEnumerator FadePanel(float targetAlpha, float duration)
        {
            exitPanelGroup.blocksRaycasts = targetAlpha > 0.5f;
            float start = exitPanelGroup.alpha;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                exitPanelGroup.alpha = Mathf.Lerp(start, targetAlpha, t / duration);
                yield return null;
            }
            exitPanelGroup.alpha = targetAlpha;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}

