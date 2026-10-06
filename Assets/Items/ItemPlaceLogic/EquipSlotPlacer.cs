using UnityEngine;
using Game.Items;
using Game.UI;

namespace Game.Items
{
    /// <summary>放置到穿戴护甲槽</summary>
    public static class EquipSlotPlacer
    {
        public static void Place(EquipmentSlotUI slot, string id, Sprite icon, Color iconColor)
        {
            var data = EquipmentConfig.GetEquipment(id);
            // Debug.Log("[购买进入]功能正常");
            // Debug.Log($"[EquipPlacer] 进入 id={id} | slot={slot?.name} | 配置空={data == null}");
            if (data == null)
            {
                // Debug.LogWarning($"[EquipPlacer] 拦截①：配置里没有这个装备ID → {id}");
                return;
            }
            if (ItemDirector.Instance.OwnsEquipment(id))
            {
                // Debug.LogWarning($"[EquipPlacer] 拦截②：已拥有该装备，禁止重复购买 → {id}");
                return;
            }
            // 购买（不入包：拖拽放置只登记到装备槽，避免重复进背包）
            if (!ItemDirector.Instance.BuyEquipment(id, false))
            {
                // Debug.LogWarning($"[EquipPlacer] 拦截③：购买失败（金币不足或购买被拒） → {id}");
                return;
            }
            // Debug.Log($"[EquipPlacer] 购买成功，开始装备 → {id}");
            ItemDirector.Instance.Equip(id);
            slot.EquipItem(data);
            // Debug.Log("[装备登记]功能正常");
            if (icon != null)
            {
                // 槽显示（用Cursor携带的按钮颜色）+ 写Cache（Cache是两面板UI同步的中枢，StatusPanel从这里读）
                slot.SetIcon(icon, iconColor);
                EquipmentIconCache.SetIcon(id, icon, iconColor);
                // Debug.Log("[Cache写入]功能正常");
            }
            else
            {
                // Debug.LogWarning("[Cache写入]失败：Cursor图标为空");
            }
        }
    }
}
