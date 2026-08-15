using System.Collections.Generic;
using UnityEngine;

namespace GraduaMetro
{
    /// <summary>
    /// 全局 UI 管理器：持有曲子列表并负责四个界面之间的切换。
    /// 在场景中挂一个，四个界面组件通过 SerializeField 引用进来。
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private SongListUI songListUI;
        [SerializeField] private EditSongUI editSongUI;
        [SerializeField] private PlayPrepUI playPrepUI;
        [SerializeField] private PlayUI playUI;

        /// <summary>内存中的曲子列表（磁盘文件的镜像）。</summary>
        public List<Song> Songs { get; private set; } = new List<Song>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Songs = SongRepository.Load();
        }

        private void Start()
        {
            ShowSongList();
        }

        private void ShowOnly(MonoBehaviour active)
        {
            songListUI.gameObject.SetActive(active == songListUI);
            editSongUI.gameObject.SetActive(active == editSongUI);
            playPrepUI.gameObject.SetActive(active == playPrepUI);
            playUI.gameObject.SetActive(active == playUI);
        }

        public void ShowSongList()
        {
            ShowOnly(songListUI);
            songListUI.Refresh();
        }

        public void ShowEditor(Song song)
        {
            ShowOnly(editSongUI);
            editSongUI.Open(song);
        }

        public void ShowPlayPrep(Song song)
        {
            ShowOnly(playPrepUI);
            playPrepUI.Open(song);
        }

        public void ShowPlay(Song song, float speedMultiplier, int countdownBeats)
        {
            ShowOnly(playUI);
            playUI.Open(song, speedMultiplier, countdownBeats);
        }
    }
}
