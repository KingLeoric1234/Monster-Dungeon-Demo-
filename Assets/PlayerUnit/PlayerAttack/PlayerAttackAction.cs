using UnityEngine;
using Game.Core;
using Game.Items;
using Player;

namespace Game.Player
{
    /// <summary>
    /// 玩家攻击的"行为入口"：只管 输入 + 攻击模式 + 把攻击请求发布给 Weapon。
    /// 不知道自己能打多快（问 AttackPhysiology）、不知道武器怎么打（发 PlayerAttackRequestMessage 给 WeaponDirector）。
    /// 挂在 Player 物体上，与 AttackPhysiology 成对存在。
    /// </summary>
    public class PlayerAttackAction : MonoBehaviour
    {
        private AttackPhysiology physiology;   // 生理限制：节律/减速/硬直（Awake 自动抓取）

        private bool canControl;
        private KnockBackHandler knockBack;
        private Camera mainCamera;

        /// <summary>当前是否处于攻击模式（广播给 CustomCursor 等响应）</summary>
        public bool IsAttackMode { get; private set; }

        private void Awake()
        {
            knockBack = GetComponent<KnockBackHandler>();
            physiology = GetComponent<AttackPhysiology>();
            mainCamera = Camera.main;
            IsAttackMode = false;
        }

        private void Update()
        {
            if (!canControl) return;
            if (knockBack != null && knockBack.IsKnockedBack) return;

            // 按E切换攻击模式
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleAttackMode();
            }

            // 攻击模式下按左键 → 问生理限制"身体能不能打"
            if (IsAttackMode && Input.GetMouseButtonDown(0))
            {
                if (physiology != null && physiology.CanAttack())
                {
                    DoAttack();
                }
            }
        }

        /// <summary>执行攻击：算方向 → 发布请求给 Weapon → 通知生理限制开始计节律</summary>
        private void DoAttack()
        {
            // 攻击方向朝向鼠标
            Vector2 mousePos = (mainCamera != null ? mainCamera : Camera.main).ScreenToWorldPoint(Input.mousePosition);
            Vector2 attackDir = (mousePos - (Vector2)transform.position).normalized;
            if (attackDir == Vector2.zero)
            {
                attackDir = Vector2.right;  // 鼠标正好压在玩家身上：给默认方向，不吞掉本次攻击
            }

            // 发布攻击请求（Player 域单向消息），WeaponDirector 订阅后执行具体攻击
            PlayerMessageBus.Publish(new PlayerAttackRequestMessage
            {
                Direction = attackDir,
                PlayerPosition = transform.position,
                MousePosition = mousePos
            });

            // 通知生理限制：一次攻击已发生（开始计节律 + 施加减速）
            if (physiology != null)
            {
                physiology.OnAttackPerformed();
            }
        }

        // ── 攻击模式 ──────────────────────────────

        private void ToggleAttackMode()
        {
            if (IsAttackMode)
            {
                ExitAttackMode();
            }
            else
            {
                EnterAttackMode();
            }
        }

        private void EnterAttackMode()
        {
            IsAttackMode = true;
            // 进模式时清节律：刚打完也能立刻进模式开打
            if (physiology != null) physiology.ResetCooldown();
            MessageBus.Publish(new AttackModeChangedMessage { IsAttackMode = true });
        }

        /// <summary>
        /// 退出攻击模式（公开接口）。
        /// 近战"砍完一刀退出" / 远程"留在模式连射"是武器策略，由 Weapon 在处理攻击后自行决定是否调用本方法。
        /// </summary>
        public void ExitAttackMode()
        {
            IsAttackMode = false;
            MessageBus.Publish(new AttackModeChangedMessage { IsAttackMode = false });
        }

        // ── 公共接口 ──────────────────────────────

        public void EnableControl() => canControl = true;
        public void DisableControl() => canControl = false;

        /// <summary>
        /// 切换武器（发布请求，WeaponDirector 订阅处理）。
        /// 临时保留在攻击入口作为对外便捷接口，后续可随武器管理迁移。
        /// </summary>
        public void SwitchWeapon(string weaponID)
        {
            MessageBus.Publish(new WeaponSwitchRequestMessage { WeaponID = weaponID });
        }
    }
}
