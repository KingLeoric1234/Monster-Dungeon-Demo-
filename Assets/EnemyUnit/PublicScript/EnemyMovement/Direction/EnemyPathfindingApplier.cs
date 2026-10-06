using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// 寻路应用（执行层）：链接寻路算法并 Apply。
    /// 追击时查 BFS 距离场拿最短路方向；不可达（场不可用/已到玩家）时直追 + 偏移预判，防止走直线。
    /// 产出"未加排斥"的方向，交给方向计算层统一合成归一化。
    /// </summary>
    public class EnemyPathfindingApplier
    {
        /// <summary>
        /// 追击方向：寻路可达时返回单位方向；不可达时返回（直追 + 偏移）的原始和（未归一化，
        /// 由方向计算层和排斥一起合成）。</summary>
        public Vector2 GetChaseDirection(Transform self, Transform player, MonsterTemplate template, EnemyMovementData data)
        {
            Vector2 pathDir = GetPathfindingDir(self);
            if (pathDir != Vector2.zero) return pathDir;

            // 寻路不可达：直追 + 偏移预判（防止走直线）
            data.offsetTimer += Time.deltaTime;
            if (data.offsetTimer >= data.nextOffsetTime)
                RandomizeOffset(data, template);

            Vector2 fallback = ((Vector2)player.position - (Vector2)self.position).normalized;
            return fallback + data.currentOffset;
        }

        /// <summary>查距离场，返回走向玩家的最短路方向，不可达返回zero</summary>
        private Vector2 GetPathfindingDir(Transform self)
        {
            if (PathfindingBFS.Instance == null || !PathfindingBFS.Instance.HasField) return Vector2.zero;
            if (GridWalkability.Instance == null) return Vector2.zero;

            Vector2Int g = GridWalkability.Instance.WorldToGrid(self.position);
            int curD = PathfindingBFS.Instance.GetDistance(g.x, g.y);
            if (curD <= 0) return Vector2.zero;   // 已到玩家或不可达

            int bestDirX = 0, bestDirY = 0;
            int bestD = curD;

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };
            for (int i = 0; i < 4; i++)
            {
                int nx = g.x + dx[i];
                int ny = g.y + dy[i];
                int d = PathfindingBFS.Instance.GetDistance(nx, ny);
                if (d >= 0 && d < bestD)
                {
                    bestD = d;
                    bestDirX = dx[i];
                    bestDirY = dy[i];
                }
            }

            if (bestDirX == 0 && bestDirY == 0) return Vector2.zero;
            return new Vector2(bestDirX, bestDirY).normalized;
        }

        /// <summary>随机化偏移预判（让怪物移动更智能，不直直追）。Init 首启 + 寻路失效时周期性触发。</summary>
        public void RandomizeOffset(EnemyMovementData data, MonsterTemplate template)
        {
            data.offsetTimer = 0;
            data.nextOffsetTime = Random.Range(template.minOffsetInterval, template.maxOffsetInterval);
            data.currentOffset = new Vector2(
                Random.Range(-template.maxOffset, template.maxOffset),
                Random.Range(-template.maxOffset, template.maxOffset));
        }
    }
}
