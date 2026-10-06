using UnityEngine;
using Game.Player;

namespace Game.Player
{
    /// <summary>
    /// 玩家朝向（四方向）
    /// </summary>
    public enum PlayerFacingDirection
    {
        Up,     // 上（背对玩家）
        Down,   // 下（面对玩家）
        Left,   // 左（朝右贴图的镜像）
        Right   // 右（默认朝向）
    }

    /// <summary>
    /// 玩家四方向动画控制器。
    /// 根据移动方向和状态（走路/奔跑/急刹），播放对应的帧动画。
    /// - W（上）：背对玩家的贴图
    /// - S（下）：面对玩家的贴图
    /// - A/D（左右）：朝右贴图，A时flipX镜像
    /// - 急刹：播放专门的急刹帧
    ///
    /// 挂在玩家物体上，与 PlayerMovement 配合使用。
    /// </summary>
    public class PlayerFacing : MonoBehaviour
    {
        [Header("上方向（W，背对玩家）")]
        [SerializeField] private Sprite[] upWalkFrames;     // 上方向走路帧
        [SerializeField] private Sprite[] upRunFrames;      // 上方向奔跑帧

        [Header("下方向（S，面对玩家）")]
        [SerializeField] private Sprite[] downWalkFrames;   // 下方向走路帧
        [SerializeField] private Sprite[] downRunFrames;    // 下方向奔跑帧

        [Header("右方向（D，默认朝向；A时用flipX镜像）")]
        [SerializeField] private Sprite[] rightWalkFrames;  // 右方向走路帧
        [SerializeField] private Sprite[] rightRunFrames;   // 右方向奔跑帧

        [Header("急刹动画")]
        [SerializeField] private Sprite brakeFrame;          // 急刹帧（冲刺结束时播放）

        [Header("动画设置")]
        [SerializeField] private float walkFrameInterval = 0.15f;  // 走路帧间隔
        [SerializeField] private float runFrameInterval = 0.1f;    // 奔跑帧间隔

        [Header("引用")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private PlayerMovement movement;

        private PlayerFacingDirection currentDirection = PlayerFacingDirection.Down;
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

        /// <summary>根据键盘输入更新朝向（优先垂直方向，其次水平方向）</summary>
        private void UpdateDirectionByInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            // 没有输入就保持当前朝向
            if (horizontal == 0 && vertical == 0) return;

            // 优先判断垂直方向（W=上，S=下）
            if (vertical > 0)
            {
                currentDirection = PlayerFacingDirection.Up;
            }
            else if (vertical < 0)
            {
                currentDirection = PlayerFacingDirection.Down;
            }
            // 没有垂直输入时判断水平方向
            else if (horizontal > 0)
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
                // Debug.LogWarning("[PlayerFacing] spriteRenderer 为 null！");
                return;
            }
            if (movement == null)
            {
                // Debug.LogWarning("[PlayerFacing] movement 为 null！");
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

            // 获取当前方向对应的帧数组
            Sprite[] currentFrames = GetFramesByDirectionAndState();
            float frameInterval = movement.IsRunning() ? runFrameInterval : walkFrameInterval;

            if (currentFrames == null || currentFrames.Length == 0)
            {
                // Debug.LogWarning($"[PlayerFacing] 当前方向={currentDirection}, 奔跑={movement.IsRunning()}, 帧数组为空！请拖入对应贴图");
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

        /// <summary>根据当前方向和状态获取对应的帧数组</summary>
        private Sprite[] GetFramesByDirectionAndState()
        {
            bool isRunning = movement.IsRunning();

            switch (currentDirection)
            {
                case PlayerFacingDirection.Up:
                    return isRunning ? upRunFrames : upWalkFrames;
                case PlayerFacingDirection.Down:
                    return isRunning ? downRunFrames : downWalkFrames;
                case PlayerFacingDirection.Left:
                case PlayerFacingDirection.Right:
                    return isRunning ? rightRunFrames : rightWalkFrames;
                default:
                    return downWalkFrames;
            }
        }

        /// <summary>获取当前朝向（供其他脚本读取）</summary>
        public PlayerFacingDirection GetCurrentDirection() => currentDirection;

        /// <summary>获取当前朝向的单位向量（供射击角度限制用）</summary>
        public Vector2 GetFacingVector()
        {
            switch (currentDirection)
            {
                case PlayerFacingDirection.Up:
                    return Vector2.up;
                case PlayerFacingDirection.Down:
                    return Vector2.down;
                case PlayerFacingDirection.Left:
                    return Vector2.left;
                case PlayerFacingDirection.Right:
                    return Vector2.right;
                default:
                    return Vector2.down;
            }
        }
    }
}
