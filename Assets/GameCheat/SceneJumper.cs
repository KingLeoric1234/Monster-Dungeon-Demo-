using UnityEngine;
using Game.Core;
using Game.World;

/// <summary>测试用：输入BOSSRUSH解锁跳关功能</summary>
public class SceneJumper : MonoBehaviour
{
    private string cheatCode = "BOSSRUSH";
    private string input = "";
    private bool unlocked;

    void Update()
    {
        // 输入彩蛋代码
        if (!unlocked)
        {
            input += Input.inputString.ToUpper();
            if (input.Length > cheatCode.Length) input = input.Substring(input.Length - cheatCode.Length);
            if (input == cheatCode)
            {
                unlocked = true;
                Debug.Log("[Cheat] 跳关功能已解锁！F1-F4");
            }
            return;
        }

        // 全部走消息，不直接加载场景
        if (Input.GetKeyDown(KeyCode.F1))
            MessageBus.Publish(new LoadWorldRequestMessage()); // 回主世界
        if (Input.GetKeyDown(KeyCode.F2))
            MessageBus.Publish(new EnterDungeonRequestMessage { Layer = 1 }); // 地牢1层
        if (Input.GetKeyDown(KeyCode.F3))
            MessageBus.Publish(new EnterDungeonRequestMessage { Layer = 2 }); // 地牢2层
        if (Input.GetKeyDown(KeyCode.F4))
            MessageBus.Publish(new EnterDungeonRequestMessage { Layer = 3 }); // 地牢3层
    }
}
