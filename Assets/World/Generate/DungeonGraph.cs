using System.Collections.Generic;
using UnityEngine;

namespace Game.World.Generate
{
    public class Corridor
    {
        public List<Vector2Int> cells = new List<Vector2Int>();
    }

    /// <summary>
    /// 抽象地图：元胞自动机生成溶洞。
    /// 新流程：CA 挖洞 → 只留最大洞 → 洞里挑出生/出口 → 撒障碍（不堵路）→ 兜底。
    /// 连通性由「主洞本身连通 + 障碍撒完校验」双重保证，不再需要 A* 凿路。
    /// </summary>
    public class DungeonGraph
    {
        public List<Corridor> corridors = new List<Corridor>();
        public HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>(); // 随机障碍
        public Vector2Int spawnCell;
        public Vector2Int exitCell;

        private int mapWidth, mapHeight;
        private int randomness; // 随机性大小
        private System.Random rng;
        public HashSet<Vector2Int> carved = new HashSet<Vector2Int>(); // 已挖过的格子（地板）

        public DungeonGraph(int mapWidth, int mapHeight, int seed, int randomness = 3)
        {
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            this.randomness = randomness;
            rng = new System.Random(seed == 0 ? System.Environment.TickCount : seed);
        }

        public void Generate()
        {
            // 第一步：元胞自动机生成整个溶洞（不依赖出生点/出口位置）
            RunCellularAutomata();

            // 第二步：只保留最大的连通洞块，小碎洞填成墙（保证地图是一整块、出口不会落在碎洞里）
            KeepLargestCave();

            // 第三步：洞挖好后，在洞里挑出生点和出口（出生点跟着洞走）
            ChooseSpawnAndExit();

            // 第四步：撒随机障碍（不贴砖，先记下来位置）
            SpawnRandomObstacles();

            // 第五步：确保出生点和出口一定是地板（兜底）
            carved.Add(spawnCell);
            carved.Add(exitCell);
        }

        /// <summary>洞挖好后，在洞里挑出生点和出口（出生点跟着洞走，出口取洞内最远）</summary>
        private void ChooseSpawnAndExit()
        {
            // 出生点：优先挑"上下左右都是地板"的格子，保证出生不卡墙
            List<Vector2Int> candidates = new List<Vector2Int>();
            foreach (var cell in carved)
            {
                if (CountOpenDirs(cell) == 4) candidates.Add(cell);
            }
            // 万一洞里没有 4 向全空的格子，退一步：洞里任意格子
            if (candidates.Count == 0) candidates.AddRange(carved);

            spawnCell = candidates[rng.Next(candidates.Count)];

            // 出口：洞里离出生点最远、且周围开阔（8 邻至少 6 个可走）的格子，
            // 避免出口落在洞的尖角/凹角里看起来像嵌在墙中
            Vector2Int best = spawnCell;
            int bestDist = -1;
            foreach (var cell in carved)
            {
                if (CountOpen8Dirs(cell) < 6) continue;
                int d = (int)Vector2Int.Distance(cell, spawnCell);
                if (d > bestDist)
                {
                    bestDist = d;
                    best = cell;
                }
            }

            // 万一洞里没有 8 邻开阔的格子（几乎不会发生），退回任意最远
            if (bestDist == -1)
            {
                foreach (var cell in carved)
                {
                    int d = (int)Vector2Int.Distance(cell, spawnCell);
                    if (d > bestDist)
                    {
                        bestDist = d;
                        best = cell;
                    }
                }
            }
            exitCell = best;
        }

        /// <summary>只保留最大的连通洞块，小碎洞填成墙（保证地图是一整块、出口不会落在碎洞里）</summary>
        private void KeepLargestCave()
        {
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            HashSet<Vector2Int> largest = new HashSet<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            foreach (var start in carved)
            {
                if (visited.Contains(start)) continue;

                // 从 start 扩散出当前这一整块
                HashSet<Vector2Int> comp = new HashSet<Vector2Int>();
                queue.Clear();
                queue.Enqueue(start);
                visited.Add(start);
                comp.Add(start);

                while (queue.Count > 0)
                {
                    Vector2Int cur = queue.Dequeue();
                    foreach (var nb in FourNeighbors(cur))
                    {
                        if (carved.Contains(nb) && !visited.Contains(nb))
                        {
                            visited.Add(nb);
                            comp.Add(nb);
                            queue.Enqueue(nb);
                        }
                    }
                }

                if (comp.Count > largest.Count) largest = comp;
            }

            // 只留最大块，其余（碎洞）变成墙
            carved.Clear();
            carved.UnionWith(largest);
        }

        private static readonly Vector2Int[] DIRS4 =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        private IEnumerable<Vector2Int> FourNeighbors(Vector2Int pos)
        {
            foreach (var d in DIRS4)
                yield return new Vector2Int(pos.x + d.x, pos.y + d.y);
        }

        /// <summary>数这个格子上下左右有几个是地板</summary>
        private int CountOpenDirs(Vector2Int pos)
        {
            int count = 0;
            foreach (var d in DIRS4)
                if (carved.Contains(new Vector2Int(pos.x + d.x, pos.y + d.y))) count++;
            return count;
        }

