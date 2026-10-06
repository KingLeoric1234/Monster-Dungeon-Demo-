using UnityEngine;

namespace Game.Enemy
{
    /// <summary>TimeScale 兜底：物体销毁时恢复时间流速，防止顿帧把游戏卡死在慢动作</summary>
    public class TimeScaleGuard : MonoBehaviour
    {
        private void OnDestroy()
        {
            if (Time.timeScale < 1f) Time.timeScale = 1f;
        }
    }
}
