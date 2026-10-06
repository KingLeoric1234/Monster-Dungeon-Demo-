using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HealthBar
{
    /// <summary>
    /// 玩家血条 UI 芯片——装配件（DataTransmission）：专门操作零件属性。
    /// 职责：运行时自举——创建 hpGroup/hpFill/hpText 三个零件、设锚点坐标、从 Resources 抓图抓字体。
    /// 数据件（HealthBarUI.cs）只管数据流（字段+订阅+收消息），本件只管"零件长什么样、装在哪"。
    /// </summary>
    public partial class HealthBarUI : MonoBehaviour
    {
        /// <summary>hpGroup 的锚定基准位置（动画滑动从这里偏移，不依赖 Inspector）</summary>
        private Vector2 hpGroupBasePos;

        private void Awake()
        {
            AutoWire(); // 挂上组件即自动长出血条，不需要任何 Inspector 操作
        }

        /// <summary>
        /// 运行时自举：三个零件全部由代码/Resources 装配。
        /// 1. hpGroup：new 一个空物体（RectTransform + CanvasGroup）挂到场景 Canvas 下
        /// 2. hpFill：子物体 + Image，图从 Resources/Fills/Fill1 抓
        /// 3. hpText：子物体 + TextMeshProUGUI，字体从 Resources/TextMesh Pro 抓
        /// </summary>
        private void AutoWire()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[HealthBarUI] 场景里没有 Canvas，血条无法显示");
                return;
            }

            // 1. hpGroup：托盘（空物体，挂在画布下，左上角）
            if (hpGroup == null)
            {
                hpGroup = new GameObject("PlayerHPGroup");
                hpGroup.AddComponent<RectTransform>();
                hpGroup.AddComponent<CanvasGroup>();
                hpGroup.transform.SetParent(canvas.transform, false);

                RectTransform groupRt = hpGroup.GetComponent<RectTransform>();
                groupRt.anchorMin = new Vector2(0f, 1f);    // 锚定画布左上角
                groupRt.anchorMax = new Vector2(0f, 1f);
                groupRt.pivot = new Vector2(0f, 1f);
                groupRt.anchoredPosition = new Vector2(20f, -20f); // 左上留边
                hpGroupBasePos = groupRt.anchoredPosition;  // 记录动画滑动基准
            }

            // 2. hpFill：填充条（子物体 Image，图从 Resources/Fills/Fill1 抓）
            if (hpFill == null)
            {
                GameObject fillGo = new GameObject("PlayerHPFill");
                fillGo.transform.SetParent(hpGroup.transform, false);
                hpFill = fillGo.AddComponent<Image>();
                hpFill.raycastTarget = false; // 血条不挡 UI 点击

                Sprite fillSprite = Resources.Load<Sprite>("Fills/Fill1");
                if (fillSprite != null)
                {
                    hpFill.sprite = fillSprite;
                }
                else
                {
                    Debug.LogWarning("[HealthBarUI] Resources/Fills/Fill1 没找到，填充条无图");
                }

                RectTransform fillRt = hpFill.GetComponent<RectTransform>();
                fillRt.anchorMin = new Vector2(0f, 0f);   // 左对齐：血少了宽度从右边缩
                fillRt.anchorMax = new Vector2(0f, 1f);
                fillRt.pivot = new Vector2(0f, 0.5f);
                fillRt.anchoredPosition = Vector2.zero;
                fillRt.sizeDelta = new Vector2(HealthBarConfig.HpBarMaxWidth, 20f); // 初始满宽
            }

            // 3. hpText：血量文字（子物体 TMP，字体从 Resources/TextMesh Pro 抓）
            if (hpText == null)
            {
                GameObject textGo = new GameObject("PlayerHpText");
                textGo.transform.SetParent(hpGroup.transform, false);
                hpText = textGo.AddComponent<TextMeshProUGUI>();
                hpText.raycastTarget = false;

                TMP_FontAsset font = Resources.Load<TMP_FontAsset>("TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF");
                if (font != null)
                {
                    hpText.font = font;
                }
                else
                {
                    Debug.LogWarning("[HealthBarUI] TMP 字体没找到，用默认字体");
                }

                hpText.fontSize = 16;
                hpText.color = Color.white;
                hpText.alignment = TextAlignmentOptions.Center;

                RectTransform textRt = hpText.GetComponent<RectTransform>();
                textRt.anchorMin = new Vector2(0f, 0f);  // 铺满 hpGroup
                textRt.anchorMax = new Vector2(1f, 1f);
                textRt.pivot = new Vector2(0.5f, 0.5f);
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;
            }
        }
    }
}
