using UnityEngine;
using System.Collections.Generic;
using Game.Core;
using Game.Items;
using Game.Player;

namespace Game.PlayerStats
{
    /// <summary>
    /// 药水Buff管理器：订阅PotionUsedMessage，处理药水效果和持续时间。
    /// 到时间自动清除Buff，发布BuffChangedMessage通知UI更新。
    /// 挂在GameManager下。
    /// </summary>
    public class PotionBuffDirector : MonoBehaviour
    {
        public static PotionBuffDirector Instance { get; private set; }

        // 当前激活的Buff：key=药水ID，value=剩余时间
        private Dictionary<string, float> activeBuffs = new Dictionary<string, float>();
        // displayName到potionID的映射（BuffDisplay用名称查剩余时间）
        private Dictionary<string, string> displayNameToID = new Dictionary<string, string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            MessageBus.Subscribe<PotionUsedMessage>(OnPotionUsed);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<PotionUsedMessage>(OnPotionUsed);
        }

        private void Update()
        {
            // 更新所有Buff的剩余时间
            if (activeBuffs.Count == 0) return;

            // 先把所有key存到列表里，避免遍历Dictionary时修改导致报错
            List<string> keys = new List<string>(activeBuffs.Keys);
            List<string> expired = new List<string>();

            foreach (string key in keys)
            {
                activeBuffs[key] -= Time.deltaTime;
                if (activeBuffs[key] <= 0f)
                {
                    expired.Add(key);
                }
            }

            // 清除过期的Buff
            foreach (string potionID in expired)
            {
                RemoveBuff(potionID);
            }
        }

        /// <summary>使用药水时触发</summary>
        private void OnPotionUsed(PotionUsedMessage msg)
        {
            // Debug.Log($"[PotionBuffDirector] 收到药水使用消息: {msg.PotionID}");
            var potionData = PotionConfig.GetPotion(msg.PotionID);
            if (potionData == null)
            {
                // Debug.LogWarning($"[PotionBuffDirector] 药水数据为null: {msg.PotionID}");
                return;
            }

            // Debug.Log($"[PotionBuffDirector] 药水: {potionData.displayName}, duration={potionData.duration}");

            // 即时效果（回血）
            if (potionData.HasEffect(PotionEffectType.Heal))
            {
                float healAmount = potionData.GetEffect(PotionEffectType.Heal);
                // 回血请求走消息：药水域不摸 PlayerHealth，由 PlayerHealth 自己订阅执行
                MessageBus.Publish(new PlayerHealRequestMessage { Amount = Mathf.RoundToInt(healAmount) });
            }

            // 持续效果
            if (potionData.duration > 0f)
            {
                // Debug.Log($"[PotionBuffDirector] 应用持续Buff");
                ApplyBuff(msg.PotionID, potionData);
            }
        }

        /// <summary>应用持续Buff</summary>
        private void ApplyBuff(string potionID, PotionData potionData)
        {
            // 如果已经有这个Buff，刷新时间
            if (activeBuffs.ContainsKey(potionID))
            {
                activeBuffs[potionID] = potionData.duration;
            }
            else
            {
                activeBuffs[potionID] = potionData.duration;
            }

            // 应用效果到PlayerStatsDirector
            if (potionData.HasEffect(PotionEffectType.MaxHealth))
            {
                PlayerStatsDirector.Instance?.SetStimulantBonus(
                    potionData.GetEffect(PotionEffectType.MaxHealth),
                    potionData.GetEffect(PotionEffectType.MaxStamina),
                    potionData.duration);
            }
            if (potionData.HasEffect(PotionEffectType.Speed))
            {
                PlayerStatsDirector.Instance?.SetSpeedBonus(
                    potionData.GetEffect(PotionEffectType.Speed),
                    potionData.duration);
            }
            if (potionData.HasEffect(PotionEffectType.CooldownReduction))
            {
                PlayerStatsDirector.Instance?.SetCooldownReduction(
                    potionData.GetEffect(PotionEffectType.CooldownReduction),
                    potionData.duration);
            }

            // 通知UI
            MessageBus.Publish(new BuffChangedMessage
            {
                BuffName = potionData.displayName,
                Duration = potionData.duration,
                Icon = potionData.icon,
                IsAdded = true,
                Description = GetDescription(potionID)
            });

            // 存displayName到potionID的映射
            displayNameToID[potionData.displayName] = potionID;
        }

        /// <summary>根据药水ID获取描述</summary>
        private string GetDescription(string potionID)
        {
            switch (potionID)
            {
                case "speed_potion": return "Movement speed +20%";
                case "stimulant": return "Max HP +30, Max PP +50";
                case "cooldown_potion": return "Weapon cooldown -20%";
                default: return "";
            }
        }

        /// <summary>移除Buff</summary>
        private void RemoveBuff(string potionID)
        {
            if (!activeBuffs.ContainsKey(potionID)) return;

            var potionData = PotionConfig.GetPotion(potionID);
            activeBuffs.Remove(potionID);

            // 移除displayName映射
            if (potionData != null && displayNameToID.ContainsKey(potionData.displayName))
            {
                displayNameToID.Remove(potionData.displayName);
            }

            // 重新计算所有Buff（因为可能有多个Buff叠加）
            RecalculateAllBuffs();

            // 通知UI
            if (potionData != null)
            {
                MessageBus.Publish(new BuffChangedMessage
                {
                    BuffName = potionData.displayName,
                    Duration = 0f,
                    Icon = null,
                    IsAdded = false,
                    Description = ""
                });
            }
        }

        /// <summary>重新计算所有Buff的效果（移除一个后需要重算）</summary>
        private void RecalculateAllBuffs()
        {
            // 先清除所有效果
            PlayerStatsDirector.Instance?.ClearAllPotionBonuses();

            // 重新应用剩余的Buff
            foreach (var kvp in activeBuffs)
            {
                var potionData = PotionConfig.GetPotion(kvp.Key);
                if (potionData == null) continue;

                if (potionData.HasEffect(PotionEffectType.MaxHealth))
                {
                    PlayerStatsDirector.Instance?.SetStimulantBonus(
                        potionData.GetEffect(PotionEffectType.MaxHealth),
                        potionData.GetEffect(PotionEffectType.MaxStamina),
                        kvp.Value);
                }
                if (potionData.HasEffect(PotionEffectType.Speed))
                {
                    PlayerStatsDirector.Instance?.SetSpeedBonus(
                        potionData.GetEffect(PotionEffectType.Speed),
                        kvp.Value);
                }
                if (potionData.HasEffect(PotionEffectType.CooldownReduction))
                {
                    PlayerStatsDirector.Instance?.SetCooldownReduction(
                        potionData.GetEffect(PotionEffectType.CooldownReduction),
                        kvp.Value);
                }
            }
        }

        /// <summary>获取某个Buff的剩余时间（按药水ID）</summary>
        public float GetBuffRemainingTime(string potionID)
        {
            return activeBuffs.TryGetValue(potionID, out float time) ? time : 0f;
        }

        /// <summary>获取某个Buff的剩余时间（按displayName）</summary>
        public float GetBuffRemainingTimeByName(string displayName)
        {
            if (displayNameToID.TryGetValue(displayName, out string potionID))
            {
                return GetBuffRemainingTime(potionID);
            }
            return 0f;
        }

        /// <summary>是否有某个Buff</summary>
        public bool HasBuff(string potionID)
        {
            return activeBuffs.ContainsKey(potionID);
        }
    }
}
