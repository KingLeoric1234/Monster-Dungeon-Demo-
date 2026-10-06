using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// 狙击镜UI。
    /// - 右键打开/关闭
    /// - 打开时：Cursor隐藏，狙击镜平滑跟随鼠标，相机平滑拉远
    /// - 用CircleMask Shader实现：全屏黑色遮罩 + 圆形区域挖空可视
    /// - ScopeRing只控制位置，显示内容手动设置
    /// 挂在UIDirector/Canvas下的全屏Panel上。
    /// </summary>
    public class SniperScopeUI : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Image maskImage;       // 全屏黑色遮罩（用CircleMask Shader）
        [SerializeField] private RectTransform scopeRing;  // 圈圈边框（只控制位置）
        [SerializeField] private Image centerDot;     // 中心小点
        [SerializeField] private Camera mainCamera;   // 主相机
        [SerializeField] private Transform playerTransform;  // 玩家Transform（用于计算最小观察距离）

        [Header("狙击镜设置")]
        [SerializeField] private float scopeRadius = 150f;    // 圈圈半径（像素）
        [SerializeField] private float zoomedCameraSize = 8f; // 开镜后Camera.Size
        [SerializeField] private float minDistanceFromPlayer = 1f;  // 狙击镜圆心离玩家的最小距离（世界单位）
        private float normalCameraSize = 5f;                   // 正常Camera.Size（Awake时自动读取）
        [SerializeField] private Color maskColor = Color.black; // 遮罩颜色

        [Header("平滑手感")]
        [SerializeField] private float scopeSmoothSpeed = 8f;   // 狙击镜跟随鼠标的平滑速度
        [SerializeField] private float cameraSmoothSpeed = 5f;  // 相机拉远的平滑速度
        [SerializeField] private float edgeSoftness = 0.005f;   // 圆形边缘柔和度

        private bool isScopeOpen = false;
        private Canvas canvas;
        private Vector2 currentScopePosition;
        private float targetCameraSize;
        private Material maskMaterial;  // 运行时创建的材质实例

        /// <summary>静态属性：狙击镜是否打开（供其他脚本访问）</summary>
        public static bool IsScopeOpenStatic { get; private set; }

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            // 创建CircleMask材质实例（不修改原材质）
            if (maskImage != null)
            {
                Shader circleMaskShader = Shader.Find("UI/CircleMask");
                if (circleMaskShader != null)
                {
                    maskMaterial = new Material(circleMaskShader);
                    maskMaterial.SetColor("_Color", maskColor);
                    // 半径用高度方向的UV单位（Shader里x方向会乘以宽高比，保持圆形）
                    maskMaterial.SetFloat("_Radius", scopeRadius / Screen.height);
                    maskMaterial.SetFloat("_EdgeSoftness", edgeSoftness);
                    maskImage.material = maskMaterial;
                    maskImage.color = Color.white;  // Image本身设为白色，颜色由Shader控制
                }
                else
                {
                    Debug.LogError("[SniperScope] 找不到UI/CircleMask Shader！");
                }
            }

            // 中心小点默认大小
            if (centerDot != null)
            {
                centerDot.rectTransform.sizeDelta = new Vector2(8, 8);
                centerDot.color = Color.red;
            }

            // 读取相机默认Size
            if (mainCamera != null)
            {
                normalCameraSize = mainCamera.orthographicSize;
            }
            targetCameraSize = normalCameraSize;

            // 默认隐藏
            gameObject.SetActive(false);
        }

        private void Update()
        {
            // 相机Size平滑过渡
            if (mainCamera != null && Mathf.Abs(mainCamera.orthographicSize - targetCameraSize) > 0.01f)
            {
                mainCamera.orthographicSize = Mathf.Lerp(
                    mainCamera.orthographicSize,
                    targetCameraSize,
                    cameraSmoothSpeed * Time.deltaTime
                );
                if (Mathf.Abs(mainCamera.orthographicSize - targetCameraSize) < 0.02f)
                {
                    mainCamera.orthographicSize = targetCameraSize;
                }
            }

            if (!isScopeOpen) return;

            UpdateScopePosition();
        }

        /// <summary>更新狙击镜位置（平滑跟随鼠标 + 最小距离钳制）</summary>
        private void UpdateScopePosition()
        {
            Vector2 mousePos = Input.mousePosition;

            // 最小距离钳制：狙击镜圆心不能进入玩家周围半径minDistanceFromPlayer的区域
            if (playerTransform != null && mainCamera != null && minDistanceFromPlayer > 0)
            {
                // 玩家世界坐标转屏幕坐标
                Vector2 playerScreenPos = mainCamera.WorldToScreenPoint(playerTransform.position);

                // 世界单位转像素：相机orthographicSize = 屏幕高度的一半（世界单位）
                float pixelsPerUnit = Screen.height / (2f * mainCamera.orthographicSize);
                float minDistancePixels = minDistanceFromPlayer * pixelsPerUnit;

                // 计算鼠标相对于玩家的方向和距离
                Vector2 offset = mousePos - playerScreenPos;
                float distance = offset.magnitude;

                // 如果距离小于最小距离，钳制在边界上
                if (distance < minDistancePixels && distance > 0.001f)
                {
                    Vector2 direction = offset.normalized;
                    mousePos = playerScreenPos + direction * minDistancePixels;
                }
            }

            // 平滑跟随
            currentScopePosition = Vector2.Lerp(
                currentScopePosition,
                mousePos,
                scopeSmoothSpeed * Time.deltaTime
            );

            // 更新遮罩的圆心位置（转换成UV空间：0-1）
            if (maskMaterial != null)
            {
                Vector2 uvCenter = new Vector2(
                    currentScopePosition.x / Screen.width,
                    currentScopePosition.y / Screen.height
                );
                maskMaterial.SetVector("_Center", new Vector4(uvCenter.x, uvCenter.y, 0, 0));
            }

            // ScopeRing只控制位置
            if (scopeRing != null)
            {
                scopeRing.position = currentScopePosition;
            }
            if (centerDot != null)
            {
                centerDot.rectTransform.position = currentScopePosition;
            }
        }

        /// <summary>打开狙击镜</summary>
        public void OpenScope()
        {
            isScopeOpen = true;
            IsScopeOpenStatic = true;
            gameObject.SetActive(true);
            Cursor.visible = false;
            currentScopePosition = Input.mousePosition;
            targetCameraSize = zoomedCameraSize;
        }

        /// <summary>关闭狙击镜</summary>
        public void CloseScope()
        {
            isScopeOpen = false;
            IsScopeOpenStatic = false;
            Cursor.visible = true;

            if (mainCamera != null)
            {
                mainCamera.orthographicSize = normalCameraSize;
            }
            targetCameraSize = normalCameraSize;

            gameObject.SetActive(false);
        }

        /// <summary>切换狙击镜</summary>
        public void ToggleScope()
        {
            if (isScopeOpen)
            {
                CloseScope();
            }
            else
            {
                OpenScope();
            }
        }

        /// <summary>狙击镜是否打开</summary>
        public bool IsScopeOpen => isScopeOpen;
    }
}
