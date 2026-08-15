using System;
using System.Collections.Generic;

namespace GraduaMetro
{
    /// <summary>一首曲子：默认属性 + 事件列表。</summary>
    [Serializable]
    public class Song
    {
        public string name = "";
        public int defaultBpm = Constants.DefaultBpm;
        public int defaultBeats = Constants.DefaultBeats;
        public int measureCount = Constants.DefaultMeasureCount;
        public AccentMode defaultAccent = AccentMode.StrongWeak;
        public int defaultSubdivision = Constants.DefaultSubdivision;
        public List<SongEvent> events = new List<SongEvent>();
    }
}
