// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechWorldMapOverlayController.cs
// - 역할: WorldMapGroup이 열렸을 때 카메라를 월드맵 배경에 맞추고 안내창을 관리합니다.
// - 영화 비유: 월드맵 무대가 열리면 촬영감독이 전체 세트를 한눈에 잡고,
//   안내 스태프가 "초상화 워프 기능은 발표 전 업데이트 예정" 팻말을 잠깐 보여줍니다.
// - 유지보수 포인트: 실제 안내창 오브젝트는 WorldMapGuideUIGroup에 두고,
//   이 Controller는 켜기/끄기와 카메라 지휘만 담당합니다.
// =============================================================================
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// WorldMapGroup 전용 오버레이 진행을 담당합니다.
/// Game View에서는 월드맵 전체가 보이고, 안내창을 클릭하면 창만 닫힌 뒤 돌아가기 버튼으로 이전 장면에 복귀합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechWorldMapOverlayController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera Camera_Main;
    [SerializeField] private float _padding = 1.04f;
    [SerializeField] private float _maximumOrthographicSize = 0f;

    [Header("Drag")]
    [SerializeField] private bool _isEnableMapDrag = true;

    [Header("Guide")]
    [SerializeField] private bool _isShowAnnouncementOnOpen = true;
    [SerializeField] private bool _isCloseAnnouncementByAnyClick = true;
    [SerializeField] private int _guideSortingOrder = 1680;
    [SerializeField] private string _guideTitle = "월드맵";
    [TextArea(3, 8)]
    [SerializeField] private string _guideDescription = "월드맵은 발표 전 업데이트 예정입니다.\n각 스테이지의 중요 인물들 초상화를 걸고, 클릭하면 그곳으로 바로 워프되는 기능을 구현 예정입니다.";

    private CameraFollowController Camera_Follow;
    private OOTechWorldMapOverlayView View_Overlay;
    private GameObject Root_GuideCanvas;
    private Bounds _mapBounds;
    private bool _hasMapBounds;
    private bool _hasSavedCameraState;
    private bool _isDraggingMap;
    private bool _isGuideClosed;
    private bool _savedOrthographic;
    private bool _savedFollowEnabled;
    private float _savedOrthographicSize;
    private Vector2 _dragStartScreenPosition;
    private Vector3 _savedCameraPosition;
    private Vector3 _dragStartCameraPosition;

    /// <summary>
    /// 월드맵 무대가 열릴 때 전체 지도에 카메라를 맞추고 안내창을 켭니다.
    /// </summary>
    private void OnEnable()
    {
        _isGuideClosed = false;
        ApplyWorldMapCameraView();

        if (_isShowAnnouncementOnOpen)
            RequestOpenAnnouncement();
        else
            SetGuideActive(false);
    }

    /// <summary>
    /// 월드맵 무대를 닫을 때 카메라를 이전 Road/Stage 시점으로 돌려놓습니다.
    /// </summary>
    private void OnDisable()
    {
        _isDraggingMap = false;
        SetGuideActive(false);
        RestoreCameraView();
    }

    /// <summary>
    /// 안내창 클릭 닫기와 월드맵 드래그 입력을 매 프레임 확인합니다.
    /// </summary>
    private void Update()
    {
        HandleGuideCloseInput();
        HandleMapDragInput();
    }

    /// <summary>
    /// 월드맵 배경 SpriteRenderer가 화면에 한눈에 들어오도록 카메라 위치와 크기를 맞춥니다.
    /// </summary>
    public void ApplyWorldMapCameraView()
    {
        ResolveCameraReference();

        if (Camera_Main == null)
            return;

        SpriteRenderer mapRenderer = GetMapRenderer();

        if (mapRenderer == null)
            return;

        EnsureRendererVisible(mapRenderer);
        SaveCameraStateIfNeeded();

        if (Camera_Follow != null)
            Camera_Follow.enabled = false;

        _mapBounds = mapRenderer.bounds;
        _hasMapBounds = true;

        float aspect = Camera_Main.aspect > 0f ? Camera_Main.aspect : 16f / 9f;
        float sizeByHeight = _mapBounds.extents.y;
        float sizeByWidth = _mapBounds.extents.x / aspect;
        float targetSize = Mathf.Max(sizeByHeight, sizeByWidth) * _padding;

        if (_maximumOrthographicSize > 0f)
            targetSize = Mathf.Min(targetSize, _maximumOrthographicSize);

        Vector3 cameraPosition = _mapBounds.center;
        cameraPosition.z = Camera_Main.transform.position.z;

        Camera_Main.orthographic = true;
        Camera_Main.orthographicSize = Mathf.Max(0.1f, targetSize);
        Camera_Main.transform.position = cameraPosition;
        ClampCameraToMapBounds();
    }

    private void RestoreCameraView()
    {
        if (!_hasSavedCameraState || Camera_Main == null)
            return;

        Camera_Main.transform.position = _savedCameraPosition;
        Camera_Main.orthographicSize = _savedOrthographicSize;
        Camera_Main.orthographic = _savedOrthographic;

        if (Camera_Follow != null)
            Camera_Follow.enabled = _savedFollowEnabled;

        _hasSavedCameraState = false;
        _hasMapBounds = false;
    }

    private void SaveCameraStateIfNeeded()
    {
        if (_hasSavedCameraState || Camera_Main == null)
            return;

        _savedCameraPosition = Camera_Main.transform.position;
        _savedOrthographicSize = Camera_Main.orthographicSize;
        _savedOrthographic = Camera_Main.orthographic;
        _savedFollowEnabled = Camera_Follow != null && Camera_Follow.enabled;
        _hasSavedCameraState = true;
    }

    private void ResolveCameraReference()
    {
        if (Camera_Main == null)
            Camera_Main = Camera.main;

        if (Camera_Follow == null && Camera_Main != null)
            Camera_Main.TryGetComponent(out Camera_Follow);
    }

    private SpriteRenderer GetMapRenderer()
    {
        SpriteRenderer[] rendererArray = GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer bestRenderer = null;
        float bestArea = -1f;

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
                continue;

            Bounds bounds = spriteRenderer.bounds;
            float area = Mathf.Abs(bounds.size.x * bounds.size.y);

            if (area <= bestArea)
                continue;

            bestRenderer = spriteRenderer;
            bestArea = area;
        }

        return bestRenderer;
    }

    private void EnsureRendererVisible(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null)
            return;

        if (!spriteRenderer.gameObject.activeSelf)
            spriteRenderer.gameObject.SetActive(true);

        spriteRenderer.enabled = true;
        Color color = spriteRenderer.color;

        if (color.a <= 0.01f)
        {
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }

    private void RequestOpenAnnouncement()
    {
        if (_isGuideClosed)
            return;

        ResolveOverlayView();
        SetGuideActive(Root_GuideCanvas != null);
    }

    private void HandleGuideCloseInput()
    {
        if (!_isCloseAnnouncementByAnyClick || _isGuideClosed || Root_GuideCanvas == null || !Root_GuideCanvas.activeInHierarchy)
            return;

        if (Input.GetMouseButtonDown(0))
            CloseGuideOnly();
    }

    private void CloseGuideOnly()
    {
        _isGuideClosed = true;
        SetGuideActive(false);
        Debug.Log("[OOTechWorldMapOverlayController] WorldMap announcement panel closed. WorldMap background and return button stay active.");
    }

    private void SetGuideActive(bool isActive)
    {
        if (Root_GuideCanvas == null)
            ResolveOverlayView();

        if (Root_GuideCanvas != null)
            Root_GuideCanvas.SetActive(isActive);
    }

    private void ResolveOverlayView()
    {
        if (View_Overlay == null)
            View_Overlay = GetComponentInChildren<OOTechWorldMapOverlayView>(true);

        if (View_Overlay == null)
            return;

        View_Overlay.ResolveReferences();
        Root_GuideCanvas = View_Overlay.GuideCanvas;

        if (View_Overlay.GuideTitleText != null)
        {
            View_Overlay.GuideTitleText.text = _guideTitle;
            OOTechTMPFontUtility.ApplyProjectFont(View_Overlay.GuideTitleText);
        }

        if (View_Overlay.GuideBodyText != null)
        {
            View_Overlay.GuideBodyText.text = _guideDescription;
            OOTechTMPFontUtility.ApplyProjectFont(View_Overlay.GuideBodyText);
        }

        if (View_Overlay.GuideCloseButton != null)
        {
            View_Overlay.GuideCloseButton.onClick.RemoveListener(CloseGuideOnly);
            View_Overlay.GuideCloseButton.onClick.AddListener(CloseGuideOnly);
        }

        Canvas guideCanvas = Root_GuideCanvas != null ? Root_GuideCanvas.GetComponent<Canvas>() : null;

        if (guideCanvas != null)
        {
            guideCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            guideCanvas.overrideSorting = true;
            guideCanvas.sortingOrder = _guideSortingOrder;
        }
    }

    private void HandleMapDragInput()
    {
        if (!_isEnableMapDrag || Camera_Main == null || !_hasMapBounds || !Camera_Main.orthographic)
            return;

        if (Root_GuideCanvas != null && Root_GuideCanvas.activeInHierarchy)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI())
                return;

            _isDraggingMap = true;
            _dragStartScreenPosition = Input.mousePosition;
            _dragStartCameraPosition = Camera_Main.transform.position;
        }

        if (Input.GetMouseButtonUp(0))
            _isDraggingMap = false;

        if (!_isDraggingMap || !Input.GetMouseButton(0))
            return;

        Vector2 screenDelta = (Vector2)Input.mousePosition - _dragStartScreenPosition;
        float worldUnitPerPixel = Camera_Main.orthographicSize * 2f / Mathf.Max(1, Screen.height);
        Vector3 cameraDelta = new Vector3(-screenDelta.x * worldUnitPerPixel, -screenDelta.y * worldUnitPerPixel, 0f);
        Camera_Main.transform.position = _dragStartCameraPosition + cameraDelta;
        ClampCameraToMapBounds();
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void ClampCameraToMapBounds()
    {
        if (Camera_Main == null || !_hasMapBounds || !Camera_Main.orthographic)
            return;

        float aspect = Camera_Main.aspect > 0f ? Camera_Main.aspect : 16f / 9f;
        float viewHeight = Camera_Main.orthographicSize * 2f;
        float viewWidth = viewHeight * aspect;
        Vector3 cameraPosition = Camera_Main.transform.position;

        if (_mapBounds.size.x <= viewWidth)
            cameraPosition.x = _mapBounds.center.x;
        else
            cameraPosition.x = Mathf.Clamp(cameraPosition.x, _mapBounds.min.x + viewWidth * 0.5f, _mapBounds.max.x - viewWidth * 0.5f);

        if (_mapBounds.size.y <= viewHeight)
            cameraPosition.y = _mapBounds.center.y;
        else
            cameraPosition.y = Mathf.Clamp(cameraPosition.y, _mapBounds.min.y + viewHeight * 0.5f, _mapBounds.max.y - viewHeight * 0.5f);

        Camera_Main.transform.position = cameraPosition;
    }
}
