using UnityEngine;
using Game.Core;
using Game.Player;

namespace Game.PlayerStats
{
    /// <summary>
    /// 移动属性应用：订阅PlayerStatsChangedMessage，把最终移速应用到PlayerMovement。
    /// 挂在Player物体上，需要引用PlayerMovement。
    /// </summary>
    public class PlayerMovementStatsApplier : MonoBehaviour
    {
        [SerializeField] private PlayerMovement playerMovement;  // 引用PlayerMovement

        private void OnEnable()
        {
            MessageBus.Subscribe<PlayerStatsChangedMessage>(OnStatsChanged);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<PlayerStatsChangedMessage>(OnStatsChanged);
        }

        private void Start()
        {
            // 自动找PlayerMovement（如果没拖引用）
            if (playerMovement == null)
            {
                playerMovement = GetComponent<PlayerMovement>();
            }
        }

        /// <summary>属性变化时应用到移动</summary>
        private void OnStatsChanged(PlayerStatsChangedMessage msg)
        {
            if (playerMovement == null) return;
            playerMovement.SetFinalSpeeds(msg.MoveSpeed, msg.RunSpeed);
        }
    }
}
