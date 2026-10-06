// Assets/Scripts/UI/VictoryPanel.cs
using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.UI
{
    public class VictoryPanel : MonoBehaviour
    {
        [SerializeField] private Button restartButton; // 重新开始游戏的Button

        private void Awake() // 监听Restart有没有被点击
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnRestartClicked() // 如果点击了Restart键
        {
            //GameManager.Instance.StartGame(); // 重启游戏
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}