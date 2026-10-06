using UnityEngine;
using Game.Core;
using Game.World;

namespace Game.Player
{
    /// <summary>地牢生成后，接收出生点消息，自己挪位置</summary>
    public class PlayerScenePlacer : MonoBehaviour
    {
        private void OnEnable() => MessageBus.Subscribe<DungeonSpawnPointMessage>(OnSpawnPoint);
        private void OnDisable() => MessageBus.Unsubscribe<DungeonSpawnPointMessage>(OnSpawnPoint);

        private void OnSpawnPoint(DungeonSpawnPointMessage msg)
        {
            transform.position = new Vector3(msg.Position.x, msg.Position.y, 0);
            // Debug.Log($"[PlayerScenePlacer] 玩家已挪到出生点 {msg.Position}");
        }
    }
}
