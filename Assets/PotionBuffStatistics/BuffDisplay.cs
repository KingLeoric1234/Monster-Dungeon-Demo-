using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Game.Core;

namespace Game.PlayerStats
{
    /// <summary>
    /// Buff显示：右上角显示当前激活的药水Buff。
    /// - SortingOrder=10，显示在所有Panel之上
    /// - 出现时0.2s线性淡入
    /// - 剩余3秒时快速闪烁
    /// - StatusPanel关闭时只显示Icon，打开时显示名称
    /// - 鼠标悬停显示详情面板（描述+剩余时间）
    /// - 手动排列，每个间隔110像素，消失后自动紧凑
    /// 挂在Canvas下的BuffContainer物体上。
    /// </summary>
    public class BuffDisplay : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private GameObject buffIconPrefab;   // Buff图标预制体（带Image+Text+CanvasGroup）
        [SerializeField] private GameObject detailPanel;      // 详情面板（悬停时显示）
        [SerializeField] private TextMeshProUGUI detailName;  // 详情面板的名称
        [SerializeField] private TextMeshProUGUI detailDesc;  // 详情面板的描述
        [SerializeField] private TextMeshProUGUI detailTime;  // 详情面板的剩余时间

        [Header("设置")]
        [SerializeField] private float fadeInDuration = 0.2f;  // 淡入时长
        [SerializeField] private float buffSpacing = 110f;     // 每个Buff之间的间距（像素）
        [SerializeField] private float blinkStartTime = 3f;    // 剩余多少秒开始闪烁
        [SerializeField] private float blinkSpeed = 8f;         // 闪烁速度
        [SerializeField] private float detailFadeDuration = 0.15f;  // DetailPanel淡入淡出时长
        [SerializeField] private float detailYOffset = -80f;   // DetailPanel在Buff下方的偏移量（负数=向下）

        // Buff显示对象：key=Buff名称，value=GameObject
        private Dictionary<string, GameObject> buffObjects = new Dictionary<string, GameObject>();
        // 当前悬停的Buff名称
        private string hoveredBuffName = null;
        // DetailPanel的CanvasGroup（用于淡入淡出）
        private CanvasGroup detailCanvasGroup;
        // DetailPanel淡出协程
        private Coroutine detailFadeCoroutine;

        private void Awake()
        {
            // 确保有Canvas组件，设置sortingOrder
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            canvas.sortingOrder = 0;  // 默认0，打开StatusPanel后调成1

            // 确保有GraphicRaycaster，否则鼠标事件无法触发
            GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = gameObject.AddComponent<GraphicRaycaster>();
            }

