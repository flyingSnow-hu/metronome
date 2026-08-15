using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraduaMetro
{
    public enum PlaybackState
    {
        Idle,
        Countdown,
        Playing,
        Paused,
        Finished
    }

    /// <summary>一次节拍（或细分音）的播放信息。</summary>
    public struct BeatInfo
    {
        public int measure;             // 1-based
        public int beatIndex;           // 0-based，小节内第几拍
        public int beatCount;           // 该小节拍数
        public int subdivisionIndex;    // 0-based，拍内第几个细分音
        public int subdivisionCount;    // 该拍细分音数
        public bool isStrong;
        public int bpm;
        public AccentMode accent;
    }

    /// <summary>
    /// 节拍器播放引擎：按解析后的小节属性依次发出强/弱音。
    /// 通过 SerializeField 引用强/弱两个 AudioClip 和一个 AudioSource。
    /// </summary>
    public class MetronomePlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip strongClip;
        [SerializeField] private AudioClip weakClip;
        [SerializeField] private float strongOffset;        // 强音音频偏移（秒），提前触发对齐拍点
        [SerializeField] private float weakOffset;          // 弱音音频偏移（秒），提前触发对齐拍点
        [SerializeField] private AudioClip countdownClip;   // 倒计时音频（从 10 数到 0）
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float countStartTime;       // 音频里数“10”的时间点（秒）
        [SerializeField] private float countZeroTime;        // 音频里数“0”的时间点（秒，倒计时在此停止，不播“0”）

        public event Action<BeatInfo> OnBeat;
        public event Action OnCountdownBeat;
        public event Action OnFinished;
        public event Action<PlaybackState> OnStateChanged;

        public PlaybackState State { get; private set; } = PlaybackState.Idle;

        private struct Click
        {
            public float time;
            public BeatInfo info;
        }

        private Song song;
        private float speedMultiplier = 1f;
        private int countdownBeats;
        private List<MeasureState> states;
        private readonly List<Click> clicks = new List<Click>();
        private int nextClickIndex;
        private Coroutine coroutine;

        public void Prepare(Song s, float multiplier, int countdown)
        {
            song = s;
            speedMultiplier = multiplier;
            countdownBeats = countdown;
            SongSimulator.Resolve(song, out states, out _);
            BuildClicks();
            nextClickIndex = 0;
            SetState(PlaybackState.Idle);
        }

        /// <summary>开始播放（含倒计时）。</summary>
        public void Play()
        {
            StopRunning();
            nextClickIndex = 0;
            SetState(PlaybackState.Countdown);
            coroutine = StartCoroutine(Run());
        }

        /// <summary>从头重播（跳过倒计时）。</summary>
        public void Replay()
        {
            StopRunning();
            nextClickIndex = 0;
            coroutine = StartCoroutine(RunClicks(Time.realtimeSinceStartup));
        }

        public void Pause()
        {
            if (State != PlaybackState.Playing)
                return;
            StopRunning();
            SetState(PlaybackState.Paused);
        }

        public void Resume()
        {
            if (State != PlaybackState.Paused)
                return;
            if (nextClickIndex >= clicks.Count)
                nextClickIndex = 0;
            // 保证暂停点的下一拍立即发声，而不是按原始时间表偏移。
            float baseTime = Time.realtimeSinceStartup - clicks[nextClickIndex].time;
            coroutine = StartCoroutine(RunClicks(baseTime));
        }

        public void Stop()
        {
            StopRunning();
            SetState(PlaybackState.Idle);
        }

        private void StopRunning()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        private void BuildClicks()
        {
            clicks.Clear();
            float t = 0f;
            for (int mi = 0; mi < states.Count; mi++)
            {
                var s = states[mi];
                float tempo = Mathf.Max(1f, s.bpm * speedMultiplier);
                float beatDur = 60f / tempo;
                float subDur = beatDur / Mathf.Max(1, s.subdivision);

                for (int b = 0; b < s.beats; b++)
                {
                    for (int d = 0; d < s.subdivision; d++)
                    {
                        bool strong = s.accent == AccentMode.AllStrong
                                      || (s.accent == AccentMode.StrongWeak && b == 0);

                        clicks.Add(new Click
                        {
                            time = t,
                            info = new BeatInfo
                            {
                                measure = mi + 1,
                                beatIndex = b,
                                beatCount = s.beats,
                                subdivisionIndex = d,
                                subdivisionCount = s.subdivision,
                                isStrong = strong,
                                bpm = s.bpm,
                                accent = s.accent
                            }
                        });
                        t += subDur;
                    }
                }
            }
        }

        private IEnumerator Run()
        {
            yield return RunCountdown();
            yield return RunClicks(Time.realtimeSinceStartup);
        }

        private IEnumerator RunCountdown()
        {
            if (countdownBeats <= 0 || countdownClip == null)
                yield break;

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                yield break;

            // 10 → 0 共 10 个间隔，每个数占 1 拍
            float interval = Mathf.Max(0f, countZeroTime - countStartTime) / 10f;
            if (interval <= 0f)
                yield break;

            int beats = Mathf.Clamp(countdownBeats, 1, 10);
            float startTime = Mathf.Max(0f, countZeroTime - beats * interval);
            float duration = beats * interval;

            double startDsp = AudioSettings.dspTime + 0.1;
            audioSource.clip = countdownClip;
            audioSource.time = startTime;
            audioSource.PlayScheduled(startDsp);
            audioSource.SetScheduledEndTime(startDsp + duration); // 到“0”就停，不播“0”

            // 与 startDsp 对齐的实时起点，用于每次数数时回调 OnCountdownBeat
            float startRealtime = Time.realtimeSinceStartup + 0.1f;
            for (int i = 1; i <= beats; i++)
            {
                yield return WaitUntil(startRealtime + i * interval);
                OnCountdownBeat?.Invoke();
            }
        }

        private IEnumerator RunClicks(float baseTime)
        {
            SetState(PlaybackState.Playing);
            for (int i = nextClickIndex; i < clicks.Count; i++)
            {
                nextClickIndex = i;
                float offset = clicks[i].info.isStrong ? strongOffset : weakOffset;
                yield return WaitUntil(baseTime + clicks[i].time - offset);
                if (State != PlaybackState.Playing)
                    yield break;
                PlayClick(clicks[i].info.isStrong);
                OnBeat?.Invoke(clicks[i].info);
                nextClickIndex = i + 1;
            }
            nextClickIndex = clicks.Count;
            SetState(PlaybackState.Finished);
            OnFinished?.Invoke();
        }

        private IEnumerator WaitUntil(float target)
        {
            float wait = target - Time.realtimeSinceStartup;
            if (wait > 0.02f)
                yield return new WaitForSecondsRealtime(wait - 0.01f);
            while (Time.realtimeSinceStartup < target)
                yield return null;
        }

        private void PlayClick(bool strong)
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                    return;
            }

            AudioClip clip = strong ? strongClip : weakClip;
            if (clip == null)
                clip = strong ? weakClip : strongClip;
            if (clip == null)
                return;

            audioSource.PlayOneShot(clip);
        }

        private void SetState(PlaybackState s)
        {
            State = s;
            OnStateChanged?.Invoke(s);
        }
    }
}
