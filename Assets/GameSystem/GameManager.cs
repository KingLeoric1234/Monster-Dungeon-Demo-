using UnityEngine;
using Game.World;

namespace Game.Core
{
    /// <summary>系统刻度：全局只有两档（主世界 / 地牢第 N 层）</summary>
    public enum SystemState
    {
        MainWorld,   // 主世界（自由活动）
        Dungeon      // 地牢模式（CurrentLayer 记录第几层）
    }

    /// <summary>
    /// 系统时针（心脏）：只做三件事——
    /// 1. 持有系统刻度（世界现在处于哪个模式）
    /// 2. 听其它模块的业务事实，触发切换
    /// 3. 切换后通过总线播报 SystemStateChangedMessage
    /// 不做任何具体工作，没有 Update。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public SystemState CurrentState { get; private set; }
        public int CurrentLayer { get; private set; }      // 层数是数据，不是状态

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            CurrentState = SystemState.MainWorld;          // 进入世界默认主世界
        }

        private void OnEnable()
        {
            MessageBus.Subscribe<DungeonGeneratedMessage>(OnDungeonGenerated);
            // 【待盘点】EnterDungeonRequest / LoadWorldRequest 现状，后续接入
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<DungeonGeneratedMessage>(OnDungeonGenerated);
        }

        private void OnDungeonGenerated(DungeonGeneratedMessage msg)
        {
            ChangeState(SystemState.Dungeon, 1);           // 地牢生成完 → 地牢模式（层数先用 1，盘点后修）
        }

        /// <summary>切换刻度并播报；刻度没变就不播报</summary>
        public void ChangeState(SystemState next, int layer = 0)
        {
            if (next == CurrentState && layer == CurrentLayer) return;
            var old = CurrentState;
            CurrentState = next;
            CurrentLayer = layer;
            MessageBus.Publish(new SystemStateChangedMessage { OldState = old, NewState = next, Layer = layer });
        }
    }
}
