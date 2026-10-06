using UnityEngine;
using Game.Core;
using Game.Items;
using Game.Player;

namespace Game.Weapons
{
    /// <summary>
    /// 近战武器（空手/长剑）：扇形范围检测，挥砍特效。
    /// 挂在玩家身上，由 SwitchWeapon 设为当前手持武器。
    /// </summary>
    public class SwordWeapon : IWeapon
    {
        [Header("引用")]
        [SerializeField] private AttackEffect attackEffect;       // 攻击特效
        [SerializeField] private SpriteRenderer playerSprite;     // 玩家默认图像（攻击时隐藏）

        [Header("辅助锁定")]
        [SerializeField] private float assistLockRange = 0.5f;   // 辅助锁定范围
        [SerializeField] private bool enableAssistLock = true;    // 是否开启辅助锁定

        public override bool IsRanged => false;

        /// <summary>执行近战攻击</summary>
        public override void Attack(Vector2 playerPosition, Vector2 direction, Vector2 mousePosition)
        {
            WeaponData currentWeapon = SwitchWeapon.Instance?.GetCurrentWeaponData();
            if (currentWeapon == null) return;

            // 播放攻击特效
            if (attackEffect != null)
            {
                attackEffect.Play(direction);

                // 攻击时隐藏玩家默认图像
                if (playerSprite != null)
                {
                    playerSprite.enabled = false;
                    float animDuration = attackEffect.GetAnimationDuration();
                    Invoke(nameof(ShowPlayerSprite), animDuration);
                }
            }

            // 扇形范围检测
            float attackRange = currentWeapon.range;
            Collider2D[] hits = Physics2D.OverlapCircleAll(playerPosition, attackRange);
            float halfAngle = PlayerConfig.AttackAngle / 2f;

            foreach (Collider2D hit in hits)
            {
                if (!hit.CompareTag("Enemy")) continue;

                Vector2 toTarget = ((Vector2)hit.transform.position - playerPosition).normalized;
                float angle = Vector2.Angle(direction, toTarget);

                bool inSector = angle <= halfAngle;
                bool inAssistRange = enableAssistLock && IsInAssistLockRange(playerPosition, hit.transform);

                if (inSector || inAssistRange)
                {
                    // 交给流水线：传唤 SwitchWeapon 检测武器状态 → 传唤 DamageCalculator 算伤害 → 输出（接口封死中）
                    if (PipelineController.Instance != null)
                    {
                        PipelineController.Instance.RunAttackPipeline(hit.gameObject, direction);
                    }
                    else
                    {
                        Debug.LogWarning("[SwordWeapon] 未找到 PipelineController，伤害未输出");
                    }
                }
            }

            // 攻击音效
            MessageBus.Publish(new PlaySoundMessage { Type = SoundType.PlayerAttack });
        }

        /// <summary>辅助锁定检测</summary>
        private bool IsInAssistLockRange(Vector2 playerPosition, Transform target)
        {
            float dist = Vector2.Distance(playerPosition, target.position);
            return dist <= assistLockRange;
        }

        /// <summary>恢复玩家默认图像</summary>
        private void ShowPlayerSprite()
        {
            if (playerSprite != null)
            {
                playerSprite.enabled = true;
            }
        }
    }
}
