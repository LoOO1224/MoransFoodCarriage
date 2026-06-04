// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechRoadToStage1Controller.cs
// - 역할: 로드맵, 월드맵, 스테이지 전환 흐름을 담당하는 장면 Controller입니다.
// - 감독 관점: 길 위의 장면 전환 큐시트를 들고 있는 무대감독입니다.
// - 유지보수 포인트: 배경/버튼/캐릭터 배치는 오브젝트와 View가 맡고, 이 스크립트는 순서 지휘만 맡아야 합니다.
// =============================================================================
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 시작 지점부터 Stage 입구까지 MFC 이동을 지휘하는 RoadGroup 컨트롤러입니다.
/// 맵 이미지는 무대 배경이고, MFC 배우는 정해진 도로 띠 위에서만 오른쪽으로 이동합니다.
/// </summary>
public class OOTechRoadToStage1Controller : MonoBehaviour
{
    // 읽는 순서:
    // 1. OnEnable: 현재 RoadGroup, MFC, 배경 맵, HUD를 준비합니다.
    // 2. Update/MoveMFC 계열: D키 이동과 도로 띠 안 위치 제한을 처리합니다.
    // 3. ChangeToNextMapRoutine: StartPointMap -> RoadMap1 -> RoadMap2 -> Stage1EntryMap 전환을 담당합니다.
    // 4. OpenTargetStageGroup: 마지막 맵 끝에 도달하면 Stage1Group 같은 목표 StageGroup을 켭니다.
    // 5. PrepareRoadHUD/PlayTutorial: HUD와 초반 튜토리얼/대사 흐름을 연결합니다.
    // 유지보수 주의:
    // - 각 RoadGroup의 배경 이미지는 하이어라키에서 직접 교체합니다.
    // - 다음 StageGroup 이름은 ConfigureRoadFlow와 Inspector 값으로 맞춥니다.
    // - Road 공통 로직이 늘어나면 RoadBaseController로 분리하는 것이 좋습니다.

    private const string _roleMFC = "MFC";
    private const string _roleStartPointMap = "StartPointMap";
    private const string _roleRoadMap1 = "RoadMap1";
    private const string _roleRoadMap2 = "RoadMap2";
    private const string _roleStage1EntryMap = "Stage1EntryMap";
    private const string _roleMFCStartPoint = "MFCStartPoint";

    [Header("Scene Components")]
    [SerializeField] private OOTechSceneContext Context_Scene;
    [SerializeField] private OOTechRoadHUDController HUD_Road;
    [SerializeField] private OOTechTutorial2Controller Tutorial2_Controller;

    [HideInInspector]
    [SerializeField] private GameObject Object_MFC;
    [HideInInspector]
    [SerializeField] private Animator Animator_MFC;
    [HideInInspector]
    [SerializeField] private SpriteRenderer Renderer_MFC;

    [HideInInspector]
    [SerializeField] private Camera Camera_Main;
    [HideInInspector]
    [SerializeField] private CameraFollowController Camera_Follow;

    [HideInInspector]
    [SerializeField] private GameObject Object_StartPointMap;
    [HideInInspector]
    [SerializeField] private GameObject Object_RoadMap1;
    [HideInInspector]
    [SerializeField] private GameObject Object_RoadMap2;
    [HideInInspector]
    [SerializeField] private GameObject Object_Stage1EntryMap;

    [HideInInspector]
    [SerializeField] private Transform Transform_MFCStartPoint;

    [Header("Road Lane")]
    [SerializeField] private KeyCode _moveRightKey = KeyCode.D;
    [SerializeField] private float _moveSpeed = 4.2f;
    [SerializeField, Range(0f, 1f)] private float _roadLaneNormalizedHeight = 0.23f;
    [SerializeField, Range(0f, 0.45f)] private float _entryMarginRatio = 0.1f;
    [SerializeField, Range(0f, 0.45f)] private float _exitMarginRatio = 0.08f;
    [SerializeField] private bool _isUseMFCStartPointOnFirstMap = true;

