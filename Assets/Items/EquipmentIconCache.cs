using UnityEngine;
using System.Collections.Generic;

namespace Game.Items
{
    /// <summary>
    /// Caches equipment icons so shop and status panel share the same sprites.
    /// Shop sets icons when loading, status panel reads from here.
    /// </summary>
    public static class EquipmentIconCache
    {
        private static Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();
        private static Dictionary<string, Color> colorCache = new Dictionary<string, Color>();

        /// <summary>Store an icon and its color for an equipment ID</summary>
        public static void SetIcon(string equipmentID, Sprite icon, Color color)
        {
            if (!string.IsNullOrEmpty(equipmentID) && icon != null)
            {
                iconCache[equipmentID] = icon;
                colorCache[equipmentID] = color;
            }
        }

        /// <summary>Get icon for an equipment ID (returns null if not found)</summary>
        public static Sprite GetIcon(string equipmentID)
        {
            if (string.IsNullOrEmpty(equipmentID)) return null;
            return iconCache.TryGetValue(equipmentID, out var icon) ? icon : null;
        }

        /// <summary>Get icon color for an equipment ID (returns white if not found)</summary>
        public static Color GetIconColor(string equipmentID)
        {
            if (string.IsNullOrEmpty(equipmentID)) return Color.white;
            return colorCache.TryGetValue(equipmentID, out var color) ? color : Color.white;
        }

        /// <summary>Check if icon exists for an equipment ID</summary>
        public static bool HasIcon(string equipmentID)
        {
            return !string.IsNullOrEmpty(equipmentID) && iconCache.ContainsKey(equipmentID);
        }
    }
}
