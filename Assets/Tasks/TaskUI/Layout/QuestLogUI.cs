using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core;
using Game.Tasks;
using System.Collections.Generic;

namespace Game.UI
{
    /// <summary>
    /// 任务日志面板：左边任务列表，右边详情。
    /// </summary>
    public class QuestLogUI : MonoBehaviour
    {
        [Header("左边任务列表")]
        [SerializeField] private Transform taskListContent;
        [SerializeField] private GameObject taskButtonPrefab;
        [SerializeField] private Game.Tasks.TaskListAutoLayout listLayout;
        [SerializeField] private float buttonHeight = 40f;
        [SerializeField] private float buttonSpacing = 10f;

        [Header("右边详情")]
        [SerializeField] private TMP_Text detailName;
        [SerializeField] private TMP_Text detailTarget;
        [SerializeField] private TMP_Text detailDescription;
        [SerializeField] private TMP_Text detailGiver;
        [SerializeField] private TMP_Text detailReward;
        [SerializeField] private QuestLogAutoLayout rightLayout;

        [Header("空状态")]
        [SerializeField] private TMP_Text leftEmptyText;
        [SerializeField] private TMP_Text rightEmptyText;

        private Dictionary<string, Button> buttons = new Dictionary<string, Button>();

        private void OnEnable()
        {
            MessageBus.Subscribe<TaskReceivedMessage>(OnTaskReceived);
            MessageBus.Subscribe<TaskCompletedMessage>(OnTaskCompleted);
            SetupLeftLayout();
            RefreshList();
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<TaskReceivedMessage>(OnTaskReceived);
            MessageBus.Unsubscribe<TaskCompletedMessage>(OnTaskCompleted);
        }

        private void OnTaskReceived(TaskReceivedMessage msg) => RefreshList();
        private void OnTaskCompleted(TaskCompletedMessage msg) => RefreshList();

        private void SetupLeftLayout()
        {
            var vlg = taskListContent.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = taskListContent.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = buttonSpacing;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var fitter = taskListContent.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = taskListContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        private void RefreshList()
        {
            foreach (var btn in buttons.Values) Destroy(btn.gameObject);
            buttons.Clear();

            if (TaskDirector.Instance == null) return;

            foreach (Task task in TaskDirector.Instance.GetActiveTasks())
            {
                if (task.status != TaskStatus.Active) continue;
                GameObject go = Instantiate(taskButtonPrefab, taskListContent);
                Button btn = go.GetComponent<Button>();
                TMP_Text txt = go.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.text = task.taskName;
                    txt.alignment = TextAlignmentOptions.Center;
                }

                Task captured = task;
                btn.onClick.AddListener(() => ShowDetail(captured));
                buttons[task.id] = btn;
            }

            if (buttons.Count > 0)
            {
                ShowDetail(TaskDirector.Instance.GetActiveTasks()[0]);
                if (leftEmptyText != null) leftEmptyText.gameObject.SetActive(false);
                if (rightEmptyText != null) rightEmptyText.gameObject.SetActive(false);
            }
            else
            {
                ClearDetail();
                if (leftEmptyText != null) leftEmptyText.gameObject.SetActive(true);
                if (rightEmptyText != null) rightEmptyText.gameObject.SetActive(true);
            }
            listLayout?.RefreshLayout();
        }

        private void ShowDetail(Task task)
        {
            detailName.text = task.taskName;
            detailTarget.text = task.targetText;
            detailDescription.text = task.description;
            detailGiver.text = "From: " + task.giverName;
            detailReward.text = "Reward: " + task.reward;
            rightLayout?.RefreshLayout();
        }

        private void ClearDetail()
        {
            detailName.text = detailTarget.text = detailDescription.text = detailGiver.text = detailReward.text = "";
        }
    }
}
