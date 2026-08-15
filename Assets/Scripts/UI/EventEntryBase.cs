using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GraduaMetro
{
    /// <summary>
    /// 事件条目基类。子类只声明一句话模板（占位符用 {name}），
    /// 基类在运行时解析模板，自动生成静态文字段与输入控件，无需手动挂输入框。
    /// </summary>
    public abstract class EventEntryBase : MonoBehaviour
    {
        [SerializeField] protected Transform content;        // 横向排列容器
        [SerializeField] protected GameObject textPrefab;     // 静态文字段（含 TMP_Text）
        [SerializeField] protected GameObject inputPrefab;    // 整数输入框（含 TMP_InputField）
        [SerializeField] protected GameObject dropdownPrefab; // 枚举下拉框（含 TMP_Dropdown）
        [SerializeField] protected Button deleteButton;

        protected enum PlaceholderKind
        {
            Integer,
            Dropdown
        }

        private struct Segment
        {
            public bool isPlaceholder;
            public string name;
            public string text;
        }

        public SongEvent EventData { get; private set; }

        /// <summary>请求删除自身。</summary>
        public event Action<EventEntryBase> RequestDelete;

        /// <summary>一句话模板，占位符用 {名字} 表示。</summary>
        protected abstract string Template { get; }

        /// <summary>占位符名 → 控件类型。</summary>
        protected abstract Dictionary<string, PlaceholderKind> PlaceholderKinds { get; }

        /// <summary>下拉型占位符的选项（默认无）。</summary>
        protected virtual List<string> GetDropdownOptions(string placeholder) => null;

        private readonly Dictionary<string, TMP_InputField> inputs = new Dictionary<string, TMP_InputField>();
        private readonly Dictionary<string, TMP_Dropdown> dropdowns = new Dictionary<string, TMP_Dropdown>();

        protected virtual void Awake()
        {
            BuildUI();
            if (deleteButton != null)
                deleteButton.onClick.AddListener(() => RequestDelete?.Invoke(this));
            StartCoroutine(CoResizeSelf());
        }

        // 延迟一帧，等 content 布局稳定后，只把本体高度设为 content 的高度（宽度保持不变）
        private IEnumerator CoResizeSelf()
        {
            yield return null;

            var contentRect = content as RectTransform;
            var rt = transform as RectTransform;
            if (contentRect == null || rt == null)
                yield break;

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            var size = rt.sizeDelta;
            rt.sizeDelta = new Vector2(size.x, contentRect.rect.height);
        }

        public void Bind(SongEvent e)
        {
            EventData = e;
            ReadFromEvent();
        }

        public void Commit()
        {
            if (EventData == null)
                return;
            WriteToEvent();
        }

        // ---- 供子类读写控件的辅助方法 ----

        protected string GetInputText(string name) =>
            inputs.TryGetValue(name, out var i) ? i.text : "";

        protected void SetInputText(string name, string value)
        {
            if (inputs.TryGetValue(name, out var i))
                i.text = value;
        }

        protected int GetDropdownValue(string name) =>
            dropdowns.TryGetValue(name, out var d) ? d.value : 0;

        protected void SetDropdownValue(string name, int value)
        {
            if (dropdowns.TryGetValue(name, out var d))
                d.value = value;
        }

        protected abstract void WriteToEvent();
        protected abstract void ReadFromEvent();

        // ---- 模板解析与 UI 生成 ----

        private void BuildUI()
        {
            if (content == null)
                return;

            foreach (var seg in ParseTemplate(Template))
            {
                if (!seg.isPlaceholder)
                {
                    if (string.IsNullOrEmpty(seg.text) || textPrefab == null)
                        continue;
                    // 为了在流式布局里能按字符换行，每个字符单独一个 TMP_Text
                    foreach (char c in seg.text)
                    {
                        var txt = Instantiate(textPrefab, content).GetComponentInChildren<TMP_Text>(true);
                        if (txt != null)
                            txt.text = c.ToString();
                    }
                }
                else
                {
                    if (GetKind(seg.name) == PlaceholderKind.Dropdown)
                    {
                        if (dropdownPrefab == null)
                            continue;
                        var dd = Instantiate(dropdownPrefab, content).GetComponentInChildren<TMP_Dropdown>(true);
                        if (dd == null)
                            continue;
                        var opts = GetDropdownOptions(seg.name) ?? new List<string>();
                        dd.ClearOptions();
                        dd.AddOptions(opts);
                        dropdowns[seg.name] = dd;
                    }
                    else
                    {
                        if (inputPrefab == null)
                            continue;
                        var inp = Instantiate(inputPrefab, content).GetComponentInChildren<TMP_InputField>(true);
                        if (inp != null)
                            inputs[seg.name] = inp;
                    }
                }
            }
        }

        private PlaceholderKind GetKind(string name)
        {
            var kinds = PlaceholderKinds;
            if (kinds != null && kinds.TryGetValue(name, out var k))
                return k;
            return PlaceholderKind.Integer;
        }

        private static List<Segment> ParseTemplate(string template)
        {
            var list = new List<Segment>();
            if (string.IsNullOrEmpty(template))
                return list;

            int i = 0;
            while (i < template.Length)
            {
                int open = template.IndexOf('{', i);
                if (open < 0)
                {
                    list.Add(new Segment { isPlaceholder = false, text = template.Substring(i) });
                    break;
                }
                if (open > i)
                    list.Add(new Segment { isPlaceholder = false, text = template.Substring(i, open - i) });

                int close = template.IndexOf('}', open + 1);
                if (close < 0)
                {
                    list.Add(new Segment { isPlaceholder = false, text = template.Substring(open) });
                    break;
                }

                list.Add(new Segment { isPlaceholder = true, name = template.Substring(open + 1, close - open - 1) });
                i = close + 1;
            }
            return list;
        }
    }
}
