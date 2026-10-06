using UnityEngine;
using Game.Tasks;

namespace Game.UI
{
    /// <summary>
    /// 任务发放总控：TaskDirector的子物体。
    /// 现在测试：按K直接发任务。
    /// </summary>
    public class QuestGiver : MonoBehaviour
    {
        private int taskIndex;
        private readonly System.Func<Task>[] taskPool = new System.Func<Task>[]
        {
            TaskConfig.CreateTalkToNpc,
            TaskConfig.CreateKillSlime,
            TaskConfig.CreateCollectKey,
            TaskConfig.CreateKillBomber,
            TaskConfig.CreateFindTreasure,
            TaskConfig.CreateRescueVillager,
            TaskConfig.CreateDefeatBoss,
            TaskConfig.CreateCollectHerb
        };

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K) && taskIndex < taskPool.Length)
            {
                GiveTask(taskPool[taskIndex]());
                taskIndex++;
            }
        }

        public void GiveTask(Task task)
        {
            if (task == null || TaskDirector.Instance == null) return;

            Task existing = TaskDirector.Instance.GetTaskById(task.id);
            if (existing != null && existing.status != TaskStatus.Completed)
            {
                Debug.Log($"[QuestGiver] 任务已存在: {task.id}");
                return;
            }

            TaskDirector.Instance.AddTask(task);
            Debug.Log($"[QuestGiver] 发放任务: {task.id}");
        }
    }
}
