using UnityEngine;

namespace Game.World
{
    /// <summary>可交互物块：声明可交互（任何物块挂上即可）</summary>
    public class Interactable : MonoBehaviour
    {
        [Header("交互设置")]
        [SerializeField] private string promptText = "Click To Interact";  // 提示文本
        [SerializeField] private float interactRange = 2f;   // 交互距离

        public string PromptText => promptText;
        public float InteractRange => interactRange;
    }
}