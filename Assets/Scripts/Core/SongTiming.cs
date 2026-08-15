using System.Collections.Generic;

namespace GraduaMetro
{
    /// <summary>把「小节」换算成「秒」的时间计算，供播放与时间轴使用。</summary>
    public static class SongTiming
    {
        /// <summary>一个小节的时长（秒）= 拍数 × 每拍时长，速度已乘倍率。</summary>
        public static float MeasureDuration(MeasureState state, float speedMultiplier)
        {
            float tempo = state.bpm * speedMultiplier;
            if (tempo <= 0f) tempo = 1f;
            return state.beats * 60f / tempo;
        }

        /// <summary>计算每小节的起始时间（秒）与总时长（秒）。</summary>
        public static void BuildMeasureTimes(
            List<MeasureState> states,
            float speedMultiplier,
            out float[] startTimes,
            out float totalTime)
        {
            startTimes = new float[states.Count];
            totalTime = 0f;
            for (int i = 0; i < states.Count; i++)
            {
                startTimes[i] = totalTime;
                totalTime += MeasureDuration(states[i], speedMultiplier);
            }
        }
    }
}
