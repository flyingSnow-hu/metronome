using System.Collections.Generic;

namespace GraduaMetro
{
    /// <summary>曲子与事件的合法性校验、冲突检查。</summary>
    public static class SongValidator
    {
        /// <summary>
        /// 校验整首曲子。返回是否合法；errors 汇总所有错误信息，conflicts 汇总冲突。
        /// </summary>
        public static bool Validate(Song song, out List<string> errors, out List<ConflictInfo> conflicts)
        {
            errors = new List<string>();
            conflicts = new List<ConflictInfo>();

            if (song == null)
            {
                errors.Add("曲子为空");
                return false;
            }

            if (string.IsNullOrWhiteSpace(song.name))
                errors.Add("名字不能为空");
            if (song.defaultBpm < Constants.BpmMin || song.defaultBpm > Constants.BpmMax)
                errors.Add($"默认速度需在 {Constants.BpmMin}~{Constants.BpmMax}");
            if (song.defaultBeats < Constants.BeatsMin || song.defaultBeats > Constants.BeatsMax)
                errors.Add($"默认拍数需在 {Constants.BeatsMin}~{Constants.BeatsMax}");
            if (song.measureCount < 1)
                errors.Add("小节数必须大于等于 1");
            if (song.defaultSubdivision < Constants.SubdivisionMin || song.defaultSubdivision > Constants.SubdivisionMax)
                errors.Add($"默认细分音需在 {Constants.SubdivisionMin}~{Constants.SubdivisionMax}");

            var events = song.events ?? new List<SongEvent>();
            for (int i = 0; i < events.Count; i++)
            {
                var e = events[i];
                if (e == null)
                {
                    errors.Add($"第 {i + 1} 个事件为空");
                    continue;
                }

                if (e.measure < 1 || e.measure > song.measureCount)
                    errors.Add($"第 {i + 1} 个事件：小节需在 1~{song.measureCount}");
                if (e.type == EventType.CurveTempoChange &&
                    (e.endMeasure < 1 || e.endMeasure > song.measureCount))
                    errors.Add($"第 {i + 1} 个事件：结束小节需在 1~{song.measureCount}");

                string evErr = e.GetValidationError();
                if (!string.IsNullOrEmpty(evErr))
                    errors.Add($"第 {i + 1} 个事件：{evErr}");
            }

            SongSimulator.Resolve(song, out _, out conflicts);
            if (conflicts.Count > 0)
            {
                foreach (var c in conflicts)
                    errors.Add(c.Message);
            }

            return errors.Count == 0;
        }
    }
}
