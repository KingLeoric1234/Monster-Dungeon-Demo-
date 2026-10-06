using UnityEngine;

namespace Game.UI
{
    public class UIDirector : MonoBehaviour
    {
        // 交互提示面板由各NPC头顶面板自己订阅处理（B方案），UIDirector不再转发
        // 以后 UIDirector 只负责它该管的（比如 HUD、血条等全局 UI）

        public void ShowWorld() { }
    }
}
