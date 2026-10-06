namespace Game.Enemy
{
    /// <summary>
    /// BFS 寻路参数集中配置：所有硬编码数值统一放这里，方便维护。
    /// 每个静态嵌套类对应一个源文件，使用时引用对应类下的常量。
    /// </summary>
    public static class BFS_Arguments
    {
        /// <summary>来自 GridWalkability.cs 的参数</summary>
        public static class GridWalkability
        {
            /// <summary>网格单位大小（一个格子 = gridSize 个世界单位）</summary>
            public const int GridSize = 1;

            /// <summary>BFS 扩散半径：距离玩家超过该步数的格子视为不可达</summary>
            public const int SearchRadius = 15;
        }

        /// <summary>来自 PathfindingBFS.cs 的参数</summary>
        public static class PathfindingBFS
        {
            /// <summary>距离场刷新间隔（秒）</summary>
            public const float RefreshInterval = 0.4f;

            /// <summary>距离场首次计算的延迟（秒，InvokeRepeating 的 initialDelay）</summary>
            public const float InitialDelay = 0.1f;
        }
    }
}
