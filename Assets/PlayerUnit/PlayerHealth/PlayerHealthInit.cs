using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// 血量初始化（业务编排）：登录/死亡复活/读档时执行一次。
    /// 只决定"初始值定多少"（当前规则：满血复活），不碰数据细节——
    /// 先让 MaxHealthManager 算初始上限，再通过 PlayerHealthAction 写满血。
    /// 以后加复活规则（半血复活/读档恢复）只改 ResetToFull 一个方法。
    /// 挂在 Player 物体上。
    /// </summary>
    public class PlayerHealthInit : MonoBehaviour
    {
        private MaxHealthManager maxHealthManager;
        private PlayerHealthAction action;

        private void Awake()
        {
            maxHealthManager = GetComponent<MaxHealthManager>();
            action = GetComponent<PlayerHealthAction>();
        }

        private void Start()
        {
            // 每次登录：满血复活
            ResetToFull();
        }

        /// <summary>满血复活（登录/死亡复活/读档都走这里）</summary>
        public void ResetToFull()
        {
            // ① 先让上限来源算出初始上限（写入 Cache）
            if (maxHealthManager != null) maxHealthManager.Initialize();
            // ② 再重置血量：清无敌帧 + 满血（通知由 Cache 统一发出）
            if (action != null) action.ResetHealth();
        }
    }
}
