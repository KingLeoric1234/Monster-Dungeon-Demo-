using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// 无敌帧 + 闪烁（独立状态机）：被 PlayerHealthAction 单向调用。
    /// 不碰血量——只回答"能挨打吗"（CanTakeDamage）、提供启动/重置。
    /// 挂在 Player 物体上。
    /// </summary>
    public class Invincibility : MonoBehaviour
    {
        // 代码驱动：不拖引用。Player 结构确认后自动抓取 Sprite；
        // 抓不到（结构未定/无 Sprite）则跳过闪烁，不影响无敌帧逻辑。
        private SpriteRenderer spriteRenderer;

        private bool isInvincible;
        private float invincibleTimer;
        private float blinkTimer;

        private void Awake()
        {
            isInvincible = false;
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            if (spriteRenderer == null)
                Debug.LogWarning("[Invincibility] 未找到 SpriteRenderer（Player 结构未定？），无敌帧期间不会闪烁，不影响逻辑");
        }

        private void Update()
        {
            if (!isInvincible) return; // 不在无敌帧就不干活

            invincibleTimer -= Time.deltaTime;
            blinkTimer -= Time.deltaTime;

            // 闪烁
            if (blinkTimer <= 0)
            {
                if (spriteRenderer != null)
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                blinkTimer = PlayerConfig.BlinkInterval;
            }

            // 无敌帧结束
            if (invincibleTimer <= 0)
            {
                isInvincible = false;
                if (spriteRenderer != null)
                    spriteRenderer.enabled = true;
            }
        }

        /// <summary>是否处于无敌帧（false = 可以挨打）</summary>
        public bool CanTakeDamage() => !isInvincible;

        /// <summary>启动无敌帧（受击后由 PlayerHealthAction 调用）</summary>
        public void StartInvincibility()
        {
            isInvincible = true;
            invincibleTimer = PlayerConfig.InvincibleDuration;
            blinkTimer = 0;
        }

        /// <summary>清除无敌帧 + 恢复渲染（重置/复活时调用）</summary>
        public void Reset()
        {
            isInvincible = false;
            invincibleTimer = 0;
            if (spriteRenderer != null)
                spriteRenderer.enabled = true;
        }
    }
}
