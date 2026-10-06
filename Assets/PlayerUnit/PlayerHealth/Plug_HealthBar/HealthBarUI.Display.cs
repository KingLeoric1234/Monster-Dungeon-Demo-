using UnityEngine;
using System.Collections;

namespace HealthBar
{
    /// <summary>
    /// 玩家血条 UI 芯片——显示件：默认显隐、地牢延迟渐显、状态面板联动、内容刷新。
    /// 数据从 Core 件的字段读，动画交给 Animation 件，本件只决定"什么时候显示、显示成什么样"。
    /// </summary>
    public partial class HealthBarUI : MonoBehaviour
    {
        [Header("Dungeon HP Delay")]
        private Coroutine delayedHPCoroutine;  // 血条延迟显示协程

        private void Start()
        {
            // 主世界默认隐藏血条
            UpdateDefaultVisibility();

            // 自动检测是否在地牢场景（防止切场景消息漏掉导致显示状态错误）
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isDungeon = false;
            foreach (string name in HealthBarConfig.DungeonSceneNames)
            {
                if (sceneName.Contains(name)) { isDungeon = true; break; }
            }
            if (isDungeon)
            {
                SetInDungeon(true);
            }
        }

        /// <summary>血条默认显隐：主世界默认隐藏（面板开时由面板联动显示）</summary>
        private void UpdateDefaultVisibility()
        {
            if (hpGroup != null) hpGroup.SetActive(false);
        }

        /// <summary>状态面板打开：血条跟随显示（带动画）</summary>
        public void OnStatusPanelOpen()
        {
            isStatusPanelOpen = true;
            ShowBarsWithAnimation();
        }

        /// <summary>状态面板关闭：血条隐藏（地牢里保留常驻）</summary>
        public void OnStatusPanelClose()
        {
            isStatusPanelOpen = false;
            HideBarsWithAnimation();
        }

        /// <summary>设置地牢状态：地牢里血条常驻（延迟2秒渐显），主世界默认隐藏</summary>
        public void SetInDungeon(bool inDungeon)
        {
            isInDungeon = inDungeon;

            // 取消之前的延迟显示协程
            if (delayedHPCoroutine != null)
            {
                StopCoroutine(delayedHPCoroutine);
                delayedHPCoroutine = null;
            }

            if (inDungeon)
            {
                // 进地牢：血条延迟 2 秒渐显
                delayedHPCoroutine = StartCoroutine(DelayedShowHP());
            }
            else
            {
                // 离开地牢：立即隐藏血条
                if (hpGroup != null)
                {
                    hpGroup.SetActive(false);
                }
            }
        }

        /// <summary>延迟显示血条：等待 delay 秒后渐显出现</summary>
        private IEnumerator DelayedShowHP()
        {
            yield return new WaitForSeconds(HealthBarConfig.DungeonHpShowDelay);

            // 如果状态面板已打开，条已显示，不用再显示
            if (isStatusPanelOpen) yield break;

            if (hpGroup != null)
            {
                hpGroup.SetActive(true);
                SetGroupAlpha(hpGroup, 0f);
            }

            float t = 0f;
            while (t < HealthBarConfig.DungeonHpFadeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / HealthBarConfig.DungeonHpFadeDuration);
                float ease = 1f - Mathf.Pow(1f - p, HealthBarConfig.FadeInEasePower); // 指数型渐显
                if (hpGroup != null) SetGroupAlpha(hpGroup, ease);
                yield return null;
            }

            if (hpGroup != null) SetGroupAlpha(hpGroup, 1f);
            delayedHPCoroutine = null;
        }

        /// <summary>刷新血条显示：宽度按比例缩 + 文本</summary>
        private void UpdateHPDisplay()
        {
            if (hpFill != null && maxHP > 0)
            {
                float ratio = (float)currentHP / maxHP;
                RectTransform rt = hpFill.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector2 size = rt.sizeDelta;
                    size.x = HealthBarConfig.HpBarMaxWidth * ratio;
                    rt.sizeDelta = size;
                }
            }
            if (hpText != null)
            {
                hpText.text = currentHP + " / " + maxHP;
            }
        }
    }
}
