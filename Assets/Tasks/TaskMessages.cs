using Game.Core;

namespace Game.Tasks
{
    /// <summary>任务接收消息</summary>
    public class TaskReceivedMessage : GameMessage
    {
        public Task Task;
    }

    /// <summary>任务完成消息</summary>
    public class TaskCompletedMessage : GameMessage
    {
        public string TaskId;
    }
}
