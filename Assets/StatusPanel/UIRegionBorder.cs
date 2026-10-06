using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// UI区域自动描边：挂在RightPanel上，拖入各个区域，自动给每个区域画边框。
    /// 你只需要把区域拉伸到正确的位置和大小，脚本自动描四条边。
    /// </summary>
    public class UIRegionBorder : MonoBehaviour
    {
        [Header("区域引用（拖入对应的RectTransform）")]
        [SerializeField] private RectTransform headSlot;        // 头槽
        [SerializeField] private RectTransform handSlot;        // 手槽
        [SerializeField] private RectTransform backpackGrid;    // 背包格子
        [SerializeField] private RectTransform armorSlot;       // 护甲槽
        [SerializeField] private RectTransform safeBox;         // 暗格（可选）

        [Header("边框样式")]
        [SerializeField] private Color borderColor = new Color(1f, 1f, 1f, 0.3f);  // 半透明白
        [SerializeField] private float borderThickness = 2f;                       // 边框粗细

        private void Start()
        {
            DrawAllBorders();
        }

        /// <summary>给所有区域画边框</summary>
        private void DrawAllBorders()
        {
            if (headSlot != null) DrawBorder(headSlot, "Border_Head");
            if (handSlot != null) DrawBorder(handSlot, "Border_Hand");
            if (backpackGrid != null) DrawBorder(backpackGrid, "Border_Backpack");
            if (armorSlot != null) DrawBorder(armorSlot, "Border_Armor");
            if (safeBox != null) DrawBorder(safeBox, "Border_SafeBox");
        }

        /// <summary>给一个区域画四条边</summary>
        private void DrawBorder(RectTransform target, string name)
        {
            // 上边
            CreateBorderLine(target, name + "_Top", true, true);
            // 下边
            CreateBorderLine(target, name + "_Bottom", true, false);
            // 左边
            CreateBorderLine(target, name + "_Left", false, true);
            // 右边
            CreateBorderLine(target, name + "_Right", false, false);
        }

        /// <summary>
        /// 创建一条边
        /// </summary>
        /// <param name="target">目标区域</param>
        /// <param name="name">边的名字</param>
        /// <param name="isHorizontal">true=横线（上下边），false=竖线（左右边）</param>
        /// <param name="isTopOrLeft">true=上边或左边，false=下边或右边</param>
        private void CreateBorderLine(RectTransform target, string name, bool isHorizontal, bool isTopOrLeft)
        {
            GameObject line = new GameObject(name, typeof(RectTransform), typeof(Image));
            line.transform.SetParent(target, false);

            RectTransform rect = line.GetComponent<RectTransform>();

            if (isHorizontal)
            {
                // 横线（上下边）：宽度拉伸，高度=边框粗细
                rect.anchorMin = new Vector2(0f, isTopOrLeft ? 1f : 0f);
                rect.anchorMax = new Vector2(1f, isTopOrLeft ? 1f : 0f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(0f, borderThickness);
            }
            else
            {
                // 竖线（左右边）：高度拉伸，宽度=边框粗细
                rect.anchorMin = new Vector2(isTopOrLeft ? 0f : 1f, 0f);
                rect.anchorMax = new Vector2(isTopOrLeft ? 0f : 1f, 1f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(borderThickness, 0f);
            }

            Image img = line.GetComponent<Image>();
            img.color = borderColor;
            img.raycastTarget = false;
        }
    }
}
