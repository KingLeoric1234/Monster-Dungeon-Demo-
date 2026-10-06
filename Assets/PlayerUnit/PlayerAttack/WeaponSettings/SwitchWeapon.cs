using UnityEngine;
using Game.Core;
using Game.Items;

namespace Game.Weapons
{
    /// <summary>
    /// 武器切换器：检测并维护玩家手持武器状态（原 WeaponDirector 的武器状态职责）。
    /// 1. 订阅切枪请求（WeaponSwitchRequestMessage）
    /// 2. 切换时设置 IWeapon.Current（决定哪个武器响应攻击请求）
    /// 3. 提供当前武器数据（WeaponData）给流水线下游（DamageCalculator）
    /// 挂在单独物体上（比如 GameManager 下），单例模式。
    /// </summary>
    public class SwitchWeapon : MonoBehaviour
    {
        public static SwitchWeapon Instance { get; private set; }

        // 所有武器脚本（通过GetComponentsInChildren找到，挂在子物体上）
        private IWeapon swordWeapon;
        private IWeapon pistolWeapon;
        private IWeapon sniperWeapon;

        private string currentWeaponID = "hand";
        private WeaponData currentWeaponData;

        /// <summary>当前手持武器ID</summary>
        public string CurrentWeaponID => currentWeaponID;

        private void Awake()
        {
            Instance = this;

            // 从子物体中查找武器脚本（SwitchWeapon下有Sword/Pistol/BurstSniper三个子物体）
            swordWeapon = GetComponentInChildren<SwordWeapon>(true);
            pistolWeapon = GetComponentInChildren<PistolWeapon>(true);
            sniperWeapon = GetComponentInChildren<SniperWeapon>(true);

            if (swordWeapon == null) Debug.LogWarning("[SwitchWeapon] 未找到SwordWeapon脚本，请挂在SwitchWeapon/Sword子物体上");
            if (pistolWeapon == null) Debug.LogWarning("[SwitchWeapon] 未找到PistolWeapon脚本，请挂在SwitchWeapon/Pistol子物体上");
            if (sniperWeapon == null) Debug.LogWarning("[SwitchWeapon] 未找到SniperWeapon脚本，请挂在SwitchWeapon/BurstSniper子物体上");

            // 默认武器：空手
            SwitchTo("hand");
        }

        private void OnEnable()
        {
            MessageBus.Subscribe<WeaponSwitchRequestMessage>(OnWeaponSwitchRequest);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<WeaponSwitchRequestMessage>(OnWeaponSwitchRequest);
        }

        /// <summary>收到切换武器请求</summary>
        private void OnWeaponSwitchRequest(WeaponSwitchRequestMessage msg)
        {
            SwitchTo(msg.WeaponID);
        }

        /// <summary>切换武器：更新手持武器状态 + 设置 IWeapon.Current + 广播切枪通知</summary>
        public void SwitchTo(string weaponID)
        {
            currentWeaponData = WeaponConfig.GetWeapon(weaponID);
            if (currentWeaponData == null)
            {
                Debug.LogWarning($"[SwitchWeapon] 未找到武器配置: {weaponID}，保持当前武器不变");
                return;
            }

            switch (weaponID)
            {
                case "hand":
                case "long_sword":
                    IWeapon.Current = swordWeapon;
                    break;
                case "pistol":
                    IWeapon.Current = pistolWeapon;
                    break;
                case "sniper":
                case "burst_sniper":
                    IWeapon.Current = sniperWeapon;
                    break;
                default:
                    IWeapon.Current = swordWeapon;
                    break;
            }

            currentWeaponID = weaponID;

            // 发布武器已切换通知（保持原行为：其他系统订阅此消息）
            MessageBus.Publish(new WeaponSwitchedMessage
            {
                WeaponID = weaponID,
                IsRanged = IWeapon.Current?.IsRanged ?? false,
                AttackSpeed = currentWeaponData.attackSpeed,
                Range = currentWeaponData.range,
                Damage = currentWeaponData.damage,
                CritChance = currentWeaponData.critChance,
                AttackAngle = currentWeaponData.attackAngle,
                BackAttackCooldownMultiplier = currentWeaponData.backAttackCooldownMultiplier,
                BackAttackAngleThreshold = currentWeaponData.backAttackAngleThreshold,
                AttackStopDuration = currentWeaponData.attackStopDuration
            });
        }

        /// <summary>检测玩家手持武器状态（流水线阶段2：提供给 DamageCalculator 计算伤害）</summary>
        public WeaponData GetCurrentWeaponData()
        {
            return currentWeaponData;
        }
    }
}
