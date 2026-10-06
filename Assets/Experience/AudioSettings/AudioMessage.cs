public enum SoundType
{
    MainBGM,

    PlayerAttack,
    PlayerHurt,

    Victory,
    GameOver,
    OnClick,
    Click,

    NormalMove,
    NormalHurt,
    NormalDie,

    EliteMove,
    EliteHurt,
    EliteDie,

    BossRoar,
    BossHurt,
    BossDie,

    UsingSword,
    UsingPotion
};

namespace Game.Core
{
    public class PlaySoundMessage : GameMessage
    {
        public SoundType Type; 
    }
}

