using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraduaMetro
{
    /// <summary>某一小节解析后的属性快照。</summary>
    public struct MeasureState
    {
        public int bpm;
        public int beats;
        public int subdivision;
        public AccentMode accent;
    }

    /// <summary>冲突信息：某小节的某属性被多个事件同时修改。</summary>
    public class ConflictInfo
    {
        public int measure;
        public string attributeName;
        public List<SongEvent> events = new List<SongEvent>();

        public string Message => $"第 {measure} 小节的{attributeName}被多个事件同时修改";
    }

    public static class SongSimulator
    {
        /// <summary>计算曲线变速事件在某小节的 bpm（线性插值，四舍五入到整数）。</summary>
        public static int CurveBpm(SongEvent curve, int measure)
        {
            if (curve.endMeasure <= curve.measure) return curve.startBpm;
            if (measure <= curve.measure) return curve.startBpm;
            if (measure >= curve.endMeasure) return curve.endBpm;
            float t = (float)(measure - curve.measure) / (curve.endMeasure - curve.measure);
            return Mathf.RoundToInt(Mathf.Lerp(curve.startBpm, curve.endBpm, t));
        }

        /// <summary>
        /// 从第 1 小节起依次模拟事件，得到每个小节各属性的唯一值；
        /// 同时检测同一小节同一属性被多个事件修改的冲突。
        /// </summary>
        public static void Resolve(Song song, out List<MeasureState> states, out List<ConflictInfo> conflicts)
        {
            states = new List<MeasureState>();
            conflicts = new List<ConflictInfo>();

            var events = song.events ?? new List<SongEvent>();

            int bpm = song.defaultBpm;
            int beats = song.defaultBeats;
            int sub = song.defaultSubdivision;
            AccentMode accent = song.defaultAccent;

            for (int m = 1; m <= song.measureCount; m++)
            {
                var bpmSources = new List<SongEvent>();
                var beatsSources = new List<SongEvent>();
                var subSources = new List<SongEvent>();
                var accentSources = new List<SongEvent>();

                var atMeasure = events.Where(e => e != null && e.measure == m).OrderBy(e => e.type).ToList();
                foreach (var e in atMeasure)
                {
                    switch (e.type)
                    {
                        case EventType.TempoChange: bpmSources.Add(e); break;
                        case EventType.BeatsChange: beatsSources.Add(e); break;
                        case EventType.SubdivisionChange: subSources.Add(e); break;
                        case EventType.AccentChange: accentSources.Add(e); break;
                    }
                }
                foreach (var e in events)
                {
                    if (e != null && e.type == EventType.CurveTempoChange && e.measure <= m && m <= e.endMeasure)
                        bpmSources.Add(e);
                }

                if (bpmSources.Count > 1)
                {
                    conflicts.Add(new ConflictInfo { measure = m, attributeName = "速度", events = bpmSources });
                }
                else if (bpmSources.Count == 1)
                {
                    var src = bpmSources[0];
                    bpm = src.type == EventType.CurveTempoChange ? CurveBpm(src, m) : src.targetBpm;
                }

                if (beatsSources.Count > 1)
                    conflicts.Add(new ConflictInfo { measure = m, attributeName = "拍数", events = beatsSources });
                else if (beatsSources.Count == 1)
                    beats = beatsSources[0].beats;

                if (subSources.Count > 1)
                    conflicts.Add(new ConflictInfo { measure = m, attributeName = "细分音", events = subSources });
                else if (subSources.Count == 1)
                    sub = subSources[0].subdivision;

                if (accentSources.Count > 1)
                    conflicts.Add(new ConflictInfo { measure = m, attributeName = "强弱拍", events = accentSources });
                else if (accentSources.Count == 1)
                    accent = accentSources[0].accentMode;

                states.Add(new MeasureState { bpm = bpm, beats = beats, subdivision = sub, accent = accent });
            }
        }
    }
}
