using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// 群体排斥（执行层）：别跟其他怪挤成一团。
    /// 检测半径内 Enemy 标签碰撞体，按距离反比生成推开方向。
    /// 静态复用缓冲防 GC。
    /// </summary>
    public class EnemySeparation
    {
        private static Collider2D[] separationBuffer = new Collider2D[16]; // 复用检测缓冲，防止GC

        public Vector2 GetSeparationDir(Transform self)
        {
            Vector2 dir = Vector2.zero;
            Collider2D selfCollider = self.GetComponent<Collider2D>(); // 缓存自身碰撞体，避免循环内反复取
            int count = Physics2D.OverlapCircleNonAlloc(
                self.position, MonsterTemplate.separationRadius, separationBuffer);
            for (int i = 0; i < count; i++)
            {
                var other = separationBuffer[i];
                if (other == null || other == selfCollider) continue;
                if (!other.CompareTag("Enemy")) continue;
                Vector2 push = (Vector2)(self.position - other.transform.position);
                if (push.sqrMagnitude > 0.001f)
                {
                    dir += push.normalized / Mathf.Max(push.magnitude, 0.1f);
                }
            }
            return dir;
        }
    }
}
