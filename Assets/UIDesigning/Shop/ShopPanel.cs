using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// 商店购买界面。
    /// 挂在Canvas下的ShopPanel物体上。
    /// 购买逻辑在ShopLogic，本文件只负责面板显示和动画。
    /// 金币显示由独立的GoldDisplay组件负责。
    /// </summary>
    public class ShopPanel : MonoBehaviour
    {
        [Header("商店设置")]
        [SerializeField] private string shopNPCID = "default_shop";  // 对应的商店NPC ID

        [Header("引用")]
        [SerializeField] private Button closeButton;                 // 关闭按钮
        [SerializeField] private CanvasGroup canvasGroup;            // 控制显示/隐藏
        [SerializeField] private ShopLogic shopLogic;                // 购买逻辑（与UI分离）

        [Header("动画")]
        [SerializeField] private float fadeDuration = 0.2f;          // 淡入淡出时长

        private Coroutine animCoroutine;

        private void Awake()
        {
            closeButton.onClick.AddListener(CloseAndNotify);
            SetVisibleImmediate(false);

            // 注册购买成功回调
            if (shopLogic != null)
            {
                shopLogic.OnPurchaseSuccess += RefreshEquipmentList;
            }
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

        private void OnOpenShop(OpenShopMessage msg)
        {
            //Debug.Log($"[ShopPanel:{shopNPCID}] 收到OpenShop, msgID={msg.ShopNPCID}, 匹配={msg.ShopNPCID == shopNPCID}");
            if (msg.ShopNPCID == shopNPCID)
            {
                Open();
                //Debug.Log($"[ShopPanel:{shopNPCID}] 执行Open()");
            }
        }

        private void OnCloseShop(CloseShopMessage msg)
        {
            //Debug.Log($"[ShopPanel:{shopNPCID}] 收到CloseShop, msgID={msg.ShopNPCID}");
            if (string.IsNullOrEmpty(msg.ShopNPCID) || msg.ShopNPCID == shopNPCID)
            {
                Close();
                //Debug.Log($"[ShopPanel:{shopNPCID}] 执行Close()");
            }
        }

        /// <summary>打开商店</summary>
        public void Open()
        {
            RefreshEquipmentList();
            SetVisible(true);
            // 注册到UIPanelStack，按ESC可以关闭
            if (UIPanelStack.Instance != null)
                UIPanelStack.Instance.RegisterPanel(gameObject);
        }

        /// <summary>关闭商店并通知NPC</summary>
        public void CloseAndNotify()
        {
            Close();
            Game.Core.MessageBus.Publish(new CloseShopMessage { ShopNPCID = shopNPCID });
        }

        /// <summary>关闭商店</summary>
        public void Close()
        {
            SetVisible(false);
            // 从UIPanelStack注销
            if (UIPanelStack.Instance != null)
                UIPanelStack.Instance.UnregisterPanel(gameObject);
        }

        /// <summary>刷新装备列表</summary>
        private void RefreshEquipmentList()
        {
            // 手动排版了，装备项已经在场景里，只需要更新状态
        }

        // ── 显示/隐藏动画 ─────────────────────────────────────

        private void SetVisible(bool show)
        {
            if (animCoroutine != null) StopCoroutine(animCoroutine);

            if (show)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                animCoroutine = StartCoroutine(FadeIn());
            }
            else
            {
                animCoroutine = StartCoroutine(FadeOut());
            }
        }

        private System.Collections.IEnumerator FadeIn()
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / fadeDuration);
                canvasGroup.alpha = p;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private System.Collections.IEnumerator FadeOut()
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / fadeDuration);
                canvasGroup.alpha = 1f - p;
                yield return null;
            }
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        private void SetVisibleImmediate(bool show)
        {
            canvasGroup.alpha = show ? 1f : 0f;
            canvasGroup.blocksRaycasts = show;
            canvasGroup.interactable = show;
        }
    }
}

