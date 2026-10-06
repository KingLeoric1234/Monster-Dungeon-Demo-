using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Player stamina (PP) change message, UI subscribes to update stamina bar.
    /// </summary>
    public class PlayerStaminaChangedMessage : GameMessage
    {
        public float CurrentStamina;
        public float MaxStamina;
        public bool IsSprinting;
    }
}
