using System.Collections;
using UnityEngine;

namespace Game.Items
{
    /// <summary>
    /// 通用攻击特效播放脚本。
    /// 拖入动画帧，播放时根据攻击方向左右镜像翻转。
    /// 空手、长剑、手枪、狙击枪都可以用这个脚本，只是拖不同的帧。
    /// 挂在攻击特效物体上，需要SpriteRenderer组件。
    /// </summary>
    public class AttackEffect : MonoBehaviour
    {
        [Header("动画帧")]
        [SerializeField] private Sprite[] attackFrames;   // 拖入所有攻击帧

        [Header("设置")]
        [SerializeField] private float frameInterval = 0.05f;  // 每帧间隔（秒），越小越快

        private SpriteRenderer spriteRenderer;
        private Coroutine playCoroutine;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            gameObject.SetActive(false);
        }

        /// <summary>播放攻击特效，direction是攻击方向</summary>
        public void Play(Vector2 direction)
        {
            if (attackFrames == null || attackFrames.Length == 0) return;

            // 如果正在播放，先停掉
            if (playCoroutine != null)
                StopCoroutine(playCoroutine);

            gameObject.SetActive(true);
            playCoroutine = StartCoroutine(PlayAnimation(direction));
        }

        /// <summary>获取动画总时长</summary>
        public float GetAnimationDuration()
        {
            if (attackFrames == null || attackFrames.Length == 0) return 0f;
            return attackFrames.Length * frameInterval;
        }

        private IEnumerator PlayAnimation(Vector2 direction)
        {
            // 左右镜像翻转：攻击方向x<0就翻转
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0;
            }

            // 逐帧播放
            for (int i = 0; i < attackFrames.Length; i++)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = attackFrames[i];
                }
                yield return new WaitForSeconds(frameInterval);
            }

            // 播完隐藏
            gameObject.SetActive(false);
            playCoroutine = null;
        }
    }
}
