using UnityEngine;
using Game.Core;
using Game.Items;
using Game.UI;

namespace Game.Player
{
    /// <summary>
    /// 狙击镜控制器。
    /// 挂在WeaponDirector物体上，跟武器相关逻辑放一起。
    /// 检测玩家是否装备了狙击枪，右键打开/关闭狙击镜。
    /// 不依赖玩家物体，通过MessageBus检测固定槽变化。
    /// </summary>
    public class SniperScopeController : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private SniperScopeUI sniperScopeUI;  // 狙击镜UI（在Canvas下）

        private bool hasSniper = false;

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
            // 初始化时检查一次
            CheckSniperEquipped();
        }

        private void Update()
        {
            // 只有装备了狙击枪才能开镜
            if (!hasSniper) return;

            // 右键切换狙击镜
            if (Input.GetMouseButtonDown(1))
            {
                if (sniperScopeUI != null)
                {
                    sniperScopeUI.ToggleScope();
                }
            }
        }

        /// <summary>固定槽变化时检查是否装备了狙击枪</summary>
        private void OnFixedSlotChanged(FixedSlotChangedMessage msg)
        {
            if (msg.Slot == FixedSlotType.HandUse)
            {
                CheckSniperEquipped();
            }
        }

        /// <summary>检查HandUse槽是否装备了狙击枪</summary>
        private void CheckSniperEquipped()
        {
            if (ItemDirector.Instance == null) return;

            var (itemType, itemID) = ItemDirector.Instance.GetFixedSlot(FixedSlotType.HandUse);
            hasSniper = (itemType == FixedItemType.Weapon && itemID == "burst_sniper");

            // 如果卸下了狙击枪，关闭狙击镜
            if (!hasSniper && sniperScopeUI != null && sniperScopeUI.IsScopeOpen)
            {
                sniperScopeUI.CloseScope();
            }
        }
    }
}
