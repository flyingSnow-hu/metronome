namespace GraduaMetro
{
    /// <summary>
    /// 事件类型。枚举顺序同时决定同一小节内多个事件的排序（见设计文档）。
    /// </summary>
    public enum EventType
    {
        TempoChange = 0,       // 变速
        CurveTempoChange = 1,  // 曲线变速
        BeatsChange = 2,       // 变拍数
        SubdivisionChange = 3, // 变细分
        AccentChange = 4       // 变强弱拍
    }

    /// <summary>
    /// 强弱拍模式。
    /// </summary>
    public enum AccentMode
    {
        StrongWeak = 0, // 强弱：第 1 拍强，其余弱
        AllStrong = 1,  // 全强
        AllWeak = 2     // 全弱
    }
}
