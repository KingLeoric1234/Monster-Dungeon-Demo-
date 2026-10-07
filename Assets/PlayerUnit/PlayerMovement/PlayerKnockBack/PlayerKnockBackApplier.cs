// Assets/PlayerUnit/PlayerMovement/PlayerKnockBack/PlayerKnockBackApplier.cs
using UnityEngine;
using Game.Core;

namespace Game.Player
{
    /// <summary>
    /// 击退插件（Plug）：移动管线面向击退的唯一接口，独立 Plug 文件夹。
    ///
    /// 职责：
    ///   1. 冻结门 —— 被击退期间 Movement 整线跳过（IsFrozen，Update 门控读取）；
    ///   2. 击退受力 / 检测半径 —— 转发给现有 KnockBackHandler（Game.Core），
    ///      未来由本插件吸收其受力逻辑，成为击退唯一宿主。
    ///
    /// 依赖方向：移动管线 → 本插件；管线不再直接碰 KnockBackHandler。
    /// 挂在玩家物体上（与 KnockBackHandler 同一物体）。
    /// </summary>
    public class PlayerKnockBackApplier : MonoBehaviour
    {
        private KnockBackHandler knockBack;

        private void Awake()
        {
            knockBack = GetComponent<KnockBackHandler>();
        }

        /// <summary>冻结信号：被击退期间 Movement 整线跳过（S0 门控）</summary>
        public bool IsFrozen => knockBack != null && knockBack.IsKnockedBack;

        /// <summary>设置击退检测半径（转发给 KnockBackHandler）</summary>
        public void SetCheckRadius(float radius)
        {
            if (knockBack != null) knockBack.SetCheckRadius(radius);
        }

        /// <summary>对外转发：施加击退（方向 + 抗性）。吸收 KnockBackHandler 后本方法自行实现。</summary>
        public void ApplyKnockBack(Vector2 direction, float resistance)
        {
            if (knockBack != null) knockBack.ApplyKnockBack(direction, resistance);
        }
    }
}
