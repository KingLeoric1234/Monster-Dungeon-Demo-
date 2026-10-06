using UnityEngine;
using System.Collections.Generic;

namespace Game.World.Generate
{
    /// <summary>
    /// 栅格化：把抽象图（房间+走廊）填成逐格的可走性表。
    /// 纯逻辑，不依赖 Unity 渲染。
    /// </summary>
    public class DungeonRasterizer
    {
        private bool[,] walkable;
        private int mapWidth, mapHeight;

        public bool[,] Walkable => walkable;

        public DungeonRasterizer(int mapWidth, int mapHeight)
        {
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            walkable = new bool[mapWidth, mapHeight];
        }

        public void Rasterize(DungeonGraph graph)
        {
            // 走廊走过的格子（A* 路径）
            foreach (var corridor in graph.corridors)
                foreach (var cell in corridor.cells)
                    Set(cell.x, cell.y);

            // 元胞自动机挖出来的洞穴格子（carved 集合）
            foreach (var cell in graph.carved)
                Set(cell.x, cell.y);

            // 障碍格：取消可走性
            foreach (var obs in graph.obstacles)
            {
                if (obs.x >= 0 && obs.x < mapWidth && obs.y >= 0 && obs.y < mapHeight)
                    walkable[obs.x, obs.y] = false;
            }
        }

        private void Set(int x, int y)
        {
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                walkable[x, y] = true;
        }

        /// <summary>BFS 校验：出生点能不能到达出口</summary>
        public bool ValidateConnectivity(Vector2Int spawn, Vector2Int exit)
        {
            bool[,] visited = new bool[mapWidth, mapHeight];
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(spawn);
            visited[spawn.x, spawn.y] = true;

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            while (queue.Count > 0)
            {
                Vector2Int cur = queue.Dequeue();
                if (cur == exit) return true;

                for (int i = 0; i < 4; i++)
                {
                    int nx = cur.x + dx[i];
                    int ny = cur.y + dy[i];
                    if (nx < 0 || nx >= mapWidth || ny < 0 || ny >= mapHeight) continue;
                    if (!walkable[nx, ny] || visited[nx, ny]) continue;
                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
            return false;
        }
    }
}
