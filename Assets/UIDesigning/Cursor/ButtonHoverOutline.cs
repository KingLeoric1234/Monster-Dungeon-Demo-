using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// 按钮悬停描边（用Unity自带Outline组件，不需要新建子物体）。
    /// 鼠标悬停在Button上时，显示黄色描边；鼠标离开时隐藏。
    ///
    /// 使用方式：
    /// 1. 选中Button里的Image（按钮背景图），加一个 Outline 组件
    /// 2. Outline组件里设置描边颜色（黄色）和距离（比如2）
    /// 3. 把Outline组件拖到本脚本的 outline 字段
    /// 4. 默认Outline是禁用的，运行时悬停自动启用
    ///
    /// 可复用：任何需要悬停描边的Button都挂这个脚本。
    /// </summary>
    public class ButtonHoverOutline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("描边设置")]
        [SerializeField] private Outline outline;  // Button的Image上的Outline组件

        private void Start()
        {
            // 默认隐藏描边
            if (outline != null)
                outline.enabled = false;
        }

        /// <summary>鼠标进入按钮时显示描边</summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (outline != null)
                outline.enabled = true;
        }

        /// <summary>鼠标离开按钮时隐藏描边</summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (outline != null)
                outline.enabled = false;
        }

        private void OnDisable()
        {
            // 物体禁用时恢复默认，防止停留在悬停状态
            if (outline != null)
                outline.enabled = false;
        }
    }
}
