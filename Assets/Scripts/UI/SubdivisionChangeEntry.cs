using System.Collections.Generic;
using UnityEngine;

namespace GraduaMetro
{
    /// <summary>变细分事件条目：第{measure}小节，细分改为{subdivision}。</summary>
    public class SubdivisionChangeEntry : EventEntryBase
    {
        protected override string Template => "第{measure}小节，细分改为{subdivision}";

        protected override Dictionary<string, PlaceholderKind> PlaceholderKinds => new Dictionary<string, PlaceholderKind>
        {
            ["measure"] = PlaceholderKind.Integer,
            ["subdivision"] = PlaceholderKind.Integer
        };

        protected override void WriteToEvent()
        {
            if (EventData == null) return;
            int.TryParse(GetInputText("measure").Trim(), out int m);
            int.TryParse(GetInputText("subdivision").Trim(), out int sub);
            EventData.measure = m;
            EventData.subdivision = sub;
        }

        protected override void ReadFromEvent()
        {
            SetInputText("measure", EventData.measure.ToString());
            SetInputText("subdivision", EventData.subdivision.ToString());
        }
    }
}
