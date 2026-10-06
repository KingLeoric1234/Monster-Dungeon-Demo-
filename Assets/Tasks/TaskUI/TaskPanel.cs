using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using Game.Tasks;
using TMPro;

namespace Game.UI
{
    /// <summary>
    /// 任务UI面板：
    /// 1. 收到任务 → 播"Task Received"banner动画 → 动画结束后才添加任务条目
    /// 2. 任务完成 → 变绿 + 上滑淡出
    /// 3. 任务条目通过TaskItem组件管理，可复用
    /// </summary>
    public class TaskPanel : MonoBehaviour
    {
        [Header("Task Received Banner")]
        [SerializeField] private TMP_Text taskReceivedBanner;
        [SerializeField] private Transform bannerStart;
        [SerializeField] private Transform bannerEnd;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float holdDuration = 1f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float startScale = 0.5f;
        [SerializeField] private float endScale = 0.5f;
        [SerializeField] private float targetAlpha = 0.8f;

        [Header("Task List")]
        [SerializeField] private Transform taskListContainer;
        [SerializeField] private GameObject taskItemPrefab;
        [SerializeField] private Transform animationLayer;

        [Header("折叠时完成动画")]
        [SerializeField] private TaskBar taskBar;
        [SerializeField] private Transform collapsedCompletePos;

        [Header("空状态")]
        [SerializeField] private TMP_Text emptyStateText;
        [SerializeField] private string emptyStateMessage = "Excellent! No Requests";
        [SerializeField] private float emptyStateShowDelay = 3f;
        [SerializeField] private float emptyStateClearDelay = 0.5f;
        [SerializeField] private float emptyStateAnimDuration = 0.3f;

        private Dictionary<string, TaskItem> taskItems = new Dictionary<string, TaskItem>();
        private bool canShowEmptyState = false;
        private Coroutine emptyStateCoroutine;

        private void Start()
        {
            StartCoroutine(EnableEmptyStateAfterDelay());
        }

        private IEnumerator EnableEmptyStateAfterDelay()
        {
            yield return new WaitForSeconds(emptyStateShowDelay);
            canShowEmptyState = true;
            UpdateEmptyState();
        }

        private void OnEnable()
        {
            MessageBus.Subscribe<TaskReceivedMessage>(OnTaskReceived);
            MessageBus.Subscribe<TaskCompletedMessage>(OnTaskCompleted);

            if (emptyStateText != null)
            {
                emptyStateText.text = emptyStateMessage;
                emptyStateText.gameObject.SetActive(false);
            }

            if (TaskDirector.Instance != null)
            {
                foreach (Task task in TaskDirector.Instance.GetActiveTasks())
                {
                    if (!taskItems.ContainsKey(task.id) && task.status == TaskStatus.Active)
                    {
                        StartCoroutine(ReceivedSequence(task));
                    }
                }
            }
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<TaskReceivedMessage>(OnTaskReceived);
            MessageBus.Unsubscribe<TaskCompletedMessage>(OnTaskCompleted);
        }

        private void OnTaskReceived(TaskReceivedMessage msg)
        {
            StartCoroutine(ReceivedSequence(msg.Task));
        }

        private void OnTaskCompleted(TaskCompletedMessage msg)
        {
            if (taskItems.TryGetValue(msg.TaskId, out TaskItem item))
            {
                taskItems.Remove(msg.TaskId);
                item.transform.SetParent(animationLayer, true);

                if (taskBar != null && !taskBar.IsExpanded && collapsedCompletePos != null)
                {
                    StartCoroutine(item.PlayCollapsedCompleteAnimation(collapsedCompletePos.position));
                }
                else
                {
                    StartCoroutine(item.PlayCompleteAnimation());
                }

                UpdateEmptyState();
            }
        }

        private IEnumerator ReceivedSequence(Task task)
        {
            yield return StartCoroutine(PlayReceivedBanner());
            AddTaskItem(task);
        }

        private IEnumerator PlayReceivedBanner()
        {
            taskReceivedBanner.gameObject.SetActive(true);
            taskReceivedBanner.transform.position = bannerStart.position;
            taskReceivedBanner.transform.localScale = Vector3.one * startScale;
            SetBannerAlpha(0);

            float t = 0;
            while (t < fadeInDuration)
            {
                t += Time.deltaTime;
                float p = t / fadeInDuration;
                float eased = 1f - Mathf.Pow(2f, -10f * p);
                taskReceivedBanner.transform.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one, eased);
                SetBannerAlpha(Mathf.Lerp(0, targetAlpha, eased));
                yield return null;
            }
            taskReceivedBanner.transform.localScale = Vector3.one;
            SetBannerAlpha(targetAlpha);

            yield return new WaitForSeconds(holdDuration);

            Vector3 startPos = taskReceivedBanner.transform.position;
            t = 0;
            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                float p = t / fadeOutDuration;
                float eased = Mathf.Pow(2f, 10f * (p - 1f));
                taskReceivedBanner.transform.position = Vector3.Lerp(startPos, bannerEnd.position, eased);
                taskReceivedBanner.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * endScale, eased);
                SetBannerAlpha(Mathf.Lerp(targetAlpha, 0, eased));
                yield return null;
            }

            SetBannerAlpha(0);
            taskReceivedBanner.gameObject.SetActive(false);
        }

        private void AddTaskItem(Task task)
        {
            GameObject go = Instantiate(taskItemPrefab, taskListContainer);
            TaskItem item = go.GetComponent<TaskItem>();
            if (item != null)
            {
                item.SetDescription(task.taskName);
                taskItems[task.id] = item;
                StartCoroutine(item.PlayEnterAnimation());
                item.StartReminder();
            }
            UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            if (emptyStateText == null) return;
            bool isEmpty = taskItems.Count == 0;

            if (isEmpty && canShowEmptyState)
            {
                if (emptyStateCoroutine != null) StopCoroutine(emptyStateCoroutine);
                emptyStateCoroutine = StartCoroutine(ShowEmptyStateWithAnimation());
            }
            else
            {
                if (emptyStateCoroutine != null) StopCoroutine(emptyStateCoroutine);
                emptyStateText.gameObject.SetActive(false);
            }
        }

        private IEnumerator ShowEmptyStateWithAnimation()
        {
            yield return new WaitForSeconds(emptyStateClearDelay);
            if (taskItems.Count > 0 || !canShowEmptyState) yield break;

            emptyStateText.text = emptyStateMessage;
            emptyStateText.gameObject.SetActive(true);

            RectTransform rect = emptyStateText.GetComponent<RectTransform>();
            Vector2 targetPos = rect.anchoredPosition;
            Vector2 startPos = targetPos + new Vector2(100f, 0f);
            rect.anchoredPosition = startPos;
            SetTextAlpha(emptyStateText, 0f);

            float t = 0;
            while (t < emptyStateAnimDuration)
            {
                t += Time.deltaTime;
                float p = t / emptyStateAnimDuration;
                float eased = 1f - Mathf.Pow(2f, -10f * p);
                rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, eased);
                SetTextAlpha(emptyStateText, Mathf.Lerp(0f, 1f, eased));
                yield return null;
            }

            rect.anchoredPosition = targetPos;
            SetTextAlpha(emptyStateText, 1f);
        }

        private void SetTextAlpha(TMP_Text text, float alpha)
        {
            Color c = text.color;
            c.a = alpha;
            text.color = c;
        }

        private void SetBannerAlpha(float a)
        {
            Color c = taskReceivedBanner.color;
            c.a = a;
            taskReceivedBanner.color = c;
        }
    }
}
