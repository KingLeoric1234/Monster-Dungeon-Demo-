namespace Game.Tasks
{
    /// <summary>任务类型（可扩展）</summary>
    public enum TaskType
    {
        TalkToNPC,
        PickupItem,
        KillEnemy
    }

    /// <summary>任务状态</summary>
    public enum TaskStatus
    {
        Active,
        Completed
    }

    /// <summary>任务数据</summary>
    [System.Serializable]
    public class Task
    {
        public string id;
        public TaskType type;
        public TaskStatus status;

        public string taskName;       // 短标题（左边按钮显示）
        public string description;    // 详细描述（右边详情）
        public string targetText;     // 目标描述
        public string giverName;      // 任务来源NPC
        public string reward;         // 奖励描述

        public string targetId;
        public int targetCount;
    }
}
