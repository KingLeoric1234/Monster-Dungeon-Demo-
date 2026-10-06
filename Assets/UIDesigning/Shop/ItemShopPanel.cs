using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Game.Core;

namespace Game.UI
{
    /// <summary>
    /// 道具商店面板（独立，不跟其它商店混用）。
    /// 只订阅 item_shop 的消息，跟装备商店、武器商店完全独立。
    /// 用CanvasGroup控制显示/隐藏，带指数型开/关动画。
    /// 挂在道具商店的Panel物体上。
    /// </summary>
    public class ItemShopPanel : MonoBehaviour
    {
        private const string ShopID = "item_shop";  // 道具商店固定ID

        [Header("动画参数")]
        [SerializeField] private float openDuration = 0.25f;   // 打开动画时长
        [SerializeField] private float closeDuration = 0.2f;   // 关闭动画时长

        [Header("引用")]
        [SerializeField] private CanvasGroup canvasGroup;  // 用CanvasGroup控制显示/隐藏
        [SerializeField] private Button closeButton;       // 关闭按钮（X）

        private Coroutine currentAnim;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

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

        /// <summary>收到打开商店消息，只响应item_shop</summary>
        private void OnOpenShop(OpenShopMessage msg)
        {
            //Debug.Log($"[ItemShop:{ShopID}] 收到OpenShop, msgID={msg.ShopNPCID}, 匹配={msg.ShopNPCID == ShopID}");
            if (msg.ShopNPCID == ShopID) Open();
        }

        private void OnCloseShop(CloseShopMessage msg)
        {
            //Debug.Log($"[ItemShop:{ShopID}] 收到CloseShop, msgID={msg.ShopNPCID}");
            if (string.IsNullOrEmpty(msg.ShopNPCID) || msg.ShopNPCID == ShopID) Close();
        }

        /// <summary>打开商店（指数型放大+淡入）</summary>
        public void Open()
        {
            if (currentAnim != null) StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(OpenAnim());

            if (UIPanelStack.Instance != null)
            {
                UIPanelStack.Instance.RegisterPanel(gameObject);
            }
        }

        public void CloseAndNotify()
        {
            Close();
            Game.Core.MessageBus.Publish(new CloseShopMessage { ShopNPCID = ShopID });
        }

        /// <summary>关闭商店（指数型缩小+淡出）</summary>
        public void Close()
        {
            if (currentAnim != null) StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(CloseAnim());

            if (UIPanelStack.Instance != null)
            {
                UIPanelStack.Instance.UnregisterPanel(gameObject);
            }
        }

        /// <summary>打开动画</summary>
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
                float ease = 1f - Mathf.Pow(1f - p, 3f);
                canvasGroup.alpha = ease;
                transform.localScale = Vector3.one * (0.8f + 0.2f * ease);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;
        }

        /// <summary>关闭动画</summary>
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

