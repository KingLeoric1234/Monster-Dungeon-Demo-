using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Game.Core;
using Game.Items;
using Game.Player;

namespace Game.UI
{
    /// <summary>
    /// 独立的战斗属性面板。
    /// 按Q键召唤/关闭，显示ATK/DEF/CRT/SPD四个属性。
    /// 位置在屏幕左上角，抽屉式弹出动画。
    /// 渲染优先级高于StatusPanel，独立存在。
    /// 挂在StatsPanel物体上，需要CanvasGroup组件。
    /// </summary>
    public class StatsPanel : MonoBehaviour
    {
        [Header("动画参数")]
        [SerializeField] private float animDuration = 0.25f;  // 动画时长
        [SerializeField] private float slideDistance = 100f;   // 抽屉滑出距离

        [Header("引用 - 属性文本")]
        [SerializeField] private TextMeshProUGUI atkText;  // 攻击力
        [SerializeField] private TextMeshProUGUI defText;  // 防御力
        [SerializeField] private TextMeshProUGUI crtText;  // 暴击率
        [SerializeField] private TextMeshProUGUI spdText;  // 移速

        [Header("引用 - 玩家移动（用于显示实际速度）")]
        [SerializeField] private PlayerMovement playerMovement;  // 玩家移动脚本

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private bool isOpen = false;
        private Coroutine currentAnim;
        private Vector2 originalPos;

        /// <summary>面板是否打开（供外部读取）</summary>
        public bool IsOpen => isOpen;

        private void Awake()
        {
            // 获取CanvasGroup
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 获取RectTransform
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                originalPos = rectTransform.anchoredPosition;
            }

            // 初始隐藏
            HideImmediate();
        }

        private void Start()
        {
            // 自动找PlayerMovement（如果没拖引用）
            if (playerMovement == null)
            {
                playerMovement = FindObjectOfType<PlayerMovement>();
            }
        }

        private void Update()
        {
            // 按Q切换面板
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Toggle();
            }

            // 面板打开时，每帧更新SPD（因为实际速度一直在变）
            if (isOpen && playerMovement != null && spdText != null)
            {
                float actualSpeed = playerMovement.GetCurrentSpeed();
                spdText.text = "SPD " + actualSpeed.ToString("F1");
            }
        }

        /// <summary>切换显示/隐藏</summary>
        public void Toggle()
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        /// <summary>打开面板（抽屉式弹出）</summary>
        public void Open()
        {
            if (isOpen) return;
            isOpen = true;

            // 更新属性显示
            UpdateStats();

            if (currentAnim != null) StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(OpenAnim());
        }

        /// <summary>关闭面板（抽屉式收回）</summary>
        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;

            if (currentAnim != null) StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(CloseAnim());
        }

        /// <summary>打开动画：从左上方滑出+淡入</summary>
        private IEnumerator OpenAnim()
        {
            if (canvasGroup == null || rectTransform == null) yield break;

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            float t = 0f;
            while (t < animDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / animDuration);
                // 指数型缓动：快速开始，渐缓结束
                float ease = 1f - Mathf.Pow(1f - p, 3f);

                canvasGroup.alpha = ease;
                rectTransform.anchoredPosition = originalPos + new Vector2(-slideDistance * (1f - ease), 0f);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            rectTransform.anchoredPosition = originalPos;
        }

        /// <summary>关闭动画：向左上方收回+淡出</summary>
        private IEnumerator CloseAnim()
        {
            if (canvasGroup == null || rectTransform == null) yield break;

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            float t = 0f;
            while (t < animDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / animDuration);
                // 指数型缓动：渐缓开始，快速结束
                float ease = Mathf.Pow(1f - p, 3f);

                canvasGroup.alpha = ease;
                rectTransform.anchoredPosition = originalPos + new Vector2(-slideDistance * (1f - ease), 0f);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            rectTransform.anchoredPosition = originalPos;
        }

        /// <summary>瞬间隐藏（初始化用）</summary>
        private void HideImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = originalPos;
            }
        }

        /// <summary>更新四个属性显示</summary>
        public void UpdateStats()
        {
            // 从PlayerStatsDirector获取最终属性
            if (PlayerStatsDirector.Instance != null)
            {
                int atk = PlayerStatsDirector.Instance.FinalAttack;
                int def = PlayerStatsDirector.Instance.FinalDefense;
                float crt = PlayerStatsDirector.Instance.FinalCritChance * 100f;
                float spd = PlayerStatsDirector.Instance.FinalMoveSpeed;

                if (atkText != null) atkText.text = "ATK " + atk;
                if (defText != null) defText.text = "DEF " + def;
                if (crtText != null) crtText.text = "CRT " + crt.ToString("F0") + "%";
                if (spdText != null) spdText.text = "SPD " + spd.ToString("F1");
            }
            else if (ItemDirector.Instance != null)
            {
                // 降级：从ItemDirector获取（PlayerStatsDirector还没初始化时）
                var stats = ItemDirector.Instance.CalculateTotalStats();
                int atk = 10;
                int def = stats.totalArmor;
                float crt = 0f;
                float spd = PlayerConfig.MoveSpeed * (1f - stats.totalSpeedPenalty);

                if (atkText != null) atkText.text = "ATK " + atk;
                if (defText != null) defText.text = "DEF " + def;
                if (crtText != null) crtText.text = "CRT " + crt.ToString("F0") + "%";
                if (spdText != null) spdText.text = "SPD " + spd.ToString("F1");
            }
            else
            {
                // 默认值
                if (atkText != null) atkText.text = "ATK 10";
                if (defText != null) defText.text = "DEF 0";
                if (crtText != null) crtText.text = "CRT 0%";
                if (spdText != null) spdText.text = "SPD 5.0";
            }
        }
    }
}
