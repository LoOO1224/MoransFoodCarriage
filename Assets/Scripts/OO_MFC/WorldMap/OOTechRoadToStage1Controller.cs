// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechRoadToStage1Controller.cs
// - 역할: 로드맵, 월드맵, 스테이지 전환 흐름을 담당하는 장면 Controller입니다.
// - 감독 관점: 길 위의 장면 전환 큐시트를 들고 있는 무대감독입니다.
// - 유지보수 포인트: 배경/버튼/캐릭터 배치는 오브젝트와 View가 맡고, 이 스크립트는 순서 지휘만 맡아야 합니다.
// =============================================================================
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
    private const string _roleWormhole = "Wormhole";
    private const string _firstRoadGroupName = "1st_Road_to_Stage1";
    private const string _thirdRoadGroupName = "3rd_Road_to_Stage3";
    private const string _fourthRoadGroupName = "4th_Road_to_Stage4";
    private const string _finalRoadGroupName = "Final_Road_to_FinalStage";
    private const string _stage1GroupName = "Stage1Group";
    private const string _stage2GroupName = "Stage2Group";
    private const string _stage3GroupName = "Stage3Group";
    private const string _stage4GroupName = "Stage4Group";
    private const string _stage4FirstGroupName = "Stage4_1Group";
    private const string _finalStageGroupName = "FinalStageGroup";

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
    [HideInInspector]
    [SerializeField] private GameObject Object_Wormhole;

    [Header("Road Lane")]
    [SerializeField] private KeyCode _moveRightKey = KeyCode.D;
    [SerializeField] private KeyCode _alternateMoveRightKey = KeyCode.RightArrow;
    [SerializeField] private float _moveSpeed = 4.2f;
    [SerializeField, Range(0f, 1f)] private float _roadLaneNormalizedHeight = 0.23f;
    [SerializeField, Range(0f, 0.45f)] private float _entryMarginRatio = 0.1f;
    [SerializeField, Range(0f, 0.45f)] private float _exitMarginRatio = 0.08f;
    [SerializeField] private bool _isUseMFCStartPointOnFirstMap = true;

    [Header("MFC Animation")]
    [SerializeField] private string _mfcWalkStateName = "MFC_isWalking";
    [SerializeField] private string _mfcLegacyWalkStateName = "MFC";
    [SerializeField] private bool _isAnimateMFCOnlyWhileMoving = true;
    [SerializeField] private float _mfcWalkAnimationSpeed = 0.85f;
    [SerializeField] private int _mfcSortingOrder = 100;

    [Header("Build Input Fail Safe")]
    [SerializeField] private float _blockedInputHoldSeconds = 1.2f;
    [SerializeField] private bool _isUseBuildAutoMoveFailSafe = true;
    [SerializeField] private float _buildAutoMoveDelaySeconds = 1.5f;

    [Header("Map Transition")]
    [SerializeField] private float _fadeOutSeconds = 0.45f;
    [SerializeField] private float _blackoutHoldSeconds = 0.15f;
    [SerializeField] private float _fadeInSeconds = 0.55f;
    [SerializeField] private Color _fadeColor = Color.black;

    [Header("Destination")]
    [SerializeField] private string _currentGroupName = "1st_Road_to_Stage1";
    [SerializeField] private string _thirdRoadName = "3rd_Road_to_Stage3";
    [SerializeField] private string _thirdRoadTargetStageName = "Stage3Group";
    [SerializeField] private string _targetStageGroupName = "Stage1Group";
    [SerializeField] private string[] _stageGroupNameArray =
    {
        "Stage1Group",
        "Stage2Group",
        "Stage3Group",
        "Stage4_1Group",
        "Stage4Group",
        "FinalStageGroup"
    };

    [Header("Stage4 BGM")]
    [SerializeField] private AudioClip _stage4BGM;
    [SerializeField] private string _stage4BGMAssetPath = "Assets/Sounds/BGM/Stage4_BGM.mp3";

    [Header("Road Opening Dialogue")]
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private string _secondRoadGroupName = "2nd_Road_to_Stage2";
    [SerializeField] private string _secondRoadMissionDataId = "Stage2_Road_Quest_01";
    [SerializeField] private string _secondRoadMissionFallbackText = "서쪽 도시에 가 탐관오리의 자택을 방문하세요.";
    [SerializeField] private string[] _secondRoadOpeningDialogueIdArray =
    {
        "character_Chunyang_06",
        "character_Mr.Jaeik_05",
        "character_Chunyang_07",
        "character_Mr.Jaeik_06",
        "character_Moran_06"
    };

    [Header("Stage1 Reward Repair")]
    [SerializeField] private string _stage1PumpkinSoupCookId = "OO_PumpkinSoup_1";
    [SerializeField] private string _stage1ChiliPepperIngredientId = "Ing_ChiliPepper_01";
    [SerializeField] private string _stage1KimchIngredientId = "Ing_Kimch_01";
    [SerializeField] private int _stage1RequiredPumpkinSoupCount = 10;
    [SerializeField] private int _stage1ChiefRewardCount = 10;

    [Header("Wormhole Transition")]
    [SerializeField] private bool _isEnableWormholeEntry = true;
    [SerializeField] private float _wormholeHitHoldSeconds = 0.2f;
    [SerializeField] private float _wormholeMapProgress = 0.5f;
    [SerializeField] private Color _wormholeEncounterFlashColor = Color.white;
    [SerializeField] private int _wormholeEncounterFlashCount = 4;
    [SerializeField] private float _wormholeEncounterFlashSeconds = 0.08f;

    private GameObject[] _mapObjectArray;
    private int _currentMapIndex;
    private bool _isChangingMap;
    private bool _isRoadTripComplete;
    private bool _isRoadMap1ArrivalCuePlayed;
    private bool _isOpeningDialoguePlaying;
    private bool _isWormholeTriggered;
    private bool _isWormholeTransitionInProgress;
    private float _wormholeHoldSeconds;
    private float _blockedInputPressedSeconds;
    private float _buildAutoMoveReadySeconds;
    private bool _isBlockedInputFailSafeLogged;
    private bool _isBuildAutoMoveLogged;
    private bool _isBuildAutoMoveLockCleared;
    private Coroutine _openingTutorialCoroutine;
    private Coroutine _openingDialogueCoroutine;
    private Canvas _fadeCanvas;
    private Image Image_FadeOverlay;
    private DialogueUI UI_Dialogue;

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
        "Stage4_1Group",
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
        ResolveRoadIdentityFromGroupName();
        ResolveSceneReferences();
        ResolveCameraReference();
        CacheHUDReference();
        CacheTutorial2Reference();
        CloseBlockingSceneGroups();
        PrepareRoadHUD();
        RepairStage1RewardInventoryIfNeeded();
        PrepareRoadTrip();
        StartOpeningTutorialIfNeeded();
        StartRoadOpeningDialogueIfNeeded();
    }

    /// <summary>
    /// Stage1 완료 보상이 빠진 상태로 2nd_Road에 들어온 경우 인벤토리를 한 번 보정합니다.
    /// 영화로 치면 이전 장면에서 소품 교환 큐가 누락됐을 때, 다음 무대 입구에서 소품 담당이 빠르게 정산하는 안전 큐입니다.
    /// </summary>
    private void RepairStage1RewardInventoryIfNeeded()
    {
        bool isSecondRoadGroup = _currentGroupName == _secondRoadGroupName || gameObject.name == _secondRoadGroupName;

        if (!isSecondRoadGroup || OOTechGameManager.Inst == null)
            return;

        if (OOTechGameManager.Inst.GetItemCount(_stage1PumpkinSoupCookId) < _stage1RequiredPumpkinSoupCount)
            return;

        if (!OOTechGameManager.Inst.RemoveItem(_stage1PumpkinSoupCookId, _stage1RequiredPumpkinSoupCount))
            return;

        AddInventoryItemToTargetCount(_stage1ChiliPepperIngredientId, _stage1ChiefRewardCount);
        AddInventoryItemToTargetCount(_stage1KimchIngredientId, _stage1ChiefRewardCount);

        if (HUD_Road != null)
        {
            HUD_Road.RequestRefreshInventoryView();
            HUD_Road.SetInventoryNewBadgeActive(true);
        }

        Debug.Log("[OOTechRoadToStage1Controller] Stage1 reward inventory was repaired on 2nd_Road_to_Stage2 entry.");
    }

    private void AddInventoryItemToTargetCount(string itemDataId, int targetCount)
    {
        if (OOTechGameManager.Inst == null || string.IsNullOrEmpty(itemDataId))
            return;

        int safeTargetCount = Mathf.Max(1, targetCount);
        int currentCount = OOTechGameManager.Inst.GetItemCount(itemDataId);
        int addCount = safeTargetCount - currentCount;

        if (addCount <= 0)
            return;

        OOTechGameManager.Inst.AddItem(itemDataId, addCount);
    }

    /// <summary>
    /// RoadGroup이 닫히면 코루틴, 애니메이션, HUD, 페이드 상태를 정리합니다.
    /// </summary>
    private void OnDisable()
    {
        StopOpeningTutorial();
        StopRoadOpeningDialogue();
        StopAllCoroutines();
        _openingTutorialCoroutine = null;
        _openingDialogueCoroutine = null;
        _isOpeningDialoguePlaying = false;
        _isWormholeTriggered = false;
        _isWormholeTransitionInProgress = false;
        _wormholeHoldSeconds = 0f;
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
        bool isMoveRightPressed = IsMoveRightPressed();
        bool isAutoMoveRequested = IsBuildAutoMoveRequested(isMoveRightPressed);

        if (isMoveRightPressed)
        {
            RequestRecoverRoadRuntimeStateForMovement();
            RequestUnlockBlockedRoadInputIfNeeded();
        }
        else
        {
            _blockedInputPressedSeconds = 0f;
            _isBlockedInputFailSafeLogged = false;
        }

        if (isAutoMoveRequested)
        {
            RequestRecoverRoadRuntimeStateForMovement();

            if (!_isBuildAutoMoveLockCleared)
            {
                RequestClearBlockedRoadInputState("Build auto-move fail-safe cleared stale road lock.");
                _isBuildAutoMoveLockCleared = true;
            }
        }

        if (IsRoadInputBlocked())
        {
            SetMFCAnimationPlaying(false);
            return;
        }

        if (_isChangingMap || _isRoadTripComplete || Object_MFC == null)
        {
            SetMFCAnimationPlaying(false);
            return;
        }

        if (ShouldUseWormholeTransition() && !_isWormholeTriggered && IsMFCAtWormholeCuePoint())
        {
            _wormholeHoldSeconds += Time.unscaledDeltaTime;

            if (_wormholeHoldSeconds >= _wormholeHitHoldSeconds)
                TriggerWormholeTransition();
            else
                SetMFCAnimationPlaying(false);

            return;
        }

        if (ShouldUseWormholeTransition() && !_isWormholeTriggered)
            _wormholeHoldSeconds = 0f;

        if (isMoveRightPressed || isAutoMoveRequested)
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
    /// D키와 오른쪽 방향키를 함께 받습니다.
    /// 빌드 환경에서 키보드 레이아웃이나 포커스 차이로 한쪽 입력이 흔들려도 Road 배우가 움직일 수 있게 하는 보험입니다.
    /// </summary>
    private bool IsMoveRightPressed()
    {
        return IsLegacyMoveRightPressed() || IsNewInputMoveRightPressed();
    }

    /// <summary>
    /// 기존 Input Manager 방식으로 D/오른쪽 방향키를 읽습니다.
    /// 빌드 설정이 New Input 전용으로 바뀐 경우 예외가 날 수 있어 안전하게 감쌉니다.
    /// </summary>
    private bool IsLegacyMoveRightPressed()
    {
        try
        {
            return Input.GetKey(_moveRightKey) ||
                   Input.GetKey(_alternateMoveRightKey) ||
                   Input.GetKey(KeyCode.D) ||
                   Input.GetKey(KeyCode.RightArrow) ||
                   Input.GetAxisRaw("Horizontal") > 0.1f;
        }
        catch (System.InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>
    /// Unity New Input System 방식으로 D/오른쪽 방향키를 읽습니다.
    /// 빌드에서 Legacy Input이 흔들릴 때도 키보드 상태를 직접 확인하기 위한 보험입니다.
    /// </summary>
    private bool IsNewInputMoveRightPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return false;

        return keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
#else
        return false;
#endif
    }

    /// <summary>
    /// 튜토리얼, 오프닝 대사, HUD 오버레이가 켜져 있으면 원래는 MFC 조작을 잠급니다.
    /// 빌드에서 이 값이 풀리지 않으면 D키가 죽은 것처럼 보이므로 한 곳에서 원인을 판정합니다.
    /// </summary>
    private bool IsRoadInputBlocked()
    {
        return IsNarrativeInputBlocked() ||
               HUD_Road != null && HUD_Road.IsOverlayOpen;
    }

    private bool IsNarrativeInputBlocked()
    {
        return Tutorial2_Controller != null && Tutorial2_Controller.IsTutorialRunning ||
               _isOpeningDialoguePlaying;
    }

    /// <summary>
    /// 빌드에서 키 입력이 잡히지 않는 최악의 경우에도 Road 장면이 멈추지 않게 하는 자동 이동 보험입니다.
    /// 영화로 보면 배우가 큐 사인을 못 들었을 때 조감독이 정해진 동선대로 밀어 주는 마지막 안전 큐입니다.
    /// </summary>
    private bool IsBuildAutoMoveRequested(bool isMoveRightPressed)
    {
        if (!_isUseBuildAutoMoveFailSafe || Application.isEditor)
            return false;

        if (isMoveRightPressed || _isChangingMap || _isRoadTripComplete || Object_MFC == null)
        {
            _buildAutoMoveReadySeconds = 0f;
            _isBuildAutoMoveLogged = false;
            _isBuildAutoMoveLockCleared = false;
            return false;
        }

        _buildAutoMoveReadySeconds += Time.unscaledDeltaTime;

        if (_buildAutoMoveReadySeconds < _buildAutoMoveDelaySeconds)
            return false;

        if (!_isBuildAutoMoveLogged)
        {
            Debug.LogWarning("[OOTechRoadToStage1Controller] Build auto-move fail-safe started because road input was not received.");
            _isBuildAutoMoveLogged = true;
        }

        return true;
    }

    /// <summary>
    /// RoadGroup에서 이동 키가 눌렸는데 시간 정지나 오버레이 잔여 상태가 남아 있으면 즉시 복구합니다.
    /// 요리/도감/월드맵에서 돌아온 뒤에도 배우가 다시 움직일 수 있게 만드는 공통 안전장치입니다.
    /// </summary>
    private void RequestRecoverRoadRuntimeStateForMovement()
    {
        if (Object_MFC == null || _mapObjectArray == null || _mapObjectArray.Length == 0)
            ResolveSceneReferences();

        if (Time.timeScale <= 0f)
        {
            Time.timeScale = 1f;
            Debug.LogWarning("[OOTechRoadToStage1Controller] Time.timeScale was 0 while road move key was pressed. Restored timeScale to 1.");
        }

        if (IsNarrativeInputBlocked())
            return;

        if (HUD_Road != null && HUD_Road.IsOverlayOpen)
            HUD_Road.RequestRestoreFromOverlayReturn();

        if (Object_MFC != null && !Object_MFC.activeSelf)
            Object_MFC.SetActive(true);

        EnsureCurrentMapVisible();
    }

    /// <summary>
    /// 빌드에서 RoadMap1 도착 가이드나 오버레이 복귀가 끊기면 이동 입력이 계속 막힐 수 있습니다.
    /// 플레이어가 D/오른쪽키를 계속 누르고 있으면 남은 잠금 큐를 정리해 MFC 배우가 다시 이동하게 합니다.
    /// </summary>
    private void RequestUnlockBlockedRoadInputIfNeeded()
    {
        if (!CanUseBlockedInputFailSafe())
            return;

        if (!IsRoadInputBlocked() && !_isChangingMap)
        {
            _blockedInputPressedSeconds = 0f;
            _isBlockedInputFailSafeLogged = false;
            return;
        }

        _blockedInputPressedSeconds += Time.unscaledDeltaTime;

        if (_blockedInputPressedSeconds < _blockedInputHoldSeconds)
            return;

        RequestClearBlockedRoadInputState("Road input was blocked after map transition. Cleared leftover tutorial/dialogue/overlay lock.");

        if (!_isBlockedInputFailSafeLogged)
        {
            _isBlockedInputFailSafeLogged = true;
        }

        _blockedInputPressedSeconds = 0f;
    }

    /// <summary>
    /// 이동을 막는 남은 튜토리얼, 대사, HUD 오버레이, 시간 정지 상태를 한 번에 정리합니다.
    /// 플레이어가 D를 눌렀는데 무대 큐가 닫히지 않은 상황을 복구하는 공통 안전장치입니다.
    /// </summary>
    private void RequestClearBlockedRoadInputState(string reason)
    {
        StopOpeningTutorial();
        StopRoadOpeningDialogue();

        if (HUD_Road != null && HUD_Road.IsOverlayOpen)
            HUD_Road.RequestRestoreFromOverlayReturn();

        _isChangingMap = false;
        _isOpeningDialoguePlaying = false;
        Time.timeScale = 1f;
        HideFadeOverlay();
        EnsureCurrentMapVisible();
        EnsureMFCVisible();

        Debug.LogWarning($"[OOTechRoadToStage1Controller] {reason}");
    }

    /// <summary>
    /// RoadGroup이 끝난 뒤에는 입력 복구가 필요 없지만, 그 전에는 첫 맵도 포함해서 복구를 허용합니다.
    /// 빌드에서 첫 튜토리얼 패널이 보이지 않고 잠금만 남는 경우가 있어서 첫 맵 제한을 두지 않습니다.
    /// </summary>
    private bool CanUseBlockedInputFailSafe()
    {
        return !_isRoadTripComplete;
    }

    /// <summary>
    /// MFC 배우와 맵 배경 오브젝트를 씬 이름 또는 역할표로 찾습니다.
    /// 인스펙터 참조가 있으면 그 값을 우선 사용해 나중에 배경만 교체하기 쉽게 둡니다.
    /// </summary>
    public void ResolveSceneReferences()
    {
        CacheSceneContextReference();

        Object_MFC = ResolveOwnedChild(Object_MFC, "MFC");
        Animator_MFC = ResolveOwnedComponent<Animator>(Object_MFC);
        Renderer_MFC = ResolveOwnedComponent<SpriteRenderer>(Object_MFC);

        Object_StartPointMap = ResolveChild(Object_StartPointMap, "StartPointMap");
        Object_RoadMap1 = ResolveChild(Object_RoadMap1, "RoadMap1", "RoadMap");
        Object_RoadMap2 = ResolveChild(Object_RoadMap2, "RoadMap2", "RoadMap3");
        Object_Stage1EntryMap = ResolveChild(Object_Stage1EntryMap, "Stage1EntryMap", "Stage2EntryMap", "Stage3EntryMap", "Stage4EntryMap", "FinalStageEntryMap", "Stage1_Entry", "Stage2_Entry");
        Object_Wormhole = ResolveChild(Object_Wormhole, _roleWormhole, "WormholeEntry", "Wormhole_Area");
        Transform_MFCStartPoint = ResolveChildTransform(Transform_MFCStartPoint, "MFC_StartPoint", "StartPoint_MFC");

        _mapObjectArray = CreateMapObjectArrayForCurrentRoad();
    }

    private void ResolveRoadIdentityFromGroupName()
    {
        _currentGroupName = ResolveRoadGroupNameByObjectName();
        _targetStageGroupName = ResolveTargetStageGroupName();

        if (HUD_Road != null)
            HUD_Road.SetOwnerGroupName(_currentGroupName);

        _isWormholeTriggered = false;
        _isWormholeTransitionInProgress = false;
        _wormholeHoldSeconds = 0f;
    }

    private string ResolveRoadGroupNameByObjectName()
    {
        string objectName = gameObject != null ? gameObject.name : string.Empty;

        if (!string.IsNullOrWhiteSpace(objectName))
        {
            if (objectName.Contains("1st") || objectName.Contains("First"))
                return _firstRoadGroupName;

            if (objectName.Contains("2nd"))
                return _secondRoadGroupName;

            if (objectName.Contains("3rd"))
                return string.IsNullOrWhiteSpace(_thirdRoadName) ? _thirdRoadName : _thirdRoadName;

            if (objectName.Contains("4th"))
                return _fourthRoadGroupName;

            if (objectName.Contains("Final"))
                return _finalRoadGroupName;
        }

        if (!string.IsNullOrWhiteSpace(_currentGroupName))
        {
            string normalizedCurrentGroupName = _currentGroupName.Trim();

            if (normalizedCurrentGroupName == _firstRoadGroupName ||
                normalizedCurrentGroupName == _secondRoadGroupName ||
                normalizedCurrentGroupName == _thirdRoadGroupName ||
                normalizedCurrentGroupName == _thirdRoadName ||
                normalizedCurrentGroupName == _fourthRoadGroupName ||
                normalizedCurrentGroupName == _finalRoadGroupName)
            {
                return normalizedCurrentGroupName;
            }
        }

        if (string.IsNullOrWhiteSpace(objectName))
            return _firstRoadGroupName;

        if (objectName.Contains("3rd"))
            return string.IsNullOrWhiteSpace(_thirdRoadName) ? _thirdRoadGroupName : _thirdRoadName;

        if (objectName.Contains("2nd"))
            return _secondRoadGroupName;

        if (objectName.Contains("4th"))
            return _fourthRoadGroupName;

        if (objectName.Contains("Final"))
            return _finalRoadGroupName;

        return _firstRoadGroupName;
    }

    private string ResolveTargetStageGroupName()
    {
        if (IsThirdRoadCurrent())
            return string.IsNullOrWhiteSpace(_thirdRoadTargetStageName) ? _stage3GroupName : _thirdRoadTargetStageName;

        if (IsFourthRoadCurrent())
            return _stage4FirstGroupName;

        return _targetStageGroupName;
    }

    /// <summary>
    /// 4th Road는 StartPointMap 다음에 곧바로 Stage4 진입맵으로 넘어가는 짧은 길입니다.
    /// 같은 Road 감독을 쓰되, 공연 큐시트만 두 장짜리로 바꿔 끼우는 방식입니다.
    /// </summary>
    private GameObject[] CreateMapObjectArrayForCurrentRoad()
    {
        if (IsFourthRoadCurrent())
            return CreateValidMapObjectArray(Object_StartPointMap, Object_Stage1EntryMap);

        return CreateValidMapObjectArray(Object_StartPointMap, Object_RoadMap1, Object_RoadMap2, Object_Stage1EntryMap);
    }

    /// <summary>
    /// 실제로 무대에 존재하는 Road 배경만 큐시트 배열에 넣습니다.
    /// 영화 비유로는 공연장에 없는 배경막은 큐시트에서 빼고, 준비된 배경막만 순서대로 넘기는 일입니다.
    /// </summary>
    private GameObject[] CreateValidMapObjectArray(params GameObject[] mapObjectArray)
    {
        List<GameObject> validMapList = new List<GameObject>();

        foreach (GameObject mapObject in mapObjectArray)
        {
            if (mapObject == null || validMapList.Contains(mapObject))
                continue;

            validMapList.Add(mapObject);
        }

        return validMapList.ToArray();
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
        FocusCameraOnCurrentMap();
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

        float deltaTime = Time.timeScale > 0f ? Time.deltaTime : Time.unscaledDeltaTime;

        if (deltaTime <= 0f)
            deltaTime = Time.unscaledDeltaTime;

        Vector3 position = Object_MFC.transform.position;
        position.x += _moveSpeed * deltaTime;
        position.y = CalculateRoadLaneY(mapRenderer);
        float exitX = CalculateExitX(mapRenderer);
        position.x = Mathf.Min(position.x, exitX);
        Object_MFC.transform.position = position;

        if (position.x >= exitX)
        {
            if (ShouldUseWormholeTransition() && _currentMapIndex == _mapObjectArray.Length - 1)
            {
                SetMFCAnimationPlaying(false);
                TriggerWormholeTransition();
                return;
            }

            StartCoroutine(ChangeToNextMapRoutine());
        }
    }

    /// <summary>
    /// 화면을 어둡게 했다가 다음 맵 또는 StageGroup으로 전환하는 페이드 루틴입니다.
    /// </summary>
    private IEnumerator ChangeToNextMapRoutine()
    {
        if (_isChangingMap)
            yield break;

        if (ShouldUseWormholeTransition() && _currentMapIndex >= _mapObjectArray.Length - 1)
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
        FocusCameraOnCurrentMap();
        UnlockCookingIfNeeded();

        yield return FadeOverlayRoutine(1f, 0f, _fadeInSeconds);
        yield return PlayRoadMapArrivalCueIfNeeded();
        _isChangingMap = false;
    }

    private bool IsThirdRoadCurrent()
    {
        if (string.IsNullOrWhiteSpace(_currentGroupName))
            return false;

        if (_currentGroupName == _thirdRoadGroupName || _currentGroupName == _thirdRoadName)
            return true;

        if (_currentGroupName.Contains("3rd"))
            return true;

        if (gameObject == null || string.IsNullOrWhiteSpace(gameObject.name))
            return false;

        return gameObject.name.Contains("3rd");
    }

    private bool IsFourthRoadCurrent()
    {
        if (!string.IsNullOrWhiteSpace(_currentGroupName) && _currentGroupName == _fourthRoadGroupName)
            return true;

        if (gameObject == null || string.IsNullOrWhiteSpace(gameObject.name))
            return false;

        return gameObject.name.Contains("4th");
    }

    private bool ShouldUseWormholeTransition()
    {
        return _isEnableWormholeEntry && IsThirdRoadCurrent();
    }

    /// <summary>
    /// 3rd Road에서는 별도 Wormhole 소품에 의존하지 않고 마지막 맵의 중간 큐 지점에서 전환을 시작합니다.
    /// 영화로 치면 배우가 특정 소품을 밟아야만 컷이 나는 방식이 아니라, 무대 중앙 표시선에 들어오면 조명 전환 큐가 나가는 방식입니다.
    /// </summary>
    private bool IsMFCAtWormholeCuePoint()
    {
        if (Object_MFC == null || _mapObjectArray == null || _mapObjectArray.Length == 0)
            return false;

        if (_currentMapIndex != _mapObjectArray.Length - 1)
            return false;

        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (mapRenderer == null)
            return false;

        float leftX = mapRenderer.bounds.min.x;
        float rightX = mapRenderer.bounds.max.x;
        float safeProgress = Mathf.Clamp01(_wormholeMapProgress);
        float cueX = Mathf.Lerp(leftX, rightX, safeProgress);

        return Object_MFC.transform.position.x >= cueX;
    }

    private void TriggerWormholeTransition()
    {
        if (_isWormholeTriggered || _isWormholeTransitionInProgress)
            return;

        _isWormholeTriggered = true;
        _isWormholeTransitionInProgress = true;
        _wormholeHoldSeconds = 0f;
        StartCoroutine(WormholeTransitionRoutine());
    }

    private IEnumerator WormholeTransitionRoutine()
    {
        _isChangingMap = true;
        _isRoadTripComplete = true;
        SetMFCAnimationPlaying(false);

        yield return PlayWormholeEncounterEffectRoutine();
        yield return FadeOverlayRoutine(0f, 1f, _fadeOutSeconds);

        if (_blackoutHoldSeconds > 0f)
            yield return new WaitForSeconds(_blackoutHoldSeconds);

        OpenTargetStageGroup();

        yield return FadeOverlayRoutine(1f, 0f, _fadeInSeconds);
        _isChangingMap = false;
        _isWormholeTransitionInProgress = false;
        SetSceneGroupActive(_currentGroupName, false);
        SetSceneGroupActive(gameObject != null ? gameObject.name : null, false);

        Debug.Log("[OOTechRoadToStage1Controller] Wormhole transition completed.");
    }

    /// <summary>
    /// Wormhole에 닿았을 때 강제 인카운터처럼 화면을 짧게 번쩍이며 다음 무대 진입을 알립니다.
    /// 새 UI를 만들지 않고, 씬에 배치된 FadeOverlay 배우를 잠깐 다른 색 조명처럼 사용합니다.
    /// </summary>
    private IEnumerator PlayWormholeEncounterEffectRoutine()
    {
        CreateFadeOverlayIfNeeded();

        if (_fadeCanvas == null || Image_FadeOverlay == null)
            yield break;

        _fadeCanvas.gameObject.SetActive(true);

        int safeFlashCount = Mathf.Max(1, _wormholeEncounterFlashCount);
        float safeFlashSeconds = Mathf.Max(0.01f, _wormholeEncounterFlashSeconds);

        for (int i = 0; i < safeFlashCount; i++)
        {
            SetFadeOverlayColorAlpha(_wormholeEncounterFlashColor, 0.85f);
            yield return new WaitForSeconds(safeFlashSeconds);
            SetFadeOverlayColorAlpha(_wormholeEncounterFlashColor, 0f);
            yield return new WaitForSeconds(safeFlashSeconds);
        }
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
        Renderer_MFC.sortingOrder = Mathf.Max(Renderer_MFC.sortingOrder, _mfcSortingOrder);

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
        PlayMFCWalkState(0f);
        Animator_MFC.Update(0f);
        SetMFCAnimationPlaying(false);
    }

    /// <summary>
    /// MFC 걷기 상태를 애니메이터에서 찾아 재생합니다.
    /// 새 상태명 MFC_isWalking을 우선 쓰고, 아직 이전 컨트롤러라면 MFC 상태로 되돌아갑니다.
    /// </summary>
    private void PlayMFCWalkState(float normalizedTime)
    {
        if (Animator_MFC == null)
            return;

        int layerIndex = 0;
        int stateHash = Animator.StringToHash(_mfcWalkStateName);

        if (Animator_MFC.HasState(layerIndex, stateHash))
        {
            Animator_MFC.Play(stateHash, layerIndex, normalizedTime);
            return;
        }

        int legacyStateHash = Animator.StringToHash(_mfcLegacyWalkStateName);

        if (Animator_MFC.HasState(layerIndex, legacyStateHash))
        {
            Animator_MFC.Play(legacyStateHash, layerIndex, normalizedTime);
            return;
        }

        Debug.LogWarning($"[OOTechRoadToStage1Controller] MFC walk animation state is missing: {_mfcWalkStateName}, {_mfcLegacyWalkStateName}");
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
        SpriteRenderer currentRenderer = currentMapObject != null ? currentMapObject.GetComponent<SpriteRenderer>() : null;

        if (currentRenderer != null && currentRenderer.enabled && currentMapObject.activeInHierarchy)
            return currentRenderer;

        return GetVisibleMapRenderer();
    }

    /// <summary>
    /// 내부 인덱스가 틀어졌을 때 실제 켜져 있는 RoadMap Renderer를 찾아 이동 기준으로 사용합니다.
    /// 감독의 큐시트 번호가 밀려도, 플레이어가 보고 있는 배경 위에서 MFC가 계속 움직이게 하는 마지막 안전망입니다.
    /// </summary>
    private SpriteRenderer GetVisibleMapRenderer()
    {
        if (_mapObjectArray == null)
            return null;

        for (int index = 0; index < _mapObjectArray.Length; index++)
        {
            GameObject mapObject = _mapObjectArray[index];

            if (mapObject == null || !mapObject.activeInHierarchy)
                continue;

            SpriteRenderer mapRenderer = mapObject.GetComponent<SpriteRenderer>();

            if (mapRenderer == null || !mapRenderer.enabled)
                continue;

            _currentMapIndex = index;
            return mapRenderer;
        }

        return null;
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
    private void FocusCameraOnCurrentMap()
    {
        ResolveCameraReference();

        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (Camera_Main == null || mapRenderer == null)
            return;

        if (Camera_Follow != null)
            Camera_Follow.enabled = false;

        Camera_Main.orthographic = true;

        if (Camera_Main.cullingMask == 0)
            Camera_Main.cullingMask = -1;

        Bounds mapBounds = mapRenderer.bounds;
        float verticalSize = mapBounds.extents.y;
        float horizontalSize = mapBounds.extents.x / Mathf.Max(0.01f, Camera_Main.aspect);

        Camera_Main.orthographicSize = Mathf.Max(verticalSize, horizontalSize);

        Vector3 cameraPosition = Camera_Main.transform.position;
        cameraPosition.x = mapBounds.center.x;
        cameraPosition.y = mapBounds.center.y;
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

        SetFadeOverlayColorAlpha(_fadeColor, alpha);
    }

    private void SetFadeOverlayColorAlpha(Color fadeColor, float alpha)
    {
        if (Image_FadeOverlay == null)
            return;

        Color color = fadeColor;
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
        ApplyRoadMissionForCurrentGroup();
        HUD_Road.SetHUDVisible(true);
    }

    /// <summary>
    /// RoadGroup 이름에 맞춰 HUD의 길 안내 임무를 데이터로 교체합니다.
    /// 영화로 치면 이동 장면마다 배우에게 다른 콜시트를 나눠주는 큐입니다.
    /// </summary>
    private void ApplyRoadMissionForCurrentGroup()
    {
        if (HUD_Road == null)
            return;

        if (_currentGroupName != _secondRoadGroupName)
            return;

        string roadMissionText = ResolveStageQuestDescription(_secondRoadMissionDataId, _secondRoadMissionFallbackText);
        HUD_Road.RequestSetRoadMissionText(roadMissionText, true);
    }

    /// <summary>
    /// OO_StageQuest에서 임무 설명을 읽고, 데이터가 아직 비어 있으면 안전 문구를 사용합니다.
    /// </summary>
    private string ResolveStageQuestDescription(string stageQuestDataId, string fallbackText)
    {
        OO_StageQuest questData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStageQuestData(stageQuestDataId) : null;

        if (questData != null && !string.IsNullOrWhiteSpace(questData.Description))
            return questData.Description;

        return fallbackText;
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
        if (IsThirdRoadCurrent())
            return;

        StopOpeningTutorial();

        if (Tutorial2_Controller == null || HUD_Road == null)
            return;

        _openingTutorialCoroutine = StartCoroutine(Tutorial2_Controller.PlayOpeningTutorialRoutine(HUD_Road, _currentGroupName));
    }

    /// <summary>
    /// 2nd Road 진입 직후 대화 큐시트를 재생합니다.
    /// Game View에서는 DialoguePanel이 먼저 뜨고, 모든 대사를 넘긴 뒤에야 MFC 이동 입력이 풀립니다.
    /// </summary>
    private void StartRoadOpeningDialogueIfNeeded()
    {
        StopRoadOpeningDialogue();

        if (_currentGroupName != _secondRoadGroupName)
            return;

        _openingDialogueCoroutine = StartCoroutine(PlayRoadOpeningDialogueRoutine(_secondRoadOpeningDialogueIdArray));
    }

    private IEnumerator PlayRoadOpeningDialogueRoutine(string[] dialogueIdArray)
    {
        _isOpeningDialoguePlaying = true;
        SetMFCAnimationPlaying(false);

        yield return null;

        if (dialogueIdArray == null || dialogueIdArray.Length == 0)
        {
            _isOpeningDialoguePlaying = false;
            yield break;
        }

        if (!TryOpenDialogueGroup())
        {
            _isOpeningDialoguePlaying = false;
            yield break;
        }

        foreach (string dialogueId in dialogueIdArray)
        {
            if (string.IsNullOrEmpty(dialogueId))
                continue;

            OO_Dialogue dialogueData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetDialogueData(dialogueId) : null;

            if (dialogueData == null)
            {
                Debug.LogWarning($"[OOTechRoadToStage1Controller] Dialogue data is missing: {dialogueId}");
                continue;
            }

            yield return ShowDialogueDataAndWait(dialogueData);
        }

        CloseDialogueGroup();
        _isOpeningDialoguePlaying = false;
    }

    private IEnumerator ShowDialogueDataAndWait(OO_Dialogue dialogueData)
    {
        if (UI_Dialogue == null || dialogueData == null)
            yield break;

        bool isDone = false;
        UI_Dialogue.RequestRoadViewLayout();
        UI_Dialogue.ShowDialogue(dialogueData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
    }

    private bool TryOpenDialogueGroup()
    {
        GameObject dialogueGroup = FindSceneObjectByName(_dialogueGroupName);

        if (dialogueGroup == null)
        {
            Debug.LogWarning($"[OOTechRoadToStage1Controller] DialogueGroup is missing: {_dialogueGroupName}");
            UI_Dialogue = null;
            return false;
        }

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.RegisterUI(_dialogueGroupName, dialogueGroup);
            OOTechUIManager.Inst.OpenUI(_dialogueGroupName);
        }
        else
        {
            dialogueGroup.SetActive(true);
        }

        UI_Dialogue = dialogueGroup.GetComponentInChildren<DialogueUI>(true);

        if (UI_Dialogue != null)
            UI_Dialogue.RequestRoadViewLayout();

        return UI_Dialogue != null;
    }

    private void CloseDialogueGroup()
    {
        if (OOTechUIManager.Inst != null && OOTechUIManager.Inst.CloseUI(_dialogueGroupName))
            return;

        GameObject dialogueGroup = FindSceneObjectByName(_dialogueGroupName);

        if (dialogueGroup != null)
            dialogueGroup.SetActive(false);
    }

    private void StopRoadOpeningDialogue()
    {
        if (_openingDialogueCoroutine != null)
        {
            StopCoroutine(_openingDialogueCoroutine);
            _openingDialogueCoroutine = null;
        }

        if (_isOpeningDialoguePlaying)
            CloseDialogueGroup();

        _isOpeningDialoguePlaying = false;
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
        {
            if (Tutorial2_Controller != null)
                Tutorial2_Controller.RequestEnsureStarterIngredients(HUD_Road);

            HUD_Road.SetCookingUnlocked(true);
        }
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
        string targetGroupName = ResolveActualTargetStageGroupName(_targetStageGroupName);

        if (_stageGroupNameArray != null)
        {
            foreach (string stageGroupName in _stageGroupNameArray)
            {
                if (string.IsNullOrEmpty(stageGroupName))
                    continue;

                SetSceneGroupActive(stageGroupName, stageGroupName == targetGroupName);
            }
        }

        SetSceneGroupActive(targetGroupName, true);
        RequestFitTargetStageCamera(targetGroupName);
        RequestPlayTargetStageBGM(targetGroupName);
    }

    private string ResolveActualTargetStageGroupName(string requestedGroupName)
    {
        if (!string.IsNullOrWhiteSpace(requestedGroupName) && FindSceneObjectByName(requestedGroupName) != null)
            return requestedGroupName;

        if (requestedGroupName == _stage4FirstGroupName && FindSceneObjectByName(_stage4GroupName) != null)
        {
            Debug.LogWarning("[OOTechRoadToStage1Controller] Stage4_1Group not found. Falling back to Stage4Group.");
            return _stage4GroupName;
        }

        return requestedGroupName;
    }

    private void RequestPlayTargetStageBGM(string targetGroupName)
    {
        if (targetGroupName != _stage4FirstGroupName && targetGroupName != _stage4GroupName)
            return;

        AudioClip bgmClip = ResolveStage4BGMClip();

        if (bgmClip == null)
        {
            Debug.LogWarning("[OOTechRoadToStage1Controller] Stage4_BGM clip missing. Assign _stage4BGM or keep it under Resources/Audio/BGM.");
            return;
        }

        if (OOTechSoundManager.Inst != null)
            OOTechSoundManager.Inst.PlayBGM(bgmClip, true);
    }

    private AudioClip ResolveStage4BGMClip()
    {
        if (_stage4BGM != null)
            return _stage4BGM;

        AudioClip resourcesClip = Resources.Load<AudioClip>("Audio/BGM/Stage4_BGM");

        if (resourcesClip != null)
            return resourcesClip;

#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(_stage4BGMAssetPath))
            return AssetDatabase.LoadAssetAtPath<AudioClip>(_stage4BGMAssetPath);
#endif

        return null;
    }

    /// <summary>
    /// StageGroup을 열자마자 배경 전체가 Game View에 들어오도록 카메라와 Canvas 기준값을 맞춥니다.
    /// 촬영감독이 새 무대에 들어가자마자 와이드샷으로 프레임을 다시 잡는 작업입니다.
    /// </summary>
    private void RequestFitTargetStageCamera(string targetGroupName)
    {
        GameObject targetGroupObject = FindSceneObjectByName(targetGroupName);

        if (targetGroupObject == null)
            return;

        NormalizeStageCanvasArray(targetGroupObject);
        SpriteRenderer backgroundRenderer = ResolveBestStageBackgroundRenderer(targetGroupObject);

        if (backgroundRenderer == null)
            return;

        ResolveCameraReference();

        if (Camera_Main == null)
            return;

        if (Camera_Follow != null)
            Camera_Follow.enabled = false;

        Camera_Main.orthographic = true;

        if (Camera_Main.cullingMask == 0)
            Camera_Main.cullingMask = -1;

        Bounds backgroundBounds = backgroundRenderer.bounds;
        float verticalSize = backgroundBounds.extents.y;
        float horizontalSize = backgroundBounds.extents.x / Mathf.Max(0.01f, Camera_Main.aspect);

        Camera_Main.orthographicSize = Mathf.Max(verticalSize, horizontalSize);

        Vector3 cameraPosition = Camera_Main.transform.position;
        cameraPosition.x = backgroundBounds.center.x;
        cameraPosition.y = backgroundBounds.center.y;
        Camera_Main.transform.position = cameraPosition;
    }

    private void NormalizeStageCanvasArray(GameObject targetGroupObject)
    {
        CanvasScaler[] canvasScalerArray = targetGroupObject.GetComponentsInChildren<CanvasScaler>(true);

        foreach (CanvasScaler canvasScaler in canvasScalerArray)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;
        }
    }

    private SpriteRenderer ResolveBestStageBackgroundRenderer(GameObject targetGroupObject)
    {
        SpriteRenderer[] rendererArray = targetGroupObject.GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer bestRenderer = null;
        float bestArea = 0f;

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null || !spriteRenderer.enabled)
                continue;

            string objectName = spriteRenderer.gameObject.name;

            if (!objectName.Contains("Background") && !objectName.Contains("Backound"))
                continue;

            float area = spriteRenderer.bounds.size.x * spriteRenderer.bounds.size.y;

            if (area <= bestArea)
                continue;

            bestRenderer = spriteRenderer;
            bestArea = area;
        }

        return bestRenderer;
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

    /// <summary>
    /// 현재 RoadGroup 안에 실제로 배치된 배우를 우선 찾습니다.
    /// 인스펙터 참조가 비어 있거나 예전 MFC를 가리켜도, 무대 안의 새 MFC를 다시 잡기 위한 안전장치입니다.
    /// </summary>
    private GameObject ResolveOwnedChild(GameObject assignedObject, params string[] childNameArray)
    {
        foreach (string childName in childNameArray)
        {
            Transform childTransform = transform.Find(childName);

            if (childTransform != null)
                return childTransform.gameObject;

            GameObject recursiveChildObject = FindChildByName(transform, childName);

            if (recursiveChildObject != null)
                return recursiveChildObject;
        }

        if (assignedObject != null && assignedObject.transform != null && assignedObject.transform.IsChildOf(transform))
            return assignedObject;

        return assignedObject;
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

    /// <summary>
    /// 현재 선택된 배우 오브젝트에서 컴포넌트를 다시 가져옵니다.
    /// 배우를 교체했을 때 예전 Animator/SpriteRenderer 참조가 남지 않도록 매번 소유 오브젝트 기준으로 읽습니다.
    /// </summary>
    private T ResolveOwnedComponent<T>(GameObject targetObject) where T : Component
    {
        return targetObject != null ? targetObject.GetComponent<T>() : null;
    }
}
