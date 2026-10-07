using UnityEngine;
using Game.Core; // PlayerConfig

namespace Game.Player
{
    /// <summary>
    /// Sprint permission & control. Call StartSprinting / StopSprinting from
    /// movement code; CanSprint decides whether sprinting is allowed.
    /// Reads stamina state from StaminaCache (same GameObject),
    /// writes the sprint flag back through it.
    /// </summary>
    public class CheckSprint : MonoBehaviour
    {
        private StaminaCache cache;

        private void Awake()
        {
            cache = GetComponent<StaminaCache>();
        }

        /// <summary>Call when player starts sprinting (must be moving + shift)</summary>
        public void StartSprinting()
        {
            if (CanSprint())
            {
                cache.SetSprinting(true);
            }
        }

        /// <summary>Call when player stops sprinting</summary>
        public void StopSprinting()
        {
            cache.SetSprinting(false);
        }

        /// <summary>Check if player can sprint</summary>
        public bool CanSprint()
        {
            if (cache.IsLocked)
            {
                // Locked: must recover above 12% to unlock
                return cache.CurrentStamina >= PlayerConfig.MaxStamina * PlayerConfig.StaminaRecoverThreshold;
            }
            else
            {
                // Not locked: just need above 2%
                return cache.CurrentStamina > PlayerConfig.MaxStamina * PlayerConfig.StaminaMinThreshold;
            }
        }
    }
}
