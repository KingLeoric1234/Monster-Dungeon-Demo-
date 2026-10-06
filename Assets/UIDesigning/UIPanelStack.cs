using UnityEngine;
using System.Collections.Generic;

namespace Game.UI
{
    /// <summary>
    /// UI面板管理器：统一管理所有面板的打开/关闭，按Tab关闭最上层面板。
    /// 解决多个面板同时监听快捷键导致冲突的问题。
    /// </summary>
    public class UIPanelStack : MonoBehaviour
    {
        public static UIPanelStack Instance { get; private set; }

        [Header("引用")]
        [SerializeField] private StatusPanelDirector statusPanel;  // 状态面板（没有其他面板时按Tab打开）

        // 面板栈：打开时Push，关闭时Pop，按Tab关闭最上层
        private Stack<GameObject> panelStack = new Stack<GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // 先移到根层级，再DontDestroyOnLoad（否则会报警告）
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // 按Tab：如果有面板打开，关闭最上层；如果没有，打开StatusPanel
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (panelStack.Count > 0)
                {
                    GameObject topPanel = panelStack.Peek();
                    if (topPanel != null)
                    {
                        // 调用面板的关闭方法（通过SendMessage）
                        topPanel.SendMessage("CloseAndNotify", SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        panelStack.Pop(); // 面板已被销毁，弹出
                    }
                }
                else
                {
                    // 没有其他面板打开，打开StatusPanel
                    if (statusPanel != null)
                    {
                        statusPanel.TogglePanel();
                    }
                }
            }
        }

        /// <summary>面板打开时调用，注册到栈顶</summary>
        public void RegisterPanel(GameObject panel)
        {
            if (panel != null && !panelStack.Contains(panel))
            {
                panelStack.Push(panel);
                // Debug.Log($"[UIPanelStack] 注册面板: {panel.name}, 当前栈深度: {panelStack.Count}");
            }
        }

        /// <summary>面板关闭时调用，从栈中移除</summary>
        public void UnregisterPanel(GameObject panel)
        {
            if (panel != null && panelStack.Contains(panel))
            {
                // 重新构建栈，移除指定面板
                var newStack = new Stack<GameObject>();
                foreach (var p in panelStack)
                {
                    if (p != panel) newStack.Push(p);
                }
                panelStack = newStack;
                // Debug.Log($"[UIPanelStack] 注销面板: {panel.name}, 当前栈深度: {panelStack.Count}");
            }
        }

        /// <summary>是否有面板打开</summary>
        public bool HasAnyPanelOpen()
        {
            return panelStack.Count > 0;
        }

        /// <summary>获取最上层面板</summary>
        public GameObject GetTopPanel()
        {
            return panelStack.Count > 0 ? panelStack.Peek() : null;
        }
    }
}

