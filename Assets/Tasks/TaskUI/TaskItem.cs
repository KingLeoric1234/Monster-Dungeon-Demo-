using UnityEngine;
using TMPro;
using System.Collections;

namespace Game.UI
{
    /// <summary>
    /// 单个任务条目：显示描述 + 出场动画 + 提醒动画 + 完成动画。
    /// 挂在任务条目预制体上，可复用于所有任务类型。
    /// 所有动画参数在Inspector里可调。
    /// </summary>
    public class TaskItem : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private TMP_Text descriptionText;  // 任务描述文本

        [Header("出场动画参数")]
        [SerializeField] private float enterDuration = 0.3f;       // 出场动画时长（秒）
        [SerializeField] private float enterMoveDistance = 50f;    // 从右向左滑入的距离（像素）

        [Header("提醒动画参数（进行中任务的微光扫过）")]
        [SerializeField] private bool enableReminder = true;        // 是否启用提醒动画
        [SerializeField] private float reminderInterval = 2f;       // 两次扫过之间的间隔（秒）
        [SerializeField] private float reminderStepTime = 0.03f;    // 条纹每移动一步的时间（秒，越小越快）
        [SerializeField] private int reminderWidth = 3;              // 条纹宽度（字符数）
        [SerializeField] private Color reminderNormalColor = new Color(0.5f, 0.5f, 0.5f, 1f);  // 默认灰色
        [SerializeField] private Color reminderHighlightColor = Color.white;  // 条纹扫过的亮色

        [Header("完成动画参数")]
        [SerializeField] private Color completeColor = Color.green;   // 完成时文字颜色
        [SerializeField] private float holdAfterExtract = 0.2f;      // 抽出后静止时间（秒）
        [SerializeField] private float extractDuration = 0.3f;        // 折叠时抽出动画时长（秒）
        [SerializeField] private float flyDuration = 0.5f;            // 向右滑出时长（秒）
        [SerializeField] private float flyDistance = 100f;            // 向右滑出距离（像素）

        private CanvasGroup canvasGroup;  // 控制整体透明度
        private string originalText;       // 原始任务描述（提醒动画用）
        private Coroutine reminderCoroutine;  // 提醒动画协程引用（用于停止）

