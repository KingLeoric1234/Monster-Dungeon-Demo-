using UnityEngine;
using Game.Core;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// 商店购买逻辑：处理装备购买、已拥有检查、购买成功回调。
    /// 与ShopPanel（UI显示）分离，职责单一。
    /// 挂在ShopPanel同一物体上。
    /// </summary>
    public class ShopLogic : MonoBehaviour
    {
        /// <summary>购买成功后的回调（ShopPanel订阅后刷新列表）</summary>
        public System.Action OnPurchaseSuccess;

        /// <summary>
        /// 尝试购买装备。
        /// 返回true表示购买成功，false表示已拥有或购买失败。
        /// </summary>
        public bool TryBuyEquipment(EquipmentData data)
        {
            if (ItemDirector.Instance == null)
            {
                // Debug.LogWarning("[ShopLogic] ItemDirector未初始化");
                return false;
            }

            // 检查是否已拥有
            if (ItemDirector.Instance.OwnsEquipment(data.ID))
            {
                // Debug.Log($"[ShopLogic] 已拥有: {data.Name}");
                return false;
            }

            // 执行购买
            bool success = ItemDirector.Instance.BuyItem(data.ID);
            if (success)
            {
                // 购买成功，触发回调
                OnPurchaseSuccess?.Invoke();
            }
            return success;
        }
    }
}
