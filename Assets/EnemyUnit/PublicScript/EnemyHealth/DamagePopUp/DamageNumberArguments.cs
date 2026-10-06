using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// 伤害跳字全部可调参数，集中在这一处（原来是两个组件里的硬编码 SerializeField）。
    /// 由 DamagePopup 持有，生成数字时随 Initialize 一起传给 DamageNumber。
    /// </summary>
    [System.Serializable]
    public class DamageNumberArguments
    {
        // ---------- DamagePopup 使用 ----------

        /// <summary>生成位置偏移（头顶）</summary>
        public Vector3 offset = new Vector3(0, 1f, 0);

        /// <summary>最小生成间隔（防止数字重叠）</summary>
        public float spawnInterval = 0.05f;

        // ---------- DamageNumber 使用 ----------

        /// <summary>跳起最大高度</summary>
        public float jumpHeight = 1.2f;

        /// <summary>重力（越大下落越快）</summary>
        public float gravity = 20f;

        /// <summary>水平移动速度</summary>
        public float horizontalSpeed = 1f;

        /// <summary>颠簸次数</summary>
        public int bounceCount = 2;

        /// <summary>颠簸衰减（每次弹跳高度乘以这个值）</summary>
        public float bounceDamping = 0.4f;

        /// <summary>普通伤害字号（世界空间）</summary>
        public float normalSize = 4f;

        /// <summary>暴击伤害字号</summary>
        public float critSize = 6f;

        /// <summary>普通伤害颜色</summary>
        public Color normalColor = Color.white;

        /// <summary>暴击伤害颜色</summary>
        public Color critColor = Color.red;

        /// <summary>暴击跳跃高度倍率</summary>
        public float critHeightMultiplier = 1.5f;

        /// <summary>暴击持续时间倍率</summary>
        public float critDurationMultiplier = 1.5f;
    }
}
