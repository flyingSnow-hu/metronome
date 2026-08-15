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
        // 精确高度需要在知道实际行宽后算，这里给个估算值；
        // 若容器宽度固定，可在这里按实际宽度模拟换行得到准确高度。
        float h = padding.vertical;
        float rowH = 0f;
        foreach (RectTransform child in rectChildren)
        {
            if (!child.gameObject.activeSelf) continue;
            rowH = Mathf.Max(rowH, LayoutUtility.GetPreferredHeight(child));
        }
        h += rowH + lineSpacing;
        SetLayoutInputForAxis(h, h, -1, 1);
    }

    public override void SetLayoutHorizontal() { }

    public override void SetLayoutVertical()
    {
        float x = padding.left;
        float y = padding.top;
        float rowH = 0f;
        float limit = rectTransform.rect.width - padding.right;

        foreach (RectTransform child in rectChildren)
        {
            if (!child.gameObject.activeSelf) continue;

            float w = LayoutUtility.GetPreferredWidth(child);
            float h = LayoutUtility.GetPreferredHeight(child);

            // 放不下就换行
            if (x + w > limit && x > padding.left)
            {
                x = padding.left;
                y += rowH + lineSpacing;
                rowH = 0f;
            }

            SetChildAlongAxis(child, 0, x, w);
            SetChildAlongAxis(child, 1, y, h);

            x += w + spacing;
            rowH = Mathf.Max(rowH, h);
        }
    }
}