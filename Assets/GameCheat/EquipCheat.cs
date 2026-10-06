using UnityEngine;
using Game.Items;
using Game.Core;

namespace Game.Cheat
{
    /// <summary>测试用：按键穿装备到指定槽位</summary>
    public class EquipCheat : MonoBehaviour
    {
        [Header("按键")]
        [SerializeField] private KeyCode headKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode handKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode bodyKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode backKey = KeyCode.Alpha4;

        [Header("装备ID")]
        [SerializeField] private string headID = "tactical_helmet";
        [SerializeField] private string handID = "tactical_bracers";
        [SerializeField] private string bodyID = "tactical_armor";
        [SerializeField] private string backID = "tactical_backpack";

        private bool unlocked = false;
        private string buffer = "";

        private void Update()
        {
            if (!unlocked)
            {
                buffer += Input.inputString;
                if (buffer.Contains("bossrush")) { unlocked = true; Debug.Log("[Cheat] 解锁"); }
                if (buffer.Length > 20) buffer = buffer.Substring(buffer.Length - 20);
                return;
            }
            if (ItemDirector.Instance == null) return;

            if (Input.GetKeyDown(headKey)) Equip(EquipmentSlotType.Head, headID);
            if (Input.GetKeyDown(handKey)) Equip(EquipmentSlotType.Hand, handID);
            if (Input.GetKeyDown(bodyKey)) Equip(EquipmentSlotType.Body, bodyID);
            if (Input.GetKeyDown(backKey)) Equip(EquipmentSlotType.Back, backID);
        }

        private void Equip(EquipmentSlotType slot, string id)
        {
            var data = EquipmentConfig.GetEquipment(id);
            if (data == null) { Debug.LogWarning($"[Cheat] 装备不存在: {id}"); return; }
            ItemDirector.Instance.CheatAddItem(id);
            ItemDirector.Instance.Equip(id);
            Debug.Log($"[Cheat] 已装备 {data.Name} 到 {slot}");
        }
    }
}
