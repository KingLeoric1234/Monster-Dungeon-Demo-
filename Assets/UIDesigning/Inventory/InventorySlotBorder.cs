using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// 自动给格子描边框。
    /// 挂在Slot上，自动创建上下左右四条边，拼成空心方框。
    /// 不需要手动拼四个Image，脚本自动生成。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class InventorySlotBorder : MonoBehaviour
    {
        [Header("边框设置")]
        [SerializeField] private Color borderColor = new Color(1f, 1f, 1f, 0.5f);  // 边框颜色
        [SerializeField] private float borderThickness = 2f;                        // 边框粗细
        [SerializeField] private bool fillBackground = false;                       // 是否填充背景
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.2f); // 背景颜色

        private RectTransform rectTransform;
        private Image topBorder, bottomBorder, leftBorder, rightBorder;
        private Image background;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            CreateBorder();
        }

        /// <summary>创建边框</summary>
        private void CreateBorder()
        {
            // 背景（可选）
            if (fillBackground)
            {
                background = CreateChildImage("Background");
                background.color = backgroundColor;
                StretchToParent(background.rectTransform);
            }

            // 上边
            topBorder = CreateChildImage("TopBorder");
            topBorder.color = borderColor;
            StretchTop(topBorder.rectTransform);

            // 下边
            bottomBorder = CreateChildImage("BottomBorder");
            bottomBorder.color = borderColor;
            StretchBottom(bottomBorder.rectTransform);

            // 左边
            leftBorder = CreateChildImage("LeftBorder");
            leftBorder.color = borderColor;
            StretchLeft(leftBorder.rectTransform);

            // 右边
            rightBorder = CreateChildImage("RightBorder");
            rightBorder.color = borderColor;
            StretchRight(rightBorder.rectTransform);
        }

        /// <summary>创建子Image</summary>
        private Image CreateChildImage(string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(transform, false);
            Image image = child.AddComponent<Image>();
            image.raycastTarget = false;  // 不阻挡点击
            return image;
        }

        /// <summary>拉伸到父物体大小</summary>
        private void StretchToParent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>顶部拉伸</summary>
        private void StretchTop(RectTransform rt)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(0, -borderThickness);
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>底部拉伸</summary>
        private void StretchBottom(RectTransform rt)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = new Vector2(0, borderThickness);
        }

        /// <summary>左侧拉伸</summary>
        private void StretchLeft(RectTransform rt)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = new Vector2(borderThickness, 0);
        }

        /// <summary>右侧拉伸</summary>
        private void StretchRight(RectTransform rt)
        {
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 0.5f);
            rt.offsetMin = new Vector2(-borderThickness, 0);
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>运行时修改边框颜色</summary>
        public void SetBorderColor(Color color)
        {
            borderColor = color;
            if (topBorder != null) topBorder.color = color;
            if (bottomBorder != null) bottomBorder.color = color;
            if (leftBorder != null) leftBorder.color = color;
            if (rightBorder != null) rightBorder.color = color;
        }

        /// <summary>运行时修改边框粗细</summary>
        public void SetBorderThickness(float thickness)
        {
            borderThickness = thickness;
            // 重新设置每条边的大小
            if (topBorder != null) topBorder.rectTransform.offsetMin = new Vector2(0, -thickness);
            if (bottomBorder != null) bottomBorder.rectTransform.offsetMax = new Vector2(0, thickness);
            if (leftBorder != null) leftBorder.rectTransform.offsetMax = new Vector2(thickness, 0);
            if (rightBorder != null) rightBorder.rectTransform.offsetMin = new Vector2(-thickness, 0);
        }
    }
}
