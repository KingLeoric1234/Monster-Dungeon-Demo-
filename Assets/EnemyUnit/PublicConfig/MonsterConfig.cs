using UnityEngine;

namespace Game.Enemy
{
    /// <summary>怪物模板数据
    /// 分两块：核心数据决定游戏规则，手感数据决定操作体验
    /// </summary>
    [System.Serializable]
    public class MonsterTemplate
    {
        // ═══════════════════════════════════════════════════════
        // 一、核心数据（决定游戏规则，改了影响数值平衡）
        // ═══════════════════════════════════════════════════════

        public string monsterType;        // 怪物类型标识（Collision/Charger/Bomber/Cell）
        public string difficulty;         // 难度（Normal/Elite）
        public string monsterName;        // 怪物名字（用于显示和日志）
        public int maxHealth;             // 最大血量
        public float moveSpeed;           // 满速速度（追击时的目标速度）
        public int attackPower;           // 攻击伤害
        public float attackCooldown;      // 攻击间隔（秒）
        public float size;                // 怪物大小（视觉缩放）
        public float knockBackResistance; // 击退抗性（0=被击退，1=完全不吃击退）
        public float knockBackForce;      // 击退力度
        public float aggroRange;          // 仇恨范围（小于这个距离就追击）
        public int maxSplits = 0;         // 最多分裂次数（细胞怪用）

        // ═══════════════════════════════════════════════════════
        // 二、手感数据（专门调手感，改了不影响游戏规则）
        // ═══════════════════════════════════════════════════════

        public float accelK = 5f;         // 起跑系数（指数趋近，越大起步越快）
        public float decelK = 4f;         // 减速系数（停下滑行衰减，越小滑得越远）
        public float patrolSpeedRatio = 0.5f; // 巡逻速度 = 满速 × 这个比例
        public float patrolMinTime;       // 巡逻最短时间（秒）
        public float patrolMaxTime;       // 巡逻最长时间（秒）
        public float minOffsetInterval;   // 追击偏移最小间隔（秒）
        public float maxOffsetInterval;   // 追击偏移最大间隔（秒）
        public float maxOffset;           // 追击最大偏移（左右随机摆动幅度）
        public float flashDuration = 0.1f;     // 受击闪红时长（秒）
        public float hitStopDuration = 0.01f;  // 顿帧时长（秒）

        // ── 公共手感（所有怪物共享，不分种类） ──
        public static float thinkIntervalMin = 4f;   // 追击时"思考"间隔最短（秒）
        public static float thinkIntervalMax = 8f;   // 追击时"思考"间隔最长（秒）
        public static float thinkPauseMin = 0.2f;    // 思考停顿最短（秒）
        public static float thinkPauseMax = 0.8f;    // 思考停顿最长（秒）
        public static float separationRadius = 0.8f;  // 同伴排斥半径
        public static float separationForce = 1f;     // 同伴排斥力度
        public float chargeCooldown = 5f;             // 冲锋怪CD（秒）
        public float chargeSpeed = 10f;               // 冲锋速度
        public float windupTime = 3f;                 // 蓄力时长（秒）
        public float chargeDuration = 0.5f;           // 冲锋持续时长（秒）
        public float recoverTime = 1.6f;              // 硬直时长（秒）
    }

    /// <summary>6种怪物数据：3种类型×2种难度</summary>
    public static class MonsterConfig
    {
        // ===== 碰撞怪 =====
        public static readonly MonsterTemplate CollisionNormal = new MonsterTemplate
        {
            monsterType = "Collision", difficulty = "Normal", monsterName = "Collision Slime",
            maxHealth = 30, moveSpeed = 1.8f, attackPower = 8, attackCooldown = 1.0f,
            size = 3f,
            knockBackResistance = 0.2f, knockBackForce = 5f,
            aggroRange = 5f,
            accelK = 5f, decelK = 4f, patrolSpeedRatio = 0.5f,
            patrolMinTime = 2f, patrolMaxTime = 4f,
            minOffsetInterval = 5f, maxOffsetInterval = 10f, maxOffset = 1f
        };

        public static readonly MonsterTemplate CollisionElite = new MonsterTemplate
        {
            monsterType = "Collision", difficulty = "Elite", monsterName = "Collision Brute",
            maxHealth = 90, moveSpeed = 3f, attackPower = 15, attackCooldown = 0.8f,
            size = 3.5f,
            knockBackResistance = 0.5f, knockBackForce = 10f,
            aggroRange = 6f,
            accelK = 3f, decelK = 2f, patrolSpeedRatio = 0.4f,  // 大块头起步慢、滑行久
            patrolMinTime = 2f, patrolMaxTime = 4f,
            minOffsetInterval = 4f, maxOffsetInterval = 7f, maxOffset = 0.8f
        };

        // ===== 狙击怪 =====
        public static readonly MonsterTemplate SniperNormal = new MonsterTemplate
        {
            monsterType = "Sniper", difficulty = "Normal", monsterName = "Sniper Dreg",
            maxHealth = 25, moveSpeed = 0.8f, attackPower = 12, attackCooldown = 1.5f,
            size = 3f,
            knockBackResistance = 0.1f, knockBackForce = 3f,
            aggroRange = 8f,
            accelK = 4f, decelK = 4f, patrolSpeedRatio = 0.6f,
            patrolMinTime = 3f, patrolMaxTime = 5f,
            minOffsetInterval = 5f, maxOffsetInterval = 10f, maxOffset = 0.5f
        };

