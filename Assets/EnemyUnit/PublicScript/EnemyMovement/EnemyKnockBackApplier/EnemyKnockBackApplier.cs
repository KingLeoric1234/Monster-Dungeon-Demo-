using UnityEngine;
using Game.Core;

namespace Game.Enemy
{
    /// <summary>
    /// 击退插件：移动管线面向击退的唯一接口，独立文件夹。
    /// 职责：
    ///   1. 冻结门 —— 被击退期间禁止移动（S0 门控读 IsFrozen）；
    ///   2. 击退受力 —— 现在转发给现有 KnockBackHandler（Game.Core），
    ///      未来由本插件吸收其受力逻辑，成为击退唯一宿主。
    /// 依赖方向：移动管线 → 本插件；管线不再直接碰 KnockBackHandler。
    /// </summary>
    public class EnemyKnockBackApplier : MonoBehaviour
    {
        private KnockBackHandler knockBack;

        private void Awake()
        {
            knockBack = GetComponent<KnockBackHandler>();
        }

        /// <summary>冻结信号：被击退期间 Movement 整线跳过（S0 门控）</summary>
        public bool IsFrozen => knockBack != null && knockBack.IsKnockedBack;

        /// <summary>对外转发：施加击退（方向 + 抗性）。吸收 KnockBackHandler 后本方法自行实现。</summary>
        public void ApplyKnockBack(Vector2 direction, float resistance)
        {
            if (knockBack != null) knockBack.ApplyKnockBack(direction, resistance);
        }
    }
}
