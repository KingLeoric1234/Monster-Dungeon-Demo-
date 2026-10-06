using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Game.Core;

namespace Game.UI
{
    /// <summary>
    /// 武器/药水商店面板管理器。
    /// 订阅OpenShopMessage，根据ShopNPCID判断是否打开。
    /// 用CanvasGroup控制显示/隐藏，带指数型开/关动画。
    /// 挂在武器/药水商店的Panel物体上。
    /// </summary>
    public class WeaponShopPanel : MonoBehaviour
    {
        [Header("商店设置")]
        [SerializeField] private string shopNPCID = "weapon_shop";  // 对应的商店NPC ID

        [Header("动画参数")]
        [SerializeField] private float openDuration = 0.25f;   // 打开动画时长
        [SerializeField] private float closeDuration = 0.2f;   // 关闭动画时长

        [Header("引用")]
        [SerializeField] private CanvasGroup canvasGroup;  // 用CanvasGroup控制显示/隐藏
        [SerializeField] private Button closeButton;       // 关闭按钮（X）

        private Coroutine currentAnim;  // 当前播放的动画协程

        private void Awake()
        {
            // 自动获取CanvasGroup（如果没拖）
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            // 绑定关闭按钮
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseAndNotify);
            }

            // 默认隐藏（在Awake里隐藏，跟ShopPanel保持一致，避免闪现）
            HideImmediate();
        }

        private void OnEnable()
        {
            MessageBus.Subscribe<OpenShopMessage>(OnOpenShop);
            MessageBus.Subscribe<CloseShopMessage>(OnCloseShop);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<OpenShopMessage>(OnOpenShop);
            MessageBus.Unsubscribe<CloseShopMessage>(OnCloseShop);
        }

        /// <summary>收到打开商店消息，判断ID是否匹配</summary>
        private void OnOpenShop(OpenShopMessage msg)
        {
            //Debug.Log($"[WeaponShop:{shopNPCID}] 收到OpenShop, msgID={msg.ShopNPCID}, 匹配={msg.ShopNPCID == shopNPCID}");
            if (msg.ShopNPCID == shopNPCID)
            {
                Open();
            }
        }

        private void OnCloseShop(CloseShopMessage msg)
        {
            //Debug.Log($"[WeaponShop:{shopNPCID}] 收到CloseShop, msgID={msg.ShopNPCID}");
            if (string.IsNullOrEmpty(msg.ShopNPCID) || msg.ShopNPCID == shopNPCID)
            {
                Close();
            }
        }

        /// <summary>打开商店（指数型放大+淡入）</summary>
        public void Open()
        {
            if (currentAnim != null) StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(OpenAnim());

            // 注册到UIPanelStack，按Tab可以关闭
            if (UIPanelStack.Instance != null)
            {
                UIPanelStack.Instance.RegisterPanel(gameObject);
            }
        }

        /// <summary>关闭商店并通知NPC</summary>
        public void CloseAndNotify()
        {
            Close();
            Game.Core.MessageBus.Publish(new CloseShopMessage { ShopNPCID = shopNPCID });
        }

        /// <summary>关闭商店（指数型缩小+淡出）</summary>
        public void Close()
        {
            if (currentAnim != null) StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(CloseAnim());

            // 从UIPanelStack注销
            if (UIPanelStack.Instance != null)
            {
                UIPanelStack.Instance.UnregisterPanel(gameObject);
            }
        }

        /// <summary>打开动画：从0.8倍放大到1倍，同时淡入</summary>
        private IEnumerator OpenAnim()
        {
            if (canvasGroup == null) yield break;

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            float t = 0f;
            while (t < openDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / openDuration);
                // 指数型缓动：快速开始，渐缓结束
                float ease = 1f - Mathf.Pow(1f - p, 3f);
                canvasGroup.alpha = ease;
                transform.localScale = Vector3.one * (0.8f + 0.2f * ease);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;
        }

        /// <summary>关闭动画：从1倍缩小到0.8倍，同时淡出</summary>
        private IEnumerator CloseAnim()
        {
            if (canvasGroup == null) yield break;

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            float t = 0f;
            while (t < closeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / closeDuration);
                // 指数型缓动：渐缓开始，快速结束
                float ease = Mathf.Pow(1f - p, 3f);
                canvasGroup.alpha = ease;
                transform.localScale = Vector3.one * (0.8f + 0.2f * ease);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            transform.localScale = Vector3.one;
        }

        /// <summary>瞬间隐藏（初始化用）</summary>
        private void HideImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            transform.localScale = Vector3.one;
        }
    }
}

