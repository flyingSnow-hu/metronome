using System.Collections.Generic;
using UnityEngine;

namespace GraduaMetro
{
    /// <summary>变拍数事件条目：第{measure}小节，拍数改为{beats}。</summary>
    public class BeatsChangeEntry : EventEntryBase
    {
        protected override string Template => "第{measure}小节，拍数改为{beats}";

        protected override Dictionary<string, PlaceholderKind> PlaceholderKinds => new Dictionary<string, PlaceholderKind>
        {
            ["measure"] = PlaceholderKind.Integer,
            ["beats"] = PlaceholderKind.Integer
        };

        protected override void WriteToEvent()
        {
            if (EventData == null) return;
            int.TryParse(GetInputText("measure").Trim(), out int m);
            int.TryParse(GetInputText("beats").Trim(), out int b);
            EventData.measure = m;
            EventData.beats = b;
        }

        protected override void ReadFromEvent()
        {
            SetInputText("measure", EventData.measure.ToString());
            SetInputText("beats", EventData.beats.ToString());
        }
    }
}
