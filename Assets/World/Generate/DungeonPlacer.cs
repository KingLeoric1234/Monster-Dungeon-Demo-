using UnityEngine;
using System.Collections.Generic;
using Game.Enemy;
using Game.Core;

namespace Game.World.Generate
{
    /// <summary>
    /// 摆放对象：根据房间类型，放出口、刷怪区，通知玩家去出生点。
    /// </summary>
    public class DungeonPlacer : MonoBehaviour
    {
        [Header("预制体（从 Project 拖）")]
        [SerializeField] private GameObject exitPortalPrefab;

        public void Place(DungeonGraph graph, int offsetX = 0, int offsetY = 0)
        {
            // 通知玩家去出生点（玩家自己订阅消息，不直接改玩家位置）
            MessageBus.Publish(new DungeonSpawnPointMessage
            {
                Position = new Vector2(graph.spawnCell.x + offsetX, graph.spawnCell.y + offsetY)
            });

            // 出口
            if (exitPortalPrefab != null)
            {
                Instantiate(exitPortalPrefab, new Vector3(graph.exitCell.x + offsetX, graph.exitCell.y + offsetY, 0), Quaternion.identity);
            }

            // 刷怪区：从地板上随机挑几个点
            int spawnAreaCount = Mathf.Max(3, graph.carved.Count / 100);
            System.Random rng = new System.Random();
            List<Vector2Int> carvedList = new List<Vector2Int>(graph.carved);

            for (int i = 0; i < spawnAreaCount; i++)
            {
                // 随机挑一个地板格子
                Vector2Int pos = carvedList[rng.Next(carvedList.Count)];

                // 别太靠近出生点和出口
                if (Vector2Int.Distance(pos, graph.spawnCell) < 10) continue;
                if (Vector2Int.Distance(pos, graph.exitCell) < 10) continue;

                GameObject spawnAreaObj = new GameObject($"SpawnArea_{pos.x}_{pos.y}");
                spawnAreaObj.transform.position = new Vector3(pos.x + offsetX, pos.y + offsetY, 0);

                // 加 SpawnArea 组件
                var spawnArea = spawnAreaObj.AddComponent<SpawnArea>();

                // 在周围建几个出生点
                for (int j = 0; j < 4; j++)
                {
                    Transform point = new GameObject($"Point_{j}").transform;
                    point.SetParent(spawnAreaObj.transform);
                    float ox = (j % 2 == 0) ? -2f : 2f;
                    float oy = (j < 2) ? -1.5f : 1.5f;
                    point.localPosition = new Vector3(ox, oy, 0);
                }
            }

            // 重建寻路网格
            GridWalkability gw = GridWalkability.Instance;
            if (gw != null) gw.Rebuild();
        }
    }
}
