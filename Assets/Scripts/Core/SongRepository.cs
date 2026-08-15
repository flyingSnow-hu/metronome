using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GraduaMetro
{
    [Serializable]
    public class SongListWrapper
    {
        public List<Song> songs = new List<Song>();
    }

    /// <summary>曲子列表的 JSON 持久化。</summary>
    public static class SongRepository
    {
        private static string FilePath => Path.Combine(Application.persistentDataPath, Constants.SaveFileName);

        public static List<Song> Load()
        {
            var songs = new List<Song>();
            if (!File.Exists(FilePath))
                return songs;

            try
            {
                string json = File.ReadAllText(FilePath);
                if (string.IsNullOrWhiteSpace(json))
                    return songs;

                var wrapper = JsonUtility.FromJson<SongListWrapper>(json);
                if (wrapper == null || wrapper.songs == null)
                    return songs;

                foreach (var s in wrapper.songs)
                {
                    if (s == null)
                        continue;
                    if (s.events == null)
                        s.events = new List<SongEvent>();
                    songs.Add(s);
                }
            }
            catch (Exception)
            {
                // 文件损坏时返回空列表，避免崩溃。
                return new List<Song>();
            }

            return songs;
        }

        public static void Save(List<Song> songs)
        {
            var wrapper = new SongListWrapper { songs = songs ?? new List<Song>() };
            File.WriteAllText(FilePath, JsonUtility.ToJson(wrapper, true));
        }
    }
}
