using UnityEngine;
using Game.Core;
using Game.Player;

namespace Game.World
{
    /// <summary>NPC动画：待机上下摆动 + 朝向玩家（左右镜像）。可复用，挂在NPC的Sprite子物体上</summary>
    public class NPCBobbing : MonoBehaviour
    {
        [Header("摆动设置")]
        [SerializeField] private float bobHeight = 0.05f;   // 上下摆动幅度
        [SerializeField] private float bobSpeed = 1.5f;      // 一次完整来回的时间（秒），越小越快

        [Header("朝向玩家")]
        [SerializeField] private bool facePlayer = true;        // 是否朝向玩家
        [SerializeField] private bool defaultFacingLeft = true; // NPC默认朝左（true=朝左，false=朝右）

        private SpriteRenderer spriteRenderer;
        private Transform player;
        private Vector3 basePos;
        private float t = 0f;

        private void Start()
        {
            basePos = transform.localPosition;
            spriteRenderer = GetComponent<SpriteRenderer>();
            TryFindPlayer();
        }

        private void Update()
        {
            // 玩家还没找到就再试一次（跨场景加载时可能延迟）
            if (player == null) TryFindPlayer();

            // ① 上下摆动（正弦波）
            t += Time.deltaTime;
            float offset = Mathf.Sin(t * (2f * Mathf.PI / bobSpeed)) * bobHeight;
            transform.localPosition = new Vector3(basePos.x, basePos.y + offset, basePos.z);

            // ② 朝向玩家（左右镜像）
            if (facePlayer && player != null && spriteRenderer != null)
            {
                bool playerOnRight = player.position.x > transform.position.x;

                // 默认朝左：玩家在右边 → 翻转（朝右）
                // 默认朝右：玩家在右边 → 不翻转
                if (defaultFacingLeft)
                    spriteRenderer.flipX = playerOnRight;
                else
                    spriteRenderer.flipX = !playerOnRight;
            }
        }

        /// <summary>通过PlayerDirector找玩家（不需要Tag）</summary>
        private void TryFindPlayer()
        {
            if (PlayerDirector.Instance != null)
                player = PlayerDirector.Instance.transform;
        }
    }
}
