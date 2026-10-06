using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace Game.Enemy
{
    /// <summary>
    /// 网格可走性表：扫描Tilemap，记录每个格子是否可走。
    /// 挂在有Tilemap的物体上，Awake自动扫描。
    /// </summary>
    public class GridWalkability : MonoBehaviour
    {
        public static GridWalkability Instance { get; private set; }

        [Header("引用")]
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Tilemap obstacleTilemap;

        [Header("参数")]
        [SerializeField] private int gridSize = BFS_Arguments.GridWalkability.GridSize;
        [SerializeField] private int searchRadius = BFS_Arguments.GridWalkability.SearchRadius;

        private bool[,] walkable;
        private int width, height;
        private int minX, minY;

        private void Awake()
        {
            Instance = this;
            BuildGrid();
        }

        /// <summary>生成器画完 Tilemap 后调用，重建可行走网格</summary>
        public void Rebuild()
        {
            BuildGrid();
        }

        private void BuildGrid()
        {
            BoundsInt bounds = obstacleTilemap.cellBounds;
            minX = bounds.xMin;
            minY = bounds.yMin;
            width = bounds.size.x;
            height = bounds.size.y;

            walkable = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int wx = minX + x;
                    int wy = minY + y;
                    walkable[x, y] = obstacleTilemap.GetTile(new Vector3Int(wx, wy, 0)) == null;
                }
            }

            //Debug.Log($"[GridWalkability] 网格: {width}x{height}, 起点({minX},{minY})");
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int gx = Mathf.FloorToInt(worldPos.x / gridSize) - minX;
            int gy = Mathf.FloorToInt(worldPos.y / gridSize) - minY;
            return new Vector2Int(gx, gy);
        }

        public bool IsWalkable(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return false;
            return walkable[x, y];
        }

        public int Width => width;
        public int Height => height;
        public int SearchRadius => searchRadius;
    }
}
