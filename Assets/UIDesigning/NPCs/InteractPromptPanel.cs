using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Game.Core;
using Game.World;

namespace Game.UI
{
    /// <summary>
    /// NPC头顶交互面板：显示提示、确认/取消（每个NPC一份，可复用）。
    /// 出场动画：文字从大小0.3+透明开始，0.5秒线性变大到1+不透明，结束后显示按钮。
    /// 淡出动画：按钮先线性消失，然后文字线性缩小+淡出，都是线性。
    /// </summary>
    public class InteractPromptPanel : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private TextMeshProUGUI promptText;   // 提示文字
        [SerializeField] private Button confirmButton;          // 确认
        [SerializeField] private Button cancelButton;           // 取消
        [SerializeField] private CanvasGroup canvasGroup;       // 控制显示/隐藏

        [Header("出场动画")]
        [SerializeField] private float showAnimDuration = 0.5f;  // 文字出场动画时长（秒）
        [SerializeField] private float startScale = 0.3f;        // 文字起始大小
        [SerializeField] private float startAlpha = 0f;           // 文字起始透明度
        [SerializeField] private float targetAlpha = 1f;          // 文字目标透明度

        [Header("淡出动画")]
        [SerializeField] private float hideButtonDuration = 0.15f; // 按钮淡出时长（秒）
        [SerializeField] private float hideTextDuration = 0.3f;    // 文字淡出时长（秒）

        private Interactable owner;   // 自己所属的NPC
        private Interactable target;  // 当前交互目标
        private Coroutine animCoroutine;
        private CanvasGroup confirmCG;
        private CanvasGroup cancelCG;

        private void Awake()
        {
            owner = GetComponentInParent<Interactable>();
            confirmButton.onClick.AddListener(OnConfirm);
            cancelButton.onClick.AddListener(OnCancel);

            // 自动给按钮加CanvasGroup（用于线性淡出）
            confirmCG = confirmButton.GetComponent<CanvasGroup>();
            if (confirmCG == null) confirmCG = confirmButton.gameObject.AddComponent<CanvasGroup>();
            cancelCG = cancelButton.GetComponent<CanvasGroup>();
            if (cancelCG == null) cancelCG = cancelButton.gameObject.AddComponent<CanvasGroup>();

            SetVisibleImmediate(false);
        }

        private void OnEnable() => MessageBus.Subscribe<InteractPromptMessage>(OnPrompt);
        private void OnDisable() => MessageBus.Unsubscribe<InteractPromptMessage>(OnPrompt);

        private void OnPrompt(InteractPromptMessage msg)
        {
            if (msg.Target != owner) return;
            target = msg.Target;
            SetVisible(msg.Show);
        }

        /// <summary>显示/隐藏面板（带动画）</summary>
        private void SetVisible(bool show)
        {
            if (animCoroutine != null) StopCoroutine(animCoroutine);

            if (show)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                animCoroutine = StartCoroutine(ShowAnimation());
            }
            else
            {
                animCoroutine = StartCoroutine(HideAnimation());
            }
        }

        // ── 出场动画 ──────────────────────────────────────────

        /// <summary>出场动画：文字从小到大+从透明到不透明（线性），结束后显示按钮</summary>
        private IEnumerator ShowAnimation()
        {
            // 起始状态：隐藏按钮，文字小且透明
            confirmButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(true);
            confirmCG.alpha = 0f;
            cancelCG.alpha = 0f;
            promptText.transform.localScale = Vector3.one * startScale;
            SetTextAlpha(startAlpha);

            // 文字线性动画
            float t = 0f;
            while (t < showAnimDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / showAnimDuration);
                promptText.transform.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, p);
                SetTextAlpha(Mathf.Lerp(startAlpha, targetAlpha, p));
                yield return null;
            }

            // 文字动画结束：按钮线性出现
            promptText.transform.localScale = Vector3.one;
            SetTextAlpha(targetAlpha);

            t = 0f;
            while (t < hideButtonDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / hideButtonDuration);
                confirmCG.alpha = p;
                cancelCG.alpha = p;
                yield return null;
            }
            confirmCG.alpha = 1f;
            cancelCG.alpha = 1f;
        }

        // ── 淡出动画 ──────────────────────────────────────────

        /// <summary>淡出动画：按钮先线性消失，然后文字线性缩小+淡出</summary>
        private IEnumerator HideAnimation()
        {
            // 第一步：按钮线性淡出
            float t = 0f;
            while (t < hideButtonDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / hideButtonDuration);
                confirmCG.alpha = 1f - p;
                cancelCG.alpha = 1f - p;
                yield return null;
            }
            confirmCG.alpha = 0f;
            cancelCG.alpha = 0f;
            confirmButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);

            // 第二步：文字线性缩小+淡出
            t = 0f;
            while (t < hideTextDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / hideTextDuration);
                promptText.transform.localScale = Vector3.one * Mathf.Lerp(1f, startScale, p);
                SetTextAlpha(Mathf.Lerp(targetAlpha, startAlpha, p));
                yield return null;
            }

            // 结束：完全隐藏
            promptText.transform.localScale = Vector3.one * startScale;
            SetTextAlpha(startAlpha);
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        // ── 工具方法 ──────────────────────────────────────────

        /// <summary>瞬间设置显示状态（无动画，初始化用）</summary>
        private void SetVisibleImmediate(bool show)
        {
            canvasGroup.alpha = show ? 1f : 0f;
            canvasGroup.blocksRaycasts = show;

            if (show)
            {
                promptText.transform.localScale = Vector3.one;
                SetTextAlpha(targetAlpha);
                confirmButton.gameObject.SetActive(true);
                cancelButton.gameObject.SetActive(true);
                if (confirmCG != null) confirmCG.alpha = 1f;
                if (cancelCG != null) cancelCG.alpha = 1f;
            }
            else
            {
                confirmButton.gameObject.SetActive(false);
                cancelButton.gameObject.SetActive(false);
            }
        }

        /// <summary>设置文字透明度</summary>
        private void SetTextAlpha(float alpha)
        {
            Color c = promptText.color;
            c.a = alpha;
            promptText.color = c;
        }

        private void OnConfirm()
        {
            MessageBus.Publish(new InteractConfirmMessage { Target = target });
            SetVisible(false);
        }

        private void OnCancel()
        {
            SetVisible(false);
        }
    }
}
