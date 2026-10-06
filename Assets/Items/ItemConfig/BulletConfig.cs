using System.Collections.Generic;
using UnityEngine;

namespace Game.Items
{
    /// <summary>
    /// 子弹数据。不同子弹提供不同的额外伤害，之后可扩展（穿甲、爆炸等）。
    /// </summary>
    [System.Serializable]
    public class BulletData
    {
        public string id;                    // 唯一ID（如 "normal_bullet"）
        public string displayName;           // 显示名称（英文）
        public int price;                    // 单价（每发）
        public float bonusDamage;            // 额外伤害
        public Sprite icon;                  // 图标（之后加）

        // ===== 扩展属性（穿甲、爆炸、燃烧等）=====
        public Dictionary<string, float> extraStats = new Dictionary<string, float>();

        /// <summary>获取扩展属性，不存在返回0</summary>
        public float GetExtraStat(string key)
        {
            return extraStats.TryGetValue(key, out float value) ? value : 0f;
        }
    }

    /// <summary>
    /// 子弹配置静态类。所有子弹数据在这里登记。
    /// </summary>
    public static class BulletConfig
    {
        private static Dictionary<string, BulletData> bullets = new Dictionary<string, BulletData>();

        static BulletConfig()
        {
            // 普通子弹
            Register(new BulletData
            {
                id = "normal_bullet",
                displayName = "Normal Bullet",
                price = 20,
                bonusDamage = 5f
            });

            // 钢制子弹
            Register(new BulletData
            {
                id = "steel_bullet",
                displayName = "Steel Bullet",
                price = 50,
                bonusDamage = 15f
            });

            // 钛钢子弹
            Register(new BulletData
            {
                id = "titanium_bullet",
                displayName = "Titanium Bullet",
                price = 100,
                bonusDamage = 35f
            });
        }

        /// <summary>登记子弹</summary>
        private static void Register(BulletData bullet)
        {
            bullets[bullet.id] = bullet;
        }

        /// <summary>根据ID获取子弹数据</summary>
        public static BulletData GetBullet(string id)
        {
            return bullets.TryGetValue(id, out BulletData bullet) ? bullet : null;
        }

        /// <summary>获取所有子弹（商店用）</summary>
        public static List<BulletData> GetAllBullets()
        {
            return new List<BulletData>(bullets.Values);
        }
    }
}
