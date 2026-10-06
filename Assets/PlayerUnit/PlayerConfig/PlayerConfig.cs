namespace Game.Player
{
    /// <summary>
    /// 玩家配置数值，所有玩家相关脚本从这里读取数据
    /// </summary>
    public static class PlayerConfig
    {
        // ══════════════════════════════════════════════════
        // 一、玩家固定数值（不随手感调整，改了影响游戏规则）
        // ══════════════════════════════════════════════════

        // Health
        /// <summary>最大生命值</summary>
        public static int MaxHealth = 200;

        /// <summary>初始防御（受击减免，实际伤害=伤害-0.5×防御，最少1点）</summary>
        public static int Defense = 0;

        /// <summary>无敌帧持续时间（秒）</summary>
        public static float InvincibleDuration = 1.5f;

        // Stamina
        /// <summary>最大体力值</summary>
        public static float MaxStamina = 100f;

        /// <summary>奔跑时每秒消耗体力</summary>
        public static float StaminaDrainRate = 10f;

        /// <summary>不奔跑时每秒回复体力</summary>
        public static float StaminaRegenRate = 15f;

        /// <summary>停止奔跑后多久开始回复（秒）</summary>
        public static float StaminaRegenDelay = 1f;

        /// <summary>体力低于多少百分比强制停止奔跑（0.02=2%）</summary>
        public static float StaminaMinThreshold = 0.02f;

        /// <summary>体力锁定后，恢复到多少百分比才能重新奔跑（0.12=12%）</summary>
        public static float StaminaRecoverThreshold = 0.15f;

        /// <summary>体力低于多少百分比显示红色</summary>
        public static float StaminaLowThreshold = 0.15f;

        /// <summary>体力满了之后多久隐藏PP条（秒）</summary>
        public static float StaminaHideDelay = 2f;

        // Speed
        /// <summary>移动速度（走路速度）</summary>
        public static float MoveSpeed = 3.5f;

        /// <summary>奔跑最大速度（Shift加速时的最大速度）</summary>
        public static float RunSpeed = 6f;

        // Attack
        /// <summary>攻击冷却时间（秒），防止一秒十刀</summary>
        public static float AttackCooldown = 0.5f;

        /// <summary>攻击范围（扇形半径）</summary>
        public static float AttackRange = 2f;

        /// <summary>击退力度</summary>
        public static float KnockDownForce = 0f;

        /// <summary>玩家攻击力</summary>
        public static int Attack = 10;

        /// <summary>玩家攻击角度</summary>
        public static int AttackAngle = 270;

        /// <summary>玩家击退抗性</summary>
        public static float KnockBackResistance = 0;

        /// <summary>伤害随机浮动下限（0.7=最低70%伤害）</summary>
        public static float DamageMinMultiplier = 0.7f;

        /// <summary>伤害随机浮动上限（1.3=最高130%伤害）</summary>
        public static float DamageMaxMultiplier = 1.3f;

        /// <summary>玩家缩放大小，也用作碰撞检测半径</summary>
        public static float PlayerSize = 1f;

        /// <summary>玩家碰撞检测半径比例系数</summary>
        public static float PlayerSizeParameter = 0.3f;


        // ══════════════════════════════════════════════════
        // 二、手感数值（专门调手感用，改了只影响操作体验）
        // ══════════════════════════════════════════════════

        /// <summary>起跑/转向曲线系数（指数趋近，k越大越跟手，5≈0.2秒到63%）</summary>
        public static float MoveAccelK = 12f;

        /// <summary>停下滑行衰减系数（k越小滑得越远，5≈0.2秒停63%）</summary>
        public static float MoveDecelK = 12f;

        /// <summary>速度低于这个值就直接视为停止（动画跟着实际速度走）</summary>
        public static float MoveStopThreshold = 0.2f;

        /// <summary>急刹硬直时间（秒，冲刺结束后硬直）</summary>
        public static float BrakeDuration = 0.05f;

        /// <summary>无敌帧闪烁间隔（秒，越小闪得越快）</summary>
        public static float BlinkInterval = 0.02f;

        /// <summary>攻击时移动降到的速度</summary>
        public static float AttackSlowSpeed = 0.5f;

        /// <summary>攻击减速指数系数（越大减速越快）</summary>
        public static float AttackSlowLerp = 3f;

        /// <summary>减速恢复阈值（速度差小于这个值就停止减速恢复）</summary>
        public static float AttackSlowThreshold = 0.1f;


        // ══════════════════════════════════════════════════
        // 三、相机跟随玩家的手感
        // ══════════════════════════════════════════════════

        /// <summary>相机跟随平滑速度（越大越紧跟）</summary>
        public static float CameraSmoothSpeed = 2.5f;

        /// <summary>输入方向平滑系数（越大越快响应输入）</summary>
        public static float CameraInputSmooth = 8f;

        /// <summary>转向后相机延迟（秒）</summary>
        public static float CameraTurnDelay = 0.6f;

        /// <summary>死区大小（玩家在屏幕中心这个范围内相机不动）</summary>
        public static float CameraDeadzoneX = 2f;

        public static float CameraDeadzoneY = 1f;

        /// <summary>死区内自由偏移最大距离（按方向键相机多往外看）</summary>
        public static float CameraFreeRadius = 1.5f;
    }
}
