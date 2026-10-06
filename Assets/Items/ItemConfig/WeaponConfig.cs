using System.Collections.Generic;
using UnityEngine;

namespace Game.Items
{
    /// <summary>
    /// 武器类型枚举
    /// </summary>
    public enum WeaponType
    {
        Melee,      // 近战（剑、匕首等）
        Ranged,     // 远程（枪、弓等，需要子弹）
        Throwable   // 投掷（之后扩展）
    }

    /// <summary>
    /// 武器数据。所有数值在Inspector/静态类里调整，不写死。
    /// extraStats字典用于扩展额外属性（暴击率、穿甲、吸血等）。
    /// </summary>
    [System.Serializable]
    public class WeaponData
    {
        public string id;                    // 唯一ID（如 "long_sword"）
        public string displayName;           // 显示名称（英文）
        public WeaponType weaponType;        // 武器类型
        public int price;                    // 价格
        public Sprite icon;                  // 图标（之后加）

        // ===== 基础属性 =====
        public float damage;                 // 基础伤害
        public float attackSpeed;            // 攻速（次/秒）
        public float range;                  // 攻击范围
        public float critChance;             // 暴击率（0-1，如0.04=4%）
        public float attackAngle;            // 攻击扇形角度（度，如120=面前120度扇形）

        // ===== 攻击惩罚 =====
        public float backAttackCooldownMultiplier;  // 背身攻击冷却倍率（如2=冷却变两倍）
        public float backAttackAngleThreshold;      // 背身攻击角度阈值（度，超过此夹角算背身）
        public float attackStopDuration;            // 移动中攻击时强制停下的时间（秒）

        // ===== 远程武器专属 =====
        public bool requiresBullets;         // 是否需要子弹
        public bool damageFalloff;           // 是否随距离递减伤害
        public float maxRange;               // 最大射程（子弹消失距离）
        public bool piercing;                // 是否穿透（同一直线多个敌人都受伤害）
        public float bulletSpread;           // 子弹散布（弧度，如0.03 = ±0.03弧度随机偏移）
        public float runSpreadMultiplier;    // 奔跑时散布倍率（默认3倍）

        // ===== 扩展属性（之后加：穿甲、吸血、击退等）=====
        public Dictionary<string, float> extraStats = new Dictionary<string, float>();

        /// <summary>获取扩展属性，不存在返回0</summary>
        public float GetExtraStat(string key)
        {
            return extraStats.TryGetValue(key, out float value) ? value : 0f;
        }

        /// <summary>设置扩展属性</summary>
        public void SetExtraStat(string key, float value)
        {
            extraStats[key] = value;
        }
    }

    /// <summary>
    /// 武器配置静态类。所有武器数据在这里登记，之后数值调整只改这里。
    /// </summary>
    public static class WeaponConfig
    {
        // ===== 武器列表 =====
        private static Dictionary<string, WeaponData> weapons = new Dictionary<string, WeaponData>();

        static WeaponConfig()
        {
            // 空手：近战，低伤害，快攻速
            Register(new WeaponData
            {
                id = "hand",
                displayName = "Hand",
                weaponType = WeaponType.Melee,
                price = 0,
                damage = 8f,
                attackSpeed = 4f,
                range = 1f,
                critChance = 0.04f,
                attackAngle = 360f,
                backAttackCooldownMultiplier = 1f,   // 空手无背身惩罚
                backAttackAngleThreshold = 360f,     // 任意方向都不算背身
                attackStopDuration = 0f,             // 空手不需要停下
                requiresBullets = false,
                damageFalloff = false,
                maxRange = 0f,
                piercing = false
            });

            // 长剑：近战，高暴击
            Register(new WeaponData
            {
                id = "long_sword",
                displayName = "Long Sword",
                weaponType = WeaponType.Melee,
                price = 50,
                damage = 15f,
                attackSpeed = 6f,
                range = 1.5f,
                critChance = 0.04f,
                attackAngle = 270f,
                requiresBullets = false,
                damageFalloff = false,
                maxRange = 0f,
                piercing = false
            });

            // 手枪：远程，伤害随距离递减
            Register(new WeaponData
            {
                id = "pistol",
                displayName = "Pistol",
                weaponType = WeaponType.Ranged,
                price = 500,
                damage = 20f,
                attackSpeed = 6f,
                range = 10f,
                critChance = 0.02f,
                attackAngle = 360f,
                backAttackCooldownMultiplier = 1f,
                backAttackAngleThreshold = 360f,
                attackStopDuration = 0f,
                requiresBullets = true,
                damageFalloff = true,
                maxRange = 12f,
                piercing = false,
                bulletSpread = 0.03f,
                runSpreadMultiplier = 3f
            });

            // 连发狙击枪：远程，高伤害，穿透，不衰减
            Register(new WeaponData
            {
                id = "burst_sniper",
                displayName = "Burst Sniper",
                weaponType = WeaponType.Ranged,
                price = 3000,  // 暂定，之后调整
                damage = 100f,
                attackSpeed = 3f,
                range = 20f,
                critChance = 0.10f,
                attackAngle = 360f,
                backAttackCooldownMultiplier = 1f,
                backAttackAngleThreshold = 360f,
                attackStopDuration = 0f,
                requiresBullets = true,
                damageFalloff = false,
                maxRange = 25f,
                piercing = true,
                bulletSpread = 0.02f,
                runSpreadMultiplier = 3f
            });
        }

        /// <summary>登记武器</summary>
        private static void Register(WeaponData weapon)
        {
            weapons[weapon.id] = weapon;
        }

        /// <summary>根据ID获取武器数据</summary>
        public static WeaponData GetWeapon(string id)
        {
            return weapons.TryGetValue(id, out WeaponData weapon) ? weapon : null;
        }

        /// <summary>获取所有武器（商店用）</summary>
        public static List<WeaponData> GetAllWeapons()
        {
            return new List<WeaponData>(weapons.Values);
        }
    }
}
