using UnityEngine;
using Game.Core;

namespace Game.World
{
    /// <summary>
    /// 道具商店NPC（独立，不跟其它商店混用）。
    /// 玩家点击确认后打开道具商店，玩家远离时自动关闭。
    /// 挂在道具商店NPC物体上，需要同时挂Interactable组件。
    /// 只发布 item_shop 的消息，跟装备商店、武器商店完全独立。
    /// </summary>
    public class ItemShopNPC : MonoBehaviour
    {
        private const string ShopID = "item_shop";  // 道具商店固定ID

        private Interactable owner;
        private bool isShopOpen = false;

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
                    if (dist > owner.InteractRange * 1.5f)
                    {
                        MessageBus.Publish(new CloseShopMessage { ShopNPCID = ShopID });
                        isShopOpen = false;
                    }
                }
            }
        }

        /// <summary>玩家点击确认</summary>
        private void OnConfirm(InteractConfirmMessage msg)
        {
            if (msg.Target != owner) return;
            MessageBus.Publish(new OpenShopMessage { ShopNPCID = ShopID });
        }

        private void OnShopOpened(OpenShopMessage msg)
        {
            if (msg.ShopNPCID == ShopID) isShopOpen = true;
        }

        private void OnShopClosed(CloseShopMessage msg)
        {
            if (msg.ShopNPCID == ShopID) isShopOpen = false;
        }
    }
}
