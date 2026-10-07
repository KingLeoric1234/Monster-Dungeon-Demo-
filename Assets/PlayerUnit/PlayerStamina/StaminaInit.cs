using UnityEngine;
using Game.Core; // PlayerConfig

namespace Game.Player
{
    /// <summary>
    /// Stamina initialization (orchestration): runs once on login / revive / load.
    /// Only decides what the initial value is (current rule: full stamina),
    /// then pushes the number into StaminaCache, which clamps + broadcasts.
    /// Future revive rules (half stamina, loaded save) only change ResetToFull.
    /// Attach to the player object (same GameObject as StaminaCache).
    /// </summary>
    public class StaminaInit : MonoBehaviour
    {
        private StaminaCache cache;

        private void Awake()
        {
            cache = GetComponent<StaminaCache>();
        }

        private void Start()
        {
            // Every login: full stamina
            ResetToFull();
        }

        /// <summary>Full stamina (login / revive / load all go through here)</summary>
        public void ResetToFull()
        {
            if (cache != null) cache.SetCurrentStamina(PlayerConfig.MaxStamina);
        }
    }
}
