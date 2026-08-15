using System.Collections.Generic;
using UnityEngine;

namespace GraduaMetro
{
    /// <summary>曲线变速事件条目：第{measure}小节起，曲线变速到第{endMeasure}小节，速度{startBpm}→{endBpm}。</summary>
    public class CurveTempoChangeEntry : EventEntryBase
    {
        protected override string Template => "第{measure}小节起，曲线变速到第{endMeasure}小节，速度{startBpm}→{endBpm}";

        protected override Dictionary<string, PlaceholderKind> PlaceholderKinds => new Dictionary<string, PlaceholderKind>
        {
            ["measure"] = PlaceholderKind.Integer,
            ["endMeasure"] = PlaceholderKind.Integer,
            ["startBpm"] = PlaceholderKind.Integer,
            ["endBpm"] = PlaceholderKind.Integer
        };

        protected override void WriteToEvent()
        {
            if (EventData == null) return;
            int.TryParse(GetInputText("measure").Trim(), out int m);
            int.TryParse(GetInputText("endMeasure").Trim(), out int em);
            int.TryParse(GetInputText("startBpm").Trim(), out int sb);
            int.TryParse(GetInputText("endBpm").Trim(), out int eb);
            EventData.measure = m;
            EventData.endMeasure = em;
            EventData.startBpm = sb;
            EventData.endBpm = eb;
        }

        protected override void ReadFromEvent()
        {
            SetInputText("measure", EventData.measure.ToString());
            SetInputText("endMeasure", EventData.endMeasure.ToString());
            SetInputText("startBpm", EventData.startBpm.ToString());
            SetInputText("endBpm", EventData.endBpm.ToString());
        }
    }
}
