using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Game.Core;
using Game.Player;
using Game.Items;
using Player; // PlayerMessageBus: stamina PP message now travels on the Player domain bus

namespace Game.UI
{
    /// <summary>
    /// HUD Panel: PP (stamina) bar, Gold display, Score display.
    /// - Main world: Gold hidden by default, PP shows when sprinting, Score hidden
    /// - Status panel open: all bars show with animation
    /// - PP bar: shows on sprint, hides when full + 2s no sprint, red below 12%
    /// - 玩家血条已拆出（Assets\PlayerUnit\PlayerHealth\Plug_HealthBar\HealthBarUI），本面板不再管血
    /// </summary>
    public class HUDPanel : MonoBehaviour
    {
        [Header("References - PP (Top Right, shown with status panel)")]
        [SerializeField] private GameObject ppGroup;       // PP bar + text container (top right)
        [SerializeField] private Image ppFill;             // PP fill image
        [SerializeField] private TextMeshProUGUI ppText;   // PP text (shown when panel open)
        [SerializeField] private float ppBarMaxWidth = 200f; // PP bar max width
        [SerializeField] private Color ppNormalColor = Color.green;
        [SerializeField] private Color ppLowColor = Color.red;

        [Header("References - PP (Floating, shown when sprinting)")]
        [SerializeField] private GameObject floatingPPGroup;  // Separate PP bar shown when sprinting
        [SerializeField] private Image floatingPPFill;        // Floating PP fill
        [SerializeField] private float floatingPPBarMaxWidth = 150f;
        [SerializeField] private float floatingPPFadeDuration = 0.5f; // Fade out duration

        [Header("References - Gold")]
        [SerializeField] private GameObject goldGroup;     // Gold display container
        [SerializeField] private TextMeshProUGUI goldText; // Gold text

        [Header("References - Score")]
        [SerializeField] private GameObject scoreGroup;    // Score display container（数据源已移除，壳保留待后续处理）

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.2f;
        [SerializeField] private float slideDistance = 50f;
        [SerializeField] private float hideDelay = 0.1f;  // Delay before hiding (slower than status panel)

        [Header("PP Settings")]
        [SerializeField] private float ppHideDelay = 2f;   // Full + no sprint for this long → hide

        private bool isStatusPanelOpen = false;
        private bool isInDungeon = false;
        private float ppHideTimer;
        private Coroutine animCoroutine;
        private Coroutine floatingPPFadeCoroutine;

        [Header("Dungeon Score Delay")]
        [SerializeField] private float dungeonScoreShowDelay = 2f;  // 进入地牢后分数延迟显示时间
        [SerializeField] private float dungeonScoreFadeDuration = 0.5f;  // 分数渐显时长

        private Coroutine delayedScoreCoroutine;  // 分数延迟显示协程

        // Current values
        private float currentPP;
        private float maxPP;
        private bool isSprinting;

        private void OnEnable()
        {
            PlayerMessageBus.Subscribe<PlayerStaminaChangedMessage>(OnStaminaChanged);
            MessageBus.Subscribe<GoldChangedMessage>(OnGoldChanged);
        }

        private void OnDisable()
        {
            PlayerMessageBus.Unsubscribe<PlayerStaminaChangedMessage>(OnStaminaChanged);
            MessageBus.Unsubscribe<GoldChangedMessage>(OnGoldChanged);
        }

        private void Start()
        {
            // Initialize default state: main world, panel closed
            UpdateDefaultVisibility();
            UpdateGoldDisplay();

            // Auto-detect if we're in a dungeon scene (in case WorldDirector call missed due to async load)
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isDungeon = sceneName.Contains("World2") || sceneName.Contains("LargeWorld") || sceneName.Contains("BossWorld");
            if (isDungeon)
            {
                SetInDungeon(true);
            }
        }

        private void Update()
        {
            // Floating PP bar auto-hide logic (only when status panel closed)
            if (!isStatusPanelOpen && floatingPPGroup != null && floatingPPGroup.activeSelf)
            {
                if (currentPP >= maxPP && !isSprinting)
                {
                    ppHideTimer += Time.deltaTime;
                    if (ppHideTimer >= ppHideDelay)
                    {
                        // Fade out instead of immediately hiding
                        if (floatingPPFadeCoroutine == null)
                        {
                            floatingPPFadeCoroutine = StartCoroutine(FadeOutFloatingPP());
                        }
                    }
                }
                else
                {
                    ppHideTimer = 0f;
                    // Cancel fade out if sprinting again
                    if (floatingPPFadeCoroutine != null)
                    {
                        StopCoroutine(floatingPPFadeCoroutine);
                        floatingPPFadeCoroutine = null;
                        SetGroupAlpha(floatingPPGroup, 1f);
                    }
                }
            }
        }

