using UnityEngine;
using Game.Core;
using Game.Tasks;

namespace Game.World
{
    /// <summary>NPC行为：确认后进入地牢（挂NPC上）</summary>
    public class EnterDungeonNPC : MonoBehaviour
    {
        [SerializeField] private int targetLayer = 1;

        private void OnEnable() => MessageBus.Subscribe<InteractConfirmMessage>(OnConfirm);
        private void OnDisable() => MessageBus.Unsubscribe<InteractConfirmMessage>(OnConfirm);

        private void OnConfirm(InteractConfirmMessage msg)
        {
            Interactable own = GetComponent<Interactable>();
            if (msg.Target != own) return;
            MessageBus.Publish(new EnterDungeonRequestMessage { Layer = targetLayer });

            if (TaskDirector.Instance != null)
                TaskDirector.Instance.CompleteTask(TaskConfig.TalkToNpcId);
        }
    }
}
