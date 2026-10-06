using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace Game.World.Generate
{
    /// <summary>
    /// 贴瓷砖：拿着 bool[,] walkable，往 Tilemap 上画地砖和墙。
    /// </summary>
    public class DungeonPainter : MonoBehaviour
    {
        [Header("瓷砖素材（拖 Project 里的 TileBase）")]
        [SerializeField] private TileBase floorTile;
        [SerializeField] private TileBase floorTileVariant; // 可选，30%概率随机贴
        [SerializeField] private TileBase obstacleTile; // 随机障碍（石头/箱子）

        [Header("直墙（4块）")]
        [SerializeField] private TileBase wallUp;      // 上墙砖（视觉在下边）
        [SerializeField] private TileBase wallDown;    // 下墙砖（视觉在上边）
        [SerializeField] private TileBase wallLeft;    // 左墙砖（视觉在右边）
        [SerializeField] private TileBase wallRight;   // 右墙砖（视觉在左边）

        [Header("转角（8块，4方向×大小）")]
        [SerializeField] private TileBase cornerTL_Small;  // 左上小转角
        [SerializeField] private TileBase cornerTL_Big;    // 左上大转角
        [SerializeField] private TileBase cornerTR_Small;  // 右上小转角
        [SerializeField] private TileBase cornerTR_Big;    // 右上大转角
        [SerializeField] private TileBase cornerBL_Small;  // 左下小转角
        [SerializeField] private TileBase cornerBL_Big;    // 左下大转角
        [SerializeField] private TileBase cornerBR_Small;  // 右下小转角
        [SerializeField] private TileBase cornerBR_Big;    // 右下大转角

        private Tilemap groundTilemap;
        private Tilemap obstacleTilemap;

        public void Paint(bool[,] walkable, int mapWidth, int mapHeight, System.Random rng, int offsetX = 0, int offsetY = 0, HashSet<Vector2Int> obstacles = null)
        {
            // 第一次 Paint 时才找 Tilemap（主世界跑 Awake 时不找）
            if (groundTilemap == null || obstacleTilemap == null)
            {
                var allTilemaps = FindObjectsOfType<Tilemap>(includeInactive: true);
                // Debug.Log($"[DungeonPainter] 场景里共找到 {allTilemaps.Length} 个 Tilemap：");
                foreach (var tm in allTilemaps)
                {
                    // Debug.Log($"  → {tm.name}");
                    if (tm.name == "Ground") groundTilemap = tm;
                    if (tm.name == "Obstacle") obstacleTilemap = tm;
                }

                if (groundTilemap == null)
                    Debug.LogError("[DungeonPainter] 找不到名为 Ground 的 Tilemap！");
                if (obstacleTilemap == null)
                    Debug.LogError("[DungeonPainter] 找不到名为 Obstacle 的 Tilemap！");
            }

            groundTilemap.ClearAllTiles();
            obstacleTilemap.ClearAllTiles();

            // 生成边界掩码：只有内点和边界点才贴砖，外面什么都不贴
            bool[,] boundaryMask = CreateBoundaryMask(walkable, mapWidth, mapHeight);

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Vector3Int cell = new Vector3Int(x + offsetX, y + offsetY, 0);

                    // 外面什么都不贴
                    if (!boundaryMask[x, y])
                    {
                        groundTilemap.SetTile(cell, null);
                        obstacleTilemap.SetTile(cell, null);
                        continue;
                    }

                    bool isFloor = walkable[x, y];
                    bool isObstacle = obstacles != null && obstacles.Contains(new Vector2Int(x, y));

                    if (isFloor)
                    {
                        TileBase tile = (floorTileVariant != null && rng.NextDouble() < 0.3)
                            ? floorTileVariant : floorTile;
                        groundTilemap.SetTile(cell, tile);
                        obstacleTilemap.SetTile(cell, null);
                    }
                    else if (isObstacle && obstacleTile != null)
                    {
                        // 随机障碍（石头/箱子）
                        groundTilemap.SetTile(cell, floorTile); // 下面还是地板
                        obstacleTilemap.SetTile(cell, obstacleTile); // 上面放障碍
                    }
                    else
                    {
                        // 洞穴墙：自动选合适的墙砖
                        TileBase wallTile = PickWallTile(walkable, x, y, mapWidth, mapHeight);
                        groundTilemap.SetTile(cell, null);
                        obstacleTilemap.SetTile(cell, wallTile);
                    }
                }
            }

            // 设置碰撞体：Ground 不要碰撞体，Obstacle 用 CompositeCollider
            SetupColliders();
        }

        private void SetupColliders()
        {
            // 1. Ground Tilemap：删掉所有碰撞体（地面是走的，不是挡的）
            var groundColliders = groundTilemap.GetComponents<Collider2D>();
            foreach (var col in groundColliders)
            {
                DestroyImmediate(col);
            }

            // 2. Obstacle Tilemap：设置 CompositeCollider + 静态 Rigidbody
            // 先删掉旧的碰撞体
            var obsColliders = obstacleTilemap.GetComponents<Collider2D>();
            foreach (var col in obsColliders)
            {
                DestroyImmediate(col);
            }

            // 加 TilemapCollider2D，勾选 Used By Composite
            var tilemapCol = obstacleTilemap.gameObject.AddComponent<TilemapCollider2D>();
            tilemapCol.usedByComposite = true;

            // 加 CompositeCollider2D
            var compositeCol = obstacleTilemap.gameObject.AddComponent<CompositeCollider2D>();
            compositeCol.generationType = CompositeCollider2D.GenerationType.Manual;

            // 加静态 Rigidbody2D
            var rb = obstacleTilemap.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = obstacleTilemap.gameObject.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Static;
            rb.simulated = true;

            // 手动生成碰撞体
            compositeCol.GenerateGeometry();

            // Debug.Log("[DungeonPainter] 碰撞体设置完毕");
        }

        /// <summary>
        /// 生成边界掩码：只有内点和边界点才贴砖，外面什么都不贴。
        /// </summary>
        private bool[,] CreateBoundaryMask(bool[,] walkable, int mapWidth, int mapHeight)
        {
            bool[,] mask = new bool[mapWidth, mapHeight];

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    // 是地板 → 内点，贴砖
                    if (walkable[x, y])
                    {
                        mask[x, y] = true;
                        continue;
                    }

                    // 是墙，但旁边挨着地板 → 边界点，贴砖
                    if (IsAdjacentToFloor(walkable, x, y, mapWidth, mapHeight))
                    {
                        mask[x, y] = true;
                        continue;
                    }

                    // 外面 → 什么都不贴
                    mask[x, y] = false;
                }
            }

            return mask;
        }

        private bool IsAdjacentToFloor(bool[,] walkable, int x, int y, int mapWidth, int mapHeight)
        {
            // 查8个方向（上下左右 + 四个对角）
            if (IsFloor(walkable, x + 1, y, mapWidth, mapHeight)) return true;
            if (IsFloor(walkable, x - 1, y, mapWidth, mapHeight)) return true;
            if (IsFloor(walkable, x, y + 1, mapWidth, mapHeight)) return true;
            if (IsFloor(walkable, x, y - 1, mapWidth, mapHeight)) return true;
            if (IsFloor(walkable, x + 1, y + 1, mapWidth, mapHeight)) return true; // 右上
            if (IsFloor(walkable, x - 1, y + 1, mapWidth, mapHeight)) return true; // 左上
            if (IsFloor(walkable, x + 1, y - 1, mapWidth, mapHeight)) return true; // 右下
            if (IsFloor(walkable, x - 1, y - 1, mapWidth, mapHeight)) return true; // 左下
            return false;
        }

        /// <summary>
        /// 自动选墙砖：根据周围邻居判断该贴哪块墙。
        /// </summary>
        private TileBase PickWallTile(bool[,] walkable, int x, int y, int mapWidth, int mapHeight)
        {
            // 查四个方向的邻居是不是地板
            bool floorUp = IsFloor(walkable, x, y + 1, mapWidth, mapHeight);
            bool floorDown = IsFloor(walkable, x, y - 1, mapWidth, mapHeight);
            bool floorLeft = IsFloor(walkable, x - 1, y, mapWidth, mapHeight);
            bool floorRight = IsFloor(walkable, x + 1, y, mapWidth, mapHeight);

            // 数有几个方向是地板
            int floorCount = 0;
            if (floorUp) floorCount++;
            if (floorDown) floorCount++;
            if (floorLeft) floorCount++;
            if (floorRight) floorCount++;

            // 查对角邻居
            bool floorUL = IsFloor(walkable, x - 1, y + 1, mapWidth, mapHeight); // 左上
            bool floorUR = IsFloor(walkable, x + 1, y + 1, mapWidth, mapHeight); // 右上
            bool floorDL = IsFloor(walkable, x - 1, y - 1, mapWidth, mapHeight); // 左下
            bool floorDR = IsFloor(walkable, x + 1, y - 1, mapWidth, mapHeight); // 右下

            // ========== 直墙：只有一个方向是地板 ==========
            if (floorCount == 1)
            {
                if (floorDown) return wallUp;    // 地板在下面 → 上墙
                if (floorUp) return wallDown;    // 地板在上面 → 下墙
                if (floorRight) return wallLeft;  // 地板在右边 → 左墙
                if (floorLeft) return wallRight;  // 地板在左边 → 右墙
            }

            // ========== 两个相对方向是地板 → 直墙中间 ==========
            if (floorUp && floorDown && !floorLeft && !floorRight)
                return wallUp; // 上下都是地板，用横墙

            if (floorLeft && floorRight && !floorUp && !floorDown)
                return wallLeft; // 左右都是地板，用竖墙

            // ========== 转角：两个相邻方向是地板 ==========
            // 地板在下面 + 右边
            if (floorDown && floorRight && !floorUp && !floorLeft)
            {
                if (floorUR) return cornerBR_Big; // 凸角
                return cornerBR_Small; // 凹角
            }

            // 地板在下面 + 左边
            if (floorDown && floorLeft && !floorUp && !floorRight)
            {
                if (floorUL) return cornerBL_Big;
                return cornerBL_Small;
            }

            // 地板在上面 + 右边
            if (floorUp && floorRight && !floorDown && !floorLeft)
            {
                if (floorDR) return cornerTR_Big;
                return cornerTR_Small;
            }

            // 地板在上面 + 左边
            if (floorUp && floorLeft && !floorDown && !floorRight)
            {
                if (floorDL) return cornerTL_Big;
                return cornerTL_Small;
            }

            // ========== T型路口 / 十字：随便用个转角 ==========
            if (floorCount >= 3)
            {
                // 哪个方向没有地板，就用对应方向的转角
                if (!floorUp) return cornerTL_Small;
                if (!floorDown) return cornerBL_Small;
                if (!floorLeft) return cornerBR_Small;
                if (!floorRight) return cornerTR_Small;
            }

            // ========== 默认 ==========
            return wallUp;
        }

        private bool IsFloor(bool[,] walkable, int x, int y, int mapWidth, int mapHeight)
        {
            if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                return false; // 边界外算墙
            return walkable[x, y]; // 是地板就是地板
        }
    }
}
