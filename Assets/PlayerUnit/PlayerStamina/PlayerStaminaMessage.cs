namespace Player
{
    /// <summary>
    /// Player stamina (PP) change message: StaminaCache publishes, StaminaBar / PP UI subscribe to refresh.
    /// Lives in the Player domain, travels on PlayerMessageBus - no longer touches the global bus.
    /// Reserved interface for StaminaBar.
    /// </summary>
    public class PlayerStaminaChangedMessage : PlayerMessage
    {
        public float CurrentStamina;
        public float MaxStamina;
        public bool IsSprinting;
    }
}
