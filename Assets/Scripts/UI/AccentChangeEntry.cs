using System.Collections.Generic;
using UnityEngine;

namespace GraduaMetro
{
    /// <summary>变强弱拍事件条目：第{measure}小节，强弱拍改为{accent}。</summary>
    public class AccentChangeEntry : EventEntryBase
    {
        protected override string Template => "第{measure}小节，强弱拍改为{accent}";

        protected override Dictionary<string, PlaceholderKind> PlaceholderKinds => new Dictionary<string, PlaceholderKind>
        {
            ["measure"] = PlaceholderKind.Integer,
            ["accent"] = PlaceholderKind.Dropdown
        };

        protected override List<string> GetDropdownOptions(string placeholder) =>
            placeholder == "accent" ? new List<string>(Constants.AccentModeNames) : null;

        protected override void WriteToEvent()
        {
            if (EventData == null) return;
            int.TryParse(GetInputText("measure").Trim(), out int m);
            EventData.measure = m;
            EventData.accentMode = (AccentMode)GetDropdownValue("accent");
        }

        protected override void ReadFromEvent()
        {
            SetInputText("measure", EventData.measure.ToString());
            SetDropdownValue("accent", (int)EventData.accentMode);
        }
    }
}