        /// <summary>数这个格子周围 8 个方向有几个是地板（用来判断开阔度）</summary>
        private int CountOpen8Dirs(Vector2Int pos)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    if (carved.Contains(new Vector2Int(pos.x + dx, pos.y + dy))) count++;
                }
            }
            return count;
        }

        /// <summary>元胞自动机：随机噪声 + 演化，生成整个溶洞</summary>
        private void RunCellularAutomata()
        {
            bool[,] map = new bool[mapWidth, mapHeight];

            // 随机初始化：48% 是地板
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    // 地图边缘一圈是墙
                    if (x < 2 || x >= mapWidth - 2 || y < 2 || y >= mapHeight - 2)
                    {
                        map[x, y] = false;
                    }
                    else
                    {
                        map[x, y] = rng.NextDouble() < 0.48;
                    }
                }
            }

            // 演化 4 次（出生点/出口在洞挖好后由 ChooseSpawnAndExit 挑选，这里不依赖）
            for (int iter = 0; iter < 4; iter++)
            {
                bool[,] newMap = (bool[,])map.Clone();

                for (int x = 1; x < mapWidth - 1; x++)
                {
                    for (int y = 1; y < mapHeight - 1; y++)
                    {
                        int wallCount = CountSurroundingWalls(map, x, y);

                        if (wallCount >= 5)
                            newMap[x, y] = false;  // 墙多 → 变成墙
                        else if (wallCount <= 3)
                            newMap[x, y] = true;   // 墙少 → 变成地板
                        // =4 不变
                    }
                }

                map = newMap;
            }

            // 把演化结果加进 carved：CA 的洞（map=true）就是地板，不再取反
            carved.Clear();
            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                    if (map[x, y]) // 洞 = 地板
                        carved.Add(new Vector2Int(x, y));
        }

        private int CountSurroundingWalls(bool[,] map, int cx, int cy)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int x = cx + dx;
                    int y = cy + dy;
                    if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                    {
                        count++; // 边界外算墙
                    }
                    else if (!map[x, y])
                    {
                        count++; // 不是地板就是墙
                    }
                }
            }
            return count;
        }

        /// <summary>撒随机障碍：在地板上放障碍，撒完校验连通，不连通就减半重撒</summary>
        private void SpawnRandomObstacles()
        {
            int obstacleCount = carved.Count / 20; // 5% 的地板放障碍

            for (int attempt = 0; attempt < 10; attempt++)
            {
                obstacles.Clear();
                for (int i = 0; i < obstacleCount; i++)
                {
                    Vector2Int pos = GetRandomCarved();

                    // 不能挡在出生点和出口周围 8 邻（保证传送门/出生点四周是干净地板）
                    if (Mathf.Abs(pos.x - spawnCell.x) <= 1 && Mathf.Abs(pos.y - spawnCell.y) <= 1) continue;
                    if (Mathf.Abs(pos.x - exitCell.x) <= 1 && Mathf.Abs(pos.y - exitCell.y) <= 1) continue;

                    // 只在开阔处放（至少 3 个方向能走），不堵 1 格宽走廊
                    if (CountWalkableDirections(pos) < 3) continue;

                    obstacles.Add(pos);
                }

                // 撒完校验：出生点到出口必须仍连通，否则障碍减半再撒
                if (IsSpawnToExitConnected()) return;
                obstacleCount /= 2;
            }

            // 兜底：多次重撒仍不连通就干脆不放障碍，保证地图一定能走
            obstacles.Clear();
        }

        /// <summary>校验：出生点到出口在现有障碍下是否仍连通</summary>
        private bool IsSpawnToExitConnected()
        {
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(spawnCell);
            visited.Add(spawnCell);

            while (queue.Count > 0)
            {
                Vector2Int cur = queue.Dequeue();
                if (cur == exitCell) return true;

                foreach (var nb in FourNeighbors(cur))
                {
                    if (carved.Contains(nb) && !obstacles.Contains(nb) && !visited.Contains(nb))
                    {
                        visited.Add(nb);
                        queue.Enqueue(nb);
                    }
                }
            }
            return false;
        }

        private int CountWalkableDirections(Vector2Int pos)
        {
            int count = 0;
            if (IsWalkable(new Vector2Int(pos.x + 1, pos.y))) count++;
            if (IsWalkable(new Vector2Int(pos.x - 1, pos.y))) count++;
            if (IsWalkable(new Vector2Int(pos.x, pos.y + 1))) count++;
            if (IsWalkable(new Vector2Int(pos.x, pos.y - 1))) count++;
            return count;
        }

        private bool IsWalkable(Vector2Int pos)
        {
            return carved.Contains(pos) && !obstacles.Contains(pos);
        }

        private Vector2Int GetRandomCarved()
        {
            int idx = rng.Next(carved.Count);
            int i = 0;
            foreach (var cell in carved)
            {
                if (i == idx) return cell;
                i++;
            }
            return spawnCell;
        }
    }
}
