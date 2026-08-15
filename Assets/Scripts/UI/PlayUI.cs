using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GraduaMetro
{
    /// <summary>
    /// 播放界面：时间轴 + 事件标记 + 当前速率/拍数/强弱拍/细分 + 暂停/重播/退出。
    /// </summary>
    public class PlayUI : MonoBehaviour
    {
        [SerializeField] private MetronomePlayer player;

        [Header("信息")]
        [SerializeField] private TMP_Text measureText;
        [SerializeField] private TMP_Text bpmText;
        [SerializeField] private TMP_Text beatsText;
        [SerializeField] private TMP_Text accentText;
        [SerializeField] private TMP_Text subdivisionText;

        [Header("时间轴")]
        [SerializeField] private RectTransform timelineContent;
        [SerializeField] private GameObject markerPrefab;
        [SerializeField] private RectTransform playhead;

        [Header("进度条")]
        [SerializeField] private RectTransform songProgress;   // X scale = 当前小节/总小节数
        [SerializeField] private RectTransform beatProgress;   // X scale = 当前拍/当前小节拍数

        [Header("按钮")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private TMP_Text pauseButtonLabel;
        [SerializeField] private Button exitButton;

        private Song song;
        private float speedMultiplier;
        private List<MeasureState> states;
        private float[] startTimes;
        private float totalTime;

        private void Awake()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseToggle);
            if (exitButton != null)
                exitButton.onClick.AddListener(OnExit);

            if (player == null)
                player = GetComponent<MetronomePlayer>() ?? FindObjectOfType<MetronomePlayer>();

            if (player != null)
            {
                player.OnBeat += OnBeat;
                player.OnFinished += OnFinished;
                player.OnStateChanged += OnStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnBeat -= OnBeat;
                player.OnFinished -= OnFinished;
                player.OnStateChanged -= OnStateChanged;
            }
        }

        public void Open(Song s, float multiplier, int countdown)
        {
            song = s;
            speedMultiplier = multiplier;
            SongSimulator.Resolve(song, out states, out _);
            SongTiming.BuildMeasureTimes(states, speedMultiplier, out startTimes, out totalTime);

            BuildTimeline();
            ResetIndicators();
            player.Prepare(song, speedMultiplier, countdown);
            player.Play();
        }

        private void BuildTimeline()
        {
            if (timelineContent == null || markerPrefab == null)
                return;

            foreach (Transform child in timelineContent)
                Destroy(child.gameObject);

            float total = totalTime > 0f ? totalTime : 1f;
            foreach (var e in song.events)
            {
                int idx = Mathf.Clamp(e.measure - 1, 0, startTimes.Length - 1);
                float norm = Mathf.Clamp01(startTimes[idx] / total);

                var go = Instantiate(markerPrefab, timelineContent);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(norm, 0f);
                rt.anchorMax = new Vector2(norm, 1f);
                rt.anchoredPosition = Vector2.zero;
            }
        }

        private void OnBeat(BeatInfo info)
        {
            if (measureText != null) measureText.text = $"{info.measure}/{song.measureCount}";
            if (bpmText != null) bpmText.text = $"{info.bpm} bpm";
            if (beatsText != null) beatsText.text = $"拍数 {info.beatCount}";
            if (accentText != null) accentText.text = $"强弱拍 {Constants.AccentModeNames[(int)info.accent]}";
            if (subdivisionText != null) subdivisionText.text = $"细分 {info.subdivisionCount}";
            UpdatePlayhead(info);
            UpdateProgress(info);
        }

        private void UpdatePlayhead(BeatInfo info)
        {
            if (playhead == null || states == null || startTimes == null || states.Count == 0)
                return;

            int mi = Mathf.Clamp(info.measure - 1, 0, states.Count - 1);
            float tempo = Mathf.Max(1f, states[mi].bpm * speedMultiplier);
            float beatDur = 60f / tempo;
            float subDur = beatDur / Mathf.Max(1, info.subdivisionCount);
            float within = info.beatIndex * beatDur + info.subdivisionIndex * subDur;

            float total = totalTime > 0f ? totalTime : 1f;
            float norm = Mathf.Clamp01((startTimes[mi] + within) / total);

            playhead.anchorMin = new Vector2(norm, playhead.anchorMin.y);
            playhead.anchorMax = new Vector2(norm, playhead.anchorMax.y);
            playhead.anchoredPosition = Vector2.zero;
        }

        private void UpdateProgress(BeatInfo info)
        {
            if (song != null && song.measureCount > 0)
                SetBarX(songProgress, Mathf.Clamp01((float)info.measure / song.measureCount));

            if (info.beatCount > 0)
                SetBarX(beatProgress, Mathf.Clamp01((float)(info.beatIndex + 1) / info.beatCount));
        }

        private void ResetIndicators()
        {
            SetBarX(songProgress, 0f);
            SetBarX(beatProgress, 0f);

            if (playhead != null)
            {
                playhead.anchorMin = new Vector2(0f, playhead.anchorMin.y);
                playhead.anchorMax = new Vector2(0f, playhead.anchorMax.y);
                playhead.anchoredPosition = Vector2.zero;
            }
        }

        private static void SetBarX(RectTransform rt, float x)
        {
            if (rt == null) return;
            var s = rt.localScale;
            rt.localScale = new Vector3(x, s.y, s.z);
        }

        private void OnPauseToggle()
        {
            switch (player.State)
            {
                case PlaybackState.Playing:
                    player.Pause();
                    break;
                case PlaybackState.Paused:
                    player.Resume();
                    break;
                case PlaybackState.Finished:
                    player.Replay();
                    break;
            }
        }

        private void OnFinished()
        {
            if (pauseButtonLabel != null)
                pauseButtonLabel.text = "重播";
            if (pauseButton != null)
                pauseButton.interactable = true;
        }

        private void OnStateChanged(PlaybackState state)
        {
            if (pauseButton == null || pauseButtonLabel == null)
                return;

            switch (state)
            {
                case PlaybackState.Countdown:
                    pauseButtonLabel.text = "准备中";
                    pauseButton.interactable = false;
                    break;
                case PlaybackState.Playing:
                    pauseButtonLabel.text = "暂停";
                    pauseButton.interactable = true;
                    break;
                case PlaybackState.Paused:
                    pauseButtonLabel.text = "继续";
                    pauseButton.interactable = true;
                    break;
                case PlaybackState.Finished:
                    pauseButtonLabel.text = "重播";
                    pauseButton.interactable = true;
                    break;
                case PlaybackState.Idle:
                    pauseButtonLabel.text = "暂停";
                    pauseButton.interactable = true;
                    break;
            }
        }

        private void OnExit()
        {
            player.Stop();
            UIManager.Instance.ShowSongList();
        }
    }
}
