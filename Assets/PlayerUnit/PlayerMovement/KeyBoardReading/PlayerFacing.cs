using UnityEngine;
using Game.Player;

namespace Game.Player
{
    /// <summary>
    /// 玩家朝向（左右）
    /// </summary>
    public enum PlayerFacingDirection
    {
        Left,   // 左（朝右贴图的镜像）
        Right   // 右（默认朝向）
    }

    /// <summary>
    /// 玩家左右朝向动画控制器。
    /// 只负责左/右方向的帧动画：朝右用原贴图，朝左用 flipX 镜像。
    /// - A（左）：flipX 镜像
    /// - D（右）：默认朝向
    /// - 急刹：播放专门的急刹帧
    ///
    /// 挂在玩家物体上，与 PlayerMovement 配合使用。
    /// 垂直输入不影响朝向（只保留左右动画）。
    /// </summary>
    public class PlayerFacing : MonoBehaviour
    {
        [Header("左右方向（D为默认朝向；A时用flipX镜像）")]
        [SerializeField] private Sprite[] rightWalkFrames;  // 走路帧
        [SerializeField] private Sprite[] rightRunFrames;   // 奔跑帧

        [Header("急刹动画")]
        [SerializeField] private Sprite brakeFrame;          // 急刹帧（冲刺结束时播放）

        [Header("动画设置")]
        [SerializeField] private float walkFrameInterval = 0.15f;  // 走路帧间隔
        [SerializeField] private float runFrameInterval = 0.1f;    // 奔跑帧间隔

        [Header("引用")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private PlayerMovement movement;

        private PlayerFacingDirection currentDirection = PlayerFacingDirection.Right;
        private float animTimer;
        private int currentAnimFrame;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (movement == null)
                movement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            UpdateDirectionByInput();
            PlayAnimationByState();
        }

        /// <summary>根据水平输入更新朝向（只有左/右；无输入保持当前朝向）</summary>
        private void UpdateDirectionByInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");

            if (horizontal > 0)
            {
                currentDirection = PlayerFacingDirection.Right;
            }
            else if (horizontal < 0)
            {
                currentDirection = PlayerFacingDirection.Left;
            }
        }

        /// <summary>根据移动状态播放对应动画</summary>
        private void PlayAnimationByState()
        {
            if (spriteRenderer == null)
            {
                return;
            }
            if (movement == null)
            {
                return;
            }

            // 急刹：播放急刹帧（朝左时也镜像）
            if (movement.IsBraking())
            {
                if (brakeFrame != null)
                {
                    spriteRenderer.sprite = brakeFrame;
                    spriteRenderer.flipX = (currentDirection == PlayerFacingDirection.Left);
                    currentAnimFrame = 0;
                    animTimer = 0f;
                }
                return;
            }

            // 左右共用同一套贴图，靠 flipX 镜像区分
            Sprite[] currentFrames = GetFramesByDirectionAndState();
            float frameInterval = movement.IsRunning() ? runFrameInterval : walkFrameInterval;

            if (currentFrames == null || currentFrames.Length == 0)
            {
                return;
            }

            // 左右镜像
            spriteRenderer.flipX = (currentDirection == PlayerFacingDirection.Left);

            if (movement.IsMoving())
            {
                // 移动或滑行中，继续播动画
                animTimer += Time.deltaTime;
                if (animTimer >= frameInterval)
                {
                    animTimer = 0f;
                    currentAnimFrame = (currentAnimFrame + 1) % currentFrames.Length;
                }
            }
            else
            {
                // 速度归零，显示第一帧
                currentAnimFrame = 0;
            }

            // Safety: make sure frame index is within array bounds
            // (currentAnimFrame is a member var, may exceed new array length when switching directions)
            if (currentAnimFrame >= currentFrames.Length)
            {
                currentAnimFrame = 0;
            }

            spriteRenderer.sprite = currentFrames[currentAnimFrame];
        }

        /// <summary>获取当前帧数组（左右共用：走路/奔跑）</summary>
        private Sprite[] GetFramesByDirectionAndState()
        {
            return movement.IsRunning() ? rightRunFrames : rightWalkFrames;
        }

        /// <summary>获取当前朝向（供其他脚本读取）</summary>
        public PlayerFacingDirection GetCurrentDirection() => currentDirection;

        /// <summary>获取当前朝向的单位向量（供射击角度限制用）</summary>
        public Vector2 GetFacingVector()
        {
            return currentDirection == PlayerFacingDirection.Left ? Vector2.left : Vector2.right;
        }
    }
}
