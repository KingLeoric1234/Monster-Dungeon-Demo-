using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// Single shop item UI: displays equipment info and handles buy button.
    /// Attach to each equipment item in the shop panel.
    /// </summary>
    public class ShopItemUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Image iconImage;

        [Header("Data")]
        [SerializeField] private string equipmentID;  // Set in inspector

        private EquipmentData data;
        private ShopLogic shopLogic;

        private void Start()
        {
            // Find ShopLogic in parent or scene
            shopLogic = FindObjectOfType<ShopLogic>();

            // Load equipment data
            if (!string.IsNullOrEmpty(equipmentID))
            {
                data = EquipmentConfig.GetEquipment(equipmentID);
                UpdateDisplay();
            }

            // Register buy button
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(OnBuyClicked);
            }
        }

        /// <summary>Set equipment ID and update display</summary>
        public void SetEquipment(string id)
        {
            equipmentID = id;
            data = EquipmentConfig.GetEquipment(id);
            UpdateDisplay();
        }

        /// <summary>Update UI display</summary>
        private void UpdateDisplay()
        {
            if (data == null) return;

            if (nameText != null) nameText.text = data.Name;
            if (descText != null) descText.text = BuildDescription(data);
            if (priceText != null) priceText.text = data.Price + " G";

            // Update button state
            UpdateButtonState();
        }

        /// <summary>Update buy button state (owned/not owned)</summary>
        private void UpdateButtonState()
        {
            if (buyButton == null) return;

            bool owned = ItemDirector.Instance != null && ItemDirector.Instance.OwnsEquipment(equipmentID);

            if (owned)
            {
                // Already owned: show "OWNED" and disable button
                var buttonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) buttonText.text = "OWNED";
                buyButton.interactable = false;
            }
            else
            {
                // Not owned: show "BUY" and enable button
                var buttonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) buttonText.text = "BUY";
                buyButton.interactable = true;
            }
        }

        /// <summary>Buy button clicked</summary>
        private void OnBuyClicked()
        {
            if (shopLogic == null || data == null) return;

            bool success = shopLogic.TryBuyEquipment(data);
            if (success)
            {
                UpdateButtonState();
            }
        }

        /// <summary>Build equipment description text</summary>
        private string BuildDescription(EquipmentData data)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (data.Armor > 0) parts.Add("Armor+" + data.Armor);
            if (data.MoveSpeedPenalty > 0) parts.Add("Speed-" + (data.MoveSpeedPenalty * 100) + "%");
            parts.Add("Weight " + data.Weight + "kg");
            return string.Join(" | ", parts);
        }

        /// <summary>Refresh when purchased (called by ShopPanel)</summary>
        public void Refresh()
        {
            UpdateButtonState();
        }
    }
}
