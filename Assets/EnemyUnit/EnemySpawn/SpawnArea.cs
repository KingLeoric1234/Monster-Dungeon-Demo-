using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Enemy 芯片的引脚（原 SpawnArea 覆写，名字保留以接住场景引用）。
    /// 接收三根光缆：谁？在哪？几只？→ 释放：在图上生成怪物。
    /// 预制体不手动拖——按老路径自动找：Assets/Resources/EnemyPrefab 1/<家族>/<原名>.prefab
    /// </summary>
    public class SpawnArea : MonoBehaviour
    {
        /// <summary>引脚·“谁”：怪物类型（对应 EnemyPrefab 1 下的老预制体）</summary>
        public enum EnemyType
        {
            CollisionNormal,   // 碰撞·普通
            CollisionElite,    // 碰撞·精英
            ChargerNormal,     // 冲锋·普通
            ChargerElite,      // 冲锋·精英
            BomberNormal,      // 自爆·普通
            BomberElite,       // 自爆·精英
            CellNormal,        // 细胞·普通
            CellElite          // 细胞·精英
        }

        /// <summary>
        /// 引脚唯一方法：谁？在哪？几只？→ 在图上生成怪物。
        /// 生成完就撒手，之后怪物自己跑，芯片不再管它。
        /// </summary>
        public void Spawn(EnemyType who, Vector2 position, int count)
        {
            GameObject prefab = GetPrefab(who);
            if (prefab == null) return;   // 文件夹里没找到这根线，有订单也不接

            for (int i = 0; i < count; i++)
            {
                Instantiate(prefab, position, Quaternion.identity);
            }
        }

        /// <summary>去老文件夹找预制体（不手动拖，按老路径访问）</summary>
        private GameObject GetPrefab(EnemyType who)
        {
            string path = GetPrefabPath(who);
            return string.IsNullOrEmpty(path) ? null : Resources.Load<GameObject>(path);
        }

        /// <summary>枚举 → 老文件夹路径（EnemyPrefab 1 下，名字结构原封不动）</summary>
        private string GetPrefabPath(EnemyType who)
        {
            switch (who)
            {
                case EnemyType.CollisionNormal: return "EnemyPrefab 1/HitMonster/NormalEnemy";
                case EnemyType.CollisionElite:  return "EnemyPrefab 1/HitMonster/EliteEnemy";
                case EnemyType.ChargerNormal:   return "EnemyPrefab 1/ChargingMonster/NormalChargingMonster";
                case EnemyType.ChargerElite:    return "EnemyPrefab 1/ChargingMonster/EliteChargingMonster";
                case EnemyType.BomberNormal:    return "EnemyPrefab 1/BomberMonster/NormalBomberMonster";
                case EnemyType.BomberElite:     return "EnemyPrefab 1/BomberMonster/EliteBomberMonster";
                case EnemyType.CellNormal:      return "EnemyPrefab 1/CellMonster/NormalCellMonster";
                case EnemyType.CellElite:       return "EnemyPrefab 1/CellMonster/EliteCellMonster";
                default:                        return null;
            }
        }
    }
}