    [Header("MFC Animation")]
    [SerializeField] private string _mfcWalkStateName = "MFC";
    [SerializeField] private bool _isAnimateMFCOnlyWhileMoving = true;
    [SerializeField] private float _mfcWalkAnimationSpeed = 0.85f;

    [Header("Map Transition")]
    [SerializeField] private float _fadeOutSeconds = 0.45f;
    [SerializeField] private float _blackoutHoldSeconds = 0.15f;
    [SerializeField] private float _fadeInSeconds = 0.55f;
    [SerializeField] private Color _fadeColor = Color.black;

    [Header("Destination")]
    [SerializeField] private string _currentGroupName = "1st_Road_to_Stage1";
    [SerializeField] private string _targetStageGroupName = "Stage1Group";
    [SerializeField] private string[] _stageGroupNameArray =
    {
        "Stage1Group",
        "Stage2Group",
        "Stage3Group",
        "Stage4Group",
        "FinalStageGroup"
    };

    private GameObject[] _mapObjectArray;
    private int _currentMapIndex;
    private bool _isChangingMap;
    private bool _isRoadTripComplete;
    private bool _isRoadMap1ArrivalCuePlayed;
    private Coroutine _openingTutorialCoroutine;
    private Canvas _fadeCanvas;
    private Image Image_FadeOverlay;

    private readonly string[] _blockingGroupNameArray =
    {
        "MainMenuGroup",
        "CodexGroup",
        "Prologue1Group",
        "Prologue2Group",
        "Tutorial1Group",
        "Senario1Group",
        "WorldMapGroup",
        "1st_Road_to_Stage1",
        "2nd_Road_to_Stage2",
        "3rd_Road_to_Stage3",
        "4th_Road_to_Stage4",
        "Final_Road_to_FinalStage",
        "CookingGroup",
        "Stage1Group",
        "Stage2Group",
        "Stage3Group",
        "Stage4Group",
        "FinalStageGroup",
        "EpilogueGroup",
        "DialogueGroup",
        "TutorialGuideGroup"
    };

    /// <summary>
    /// 씬 참조, 카메라, HUD, 페이드 소품을 미리 연결합니다.
    /// </summary>
    private void Awake()
    {
        ResolveSceneReferences();
        ResolveCameraReference();
        CacheHUDReference();
        CacheTutorial2Reference();
        CreateFadeOverlayIfNeeded();
    }

    /// <summary>
    /// 같은 RoadGroup 구조를 Stage 번호별로 재사용하기 위해 현재/목표 그룹 이름을 설정합니다.
    /// </summary>
    public void ConfigureRoadFlow(string currentGroupName, string targetStageGroupName)
    {
        if (!string.IsNullOrEmpty(currentGroupName))
            _currentGroupName = currentGroupName;

        if (!string.IsNullOrEmpty(targetStageGroupName))
            _targetStageGroupName = targetStageGroupName;

        if (HUD_Road != null)
            HUD_Road.SetOwnerGroupName(_currentGroupName);
    }

