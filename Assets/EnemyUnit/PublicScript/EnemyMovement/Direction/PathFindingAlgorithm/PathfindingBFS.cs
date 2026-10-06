using UnityEngine;
using System.Collections.Generic;

namespace Game.Enemy
{
    /// <summary>
    /// BFS距离场：从玩家位置向外扩散，记录每个格子到玩家的最短步数。
    /// 怪物查距离场决定走哪个方向。
    /// </summary>
    public class PathfindingBFS : MonoBehaviour
    {
        public static PathfindingBFS Instance { get; private set; }

        [Header("参数")]
        [SerializeField] private float refreshInterval = BFS_Arguments.PathfindingBFS.RefreshInterval;

        private GridWalkability grid;
        private int[,] distance;
        private int width, height;
        private Vector2Int playerGrid;
        private bool hasField;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            grid = GridWalkability.Instance;
            if (grid == null)
            {
                Debug.LogError("[PathfindingBFS] 找不到GridWalkability，检查是否挂在同一物体上");
                return;
            }
            width = grid.Width;
            height = grid.Height;
            distance = new int[width, height];
            InvokeRepeating(nameof(ComputeField), BFS_Arguments.PathfindingBFS.InitialDelay, refreshInterval);
        }

        private void ComputeField()
        {
            if (grid == null) return;
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null) return;
            Transform player = playerObj.transform;
            if (player == null) return;

            playerGrid = grid.WorldToGrid(player.position);
            int radius = grid.SearchRadius;

            // 全部标记为不可达(-1)
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    distance[x, y] = -1;

            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            if (grid.IsWalkable(playerGrid.x, playerGrid.y))
            {
                distance[playerGrid.x, playerGrid.y] = 0;
                queue.Enqueue(playerGrid);
            }

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            while (queue.Count > 0)
            {
                Vector2Int cur = queue.Dequeue();
                int d = distance[cur.x, cur.y];

                if (d >= radius) continue;

                for (int i = 0; i < 4; i++)
                {
                    int nx = cur.x + dx[i];
                    int ny = cur.y + dy[i];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    if (!grid.IsWalkable(nx, ny)) continue;
                    if (distance[nx, ny] != -1) continue;

                    distance[nx, ny] = d + 1;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }

            hasField = true;
        }

        /// <summary>查某格到玩家的距离，-1=不可达</summary>
        public int GetDistance(int gx, int gy)
        {
            if (gx < 0 || gx >= width || gy < 0 || gy >= height) return -1;
            return distance[gx, gy];
        }

        public bool HasField => hasField;
    }
}
