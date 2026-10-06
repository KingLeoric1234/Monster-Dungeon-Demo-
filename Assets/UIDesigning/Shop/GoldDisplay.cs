using UnityEngine;
using TMPro;
using System.Collections;
using Game.Core;

namespace Game.UI
{
    /// <summary>
    /// 通用金币显示组件。
    /// 挂在任何需要显示金币的面板上，拖一个TextMeshProUGUI进来就行。
    /// 自动订阅金币变化，变化时播放Shimmer动画。
    /// </summary>
    public class GoldDisplay : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private TextMeshProUGUI goldText;  // 金币显示文字

        [Header("Shimmer动画")]
        [SerializeField] private float shimmerDuration = 0.4f;   // Shimmer扫过时长
        [SerializeField] private float shimmerPause = 1f;         // 停顿时长
        [SerializeField] private Color baseColor = new Color(1f, 0.84f, 0f);     // 金黄色
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.6f); // 亮黄色

        private Coroutine shimmerCoroutine;
        private int currentGold = 0;

        private void OnEnable()
        {
            MessageBus.Subscribe<GoldChangedMessage>(OnGoldChanged);
            // 启用时刷新并开始Shimmer
            RefreshGold();
            PlayShimmer();
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<GoldChangedMessage>(OnGoldChanged);
        }

        /// <summary>金币变化时触发</summary>
        private void OnGoldChanged(GoldChangedMessage msg)
        {
            currentGold = msg.NewGold;
            UpdateGoldText();
            PlayShimmer();
        }

        /// <summary>刷新金币显示</summary>
        private void RefreshGold()
        {
            if (Game.Items.ItemDirector.Instance != null)
            {
                currentGold = Game.Items.ItemDirector.Instance.CurrentGold;
            }
            else
            {
                // ItemDirector还没初始化，用初始金币兜底
                currentGold = Game.Items.EquipmentConfig.InitialGold;
            }
            UpdateGoldText();
        }

        /// <summary>更新文字</summary>
        private void UpdateGoldText()
        {
            if (goldText != null)
            {
                goldText.text = $"Gold: {currentGold}";
                goldText.color = baseColor;
            }
        }

        /// <summary>播放Shimmer动画</summary>
        private void PlayShimmer()
        {
            if (goldText == null) return;

            if (shimmerCoroutine != null)
                StopCoroutine(shimmerCoroutine);

            shimmerCoroutine = StartCoroutine(ShimmerLoop());
        }

        /// <summary>Shimmer循环：微光从左到右扫过，循环播放</summary>
        private IEnumerator ShimmerLoop()
        {
            if (goldText == null) yield break;

            int highlightWidth = 2;  // 高亮宽度2个字符

            while (true)
            {
                string baseText = $"Gold: {currentGold}";
                int totalChars = baseText.Length;
                int maxPos = Mathf.Max(0, totalChars - highlightWidth);

                // 高亮从左到右扫过
                float timer = 0f;
                while (timer < shimmerDuration)
                {
                    timer += Time.deltaTime;
                    float p = Mathf.Clamp01(timer / shimmerDuration);

                    // 计算高亮位置
                    int pos = Mathf.FloorToInt(p * maxPos);
                    pos = Mathf.Clamp(pos, 0, maxPos);

                    // 构建带高亮的文字
                    string result = "";
                    for (int i = 0; i < totalChars; i++)
                    {
                        if (i >= pos && i < pos + highlightWidth)
                        {
                            string hexColor = ColorUtility.ToHtmlStringRGB(highlightColor);
                            result += "<color=#" + hexColor + ">" + baseText[i] + "</color>";
                        }
                        else
                        {
                            result += baseText[i];
                        }
                    }
                    goldText.text = result;
                    yield return null;
                }

                // 一轮结束，恢复默认颜色，停顿
                goldText.text = baseText;
                goldText.color = baseColor;
                yield return new WaitForSeconds(shimmerPause);
            }
        }
    }
}
