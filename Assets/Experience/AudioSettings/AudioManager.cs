using UnityEngine;
using Game.Core;

namespace Game.Experience
{
    public class AudioManager : MonoBehaviour
    {
        [Header("BGM")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioClip bgmClip;

        [Header("音效")]
        [SerializeField] private AudioSource sfxSource;

        [SerializeField] private AudioClip victorySound;
        [SerializeField] private AudioClip gameOverSound;
        [SerializeField] private AudioClip playerAttackSound;
        [SerializeField] private AudioClip playerHurtSound;
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip onClickSound;
        [SerializeField] private AudioClip normalMoveSound;
        [SerializeField] private AudioClip normalDieSound;
        [SerializeField] private AudioClip normalHurtSound;
        [SerializeField] private AudioClip eliteMoveSound;
        [SerializeField] private AudioClip eliteDieSound;
        [SerializeField] private AudioClip eliteHurtSound;
        [SerializeField] private AudioClip bossRoarSound;
        [SerializeField] private AudioClip bossHurtSound;
        [SerializeField] private AudioClip bossDieSound;
        [SerializeField] private AudioClip usingPotionSound;

        public void PlayBGM()
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            bgmSource.Stop();
        }

        public void PlayVictory() => sfxSource.PlayOneShot(victorySound);
        public void PlayGameOver() => sfxSource.PlayOneShot(gameOverSound);
        public void PlayPlayerAttack() => sfxSource.PlayOneShot(playerAttackSound);
        public void PlayPlayerHurt() => sfxSource.PlayOneShot(playerHurtSound);
        public void PlayClickSound() => sfxSource.PlayOneShot(clickSound);
        public void PlayOnClickSound() => sfxSource.PlayOneShot(onClickSound);
        public void PlayNormalMove() => sfxSource.PlayOneShot(normalMoveSound);
        public void PlayNormalDie() => sfxSource.PlayOneShot(normalDieSound);
        public void PlayNormalHurt() => sfxSource.PlayOneShot(normalHurtSound);
        public void PlayEliteMove() => sfxSource.PlayOneShot(eliteMoveSound);
        public void PlayEliteDie() => sfxSource.PlayOneShot(eliteDieSound);
        public void PlayEliteHurt() => sfxSource.PlayOneShot(eliteHurtSound);
        public void PlayBossRoar() => sfxSource.PlayOneShot(bossRoarSound);
        public void PlayBossHurt() => sfxSource.PlayOneShot(bossHurtSound);
        public void PlayBossDie() => sfxSource.PlayOneShot(bossDieSound);
        public void PlayUsingPotion() => sfxSource.PlayOneShot(usingPotionSound);

        private void OnEnable()
        {
            MessageBus.Subscribe<PlaySoundMessage>(OnPlaySound);
        }

        private void OnDisable()
        {
            MessageBus.Unsubscribe<PlaySoundMessage>(OnPlaySound);
        }

        private void OnPlaySound(PlaySoundMessage msg)
        {
            switch (msg.Type)
            {
                case SoundType.PlayerAttack:
                    PlayPlayerAttack();
                    break;
                case SoundType.PlayerHurt:
                    PlayPlayerHurt();
                    break;
                case SoundType.Victory:
                    PlayVictory();
                    break;
                case SoundType.GameOver:
                    PlayGameOver();
                    break;
                case SoundType.OnClick:
                    PlayOnClickSound();
                    break;
                case SoundType.Click:
                    PlayClickSound();
                    break;
                case SoundType.NormalMove:
                    PlayNormalMove();
                    break;
                case SoundType.NormalDie:
                    PlayNormalDie();
                    break;
                case SoundType.NormalHurt:
                    PlayNormalHurt();
                    break;
                case SoundType.EliteMove:
                    PlayEliteMove();
                    break;
                case SoundType.EliteDie:
                    PlayEliteDie();
                    break;
                case SoundType.EliteHurt:
                    PlayEliteHurt();
                    break;
                case SoundType.BossRoar:
                    PlayBossRoar();
                    break;
                case SoundType.BossHurt:
                    PlayBossHurt();
                    break;
                case SoundType.BossDie:
                    PlayBossDie();
                    break;
                case SoundType.UsingPotion:
                    PlayUsingPotion();
                    break;
            }
        }
    }
}
