using UnityEngine;
using System.Collections.Generic;
using Game.Core;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// 背包格自动排版：监听装备变化，按穿的背包生成对应数量的槽。
    /// 挂在BackpackAnchor空物体上。
    /// </summary>
    public class BackpackLayout : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private GameObject defaultEmptyText;
        [SerializeField] private int columns = 4;
        [SerializeField] private Vector2 cellSize = new Vector2(80, 80);
        [SerializeField] private Vector2 spacing = new Vector2(10, 10);
        [SerializeField] private Vector2 margin = new Vector2(80, 80);

        private readonly List<GameObject> spawned = new List<GameObject>();

        private void OnEnable()
        {
            MessageBus.Subscribe<EquipmentEquippedMessage>(OnEquip);
            Refresh();
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<EquipmentEquippedMessage>(OnEquip);
        }

        private void OnEquip(EquipmentEquippedMessage msg) => Refresh();

        private void Refresh()
        {
            int count = GetBackpackSlots();
            ClearSlots();
            InventoryDirector.Instance?.SetSlotCount(BagSlotType.BackPack, count);
            SpawnSlots(count);
            if (defaultEmptyText != null) defaultEmptyText.SetActive(count <= 0);
        }

        private int GetBackpackSlots()
        {
            int total = EquipmentConfig.BaseBackpackSlots;
            if (ItemDirector.Instance == null) return total;

            string backID = ItemDirector.Instance.GetEquipped(EquipmentSlotType.Back);
            if (!string.IsNullOrEmpty(backID))
            {
                var data = EquipmentConfig.GetEquipment(backID);
                if (data != null) total += data.backpackSlots;
            }
            return total;
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
                int row = i / columns;
                int col = i % columns;
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
                var slotUI = go.GetComponent<Game.Items.InventorySlotUI>();
                if (slotUI != null) slotUI.Setup(Game.Items.BagSlotType.BackPack, i);
            }
        }
    }
}
