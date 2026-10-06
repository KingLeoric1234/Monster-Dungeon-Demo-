using UnityEngine;

public class EnemyFacing : MonoBehaviour
{
    public enum FacingMode
    {
        Flip,    // 翻转模式（普通怪用，只有左右/上下翻转）
        Rotate   // 旋转模式（Boss用，四面八方转）
    }

    public enum DefaultFacing
    {
        Left,
        Right,
        Down,
        Up
    }

    public enum RotateMode
    {
        Smooth,     // 平滑旋转（连续转，适合可以任意方向转的）
        EightWay    // 八方向锁定（每45度一档，适合有8个方向图片的）
    }

    [Header("朝向模式")]
    [SerializeField] private FacingMode facingMode = FacingMode.Flip;

    [Header("翻转模式设置（普通怪用）")]
    [SerializeField] private DefaultFacing defaultFacing = DefaultFacing.Left;

    [Header("旋转模式设置（Boss用）")]
    [SerializeField] private RotateMode rotateMode = RotateMode.Smooth;
    [SerializeField] private float rotateSpeed = 10f;   // 平滑旋转速度
    [SerializeField] private DefaultFacing rotateDefaultFacing = DefaultFacing.Up;  // 旋转模式下，图片默认朝哪

    [Header("引用")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null) return;

        if (facingMode == FacingMode.Flip)
            UpdateFacingByFlip();
        else
            UpdateFacingByRotate();
    }

    // ===== 翻转模式（普通怪）=====
    private void UpdateFacingByFlip()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        switch (defaultFacing)
        {
            case DefaultFacing.Left:
                spriteRenderer.flipX = direction.x > 0;
                break;
            case DefaultFacing.Right:
                spriteRenderer.flipX = direction.x < 0;
                break;
            case DefaultFacing.Down:
                spriteRenderer.flipY = direction.y > 0;
                break;
            case DefaultFacing.Up:
                spriteRenderer.flipY = direction.y < 0;
                break;
        }
    }

    // ===== 旋转模式（Boss）=====
    private void UpdateFacingByRotate()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // 计算目标角度（Atan2返回弧度，转成角度）
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 根据图片默认朝向修正角度
        // 比如图片默认朝上（Up），需要加90度偏移
        switch (rotateDefaultFacing)
        {
            case DefaultFacing.Right:
                // 默认朝右，不需要偏移
                break;
            case DefaultFacing.Up:
                // 默认朝上，加90度
                targetAngle -= 90f;
                break;
            case DefaultFacing.Left:
                // 默认朝左，加180度
                targetAngle -= 180f;
                break;
            case DefaultFacing.Down:
                // 默认朝下，减90度
                targetAngle += 90f;
                break;
        }

        if (rotateMode == RotateMode.EightWay)
        {
            // 八方向锁定：把角度取整到最近的45度
            targetAngle = Mathf.Round(targetAngle / 45f) * 45f;
        }

        // 应用旋转
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        if (rotateMode == RotateMode.Smooth)
        {
            // 平滑旋转
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime);
        }
        else
        {
            // 八方向直接转
            transform.rotation = targetRotation;
        }
    }

    /// <summary>
    /// 外部设置玩家引用
    /// </summary>
    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }
}

