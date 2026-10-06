// Assets/Scripts/Experience/ExperienceConfig.cs
namespace Game.Experience
{
    /// <summary>体验相关配置：倒计时、音效等</summary>
    public static class ExperienceConfig
    {
        /// <summary>每轮倒计时时长（秒）</summary>
        public static float RoundDuration = 5f;

        /// <summary>倒计时UI更新间隔（秒），越小越平滑</summary>
        public static float TimerTickInterval = 0.1f;

        
    }
}