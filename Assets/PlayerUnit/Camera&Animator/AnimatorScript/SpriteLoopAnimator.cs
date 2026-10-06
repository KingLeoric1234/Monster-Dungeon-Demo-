using UnityEngine;
using UnityEngine.UI;

namespace Game.Effect
{
    /// <summary>循环帧动画：拖入图片自动循环播放（通用，支持SpriteRenderer和UI Image）</summary>
    public class SpriteLoopAnimator : MonoBehaviour
    {
        [Header("序列帧")]
        [SerializeField] private Sprite[] frames;   // 拖入所有帧图片

        [Header("设置")]
        [SerializeField] private float frameInterval = 0.1f;  // 每帧间隔（秒），越小越快

        private SpriteRenderer spriteRenderer;
        private Image image;  // UI Image组件支持
        private float timer;
        private int currentFrame;

        private void Awake()
        {
            // 先找SpriteRenderer，找不到再找Image（UI用）
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                image = GetComponent<Image>();
            }
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;
            if (spriteRenderer == null && image == null) return;

            timer += Time.deltaTime;
            if (timer >= frameInterval)
            {
                timer = 0;
                currentFrame = (currentFrame + 1) % frames.Length;  // 循环播放

                // 根据组件类型更新sprite
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = frames[currentFrame];
                }
                else if (image != null)
                {
                    image.sprite = frames[currentFrame];
                }
            }
        }
    }
}
