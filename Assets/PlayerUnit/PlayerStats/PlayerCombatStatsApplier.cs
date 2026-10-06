using UnityEngine;
using Game.Core;
using Game.Items;
using Game.Player;

namespace Game.PlayerStats
{
    /// <summary>
    /// 战斗属性应用：订阅固定槽变化，当HandUse槽的武器变化时，切换PlayerAttackAction的武器。
    /// 挂在Player物体上，需要引用PlayerAttackAction。
    /// </summary>
    public class PlayerCombatStatsApplier : MonoBehaviour
    {
        [SerializeField] private PlayerAttackAction playerAttack;  // 引用PlayerAttackAction

        private void OnEnable()
        {
            MessageBus.Subscribe<FixedSlotChangedMessage>(OnFixedSlotChanged);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<FixedSlotChangedMessage>(OnFixedSlotChanged);
        }

        private void Start()
        {
            // 自动找PlayerAttackAction（如果没拖引用）
            if (playerAttack == null)
            {
                playerAttack = GetComponent<PlayerAttackAction>();
            }

            // 初始化时同步一次武器
            SyncWeapon();
        }

        /// <summary>固定槽变化时检查是否是HandUse</summary>
        private void OnFixedSlotChanged(FixedSlotChangedMessage msg)
        {
            if (msg.Slot == FixedSlotType.HandUse)
            {
                SyncWeapon();
            }
        }

        /// <summary>同步武器到PlayerAttackAction</summary>
        private void SyncWeapon()
        {
            if (playerAttack == null || ItemDirector.Instance == null) return;

            var (itemType, itemID) = ItemDirector.Instance.GetFixedSlot(FixedSlotType.HandUse);
            if (itemType == FixedItemType.Weapon && !string.IsNullOrEmpty(itemID))
            {
                playerAttack.SwitchWeapon(itemID);
            }
            else
            {
                playerAttack.SwitchWeapon("hand"); // 空手
            }
        }
    }
}
