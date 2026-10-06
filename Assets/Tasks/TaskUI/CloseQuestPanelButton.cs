using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.Tasks
{
    /// <summary>
    /// 关闭任务面板按钮：点击发CloseQuestPanelMessage，TaskDirector收到后播关闭动画。
    /// 挂在叉号Button上，自动获取Button组件。
    /// </summary>
    public class CloseQuestPanelButton : MonoBehaviour
    {
        private void Start()
        {
            var btn = GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            MessageBus.Publish(new CloseQuestPanelMessage());
        }
    }

    /// <summary>关闭任务面板消息</summary>
    public class CloseQuestPanelMessage : GameMessage { }
}
