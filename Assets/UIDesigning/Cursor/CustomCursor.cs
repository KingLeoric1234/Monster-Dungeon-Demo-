using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using Game.Core;

namespace Game.UI
{
    /// <summary>
    /// 光标状态
    /// </summary>
    public enum CursorState
    {
        Default,    // 默认箭头
        Hover,      // 悬停在碰撞体上（手套）
        Click,      // 鼠标按下时（手套点下帧）
        Attack,     // 悬停在敌人上（小剑）
        Busy        // 忙碌/无法操作（沙漏转动）
    }

    /// <summary>
    /// 自定义多状态光标（简化版）。
    /// - 默认：像素箭头
    /// - 鼠标下有任何碰撞体（NPC/宝箱/门/障碍物等）：手套悬停帧
    /// - 鼠标按下：手套点下帧
    /// - 忙碌/转场/运镜：沙漏多帧转动动画
    ///
    /// 挂在持久存在的物体上（比如GameManager），整个游戏生效。
    /// 外部调用 SetBusy(true/false) 切换忙碌状态。
    /// </summary>
    public class CustomCursor : MonoBehaviour
    {
        [Header("默认光标")]
        [SerializeField] private Texture2D defaultCursor;      // 默认箭头
        [SerializeField] private Vector2 defaultHotspot = Vector2.zero;  // 热点（箭头左上角）

        [Header("悬停（手套）")]
        [SerializeField] private Texture2D hoverCursor;        // 手套悬停帧
        [SerializeField] private Vector2 hoverHotspot = Vector2.zero;     // 热点（手套指尖）

        [Header("点击（手套点下帧）")]
        [SerializeField] private Texture2D clickCursor;        // 手套点下帧
        [SerializeField] private Vector2 clickHotspot = Vector2.zero;     // 热点

        [Header("攻击（小剑）")]
        [SerializeField] private Texture2D attackCursor;       // 小剑Icon
        [SerializeField] private Vector2 attackHotspot = Vector2.zero;    // 热点（剑尖）

        [Header("忙碌（沙漏多帧动画）")]
        [SerializeField] private Texture2D[] busyCursorFrames; // 沙漏帧数组（按顺序拖入）
        [SerializeField] private Vector2 busyHotspot = Vector2.zero;       // 热点（沙漏中心）
        [SerializeField] private float busyFrameInterval = 0.15f;          // 每帧间隔（秒），越小转越快

        private CursorState currentState = CursorState.Default;
        private Coroutine busyAnimationCoroutine;
        private bool isMouseDown = false;
        private bool isAttackMode;  // 攻击模式状态（订阅AttackModeChangedMessage）

        private void OnEnable()
        {
            MessageBus.Subscribe<AttackModeChangedMessage>(OnAttackModeChanged);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<AttackModeChangedMessage>(OnAttackModeChanged);
        }

        private void OnAttackModeChanged(AttackModeChangedMessage msg)
        {
            isAttackMode = msg.IsAttackMode;
        }

        private void Start()
        {
            ApplyCursor(CursorState.Default);
        }

        private void Update()
        {
            // 忙碌状态时不做其他检测
            if (currentState == CursorState.Busy) return;

            // 鼠标按下/松开检测
            if (Input.GetMouseButtonDown(0))
            {
                isMouseDown = true;
                // Hover状态变Click，Attack状态保持Attack
                if (currentState == CursorState.Hover)
                    ApplyCursor(CursorState.Click);
            }
            if (Input.GetMouseButtonUp(0))
            {
                isMouseDown = false;
                UpdateHoverState();  // 松开后重新检测悬停
            }

            // 没按鼠标时，持续检测悬停
            if (!isMouseDown)
            {
                UpdateHoverState();
            }
        }

