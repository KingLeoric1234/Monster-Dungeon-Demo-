namespace HealthBar
{
    /// <summary>
    /// 血条 UI 芯片配置：所有数值集中在这里，脚本只读不写死。
    /// 物体引用（hpGroup/hpFill/hpText）仍在 Inspector 拖，数值全部走这里。
    /// </summary>
    public static class HealthBarConfig
    {
        // ═══════════ 显示 ═══════════

        /// <summary>血条最大宽度（满血时的宽度，按血量比例缩）</summary>
        public static float HpBarMaxWidth = 200f;

        /// <summary>进入地牢后血条延迟显示时间（秒）</summary>
        public static float DungeonHpShowDelay = 2f;

        /// <summary>地牢血条渐显时长（秒）</summary>
        public static float DungeonHpFadeDuration = 0.5f;

        /// <summary>地牢场景识别名（场景名包含任一即算地牢）</summary>
        public static string[] DungeonSceneNames = { "World2", "LargeWorld", "BossWorld" };

        /// <summary>渐显指数系数（指数越大，渐显越早收尾）</summary>
        public static float FadeInEasePower = 3f;

        // ═══════════ 动画 ═══════════

        /// <summary>淡入淡出时长（秒）</summary>
        public static float FadeDuration = 0.2f;

        /// <summary>滑动距离（像素）</summary>
        public static float SlideDistance = 50f;

        /// <summary>隐藏前延迟（秒，比状态面板慢半拍）</summary>
        public static float HideDelay = 0.1f;

        /// <summary>easeOutExpo 指数系数（动画缓动）</summary>
        public static float EaseOutExpoK = 10f;
    }
}
