using UnityEngine;
using TMPro;
using System.Collections;
using Game.Player;

namespace Game.World
{
    /// <summary>
    /// NPC交互距离提示。
    /// 玩家按下交互键但距离NPC太远时，在NPC头顶弹出"So Far Away!"提示。
    /// 挂在NPC上，可复用。
    /// </summary>
    public class NPCDistanceHint : MonoBehaviour
    {
        [Header("提示设置")]
        [SerializeField] private TMP_Text hintText;           // 提示文字（NPC头顶的文字物体）
        [SerializeField] private string hintMessage = "So Far Away!";  // 提示文字内容
        [SerializeField] private float hintDuration = 1f;     // 提示持续时间（秒）
        [SerializeField] private float cooldown = 2f;          // 冷却时间（秒，防止一直弹）
        [SerializeField] private float popHeight = 0.5f;       // 弹出高度（往上弹多少）
        [SerializeField] private float animDuration = 0.3f;    // 弹出/淡出动画时长

        private Transform player;
        private float lastHintTime;
        private Coroutine hintCoroutine;
        private Vector3 textOriginalPos;
        private Collider2D myCollider;
        private Interactable interactable;  // 交互组件（用它的InteractRange，数据统一）

        private void Awake()
        {
            // 自动找玩家
            PlayerMovement pm = FindObjectOfType<PlayerMovement>();
            if (pm != null) player = pm.transform;

            // 自动获取自己的碰撞体和交互组件
            myCollider = GetComponent<Collider2D>();
            interactable = GetComponent<Interactable>();

            // 记录文字原始位置
            if (hintText != null)
            {
                textOriginalPos = hintText.transform.localPosition;
                hintText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            // 玩家按下左键时，先检测点击的是不是自己（NPC）
            if (Input.GetMouseButtonDown(0))
            {
                if (IsClickedOnSelf())
                {
                    CheckDistanceAndHint();
                }
            }
        }

        /// <summary>检测鼠标点击的是不是自己的碰撞体</summary>
        private bool IsClickedOnSelf()
        {
            if (myCollider == null || Camera.main == null) return false;
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return myCollider.OverlapPoint(mouseWorldPos);
        }

        /// <summary>检测玩家距离，太远就提示</summary>
        private void CheckDistanceAndHint()
        {
            if (player == null) return;
            if (hintText == null) return;
            if (interactable == null) return;

            // 冷却中不提示
            if (Time.time - lastHintTime < cooldown) return;

            float distance = Vector2.Distance(player.position, transform.position);

            // 超过交互范围 → 提示（用Interactable的InteractRange，数据统一）
            if (distance > interactable.InteractRange)
            {
                lastHintTime = Time.time;
                ShowHint();
            }
        }

        /// <summary>显示提示动画：弹出+淡入 → 停留 → 淡出</summary>
        private void ShowHint()
        {
            if (hintCoroutine != null) StopCoroutine(hintCoroutine);
            hintCoroutine = StartCoroutine(HintAnimation());
        }

        /// <summary>提示动画协程</summary>
        private IEnumerator HintAnimation()
        {
            hintText.text = hintMessage;
            hintText.gameObject.SetActive(true);

            // 起始位置：原始位置（下方）+ 透明
            hintText.transform.localPosition = textOriginalPos;
            SetTextAlpha(0f);

            // ① 弹出+淡入（线性）
            float t = 0f;
            while (t < animDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / animDuration);
                hintText.transform.localPosition = textOriginalPos + Vector3.up * popHeight * p;
                SetTextAlpha(p);
                yield return null;
            }
            hintText.transform.localPosition = textOriginalPos + Vector3.up * popHeight;
            SetTextAlpha(1f);

            // ② 停留
            yield return new WaitForSeconds(hintDuration);

            // ③ 淡出（线性，位置保持）
            t = 0f;
            while (t < animDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / animDuration);
                SetTextAlpha(1f - p);
                yield return null;
            }
            SetTextAlpha(0f);
            hintText.gameObject.SetActive(false);

            // 恢复文字位置
            hintText.transform.localPosition = textOriginalPos;
        }

        /// <summary>设置文字透明度</summary>
        private void SetTextAlpha(float alpha)
        {
            Color c = hintText.color;
            c.a = alpha;
            hintText.color = c;
        }

        /// <summary>绘制Gizmos（在Scene里显示交互范围）</summary>
        private void OnDrawGizmosSelected()
        {
            if (interactable == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactable.InteractRange);
        }
    }
}
