// Assets/Scripts/Experience/ExperienceDirector.cs
using UnityEngine;

namespace Game.Experience
{
    /// <summary>体验秘书：协调倒计时和音效</summary>
    public class ExperienceDirector : MonoBehaviour
    {
        [Header("手下引用")]
        [SerializeField] private AudioManager audioManager;

        // === 音效 ===
        public void PlayBGM() => audioManager.PlayBGM();
        public void PlayVictory() => audioManager.PlayVictory();
        public void PlayGameOver() => audioManager.PlayGameOver();
    }
}