using UnityEngine;
using System.Collections.Generic;
using Game.Core;
using Game.Items;

namespace Game.UI
{
    /// <summary>头部饰品槽排版：根据穿的头盔accessorySlots生成</summary>
    public class HeadSlotLayout : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private GameObject defaultEmptyText;
        [SerializeField] private int columns = 2;
        [SerializeField] private Vector2 cellSize = new Vector2(60, 60);
        [SerializeField] private Vector2 spacing = new Vector2(8, 8);
        [SerializeField] private Vector2 margin = new Vector2(0, 0);

        private readonly List<GameObject> spawned = new List<GameObject>();

        private void OnEnable()
        {
            MessageBus.Subscribe<EquipmentEquippedMessage>(OnEquip);
            Refresh();
        }

        private void OnDisable() => MessageBus.Unsubscribe<EquipmentEquippedMessage>(OnEquip);

        private void OnEquip(EquipmentEquippedMessage msg) => Refresh();

        private void Refresh()
        {
            int total = 0;
            if (ItemDirector.Instance != null)
            {
                string id = ItemDirector.Instance.GetEquipped(EquipmentSlotType.Head);
                if (!string.IsNullOrEmpty(id))
                {
                    var data = EquipmentConfig.GetEquipment(id);
                    if (data != null) total = data.accessorySlots;
                }
            }
            ClearSlots();
            InventoryDirector.Instance?.SetSlotCount(BagSlotType.Head, total);
            SpawnSlots(total);
            if (defaultEmptyText != null) defaultEmptyText.SetActive(total <= 0);
        }

        private void ClearSlots()
        {
            foreach (var s in spawned) Destroy(s);
            spawned.Clear();
        }

        private void SpawnSlots(int count)
        {
            if (count <= 0 || slotPrefab == null) return;
            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(slotPrefab, transform);
                int row = i / columns, col = i % columns;
                float x = col * (cellSize.x + spacing.x);
                float y = -row * (cellSize.y + spacing.y);
                var rt = go.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0, 1);
                    rt.anchorMax = new Vector2(0, 1);
                    rt.pivot = new Vector2(0, 1);
                    rt.anchoredPosition = new Vector2(x + margin.x, y - margin.y);
                    rt.sizeDelta = cellSize;
                }
                spawned.Add(go);
                var slotUI = go.GetComponent<InventorySlotUI>();
                if (slotUI != null) slotUI.Setup(BagSlotType.Head, i);
            }
        }
    }
}
