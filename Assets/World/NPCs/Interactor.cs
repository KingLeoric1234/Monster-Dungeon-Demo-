using UnityEngine;
using Game.Core;

namespace Game.World
{
    /// <summary>交互检测器：检测鼠标指向的物块，发布提示/确认消息</summary>
    public class Interactor : MonoBehaviour
    {
        private Interactable currentTarget;  // 当前交互的物块

        private void Update()
        {
            Interactable hovered = GetHoveredInteractable();

            // 鼠标左键点击 + 指向可交互物块 + 在交互范围内 → 显示对话框
            if (Input.GetMouseButtonDown(0) && hovered != null && InRange(hovered))
            {
                currentTarget = hovered;
                MessageBus.Publish(new InteractPromptMessage
                {
                    Show = true,
                    PromptText = hovered.PromptText,
                    Target = hovered
                });
            }

            // 玩家离开当前物块太远 → 隐藏对话框
            if (currentTarget != null && !InRange(currentTarget))
            {
                MessageBus.Publish(new InteractPromptMessage
                {
                    Show = false,
                    Target = currentTarget
                });

                currentTarget = null;
            }
        }

        /// <summary>鼠标指向的物块（从Camera向鼠标位置射线检测）</summary>
        private Interactable GetHoveredInteractable()
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);
            return hit.collider != null ? hit.collider.GetComponent<Interactable>() : null;
        }

        /// <summary>玩家与物块距离是否在交互范围内</summary>
        private bool InRange(Interactable target)
        {
            float dist = Vector2.Distance(transform.position, target.transform.position);
            return dist <= target.InteractRange;
        }
    }
}