        /// <summary>Fade out floating PP bar with linear animation</summary>
        private IEnumerator FadeOutFloatingPP()
        {
            float startAlpha = 1f;
            float t = 0f;

            while (t < floatingPPFadeDuration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, 0f, t / floatingPPFadeDuration);
                SetGroupAlpha(floatingPPGroup, alpha);
                yield return null;
            }

            SetGroupAlpha(floatingPPGroup, 0f);
            floatingPPGroup.SetActive(false);
            floatingPPFadeCoroutine = null;
        }

        // ── Message handlers ──────────────────────────────────

        private void OnStaminaChanged(PlayerStaminaChangedMessage msg)
        {
            currentPP = msg.CurrentStamina;
            maxPP = msg.MaxStamina;
            isSprinting = msg.IsSprinting;
            UpdatePPDisplay();

            // Show floating PP bar when sprinting (if panel closed)
            if (!isStatusPanelOpen && isSprinting && floatingPPGroup != null && !floatingPPGroup.activeSelf)
            {
                floatingPPGroup.SetActive(true);
                SetGroupAlpha(floatingPPGroup, 1f);
                ppHideTimer = 0f;
                if (floatingPPFadeCoroutine != null)
                {
                    StopCoroutine(floatingPPFadeCoroutine);
                    floatingPPFadeCoroutine = null;
                }
            }
        }

        private void OnGoldChanged(GoldChangedMessage msg)
        {
            UpdateGoldDisplay();
        }

        // ── Public methods (called by StatusPanelDirector) ────

        /// <summary>Called when status panel opens: show HP/Gold/PP with animation</summary>
        public void OnStatusPanelOpen()
        {
            isStatusPanelOpen = true;
            ShowBarsWithAnimation();
        }

        /// <summary>Called when status panel closes: hide HP/Gold, keep PP if sprinting</summary>
        public void OnStatusPanelClose()
        {
            isStatusPanelOpen = false;
            HideBarsWithAnimation();
        }

        /// <summary>Set whether player is in dungeon (shows score by default)</summary>
        public void SetInDungeon(bool inDungeon)
        {
            isInDungeon = inDungeon;

            // 取消之前的延迟显示协程
            if (delayedScoreCoroutine != null)
            {
                StopCoroutine(delayedScoreCoroutine);
                delayedScoreCoroutine = null;
            }

            if (inDungeon)
            {
                // 进入地牢：Score延迟2秒渐显
                delayedScoreCoroutine = StartCoroutine(DelayedShowScore());
            }
            else
            {
                // 离开地牢：立即隐藏Score
                if (scoreGroup != null && !isStatusPanelOpen)
                {
                    scoreGroup.SetActive(false);
                }
            }
        }

        /// <summary>延迟显示Score：等待2秒后渐显出现</summary>
        private IEnumerator DelayedShowScore()
        {
            // 等待延迟时间
            yield return new WaitForSeconds(dungeonScoreShowDelay);

            // 如果StatusPanel已经打开，条已经显示了，不用再显示
            if (isStatusPanelOpen) yield break;

            // 渐显Score
            if (scoreGroup != null)
            {
                scoreGroup.SetActive(true);
                SetGroupAlpha(scoreGroup, 0f);
            }

            float t = 0f;
            while (t < dungeonScoreFadeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / dungeonScoreFadeDuration);
                // 指数型渐显
                float ease = 1f - Mathf.Pow(1f - p, 3f);
                if (scoreGroup != null) SetGroupAlpha(scoreGroup, ease);
                yield return null;
            }

            if (scoreGroup != null) SetGroupAlpha(scoreGroup, 1f);

