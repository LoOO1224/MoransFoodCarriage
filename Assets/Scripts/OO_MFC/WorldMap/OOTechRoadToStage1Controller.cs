// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechRoadToStage1Controller.cs
// - 역할: 월드맵, 도로, 스테이지 진입, HUD 흐름을 연결합니다.
// - 유지보수: UIManager 전환과 그룹 활성/비활성 순서가 게임 진행을 결정하므로 호출 순서를 유지합니다.
// =============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ?쒖옉 吏?먮???Stage ?낃뎄源뚯? MFC ?대룞??吏?섑븯??RoadGroup 而⑦듃濡ㅻ윭?낅땲??
/// 留??대?吏??臾대? 諛곌꼍?닿퀬, MFC 諛곗슦???뺥빐吏??꾨줈 ???꾩뿉?쒕쭔 ?ㅻⅨ履쎌쑝濡??대룞?⑸땲??
/// </summary>
public class OOTechRoadToStage1Controller : MonoBehaviour
{
    // ?쎈뒗 ?쒖꽌:
    // 1. OnEnable: ?꾩옱 RoadGroup, MFC, 諛곌꼍 留? HUD瑜?以鍮꾪빀?덈떎.
    // 2. Update/MoveMFC 怨꾩뿴: D???대룞怨??꾨줈 ?????꾩튂 ?쒗븳??泥섎━?⑸땲??
    // 3. ChangeToNextMapRoutine: StartPointMap -> RoadMap1 -> RoadMap2 -> Stage1EntryMap ?꾪솚???대떦?⑸땲??
    // 4. OpenTargetStageGroup: 留덉?留?留??앹뿉 ?꾨떖?섎㈃ Stage1Group 媛숈? 紐⑺몴 StageGroup??耳?땲??
    // 5. PrepareRoadHUD/PlayTutorial: HUD? 珥덈컲 ?쒗넗由ъ뼹/????먮쫫???곌껐?⑸땲??
    // ?좎?蹂댁닔 二쇱쓽:
    // - 媛?RoadGroup??諛곌꼍 ?대?吏???섏씠?대씪?ㅼ뿉??吏곸젒 援먯껜?⑸땲??
    // - ?ㅼ쓬 StageGroup ?대쫫? ConfigureRoadFlow? Inspector 媛믪쑝濡?留욎땅?덈떎.
    // - Road 怨듯넻 濡쒖쭅???섏뼱?섎㈃ RoadBaseController濡?遺꾨━?섎뒗 寃껋씠 醫뗭뒿?덈떎.

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
    private const string _stage4SecondGroupName = "Stage4_2Group";
    private const string _finalStageGroupName = "FinalStageGroup";
    private const int _mfcMinimumVisibleSortingOrder = 2000;

