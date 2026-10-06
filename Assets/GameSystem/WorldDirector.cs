using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;
using Game.UI;
using Game.World.Generate;

namespace Game.World
{
    public class WorldDirector : MonoBehaviour
    {
        [Header("场景引用（在Inspector里拖入）")]
        [SerializeField] private UnityEngine.Object worldScene;        // 主世界
        [SerializeField] private UnityEngine.Object[] dungeonScenes;   // 地牢1,2,3层（按顺序）

        private int pendingLayer = 0; // 待生成的层号（场景加载后用）

        private void OnEnable()
        {
            MessageBus.Subscribe<EnterDungeonRequestMessage>(OnEnterRequest);
            MessageBus.Subscribe<LoadWorldRequestMessage>(OnLoadWorldRequest);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<EnterDungeonRequestMessage>(OnEnterRequest);
            MessageBus.Unsubscribe<LoadWorldRequestMessage>(OnLoadWorldRequest);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnEnterRequest(EnterDungeonRequestMessage msg)
        {
            LoadDungeon(msg.Layer);
        }

        private void OnLoadWorldRequest(LoadWorldRequestMessage msg)
        {
            LoadWorld();
        }

        /// <summary>场景加载完成后触发地牢生成</summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (pendingLayer > 0)
            {
                DungeonGenerator gen = FindObjectOfType<DungeonGenerator>();
                if (gen != null)
                {
                    gen.Generate(pendingLayer);
                }
                else
                {
                    Debug.LogError("[WorldDirector] 场景里找不到 DungeonGenerator！");
                }
                pendingLayer = 0;
            }
        }

        /// <summary>回到主世界</summary>
        public void LoadWorld()
        {
            pendingLayer = 0; // 回主世界，不生成地牢

            // 回到主世界：隐藏地牢HUD（HP/Score）
            SetHUDInDungeon(false);

            // ──CameraPan已删除，暂直接LoadScene，待重写黑屏过渡 ──
            SceneManager.LoadScene(worldScene.name);
        }

        /// <summary>加载地牢某一层（从Inspector引用的场景取）</summary>
        public void LoadDungeon(int layer)
        {
            pendingLayer = layer; // 记录层号，场景加载后触发生成

            int index = Mathf.Clamp(layer - 1, 0, dungeonScenes.Length - 1);
            string sceneName = dungeonScenes[index].name;

            // 进入地牢：显示地牢HUD（HP/Score）
            SetHUDInDungeon(true);

            // ──CameraPan已删除，暂直接LoadScene，待重写黑屏过渡 ──
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>找到当前场景的HUDPanel并设置地牢状态</summary>
        private void SetHUDInDungeon(bool inDungeon)
        {
            HUDPanel hud = FindObjectOfType<HUDPanel>();
            if (hud != null)
            {
                hud.SetInDungeon(inDungeon);
            }
            // 如果找不到，说明当前场景没有HUDPanel，不报错（可能还没加载完）
        }
    }
}
