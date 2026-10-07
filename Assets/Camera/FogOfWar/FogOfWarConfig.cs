using UnityEngine;

namespace Game.Items
{
    /// <summary>
    /// 战争迷雾配置：所有参数集中在这里，方便后续调整。
    /// 静态类，不继承MonoBehaviour，其它脚本不需要知道FogOfWarDirector的存在。
    /// </summary>
    public static class FogOfWarConfig
    {
        // ===== 视野设置 =====
        public const float ViewRadius = 10f;           // 玩家视野半径（世界单位）
        public const float RTMargin = 12f;              // RT比视野半径多扩的一圈（容错：雾中心偏移时边缘不漏）
        public const float EdgeSmoothness = 0.8f;     // 视野边缘平滑过渡距离
        public const float EdgeBlurWidth = 0.9f;      // 软边过渡带总宽度（世界单位）：分3层渐变

        // ===== 迷雾设置 =====
        public static readonly Color FogColor = new Color(0f, 0f, 0f, 0.8f);  // 迷雾颜色（半透明：景观隐约可见；alpha=1纯黑=景观全遮）
        public const string FogSortingLayerName = "FogOfWar";  // 迷雾的Sorting Layer（需要在Unity里创建）
        public const float FogZPosition = -5f;        // 迷雾Mesh的z轴位置（必须在游戏画面前面，否则被挡住）

        // ===== 射线检测设置 =====
        public const string ObstacleLayerName = "Obstacle";  // 阻挡视线的层名
        public const int RaycastCount = 30;             // 每帧发射的射线数量（越多越精确，越耗性能）
        public const float RaycastDistance = 30f;        // 射线最大距离
        public const float ObstaclePadding = 0.6f;       // 可见区在障碍物方向额外外扩的距离（让障碍物本体不被迷雾盖住；要回档改0）

        // ===== 性能设置 =====
        public const float UpdateInterval = 0f;       // 迷雾重算间隔（秒）：算新形状
        public const float FogFadeDuration = 0.5f;      // 形状缓变时长（秒）：旧形状→新形状的过渡时间
        public const float FogSmoothSpeed = 6f;         // 形状平滑速度：越大越快追上（60帧≈0.13/帧，想更慢就调小）
        public const float EntityScanInterval = 2f;      // 重新扫描场景实体的间隔（秒），敌人生成/销毁不频繁，慢扫即可
    }
}
