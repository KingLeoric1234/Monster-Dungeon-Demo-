using UnityEngine;

namespace Game.Enemy
{
    /// <summary>怪物移动状态（核心中的核心）</summary>
    public enum EnemyState { Patrol, Chase }

    /// <summary>
    /// 台帐：Movement 管线共享数据。只存不算，各计算单元读写它，段与段不直接传参。
    /// </summary>
    public class EnemyMovementData
    {
        // ===== 方向 =====
        public Vector2 currentDirection;   // 当前实际方向（S5 写 · S8 读）
        public Vector2 patrolDirection;    // 巡逻随机方向（Init 播种 · S3 用）
        public float patrolTimer;          // 巡逻换向倒计时（Init 播种 · S3 用）

        // ===== 速度 =====
        public float currentSpeed;         // 当前实际速度（S7 写 · S8 读）

        // ===== 状态 =====
        public EnemyState currentState;    // 巡逻/追击（S1 写 · S2/S3/S6 读）

        // ===== 思考节奏（S2 用）=====
        public float thinkTimer;           // 距离下次思考的倒计时
        public float thinkPauseLeft;       // 当前剩余停顿时间
        public bool isThinking;            // 正在停顿思考中

        // ===== 偏移预判（S3 用）=====
        public Vector2 currentOffset;      // 当前偏移向量
        public float offsetTimer;          // 距下次随机化的倒计时
        public float nextOffsetTime;       // 下次随机化的触发时间

        /// <summary>只读暴露：外部（表现层/动画）读当前方向</summary>
        public Vector2 CurrentDirection => currentDirection;
    }
}
