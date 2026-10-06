using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Tasks
{
    /// <summary>
    /// 任务总控：管任务数据 + 任务面板动画。
    /// 挂在TaskDirector物体上，恒久存在。
    /// </summary>
    public class TaskDirector : MonoBehaviour
    {
        public static TaskDirector Instance { get; private set; }

        private List<Task> activeTasks = new List<Task>();

        [Header("面板动画")]
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private GameObject blackOverlay;
        [SerializeField] private float fadeDuration = 0.2f;

        private bool isOpen;
        private Coroutine fadeRoutine;
        private CanvasGroup panelCanvas;
        private CanvasGroup blackCanvas;

        private void Awake()
        {
            Instance = this;
            MessageBus.Subscribe<CloseQuestPanelMessage>(OnCloseQuestPanel);
        }

        private void OnDestroy()
        {
            MessageBus.Unsubscribe<CloseQuestPanelMessage>(OnCloseQuestPanel);
        }

        private void OnCloseQuestPanel(CloseQuestPanelMessage msg)
        {
            if (isOpen) ClosePanel();
        }

        private void Start()
        {
            if (panelRect != null)
            {
                panelCanvas = panelRect.GetComponent<CanvasGroup>();
                if (panelCanvas == null) panelCanvas = panelRect.gameObject.AddComponent<CanvasGroup>();
                panelCanvas.alpha = 0;
                panelRect.gameObject.SetActive(false);
            }
            if (blackOverlay != null)
            {
                blackCanvas = blackOverlay.GetComponent<CanvasGroup>();
                if (blackCanvas == null) blackCanvas = blackOverlay.AddComponent<CanvasGroup>();
                blackCanvas.alpha = 0;
                blackOverlay.SetActive(false);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (isOpen) ClosePanel();
                else OpenPanel();
            }
        }

        // ── 任务数据 ──

        public void AddTask(Task task)
        {
            if (task == null) return;
            activeTasks.Add(task);
            MessageBus.Publish(new TaskReceivedMessage { Task = task });
        }

        public void CompleteTask(string taskId)
        {
            Task task = activeTasks.Find(t => t.id == taskId);
            if (task == null || task.status == TaskStatus.Completed) return;
            task.status = TaskStatus.Completed;
            MessageBus.Publish(new TaskCompletedMessage { TaskId = taskId });
        }

        public List<Task> GetActiveTasks() => activeTasks;

        public Task GetTaskById(string taskId) => activeTasks.Find(t => t.id == taskId);

        // ── 面板动画（渐入渐出）──

        public void OpenPanel()
        {
            isOpen = true;
            panelRect.gameObject.SetActive(true);
            blackOverlay.SetActive(true);
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeIn());
        }

        public void ClosePanel()
        {
            isOpen = false;
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeOut());
        }

        private IEnumerator FadeIn()
        {
            panelCanvas.alpha = 0;
            blackCanvas.alpha = 0;

            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / fadeDuration);
                panelCanvas.alpha = p;
                blackCanvas.alpha = p;
                yield return null;
            }
            panelCanvas.alpha = 1;
            blackCanvas.alpha = 1;
        }

        private IEnumerator FadeOut()
        {
            panelCanvas.alpha = 1;
            blackCanvas.alpha = 1;

            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / fadeDuration);
                panelCanvas.alpha = 1 - p;
                blackCanvas.alpha = 1 - p;
                yield return null;
            }
            panelCanvas.alpha = 0;
            blackCanvas.alpha = 0;
            panelRect.gameObject.SetActive(false);
            blackOverlay.SetActive(false);
        }
    }
}
