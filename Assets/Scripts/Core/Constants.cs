namespace GraduaMetro
{
    /// <summary>
    /// 程序常量。先以代码形式写死，后续如需改为配置文件再迁移。
    /// </summary>
    public static class Constants
    {
        // 速度（bpm）
        public const int BpmMin = 20;
        public const int BpmMax = 300;
        public const int DefaultBpm = 120;

        // 拍数（每小节拍数）
        public const int BeatsMin = 1;
        public const int BeatsMax = 8;
        public const int DefaultBeats = 4;

        // 细分音
        public const int SubdivisionMin = 1;
        public const int SubdivisionMax = 8;
        public const int DefaultSubdivision = 1;

        // 小节数
        public const int DefaultMeasureCount = 100;

        // 整体播放速度倍率
        public static readonly float[] SpeedMultipliers = { 0.25f, 0.5f, 1f, 2f, 4f };
        public const float DefaultSpeedMultiplier = 1f;

        // 倒计时（单位：秒）
        public static readonly int[] CountdownOptions = { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        public const int DefaultCountdown = 3;

        // 持久化
        public const string SaveFileName = "songs.json";

        // UI 显示名（顺序与枚举一致）
        public static readonly string[] EventTypeNames = { "变速", "曲线变速", "变拍数", "变细分", "变强弱拍" };
        public static readonly string[] AccentModeNames = { "强弱", "全强", "全弱" };
    }
}
