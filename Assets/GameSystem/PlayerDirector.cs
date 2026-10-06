// Assets/Scripts/Player/PlayerDirector.cs
using UnityEngine;
using Game.Core;

namespace Game.Player
{
    /// <summary>
    /// 玩家秘书：只负责调度三个手下，不碰数值和计算
    /// </summary>
    public class PlayerDirector : MonoBehaviour
    {
        // 先声明变量之后才能用
        private PlayerHealthAction health;
        private PlayerMovement movement;
        private PlayerAttackAction attack;
        private AttackPhysiology physiology;

        public static PlayerDirector Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            health = GetComponent<PlayerHealthAction>();
            movement = GetComponent<PlayerMovement>();
            attack = GetComponent<PlayerAttackAction>();
            physiology = GetComponent<AttackPhysiology>();

            //设置玩家的大小
            float size = PlayerConfig.PlayerSize;
            transform.localScale = new Vector3(size, size, 1f);
            //DontDestroyOnLoad(gameObject);   // 玩家跨场景存活
        }

        //以下代码都来自对应的Health，Movement，Attack代码

        /// <summary>重置玩家到初始状态</summary>
        public void ResetPlayer() // 重置玩家的血量，位置，攻击冷却
        {
            health?.ResetHealth(); // PlayerHealthAction
            movement.ResetPosition(); // PlayerMovement
            physiology?.ResetCooldown(); // AttackPhysiology
        }

        /// <summary>允许玩家操作</summary>
        public void EnableControl()
        {
            movement.EnableControl();
            attack.EnableControl();
        }

        /// <summary>禁止玩家操作</summary>
        public void DisableControl()
        {
            movement.DisableControl();
            attack.DisableControl();
        }

        /// <summary>玩家死亡时由 PlayerHealthAction 调用</summary>
        public void OnPlayerDied()
        {
            DisableControl();
            //GameManager.Instance.OnPlayerDied();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
