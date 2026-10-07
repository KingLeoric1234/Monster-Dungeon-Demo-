using UnityEngine;
using UnityEngine.UI;
using Game.GameDirector;

namespace Game.Items
{
    /// <summary>
    /// 小地图：把迷雾的displayRT贴到RawImage上，玩家箭头指向朝向。
    /// 挂在Canvas下的一个空物体上，拖入RawImage和玩家箭头。
    /// </summary>
    public class MiniMap : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private RawImage mapImage;       // 小地图显示区域
        [SerializeField] private RectTransform playerArrow; // 玩家箭头（小地图中心的小图标）
        [SerializeField] private Transform player;          // 玩家Transform（读朝向用）

        private void Update()
        {
            if (FogOfWarDirector.Instance == null) return;

            // 1. 贴迷雾贴图（白=已探索，黑=未探索）
            if (mapImage != null)
                mapImage.texture = FogOfWarDirector.Instance.DisplayRT;

            // 2. 玩家箭头：小地图中心，旋转指向玩家朝向
            if (playerArrow != null && player != null)
            {
                playerArrow.anchoredPosition = Vector2.zero; // 始终在小地图中心
                float angle = Mathf.Atan2(player.up.y, player.up.x) * Mathf.Rad2Deg;
                playerArrow.localEulerAngles = new Vector3(0, 0, angle);
            }
        }
    }
}
