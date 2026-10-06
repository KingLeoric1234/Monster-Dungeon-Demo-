using UnityEngine;
using System;

namespace Game.World.Generate
{
    /// <summary>
    /// 地牢生成编排器：按顺序协调 Graph → Rasterizer → Painter → Placer。
    /// 由 WorldDirector 在场景加载后调用 Generate(layer)。
    /// </summary>
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("地图尺寸（格，正方形）")]
        [SerializeField] private int mapSize = 80;

        [Header("种子（0 = 随机；同种子 = 同一张图）")]
        [SerializeField] private int seed = 0;

        [Header("A* 随机性（越大路越弯）")]
        [Range(0, 10)]
        [SerializeField] private int pathRandomness = 3;

        [Header("地图偏移（正=右/上，负=左/下）")]
        [SerializeField] private int offsetX = 0;
        [SerializeField] private int offsetY = 0;

        private DungeonPainter painter;
        private DungeonPlacer placer;

        private void Awake()
        {
            painter = GetComponent<DungeonPainter>();
            placer = GetComponent<DungeonPlacer>();
        }

        /// <summary>由 WorldDirector 在场景加载后调用。主世界里不会被调用。</summary>
        public void Generate(int layer)
        {
            var rng = new System.Random(seed == 0 ? Environment.TickCount : seed);

            // 偏移量限制：不能超过地图的一半，防止跑出地图外
            int clampedOffsetX = Mathf.Clamp(offsetX, -mapSize / 4, mapSize / 4);
            int clampedOffsetY = Mathf.Clamp(offsetY, -mapSize / 4, mapSize / 4);

            // 1. 生成抽象图（元胞自动机 + A* 找路）
            DungeonGraph graph = new DungeonGraph(mapSize, mapSize, seed, pathRandomness);
            graph.Generate();

            // 2. 栅格化 + BFS 连通性校验
            DungeonRasterizer rasterizer = new DungeonRasterizer(mapSize, mapSize);
            rasterizer.Rasterize(graph);
            bool connected = rasterizer.ValidateConnectivity(graph.spawnCell, graph.exitCell);
            if (!connected)
                Debug.LogError("[DungeonGenerator] 连通性校验失败：出生点到出口不可达！");

            // 3. 贴瓷砖（带偏移）
            painter.Paint(rasterizer.Walkable, mapSize, mapSize, rng, clampedOffsetX, clampedOffsetY, graph.obstacles);

            // 4. 摆玩家/出口/刷怪区（带偏移）
            placer.Place(graph, clampedOffsetX, clampedOffsetY);

            // 发消息：地牢生成完毕
            Game.Core.MessageBus.Publish(new DungeonGeneratedMessage { Layer = layer });
        }
    }
}
