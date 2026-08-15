using UnityEngine;

/// <summary>
/// 移动端安全区适配：把挂载的 RectTransform 缩进到 Screen.safeArea 内，
/// 避免内容被刘海/挖孔/底部手势条遮挡。挂在每个全屏 Panel 的根 RectTransform 上即可。
/// </summary>
[DisallowMultipleComponent]
public class SafeArea : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        ApplySafeArea();
    }

    private void Update()
    {
        // 屏幕尺寸或安全区变化时（横竖屏切换、折叠屏等）重新应用
        var screenSize = new Vector2Int(Screen.width, Screen.height);
        if (Screen.safeArea != lastSafeArea || screenSize != lastScreenSize)
            ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        if (rectTransform == null)
            return;

        var safeArea = Screen.safeArea;
        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        // 把像素坐标归一化到 0~1，设成锚点，面板就会缩进到安全区内
        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}
