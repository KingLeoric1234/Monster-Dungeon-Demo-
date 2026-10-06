using UnityEngine;
using Game.Core;

namespace Game.Player
{
    /// <summary>
    /// Player stamina (PP) management: drain when sprinting, regen when not.
    /// Lock mechanism: below 2% forces stop, must recover to 12% to sprint again.
    /// Publishes PlayerStaminaChangedMessage for UI to follow.
    /// Attach to player object.
    /// </summary>
    public class PlayerStamina : MonoBehaviour
    {
        private float currentStamina;
        private float regenTimer;          // Time since last sprint
        private bool isSprinting;
        private bool isLocked;             // Locked after dropping below 2%

        private void Awake()
        {
            currentStamina = PlayerConfig.MaxStamina;
            isLocked = false;
        }

        private void Update()
        {
            if (isSprinting)
            {
                // Drain stamina
                currentStamina -= PlayerConfig.StaminaDrainRate * Time.deltaTime;
                if (currentStamina < 0) currentStamina = 0;
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
                    if (currentStamina > PlayerConfig.MaxStamina)
                        currentStamina = PlayerConfig.MaxStamina;
                }

                // Unlock when recovered above threshold
                if (isLocked && currentStamina >= PlayerConfig.MaxStamina * PlayerConfig.StaminaRecoverThreshold)
                {
                    isLocked = false;
                }
            }

            // Publish change
            MessageBus.Publish(new PlayerStaminaChangedMessage
            {
                CurrentStamina = currentStamina,
                MaxStamina = PlayerConfig.MaxStamina,
                IsSprinting = isSprinting
            });
        }

        /// <summary>Call when player starts sprinting (must be moving + shift)</summary>
        public void StartSprinting()
        {
            if (CanSprint())
            {
                isSprinting = true;
            }
        }

        /// <summary>Call when player stops sprinting</summary>
        public void StopSprinting()
        {
            isSprinting = false;
        }

        /// <summary>Check if player can sprint</summary>
        public bool CanSprint()
        {
            if (isLocked)
            {
                // Locked: must recover above 12% to unlock
                return currentStamina >= PlayerConfig.MaxStamina * PlayerConfig.StaminaRecoverThreshold;
            }
            else
            {
                // Not locked: just need above 2%
                return currentStamina > PlayerConfig.MaxStamina * PlayerConfig.StaminaMinThreshold;
            }
        }

        /// <summary>Current stamina value</summary>
        public float CurrentStamina => currentStamina;

        /// <summary>Is stamina full?</summary>
        public bool IsFull => currentStamina >= PlayerConfig.MaxStamina;

        /// <summary>Is currently locked (below 2% and waiting to recover)?</summary>
        public bool IsLocked => isLocked;
    }
}
