using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GraduaMetro
{
    /// <summary>
    /// 编辑界面：修改曲子级属性 + 增删改事件 + 保存/删除整首曲子。
    /// </summary>
    public class EditSongUI : MonoBehaviour
    {
        [Header("曲子属性")]
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField bpmInput;
        [SerializeField] private TMP_InputField beatsInput;
        [SerializeField] private TMP_InputField measureCountInput;
        [SerializeField] private TMP_Dropdown accentDropdown;
        [SerializeField] private TMP_InputField subdivisionInput;

        [Header("事件列表")]
        [SerializeField] private Transform eventListContent;
        [SerializeField] private GameObject[] eventEntryPrefabs; // 按 EventType 枚举顺序
        [SerializeField] private TMP_Dropdown addEventDropdown;

        [Header("操作")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button deleteSongButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text messageText;

        [Header("基本信息面板")]
        [SerializeField] private Transform baseInfoPanel;
        [SerializeField] private Button toggleBaseInfoButton;

        private Song song;
        private readonly List<EventEntryBase> entries = new List<EventEntryBase>();
        private bool ignoreAddDropdown;

        private void Awake()
        {
            PopulateDropdown(accentDropdown, new List<string>(Constants.AccentModeNames));
            PopulateAddDropdown();

            saveButton.onClick.AddListener(OnSave);
            deleteSongButton.onClick.AddListener(OnDeleteSong);
            backButton.onClick.AddListener(OnBack);
            if (toggleBaseInfoButton != null)
                toggleBaseInfoButton.onClick.AddListener(OnToggleBaseInfo);
            addEventDropdown.onValueChanged.AddListener(OnAddDropdownChanged);
        }

        public void Open(Song s)
        {
            song = s;
            nameInput.text = s.name;
            bpmInput.text = s.defaultBpm.ToString();
            beatsInput.text = s.defaultBeats.ToString();
            measureCountInput.text = s.measureCount.ToString();
            accentDropdown.value = (int)s.defaultAccent;
            subdivisionInput.text = s.defaultSubdivision.ToString();
            messageText.text = "";
            RebuildEventList();
        }

        // ---------- 事件列表 ----------

        private void PopulateAddDropdown()
        {
            var opts = new List<string> { "＋ 添加事件" };
            opts.AddRange(Constants.EventTypeNames);
            addEventDropdown.ClearOptions();
            addEventDropdown.AddOptions(opts);
            addEventDropdown.value = 0;
        }

        private void OnAddDropdownChanged(int value)
        {
            if (ignoreAddDropdown || value <= 0)
                return;

            EventType type = (EventType)(value - 1);
            song.events.Add(CreateDefaultEvent(type));

            ignoreAddDropdown = true;
            addEventDropdown.value = 0;
            ignoreAddDropdown = false;

            RebuildEventList();
        }

        private SongEvent CreateDefaultEvent(EventType type)
        {
            int measure = song.events.Count > 0
                ? Mathf.Min(song.measureCount, song.events[song.events.Count - 1].measure + 1)
                : 1;

            switch (type)
            {
                case EventType.TempoChange:
                    return SongEvent.TempoChange(measure, Constants.DefaultBpm);
                case EventType.CurveTempoChange:
                    return SongEvent.CurveTempoChange(measure, Mathf.Min(song.measureCount, measure + 1), Constants.DefaultBpm, Constants.DefaultBpm);
                case EventType.BeatsChange:
                    return SongEvent.BeatsChange(measure, Constants.DefaultBeats);
                case EventType.SubdivisionChange:
                    return SongEvent.SubdivisionChange(measure, Constants.DefaultSubdivision);
                case EventType.AccentChange:
                    return SongEvent.AccentChange(measure, AccentMode.StrongWeak);
            }
            return SongEvent.TempoChange(measure, Constants.DefaultBpm);
        }

        private void RebuildEventList()
        {
            foreach (Transform child in eventListContent)
                Destroy(child.gameObject);
            entries.Clear();

            song.events.Sort((a, b) =>
                a.measure != b.measure ? a.measure.CompareTo(b.measure) : a.type.CompareTo(b.type));

            foreach (var e in song.events)
            {
                if ((int)e.type < 0 || (int)e.type >= eventEntryPrefabs.Length)
                    continue;

                var go = Instantiate(eventEntryPrefabs[(int)e.type], eventListContent);
                var entry = go.GetComponent<EventEntryBase>();
                if (entry == null)
                {
                    Destroy(go);
                    continue;
                }

                entry.RequestDelete += OnEntryDelete;
                entry.Bind(e);
                entries.Add(entry);
            }
        }

        private void OnEntryDelete(EventEntryBase entry)
        {
            if (entry.EventData == null)
                return;
            song.events.Remove(entry.EventData);
            RebuildEventList();
        }

        // ---------- 保存 / 删除 / 返回 ----------

        private void OnSave()
        {
            if (!ReadSongAttributes(out string error))
            {
                messageText.text = error;
                return;
            }

            foreach (var entry in entries)
                entry.Commit();

            if (!SongValidator.Validate(song, out var errors, out _))
            {
                messageText.text = string.Join("\n", errors);
                return;
            }

            SongRepository.Save(UIManager.Instance.Songs);
            messageText.text = "";
            UIManager.Instance.ShowSongList();
        }

        private bool ReadSongAttributes(out string error)
        {
            error = null;

            string name = nameInput.text.Trim();
            if (string.IsNullOrEmpty(name)) { error = "名字不能为空"; return false; }
            if (!int.TryParse(bpmInput.text.Trim(), out int bpm)) { error = "默认速度需为整数"; return false; }
            if (!int.TryParse(beatsInput.text.Trim(), out int beats)) { error = "默认拍数需为整数"; return false; }
            if (!int.TryParse(measureCountInput.text.Trim(), out int mc)) { error = "小节数需为整数"; return false; }
            if (!int.TryParse(subdivisionInput.text.Trim(), out int sub)) { error = "默认细分音需为整数"; return false; }

            song.name = name;
            song.defaultBpm = bpm;
            song.defaultBeats = beats;
            song.measureCount = mc;
            song.defaultSubdivision = sub;
            song.defaultAccent = (AccentMode)accentDropdown.value;
            return true;
        }

        private void OnDeleteSong()
        {
            UIManager.Instance.Songs.Remove(song);
            SongRepository.Save(UIManager.Instance.Songs);
            UIManager.Instance.ShowSongList();
        }

        private void OnBack()
        {
            UIManager.Instance.ShowSongList();
        }

        private void OnToggleBaseInfo()
        {
            if (baseInfoPanel != null)
                baseInfoPanel.gameObject.SetActive(!baseInfoPanel.gameObject.activeSelf);
        }

        private static void PopulateDropdown(TMP_Dropdown dd, List<string> options)
        {
            dd.ClearOptions();
            dd.AddOptions(options);
        }
    }
}
