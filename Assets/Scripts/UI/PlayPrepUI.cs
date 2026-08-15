using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GraduaMetro
{
    /// <summary>
    /// 播放预备界面：选择倒计时拍数和整体播放速度倍率。
    /// </summary>
    public class PlayPrepUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text songNameText;
        [SerializeField] private TMP_Dropdown countdownDropdown;
        [SerializeField] private TMP_Dropdown speedMultiplierDropdown;
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;

        private Song song;

        private void Awake()
        {
            PopulateCountdown();
            PopulateSpeed();

            if (startButton != null)
                startButton.onClick.AddListener(OnStart);
            if (backButton != null)
                backButton.onClick.AddListener(OnBack);
        }

        public void Open(Song s)
        {
            song = s;
            if (songNameText != null)
                songNameText.text = s.name;

            countdownDropdown.value = IndexOf(Constants.CountdownOptions, Constants.DefaultCountdown);
            speedMultiplierDropdown.value = IndexOf(Constants.SpeedMultipliers, Constants.DefaultSpeedMultiplier);
        }

        private void PopulateCountdown()
        {
            var opts = new List<string>();
            foreach (var c in Constants.CountdownOptions)
                opts.Add(c == 0 ? "无" : $"{c} 秒");
            countdownDropdown.ClearOptions();
            countdownDropdown.AddOptions(opts);
        }

        private void PopulateSpeed()
        {
            var opts = new List<string>();
            foreach (var m in Constants.SpeedMultipliers)
                opts.Add(m % 1f == 0f ? $"×{m:0}" : $"×{m:0.##}");
            speedMultiplierDropdown.ClearOptions();
            speedMultiplierDropdown.AddOptions(opts);
        }

        private void OnStart()
        {
            int countdown = Constants.CountdownOptions[countdownDropdown.value];
            float multiplier = Constants.SpeedMultipliers[speedMultiplierDropdown.value];
            UIManager.Instance.ShowPlay(song, multiplier, countdown);
        }

        private void OnBack() => UIManager.Instance.ShowSongList();

        private static int IndexOf(float[] arr, float value)
        {
            for (int i = 0; i < arr.Length; i++)
                if (Mathf.Approximately(arr[i], value))
                    return i;
            return 0;
        }

        private static int IndexOf(int[] arr, int value)
        {
            for (int i = 0; i < arr.Length; i++)
                if (arr[i] == value)
                    return i;
            return 0;
        }
    }
}