            delayedScoreCoroutine = null;
        }

        // ── Display updates ───────────────────────────────────

        private void UpdatePPDisplay()
        {
            float ppRatio = maxPP > 0 ? currentPP / maxPP : 0f;

            // Update top-right PP bar (shown with status panel)
            if (ppFill != null)
            {
                RectTransform rt = ppFill.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector2 size = rt.sizeDelta;
                    size.x = ppBarMaxWidth * ppRatio;
                    rt.sizeDelta = size;
                }

                // Color: red below threshold, normal otherwise
                if (ppRatio <= PlayerConfig.StaminaLowThreshold)
                {
                    ppFill.color = Color.Lerp(ppFill.color, ppLowColor, Time.deltaTime * 10f);
                }
                else
                {
                    ppFill.color = Color.Lerp(ppFill.color, ppNormalColor, Time.deltaTime * 10f);
                }
            }

            // Update floating PP bar (shown when sprinting)
            if (floatingPPFill != null)
            {
                RectTransform rt = floatingPPFill.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector2 size = rt.sizeDelta;
                    size.x = floatingPPBarMaxWidth * ppRatio;
                    rt.sizeDelta = size;
                }

                // Same color logic
                if (ppRatio <= PlayerConfig.StaminaLowThreshold)
                {
                    floatingPPFill.color = Color.Lerp(floatingPPFill.color, ppLowColor, Time.deltaTime * 10f);
                }
                else
                {
                    floatingPPFill.color = Color.Lerp(floatingPPFill.color, ppNormalColor, Time.deltaTime * 10f);
                }
            }

            if (ppText != null)
            {
                ppText.text = Mathf.RoundToInt(currentPP) + " / " + maxPP;
                // Show text only when status panel is open
                ppText.gameObject.SetActive(isStatusPanelOpen);
            }
        }

        private void UpdateGoldDisplay()
        {
            if (goldText != null)
            {
                int gold = ItemDirector.Instance != null ? ItemDirector.Instance.CurrentGold : EquipmentConfig.InitialGold;
                goldText.text = gold + " G";
            }
        }

        private void UpdateDefaultVisibility()
        {
            // Main world default: Gold hidden, PP hidden, Score hidden
            if (goldGroup != null) goldGroup.SetActive(false);
            if (ppGroup != null) ppGroup.SetActive(false);
            if (floatingPPGroup != null) floatingPPGroup.SetActive(false);
            if (scoreGroup != null) scoreGroup.SetActive(false);
        }

        // ── Animations ────────────────────────────────────────

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
            // Show all groups
            if (goldGroup != null) goldGroup.SetActive(true);
            if (ppGroup != null) ppGroup.SetActive(true);
            if (scoreGroup != null && isInDungeon) scoreGroup.SetActive(true);

            // Fade in + slide in (exponential)
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / fadeDuration);
                float eased = 1f - Mathf.Pow(2f, -10f * p); // easeOutExpo

                SetBarsAlpha(eased);
                SetBarsSlideOffset(slideDistance * (1f - eased));

                yield return null;
            }

            SetBarsAlpha(1f);
            SetBarsSlideOffset(0f);
        }

        private IEnumerator HideBarsCoroutine()
        {
            // Wait a bit (slower than status panel - "慢半拍")
            yield return new WaitForSeconds(hideDelay);

            // Fade out + slide out (easeOutExpo: fast start, slow end)
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / fadeDuration);
                float eased = 1f - Mathf.Pow(2f, -10f * p); // easeOutExpo (fast start)

                SetBarsAlpha(1f - eased);
                SetBarsSlideOffset(slideDistance * eased);

                yield return null;
            }

            // Hide Gold, keep Score if in dungeon
            // 左上角PP条只在StatusPanel打开时显示，关闭时一定隐藏（奔跑时用floatingPPGroup）
            if (goldGroup != null) goldGroup.SetActive(false);
            if (ppGroup != null) ppGroup.SetActive(false);
            if (scoreGroup != null && !isInDungeon) scoreGroup.SetActive(false);

            SetBarsAlpha(1f);
            SetBarsSlideOffset(0f);
        }

        private void SetBarsAlpha(float alpha)
        {
            SetGroupAlpha(ppGroup, alpha);
            SetGroupAlpha(goldGroup, alpha);
            SetGroupAlpha(scoreGroup, alpha);
        }

        private void SetGroupAlpha(GameObject group, float alpha)
        {
            if (group == null) return;
            CanvasGroup cg = group.GetComponent<CanvasGroup>();
            if (cg == null) cg = group.AddComponent<CanvasGroup>();
            cg.alpha = alpha;
        }

        private void SetBarsSlideOffset(float offset)
        {
            SetGroupSlide(ppGroup, offset);
            SetGroupSlide(goldGroup, offset);
            SetGroupSlide(scoreGroup, offset);
        }

        private void SetGroupSlide(GameObject group, float offset)
        {
            if (group == null) return;
            RectTransform rt = group.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Slide from left
                Vector2 pos = rt.anchoredPosition;
                pos.x = -offset; // Store original position in inspector
                rt.anchoredPosition = pos;
            }
        }
    }
}
