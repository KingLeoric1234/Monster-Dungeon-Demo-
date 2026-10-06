using UnityEngine;
using Game.Core;
using Game.Items;

namespace Game.Player
{
    /// <summary>
    /// 玩家属性计算器：单例，管玩家最终属性。
    /// 订阅装备/固定槽变化消息，重新计算，发布PlayerStatsChangedMessage。
    /// 所有需要玩家属性的脚本从这里读取，不要自己算。
    /// 挂在GameManager下。
    /// </summary>
    public class PlayerStatsDirector : MonoBehaviour
    {
        public static PlayerStatsDirector Instance { get; private set; }

        // ── 最终属性（其他脚本从这里读） ──────────────────────
        public int FinalAttack { get; private set; }
        public int FinalDefense { get; private set; }
        public float FinalMoveSpeed { get; private set; }
        public float FinalRunSpeed { get; private set; }
        public int FinalMaxHealth { get; private set; }
        public float FinalMaxStamina { get; private set; }
        public float FinalAttackSpeed { get; private set; }
        public float FinalAttackRange { get; private set; }
        public float FinalCritChance { get; private set; }
        public int TotalArmor { get; private set; }
        public float TotalSpeedPenalty { get; private set; }

        // ── 药水临时加成（之后做药水使用时更新） ──────────────
        private float potionHealthBonus = 0f;    // 兴奋剂血量上限加成
        private float potionStaminaBonus = 0f;   // 兴奋剂体力上限加成
        private float potionSpeedBonus = 0f;     // 速度药水加成
        private float potionCooldownReduction = 0f;  // 冷却药水减冷却

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
            MessageBus.Subscribe<EquipmentEquippedMessage>(OnEquipmentChanged);
            MessageBus.Subscribe<FixedSlotChangedMessage>(OnFixedSlotChanged);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<EquipmentEquippedMessage>(OnEquipmentChanged);
            MessageBus.Unsubscribe<FixedSlotChangedMessage>(OnFixedSlotChanged);
        }

        private void Start()
        {
            // 初始化时算一次
            RecalculateStats();
        }

        /// <summary>装备变化时重新计算</summary>
        private void OnEquipmentChanged(EquipmentEquippedMessage msg)
        {
            RecalculateStats();
        }

        /// <summary>固定槽变化时重新计算（武器在HandUse里）</summary>
        private void OnFixedSlotChanged(FixedSlotChangedMessage msg)
        {
            RecalculateStats();
        }

        /// <summary>重新计算所有属性</summary>
        public void RecalculateStats()
        {
            if (ItemDirector.Instance == null) return;

            // 1. 算装备总属性（护甲、移速惩罚）
            var (armor, speedPenalty) = ItemDirector.Instance.CalculateTotalStats();
            TotalArmor = armor;
            TotalSpeedPenalty = Mathf.Clamp01(speedPenalty);

            // 2. 算武器属性（从HandUse固定槽读取）
            var (weaponType, weaponID) = ItemDirector.Instance.GetFixedSlot(FixedSlotType.HandUse);
            WeaponData weapon = null;
            if (weaponType == FixedItemType.Weapon && !string.IsNullOrEmpty(weaponID))
            {
                weapon = WeaponConfig.GetWeapon(weaponID);
            }
            if (weapon == null)
            {
                weapon = WeaponConfig.GetWeapon("hand"); // 空手
            }

            // 3. 最终属性
            FinalAttack = weapon != null ? Mathf.RoundToInt(weapon.damage) : PlayerConfig.Attack;
            FinalDefense = EquipmentConfig.BaseArmor + TotalArmor;
            FinalMoveSpeed = PlayerConfig.MoveSpeed * (1f - TotalSpeedPenalty) * (1f + potionSpeedBonus);
            FinalRunSpeed = PlayerConfig.RunSpeed * (1f - TotalSpeedPenalty) * (1f + potionSpeedBonus);
            FinalMaxHealth = Mathf.RoundToInt(PlayerConfig.MaxHealth + potionHealthBonus);
            FinalMaxStamina = PlayerConfig.MaxStamina + potionStaminaBonus;
            FinalAttackSpeed = weapon != null ? weapon.attackSpeed * (1f - potionCooldownReduction) : PlayerConfig.AttackCooldown;
            FinalAttackRange = weapon != null ? weapon.range : PlayerConfig.AttackRange;
            FinalCritChance = weapon != null ? weapon.critChance : 0.04f; // 默认4%暴击

            // 4. 发布消息
            MessageBus.Publish(new PlayerStatsChangedMessage
            {
                Attack = FinalAttack,
                Defense = FinalDefense,
                MoveSpeed = FinalMoveSpeed,
                RunSpeed = FinalRunSpeed,
                MaxHealth = FinalMaxHealth,
                MaxStamina = FinalMaxStamina,
                AttackSpeed = FinalAttackSpeed,
                AttackRange = FinalAttackRange,
                CritChance = FinalCritChance,
                Armor = TotalArmor,
                SpeedPenalty = TotalSpeedPenalty
            });
        }

        // ── 药水加成接口（之后做药水使用时调用） ──────────────

        /// <summary>设置兴奋剂加成（血量上限+体力上限）</summary>
        public void SetStimulantBonus(float healthBonus, float staminaBonus, float duration)
        {
            potionHealthBonus = healthBonus;
            potionStaminaBonus = staminaBonus;
            RecalculateStats();
            // TODO: duration后自动清除（之后做）
        }

        /// <summary>设置速度药水加成</summary>
        public void SetSpeedBonus(float speedBonus, float duration)
        {
            potionSpeedBonus = speedBonus;
            RecalculateStats();
        }

        /// <summary>设置冷却药水加成</summary>
        public void SetCooldownReduction(float reduction, float duration)
        {
            potionCooldownReduction = reduction;
            RecalculateStats();
        }

        /// <summary>清除所有药水加成</summary>
        public void ClearAllPotionBonuses()
        {
            potionHealthBonus = 0f;
            potionStaminaBonus = 0f;
            potionSpeedBonus = 0f;
            potionCooldownReduction = 0f;
            RecalculateStats();
        }
    }
}
