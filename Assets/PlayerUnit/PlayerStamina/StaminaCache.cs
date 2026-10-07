using UnityEngine;
using Game.Core; // PlayerConfig
using Player;    // PlayerStaminaChangedMessage + PlayerMessageBus（UI 通知走 Player 域总线）

namespace Game.Player
{
    /// <summary>
    /// Stamina ledger (data layer): sole authority over currentStamina.
    /// Rule: nobody touches the field directly - init goes through SetCurrentStamina,
    /// the sprint flag through SetSprinting. Drain/regen is maintained here every
    /// frame, and every change is clamped + broadcast once via PlayerMessageBus.
    /// Attach to the player object. Called one-way by StaminaInit / CheckSprint.
    /// </summary>
    public class StaminaCache : MonoBehaviour
    {
        private float currentStamina;
        private float regenTimer;   // Time since last sprint
        private bool isSprinting;
        private bool isLocked;      // Locked after dropping below 2%

        public float CurrentStamina => currentStamina;

        /// <summary>Is stamina full?</summary>
        public bool IsFull => currentStamina >= PlayerConfig.MaxStamina;

        /// <summary>Is currently locked (below 2% and waiting to recover)?</summary>
        public bool IsLocked => isLocked;

        /// <summary>
        /// Initialize / load / revive: write the current stamina directly
        /// (no add/sub semantics). Resets lock and regen timer, then clamps + notifies once.
        /// </summary>
        public void SetCurrentStamina(float value)
        {
            currentStamina = value;
            isLocked = false;
            regenTimer = 0f;
            ClampAndNotify();
        }

        /// <summary>Flip sprint state. CheckSprint decides permission, this only applies it.</summary>
        public void SetSprinting(bool sprinting)
        {
            isSprinting = sprinting;
        }

        private void Update()
        {
            if (isSprinting)
            {
                // Drain stamina
                currentStamina -= PlayerConfig.StaminaDrainRate * Time.deltaTime;
                regenTimer = 0f;

                // Force stop if below minimum threshold
                if (currentStamina <= PlayerConfig.MaxStamina * PlayerConfig.StaminaMinThreshold)
                {
                    isLocked = true;
                    isSprinting = false;
                }
            }
            else
            {
                // Wait for regen delay, then regen
                regenTimer += Time.deltaTime;
                if (regenTimer >= PlayerConfig.StaminaRegenDelay)
                {
                    currentStamina += PlayerConfig.StaminaRegenRate * Time.deltaTime;
                }

                // Unlock when recovered above threshold
                if (isLocked && currentStamina >= PlayerConfig.MaxStamina * PlayerConfig.StaminaRecoverThreshold)
                {
                    isLocked = false;
                }
            }

            ClampAndNotify();
        }

        /// <summary>Clamp + broadcast (single exit for every change)</summary>
        private void ClampAndNotify()
        {
            if (currentStamina < 0) currentStamina = 0;
            if (currentStamina > PlayerConfig.MaxStamina) currentStamina = PlayerConfig.MaxStamina;

            PlayerMessageBus.Publish(new PlayerStaminaChangedMessage
            {
                CurrentStamina = currentStamina,
                MaxStamina = PlayerConfig.MaxStamina,
                IsSprinting = isSprinting
            });
        }
    }
}
