// Assets/Scripts/UI/GameOverPanel.cs
using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.UI
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private Button restartButton; // 重启游戏的按钮

        private void Awake()
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnRestartClicked()
        {
            //GameManager.Instance.StartGame(); // 重启游戏
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
