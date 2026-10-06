using System.Collections.Generic;
using UnityEngine;

namespace Game.Items
{
    /// <summary>
    /// 药水效果类型。之后加新效果只需要加枚举值。
    /// </summary>
    public enum PotionEffectType
    {
        Heal,               // 回血（即时）
        MaxHealth,          // 血量上限提升（持续）
        MaxStamina,         // 体力上限提升（持续）
        Speed,              // 移动速度提升（持续）
        CooldownReduction,  // 武器冷却减少（持续）
        AttackBoost,        // 攻击力提升（之后扩展）
        DefenseBoost        // 防御力提升（之后扩展）
    }

    /// <summary>
    /// 药水数据。
    /// duration=0 表示即时效果（如回血）。
    /// duration>0 表示持续效果（如加速），时间到后自动消失。
    /// effects字典存储所有效果，之后加新效果只需要加PotionEffectType枚举值。
    /// </summary>
    [System.Serializable]
    public class PotionData
    {
        public string id;                    // 唯一ID（如 "health_potion"）
        public string displayName;           // 显示名称（英文）
        public int price;                    // 价格
        public Sprite icon;                  // 图标（之后加）
        public float duration;               // 持续时间（秒），0=即时

        // ===== 效果列表 =====
        public Dictionary<PotionEffectType, float> effects = new Dictionary<PotionEffectType, float>();

        /// <summary>获取效果数值，不存在返回0</summary>
        public float GetEffect(PotionEffectType type)
        {
            return effects.TryGetValue(type, out float value) ? value : 0f;
        }

        /// <summary>是否有某个效果</summary>
        public bool HasEffect(PotionEffectType type)
        {
            return effects.ContainsKey(type);
        }
    }

    /// <summary>
    /// 药水配置静态类。所有药水数据在这里登记，数值调整只改这里。
    /// </summary>
    public static class PotionConfig
    {
        private static Dictionary<string, PotionData> potions = new Dictionary<string, PotionData>();

        static PotionConfig()
        {
            // 预加载图标（从Resources文件夹读取）
            Sprite healthIcon = Resources.Load<Sprite>("health_potion");
            Sprite stimulantIcon = Resources.Load<Sprite>("stimulant");
            Sprite speedIcon = Resources.Load<Sprite>("speed_potion");
            Sprite cooldownIcon = Resources.Load<Sprite>("cooldown_potion");

            // 回血药：即时回血20
            Register(new PotionData
            {
                id = "health_potion",
                displayName = "Health Potion",
                price = 50,
                duration = 0f,  // 即时
                icon = healthIcon,
                effects = new Dictionary<PotionEffectType, float>
                {
                    { PotionEffectType.Heal, 20f }
                }
            });

            // 兴奋剂：血量上限+30，体力上限+50（持续60秒）
            Register(new PotionData
            {
                id = "stimulant",
                displayName = "Vitality",
                price = 350,
                duration = 60f,  // 持续60秒
                icon = stimulantIcon,
                effects = new Dictionary<PotionEffectType, float>
                {
                    { PotionEffectType.MaxHealth, 30f },
                    { PotionEffectType.MaxStamina, 50f }
                }
            });

            // 速度药水：移动速度+20%（持续60秒）
            Register(new PotionData
            {
                id = "speed_potion",
                displayName = "Acceleration",
                price = 100,
                duration = 60f,  // 持续60秒
                icon = speedIcon,
                effects = new Dictionary<PotionEffectType, float>
                {
                    { PotionEffectType.Speed, 0.20f }  // +20%
                }
            });

            // 冷却药水：武器冷却-20%（持续60秒）
            Register(new PotionData
            {
                id = "cooldown_potion",
                displayName = "Haste",
                price = 150,
                duration = 60f,  // 持续60秒
                icon = cooldownIcon,
                effects = new Dictionary<PotionEffectType, float>
                {
                    { PotionEffectType.CooldownReduction, 0.20f }  // -20%
                }
            });
        }

        /// <summary>登记药水</summary>
        private static void Register(PotionData potion)
        {
            potions[potion.id] = potion;
        }

        /// <summary>根据ID获取药水数据</summary>
        public static PotionData GetPotion(string id)
        {
            return potions.TryGetValue(id, out PotionData potion) ? potion : null;
        }

        /// <summary>获取所有药水（商店用）</summary>
        public static List<PotionData> GetAllPotions()
        {
            return new List<PotionData>(potions.Values);
        }
    }
}
