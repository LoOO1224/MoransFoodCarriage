using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// WorldMapGroup이 열렸을 때 카메라를 월드맵 배경에 맞추고 드래그 이동을 처리합니다.
/// Game View에서는 플레이어가 좌클릭을 누른 채 맵을 둘러볼 수 있게 합니다.
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
    [SerializeField] private bool _isShowDragGuide = false;
    [SerializeField] private int _guideSortingOrder = 1680;
    [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private string _guideTitle = "월드맵";
    [SerializeField] private string _guideDescription = "좌클릭을 누른 채 맵을 움직여 볼 수 있습니다.";

    private CameraFollowController Camera_Follow;
    private bool _hasSavedCameraState;
    private Vector3 _savedCameraPosition;
    private float _savedOrthographicSize;
    private bool _savedOrthographic;
    private bool _savedFollowEnabled;
    private Bounds _mapBounds;
    private bool _hasMapBounds;
    private bool _isDraggingMap;
    private bool _isGuideClosed;
    private Vector2 _dragStartScreenPosition;
    private Vector3 _dragStartCameraPosition;
    private OOTechWorldMapOverlayView View_Overlay;
    private GameObject Root_GuideCanvas;

    /// <summary>
    /// 월드맵 무대가 열리면 카메라를 맵 보기 상태로 바꾸고 안내 말풍선을 켭니다.
    /// </summary>
    private void OnEnable()
    {
        ApplyWorldMapCameraView();

        if (_isShowDragGuide)
            CreateDragGuideIfNeeded();
        else
            SetDragGuideActive(false);
    }

    /// <summary>
    /// 월드맵 무대가 닫히면 안내와 카메라 상태를 원래 장면으로 복구합니다.
    /// </summary>
    private void OnDisable()
    {
        _isDraggingMap = false;
        SetDragGuideActive(false);
        RestoreCameraView();
    }

    /// <summary>
    /// 월드맵 위에서 마우스 드래그 입력을 계속 확인합니다.
    /// </summary>
    private void Update()
    {
        HandleMapDragInput();
    }

    /// <summary>
    /// 월드맵 배경 이미지가 한 화면에 들어오도록 카메라 위치와 크기를 맞춥니다.
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

    /// <summary>
    /// 월드맵을 닫을 때 이전 로드/스테이지 카메라 상태로 되돌립니다.
    /// </summary>
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

    /// <summary>
    /// 월드맵 전용 카메라로 바꾸기 전에 원래 카메라 값을 저장합니다.
    /// </summary>
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

    /// <summary>
    /// WorldMapGroup 안의 SpriteRenderer를 찾아 맵 크기 계산에 사용합니다.
    /// </summary>
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

    /// <summary>
    /// 월드맵 스프라이트가 꺼져 있으면 보이도록 켭니다.
    /// </summary>
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

    /// <summary>
    /// 좌클릭을 누른 채 움직이면 카메라를 반대 방향으로 이동해 맵을 둘러보게 합니다.
    /// </summary>
    private void HandleMapDragInput()
    {
        if (!_isEnableMapDrag || Camera_Main == null || !_hasMapBounds || !Camera_Main.orthographic)
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

    /// <summary>
    /// 카메라가 월드맵 바깥까지 나가지 않도록 맵 Bounds 안으로 제한합니다.
    /// </summary>
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

    /// <summary>
    /// 씬에 배치된 월드맵 드래그 안내 말풍선을 연결하고 표시합니다.
    /// </summary>
    private void CreateDragGuideIfNeeded()
    {
        if (_isGuideClosed)
            return;

        ResolveOverlayView();

        if (Root_GuideCanvas != null)
        {
            Root_GuideCanvas.SetActive(true);
            return;
        }

        Debug.LogWarning("[OOTechWorldMapOverlayController] WorldMapGuideUIGroup with OOTechWorldMapOverlayView is missing.");
    }

    /// <summary>
    /// 월드맵 드래그 안내 말풍선을 닫습니다.
    /// </summary>
    private void CloseDragGuide()
    {
        _isGuideClosed = true;
        SetDragGuideActive(false);
    }

    /// <summary>
    /// 월드맵 안내 말풍선을 켜고 끕니다. 맵 전체가 보이는 현재 연출에서는 기본적으로 꺼 둡니다.
    /// </summary>
    private void SetDragGuideActive(bool isActive)
    {
        if (Root_GuideCanvas == null)
            ResolveOverlayView();

        if (Root_GuideCanvas != null)
            Root_GuideCanvas.SetActive(isActive);
    }

    /// <summary>
    /// OOTechWorldMapOverlayView에서 안내 UI 참조를 읽습니다.
    /// </summary>
    private void ResolveOverlayView()
    {
        if (View_Overlay == null)
            View_Overlay = GetComponentInChildren<OOTechWorldMapOverlayView>(true);

        if (View_Overlay == null)
            return;

        View_Overlay.ResolveReferences();
        Root_GuideCanvas = View_Overlay.GuideCanvas;

        if (View_Overlay.GuideTitleText != null)
            View_Overlay.GuideTitleText.text = _guideTitle;

        if (View_Overlay.GuideBodyText != null)
            View_Overlay.GuideBodyText.text = _guideDescription;

        if (View_Overlay.GuideCloseButton != null)
        {
            View_Overlay.GuideCloseButton.onClick.RemoveListener(CloseDragGuide);
            View_Overlay.GuideCloseButton.onClick.AddListener(CloseDragGuide);
        }

        Canvas guideCanvas = Root_GuideCanvas != null ? Root_GuideCanvas.GetComponent<Canvas>() : null;

        if (guideCanvas != null)
        {
            guideCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            guideCanvas.overrideSorting = true;
            guideCanvas.sortingOrder = _guideSortingOrder;
        }
    }
}
