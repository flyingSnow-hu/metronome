using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GraduaMetro
{
    /// <summary>曲子列表中的一个条目：名字 + 播放 + 编辑。</summary>
    public class SongListItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button editButton;

        private Song song;
        private Action<Song> onPlay;
        private Action<Song> onEdit;

        public void Setup(Song s, Action<Song> play, Action<Song> edit)
        {
            song = s;
            onPlay = play;
            onEdit = edit;

            nameText.text = s.name;

            playButton.onClick.RemoveAllListeners();
            editButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => onPlay?.Invoke(song));
            editButton.onClick.AddListener(() => onEdit?.Invoke(song));
        }
    }
}
