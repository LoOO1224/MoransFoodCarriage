// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechWorldMapOverlayController.cs
// - ??븷: WorldMapGroup???대졇????移대찓?쇰? ?붾뱶留?諛곌꼍??留욎텛怨??덈궡李쎌쓣 愿由ы빀?덈떎.
// - ?곹솕 鍮꾩쑀: ?붾뱶留?臾대?媛 ?대━硫?珥ъ쁺媛먮룆???꾩껜 ?명듃瑜??쒕늿???↔퀬,
//   ?덈궡 ?ㅽ깭?꾧? "珥덉긽???뚰봽 湲곕뒫? 諛쒗몴 ???낅뜲?댄듃 ?덉젙" ?삳쭚???좉퉸 蹂댁뿬以띾땲??
// - ?좎?蹂댁닔 ?ъ씤?? ?ㅼ젣 ?덈궡李??ㅻ툕?앺듃??WorldMapGuideUIGroup???먭퀬,
//   ??Controller??耳쒓린/?꾧린? 移대찓??吏?섎쭔 ?대떦?⑸땲??
// =============================================================================
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// WorldMapGroup ?꾩슜 ?ㅻ쾭?덉씠 吏꾪뻾???대떦?⑸땲??
/// Game View?먯꽌???붾뱶留??꾩껜媛 蹂댁씠怨? ?덈궡李쎌쓣 ?대┃?섎㈃ 李쎈쭔 ?ロ엺 ???뚯븘媛湲?踰꾪듉?쇰줈 ?댁쟾 ?λ㈃??蹂듦??⑸땲??
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
    [SerializeField] private float _wheelZoomSpeed = 0.18f;
    [SerializeField] private float _minimumZoomRatio = 0.55f;

    [Header("Guide")]
    [SerializeField] private bool _isShowAnnouncementOnOpen = true;
    [SerializeField] private bool _isCloseAnnouncementByAnyClick = true;
    [SerializeField] private int _guideSortingOrder = 1680;
    [SerializeField] private string _guideTitle = "월드맵";
    [TextArea(3, 8)]
    [SerializeField] private string _guideDescription = "각 스테이지의 중요 인물 초상화가 이곳에 표시됩니다.\n클리어한 스테이지는 완료 초상화로 바뀝니다.";

    private CameraFollowController Camera_Follow;
    private OOTechWorldMapOverlayView View_Overlay;
    private GameObject Root_GuideCanvas;
    private Bounds _mapBounds;
    private float _baseMapOrthographicSize;
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
    /// ?붾뱶留?臾대?媛 ?대┫ ???꾩껜 吏?꾩뿉 移대찓?쇰? 留욎텛怨??덈궡李쎌쓣 耳?땲??
    /// </summary>
    private void OnEnable()
    {
        _isGuideClosed = false;
        ApplyStageClearPortraitState();
        ApplyWorldMapCameraView();

        if (_isShowAnnouncementOnOpen)
            RequestOpenAnnouncement();
        else
            SetGuideActive(false);
    }

    /// <summary>
    /// ?붾뱶留?臾대?瑜??レ쓣 ??移대찓?쇰? ?댁쟾 Road/Stage ?쒖젏?쇰줈 ?뚮젮?볦뒿?덈떎.
    /// </summary>
    private void OnDisable()
    {
        _isDraggingMap = false;
        SetGuideActive(false);
        RestoreCameraView();
    }

    /// <summary>
    /// ?덈궡李??대┃ ?リ린? ?붾뱶留??쒕옒洹??낅젰??留??꾨젅???뺤씤?⑸땲??
    /// </summary>
    private void Update()
    {
        HandleGuideCloseInput();
        HandleWheelZoomInput();
        HandleMapDragInput();
    }

    /// <summary>
    /// ?붾뱶留?諛곌꼍 SpriteRenderer媛 ?붾㈃???쒕늿???ㅼ뼱?ㅻ룄濡?移대찓???꾩튂? ?ш린瑜?留욎땅?덈떎.
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

        _baseMapOrthographicSize = Mathf.Max(0.1f, targetSize);

        Vector3 cameraPosition = _mapBounds.center;
        cameraPosition.z = Camera_Main.transform.position.z;

        Camera_Main.orthographic = true;
        Camera_Main.orthographicSize = _baseMapOrthographicSize;
        Camera_Main.transform.position = cameraPosition;
        ClampCameraToMapBounds();
    }

    private void ApplyStageClearPortraitState()
    {
        ApplySingleStagePortraitState("Stage1", "S1_1", "S1_2");
        ApplySingleStagePortraitState("Stage2", "S2_1", "S2_2");
        ApplySingleStagePortraitState("Stage3", "S3_1", "S3_2");
    }

    private void ApplySingleStagePortraitState(string stageId, string aliveObjectName, string clearObjectName)
    {
        bool isCleared = OOTechGameManager.Inst != null && OOTechGameManager.Inst.IsStageCleared(stageId);
        SetChildActive(aliveObjectName, !isCleared);
        SetChildActive(clearObjectName, isCleared);
    }

    private void SetChildActive(string childName, bool isActive)
    {
        Transform childTransform = RequestChildObjectByName(transform, childName);

        if (childTransform != null)
            childTransform.gameObject.SetActive(isActive);
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

    private void HandleWheelZoomInput()
    {
        if (Camera_Main == null || !_hasMapBounds || !Camera_Main.orthographic)
            return;

        if (Root_GuideCanvas != null && Root_GuideCanvas.activeInHierarchy)
            return;

        float scrollDelta = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scrollDelta) <= 0.01f)
            return;

        float minimumSize = _baseMapOrthographicSize * Mathf.Clamp(_minimumZoomRatio, 0.1f, 1f);
        float maximumSize = _maximumOrthographicSize > 0f ? _maximumOrthographicSize : _baseMapOrthographicSize;
        float targetSize = Camera_Main.orthographicSize * (1f - scrollDelta * _wheelZoomSpeed);
        Camera_Main.orthographicSize = Mathf.Clamp(targetSize, minimumSize, maximumSize);
        ClampCameraToMapBounds();
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private Transform RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrWhiteSpace(objectName))
            return null;

        if (rootTransform.name == objectName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = RequestChildObjectByName(rootTransform.GetChild(index), objectName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
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

