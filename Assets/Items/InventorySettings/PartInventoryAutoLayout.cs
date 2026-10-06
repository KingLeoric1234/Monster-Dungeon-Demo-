using UnityEngine;
using UnityEngine.UI;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// 自动生成并排版背包槽。
    /// 挂在背包Panel的容器物体上，拖入SlotPrefab。
    /// </summary>
    public class InventoryAutoLayout : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private InventorySlotUI slotPrefab;

        [Header("固定槽数量")]
        [SerializeField] private int headCount = 1;
        [SerializeField] private int handCount = 1;
        [SerializeField] private int armorCount = 2;

        [Header("BackPack数量")]
        [SerializeField] private int backpackCount = 8;
        [SerializeField] private int columns = 4;
        [SerializeField] private Vector2 cellSize = new Vector2(80, 80);
        [SerializeField] private Vector2 spacing = new Vector2(10, 10);

        private void Start()
        {
            SpawnSlots();
        }

        private void SpawnSlots()
        {
            int index = 0;
            // 简单两列布局：固定槽一行，背包一行
            SpawnRow(BagSlotType.Head, headCount, ref index, new Vector2(0, 200));
            SpawnRow(BagSlotType.Hand, handCount, ref index, new Vector2(0, 110));
            SpawnRow(BagSlotType.Armor, armorCount, ref index, new Vector2(0, 20));
            SpawnGrid(BagSlotType.BackPack, backpackCount, columns, new Vector2(0, -120));
        }

        private void SpawnRow(BagSlotType type, int count, ref int index, Vector2 startPos)
        {
            float totalWidth = count * (cellSize.x + spacing.x) - spacing.x;
            float startX = -totalWidth / 2f + cellSize.x / 2f;
            for (int i = 0; i < count; i++)
            {
                var slot = Instantiate(slotPrefab, transform);
                slot.Setup(type, i);
                var rt = slot.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(startX + i * (cellSize.x + spacing.x), startPos.y);
                rt.sizeDelta = cellSize;
            }
        }

        private void SpawnGrid(BagSlotType type, int count, int cols, Vector2 startPos)
        {
            float totalWidth = cols * (cellSize.x + spacing.x) - spacing.x;
            float startX = -totalWidth / 2f + cellSize.x / 2f;
            int rows = Mathf.CeilToInt((float)count / cols);
            for (int i = 0; i < count; i++)
            {
                int r = i / cols;
                int c = i % cols;
                var slot = Instantiate(slotPrefab, transform);
                slot.Setup(type, i);
                var rt = slot.GetComponent<RectTransform>();
                float x = startX + c * (cellSize.x + spacing.x);
                float y = startPos.y - r * (cellSize.y + spacing.y);
                rt.anchoredPosition = new Vector2(x, y);
                rt.sizeDelta = cellSize;
            }
        }
    }
}
