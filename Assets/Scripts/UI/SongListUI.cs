using UnityEngine;
using UnityEngine.UI;

namespace GraduaMetro
{
    /// <summary>曲子列表界面：显示所有曲子，右上角新增按钮。</summary>
    public class SongListUI : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private Button addButton;

        private void Awake()
        {
            if (addButton != null)
                addButton.onClick.AddListener(OnAdd);
        }

        private void OnAdd()
        {
            var song = new Song { name = "新曲子" };
            UIManager.Instance.Songs.Add(song);
            SongRepository.Save(UIManager.Instance.Songs);
            UIManager.Instance.ShowEditor(song);
        }

        public void Refresh()
        {
            foreach (Transform child in content)
                Destroy(child.gameObject);

            foreach (var s in UIManager.Instance.Songs)
            {
                var go = Instantiate(itemPrefab, content);
                var item = go.GetComponent<SongListItemUI>();
                item.Setup(s, OnPlay, OnEdit);
            }
        }

        private void OnPlay(Song s) => UIManager.Instance.ShowPlayPrep(s);
        private void OnEdit(Song s) => UIManager.Instance.ShowEditor(s);
    }
}
