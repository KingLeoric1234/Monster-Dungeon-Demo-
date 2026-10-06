using UnityEngine;
using Game.Items;

namespace Game.Weapons
{
    /// <summary>
    /// 攻击流水线控制器：控制"攻击请求 → 武器执行 → 武器状态 → 伤害计算 → 伤害输出"整条 Pipeline。
    /// 挂在单独物体上（比如 GameManager 下），单例模式。
    ///
    /// 流水线阶段：
    /// 1. IWeapon（当前手持武器）收到 PlayerAttackRequestMessage，执行 Attack（命中检测等武器自身逻辑）
    /// 2. 命中后调用本控制器 → 传唤 SwitchWeapon 检测玩家手持武器状态
    /// 3. 传唤 DamageCalculator 获得武器状态并计算伤害
    /// 4. 伤害输出（接口封死中，见 DamageCalculator.OutputDamage）
    /// </summary>
    public class PipelineController : MonoBehaviour
    {
        public static PipelineController Instance { get; private set; }

        /// <summary>武器状态源（GetComponent：与 PipelineController 挂载在同一物体上）</summary>
        private SwitchWeapon switchWeapon;

        private void Awake()
        {
            Instance = this;
            // GetComponent：SwitchWeapon 需与 PipelineController 挂在同一个物体上
            switchWeapon = GetComponent<SwitchWeapon>();
            if (switchWeapon == null)
            {
                Debug.LogWarning("[PipelineController] 未抓到 SwitchWeapon 组件，攻击流水线无法获得武器状态。请将 SwitchWeapon 与 PipelineController 挂在同一物体上。");
            }
        }

        /// <summary>
        /// 攻击流水线入口：武器命中目标后调用。
        /// 阶段2 → 传唤 SwitchWeapon 检测手持武器状态
        /// 阶段3 → 传唤 DamageCalculator 计算伤害
        /// 阶段4 → 伤害输出（接口封死中）
        /// </summary>
        /// <param name="target">被命中的目标（Enemy）</param>
        /// <param name="direction">攻击方向（击退/特效用）</param>
        public void RunAttackPipeline(GameObject target, Vector2 direction)
        {
            // ===== 阶段2：传唤 SwitchWeapon，检测玩家手持武器状态 =====
            WeaponData weapon = switchWeapon != null ? switchWeapon.GetCurrentWeaponData() : null;
            if (weapon == null)
            {
                Debug.LogWarning("[PipelineController] 未获得当前武器状态，攻击流水线中断（检查 SwitchWeapon 挂载与武器配置）");
                return;
            }

            // ===== 阶段3：传唤 DamageCalculator，获得武器状态并计算伤害 =====
            var (damage, isCrit) = DamageCalculator.Calculate(weapon.damage, weapon.critChance);

            // ===== 阶段4：伤害输出（接口封死，未链接下游）=====
            DamageCalculator.OutputDamage(target, direction, damage, isCrit);
        }
    }
}
