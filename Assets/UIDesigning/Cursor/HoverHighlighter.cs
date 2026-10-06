using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// 悬停高亮框。
    /// 鼠标悬停在可交互物体上时，用几帧动画框住该物体。
    /// - 排除 Obstacle 层（地图障碍不框）
    /// - Enemy 等移动物体也跟踪（框跟随物体移动）
    /// - 鼠标离开时框消失
    ///
    /// 挂在持久存在的物体上（比如GameManager）。
    /// highlightPrefab 需要挂 SpriteLoop 脚本（几帧动画循环播放），
    /// 并设置好 Pixels Per Unit 让框的大小合适。
    /// </summary>
    public class HoverHighlighter : MonoBehaviour
    {
        [Header("高亮框预制体")]
        [SerializeField] private GameObject highlightPrefab;   // 高亮框预制体（挂SpriteLoop，几帧动画）
        [SerializeField] private Vector2 highlightOffset = Vector2.zero;  // 框相对于物体的偏移

        [Header("排除层")]
        [SerializeField] private LayerMask excludeLayer;       // 排除的层（比如Obstacle层，不框）

        [Header("检测设置")]
        [SerializeField] private float checkInterval = 0.05f;  // 检测间隔（秒），越小跟踪越及时
        [SerializeField] private string interactableTag = "Interactable";  // 只有这个Tag的物体才显示高亮

        private GameObject highlightInstance;   // 实例化的高亮框
        private Transform currentTarget;        // 当前高亮的物体
        private float nextCheckTime;

        private void Start()
        {
            // 实例化高亮框，默认隐藏
            if (highlightPrefab != null)
            {
                highlightInstance = Instantiate(highlightPrefab);
                highlightInstance.SetActive(false);
            }
        }

        private void Update()
        {
            // 控制检测频率，不用每帧都检测
            if (Time.time < nextCheckTime) return;
            nextCheckTime = Time.time + checkInterval;

            CheckHover();
        }

        private void LateUpdate()
        {
            // 每帧跟随目标移动（Enemy走动时框也跟着走）
            if (currentTarget != null && highlightInstance != null && highlightInstance.activeSelf)
            {
                highlightInstance.transform.position = (Vector2)currentTarget.position + highlightOffset;
            }
        }

        /// <summary>检测鼠标悬停的物体</summary>
        private void CheckHover()
        {
            if (Camera.main == null) return;

            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

            if (hit != null && !IsExcluded(hit.gameObject) && hit.CompareTag(interactableTag))
            {
                // 命中可交互物体（Tag匹配），且不是排除层
                if (currentTarget != hit.transform)
                {
                    currentTarget = hit.transform;
                    ShowHighlight(currentTarget);
                }
            }
            else
            {
                // 没命中，或是排除层，隐藏高亮框
                if (currentTarget != null)
                {
                    currentTarget = null;
                    HideHighlight();
                }
            }
        }

        /// <summary>判断物体是否在排除层</summary>
        private bool IsExcluded(GameObject obj)
        {
            // excludeLayer是LayerMask，值为0表示没设置排除层
            if (excludeLayer.value == 0) return false;
            return ((1 << obj.layer) & excludeLayer.value) != 0;
        }

        /// <summary>显示高亮框，放在目标位置</summary>
        private void ShowHighlight(Transform target)
        {
            if (highlightInstance == null) return;

            highlightInstance.SetActive(true);
            highlightInstance.transform.position = (Vector2)target.position + highlightOffset;
        }

        /// <summary>隐藏高亮框</summary>
        private void HideHighlight()
        {
            if (highlightInstance != null)
                highlightInstance.SetActive(false);
        }

        private void OnDestroy()
        {
            if (highlightInstance != null)
                Destroy(highlightInstance);
        }
    }
}
