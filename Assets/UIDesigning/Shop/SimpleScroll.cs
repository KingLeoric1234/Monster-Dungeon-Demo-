using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Simple scroll: mouse wheel changes panel position. No ScrollView needed.
    /// Attach to the big panel that holds all equipment items.
    /// </summary>
    public class SimpleScroll : MonoBehaviour
    {
        [Header("Scroll Settings")]
        [SerializeField] private float scrollSpeed = 20f;    // How fast to scroll
        [SerializeField] private float minY = -500f;          // Lowest position (bottom)
        [SerializeField] private float maxY = 0f;             // Highest position (top)

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            // Get mouse wheel input
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll != 0f)
            {
                // Move panel up/down
                Vector2 pos = rectTransform.anchoredPosition;
                pos.y += scroll * scrollSpeed * 10f;

                // Clamp to range
                pos.y = Mathf.Clamp(pos.y, minY, maxY);

                rectTransform.anchoredPosition = pos;
            }
        }
    }
}