    /// <summary>
    /// RoadGroup이 켜지면 MFC를 첫 맵 시작점에 놓고 HUD와 튜토리얼을 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        ResolveSceneReferences();
        ResolveCameraReference();
        CacheHUDReference();
        CacheTutorial2Reference();
        CloseBlockingSceneGroups();
        PrepareRoadHUD();
        PrepareRoadTrip();
        StartOpeningTutorialIfNeeded();
    }

    /// <summary>
    /// RoadGroup이 닫히면 코루틴, 애니메이션, HUD, 페이드 상태를 정리합니다.
    /// </summary>
    private void OnDisable()
    {
        StopOpeningTutorial();
        StopAllCoroutines();
        _openingTutorialCoroutine = null;
        _isChangingMap = false;
        SetMFCAnimationPlaying(false);
        SetRoadHUDVisible(false);
        HideFadeOverlay();
    }

    /// <summary>
    /// 튜토리얼/오버레이/페이드 중이 아닐 때 D키 입력으로 MFC를 오른쪽 이동시킵니다.
    /// </summary>
    private void Update()
    {
        if (Tutorial2_Controller != null && Tutorial2_Controller.IsTutorialRunning)
        {
            SetMFCAnimationPlaying(false);
            return;
        }

        if (HUD_Road != null && HUD_Road.IsOverlayOpen)
        {
            SetMFCAnimationPlaying(false);
            return;
        }

        if (_isChangingMap || _isRoadTripComplete || Object_MFC == null)
        {
            SetMFCAnimationPlaying(false);
            return;
        }

        if (Input.GetKey(_moveRightKey))
        {
            SetMFCAnimationPlaying(true);
            MoveMFCRight();
        }
        else
        {
            SetMFCAnimationPlaying(false);
        }
    }

    /// <summary>
    /// MFC 배우와 맵 배경 오브젝트를 씬 이름 또는 역할표로 찾습니다.
    /// 인스펙터 참조가 있으면 그 값을 우선 사용해 나중에 배경만 교체하기 쉽게 둡니다.
    /// </summary>
    public void ResolveSceneReferences()
    {
        CacheSceneContextReference();

        Object_MFC = ResolveChild(Object_MFC, "MFC");
        Animator_MFC = ResolveComponent(Animator_MFC, Object_MFC);
        Renderer_MFC = ResolveComponent(Renderer_MFC, Object_MFC);

        Object_StartPointMap = ResolveChild(Object_StartPointMap, "StartPointMap", "RoadMap1");
        Object_RoadMap1 = ResolveChild(Object_RoadMap1, "RoadMap1", "RoadMap2");
        Object_RoadMap2 = ResolveChild(Object_RoadMap2, "RoadMap2", "RoadMap3");
        Object_Stage1EntryMap = ResolveChild(Object_Stage1EntryMap, "Stage1EntryMap", "Stage1_Entry");
        Transform_MFCStartPoint = ResolveChildTransform(Transform_MFCStartPoint, "MFC_StartPoint", "StartPoint_MFC");

        _mapObjectArray = new[]
        {
            Object_StartPointMap,
            Object_RoadMap1,
            Object_RoadMap2,
            Object_Stage1EntryMap
        };
    }

    /// <summary>
    /// 이름 검색보다 먼저 OOTechSceneObject 역할표를 읽습니다.
    /// Road 감독은 "MFC", "RoadMap1" 같은 역할만 알고, 구체 오브젝트 배치는 배우가 갖습니다.
    /// </summary>
    private void CacheSceneContextReference()
    {
        if (Context_Scene == null)
            Context_Scene = GetComponent<OOTechSceneContext>();

        if (Context_Scene == null)
            return;

        Context_Scene.CacheSceneObjects();
        Object_MFC = ResolveRoleObject(_roleMFC, Object_MFC);
        Object_StartPointMap = ResolveRoleObject(_roleStartPointMap, Object_StartPointMap);
        Object_RoadMap1 = ResolveRoleObject(_roleRoadMap1, Object_RoadMap1);
        Object_RoadMap2 = ResolveRoleObject(_roleRoadMap2, Object_RoadMap2);
        Object_Stage1EntryMap = ResolveRoleObject(_roleStage1EntryMap, Object_Stage1EntryMap);

        GameObject startPointObject = ResolveRoleObject(_roleMFCStartPoint, Transform_MFCStartPoint != null ? Transform_MFCStartPoint.gameObject : null);
        Transform_MFCStartPoint = startPointObject != null ? startPointObject.transform : Transform_MFCStartPoint;
    }

    private GameObject ResolveRoleObject(string roleId, GameObject fallback)
    {
        GameObject roleObject = Context_Scene.GetRoleObject(roleId);
        return roleObject != null ? roleObject : fallback;
    }

    /// <summary>
    /// 첫 맵을 켜고 MFC를 도로 시작점에 배치해 로드 여행을 준비합니다.
    /// </summary>
    private void PrepareRoadTrip()
    {
        if (_mapObjectArray == null || _mapObjectArray.Length == 0)
            ResolveSceneReferences();

        _currentMapIndex = 0;
        _isChangingMap = false;
        _isRoadTripComplete = false;
        _isRoadMap1ArrivalCuePlayed = false;

        SetOnlyCurrentMapActive();
        EnsureCurrentMapVisible();
        SetMFCActive(true);
        EnsureMFCVisible();
        PlaceMFCAtOpeningPosition();
        FocusCameraOnMFC(true);
        HideFadeOverlay();
        PrepareMFCAnimation();
    }

    /// <summary>
    /// 현재 맵의 도로 띠를 따라 MFC를 오른쪽으로 이동시키고 끝에 닿으면 다음 맵으로 넘깁니다.
    /// </summary>
    private void MoveMFCRight()
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (mapRenderer == null)
            return;

        Vector3 position = Object_MFC.transform.position;
        position.x += _moveSpeed * Time.deltaTime;
        position.y = CalculateRoadLaneY(mapRenderer);
        position.x = Mathf.Min(position.x, CalculateExitX(mapRenderer));
        Object_MFC.transform.position = position;

        if (position.x >= CalculateExitX(mapRenderer))
            StartCoroutine(ChangeToNextMapRoutine());
    }

    /// <summary>
    /// 화면을 어둡게 했다가 다음 맵 또는 StageGroup으로 전환하는 페이드 루틴입니다.
    /// </summary>
    private IEnumerator ChangeToNextMapRoutine()
    {
        if (_isChangingMap)
            yield break;

        _isChangingMap = true;
        SetMFCAnimationPlaying(false);
        yield return FadeOverlayRoutine(0f, 1f, _fadeOutSeconds);

        if (_blackoutHoldSeconds > 0f)
            yield return new WaitForSeconds(_blackoutHoldSeconds);

        if (_currentMapIndex >= _mapObjectArray.Length - 1)
        {
            _isRoadTripComplete = true;
            OpenTargetStageGroup();
            yield return FadeOverlayRoutine(1f, 0f, _fadeInSeconds);
            _isChangingMap = false;
            SetSceneGroupActive(_currentGroupName, false);
            Debug.Log("[OOTechRoadToStage1Controller] Stage 1 entrance reached.");
            yield break;
        }

        _currentMapIndex++;
        SetOnlyCurrentMapActive();
        EnsureCurrentMapVisible();
        PlaceMFCAtMapEntry();
        EnsureMFCVisible();
        FocusCameraOnMFC(true);
        UnlockCookingIfNeeded();

        yield return FadeOverlayRoutine(1f, 0f, _fadeInSeconds);
        yield return PlayRoadMapArrivalCueIfNeeded();
        _isChangingMap = false;
    }

    /// <summary>
    /// 현재 인덱스의 맵 배경만 켜고 나머지 배경은 끕니다.
    /// </summary>
    private void SetOnlyCurrentMapActive()
    {
        if (_mapObjectArray == null)
            return;

        for (int index = 0; index < _mapObjectArray.Length; index++)
        {
            if (_mapObjectArray[index] != null)
                _mapObjectArray[index].SetActive(index == _currentMapIndex);
        }
    }

    /// <summary>
    /// MFC를 현재 맵 왼쪽 진입 위치와 도로 높이에 맞춥니다.
    /// </summary>
    private void PlaceMFCAtMapEntry()
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (Object_MFC == null || mapRenderer == null)
            return;

        Object_MFC.transform.position = CalculateMapEntryPosition(mapRenderer);
    }

    /// <summary>
    /// 첫 맵 시작은 사용자가 하이어라키에 둔 MFC_StartPoint 위치표를 우선 사용합니다.
    /// 위치표가 없으면 현재 배치된 MFC를 그대로 두어 감독이 잡아둔 무대 위치를 존중합니다.
    /// </summary>
    private void PlaceMFCAtOpeningPosition()
    {
        if (Object_MFC == null)
            return;

        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (_isUseMFCStartPointOnFirstMap && Transform_MFCStartPoint != null)
        {
            if (mapRenderer == null || IsPositionInsideMapBounds(Transform_MFCStartPoint.position, mapRenderer))
            {
                Object_MFC.transform.position = Transform_MFCStartPoint.position;
                return;
            }

            Vector3 entryPosition = CalculateMapEntryPosition(mapRenderer);
            Transform_MFCStartPoint.position = entryPosition;
            Object_MFC.transform.position = entryPosition;
            Debug.LogWarning($"[OOTechRoadToStage1Controller] {gameObject.name} MFC_StartPoint was outside StartPointMap, so it was moved back onto the road.");
            return;
        }

        if (!_isUseMFCStartPointOnFirstMap)
            return;

        if (mapRenderer == null)
            return;

        Vector3 position = Object_MFC.transform.position;
        position.y = CalculateRoadLaneY(mapRenderer);
        Object_MFC.transform.position = position;
    }

    /// <summary>
    /// 현재 맵 왼쪽 도로 진입 위치를 계산합니다.
    /// </summary>
    private Vector3 CalculateMapEntryPosition(SpriteRenderer mapRenderer)
    {
        Bounds bounds = mapRenderer.bounds;
        Vector3 position = Object_MFC != null ? Object_MFC.transform.position : Vector3.zero;
        position.x = bounds.min.x + (bounds.size.x * _entryMarginRatio);
        position.y = CalculateRoadLaneY(mapRenderer);
        return position;
    }

    /// <summary>
    /// MFC 시작 위치표가 현재 배경 안쪽에 있는지 확인합니다.
    /// </summary>
    private bool IsPositionInsideMapBounds(Vector3 position, SpriteRenderer mapRenderer)
    {
        if (mapRenderer == null)
            return true;

        Bounds bounds = mapRenderer.bounds;
        float horizontalMargin = bounds.size.x * 0.05f;
        float verticalMargin = bounds.size.y * 0.05f;
        return position.x >= bounds.min.x - horizontalMargin &&
               position.x <= bounds.max.x + horizontalMargin &&
               position.y >= bounds.min.y - verticalMargin &&
               position.y <= bounds.max.y + verticalMargin;
    }

    /// <summary>
    /// 현재 맵 배경 스프라이트가 보이도록 Renderer 상태를 복구합니다.
    /// </summary>
    private void EnsureCurrentMapVisible()
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (mapRenderer == null)
            return;

        mapRenderer.enabled = true;
        Color color = mapRenderer.color;

        if (color.a <= 0.01f)
        {
            color.a = 1f;
            mapRenderer.color = color;
        }
    }

    /// <summary>
    /// MFC 배우가 실수로 꺼지거나 투명해진 경우 화면에 보이도록 복구합니다.
    /// </summary>
    private void EnsureMFCVisible()
    {
        if (Object_MFC == null)
            return;

        Object_MFC.SetActive(true);

        if (Renderer_MFC == null)
            Renderer_MFC = Object_MFC.GetComponent<SpriteRenderer>();

        if (Renderer_MFC == null)
            return;

        Renderer_MFC.enabled = true;
        Color color = Renderer_MFC.color;

        if (color.a <= 0.01f)
        {
            color.a = 1f;
            Renderer_MFC.color = color;
        }
    }

    /// <summary>
    /// MFC 걷기 애니메이션을 첫 프레임에 준비합니다.
    /// 감독이 이동 큐를 줄 때만 실제 애니메이션 속도를 올립니다.
    /// </summary>
    private void PrepareMFCAnimation()
    {
        if (Animator_MFC == null)
            return;

        Animator_MFC.enabled = true;
        Animator_MFC.Play(_mfcWalkStateName, 0, 0f);
        Animator_MFC.Update(0f);
        SetMFCAnimationPlaying(false);
    }

    /// <summary>
    /// D키 이동 중일 때만 MFC 애니메이션이 재생되게 합니다.
    /// </summary>
    private void SetMFCAnimationPlaying(bool isPlaying)
    {
        if (Animator_MFC == null || !_isAnimateMFCOnlyWhileMoving)
            return;

        Animator_MFC.speed = isPlaying ? Mathf.Max(0.01f, _mfcWalkAnimationSpeed) : 0f;
    }

    /// <summary>
    /// 현재 맵 이미지의 세로 크기에서 도로 띠 Y 위치를 계산합니다.
    /// </summary>
    private float CalculateRoadLaneY(SpriteRenderer mapRenderer)
    {
        Bounds bounds = mapRenderer.bounds;
        return bounds.min.y + (bounds.size.y * _roadLaneNormalizedHeight);
    }

    /// <summary>
    /// 현재 맵 오른쪽 끝에서 다음 장면으로 넘어갈 X 위치를 계산합니다.
    /// </summary>
    private float CalculateExitX(SpriteRenderer mapRenderer)
    {
        Bounds bounds = mapRenderer.bounds;
        return bounds.max.x - (bounds.size.x * _exitMarginRatio);
    }

    private SpriteRenderer GetCurrentMapRenderer()
    {
        if (_mapObjectArray == null || _currentMapIndex < 0 || _currentMapIndex >= _mapObjectArray.Length)
            return null;

        GameObject currentMapObject = _mapObjectArray[_currentMapIndex];
        return currentMapObject != null ? currentMapObject.GetComponent<SpriteRenderer>() : null;
    }

    private void ResolveCameraReference()
    {
        if (Camera_Main == null)
            Camera_Main = Camera.main;

        if (Camera_Follow == null && Camera_Main != null)
            Camera_Main.TryGetComponent(out Camera_Follow);
    }

    /// <summary>
    /// 카메라 팔로우 대상을 MFC로 바꾸고, 필요하면 즉시 MFC 위치로 스냅합니다.
    /// </summary>
    private void FocusCameraOnMFC(bool isSnapImmediately)
    {
        ResolveCameraReference();

        if (Object_MFC == null)
            return;

        if (Camera_Follow != null)
        {
            Camera_Follow.enabled = true;
            Camera_Follow.SetTarget(Object_MFC.transform);
        }

        if (!isSnapImmediately || Camera_Main == null)
            return;

        Camera_Main.orthographic = true;

        if (Camera_Main.cullingMask == 0)
            Camera_Main.cullingMask = -1;

        Vector3 cameraPosition = Camera_Main.transform.position;
        cameraPosition.x = Object_MFC.transform.position.x;
        cameraPosition.y = Object_MFC.transform.position.y;
        Camera_Main.transform.position = cameraPosition;
    }

    private void SetMFCActive(bool isActive)
    {
        if (Object_MFC != null)
            Object_MFC.SetActive(isActive);
    }

    /// <summary>
    /// 씬에 배치된 RoadMapFadeCanvas와 Image_FadeOverlay를 찾아 페이드 소품으로 연결합니다.
    /// </summary>
    private void CreateFadeOverlayIfNeeded()
    {
        if (_fadeCanvas != null)
            return;

        GameObject canvasObject = FindChildByName(transform, "RoadMapFadeCanvas");

        if (canvasObject == null)
        {
            Debug.LogWarning($"[OOTechRoadToStage1Controller] {gameObject.name} needs RoadMapFadeCanvas as a child object.");
            return;
        }

        _fadeCanvas = canvasObject.GetComponent<Canvas>();

        if (_fadeCanvas == null)
        {
            Debug.LogWarning($"[OOTechRoadToStage1Controller] RoadMapFadeCanvas needs Canvas: {gameObject.name}");
            return;
        }

        _fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _fadeCanvas.sortingOrder = 5000;

        GameObject imageObject = FindChildByName(canvasObject.transform, "Image_FadeOverlay");

        if (imageObject == null)
        {
            Debug.LogWarning($"[OOTechRoadToStage1Controller] RoadMapFadeCanvas needs Image_FadeOverlay: {gameObject.name}");
            return;
        }

        Image_FadeOverlay = imageObject.GetComponent<Image>();

        if (Image_FadeOverlay == null)
        {
            Debug.LogWarning($"[OOTechRoadToStage1Controller] Image_FadeOverlay needs Image: {gameObject.name}");
            return;
        }

        Image_FadeOverlay.raycastTarget = false;
        SetFadeOverlayAlpha(0f);
    }

    /// <summary>
    /// 검은 페이드 오버레이의 알파를 서서히 바꿔 맵 전환을 연출합니다.
    /// </summary>
    private IEnumerator FadeOverlayRoutine(float fromAlpha, float toAlpha, float duration)
    {
        CreateFadeOverlayIfNeeded();

        if (_fadeCanvas == null || Image_FadeOverlay == null)
            yield break;

        _fadeCanvas.gameObject.SetActive(true);

        if (duration <= 0f)
        {
            SetFadeOverlayAlpha(toAlpha);
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            SetFadeOverlayAlpha(Mathf.Lerp(fromAlpha, toAlpha, elapsedTime / duration));
            yield return null;
        }

        SetFadeOverlayAlpha(toAlpha);

        if (toAlpha <= 0f)
            _fadeCanvas.gameObject.SetActive(false);
    }

    private void SetFadeOverlayAlpha(float alpha)
    {
        if (Image_FadeOverlay == null)
            return;

        Color color = _fadeColor;
        color.a = Mathf.Clamp01(alpha);
        Image_FadeOverlay.color = color;
    }

    private void HideFadeOverlay()
    {
        if (Image_FadeOverlay != null)
            SetFadeOverlayAlpha(0f);

        if (_fadeCanvas != null)
            _fadeCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// 현재 그룹의 공용 HUD 컨트롤러를 찾습니다.
    /// </summary>
    private void CacheHUDReference()
    {
        if (HUD_Road == null)
            HUD_Road = GetComponent<OOTechRoadHUDController>();

        if (HUD_Road == null)
            Debug.LogWarning($"[OOTechRoadToStage1Controller] {gameObject.name} needs OOTechRoadHUDController attached in the scene.");
    }

    /// <summary>
    /// Tutorial2Controller를 찾아 HUD 소개와 RoadMap1 도착 안내를 맡깁니다.
    /// </summary>
    private void CacheTutorial2Reference()
    {
        if (Tutorial2_Controller == null)
            Tutorial2_Controller = GetComponent<OOTechTutorial2Controller>();

        if (Tutorial2_Controller == null)
            Debug.LogWarning($"[OOTechRoadToStage1Controller] {gameObject.name} needs OOTechTutorial2Controller attached in the scene.");
    }

    /// <summary>
    /// 로드 화면용 HUD를 준비하고 현재 그룹 규칙에 맞게 요리 버튼 잠금을 적용합니다.
    /// </summary>
    private void PrepareRoadHUD()
    {
        if (HUD_Road == null)
            return;

        HUD_Road.SetOwnerGroupName(_currentGroupName);
        HUD_Road.PrepareHUD();
        HUD_Road.SetCookingUnlocked(_currentGroupName != "1st_Road_to_Stage1");
        HUD_Road.SetHUDVisible(true);
    }

    private void SetRoadHUDVisible(bool isVisible)
    {
        if (HUD_Road != null)
            HUD_Road.SetHUDVisible(isVisible);
    }

    /// <summary>
    /// RoadGroup 진입 시 이전 무대의 전체 화면 UI가 남아 카메라와 버튼을 막지 않도록 정리합니다.
    /// </summary>
    private void CloseBlockingSceneGroups()
    {
        foreach (string groupName in _blockingGroupNameArray)
        {
            if (string.IsNullOrEmpty(groupName) || groupName == _currentGroupName)
                continue;

            GameObject groupObject = FindSceneObjectByName(groupName);

            if (groupObject == null || !groupObject.activeSelf)
                continue;

            if (OOTechUIManager.Inst != null)
            {
                OOTechUIManager.Inst.RegisterUI(groupName, groupObject);
                OOTechUIManager.Inst.CloseUI(groupName);
            }
            else
            {
                groupObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 1st_Road_to_Stage1 진입 시 HUD 소개 튜토리얼을 시작합니다.
    /// </summary>
    private void StartOpeningTutorialIfNeeded()
    {
        StopOpeningTutorial();

        if (Tutorial2_Controller == null || HUD_Road == null)
            return;

        _openingTutorialCoroutine = StartCoroutine(Tutorial2_Controller.PlayOpeningTutorialRoutine(HUD_Road, _currentGroupName));
    }

    private void StopOpeningTutorial()
    {
        if (_openingTutorialCoroutine != null)
        {
            StopCoroutine(_openingTutorialCoroutine);
            _openingTutorialCoroutine = null;
        }

        if (Tutorial2_Controller != null)
            Tutorial2_Controller.StopTutorial(HUD_Road);
    }

    /// <summary>
    /// RoadMap1 이후에는 요리하기 HUD를 사용할 수 있게 엽니다.
    /// </summary>
    private void UnlockCookingIfNeeded()
    {
        if (HUD_Road == null)
            return;

        if (_currentGroupName == "1st_Road_to_Stage1" && _currentMapIndex >= 1)
            HUD_Road.SetCookingUnlocked(true);
    }

    /// <summary>
    /// 첫 번째 RoadMap1에 도착했을 때만 요리 안내 대화/튜토리얼을 재생합니다.
    /// </summary>
    private IEnumerator PlayRoadMapArrivalCueIfNeeded()
    {
        if (_isRoadMap1ArrivalCuePlayed)
            yield break;

        if (_currentGroupName != "1st_Road_to_Stage1" || _currentMapIndex != 1)
            yield break;

        if (Tutorial2_Controller == null)
            yield break;

        _isRoadMap1ArrivalCuePlayed = true;
        yield return Tutorial2_Controller.PlayRoadMap1ArrivalRoutine();
    }

    /// <summary>
    /// 로드 마지막 맵 끝에 도달하면 목표 StageGroup만 켜고 나머지 StageGroup은 끕니다.
    /// </summary>
    private void OpenTargetStageGroup()
    {
        if (_stageGroupNameArray != null)
        {
            foreach (string stageGroupName in _stageGroupNameArray)
            {
                if (string.IsNullOrEmpty(stageGroupName))
                    continue;

                SetSceneGroupActive(stageGroupName, stageGroupName == _targetStageGroupName);
            }
        }

        SetSceneGroupActive(_targetStageGroupName, true);
    }

    private bool SetSceneGroupActive(string groupName, bool isActive)
    {
        if (string.IsNullOrEmpty(groupName))
            return false;

        GameObject groupObject = FindSceneObjectByName(groupName);

        if (OOTechUIManager.Inst != null && groupObject != null)
        {
            OOTechUIManager.Inst.RegisterUI(groupName, groupObject);
            bool isChangedByUIManager = isActive ? OOTechUIManager.Inst.OpenUI(groupName) : OOTechUIManager.Inst.CloseUI(groupName);

            if (isChangedByUIManager)
                return true;
        }

        if (groupObject == null)
        {
            if (isActive)
                Debug.LogWarning($"[OOTechRoadToStage1Controller] Scene group not found: {groupName}");

            return false;
        }

        groupObject.SetActive(isActive);
        return true;
    }

    private GameObject FindSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return null;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = FindChildByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private GameObject FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = FindChildByName(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private GameObject ResolveChild(GameObject assignedObject, params string[] childNameArray)
    {
        if (assignedObject != null)
            return assignedObject;

        foreach (string childName in childNameArray)
        {
            Transform childTransform = transform.Find(childName);

            if (childTransform != null)
                return childTransform.gameObject;
        }

        return null;
    }

    private Transform ResolveChildTransform(Transform assignedTransform, params string[] childNameArray)
    {
        if (assignedTransform != null)
            return assignedTransform;

        GameObject childObject = ResolveChild(null, childNameArray);
        return childObject != null ? childObject.transform : null;
    }

    private T ResolveComponent<T>(T assignedComponent, GameObject targetObject) where T : Component
    {
        if (assignedComponent != null)
            return assignedComponent;

        return targetObject != null ? targetObject.GetComponent<T>() : null;
    }
}
