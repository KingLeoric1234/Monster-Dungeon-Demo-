using Game.Core;

namespace Game.World
{
    /// <summary>交互提示：显示/隐藏对话框</summary>
    public class InteractPromptMessage : GameMessage
    {
        public bool Show;          // true=显示 false=隐藏
        public string PromptText;  // 提示文本（如"进入地牢？"）
        public Interactable Target; // 当前交互物块（确认时用）
    }

    /// <summary>交互确认：玩家点了确认</summary>
    public class InteractConfirmMessage : GameMessage
    {
        public Interactable Target; // 被确认的物块
    }

    /// <summary>请求进入地牢（物块发出，WorldDirector处理）</summary>
    public class EnterDungeonRequestMessage : GameMessage
    {
        public int Layer;  // 要进入的层
    }

    /// <summary>请求回主世界（WorldDirector处理）</summary>
    public class LoadWorldRequestMessage : GameMessage
    {
    }

    /// <summary>地牢生成完毕，通知玩家去出生点</summary>
    public class DungeonSpawnPointMessage : GameMessage
    {
        public UnityEngine.Vector2 Position;  // 出生点世界坐标
    }

    /// <summary>地牢生成完毕（触发状态机切换）</summary>
    public class DungeonGeneratedMessage : GameMessage
    {
        public int Layer;  // 第几层
    }
}