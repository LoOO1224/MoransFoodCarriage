// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechWorldInteractionMarker.cs
// - 역할: NPC/상호작용 오브젝트 머리 위에 반짝이는 화살표 UI를 띄우는 컴포넌트입니다.
// - 영화 비유: 배우 머리 위에 관객이 봐야 할 작은 조명 표시를 띄우는 무대 표식 담당입니다.
// - 유지보수 포인트: 상호작용 판정은 Actor가 맡고, 이 클래스는 화면에 보이는 표식만 맡습니다.
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 대상 Transform의 월드 위치를 카메라 화면 좌표로 바꿔 Screen Space UI 화살표를 표시합니다.
/// Game View에서는 배경/스프라이트 정렬에 묻히지 않고 항상 위쪽 UI처럼 보입니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechWorldInteractionMarker : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform Transform_Target;
    [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 1.8f, 0f);

    [Header("View")]
    [SerializeField] private string _markerText = "\u25BC";
    [SerializeField] private Color _markerColor = Color.red;
    [SerializeField] private float _uiFontSize = 42f;
    [SerializeField] private float _blinkSpeed = 6f;
    [SerializeField] private float _minimumAlpha = 0.25f;
    [SerializeField] private int _sortingOrder = 45000;

    private Canvas Canvas_Marker;
    private RectTransform Rect_Marker;
    private TextMeshProUGUI Text_Marker;
    private Camera Camera_Main;

    /// <summary>
    /// 표식 배우가 무대에 올라올 때 화면용 Canvas와 TMP 텍스트를 준비합니다.
    /// </summary>
    private void Awake()
    {
        PrepareMarkerView();
    }

    /// <summary>
    /// 매 프레임 대상 배우의 머리 위 월드 좌표를 화면 좌표로 변환합니다.
    /// </summary>
    private void LateUpdate()
    {
        if (Transform_Target == null || !Transform_Target.gameObject.activeInHierarchy)
        {
            SetTextVisible(false);
            return;
        }

        if (Camera_Main == null)
            Camera_Main = Camera.main;

        if (Camera_Main == null)
        {
            SetTextVisible(false);
            return;
        }

        Vector3 screenPosition = Camera_Main.WorldToScreenPoint(Transform_Target.position + _worldOffset);

        if (screenPosition.z < 0f)
        {
            SetTextVisible(false);
            return;
        }

        PrepareMarkerView();
        SetTextVisible(true);
        Rect_Marker.position = screenPosition;
        UpdateBlinkAlpha();
    }

    /// <summary>
    /// Stage Controller가 표식의 대상, 위치, 문양을 지정합니다.
    /// </summary>
    public void RequestSetup(Transform targetTransform, Vector3 worldOffset, string markerText)
    {
        RequestSetup(targetTransform, worldOffset, markerText, _markerColor);
    }

    /// <summary>
    /// Stage Controller가 표식의 대상, 위치, 문양, 색을 함께 지정합니다.
    /// 촌장과 수레처럼 서로 다른 배우도 같은 표식 컴포넌트를 재사용합니다.
    /// </summary>
    public void RequestSetup(Transform targetTransform, Vector3 worldOffset, string markerText, Color markerColor)
    {
        Transform_Target = targetTransform;
        _worldOffset = worldOffset;
        _markerColor = markerColor;

        if (!string.IsNullOrEmpty(markerText))
            _markerText = markerText;

        PrepareMarkerView();
        ApplyMarkerText();
    }

    /// <summary>
    /// 현재 장면 상태에 따라 표식을 켜거나 끕니다.
    /// </summary>
    public void RequestSetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    private void PrepareMarkerView()
    {
        DisableLegacyWorldTextIfNeeded();

        if (Canvas_Marker == null)
        {
            GameObject canvasObject = new GameObject("Canvas_WorldInteractionMarker");
            canvasObject.transform.SetParent(transform, false);

            Canvas_Marker = canvasObject.AddComponent<Canvas>();
            Canvas_Marker.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas_Marker.overrideSorting = true;
            Canvas_Marker.sortingOrder = _sortingOrder;

            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        if (Rect_Marker == null)
        {
            GameObject textObject = new GameObject("Text_Marker");
            textObject.transform.SetParent(Canvas_Marker.transform, false);

            Rect_Marker = textObject.AddComponent<RectTransform>();
            Rect_Marker.anchorMin = new Vector2(0.5f, 0.5f);
            Rect_Marker.anchorMax = new Vector2(0.5f, 0.5f);
            Rect_Marker.pivot = new Vector2(0.5f, 0.5f);
            Rect_Marker.sizeDelta = new Vector2(96f, 72f);

            Text_Marker = textObject.AddComponent<TextMeshProUGUI>();
        }

        ApplyMarkerText();
    }

    private void DisableLegacyWorldTextIfNeeded()
    {
        TextMeshPro legacyText = GetComponent<TextMeshPro>();

        if (legacyText != null)
            legacyText.enabled = false;
    }

    private void ApplyMarkerText()
    {
        if (Text_Marker == null)
            return;

        Text_Marker.text = _markerText;
        Text_Marker.fontSize = _uiFontSize;
        Text_Marker.alignment = TextAlignmentOptions.Center;
        Text_Marker.color = _markerColor;
        Text_Marker.raycastTarget = false;
        Text_Marker.textWrappingMode = TextWrappingModes.NoWrap;
        Text_Marker.fontStyle = FontStyles.Bold;
        OOTechTMPFontUtility.ApplyProjectFont(Text_Marker);
    }

    private void UpdateBlinkAlpha()
    {
        if (Text_Marker == null)
            return;

        Color color = _markerColor;
        float blink01 = (Mathf.Sin(Time.unscaledTime * _blinkSpeed) + 1f) * 0.5f;
        color.a = Mathf.Lerp(_minimumAlpha, 1f, blink01);
        Text_Marker.color = color;
    }

    private void SetTextVisible(bool isVisible)
    {
        if (Text_Marker != null)
            Text_Marker.enabled = isVisible;
    }
}
