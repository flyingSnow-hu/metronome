using UnityEngine;
using UnityEngine.UI;

/// <summary>自动换行的流式布局。</summary>
[DisallowMultipleComponent]
public class FlowLayoutGroup : LayoutGroup
{
    [SerializeField] private float spacing = 5f;      // 水平间距
    [SerializeField] private float lineSpacing = 5f;  // 行间距

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        float w = padding.horizontal;
        float max = w;
        foreach (RectTransform child in rectChildren)
        {
            if (!child.gameObject.activeSelf) continue;
            w += LayoutUtility.GetPreferredWidth(child) + spacing;
            if (w - spacing > max) max = w - spacing;
        }
        SetLayoutInputForAxis(max, max, -1, 0);
    }

    public override void CalculateLayoutInputVertical()
    {
        float h = ComputeHeight(rectTransform.rect.width, false);
        SetLayoutInputForAxis(h, h, -1, 1);
    }

    public override void SetLayoutHorizontal() { }

    public override void SetLayoutVertical()
    {
        ComputeHeight(rectTransform.rect.width, true);
    }

    // 按容器宽度模拟换行；apply 为 true 时同时摆放子项。只设置位置，不修改子元素尺寸。
    private float ComputeHeight(float width, bool apply)
    {
        if (width <= 0f || rectChildren.Count == 0)
            return padding.vertical;

        float limit = width - padding.right;
        float x = padding.left;
        float y = padding.top;
        float rowH = 0f;

        foreach (RectTransform child in rectChildren)
        {
            if (!child.gameObject.activeSelf) continue;

            // 使用子元素自身尺寸，布局不改动它们
            float w = child.rect.width;
            float h = child.rect.height;

            // 放不下就换行
            if (x + w > limit && x > padding.left)
            {
                x = padding.left;
                y += rowH + lineSpacing;
                rowH = 0f;
            }

            if (apply)
                PositionChild(child, x, y, w, h);

            x += w + spacing;
            rowH = Mathf.Max(rowH, h);
        }
        return y + rowH + padding.bottom;
    }

    // 只设置位置（左上角锚定），不改子元素尺寸
    private void PositionChild(RectTransform child, float x, float y, float w, float h)
    {
        var min = child.anchorMin;
        var max = child.anchorMax;
        min.x = 0f; max.x = 0f;
        min.y = 1f; max.y = 1f;
        child.anchorMin = min;
        child.anchorMax = max;

        child.anchoredPosition = new Vector2(x + w * child.pivot.x, -(y + h * child.pivot.y));
    }
}