            // 自动加Horizontal Layout Group，让Buff自动排列
            HorizontalLayoutGroup layout = GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            }
            layout.spacing = buffSpacing;
            layout.childAlignment = TextAnchor.UpperRight;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // 详情面板默认隐藏，加CanvasGroup用于淡入淡出
            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
                detailCanvasGroup = detailPanel.GetComponent<CanvasGroup>();
                if (detailCanvasGroup == null)
                {
                    detailCanvasGroup = detailPanel.AddComponent<CanvasGroup>();
                }
                detailCanvasGroup.alpha = 0f;
            }
        }

        private void OnEnable()
        {
            MessageBus.Subscribe<BuffChangedMessage>(OnBuffChanged);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<BuffChangedMessage>(OnBuffChanged);
        }

        private void Update()
        {
            // 检查StatusPanel是否打开，动态调整sortingOrder
            bool statusPanelOpen = IsStatusPanelOpen();
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = statusPanelOpen ? 1 : 0;
            }

            // 控制Buff名称的显示/隐藏
            foreach (var kvp in buffObjects)
            {
                TextMeshProUGUI nameText = kvp.Value.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.enabled = statusPanelOpen;
                }

                // 剩余时间小于blinkStartTime时闪烁
                float remaining = PotionBuffDirector.Instance != null ?
                    PotionBuffDirector.Instance.GetBuffRemainingTimeByName(kvp.Key) : 0f;
                CanvasGroup cg = kvp.Value.GetComponent<CanvasGroup>();
                if (cg != null && remaining > 0f && remaining <= blinkStartTime)
                {
                    // 快速渐显渐隐
                    cg.alpha = 0.4f + 0.6f * Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
                }
                else if (cg != null)
                {
                    cg.alpha = 1f;
                }
            }

            // 更新悬停详情面板的剩余时间
            if (hoveredBuffName != null && detailPanel != null && detailPanel.activeSelf)
            {
                float remaining = PotionBuffDirector.Instance != null ?
                    PotionBuffDirector.Instance.GetBuffRemainingTimeByName(hoveredBuffName) : 0f;
                if (detailTime != null)
                {
                    detailTime.text = "Time: " + remaining.ToString("F1") + "s";
                }
            }
        }

        /// <summary>检查StatusPanel是否打开</summary>
        private bool IsStatusPanelOpen()
        {
            var director = FindObjectOfType<Game.UI.StatusPanelDirector>();
            if (director != null)
            {
                return director.IsOpen;
            }
            return false;
        }

        /// <summary>Buff变化时更新显示</summary>
        private void OnBuffChanged(BuffChangedMessage msg)
        {
            if (msg.IsAdded)
            {
                AddBuff(msg.BuffName, msg.Icon, msg.Duration, msg.Description);
            }
            else
            {
                RemoveBuff(msg.BuffName);
            }
        }

        /// <summary>增加一个Buff显示</summary>
        private void AddBuff(string buffName, Sprite icon, float duration, string description)
        {
            if (buffIconPrefab == null) return;

            // 如果已经有这个Buff，不重复添加
            if (buffObjects.ContainsKey(buffName))
            {
                return;
            }

            // 新建Buff图标
            GameObject buffObj = Instantiate(buffIconPrefab, transform);
            buffObj.name = buffName;

            // 设置图标
            Image iconImage = buffObj.GetComponentInChildren<Image>();
            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
            }

            // 设置名称
            TextMeshProUGUI nameText = buffObj.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = buffName;
                nameText.enabled = IsStatusPanelOpen();
            }

            // 确保根物体有一个可射线检测的Image（透明的，用于接收鼠标事件）
            Image raycastImage = buffObj.GetComponent<Image>();
            if (raycastImage == null)
            {
                raycastImage = buffObj.AddComponent<Image>();
                raycastImage.color = new Color(0, 0, 0, 0); // 透明
            }
            raycastImage.raycastTarget = true;

            // 加BuffHover组件，处理鼠标悬停
            BuffHover hover = buffObj.AddComponent<BuffHover>();
            hover.Initialize(buffName, description, this);

            // 加CanvasGroup用于淡入和闪烁
            CanvasGroup canvasGroup = buffObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = buffObj.AddComponent<CanvasGroup>();
            }
            canvasGroup.blocksRaycasts = true;  // 确保能接收鼠标事件

            // 加入字典
            buffObjects[buffName] = buffObj;

            // 淡入动画
            StartCoroutine(FadeIn(canvasGroup, fadeInDuration));
        }

        /// <summary>移除一个Buff显示</summary>
        private void RemoveBuff(string buffName)
        {
            if (buffObjects.ContainsKey(buffName))
            {
                // 如果正在悬停这个Buff，隐藏详情面板
                if (hoveredBuffName == buffName)
                {
                    HideDetailPanel();
                    hoveredBuffName = null;
                }

                Destroy(buffObjects[buffName]);
                buffObjects.Remove(buffName);
            }
        }

        /// <summary>淡入动画</summary>
        private IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
        {
            canvasGroup.alpha = 0f;
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(timer / duration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        /// <summary>显示详情面板（由BuffHover调用）</summary>
        public void ShowDetailPanel(string buffName, string description)
        {
            hoveredBuffName = buffName;
            if (detailPanel == null || detailCanvasGroup == null) return;

            // 找到对应的Buff物体，把DetailPanel放在它的下方
            if (buffObjects.TryGetValue(buffName, out GameObject buffObj))
            {
                RectTransform buffRT = buffObj.GetComponent<RectTransform>();
                RectTransform detailRT = detailPanel.GetComponent<RectTransform>();
                if (buffRT != null && detailRT != null && detailRT.parent != null)
                {
                    // 把Buff的局部坐标转成世界坐标，再转成DetailPanel父物体的局部坐标
                    Vector3 worldPos = buffRT.TransformPoint(Vector3.zero);
                    Vector3 localPos = detailRT.parent.InverseTransformPoint(worldPos);
                    // Y轴加上偏移量，放在Buff下方
                    detailRT.anchoredPosition = new Vector2(localPos.x, localPos.y + detailYOffset);
                }
            }

            // 设置内容
            if (detailName != null) detailName.text = buffName;
            if (detailDesc != null) detailDesc.text = description;

            // 显示并淡入
            detailPanel.SetActive(true);
            if (detailFadeCoroutine != null) StopCoroutine(detailFadeCoroutine);
            detailFadeCoroutine = StartCoroutine(FadeDetailPanel(true, detailFadeDuration));
        }

        /// <summary>隐藏详情面板（由BuffHover调用）</summary>
        public void HideDetailPanel()
        {
            hoveredBuffName = null;
            if (detailPanel == null || detailCanvasGroup == null) return;

            // 淡出后隐藏
            if (detailFadeCoroutine != null) StopCoroutine(detailFadeCoroutine);
            detailFadeCoroutine = StartCoroutine(FadeDetailPanel(false, detailFadeDuration));
        }

        /// <summary>DetailPanel淡入淡出协程</summary>
        private IEnumerator FadeDetailPanel(bool fadeIn, float duration)
        {
            float startAlpha = detailCanvasGroup.alpha;
            float targetAlpha = fadeIn ? 1f : 0f;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                detailCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
                yield return null;
            }

            detailCanvasGroup.alpha = targetAlpha;
            if (!fadeIn)
            {
                detailPanel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Buff悬停组件：挂在每个Buff图标上，处理鼠标进入/离开。
    /// </summary>
    public class BuffHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private string buffName;
        private string description;
        private BuffDisplay parent;

        public void Initialize(string name, string desc, BuffDisplay p)
        {
            buffName = name;
            description = desc;
            parent = p;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (parent != null)
            {
                parent.ShowDetailPanel(buffName, description);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (parent != null)
            {
                parent.HideDetailPanel();
            }
        }
    }
}