    [Header("Scene Components")]
    [SerializeField] private OOTechSceneContext Context_Scene;
    [SerializeField] private OOTechRoadHUDController HUD_Road;
    [SerializeField] private OOTechTutorial2Controller Tutorial2_Controller;
    [SerializeField] private OOTechRoadMoveInputReader MoveInput_Reader;
    [SerializeField] private OOTechRoadAutoMoveFailSafe AutoMove_FailSafe;
    [SerializeField] private OOTechTargetStageBGMPlayer TargetStage_BGMPlayer;

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
        "Stage4_2Group",
        "Stage4Group",
        "FinalStageGroup"
    };

    [Header("Road Data")]
    [SerializeField] private string _stage3CueSheetDataId = "Stage3_CueSheet_01";
    [SerializeField] private string _stage4CueSheetDataId = "Stage4_CueSheet_01";
    [SerializeField] private string _thirdRoadMissionDataId = "Stage3_Road_Quest_01";
    [SerializeField] private string _thirdRoadMissionFallbackText = "남쪽 숲으로 가세요!";
    [SerializeField] private string _fourthRoadMissionDataId = "Stage4_Road_Quest_01";
    [SerializeField] private string _fourthRoadMissionFallbackText = "북쪽 농경지대로 가세요.";
    [SerializeField] private string _stage4RewardCarrotIngredientId = "Ing_Carrot_01";
    [SerializeField] private int _stage4RewardCarrotCount = 10;

    [Header("Road Opening Dialogue")]
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private string _secondRoadGroupName = "2nd_Road_to_Stage2";
    [SerializeField] private string _secondRoadMissionDataId = "Stage2_Road_Quest_01";
    [SerializeField] private string _secondRoadMissionFallbackText = "서쪽 도시에 가 탐관오리의 자택을 방문하세요.";
    [SerializeField] private string _roadMap1CookingGuideTitle = "요리하기 안내";
    [TextArea(2, 4)]
    [SerializeField] private string _roadMap1CookingGuideText = "요리를 해야 다음 길로 넘어갈 수 있습니다. 요리하기 버튼을 눌러 부엌에서 필요한 음식을 완성하세요.";
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
    private bool _isBlockedInputFailSafeLogged;
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
        "Stage4_2Group",
        "Stage4Group",
        "FinalStageGroup",
        "EpilogueGroup",
        "DialogueGroup",
        "TutorialGuideGroup"
    };

    /// <summary>
    /// ??李몄“, 移대찓?? HUD, ?섏씠???뚰뭹??誘몃━ ?곌껐?⑸땲??
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
    /// 媛숈? RoadGroup 援ъ“瑜?Stage 踰덊샇蹂꾨줈 ?ъ궗?⑺븯湲??꾪빐 ?꾩옱/紐⑺몴 洹몃９ ?대쫫???ㅼ젙?⑸땲??
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
    /// RoadGroup??耳쒖?硫?MFC瑜?泥?留??쒖옉?먯뿉 ?볤퀬 HUD? ?쒗넗由ъ뼹???쒖옉?⑸땲??
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
        RepairStage4RoadInventoryIfNeeded();
        PrepareRoadTrip();
        StartOpeningTutorialIfNeeded();
        StartRoadOpeningDialogueIfNeeded();
    }

    /// <summary>
    /// Stage1 ?꾨즺 蹂댁긽??鍮좎쭊 ?곹깭濡?2nd_Road???ㅼ뼱??寃쎌슦 ?몃깽?좊━瑜???踰?蹂댁젙?⑸땲??
    /// ?곹솕濡?移섎㈃ ?댁쟾 ?λ㈃?먯꽌 ?뚰뭹 援먰솚 ?먭? ?꾨씫?먯쓣 ?? ?ㅼ쓬 臾대? ?낃뎄?먯꽌 ?뚰뭹 ?대떦??鍮좊Ⅴ寃??뺤궛?섎뒗 ?덉쟾 ?먯엯?덈떎.
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

    private void RepairStage4RoadInventoryIfNeeded()
    {
        if (!IsFourthRoadCurrent() || OOTechGameManager.Inst == null)
            return;

        OO_Stage4CueSheet cueSheetData = ResolveStage4CueSheetData();
        string carrotIngredientId = cueSheetData != null && !string.IsNullOrWhiteSpace(cueSheetData.CarrotIngredientId)
            ? cueSheetData.CarrotIngredientId
            : _stage4RewardCarrotIngredientId;

        AddInventoryItemToTargetCount(carrotIngredientId, _stage4RewardCarrotCount);

        if (HUD_Road != null)
        {
            HUD_Road.RequestRefreshInventoryView();
            HUD_Road.SetInventoryNewBadgeActive(true);
        }
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
    /// RoadGroup???ロ엳硫?肄붾（?? ?좊땲硫붿씠?? HUD, ?섏씠???곹깭瑜??뺣━?⑸땲??
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

        if (AutoMove_FailSafe != null)
            AutoMove_FailSafe.ResetState();
    }

    /// <summary>
    /// ?쒗넗由ъ뼹/?ㅻ쾭?덉씠/?섏씠??以묒씠 ?꾨땺 ??D???낅젰?쇰줈 MFC瑜??ㅻⅨ履??대룞?쒗궢?덈떎.
    /// </summary>
    private void Update()
    {
        bool isMoveRightPressed = MoveInput_Reader != null && MoveInput_Reader.IsMoveRightPressed();
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

            if (AutoMove_FailSafe != null && !AutoMove_FailSafe.IsLockCleared)
            {
                RequestClearBlockedRoadInputState("Build auto-move fail-safe cleared stale road lock.");
                AutoMove_FailSafe.MarkLockCleared();
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
    /// D?ㅼ? ?ㅻⅨ履?諛⑺뼢?ㅻ? ?④퍡 諛쏆뒿?덈떎.
    /// 鍮뚮뱶 ?섍꼍?먯꽌 ?ㅻ낫???덉씠?꾩썐?대굹 ?ъ빱??李⑥씠濡??쒖そ ?낅젰???붾뱾?ㅻ룄 Road 諛곗슦媛 ?吏곸씪 ???덇쾶 ?섎뒗 蹂댄뿕?낅땲??
    /// </summary>
    /// <summary>
    /// 湲곗〈 Input Manager 諛⑹떇?쇰줈 D/?ㅻⅨ履?諛⑺뼢?ㅻ? ?쎌뒿?덈떎.
    /// 鍮뚮뱶 ?ㅼ젙??New Input ?꾩슜?쇰줈 諛붾?寃쎌슦 ?덉쇅媛 ?????덉뼱 ?덉쟾?섍쾶 媛먯뙃?덈떎.
    /// </summary>
    /// <summary>
    /// Unity New Input System 諛⑹떇?쇰줈 D/?ㅻⅨ履?諛⑺뼢?ㅻ? ?쎌뒿?덈떎.
    /// 鍮뚮뱶?먯꽌 Legacy Input???붾뱾由??뚮룄 ?ㅻ낫???곹깭瑜?吏곸젒 ?뺤씤?섍린 ?꾪븳 蹂댄뿕?낅땲??
    /// </summary>
    /// <summary>
    /// ?쒗넗由ъ뼹, ?ㅽ봽????? HUD ?ㅻ쾭?덉씠媛 耳쒖졇 ?덉쑝硫??먮옒??MFC 議곗옉???좉툒?덈떎.
    /// 鍮뚮뱶?먯꽌 ??媛믪씠 ?由ъ? ?딆쑝硫?D?ㅺ? 二쎌? 寃껋쿂??蹂댁씠誘濡???怨녹뿉???먯씤???먯젙?⑸땲??
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
    /// 鍮뚮뱶?먯꽌 ???낅젰???≫엳吏 ?딅뒗 理쒖븙??寃쎌슦?먮룄 Road ?λ㈃??硫덉텛吏 ?딄쾶 ?섎뒗 ?먮룞 ?대룞 蹂댄뿕?낅땲??
    /// ?곹솕濡?蹂대㈃ 諛곗슦媛 ???ъ씤??紐??ㅼ뿀????議곌컧?낆씠 ?뺥빐吏??숈꽑?濡?諛??二쇰뒗 留덉?留??덉쟾 ?먯엯?덈떎.
    /// </summary>
    private bool IsBuildAutoMoveRequested(bool isMoveRightPressed)
    {
        CacheRoadHelperComponents();

        return AutoMove_FailSafe != null &&
               AutoMove_FailSafe.RequestAutoMove(isMoveRightPressed, _isChangingMap, _isRoadTripComplete, Object_MFC);
    }

    /// <summary>
    /// RoadGroup?먯꽌 ?대룞 ?ㅺ? ?뚮졇?붾뜲 ?쒓컙 ?뺤????ㅻ쾭?덉씠 ?붿뿬 ?곹깭媛 ?⑥븘 ?덉쑝硫?利됱떆 蹂듦뎄?⑸땲??
    /// ?붾━/?꾧컧/?붾뱶留듭뿉???뚯븘???ㅼ뿉??諛곗슦媛 ?ㅼ떆 ?吏곸씪 ???덇쾶 留뚮뱶??怨듯넻 ?덉쟾?μ튂?낅땲??
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
    /// 鍮뚮뱶?먯꽌 RoadMap1 ?꾩갑 媛?대뱶???ㅻ쾭?덉씠 蹂듦?媛 ?딄린硫??대룞 ?낅젰??怨꾩냽 留됲옄 ???덉뒿?덈떎.
    /// ?뚮젅?댁뼱媛 D/?ㅻⅨ履쏀궎瑜?怨꾩냽 ?꾨Ⅴ怨??덉쑝硫??⑥? ?좉툑 ?먮? ?뺣━??MFC 諛곗슦媛 ?ㅼ떆 ?대룞?섍쾶 ?⑸땲??
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
    /// ?대룞??留됰뒗 ?⑥? ?쒗넗由ъ뼹, ??? HUD ?ㅻ쾭?덉씠, ?쒓컙 ?뺤? ?곹깭瑜???踰덉뿉 ?뺣━?⑸땲??
    /// ?뚮젅?댁뼱媛 D瑜??뚮??붾뜲 臾대? ?먭? ?ロ엳吏 ?딆? ?곹솴??蹂듦뎄?섎뒗 怨듯넻 ?덉쟾?μ튂?낅땲??
    /// </summary>
    public void RequestRestoreRoadMovementAfterOverlayReturn()
    {
        _isRoadTripComplete = false;
        RequestClearBlockedRoadInputState("Road movement restored after returning from an overlay.", false);

        if (AutoMove_FailSafe != null)
            AutoMove_FailSafe.ResetState();
    }

    private void RequestClearBlockedRoadInputState(string reason)
    {
        RequestClearBlockedRoadInputState(reason, true);
    }

    private void RequestClearBlockedRoadInputState(string reason, bool isRestoreHUDOverlay)
    {
        StopOpeningTutorial();
        StopRoadOpeningDialogue();

        if (isRestoreHUDOverlay && HUD_Road != null && HUD_Road.IsOverlayOpen)
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
    /// RoadGroup???앸궃 ?ㅼ뿉???낅젰 蹂듦뎄媛 ?꾩슂 ?놁?留? 洹??꾩뿉??泥?留듬룄 ?ы븿?댁꽌 蹂듦뎄瑜??덉슜?⑸땲??
    /// 鍮뚮뱶?먯꽌 泥??쒗넗由ъ뼹 ?⑤꼸??蹂댁씠吏 ?딄퀬 ?좉툑留??⑤뒗 寃쎌슦媛 ?덉뼱??泥?留??쒗븳???먯? ?딆뒿?덈떎.
    /// </summary>
    private bool CanUseBlockedInputFailSafe()
    {
        return !_isRoadTripComplete;
    }

    /// <summary>
    /// MFC 諛곗슦? 留?諛곌꼍 ?ㅻ툕?앺듃瑜????대쫫 ?먮뒗 ??븷?쒕줈 李얠뒿?덈떎.
    /// ?몄뒪?숉꽣 李몄“媛 ?덉쑝硫?洹?媛믪쓣 ?곗꽑 ?ъ슜???섏쨷??諛곌꼍留?援먯껜?섍린 ?쎄쾶 ?〓땲??
    /// </summary>
    public void ResolveSceneReferences()
    {
        CacheSceneContextReference();
        CacheRoadHelperComponents();

        Object_MFC = ResolveBestMFCObject(Object_MFC);
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
    /// 4th Road??StartPointMap ?ㅼ쓬??怨㏓컮濡?Stage4 吏꾩엯留듭쑝濡??섏뼱媛??吏㏃? 湲몄엯?덈떎.
    /// 媛숈? Road 媛먮룆???곕릺, 怨듭뿰 ?먯떆?몃쭔 ???μ쭨由щ줈 諛붽퓭 ?쇱슦??諛⑹떇?낅땲??
    /// </summary>
    private GameObject[] CreateMapObjectArrayForCurrentRoad()
    {
        if (IsFourthRoadCurrent())
            return CreateValidMapObjectArray(Object_StartPointMap, Object_Stage1EntryMap);

        return CreateValidMapObjectArray(Object_StartPointMap, Object_RoadMap1, Object_RoadMap2, Object_Stage1EntryMap);
    }

    /// <summary>
    /// ?ㅼ젣濡?臾대???議댁옱?섎뒗 Road 諛곌꼍留??먯떆??諛곗뿴???ｌ뒿?덈떎.
    /// ?곹솕 鍮꾩쑀濡쒕뒗 怨듭뿰?μ뿉 ?녿뒗 諛곌꼍留됱? ?먯떆?몄뿉??鍮쇨퀬, 以鍮꾨맂 諛곌꼍留됰쭔 ?쒖꽌?濡??섍린???쇱엯?덈떎.
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
    /// ?대쫫 寃?됰낫??癒쇱? OOTechSceneObject ??븷?쒕? ?쎌뒿?덈떎.
    /// Road 媛먮룆? "MFC", "RoadMap1" 媛숈? ??븷留??뚭퀬, 援ъ껜 ?ㅻ툕?앺듃 諛곗튂??諛곗슦媛 媛뽰뒿?덈떎.
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
    /// 泥?留듭쓣 耳쒓퀬 MFC瑜??꾨줈 ?쒖옉?먯뿉 諛곗튂??濡쒕뱶 ?ы뻾??以鍮꾪빀?덈떎.
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
        EnsureMFCVisible();
        FocusCameraOnCurrentMap();
        HideFadeOverlay();
        PrepareMFCAnimation();
    }

    /// <summary>
    /// ?꾩옱 留듭쓽 ?꾨줈 ?좊? ?곕씪 MFC瑜??ㅻⅨ履쎌쑝濡??대룞?쒗궎怨??앹뿉 ?우쑝硫??ㅼ쓬 留듭쑝濡??섍퉩?덈떎.
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
    /// ?붾㈃???대몼寃??덈떎媛 ?ㅼ쓬 留??먮뒗 StageGroup?쇰줈 ?꾪솚?섎뒗 ?섏씠??猷⑦떞?낅땲??
    /// </summary>
    private IEnumerator ChangeToNextMapRoutine()
    {
        if (_isChangingMap)
            yield break;

        if (ShouldUseWormholeTransition() && _currentMapIndex >= _mapObjectArray.Length - 1)
            yield break;

        _isChangingMap = true;
        SetMFCAnimationPlaying(false);
        HUD_Road?.CloseHUDGuide();
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
    /// 3rd Road?먯꽌??蹂꾨룄 Wormhole ?뚰뭹???섏〈?섏? ?딄퀬 留덉?留?留듭쓽 以묎컙 ??吏?먯뿉???꾪솚???쒖옉?⑸땲??
    /// ?곹솕濡?移섎㈃ 諛곗슦媛 ?뱀젙 ?뚰뭹??諛잛븘?쇰쭔 而룹씠 ?섎뒗 諛⑹떇???꾨땲?? 臾대? 以묒븰 ?쒖떆?좎뿉 ?ㅼ뼱?ㅻ㈃ 議곕챸 ?꾪솚 ?먭? ?섍???諛⑹떇?낅땲??
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
    /// Wormhole???우븯????媛뺤젣 ?몄뭅?댄꽣泥섎읆 ?붾㈃??吏㏐쾶 踰덉찉?대ŉ ?ㅼ쓬 臾대? 吏꾩엯???뚮┰?덈떎.
    /// ??UI瑜?留뚮뱾吏 ?딄퀬, ?ъ뿉 諛곗튂??FadeOverlay 諛곗슦瑜??좉퉸 ?ㅻⅨ ??議곕챸泥섎읆 ?ъ슜?⑸땲??
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
    /// ?꾩옱 ?몃뜳?ㅼ쓽 留?諛곌꼍留?耳쒓퀬 ?섎㉧吏 諛곌꼍? ?뺣땲??
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
    /// MFC瑜??꾩옱 留??쇱そ 吏꾩엯 ?꾩튂? ?꾨줈 ?믪씠??留욎땅?덈떎.
    /// </summary>
    private void PlaceMFCAtMapEntry()
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (Object_MFC == null || mapRenderer == null)
            return;

        Object_MFC.transform.position = CalculateMapEntryPosition(mapRenderer);
    }

    /// <summary>
    /// 泥?留??쒖옉? ?ъ슜?먭? ?섏씠?대씪?ㅼ뿉 ??MFC_StartPoint ?꾩튂?쒕? ?곗꽑 ?ъ슜?⑸땲??
    /// ?꾩튂?쒓? ?놁쑝硫??꾩옱 諛곗튂??MFC瑜?洹몃?濡??먯뼱 媛먮룆???≪븘??臾대? ?꾩튂瑜?議댁쨷?⑸땲??
    /// </summary>
    private void PlaceMFCAtOpeningPosition()
    {
        if (Object_MFC == null)
            return;

        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (IsFourthRoadCurrent())
        {
            if (mapRenderer != null)
                Object_MFC.transform.position = CalculateMapEntryPosition(mapRenderer);

            return;
        }

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

        if (IsFourthRoadCurrent() || !IsPositionInsideMapBounds(Object_MFC.transform.position, mapRenderer))
        {
            Object_MFC.transform.position = CalculateMapEntryPosition(mapRenderer);
            return;
        }

        Vector3 position = Object_MFC.transform.position;
        position.y = CalculateRoadLaneY(mapRenderer);
        Object_MFC.transform.position = position;
    }

    /// <summary>
    /// ?꾩옱 留??쇱そ ?꾨줈 吏꾩엯 ?꾩튂瑜?怨꾩궛?⑸땲??
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
    /// MFC ?쒖옉 ?꾩튂?쒓? ?꾩옱 諛곌꼍 ?덉そ???덈뒗吏 ?뺤씤?⑸땲??
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
    /// ?꾩옱 留?諛곌꼍 ?ㅽ봽?쇱씠?멸? 蹂댁씠?꾨줉 Renderer ?곹깭瑜?蹂듦뎄?⑸땲??
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
    /// MFC 諛곗슦媛 ?ㅼ닔濡?爰쇱?嫄곕굹 ?щ챸?댁쭊 寃쎌슦 ?붾㈃??蹂댁씠?꾨줉 蹂듦뎄?⑸땲??
    /// </summary>
    private void EnsureMFCVisible()
    {
        if (Object_MFC == null)
            return;

        Object_MFC.SetActive(true);

        if (Renderer_MFC == null)
            Renderer_MFC = Object_MFC.GetComponentInChildren<SpriteRenderer>(true);

        SpriteRenderer[] rendererArray = Object_MFC.GetComponentsInChildren<SpriteRenderer>(true);

        if (rendererArray == null || rendererArray.Length == 0)
            return;

        int safeSortingOrder = Mathf.Max(_mfcSortingOrder, _mfcMinimumVisibleSortingOrder);

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null)
                continue;

            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = Mathf.Max(spriteRenderer.sortingOrder, safeSortingOrder);

            Color color = spriteRenderer.color;

            if (color.a <= 0.01f)
            {
                color.a = 1f;
                spriteRenderer.color = color;
            }
        }
    }

    /// <summary>
    /// MFC 嫄룰린 ?좊땲硫붿씠?섏쓣 泥??꾨젅?꾩뿉 以鍮꾪빀?덈떎.
    /// 媛먮룆???대룞 ?먮? 以??뚮쭔 ?ㅼ젣 ?좊땲硫붿씠???띾룄瑜??щ┰?덈떎.
    /// </summary>
    private void PrepareMFCAnimation()
    {
        if (Animator_MFC == null)
            Animator_MFC = Object_MFC != null ? Object_MFC.GetComponentInChildren<Animator>(true) : null;

        if (Animator_MFC == null)
            return;

        Animator_MFC.enabled = true;
        PlayMFCWalkState(0f);
        Animator_MFC.Update(0f);
        SetMFCAnimationPlaying(false);
    }

    /// <summary>
    /// MFC 嫄룰린 ?곹깭瑜??좊땲硫붿씠?곗뿉??李얠븘 ?ъ깮?⑸땲??
    /// ???곹깭紐?MFC_isWalking???곗꽑 ?곌퀬, ?꾩쭅 ?댁쟾 而⑦듃濡ㅻ윭?쇰㈃ MFC ?곹깭濡??섎룎?꾧컩?덈떎.
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
    /// D???대룞 以묒씪 ?뚮쭔 MFC ?좊땲硫붿씠?섏씠 ?ъ깮?섍쾶 ?⑸땲??
    /// </summary>
    private void SetMFCAnimationPlaying(bool isPlaying)
    {
        if (Animator_MFC == null || !_isAnimateMFCOnlyWhileMoving)
            return;

        Animator_MFC.speed = isPlaying ? Mathf.Max(0.01f, _mfcWalkAnimationSpeed) : 0f;
    }

    /// <summary>
    /// ?꾩옱 留??대?吏???몃줈 ?ш린?먯꽌 ?꾨줈 ??Y ?꾩튂瑜?怨꾩궛?⑸땲??
    /// </summary>
    private float CalculateRoadLaneY(SpriteRenderer mapRenderer)
    {
        Bounds bounds = mapRenderer.bounds;
        return bounds.min.y + (bounds.size.y * _roadLaneNormalizedHeight);
    }

    /// <summary>
    /// ?꾩옱 留??ㅻⅨ履??앹뿉???ㅼ쓬 ?λ㈃?쇰줈 ?섏뼱媛?X ?꾩튂瑜?怨꾩궛?⑸땲??
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
    /// ?대? ?몃뜳?ㅺ? ??댁죱?????ㅼ젣 耳쒖졇 ?덈뒗 RoadMap Renderer瑜?李얠븘 ?대룞 湲곗??쇰줈 ?ъ슜?⑸땲??
    /// 媛먮룆???먯떆??踰덊샇媛 諛?ㅻ룄, ?뚮젅?댁뼱媛 蹂닿퀬 ?덈뒗 諛곌꼍 ?꾩뿉??MFC媛 怨꾩냽 ?吏곸씠寃??섎뒗 留덉?留??덉쟾留앹엯?덈떎.
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
    /// 移대찓???붾줈????곸쓣 MFC濡?諛붽씀怨? ?꾩슂?섎㈃ 利됱떆 MFC ?꾩튂濡??ㅻ깄?⑸땲??
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
    /// ?ъ뿉 諛곗튂??RoadMapFadeCanvas? Image_FadeOverlay瑜?李얠븘 ?섏씠???뚰뭹?쇰줈 ?곌껐?⑸땲??
    /// </summary>
    private void CreateFadeOverlayIfNeeded()
    {
        if (_fadeCanvas != null)
            return;

        GameObject canvasObject = RequestChildObjectByName(transform, "RoadMapFadeCanvas");

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

        GameObject imageObject = RequestChildObjectByName(canvasObject.transform, "Image_FadeOverlay");

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
    /// 寃? ?섏씠???ㅻ쾭?덉씠???뚰뙆瑜??쒖꽌??諛붽퓭 留??꾪솚???곗텧?⑸땲??
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
    /// ?꾩옱 洹몃９??怨듭슜 HUD 而⑦듃濡ㅻ윭瑜?李얠뒿?덈떎.
    /// </summary>
    private void CacheHUDReference()
    {
        if (HUD_Road == null)
            HUD_Road = GetComponent<OOTechRoadHUDController>();

        if (HUD_Road == null)
            Debug.LogWarning($"[OOTechRoadToStage1Controller] {gameObject.name} needs OOTechRoadHUDController attached in the scene.");
    }

    /// <summary>
    /// Tutorial2Controller瑜?李얠븘 HUD ?뚭컻? RoadMap1 ?꾩갑 ?덈궡瑜?留↔퉩?덈떎.
    /// </summary>
    private void CacheTutorial2Reference()
    {
        if (Tutorial2_Controller == null)
            Tutorial2_Controller = GetComponent<OOTechTutorial2Controller>();

        if (Tutorial2_Controller == null)
            Debug.LogWarning($"[OOTechRoadToStage1Controller] {gameObject.name} needs OOTechTutorial2Controller attached in the scene.");
    }

    /// <summary>
    /// 濡쒕뱶 ?붾㈃??HUD瑜?以鍮꾪븯怨??꾩옱 洹몃９ 洹쒖튃??留욊쾶 ?붾━ 踰꾪듉 ?좉툑???곸슜?⑸땲??
    /// </summary>
    private void PrepareRoadHUD()
    {
        if (HUD_Road == null)
            return;

        HUD_Road.SetOwnerGroupName(_currentGroupName);
        HUD_Road.PrepareHUD();
        HUD_Road.SetCookingUnlocked(_currentGroupName != "1st_Road_to_Stage1", false);
        RequestRoadEntryNewBadgeIfNeeded();
        ApplyRoadMissionForCurrentGroup();
        HUD_Road.SetHUDVisible(true);

        if (IsFourthRoadCurrent())
            RequestForceStage4PresentationHUDOnRoad();
    }

    /// <summary>
    /// 특정 RoadGroup 진입 때 보여 줄 NEW 뱃지를 한 번 정리합니다.
    /// Game View에서는 Stage2 보상을 받고 3rd Road에 들어왔을 때 요리하기가 아니라 인벤토리가 먼저 열립니다.
    /// </summary>
    private void RequestRoadEntryNewBadgeIfNeeded()
    {
        if (HUD_Road == null)
            return;

        if (_currentGroupName == _thirdRoadGroupName)
            HUD_Road.SetInventoryNewBadgeActive(true);

        if (IsFourthRoadCurrent())
            HUD_Road.SetInventoryNewBadgeActive(true);
    }

    /// <summary>
    /// RoadGroup ?대쫫??留욎떠 HUD??湲??덈궡 ?꾨Т瑜??곗씠?곕줈 援먯껜?⑸땲??
    /// ?곹솕濡?移섎㈃ ?대룞 ?λ㈃留덈떎 諛곗슦?먭쾶 ?ㅻⅨ 肄쒖떆?몃? ?섎닠二쇰뒗 ?먯엯?덈떎.
    /// </summary>
    private void ApplyRoadMissionForCurrentGroup()
    {
        if (HUD_Road == null)
            return;

        if (_currentGroupName == _secondRoadGroupName)
        {
            ApplyRoadMission(_secondRoadMissionDataId, _secondRoadMissionFallbackText);
            return;
        }

        if (IsThirdRoadCurrent())
        {
            OO_Stage3CueSheet cueSheetData = ResolveStage3CueSheetData();
            string missionDataId = cueSheetData != null && !string.IsNullOrWhiteSpace(cueSheetData.RoadMissionDataId)
                ? cueSheetData.RoadMissionDataId
                : _thirdRoadMissionDataId;
            string fallbackText = cueSheetData != null && !string.IsNullOrWhiteSpace(cueSheetData.RoadMissionFallbackText)
                ? cueSheetData.RoadMissionFallbackText
                : _thirdRoadMissionFallbackText;
            ApplyRoadMission(missionDataId, fallbackText);
            return;
        }

        if (IsFourthRoadCurrent())
        {
            OO_Stage4CueSheet cueSheetData = ResolveStage4CueSheetData();
            string missionDataId = cueSheetData != null && !string.IsNullOrWhiteSpace(cueSheetData.RoadMissionDataId)
                ? cueSheetData.RoadMissionDataId
                : _fourthRoadMissionDataId;
            string fallbackText = cueSheetData != null && !string.IsNullOrWhiteSpace(cueSheetData.RoadMissionFallbackText)
                ? cueSheetData.RoadMissionFallbackText
                : _fourthRoadMissionFallbackText;
            ApplyRoadMission(missionDataId, fallbackText, false);
        }
    }

    private void ApplyRoadMission(string missionDataId, string fallbackText, bool isAutoOpenMissionPanel = true)
    {
        string roadMissionText = ResolveStageQuestDescription(missionDataId, fallbackText);
        HUD_Road.RequestSetRoadMissionText(roadMissionText, true, isAutoOpenMissionPanel);
    }

    private OO_Stage3CueSheet ResolveStage3CueSheetData()
    {
        return OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStage3CueSheetData(_stage3CueSheetDataId) : null;
    }

    private OO_Stage4CueSheet ResolveStage4CueSheetData()
    {
        return OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStage4CueSheetData(_stage4CueSheetDataId) : null;
    }

    private void RequestForceStage4PresentationHUDOnRoad()
    {
        if (HUD_Road == null)
            return;

        OO_Stage4CueSheet cueSheetData = ResolveStage4CueSheetData();
        string missionDataId = cueSheetData != null && !string.IsNullOrWhiteSpace(cueSheetData.RoadMissionDataId)
            ? cueSheetData.RoadMissionDataId
            : _fourthRoadMissionDataId;
        string fallbackText = cueSheetData != null && !string.IsNullOrWhiteSpace(cueSheetData.RoadMissionFallbackText)
            ? cueSheetData.RoadMissionFallbackText
            : _fourthRoadMissionFallbackText;
        string missionText = ResolveStageQuestDescription(missionDataId, fallbackText);

        HUD_Road.RequestEnableStage4PresentationHUD(_currentGroupName, missionText, "당근전 조리 가이드", ResolveStage4RecipeGuideText());
    }

    private string ResolveStage4RecipeGuideText()
    {
        string starchGuide = ResolveRecipeGuideByResultItemId(
            "OO_CarrotStarch_1",
            "레시피 조합 버튼을 누른 뒤 떡 1개와 당근 1개를 조합 칸에 올려 당근전분을 만드세요.");
        string carrotCakeGuide = ResolveRecipeGuideByResultItemId(
            "OO_CarrotCake_1",
            "당근전분 1개를 인벤토리에서 집어 가마솥에 드래그하면 당근전이 완성됩니다.");

        return starchGuide + "\n" + carrotCakeGuide;
    }

    private string ResolveRecipeGuideByResultItemId(string resultItemId, string fallbackText)
    {
        if (OOTechGameDataManager.Inst == null || string.IsNullOrEmpty(resultItemId))
            return fallbackText;

        List<OO_Recipe> recipeDataList = OOTechGameDataManager.Inst.GetRecipeDataList();

        foreach (OO_Recipe recipeData in recipeDataList)
        {
            if (recipeData == null || recipeData.ResultItemId != resultItemId)
                continue;

            if (IsReadableStage4RecipeText(recipeData.QuantityGuideText))
                return recipeData.QuantityGuideText;

            if (IsReadableStage4RecipeText(recipeData.Description))
                return recipeData.Description;
        }

        return fallbackText;
    }

    private bool IsReadableStage4RecipeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return value.Contains("당근") || value.Contains("떡") || value.Contains("가마솥") || value.Contains("레시피");
    }

    /// <summary>
    /// OO_StageQuest?먯꽌 ?꾨Т ?ㅻ챸???쎄퀬, ?곗씠?곌? ?꾩쭅 鍮꾩뼱 ?덉쑝硫??덉쟾 臾멸뎄瑜??ъ슜?⑸땲??
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
    /// RoadGroup 吏꾩엯 ???댁쟾 臾대????꾩껜 ?붾㈃ UI媛 ?⑥븘 移대찓?쇱? 踰꾪듉??留됱? ?딅룄濡??뺣━?⑸땲??
    /// </summary>
    private void CloseBlockingSceneGroups()
    {
        foreach (string groupName in _blockingGroupNameArray)
        {
            if (string.IsNullOrEmpty(groupName) || groupName == _currentGroupName)
                continue;

            GameObject groupObject = RequestSceneObjectByName(groupName);

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
    /// 1st_Road_to_Stage1 吏꾩엯 ??HUD ?뚭컻 ?쒗넗由ъ뼹???쒖옉?⑸땲??
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
    /// 2nd Road 吏꾩엯 吏곹썑 ????먯떆?몃? ?ъ깮?⑸땲??
    /// Game View?먯꽌??DialoguePanel??癒쇱? ?④퀬, 紐⑤뱺 ??щ? ?섍릿 ?ㅼ뿉??MFC ?대룞 ?낅젰???由쎈땲??
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
        GameObject dialogueGroup = RequestSceneObjectByName(_dialogueGroupName);

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

        GameObject dialogueGroup = RequestSceneObjectByName(_dialogueGroupName);

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
    /// RoadMap1 ?댄썑?먮뒗 ?붾━?섍린 HUD瑜??ъ슜?????덇쾶 ?쎈땲??
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
    /// 泥?踰덉㎏ RoadMap1???꾩갑?덉쓣 ?뚮쭔 ?붾━ ?덈궡 ????쒗넗由ъ뼹???ъ깮?⑸땲??
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
        yield return Tutorial2_Controller.PlayRoadMap1ArrivalRoutine(HUD_Road);

        if (HUD_Road != null)
            HUD_Road.ShowRoadMessageTopCenter(_roadMap1CookingGuideTitle, _roadMap1CookingGuideText, null);
    }

    /// <summary>
    /// 濡쒕뱶 留덉?留?留??앹뿉 ?꾨떖?섎㈃ 紐⑺몴 StageGroup留?耳쒓퀬 ?섎㉧吏 StageGroup? ?뺣땲??
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
        if (!string.IsNullOrWhiteSpace(requestedGroupName) && RequestSceneObjectByName(requestedGroupName) != null)
            return requestedGroupName;

        if (requestedGroupName == _stage4FirstGroupName && RequestSceneObjectByName(_stage4GroupName) != null)
        {
            Debug.LogWarning("[OOTechRoadToStage1Controller] Stage4_1Group not found. Falling back to Stage4Group.");
            return _stage4GroupName;
        }

        return requestedGroupName;
    }

    private void RequestPlayTargetStageBGM(string targetGroupName)
    {
        CacheRoadHelperComponents();

        if (TargetStage_BGMPlayer != null)
            TargetStage_BGMPlayer.RequestPlayForTargetGroup(targetGroupName);
    }

    private void CacheRoadHelperComponents()
    {
        if (AutoMove_FailSafe == null)
            AutoMove_FailSafe = GetComponent<OOTechRoadAutoMoveFailSafe>();

        if (AutoMove_FailSafe == null)
            AutoMove_FailSafe = gameObject.AddComponent<OOTechRoadAutoMoveFailSafe>();

        AutoMove_FailSafe.Configure(true, 1.5f);

        if (TargetStage_BGMPlayer == null)
            TargetStage_BGMPlayer = GetComponent<OOTechTargetStageBGMPlayer>();

        if (MoveInput_Reader == null)
            MoveInput_Reader = GetComponent<OOTechRoadMoveInputReader>();

        if (MoveInput_Reader == null)
            MoveInput_Reader = gameObject.AddComponent<OOTechRoadMoveInputReader>();
    }

    /// <summary>
    /// StageGroup???댁옄留덉옄 諛곌꼍 ?꾩껜媛 Game View???ㅼ뼱?ㅻ룄濡?移대찓?쇱? Canvas 湲곗?媛믪쓣 留욎땅?덈떎.
    /// 珥ъ쁺媛먮룆????臾대????ㅼ뼱媛?먮쭏????대뱶?룹쑝濡??꾨젅?꾩쓣 ?ㅼ떆 ?〓뒗 ?묒뾽?낅땲??
    /// </summary>
    private void RequestFitTargetStageCamera(string targetGroupName)
    {
        GameObject targetGroupObject = RequestSceneObjectByName(targetGroupName);

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

        GameObject groupObject = RequestSceneObjectByName(groupName);

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

    private GameObject RequestSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return null;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = RequestChildObjectByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private GameObject RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = RequestChildObjectByName(rootTransform.GetChild(index), objectName);

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
            GameObject childObject = OOTechSceneQuery.RequestChildObjectByName(transform, childName);
            Transform childTransform = childObject != null ? childObject.transform : null;

            if (childTransform != null)
                return childTransform.gameObject;
        }

        return null;
    }

    /// <summary>
    /// ?꾩옱 RoadGroup ?덉뿉 ?ㅼ젣濡?諛곗튂??諛곗슦瑜??곗꽑 李얠뒿?덈떎.
    /// ?몄뒪?숉꽣 李몄“媛 鍮꾩뼱 ?덇굅???덉쟾 MFC瑜?媛由ъ폒?? 臾대? ?덉쓽 ??MFC瑜??ㅼ떆 ?↔린 ?꾪븳 ?덉쟾?μ튂?낅땲??
    /// </summary>
    private GameObject ResolveOwnedChild(GameObject assignedObject, params string[] childNameArray)
    {
        foreach (string childName in childNameArray)
        {
            GameObject childObject = OOTechSceneQuery.RequestChildObjectByName(transform, childName);
            Transform childTransform = childObject != null ? childObject.transform : null;

            if (childTransform != null)
                return childTransform.gameObject;

            GameObject recursiveChildObject = RequestChildObjectByName(transform, childName);

            if (recursiveChildObject != null)
                return recursiveChildObject;
        }

        if (assignedObject != null && assignedObject.transform != null && assignedObject.transform.IsChildOf(transform))
            return assignedObject;

        return assignedObject;
    }

    private GameObject ResolveBestMFCObject(GameObject assignedObject)
    {
        List<GameObject> candidateList = new List<GameObject>();

        if (assignedObject != null)
            candidateList.Add(assignedObject);

        Transform[] childTransformArray = GetComponentsInChildren<Transform>(true);

        foreach (Transform childTransform in childTransformArray)
        {
            if (childTransform == null || childTransform.name != "MFC")
                continue;

            if (!candidateList.Contains(childTransform.gameObject))
                candidateList.Add(childTransform.gameObject);
        }

        if (candidateList.Count == 0)
            return ResolveOwnedChild(assignedObject, "MFC");

        GameObject bestObject = candidateList[0];
        int bestPriority = GetMFCObjectResolvePriority(bestObject, assignedObject);

        for (int index = 1; index < candidateList.Count; index++)
        {
            GameObject candidateObject = candidateList[index];
            int priority = GetMFCObjectResolvePriority(candidateObject, assignedObject);

            if (priority <= bestPriority)
                continue;

            bestObject = candidateObject;
            bestPriority = priority;
        }

        return bestObject;
    }

    private int GetMFCObjectResolvePriority(GameObject candidateObject, GameObject assignedObject)
    {
        if (candidateObject == null)
            return int.MinValue;

        int priority = 0;

        if (candidateObject.transform != null && candidateObject.transform.IsChildOf(transform))
            priority += 1000;

        SpriteRenderer spriteRenderer = candidateObject.GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
            priority += 100;

        if (spriteRenderer != null && spriteRenderer.sprite != null)
            priority += 1000;

        Animator animator = candidateObject.GetComponentInChildren<Animator>(true);

        if (animator != null)
            priority += 50;

        if (animator != null && animator.runtimeAnimatorController != null)
            priority += 50;

        if (candidateObject.activeSelf)
            priority += 10;

        if (candidateObject == assignedObject)
            priority += 1;

        return priority;
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
    /// ?꾩옱 ?좏깮??諛곗슦 ?ㅻ툕?앺듃?먯꽌 而댄룷?뚰듃瑜??ㅼ떆 媛?몄샃?덈떎.
    /// 諛곗슦瑜?援먯껜?덉쓣 ???덉쟾 Animator/SpriteRenderer 李몄“媛 ?⑥? ?딅룄濡?留ㅻ쾲 ?뚯쑀 ?ㅻ툕?앺듃 湲곗??쇰줈 ?쎌뒿?덈떎.
    /// </summary>
    private T ResolveOwnedComponent<T>(GameObject targetObject) where T : Component
    {
        return targetObject != null ? targetObject.GetComponentInChildren<T>(true) : null;
    }
}
