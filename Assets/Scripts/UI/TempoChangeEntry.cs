using System.Collections.Generic;
using UnityEngine;

namespace GraduaMetro
{
    /// <summary>变速事件条目：第{measure}小节，变速到{speed}。</summary>
    public class TempoChangeEntry : EventEntryBase
    {
        protected override string Template => "第{measure}小节，变速到{speed}";

        protected override Dictionary<string, PlaceholderKind> PlaceholderKinds => new Dictionary<string, PlaceholderKind>
        {
            ["measure"] = PlaceholderKind.Integer,
            ["speed"] = PlaceholderKind.Integer
        };

        protected override void WriteToEvent()
        {
            if (EventData == null) return;
            int.TryParse(GetInputText("measure").Trim(), out int m);
            int.TryParse(GetInputText("speed").Trim(), out int s);
            EventData.measure = m;
            EventData.targetBpm = s;
        }

        protected override void ReadFromEvent()
        {
            SetInputText("measure", EventData.measure.ToString());
            SetInputText("speed", EventData.targetBpm.ToString());
        }
    }
}