        public static readonly MonsterTemplate SniperElite = new MonsterTemplate
        {
            monsterType = "Sniper", difficulty = "Elite", monsterName = "Sniper Veteran",
            maxHealth = 70, moveSpeed = 0.8f, attackPower = 25, attackCooldown = 1.2f,
            size = 3.2f,
            knockBackResistance = 0.3f, knockBackForce = 5f,
            aggroRange = 10f,
            accelK = 4f, decelK = 4f, patrolSpeedRatio = 0.6f,
            patrolMinTime = 3f, patrolMaxTime = 5f,
            minOffsetInterval = 4f, maxOffsetInterval = 8f, maxOffset = 0.5f
        };

        // ===== 自爆怪 =====
        public static readonly MonsterTemplate BomberNormal = new MonsterTemplate
        {
            monsterType = "Bomber", difficulty = "Normal", monsterName = "Bomber Grub",
            maxHealth = 20, moveSpeed = 1.9f, attackPower = 20, attackCooldown = 1.0f,
            size = 2.5f,
            knockBackResistance = 0.1f, knockBackForce = 2f,
            aggroRange = 6f,
            accelK = 6f, decelK = 3f, patrolSpeedRatio = 0.7f,  // 追你追得快，停得慢
            patrolMinTime = 2f, patrolMaxTime = 4f,
            minOffsetInterval = 5f, maxOffsetInterval = 10f, maxOffset = 0.8f
        };

        public static readonly MonsterTemplate BomberElite = new MonsterTemplate
        {
            monsterType = "Bomber", difficulty = "Elite", monsterName = "Bomber Fiend",
            maxHealth = 55, moveSpeed = 2.3f, attackPower = 40, attackCooldown = 0.8f,
            size = 3f,
            knockBackResistance = 0.2f, knockBackForce = 3f,
            aggroRange = 8f,
            accelK = 6f, decelK = 3f, patrolSpeedRatio = 0.7f,
            patrolMinTime = 2f, patrolMaxTime = 4f,
            minOffsetInterval = 4f, maxOffsetInterval = 8f, maxOffset = 0.8f
        };

        // ===== 冲锋怪 =====
        public static readonly MonsterTemplate ChargerNormal = new MonsterTemplate
        {
            monsterType = "Charger", difficulty = "Normal", monsterName = "Charger Runner",
            maxHealth = 35, moveSpeed = 1f, attackPower = 12, attackCooldown = 1.0f,
            size = 3f,
            knockBackResistance = 0.3f, knockBackForce = 5f,
            aggroRange = 8f,
            accelK = 5f, decelK = 4f, patrolSpeedRatio = 0.5f,
            patrolMinTime = 2f, patrolMaxTime = 4f,
            minOffsetInterval = 5f, maxOffsetInterval = 10f, maxOffset = 0.8f,
            chargeCooldown = 5f, chargeSpeed = 10f, windupTime = 3f,
            chargeDuration = 0.5f, recoverTime = 1.6f
        };

        public static readonly MonsterTemplate ChargerElite = new MonsterTemplate
        {
            monsterType = "Charger", difficulty = "Elite", monsterName = "Charger Berserker",
            maxHealth = 100, moveSpeed = 1f, attackPower = 25, attackCooldown = 0.8f,
            size = 3.5f,
            knockBackResistance = 0.5f, knockBackForce = 8f,
            aggroRange = 10f,
            accelK = 3f, decelK = 2f, patrolSpeedRatio = 0.4f,
            patrolMinTime = 2f, patrolMaxTime = 4f,
            minOffsetInterval = 4f, maxOffsetInterval = 8f, maxOffset = 0.8f,
            chargeCooldown = 3f, chargeSpeed = 12f, windupTime = 2f,
            chargeDuration = 0.6f, recoverTime = 1.2f
        };

        // ===== 细胞怪 =====
        public static readonly MonsterTemplate CellNormal = new MonsterTemplate
        {
            monsterType = "Cell", difficulty = "Normal", monsterName = "Cell Splitter",
            maxHealth = 40, moveSpeed = 1.3f, attackPower = 8, attackCooldown = 1.0f,
            size = 4f,
            knockBackResistance = 0.3f, knockBackForce = 4f,
            aggroRange = 5f,
            accelK = 4f, decelK = 3f, patrolSpeedRatio = 0.5f,
            patrolMinTime = 2f, patrolMaxTime = 4f,
            minOffsetInterval = 5f, maxOffsetInterval = 10f, maxOffset = 0.8f,
            maxSplits = 1
        };

        public static readonly MonsterTemplate CellElite = new MonsterTemplate
        {
            monsterType = "Cell", difficulty = "Elite", monsterName = "Cell Nucleus",
            maxHealth = 120, moveSpeed = 1.2f, attackPower = 12, attackCooldown = 0.8f,
            size = 6f,
            knockBackResistance = 0.5f, knockBackForce = 6f,
            aggroRange = 6f,
            accelK = 2f, decelK = 2f, patrolSpeedRatio = 0.4f,  // 大块头慢
            patrolMinTime = 2f, patrolMaxTime = 4f,
            minOffsetInterval = 4f, maxOffsetInterval = 8f, maxOffset = 0.8f,
            maxSplits = 2
        };
    }
}
