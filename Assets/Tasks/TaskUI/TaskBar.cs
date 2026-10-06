using UnityEngine;
using System.Collections;

namespace Game.UI
{
    /// <summary>
    /// 任务栏折叠/展开控制器。
    /// 展开时显示<按钮，点击<折叠到角落显示>按钮，点击>展开。
    /// <和>按钮循环播放"戳"动画（指数缓动），提醒玩家可点击。
    /// </summary>
    public class TaskBar : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private GameObject collapseButton;   // <按钮（展开状态显示，点击折叠）
        [SerializeField] private GameObject expandButton;     // >按钮（折叠状态显示，点击展开）
        [SerializeField] private GameObject taskContent;      // 任务内容容器（折叠时隐藏）

        [Header("折叠/展开位置（抽屉式，同一水平线左右移动）")]
        [SerializeField] private Vector2 expandedAnchoredPos;   // 展开时任务栏位置（左下角）
        [SerializeField] private float collapseOffsetX = -300f;  // 折叠时向左偏移多少像素（负数=向左）
        [SerializeField] private float toggleDuration = 0.3f;   // 折叠/展开移动时长

        [Header("按钮显示延迟")]
        [SerializeField] private float showButtonDelay = 3f;    // 游戏开始后多少秒显示<按钮（等所有动画播完）

        [Header("按钮戳动画")]
        [SerializeField] private float pokeWaitTime = 1.2f;      // 每次戳之前等待多久
        [SerializeField] private float pokeOutDuration = 0.3f;   // 戳出时长（渐缓 easeOut）
        [SerializeField] private float pokeBackDuration = 0.2f;  // 戳回时长（加速 easeIn）
        [SerializeField] private float pokeDistance = 12f;        // 戳动距离（像素）

        private bool isExpanded = true;
        private RectTransform rectTransform;
        private Coroutine toggleCoroutine;

        /// <summary>任务栏是否展开（外部读取用）</summary>
        public bool IsExpanded => isExpanded;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            // 默认展开
            SetExpandedImmediate(true);
            // 先隐藏<按钮，等3秒（所有动画播完）再显示
            if (collapseButton != null) collapseButton.SetActive(false);
            StartCoroutine(ShowButtonAfterDelay());
            StartCoroutine(PokeLoop());
        }

        /// <summary>延迟显示<按钮（等所有动画播完）</summary>
        private IEnumerator ShowButtonAfterDelay()
        {
            yield return new WaitForSeconds(showButtonDelay);
            if (collapseButton != null && isExpanded)
                collapseButton.SetActive(true);
        }

        /// <summary>切换折叠/展开（按钮点击时调用）</summary>
        public void Toggle()
        {
            SetExpanded(!isExpanded);
        }

        /// <summary>折叠（外部调用）</summary>
        public void Collapse() => SetExpanded(false);

        /// <summary>展开（外部调用）</summary>
        public void Expand() => SetExpanded(true);

        private void SetExpanded(bool expanded)
        {
            isExpanded = expanded;

            // 切换按钮显示
            if (collapseButton != null) collapseButton.SetActive(expanded);
            if (expandButton != null) expandButton.SetActive(!expanded);

            // 任务内容显示/隐藏
            if (taskContent != null) taskContent.SetActive(expanded);

            // 任务栏位置移动动画（折叠时=展开位置+向左偏移）
            if (toggleCoroutine != null) StopCoroutine(toggleCoroutine);
            Vector2 targetPos = expanded ? expandedAnchoredPos : expandedAnchoredPos + new Vector2(collapseOffsetX, 0f);
            toggleCoroutine = StartCoroutine(MoveToPosition(targetPos));
        }

        /// <summary>瞬间设置状态（初始化用，无动画）</summary>
        private void SetExpandedImmediate(bool expanded)
        {
            isExpanded = expanded;
            if (collapseButton != null) collapseButton.SetActive(expanded);
            if (expandButton != null) expandButton.SetActive(!expanded);
            if (taskContent != null) taskContent.SetActive(expanded);
            if (rectTransform != null)
            {
                Vector2 pos = expanded ? expandedAnchoredPos : expandedAnchoredPos + new Vector2(collapseOffsetX, 0f);
                rectTransform.anchoredPosition = pos;
            }
        }

        /// <summary>任务栏位置移动（指数平滑）</summary>
        private IEnumerator MoveToPosition(Vector2 targetPos)
        {
            Vector2 startPos = rectTransform.anchoredPosition;
            float t = 0;
            while (t < toggleDuration)
            {
                t += Time.deltaTime;
                float p = t / toggleDuration;
                float eased = 1f - Mathf.Exp(-5f * p);  // 指数平滑
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, eased);
                yield return null;
            }
            rectTransform.anchoredPosition = targetPos;
        }

        /// <summary>按钮戳动画循环（<向右戳，>向左戳）</summary>
        private IEnumerator PokeLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(pokeWaitTime);

                // 当前显示的按钮
                GameObject currentButton = isExpanded ? collapseButton : expandButton;
                if (currentButton == null) continue;
                if (!currentButton.activeSelf) continue;  // 按钮没显示就不播

                // <按钮向右戳，>按钮向左戳
                float direction = isExpanded ? 1f : -1f;
                Vector3 originPos = currentButton.transform.localPosition;

                // ── 戳出：easeOutExpo（渐缓，先快后慢）──
                float t = 0;
                while (t < pokeOutDuration)
                {
                    t += Time.deltaTime;
                    float p = t / pokeOutDuration;
                    float eased = 1f - Mathf.Pow(2f, -10f * p);  // easeOutExpo
                    currentButton.transform.localPosition = originPos + Vector3.right * direction * pokeDistance * eased;
                    yield return null;
                }

                // ── 戳回：easeInExpo（加速，先慢后快，形成"戳"的冲击力）──
                t = 0;
                while (t < pokeBackDuration)
                {
                    t += Time.deltaTime;
                    float p = t / pokeBackDuration;
                    float eased = Mathf.Pow(2f, 10f * (p - 1f));  // easeInExpo
                    currentButton.transform.localPosition = originPos + Vector3.right * direction * pokeDistance * (1f - eased);
                    yield return null;
                }

                currentButton.transform.localPosition = originPos;
            }
        }
    }
}