        /// <summary>检测鼠标下有没有碰撞体或UI，更新悬停状态</summary>
        private void UpdateHoverState()
        {
            // 攻击模式下，强制显示小剑
            if (isAttackMode)
            {
                if (currentState != CursorState.Attack)
                {
                    ApplyCursor(CursorState.Attack);
                }
                return;
            }

            // 检测2D世界碰撞体
            bool hit2D = false;
            bool hitEnemy = false;
            if (Camera.main != null)
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
                hit2D = hit != null;
                // 检测是不是Enemy
                if (hit != null && hit.CompareTag("Enemy"))
                {
                    hitEnemy = true;
                }
            }

            // 检测UI
            bool hitUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            // 优先级：Enemy > UI/碰撞体 > 默认
            if (hitEnemy && currentState != CursorState.Attack)
            {
                ApplyCursor(CursorState.Attack);
            }
            else if (!hitEnemy && (hit2D || hitUI) && currentState != CursorState.Hover)
            {
                ApplyCursor(CursorState.Hover);
            }
            else if (!hit2D && !hitUI && !hitEnemy && currentState != CursorState.Default)
            {
                ApplyCursor(CursorState.Default);
            }
        }

        /// <summary>设置忙碌状态（运镜/转场时调用）</summary>
        public void SetBusy(bool busy)
        {
            if (busy && currentState != CursorState.Busy)
            {
                ApplyCursor(CursorState.Busy);
                StartBusyAnimation();
            }
            else if (!busy && currentState == CursorState.Busy)
            {
                StopBusyAnimation();
                ApplyCursor(CursorState.Default);
            }
        }

        /// <summary>应用指定状态的光标</summary>
        private void ApplyCursor(CursorState state)
        {
            currentState = state;

            switch (state)
            {
                case CursorState.Default:
                    if (IsTextureValid(defaultCursor))
                        Cursor.SetCursor(defaultCursor, defaultHotspot, CursorMode.Auto);
                    break;
                case CursorState.Hover:
                    if (IsTextureValid(hoverCursor))
                        Cursor.SetCursor(hoverCursor, hoverHotspot, CursorMode.Auto);
                    break;
                case CursorState.Click:
                    if (IsTextureValid(clickCursor))
                        Cursor.SetCursor(clickCursor, clickHotspot, CursorMode.Auto);
                    break;
                case CursorState.Attack:
                    if (IsTextureValid(attackCursor))
                        Cursor.SetCursor(attackCursor, attackHotspot, CursorMode.Auto);
                    break;
                case CursorState.Busy:
                    if (busyCursorFrames != null && busyCursorFrames.Length > 0 && IsTextureValid(busyCursorFrames[0]))
                        Cursor.SetCursor(busyCursorFrames[0], busyHotspot, CursorMode.Auto);
                    break;
            }
        }

        /// <summary>检查纹理是否符合Cursor要求（不符合就不设置，避免警告）</summary>
        private bool IsTextureValid(Texture2D tex)
        {
            if (tex == null) return false;
            try
            {
                // 尝试读取像素，如果不可读会抛异常
                tex.GetPixel(0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>开始沙漏转动动画</summary>
        private void StartBusyAnimation()
        {
            StopBusyAnimation();
            busyAnimationCoroutine = StartCoroutine(BusyAnimationLoop());
        }

        /// <summary>停止沙漏转动动画</summary>
        private void StopBusyAnimation()
        {
            if (busyAnimationCoroutine != null)
            {
                StopCoroutine(busyAnimationCoroutine);
                busyAnimationCoroutine = null;
            }
        }

        /// <summary>沙漏多帧循环动画</summary>
        private IEnumerator BusyAnimationLoop()
        {
            if (busyCursorFrames == null || busyCursorFrames.Length == 0) yield break;

            int frameIndex = 0;
            while (currentState == CursorState.Busy)
            {
                Cursor.SetCursor(busyCursorFrames[frameIndex], busyHotspot, CursorMode.Auto);
                frameIndex = (frameIndex + 1) % busyCursorFrames.Length;
                yield return new WaitForSeconds(busyFrameInterval);
            }
        }

        /// <summary>恢复系统默认光标</summary>
        public void ResetToDefault()
        {
            StopBusyAnimation();
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            currentState = CursorState.Default;
        }

        private void OnDestroy()
        {
            StopBusyAnimation();
        }
    }
}
