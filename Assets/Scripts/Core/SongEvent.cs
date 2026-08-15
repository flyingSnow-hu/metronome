using System;

namespace GraduaMetro
{
    /// <summary>
    /// 节拍器事件。按事件类型使用对应的参数字段，其余字段忽略。
    /// </summary>
    [Serializable]
    public class SongEvent
    {
        public int measure;
        public EventType type;

        // 变速
        public int targetBpm;

        // 曲线变速
        public int endMeasure;
        public int startBpm;
        public int endBpm;

        // 变拍数
        public int beats;

        // 变细分
        public int subdivision;

        // 变强弱拍
        public AccentMode accentMode;

        public static SongEvent TempoChange(int measure, int targetBpm) =>
            new SongEvent { measure = measure, type = EventType.TempoChange, targetBpm = targetBpm };

        public static SongEvent CurveTempoChange(int measure, int endMeasure, int startBpm, int endBpm) =>
            new SongEvent { measure = measure, type = EventType.CurveTempoChange, endMeasure = endMeasure, startBpm = startBpm, endBpm = endBpm };

        public static SongEvent BeatsChange(int measure, int beats) =>
            new SongEvent { measure = measure, type = EventType.BeatsChange, beats = beats };

        public static SongEvent SubdivisionChange(int measure, int subdivision) =>
            new SongEvent { measure = measure, type = EventType.SubdivisionChange, subdivision = subdivision };

        public static SongEvent AccentChange(int measure, AccentMode mode) =>
            new SongEvent { measure = measure, type = EventType.AccentChange, accentMode = mode };

        public string TypeName => Constants.EventTypeNames[(int)type];

        /// <summary>事件的简短文字描述，用于列表展示。</summary>
        public string Summary
        {
            get
            {
                switch (type)
                {
                    case EventType.TempoChange: return $"第 {measure} 小节 变速 {targetBpm}";
                    case EventType.CurveTempoChange: return $"第 {measure} 小节 曲线变速 → 第 {endMeasure} 小节 {startBpm}→{endBpm}";
                    case EventType.BeatsChange: return $"第 {measure} 小节 变拍数 {beats}";
                    case EventType.SubdivisionChange: return $"第 {measure} 小节 变细分 {subdivision}";
                    case EventType.AccentChange: return $"第 {measure} 小节 变强弱拍 {Constants.AccentModeNames[(int)accentMode]}";
                }
                return "";
            }
        }

        /// <summary>事件参数合法性检查（不含小节范围，小节范围由 SongValidator 检查）。</summary>
        public string GetValidationError()
        {
            switch (type)
            {
                case EventType.TempoChange:
                    if (targetBpm < Constants.BpmMin || targetBpm > Constants.BpmMax)
                        return $"速度需在 {Constants.BpmMin}~{Constants.BpmMax}";
                    break;
                case EventType.CurveTempoChange:
                    if (endMeasure <= measure)
                        return "曲线变速的结束小节必须大于开始小节";
                    if (startBpm < Constants.BpmMin || startBpm > Constants.BpmMax)
                        return $"起始速度需在 {Constants.BpmMin}~{Constants.BpmMax}";
                    if (endBpm < Constants.BpmMin || endBpm > Constants.BpmMax)
                        return $"目标速度需在 {Constants.BpmMin}~{Constants.BpmMax}";
                    break;
                case EventType.BeatsChange:
                    if (beats < Constants.BeatsMin || beats > Constants.BeatsMax)
                        return $"拍数需在 {Constants.BeatsMin}~{Constants.BeatsMax}";
                    break;
                case EventType.SubdivisionChange:
                    if (subdivision < Constants.SubdivisionMin || subdivision > Constants.SubdivisionMax)
                        return $"细分音需在 {Constants.SubdivisionMin}~{Constants.SubdivisionMax}";
                    break;
                case EventType.AccentChange:
                    break;
            }
            return null;
        }
    }
}
