using UnityEngine;
using Game.Core;

namespace Game.Player
{
    /// <summary>
    /// 玩家攻击的"生理限制"：管 Player 身体能不能发起下一次攻击。
    /// 只负责：动作节律（0.5s 生理上限）、攻击减速（出招站定）、受击硬直（被打飞冻结）。
    /// 不知道输入、不知道攻击模式、不知道武器存在——纯身体状态，谁都能问。
    /// 挂在 Player 物体上，与 PlayerAttackAction 成对存在。
    /// </summary>
    public class AttackPhysiology : MonoBehaviour
    {
        private PlayerMovement playerMovement;   // 玩家移动（用于攻击减速，Awake 自动抓取）

        // 受击硬直（被打飞时身体状态冻结）
        private KnockBackHandler knockBack;

        // 动作节律状态：一次攻击后要等 PlayerConfig.AttackCooldown 才能再打
        private float cooldownTimer;

        // 攻击减速状态：出招时身体前倾、移速下降，指数恢复
        private bool isAttackSlowed = false;
        private float originalSpeed;

        private void Awake()
        {
            knockBack = GetComponent<KnockBackHandler>();

            playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogWarning("[AttackPhysiology] 未抓到 PlayerMovement 组件，攻击减速将不生效。请确认 PlayerMovement 已挂在 Player 物体上。");
            }
        }

        private void Update()
        {
            // 受击硬直：被打飞时身体节律冻结（冷却计时、减速恢复都暂停，击退结束继续）
            if (knockBack != null && knockBack.IsKnockedBack) return;

            // 动作节律递减
            if (cooldownTimer > 0)
                cooldownTimer -= Time.deltaTime;

            // 攻击减速恢复（指数型趋近原速，阈值内视为恢复完成）
            if (isAttackSlowed && playerMovement != null)
            {
                float currentSpeed = playerMovement.GetCurrentSpeed();
                float newSpeed = Mathf.Lerp(currentSpeed, originalSpeed, PlayerConfig.AttackSlowLerp * Time.deltaTime);
                playerMovement.SetForcedSpeed(newSpeed);
                if (Mathf.Abs(newSpeed - originalSpeed) < PlayerConfig.AttackSlowThreshold)
                {
                    isAttackSlowed = false;
                    playerMovement.ClearForcedSpeed();
                }
            }
        }

        /// <summary>
        /// 身体现在能不能发起攻击？
        /// 节律就绪（冷却结束）&& 未被击退。只管 Player 自身，不含任何武器状态。
        /// </summary>
        public bool CanAttack()
        {
            if (knockBack != null && knockBack.IsKnockedBack) return false;
            return cooldownTimer <= 0;
        }

        /// <summary>
        /// 一次攻击已发生（由 PlayerAttackAction 在发布攻击请求后调用）：
        /// 1. 启动动作节律——攻击间隔 = PlayerConfig.AttackCooldown，Player 的生理上限，与武器无关
        /// 2. 施加减速——出招站定
        /// </summary>
        public void OnAttackPerformed()
        {
            // 启动动作节律
            cooldownTimer = PlayerConfig.AttackCooldown;

            // 攻击减速
            if (playerMovement != null)
            {
                originalSpeed = playerMovement.GetCurrentSpeed();
                if (originalSpeed > PlayerConfig.AttackSlowSpeed)
                {
                    isAttackSlowed = true;
                    playerMovement.SetForcedSpeed(PlayerConfig.AttackSlowSpeed);
                }
            }
        }

        /// <summary>强制清除节律（进入攻击模式 / 重置玩家时调用）</summary>
        public void ResetCooldown() => cooldownTimer = 0;
    }
}
