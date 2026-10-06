using UnityEngine;
using Game.Core;
using Game.World;

namespace Game.World
{
    /// <summary>
    /// 商店NPC：玩家点击确认后打开商店界面。
    /// 玩家远离商人时自动关闭商店。
    /// 挂在NPC物体上，需要同时挂Interactable组件。
    /// 通过MessageBus解耦，NPC只负责发布消息，Panel负责订阅。
    /// </summary>
    public class ShopNPC : MonoBehaviour
    {
        [Header("商店设置")]
        [SerializeField] private string shopNPCID = "default_shop";  // 商店NPC ID

        private Interactable owner;
        private bool isShopOpen = false;  // 商店是否打开

        private void Awake()
        {
            owner = GetComponent<Interactable>();
        }

        private void OnEnable()
        {
            MessageBus.Subscribe<InteractConfirmMessage>(OnConfirm);
            MessageBus.Subscribe<OpenShopMessage>(OnShopOpened);
            MessageBus.Subscribe<CloseShopMessage>(OnShopClosed);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<InteractConfirmMessage>(OnConfirm);
            MessageBus.Unsubscribe<OpenShopMessage>(OnShopOpened);
            MessageBus.Unsubscribe<CloseShopMessage>(OnShopClosed);
        }

        private void Update()
        {
            // 商店打开时，检测玩家是否远离
            if (isShopOpen && owner != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    float dist = Vector2.Distance(player.transform.position, transform.position);
                    //Debug.Log($"[ShopNPC] {gameObject.name}, ID={shopNPCID}, dist={dist}, range={owner.InteractRange}, threshold={owner.InteractRange * 1.5f}");
                    // 超过交互范围的1.5倍时关闭商店
                    if (dist > owner.InteractRange * 1.5f)
                    {
                        MessageBus.Publish(new CloseShopMessage { ShopNPCID = shopNPCID });
                        isShopOpen = false;
                    }
                }
                // else
                // {
                //     Debug.LogWarning($"[ShopNPC] {gameObject.name}, ID={shopNPCID}, 找不到Player！");
                // }
            }
            // else if (isShopOpen && owner == null)
            // {
            //     Debug.LogWarning($"[ShopNPC] {gameObject.name}, ID={shopNPCID}, owner为null！");
            // }
        }

        /// <summary>玩家点击确认</summary>
        private void OnConfirm(InteractConfirmMessage msg)
        {
            if (msg.Target != owner) return;

            // 打开商店（通过MessageBus解耦）
            MessageBus.Publish(new OpenShopMessage { ShopNPCID = shopNPCID });
        }

        private void OnShopOpened(OpenShopMessage msg)
        {
            if (msg.ShopNPCID == shopNPCID) isShopOpen = true;
        }

        private void OnShopClosed(CloseShopMessage msg)
        {
            isShopOpen = false;
        }
    }
}