        private void Awake()
        {
            // 自动获取或添加CanvasGroup，用于控制淡入淡出
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>设置任务描述文字</summary>
        public void SetDescription(string description)
        {
            if (descriptionText != null)
            {
                descriptionText.text = description;
                originalText = description;  // 保存原始文字，提醒动画用
            }
        }

        // ── 提醒动画（进行中任务的微光扫过） ──

        /// <summary>开始播放提醒动画（任务出现在列表时调用）</summary>
        public void StartReminder()
        {
            if (!enableReminder || descriptionText == null) return;
            StopReminder();  // 先停掉之前的
            reminderCoroutine = StartCoroutine(PlayReminder());
        }

        /// <summary>停止提醒动画（任务完成时调用）</summary>
        public void StopReminder()
        {
            if (reminderCoroutine != null)
            {
                StopCoroutine(reminderCoroutine);
                reminderCoroutine = null;
            }
            // 恢复原始文字和颜色
            if (descriptionText != null)
            {
                descriptionText.text = originalText;
            }
        }

        /// <summary>提醒动画：灰色文字 + 亮色条纹从左扫到右，循环播放</summary>
        private IEnumerator PlayReminder()
        {
            // 先设为灰色
            descriptionText.color = reminderNormalColor;

            while (true)
            {
                int length = originalText.Length;

                // 条纹从左移到右（包含从外进入和从外离开）
                for (int i = -reminderWidth; i < length; i++)
                {
                    string result = "";
                    for (int j = 0; j < length; j++)
                    {
                        if (j >= i && j < i + reminderWidth)
                        {
                            // 条纹内的字符用亮色
                            result += $"<color=#{ColorUtility.ToHtmlStringRGB(reminderHighlightColor)}>{originalText[j]}</color>";
                        }
                        else
                        {
                            result += originalText[j];
                        }
                    }
                    descriptionText.text = result;
                    yield return new WaitForSeconds(reminderStepTime);
                }

                // 恢复原始文字，等待下一次扫过
                descriptionText.text = originalText;
                yield return new WaitForSeconds(reminderInterval);
            }
        }

        /// <summary>
        /// 出场动画：从右向左滑入 + 淡入。
        /// 等一帧让LayoutGroup算好位置，再从偏移位置滑到目标位置。
        /// </summary>
        public IEnumerator PlayEnterAnimation()
        {
            // 等一帧，让VerticalLayoutGroup计算好条目位置
            yield return null;

            Vector3 targetPos = transform.position;
            Vector3 startPos = targetPos + Vector3.right * enterMoveDistance;

            // 起始状态：偏右 + 透明
            transform.position = startPos;
            canvasGroup.alpha = 0f;

            // 滑入 + 淡入
            float t = 0;
            while (t < enterDuration)
            {
                t += Time.deltaTime;
                float p = t / enterDuration;
                transform.position = Vector3.Lerp(startPos, targetPos, p);
                canvasGroup.alpha = p;
                yield return null;
            }

            // 确保最终状态正确
            transform.position = targetPos;
            canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 播放完成动画（三阶段）：
        /// ① 抽出后原封不动静止0.2秒
        /// ② 变绿+划线
        /// ③ 向右指数滑出 + 淡出（easeInExpo）
        /// 调用前需要先把条目移出LayoutGroup（SetParent到动画层）。
        /// 回档说明：如果划线显示异常，删除 descriptionText.fontStyle 那一行即可。
        /// </summary>
        public IEnumerator PlayCompleteAnimation()
        {
            // 先停止提醒动画
            StopReminder();
            // 移到最上层，防止被挡住
            transform.SetAsLastSibling();

            // ── 阶段①：抽出后原封不动静止 ──
            yield return new WaitForSeconds(holdAfterExtract);

            // ── 阶段②：变绿 + 划线 ──
            if (descriptionText != null)
            {
                descriptionText.color = completeColor;
                descriptionText.fontStyle |= FontStyles.Strikethrough;  // 回档删这行
            }

            // ── 阶段③：向右指数滑出 + 淡出（easeInExpo，保持原大小，同一水平线）──
            Vector3 startPos = transform.position;
            float t = 0;
            while (t < flyDuration)
            {
                t += Time.deltaTime;
                float p = t / flyDuration;
                float eased = Mathf.Pow(2f, 10f * (p - 1f));  // easeInExpo
                transform.position = startPos + Vector3.right * eased * flyDistance;
                canvasGroup.alpha = 1f - eased;
                yield return null;
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// 折叠状态下的完成动画（抽屉抽出式）：
        /// ① 从屏幕外快速向右抽出（easeOutExpo，先快后慢），y对齐目标位置
        /// ② 变绿+划线，短暂停留
        /// ③ 向右移动一小段距离，淡出（easeInExpo）
        /// </summary>
        public IEnumerator PlayCollapsedCompleteAnimation(Vector3 targetPos)
        {
            // 先停止提醒动画
            StopReminder();
            // 移到最上层，防止被挡住
            transform.SetAsLastSibling();

            // 起始位置：当前位置（屏幕外），y对齐目标位置
            Vector3 startPos = transform.position;
            startPos.y = targetPos.y;
            transform.position = startPos;
            canvasGroup.alpha = 1f;  // 确保可见

            // ── 阶段①：向右快速抽出（easeOutExpo，先快后慢）──
            float t = 0;
            while (t < extractDuration)
            {
                t += Time.deltaTime;
                float p = t / extractDuration;
                float eased = 1f - Mathf.Pow(2f, -10f * p);  // easeOutExpo
                transform.position = Vector3.Lerp(startPos, targetPos, eased);
                yield return null;
            }
            transform.position = targetPos;

            // ── 阶段②：变绿 + 划线，短暂停留 ──
            if (descriptionText != null)
            {
                descriptionText.color = completeColor;
                descriptionText.fontStyle |= FontStyles.Strikethrough;
            }
            yield return new WaitForSeconds(holdAfterExtract);

            // ── 阶段③：向右移动一小段距离，淡出（easeInExpo）──
            Vector3 flyStartPos = transform.position;
            Vector3 flyEndPos = flyStartPos + Vector3.right * flyDistance;
            t = 0;
            while (t < flyDuration)
            {
                t += Time.deltaTime;
                float p = t / flyDuration;
                float eased = Mathf.Pow(2f, 10f * (p - 1f));  // easeInExpo
                transform.position = Vector3.Lerp(flyStartPos, flyEndPos, eased);
                canvasGroup.alpha = 1f - eased;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
