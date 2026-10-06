using UnityEngine;
using Game.Core;
using Game.Weapons;

namespace Game.Items
{
    /// <summary>
    /// 攻击范围指示器。
    /// 用LineRenderer实时画一个圆圈，半径跟当前武器攻击范围匹配。
    /// 自己订阅WeaponSwitchedMessage和AttackModeChangedMessage，完全解耦。
    /// 挂在Player下的空物体上，需要LineRenderer组件。
    /// </summary>
    public class AttackRangeIndicator : MonoBehaviour
    {
        [Header("圆圈设置")]
        [SerializeField] private int segments = 32;           // 圆圈点数（越多越圆）
        [SerializeField] private float lineWidth = 0.1f;       // 线宽
        [SerializeField] private Color lineColor = new Color(0.2f, 0.6f, 1f, 0.8f);  // 亮蓝色，半透明

        private LineRenderer lineRenderer;
        private float currentRadius = 2f;

        private void Awake()
        {
            // 自动添加LineRenderer
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            // 初始化LineRenderer
            lineRenderer.positionCount = segments + 1;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = false;

            // 使用Sprites/Default材质，确保2D游戏能显示
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                lineRenderer.material = new Material(shader);
            }

            // 确保在最上层显示
            lineRenderer.sortingOrder = 999;

            // 默认隐藏（用lineRenderer.enabled控制，不用SetActive，保证能订阅消息）
            lineRenderer.enabled = false;
        }

        private void OnEnable()
        {
            // Debug.Log("[AttackRangeIndicator] OnEnable，开始订阅消息");
            MessageBus.Subscribe<WeaponSwitchedMessage>(OnWeaponSwitched);
            MessageBus.Subscribe<AttackModeChangedMessage>(OnAttackModeChanged);
            // Debug.Log("[AttackRangeIndicator] 订阅完成：WeaponSwitchedMessage + AttackModeChangedMessage");
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<WeaponSwitchedMessage>(OnWeaponSwitched);
            MessageBus.Unsubscribe<AttackModeChangedMessage>(OnAttackModeChanged);
        }

        private void Start()
        {
            // Start里再画一次，确保Awake后材质加载完成
            DrawCircle();
        }

        /// <summary>武器切换时更新攻击范围</summary>
        private void OnWeaponSwitched(WeaponSwitchedMessage msg)
        {
            // Debug.Log($"[AttackRangeIndicator] 收到WeaponSwitchedMessage: WeaponID={msg.WeaponID}, Range={msg.Range}");
            SetRadius(msg.Range);
            // Debug.Log($"[AttackRangeIndicator] SetRadius调用完成，currentRadius={currentRadius}");
        }

        /// <summary>攻击模式变化时显示/隐藏</summary>
        private void OnAttackModeChanged(AttackModeChangedMessage msg)
        {
            // Debug.Log($"[AttackRangeIndicator] 收到AttackModeChangedMessage: IsAttackMode={msg.IsAttackMode}");
            if (msg.IsAttackMode)
            {
                Show();
                // Debug.Log("[AttackRangeIndicator] Show调用完成，lineRenderer.enabled=" + lineRenderer.enabled);
            }
            else
            {
                Hide();
                // Debug.Log("[AttackRangeIndicator] Hide调用完成，lineRenderer.enabled=" + lineRenderer.enabled);
            }
        }

        /// <summary>设置圆圈半径（武器攻击范围）</summary>
        public void SetRadius(float weaponRange)
        {
            currentRadius = weaponRange;
            DrawCircle();
        }

        /// <summary>画圆圈</summary>
        private void DrawCircle()
        {
            if (lineRenderer == null) return;

            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * currentRadius;
                float y = Mathf.Sin(angle) * currentRadius;
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            }
        }

        /// <summary>显示</summary>
        public void Show()
        {
            lineRenderer.enabled = true;
            DrawCircle();  // 显示时重新画，确保位置正确
        }

        /// <summary>隐藏</summary>
        public void Hide()
        {
            lineRenderer.enabled = false;
        }

        /// <summary>切换显示/隐藏</summary>
        public void Toggle()
        {
            lineRenderer.enabled = !lineRenderer.enabled;
            if (lineRenderer.enabled)
            {
                DrawCircle();
            }
        }
    }
}
