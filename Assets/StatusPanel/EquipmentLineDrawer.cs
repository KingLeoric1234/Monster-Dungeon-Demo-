using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// 装备折线管理器：从玩家Sprite的对应部位画一条L形折线到对应装备槽。
    /// 4条线：头部→头槽、手部→手槽、身子→护甲槽、背包→背包槽。
    /// 折线形状：先水平从玩家部位伸出，再垂直走到槽（L形，像电路图一样）。
    /// 槽的位置在Inspector手动调整，脚本自动计算连线。
    /// </summary>
    public class EquipmentLineDrawer : MonoBehaviour
    {
        [Header("玩家部位锚点（在玩家Sprite上建空物体标记位置）")]
        [SerializeField] private RectTransform headAnchor;    // 脑门位置
        [SerializeField] private RectTransform handAnchor;    // 手的位置
        [SerializeField] private RectTransform bodyAnchor;    // 身子/护甲位置
        [SerializeField] private RectTransform backAnchor;    // 背包位置（原腿槽改为背包）

        [Header("装备槽引用")]
        [SerializeField] private RectTransform headSlot;      // 头部槽
        [SerializeField] private RectTransform handSlot;      // 手部槽
        [SerializeField] private RectTransform bodySlot;      // 身子/护甲槽
        [SerializeField] private RectTransform backSlot;      // 背包槽

        [Header("折线设置")]
        [SerializeField] private GameObject linePrefab;        // 线的预制体（细的Image）
        [SerializeField] private float lineWidth = 2f;         // 线宽
        [SerializeField] private Color lineColor = new Color(1f, 1f, 1f, 0.5f);  // 线颜色（半透明白）

        // 每条折线存两个线段：水平段和垂直段（L形）
        private Dictionary<EquipmentSlotType, RectTransform> horizontalLines = new Dictionary<EquipmentSlotType, RectTransform>();
        private Dictionary<EquipmentSlotType, RectTransform> verticalLines = new Dictionary<EquipmentSlotType, RectTransform>();

        private void Start()
        {
            CreateLines();
            UpdateAllLines();
        }

        private void Update()
        {
            // 每帧更新线的位置（面板滑入滑出时线也跟着动）
            UpdateAllLines();
        }

        /// <summary>创建4条折线（每条2段：水平+垂直）</summary>
        private void CreateLines()
        {
            if (linePrefab == null) return;

            CreateLine(EquipmentSlotType.Head, headAnchor, headSlot);
            CreateLine(EquipmentSlotType.Hand, handAnchor, handSlot);
            CreateLine(EquipmentSlotType.Body, bodyAnchor, bodySlot);
            CreateLine(EquipmentSlotType.Back, backAnchor, backSlot);
        }

        /// <summary>创建一条折线（水平段+垂直段）</summary>
        private void CreateLine(EquipmentSlotType type, RectTransform start, RectTransform slot)
        {
            if (start == null || slot == null) return;

            // 水平段
            GameObject hLine = Instantiate(linePrefab, transform);
            hLine.name = $"Line_{type}_H";
            RectTransform hRect = hLine.GetComponent<RectTransform>();
            if (hRect == null) hRect = hLine.AddComponent<RectTransform>();
            Image hImg = hLine.GetComponent<Image>();
            if (hImg != null) hImg.color = lineColor;
            horizontalLines[type] = hRect;

            // 垂直段
            GameObject vLine = Instantiate(linePrefab, transform);
            vLine.name = $"Line_{type}_V";
            RectTransform vRect = vLine.GetComponent<RectTransform>();
            if (vRect == null) vRect = vLine.AddComponent<RectTransform>();
            Image vImg = vLine.GetComponent<Image>();
            if (vImg != null) vImg.color = lineColor;
            verticalLines[type] = vRect;
        }

        /// <summary>更新所有折线</summary>
        private void UpdateAllLines()
        {
            UpdateLine(EquipmentSlotType.Head, headAnchor, headSlot);
            UpdateLine(EquipmentSlotType.Hand, handAnchor, handSlot);
            UpdateLine(EquipmentSlotType.Body, bodyAnchor, bodySlot);
            UpdateLine(EquipmentSlotType.Back, backAnchor, backSlot);
        }

        /// <summary>更新一条L形折线（先水平从玩家部位伸出，再垂直走到槽）</summary>
        private void UpdateLine(EquipmentSlotType type, RectTransform start, RectTransform slot)
        {
            if (!horizontalLines.ContainsKey(type) || horizontalLines[type] == null) return;
            if (!verticalLines.ContainsKey(type) || verticalLines[type] == null) return;
            if (start == null || slot == null) return;

            RectTransform endRect = slot;

            // 起点（玩家部位）和终点（槽）的世界位置
            Vector2 startPos = start.position;
            Vector2 endPos = endRect.position;

            // 拐点：(起点.x, 终点.y)，先水平再垂直（L形）
            Vector2 cornerPos = new Vector2(startPos.x, endPos.y);

            // 水平段：从起点到拐点
            UpdateLineSegment(horizontalLines[type], startPos, cornerPos);

            // 垂直段：从拐点到终点
            UpdateLineSegment(verticalLines[type], cornerPos, endPos);
        }

        /// <summary>更新一条线段的位置、长度、旋转</summary>
        private void UpdateLineSegment(RectTransform lineRect, Vector2 startPos, Vector2 endPos)
        {
            // 中点
            Vector2 midPos = (startPos + endPos) / 2f;
            lineRect.position = midPos;

            // 长度
            float distance = Vector2.Distance(startPos, endPos);
            lineRect.sizeDelta = new Vector2(distance, lineWidth);

            // 旋转角度
            float angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg;
            lineRect.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}

