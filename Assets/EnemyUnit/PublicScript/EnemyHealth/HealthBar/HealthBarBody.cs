using UnityEngine;
using UnityEngine.UI;

namespace Game.Enemy
{
    /// <summary>
    /// 血条本体（Body）：Slider 显示血量 + 按血量百分比变色（正常绿/半血橙/残血红）。
    /// 由 HealthBarBinder 订阅消息后调用 Initialize/UpdateHealth。
    /// Body=本体只负责显示，Binder=接线员只负责听消息，两者配对。
    /// </summary>
    public class HealthBarBody : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Slider slider;
        [SerializeField] private Image fillImage;  // Fill 的 Image 组件，用来变色

        [Header("残血颜色")]
        [SerializeField] private Color normalColor = Color.green;      // 正常颜色
        [SerializeField] private Color halfHealthColor = new Color(1f, 0.5f, 0f, 0.8f);  // 半透明橙色
        [SerializeField] private Color lowHealthColor = new Color(1f, 0f, 0f, 0.8f);    // 半透明红色

        private int maxHealth;

        /// <summary>初始化最大血量</summary>
        public void Initialize(int maxHp)
        {
            maxHealth = maxHp;
            slider.maxValue = maxHp;
            slider.value = maxHp;
            fillImage.color = normalColor;
        }

        /// <summary>更新血条</summary>
        public void UpdateHealth(int currentHealth)
        {
            slider.value = currentHealth;

            // 根据血量百分比变色
            float ratio = (float)currentHealth / maxHealth;

            if (ratio <= 0.1f)
            {
                fillImage.color = lowHealthColor;   // 低于10%，半透明红色
            }
            else if (ratio <= 0.5f)
            {
                fillImage.color = halfHealthColor;  // 低于50%，半透明橙色
            }
            else
            {
                fillImage.color = normalColor;      // 正常颜色
            }
        }
    }
}
