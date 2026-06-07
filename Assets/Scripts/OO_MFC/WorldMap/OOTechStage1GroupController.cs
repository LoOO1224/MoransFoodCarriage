// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechStage1GroupController.cs
// - ??븷: Stage1Group???먯떆??吏꾪뻾??留〓뒗 Controller?낅땲??
// - ?곹솕 鍮꾩쑀: 臾대?媛먮룆? "1留?諛곌꼍 耳쒓린, 珥뚯옣 ?깆옣, ?섎젅 ?좏깮吏, ?꾨Т ?꾨즺" ?쒖꽌留?吏?섑빀?덈떎.
// - ?좎?蹂댁닔 ?ъ씤?? ?쒖떇? Marker, ?곹샇?묒슜 ?먯젙? Actor, ????좏깮吏/?꾨Т 臾몄옣? DataManager??留↔퉩?덈떎.
// =============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Stage1Group??1-1, 1-2, 1-3 ?λ㈃ ?대룞怨?珥뚯옣/?섎젅 ?곹샇?묒슜??吏?섑빀?덈떎.
/// Game View?먯꽌??Moran??醫뚯슦濡??대룞?섍퀬, 珥뚯옣怨??섎젅 ?꾩쓽 ?몃? ?몃え瑜??곕씪媛 ?곹샇?묒슜?⑸땲??
/// </summary>
[DisallowMultipleComponent]
public class OOTechStage1GroupController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string _stageDataId = "OO_Stage_1";
    [SerializeField] private string _stageQuestDataId = "Stage1_Quest_01";
    [SerializeField] private string _currentGroupName = "Stage1Group";

    [Header("Scene Object Names")]
    [SerializeField] private string _stageMap1Name = "Stage1-1";
    [SerializeField] private string _stageMap2Name = "Stage1-2";
    [SerializeField] private string _stageMap3Name = "Stage1-3";
    [SerializeField] private string _moranObjectName = "Moran";
    [SerializeField] private string _villageChiefObjectName = "VillageChief";
    [SerializeField] private string _cartObjectName = "Cart";
    [SerializeField] private string _stageNameObjectName = "StageName";
    [SerializeField] private string _placeholderCanvasName = "Canvas_StagePlaceholder";

    [Header("Moran Movement")]
    [SerializeField] private KeyCode _moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode _moveRightKey = KeyCode.D;
    [SerializeField] private KeyCode _runKey = KeyCode.LeftShift;
    [SerializeField] private float _walkSpeed = 2.2f;
    [SerializeField] private float _runSpeed = 4.8f;
    [SerializeField] private float _leftStartMargin = 0.65f;
    [SerializeField] private float _edgeExitMargin = 0.1f;
    [SerializeField] private float _moranLaneNormalizedHeight = 0.2f;

    [Header("Animation State")]
    [SerializeField] private string _idleStateName = "Moran_idle";
    [SerializeField] private string _walkingStateName = "Moran_isWalking";
    [SerializeField] private string _runningStateName = "Moran_isRunning";
    [SerializeField] private string _victoryStateName = "Moran_Victory";
    [SerializeField] private string _victoryTriggerName = "MoranVictory";
    [SerializeField] private float _walkingAnimationSpeed = 0.75f;
    [SerializeField] private float _runningAnimationSpeed = 1.5f;

    [Header("Transition")]
    [SerializeField] private float _fadeOutSeconds = 0.35f;
    [SerializeField] private float _blackHoldSeconds = 0.18f;
    [SerializeField] private float _fadeInSeconds = 0.4f;
    [SerializeField] private float _harvestTimePassSeconds = 0.85f;

    [Header("Stage Title")]
    [SerializeField] private float _stageTitleFadeSeconds = 0.6f;
    [SerializeField] private float _stageTitleHoldSeconds = 1.1f;

    [Header("BGM")]
    [SerializeField] private AudioClip _stage1BGM;
#if UNITY_EDITOR
    [SerializeField] private string _stage1BGMAssetPath = "Assets/Sounds/BGM/Stage1Group_BGM.mp3";
#endif

    [Header("Entry Tutorial")]
    [SerializeField] private string _tutorialGuideGroupName = "TutorialGuideGroup";
    [SerializeField] private string _entryTutorialId = "narration_tutorial_13";
    [SerializeField] private float _entryTutorialVisibleCheckSeconds = 0.35f;
    [SerializeField] private int _tutorialCanvasSortingOrder = 36000;

    [Header("Dialogue")]
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private string _villageChiefDialogueId = "character_VillageChief_01";
    [SerializeField] private string _villageChiefFinalDialogueId = "character_VillageChief_02";
    [SerializeField] private string _cartChoiceId = "Choice_Stage1_CartPumpkin_01";

    [Header("Stage Quest")]
    [SerializeField] private string _villageChiefNextQuestDataId = "Stage1_Quest_02";
    [SerializeField] private string _pumpkinCookingQuestDataId = "Stage1_Quest_03";
    [SerializeField] private string _returnChiefQuestDataId = "Stage1_Quest_04";
    [SerializeField] private string _pumpkinCookingQuestFallbackText = "호박과 쌀로 호박죽 10그릇을 만드세요.";
    [SerializeField] private string _returnChiefQuestFallbackText = "호박죽 10그릇을 완성했습니다. 동쪽 마을의 촌장에게 돌아가세요.";

    [Header("Inventory Item")]
    [SerializeField] private string _pumpkinIngredientId = "Ing_Pumpkin_01";
    [SerializeField] private string _pumpkinSoupCookId = "OO_PumpkinSoup_1";
    [SerializeField] private string _chiliPepperIngredientId = "Ing_ChiliPepper_01";
    [SerializeField] private string _kimchIngredientId = "Ing_Kimch_01";
    [SerializeField] private int _pumpkinRewardCount = 10;
    [SerializeField] private int _requiredPumpkinSoupCount = 10;
    [SerializeField] private int _chiefRewardCount = 10;

    [Header("Interaction")]
    [SerializeField] private KeyCode _interactionKey = KeyCode.E;
    [SerializeField] private float _villageChiefInteractionDistance = 5.5f;
    [SerializeField] private float _cartInteractionDistance = 4.4f;
    [SerializeField] private Vector3 _villageChiefMarkerOffset = new Vector3(0f, 0.95f, 0f);
    [SerializeField] private Vector3 _cartMarkerOffset = new Vector3(0f, 1.1f, 0f);
    [SerializeField] private string _markerText = "\u25BC";
    [SerializeField] private Color _villageChiefMarkerColor = Color.red;
    [SerializeField] private Color _cartMarkerColor = Color.red;

    [Header("Stage Clear")]
    [SerializeField] private string _stageClearTitle = "동쪽 마을 임무 완수";
    [SerializeField] private string _stageClearMessage = "촌장이 감사의 뜻으로 청양고추 10개와 김치 10개를 건넸습니다. 다음 길로 나설 준비가 끝났습니다.";
    [SerializeField] private string _nextRoadGroupName = "2nd_Road_to_Stage2";

    private GameObject Object_StageMap1;
    private GameObject Object_StageMap2;
    private GameObject Object_StageMap3;
    private GameObject Object_Moran;
    private GameObject Object_VillageChief;
    private GameObject Object_Cart;
    private SpriteRenderer Renderer_Moran;
    private Animator Animator_Moran;
    private TextMeshProUGUI Text_StageName;
    private OOTechNPCInteractionActor Actor_VillageChief;
    private OOTechNPCInteractionActor Actor_Cart;
    private OOTechWorldInteractionMarker Marker_VillageChief;
    private OOTechWorldInteractionMarker Marker_Cart;
    private OOTechTutorialGuideUI UI_TutorialGuide;
    private DialogueUI UI_Dialogue;
    private OOTechRoadHUDController HUD_Road;
    private Camera Camera_Main;
    private CameraFollowController Camera_Follow;
    private Canvas Canvas_Fade;
    private Image Image_Fade;
    private Canvas Canvas_Clear;
    private Button Button_NextRoad;
    private Canvas Canvas_EntryTutorialFallback;
    private TextMeshProUGUI Text_EntryTutorialFallbackTitle;
    private TextMeshProUGUI Text_EntryTutorialFallbackBody;
    private Coroutine Coroutine_EntryTutorial;
    private Coroutine Coroutine_Stage1BGM;
    private int _currentMapIndex;
    private bool _isChangingMap;
    private bool _isInputLocked;
    private bool _isDialoguePlaying;
    private bool _isCartChoiceComplete;
    private bool _isPumpkinSoupReady;
    private bool _isVillageChiefFinalRewardComplete;
    private bool _isStageClearSequencePlaying;
    private bool _isEntryTutorialFinished;
    private bool _hasSavedCameraFollowState;
    private bool _savedCameraFollowEnabled;
    private string _currentAnimationStateName;

    /// <summary>
    /// Stage1Group??耳쒖?硫?泥?臾대?? 諛곗슦?ㅼ쓣 ?뺣젹?섍퀬 ?낆옣 ?쒗넗由ъ뼹???쒖옉?⑸땲??
    /// </summary>
    private void OnEnable()
    {
        ResolveReferences();
        RequestPlayStage1BGM();
        SaveAndDisableCameraFollow();
        DisablePlaceholderCanvas();
        PrepareHUDMission();
        PrepareStage();
        PrepareInteractionActors();
        PrepareInteractionMarkers();
        StartCoroutine(PlayStageNameRoutine());
        RequestShowEntryTutorial();
    }

    /// <summary>
    /// Stage1Group??爰쇱?硫?諛곗슦?ㅼ쓽 ?대깽???곌껐怨?移대찓???곹깭瑜??먯쐞移섎줈 ?뚮┰?덈떎.
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
        Coroutine_EntryTutorial = null;
        Coroutine_Stage1BGM = null;
        _isChangingMap = false;
        _isInputLocked = false;
        _isDialoguePlaying = false;
        _isEntryTutorialFinished = false;
        _isStageClearSequencePlaying = false;
        SetFadeAlpha(0f);
        SetClearPanelActive(false);
        CloseEntryTutorialFallback();
        PlayMoranState(_idleStateName, 1f, true);
        ReleaseInteractionActors();
        RestoreCameraFollow();
    }

    /// <summary>
    /// 留??꾨젅??Moran ?대룞, ?곹샇?묒슜 媛???곹깭, ?몃컯二??꾩꽦 議곌굔??媛깆떊?⑸땲??
    /// </summary>
    private void Update()
    {
        UpdateStageProgressByInventory();
        UpdateInteractionState();

        if (ShouldBlockPlayerInput())
        {
            if (!_isStageClearSequencePlaying)
                PlayMoranState(_idleStateName, 1f);

            return;
        }

        int direction = 0;

        if (Input.GetKey(_moveLeftKey))
            direction -= 1;

        if (Input.GetKey(_moveRightKey))
            direction += 1;

        if (direction == 0)
        {
            PlayMoranState(_idleStateName, 1f);
            return;
        }

        MoveMoran(direction, Input.GetKey(_runKey));
    }

    private void ResolveReferences()
    {
        Object_StageMap1 = RequestChildObjectByName(transform, _stageMap1Name);
        Object_StageMap2 = RequestChildObjectByName(transform, _stageMap2Name);
        Object_StageMap3 = RequestChildObjectByName(transform, _stageMap3Name);
        Object_Moran = RequestChildObjectByName(transform, _moranObjectName);
        Object_VillageChief = RequestChildObjectByName(transform, _villageChiefObjectName);
        Object_Cart = RequestChildObjectByName(transform, _cartObjectName);

        if (Object_Moran != null)
        {
            Renderer_Moran = Object_Moran.GetComponentInChildren<SpriteRenderer>(true);
            Animator_Moran = Object_Moran.GetComponentInChildren<Animator>(true);
        }

        HUD_Road = GetComponent<OOTechRoadHUDController>();
        Camera_Main = Camera_Main != null ? Camera_Main : Camera.main;

        if (Camera_Follow == null && Camera_Main != null)
            Camera_Main.TryGetComponent(out Camera_Follow);

        ResolveStageNameText();
        CreateFadeCanvasIfNeeded();
        CreateClearCanvasIfNeeded();
    }

    /// <summary>
    /// Stage1Group???대┫ ???꾩슜 BGM??SoundManager?먭쾶 ?붿껌?⑸땲??
    /// ?λ㈃ 諛곗슦媛 AudioSource瑜?吏곸젒 ?≪? ?딄퀬, ?뚰뼢 媛먮룆?먭쾶 ?뚯븙 ?먮쭔 ?꾨떖?섎뒗 援ъ“?낅땲??
    /// </summary>
    private void RequestPlayStage1BGM()
    {
        if (Coroutine_Stage1BGM != null)
            StopCoroutine(Coroutine_Stage1BGM);

        Coroutine_Stage1BGM = StartCoroutine(PlayStage1BGMRoutine());
    }

    private IEnumerator PlayStage1BGMRoutine()
    {
        AudioClip bgmClip = ResolveStage1BGMClip();
        int retryCount = 0;

        while (OOTechSoundManager.Inst == null && retryCount < 30)
        {
            retryCount++;
            yield return null;
        }

        if (OOTechSoundManager.Inst == null)
        {
            Debug.LogWarning("[OOTechStage1GroupController] Stage1 BGM failed. OOTechSoundManager is missing.");
            Coroutine_Stage1BGM = null;
            yield break;
        }

        if (bgmClip == null)
        {
            Debug.LogWarning("[OOTechStage1GroupController] Stage1Group_BGM clip is missing.");
            Coroutine_Stage1BGM = null;
            yield break;
        }

        OOTechSoundManager.Inst.PlayBGM(bgmClip, true);
        Debug.Log($"[OOTechStage1GroupController] Stage1Group_BGM requested: {bgmClip.name}");
        Coroutine_Stage1BGM = null;
    }

    private void StopStage1BGM()
    {
        AudioClip bgmClip = ResolveStage1BGMClip();

        if (OOTechSoundManager.Inst != null && bgmClip != null)
            OOTechSoundManager.Inst.StopBGM(bgmClip);
    }

    private AudioClip ResolveStage1BGMClip()
    {
        if (_stage1BGM != null)
            return _stage1BGM;

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(_stage1BGMAssetPath))
            return AssetDatabase.LoadAssetAtPath<AudioClip>(_stage1BGMAssetPath);
#endif

        return null;
    }

    private void ResolveStageNameText()
    {
        GameObject stageNameObject = RequestChildObjectByName(transform, _stageNameObjectName);

        if (stageNameObject == null)
            stageNameObject = RequestSceneObjectByName(_stageNameObjectName);

        Text_StageName = stageNameObject != null ? stageNameObject.GetComponentInChildren<TextMeshProUGUI>(true) : null;

        if (Text_StageName != null)
            Text_StageName.gameObject.SetActive(false);
    }

    private void DisablePlaceholderCanvas()
    {
        GameObject placeholderCanvas = RequestChildObjectByName(transform, _placeholderCanvasName);

        if (placeholderCanvas != null)
            placeholderCanvas.SetActive(false);
    }

    private void PrepareHUDMission()
    {
        if (HUD_Road == null)
            return;

        HUD_Road.SetOwnerGroupName(_currentGroupName);
        HUD_Road.PrepareHUD();
        HUD_Road.SetCookingUnlocked(true);
        HUD_Road.SetHUDVisible(true);
        RequestUpdateStageQuestWithFallback(_stageQuestDataId, "동쪽 마을의 촌장을 만나세요.");
    }

    private void PrepareStage()
    {
        _currentMapIndex = 0;
        SetCurrentMapActive();
        SetMoranActive(true);
        PlaceMoranAtMapEntry(false);
        FocusCameraOnCurrentMap();
        PlayMoranState(_idleStateName, 1f, true);
        SetFadeAlpha(0f);
        SetClearPanelActive(false);
    }

    /// <summary>
    /// NPC? ?섎젅 諛곗슦?먭쾶 ?곹샇?묒슜 ??븷?쒕? 遺숈엯?덈떎.
    /// ?쒖떇? 蹂꾨룄 Marker媛 留↔린 ?뚮Ц??Actor?먮뒗 E 踰꾪듉 ?ㅻ툕?앺듃瑜??곌껐?섏? ?딆뒿?덈떎.
    /// </summary>
    private void PrepareInteractionActors()
    {
        Actor_VillageChief = PrepareActor(Object_VillageChief);

        if (Actor_VillageChief != null)
        {
            Actor_VillageChief.InteractionRequested -= OnVillageChiefInteractionRequested;
            Actor_VillageChief.InteractionRequested += OnVillageChiefInteractionRequested;
            Actor_VillageChief.RequestSetup(Object_Moran != null ? Object_Moran.transform : null, null);
            Actor_VillageChief.RequestSetInteractionData(_villageChiefDialogueId, _villageChiefNextQuestDataId);
            Actor_VillageChief.RequestSetInteractionRule(_villageChiefInteractionDistance, Vector3.zero, _interactionKey);
            Actor_VillageChief.RequestSetOneShot(false);
            Actor_VillageChief.RequestSetInteractable(false);
        }

        Actor_Cart = PrepareActor(Object_Cart);

        if (Actor_Cart != null)
        {
            Actor_Cart.InteractionRequested -= OnCartInteractionRequested;
            Actor_Cart.InteractionRequested += OnCartInteractionRequested;
            Actor_Cart.RequestSetup(Object_Moran != null ? Object_Moran.transform : null, null);
            Actor_Cart.RequestSetInteractionData(string.Empty, string.Empty);
            Actor_Cart.RequestSetInteractionRule(_cartInteractionDistance, Vector3.zero, _interactionKey);
            Actor_Cart.RequestSetOneShot(true);
            Actor_Cart.RequestSetInteractable(false);
        }
    }

    private OOTechNPCInteractionActor PrepareActor(GameObject actorObject)
    {
        if (actorObject == null)
            return null;

        OOTechNPCInteractionActor actor = actorObject.GetComponent<OOTechNPCInteractionActor>();

        if (actor == null)
            Debug.LogWarning($"[OOTechStage1GroupController] {actorObject.name} needs OOTechNPCInteractionActor.");

        return actor;
    }

    private void ReleaseInteractionActors()
    {
        if (Actor_VillageChief != null)
        {
            Actor_VillageChief.InteractionRequested -= OnVillageChiefInteractionRequested;
            Actor_VillageChief.RequestSetInteractable(false);
        }

        if (Actor_Cart != null)
        {
            Actor_Cart.InteractionRequested -= OnCartInteractionRequested;
            Actor_Cart.RequestSetInteractable(false);
        }
    }

    private void PrepareInteractionMarkers()
    {
        _villageChiefMarkerColor = Color.red;
        _cartMarkerColor = Color.red;
        Marker_VillageChief = PrepareMarker("Marker_VillageChief_InteractionArrow", Object_VillageChief, _villageChiefMarkerOffset, _villageChiefMarkerColor);
        Marker_Cart = PrepareMarker("Marker_Cart_InteractionArrow", Object_Cart, _cartMarkerOffset, _cartMarkerColor);
        UpdateMarkerVisibility();
    }

    private OOTechWorldInteractionMarker PrepareMarker(string markerObjectName, GameObject targetObject, Vector3 worldOffset, Color markerColor)
    {
        if (targetObject == null)
            return null;

        GameObject markerObject = RequestChildObjectByName(transform, markerObjectName);

        if (markerObject == null)
            markerObject = RequestSceneObjectByName(markerObjectName);

        if (markerObject == null)
        {
            Debug.LogWarning($"[OOTechStage1GroupController] {markerObjectName} scene marker is missing.");
            return null;
        }

        OOTechWorldInteractionMarker marker = markerObject.GetComponent<OOTechWorldInteractionMarker>();

        if (marker == null)
        {
            Debug.LogWarning($"[OOTechStage1GroupController] {markerObjectName} needs OOTechWorldInteractionMarker.");
            return null;
        }

        marker.RequestSetup(targetObject.transform, worldOffset, _markerText, markerColor);
        marker.RequestSetVisible(false);
        return marker;
    }

    private void UpdateInteractionState()
    {
        bool canInteract = !ShouldBlockPlayerInput();

        if (Actor_VillageChief != null)
            Actor_VillageChief.RequestSetInteractable(canInteract && _currentMapIndex == 0 && !_isVillageChiefFinalRewardComplete);

        if (Actor_Cart != null)
            Actor_Cart.RequestSetInteractable(canInteract && _currentMapIndex == 1 && !_isCartChoiceComplete);

        UpdateMarkerVisibility();
    }

    private void UpdateMarkerVisibility()
    {
        if (Marker_VillageChief != null)
            Marker_VillageChief.RequestSetVisible(_currentMapIndex == 0);

        if (Marker_Cart != null)
            Marker_Cart.RequestSetVisible(_currentMapIndex == 1 && !_isCartChoiceComplete);
    }

    private bool ShouldBlockPlayerInput()
    {
        if (_isChangingMap || _isInputLocked || _isDialoguePlaying || _isStageClearSequencePlaying || Object_Moran == null)
            return true;

        if (OOTechUIManager.Inst == null)
            return false;

        return OOTechUIManager.Inst.IsOpenedUI("CookingGroup") ||
               OOTechUIManager.Inst.IsOpenedUI("WorldMapGroup") ||
               OOTechUIManager.Inst.IsOpenedUI("CodexGroup");
    }

    private void RequestShowEntryTutorial()
    {
        if (Coroutine_EntryTutorial != null)
            StopCoroutine(Coroutine_EntryTutorial);

        Coroutine_EntryTutorial = StartCoroutine(ShowEntryTutorialRoutine());
    }

    /// <summary>
    /// Stage1 ?낆옣 ?쒗넗由ъ뼹???곗씠??湲곕컲?쇰줈 ?꾩슦怨? ?ㅽ뙣?섎㈃ ?泥??덈궡?먯쓣 ?꾩썎?덈떎.
    /// 媛먮룆 鍮꾩쑀濡쒕뒗 ?뺤떇 ?덈궡 諛곗슦媛 臾대???紐??щ씪?ㅻ㈃ ?덈퉬 ?덈궡?먯쓣 利됱떆 ?몄썙 怨듭뿰??硫덉텛吏 ?딅뒗 ?μ튂?낅땲??
    /// </summary>
    private IEnumerator ShowEntryTutorialRoutine()
    {
        _isEntryTutorialFinished = false;
        _isInputLocked = true;
        UpdateInteractionState();

        Debug.Log($"[OOTechStage1GroupController] Stage1 entered. Entry tutorial request: {_entryTutorialId}");

        OO_Tutorial tutorialData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetTutorialData(_entryTutorialId) : null;

        if (tutorialData == null)
        {
            Debug.LogWarning($"[OOTechStage1GroupController] Tutorial data is missing: {_entryTutorialId}");
            ShowEntryTutorialFallback("달리기", "Shift를 누른 채 이동하면 달립니다!");
            yield break;
        }

        UI_TutorialGuide = ResolveTutorialGuideUI();

        if (UI_TutorialGuide == null)
        {
            Debug.LogWarning("[OOTechStage1GroupController] TutorialGuideGroup is missing.");
            ShowEntryTutorialFallback(tutorialData.Title, tutorialData.Description);
            yield break;
        }

        ForceTutorialGuideVisible(UI_TutorialGuide.gameObject);
        UI_TutorialGuide.ShowGuide(tutorialData, FinishEntryTutorial);
        ForceTutorialGuideVisible(UI_TutorialGuide.gameObject);
        Debug.Log($"[OOTechStage1GroupController] TutorialGuideGroup opened: {_entryTutorialId}");

        if (_entryTutorialVisibleCheckSeconds > 0f)
            yield return new WaitForSeconds(_entryTutorialVisibleCheckSeconds);

        if (!_isEntryTutorialFinished && !IsTutorialGuideActuallyVisible())
        {
            Debug.Log("[OOTechStage1GroupController] TutorialGuideGroup was opened but is not visible. Fallback guide is shown.");
            ShowEntryTutorialFallback(tutorialData.Title, tutorialData.Description);
        }
    }

    private void FinishEntryTutorial()
    {
        if (_isEntryTutorialFinished)
            return;

        _isEntryTutorialFinished = true;
        _isInputLocked = false;
        CloseEntryTutorialFallback();
        UpdateInteractionState();
        Debug.Log("[OOTechStage1GroupController] Entry tutorial finished. Player control unlocked.");
    }

    private OOTechTutorialGuideUI ResolveTutorialGuideUI()
    {
        GameObject tutorialGroup = null;

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.OpenUI(_tutorialGuideGroupName);
            tutorialGroup = OOTechUIManager.Inst.GetCreatedUI(_tutorialGuideGroupName);
        }

        if (tutorialGroup == null)
            tutorialGroup = RequestSceneObjectByName(_tutorialGuideGroupName);

        if (tutorialGroup == null)
            return null;

        tutorialGroup.SetActive(true);
        ForceTutorialGuideVisible(tutorialGroup);
        return tutorialGroup.GetComponentInChildren<OOTechTutorialGuideUI>(true);
    }

    /// <summary>
    /// TutorialGuideGroup???ㅻⅨ UI ?ㅼ뿉 臾삵엳吏 ?딅룄濡?Canvas ?곗꽑?쒖쐞瑜??щ┰?덈떎.
    /// Game View?먯꽌???낆옣 ?덈궡媛 HUD? 諛곌꼍蹂대떎 ?욎뿉??蹂댁씠寃??⑸땲??
    /// </summary>
    private void ForceTutorialGuideVisible(GameObject tutorialGroup)
    {
        if (tutorialGroup == null)
            return;

        tutorialGroup.SetActive(true);

        Canvas[] canvasArray = tutorialGroup.GetComponentsInChildren<Canvas>(true);

        foreach (Canvas canvas in canvasArray)
        {
            if (canvas == null)
                continue;

            canvas.gameObject.SetActive(true);
            canvas.overrideSorting = true;
            canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, _tutorialCanvasSortingOrder);
        }

        CanvasGroup[] canvasGroupArray = tutorialGroup.GetComponentsInChildren<CanvasGroup>(true);

        foreach (CanvasGroup canvasGroup in canvasGroupArray)
        {
            if (canvasGroup == null)
                continue;

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        GraphicRaycaster[] raycasterArray = tutorialGroup.GetComponentsInChildren<GraphicRaycaster>(true);

        foreach (GraphicRaycaster raycaster in raycasterArray)
        {
            if (raycaster != null)
                raycaster.enabled = true;
        }
    }

    /// <summary>
    /// ?뺤떇 TutorialGuideGroup???ㅼ젣濡?Game View???쒖떆??以鍮꾧? ?먮뒗吏 ?뺤씤?⑸땲??
    /// false硫??낅젰 ?좉툑留??⑥? ?딅룄濡?fallback ?덈궡?먯쓣 耳?땲??
    /// </summary>
    private bool IsTutorialGuideActuallyVisible()
    {
        if (UI_TutorialGuide == null || !UI_TutorialGuide.gameObject.activeInHierarchy)
            return false;

        Canvas canvas = UI_TutorialGuide.GetComponentInParent<Canvas>();

        if (canvas != null && !canvas.gameObject.activeInHierarchy)
            return false;

        CanvasGroup canvasGroup = UI_TutorialGuide.GetComponentInParent<CanvasGroup>();

        if (canvasGroup != null && canvasGroup.alpha <= 0.01f)
            return false;

        return true;
    }

    /// <summary>
    /// ?뺤떇 ?쒗넗由ъ뼹 UI媛 ?ㅼ쥌?먯쓣 ?뚮쭔 ?ъ슜?섎뒗 ?덉쟾 ?덈궡?먯엯?덈떎.
    /// ?대┃?섎㈃ FinishEntryTutorial???몄텧?댁꽌 ?뚮젅?댁뼱媛 ?뺤? ?곹깭??媛뉙엳吏 ?딆뒿?덈떎.
    /// </summary>
    private void ShowEntryTutorialFallback(string title, string body)
    {
        CreateEntryTutorialFallbackIfNeeded();

        if (Text_EntryTutorialFallbackTitle != null)
            Text_EntryTutorialFallbackTitle.text = string.IsNullOrEmpty(title) ? "Stage1 Tutorial" : title;

        if (Text_EntryTutorialFallbackBody != null)
            Text_EntryTutorialFallbackBody.text = string.IsNullOrEmpty(body) ? "Click to start playing." : body;

        if (Canvas_EntryTutorialFallback != null)
            Canvas_EntryTutorialFallback.gameObject.SetActive(true);

        Debug.Log("[OOTechStage1GroupController] Fallback entry tutorial is visible. Click the panel to unlock player control.");
    }

    private void CreateEntryTutorialFallbackIfNeeded()
    {
        if (Canvas_EntryTutorialFallback != null)
            return;

        GameObject canvasObject = new GameObject("Canvas_Stage1EntryTutorialFallback");
        canvasObject.transform.SetParent(transform, false);

        Canvas_EntryTutorialFallback = canvasObject.AddComponent<Canvas>();
        Canvas_EntryTutorialFallback.renderMode = RenderMode.ScreenSpaceOverlay;
        Canvas_EntryTutorialFallback.overrideSorting = true;
        Canvas_EntryTutorialFallback.sortingOrder = _tutorialCanvasSortingOrder + 10;
        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;

        Image backgroundImage = canvasObject.AddComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.55f);

        Button fallbackButton = canvasObject.AddComponent<Button>();
        fallbackButton.transition = Selectable.Transition.None;
        fallbackButton.onClick.AddListener(FinishEntryTutorial);

        GameObject panelObject = new GameObject("Panel_EntryTutorialFallback");
        panelObject.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(960f, 360f);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.06f, 0.06f, 0.06f, 0.94f);

        Text_EntryTutorialFallbackTitle = CreateFallbackTutorialText(panelObject.transform, "Text_FallbackTitle", "Stage1 Tutorial", 42f, new Vector2(0f, 92f), new Vector2(860f, 70f));
        Text_EntryTutorialFallbackTitle.color = new Color(1f, 0.86f, 0.1f, 1f);
        Text_EntryTutorialFallbackBody = CreateFallbackTutorialText(panelObject.transform, "Text_FallbackBody", "Click to start playing.", 30f, new Vector2(0f, -28f), new Vector2(820f, 180f));
        Canvas_EntryTutorialFallback.gameObject.SetActive(false);
    }

    private TextMeshProUGUI CreateFallbackTutorialText(Transform parentTransform, string objectName, string text, float fontSize, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parentTransform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = anchoredPosition;
        textRect.sizeDelta = sizeDelta;

        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.textWrappingMode = TextWrappingModes.Normal;
        OOTechTMPFontUtility.ApplyProjectFont(textComponent);
        return textComponent;
    }

    private void CloseEntryTutorialFallback()
    {
        if (Canvas_EntryTutorialFallback != null)
            Canvas_EntryTutorialFallback.gameObject.SetActive(false);
    }

    private DialogueUI ResolveDialogueUI()
    {
        GameObject dialogueGroup = null;

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.OpenUI(_dialogueGroupName);
            dialogueGroup = OOTechUIManager.Inst.GetCreatedUI(_dialogueGroupName);
        }

        if (dialogueGroup == null)
            dialogueGroup = RequestSceneObjectByName(_dialogueGroupName);

        if (dialogueGroup == null)
            return null;

        dialogueGroup.SetActive(true);
        return dialogueGroup.GetComponentInChildren<DialogueUI>(true);
    }

    private void CloseDialogueUI()
    {
        if (OOTechUIManager.Inst != null && OOTechUIManager.Inst.CloseUI(_dialogueGroupName))
            return;

        GameObject dialogueGroup = RequestSceneObjectByName(_dialogueGroupName);

        if (dialogueGroup != null)
            dialogueGroup.SetActive(false);
    }

    private void OnVillageChiefInteractionRequested(OOTechNPCInteractionActor interactionActor)
    {
        if (_isDialoguePlaying)
            return;

        StartCoroutine(PlayVillageChiefDialogueRoutine());
    }

    /// <summary>
    /// 珥뚯옣怨???뷀빀?덈떎. 泥섏쓬?먮뒗 ?섎ː, ?몃컯二?10媛??댄썑?먮뒗 蹂댁긽怨?Stage1 ?꾨즺 ?곗텧濡?媛덈씪吏묐땲??
    /// </summary>
    private IEnumerator PlayVillageChiefDialogueRoutine()
    {
        _isInputLocked = true;
        _isDialoguePlaying = true;
        UpdateInteractionState();
        PlayMoranState(_idleStateName, 1f);

        bool isFinalTalk = _isPumpkinSoupReady && !_isVillageChiefFinalRewardComplete;
        string dialogueId = isFinalTalk ? _villageChiefFinalDialogueId : _villageChiefDialogueId;
        OO_Dialogue dialogueData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetDialogueData(dialogueId) : null;

        if (dialogueData != null)
            yield return ShowDialogueDataAndWait(dialogueData);
        else
            Debug.LogWarning($"[OOTechStage1GroupController] Dialogue data is missing: {dialogueId}");

        if (isFinalTalk)
            yield return CompleteVillageChiefFinalRewardRoutine();
        else
            RequestUpdateStageQuestWithFallback(_villageChiefNextQuestDataId, "촌장의 부탁을 듣고 마을에 필요한 음식을 준비하세요.");

        _isDialoguePlaying = false;
        _isInputLocked = false;
        UpdateInteractionState();
    }

    private void OnCartInteractionRequested(OOTechNPCInteractionActor interactionActor)
    {
        if (_isDialoguePlaying || _isCartChoiceComplete)
            return;

        StartCoroutine(PlayCartChoiceRoutine());
    }

    /// <summary>
    /// Stage1-2 ?섎젅 ?좏깮吏?낅땲?? ?덈? ?꾨Ⅴ硫??쒓컙???먮Ⅸ ?곗텧 ??Stage1-3?쇰줈 ?섏뼱媛怨??몃컯 10媛쒓? ?ㅼ뼱?듬땲??
    /// </summary>
    private IEnumerator PlayCartChoiceRoutine()
    {
        _isInputLocked = true;
        _isDialoguePlaying = true;
        UpdateInteractionState();
        PlayMoranState(_idleStateName, 1f);

        int selectedIndex = -1;
        OO_Choice choiceData = GetCartChoiceData();
        yield return ShowChoiceDataAndWait(choiceData, delegate (int index)
        {
            selectedIndex = index;
        });

        if (selectedIndex != 0)
        {
            if (Actor_Cart != null)
                Actor_Cart.RequestResetInteraction();

            _isDialoguePlaying = false;
            _isInputLocked = false;
            UpdateInteractionState();
            yield break;
        }

        float moranNormalizedX = CalculateMoranNormalizedMapX();

        yield return FadeRoutine(0f, 1f, _fadeOutSeconds);
        yield return new WaitForSeconds(_harvestTimePassSeconds);

        _isCartChoiceComplete = true;
        _currentMapIndex = 2;
        SetCurrentMapActive();
        PlaceMoranAtMapNormalizedX(moranNormalizedX);
        FocusCameraOnCurrentMap();
        AddInventoryItem(_pumpkinIngredientId, _pumpkinRewardCount);
        RequestUpdateStageQuestWithFallback(_pumpkinCookingQuestDataId, _pumpkinCookingQuestFallbackText);

        yield return FadeRoutine(1f, 0f, _fadeInSeconds);

        _isDialoguePlaying = false;
        _isInputLocked = false;
        UpdateInteractionState();
    }

    private OO_Choice GetCartChoiceData()
    {
        OO_Choice choiceData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetChoiceData(_cartChoiceId) : null;

        if (choiceData != null)
            return choiceData;

        return CreateFallbackCartChoiceData();
    }

    private OO_Choice CreateFallbackCartChoiceData()
    {
        return new OO_Choice
        {
            Id = _cartChoiceId,
            SpeakerId = "character_narrator_01",
            SpeakerName = ResolveSpeakerName("character_narrator_01", "나레이션"),
            PromptText = "탐관오리처럼 탐욕스럽게 생긴 호박들입니다. 호박을 수확하시겠습니까?",
            ChoiceMode = "YesNo",
            OptionCount = 2,
            OptionKeyList = new List<string> { "Y", "N" },
            OptionTextList = new List<string> { "예", "아니오" },
            ResultTypeList = new List<string> { "AddItem", "Close" },
            ResultValueList = new List<string> { _pumpkinIngredientId, string.Empty },
            ResultCountList = new List<int> { _pumpkinRewardCount, 0 },
            StageQuestIdList = new List<string> { _pumpkinCookingQuestDataId, string.Empty }
        };
    }

    private IEnumerator ShowDialogueDataAndWait(OO_Dialogue dialogueData)
    {
        UI_Dialogue = ResolveDialogueUI();

        if (UI_Dialogue == null)
            yield break;

        bool isDone = false;
        UI_Dialogue.RequestRoadViewLayout();
        UI_Dialogue.ShowDialogue(dialogueData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
        CloseDialogueUI();
    }

    private IEnumerator ShowChoiceDataAndWait(OO_Choice choiceData, Action<int> onChoiceSelected)
    {
        UI_Dialogue = ResolveDialogueUI();

        if (UI_Dialogue == null)
            yield break;

        bool isDone = false;
        int selectedIndex = -1;
        UI_Dialogue.RequestRoadViewLayout();
        UI_Dialogue.ShowChoice(choiceData, delegate (int index)
        {
            selectedIndex = index;
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
        CloseDialogueUI();
        onChoiceSelected?.Invoke(selectedIndex);
    }

    private IEnumerator CompleteVillageChiefFinalRewardRoutine()
    {
        if (!RequestExchangePumpkinSoupForChiefReward())
        {
            Debug.LogWarning("[OOTechStage1GroupController] Stage1 reward exchange failed. Pumpkin soup count is not enough.");
            yield break;
        }

        _isVillageChiefFinalRewardComplete = true;
        UpdateInteractionState();

        yield return PlayStageClearSequenceRoutine();
    }

    private IEnumerator PlayStageClearSequenceRoutine()
    {
        if (_isStageClearSequencePlaying)
            yield break;

        _isStageClearSequencePlaying = true;
        if (OOTechGameManager.Inst != null)
            OOTechGameManager.Inst.MarkStageCleared("Stage1");

        StartCoroutine(PlayMoranVictoryRoutine());
        SetClearPanelActive(true);
    }

    private IEnumerator PlayMoranVictoryRoutine()
    {
        float clipLength = GetMoranAnimationClipLength(_victoryStateName, 0.65f);

        while (_isStageClearSequencePlaying && gameObject.activeInHierarchy)
        {
            if (!PlayMoranState(_victoryStateName, 1f, true))
            {
                Debug.LogWarning($"[OOTechStage1GroupController] Moran victory state is missing: {_victoryStateName}");
                yield break;
            }

            yield return new WaitForSeconds(clipLength);
        }
    }

    private void AddInventoryItem(string itemDataId, int count)
    {
        if (OOTechGameManager.Inst == null || string.IsNullOrEmpty(itemDataId))
            return;

        OOTechGameManager.Inst.AddItem(itemDataId, Mathf.Max(1, count));

        if (HUD_Road != null)
        {
            HUD_Road.RequestRefreshInventoryView();
            HUD_Road.SetInventoryNewBadgeActive(true);
        }
    }

    /// <summary>
    /// 珥뚯옣?먭쾶 ?몃컯二?10媛쒕? ?섍린怨?Stage2 ?붾━???꾩슂??泥?뼇怨좎텛? 源移섎? 諛쏆뒿?덈떎.
    /// ?곹솕濡?移섎㈃ 諛곗슦媛 ?뚰뭹 ?대떦?먭쾶 ?꾩꽦???뚯떇 ?뚰뭹??諛섎궔?섍퀬, ?ㅼ쓬 ?λ㈃ ?뚰뭹??諛쏆븘 媛??援먰솚 ?먯엯?덈떎.
    /// </summary>
    private bool RequestExchangePumpkinSoupForChiefReward()
    {
        if (OOTechGameManager.Inst == null)
            return false;

        if (OOTechGameManager.Inst.GetItemCount(_pumpkinSoupCookId) < _requiredPumpkinSoupCount)
            return false;

        if (!OOTechGameManager.Inst.RemoveItem(_pumpkinSoupCookId, _requiredPumpkinSoupCount))
            return false;

        AddInventoryItemToTargetCount(_chiliPepperIngredientId, _chiefRewardCount);
        AddInventoryItemToTargetCount(_kimchIngredientId, _chiefRewardCount);

        if (HUD_Road != null)
        {
            HUD_Road.RequestRefreshInventoryView();
            HUD_Road.SetInventoryNewBadgeActive(true);
        }

        Debug.Log("[OOTechStage1GroupController] Pumpkin soup exchanged for chili pepper and kimchi rewards.");
        return true;
    }

    /// <summary>
    /// 蹂댁긽 ?꾩씠?쒖씠 ?대? ?쇰? ?ㅼ뼱? ?덉뼱??紐⑺몴 ?섎웾源뚯?留?梨꾩썎?덈떎.
    /// 媛숈? ?λ㈃??諛섎났 ?ㅽ뻾?섏뼱??蹂댁긽??以묐났?쇰줈 遺덉뼱?섎뒗 ?쇱쓣 留됯린 ?꾪븳 ?덉쟾?μ튂?낅땲??
    /// </summary>
    private void AddInventoryItemToTargetCount(string itemDataId, int targetCount)
    {
        if (OOTechGameManager.Inst == null || string.IsNullOrEmpty(itemDataId))
            return;

        int safeTargetCount = Mathf.Max(1, targetCount);
        int currentCount = OOTechGameManager.Inst.GetItemCount(itemDataId);
        int addCount = safeTargetCount - currentCount;

        if (addCount <= 0)
            return;

        AddInventoryItem(itemDataId, addCount);
    }

    private void UpdateStageProgressByInventory()
    {
        if (!_isCartChoiceComplete || _isPumpkinSoupReady || OOTechGameManager.Inst == null)
            return;

        if (OOTechGameManager.Inst.GetItemCount(_pumpkinSoupCookId) < _requiredPumpkinSoupCount)
            return;

        _isPumpkinSoupReady = true;
        RequestUpdateStageQuestWithFallback(_returnChiefQuestDataId, _returnChiefQuestFallbackText);
        UpdateInteractionState();
    }

    private void RequestUpdateStageQuestWithFallback(string stageQuestDataId, string fallbackText)
    {
        if (string.IsNullOrEmpty(stageQuestDataId))
            return;

        _stageQuestDataId = stageQuestDataId;
        OO_StageQuest questData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStageQuestData(_stageQuestDataId) : null;
        string questText = questData != null && !string.IsNullOrEmpty(questData.Description) ? questData.Description : fallbackText;

        if (HUD_Road != null)
        {
            HUD_Road.RequestSetStageQuestMission(questText);
            HUD_Road.SetMissionNewBadgeActive(true);
        }
    }

    private void MoveMoran(int direction, bool isRunning)
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (mapRenderer == null || Object_Moran == null)
            return;

        float speed = isRunning ? _runSpeed : _walkSpeed;
        Vector3 position = Object_Moran.transform.position;
        position.x += direction * speed * Time.deltaTime;
        position.y = CalculateLaneY(mapRenderer);

        float leftEdgeX = mapRenderer.bounds.min.x + _edgeExitMargin;
        float rightEdgeX = mapRenderer.bounds.max.x - _edgeExitMargin;
        float leftClampX = mapRenderer.bounds.min.x + _leftStartMargin;
        float rightClampX = mapRenderer.bounds.max.x - _leftStartMargin;

        if (_currentMapIndex == 0)
            position.x = Mathf.Max(position.x, leftClampX);

        if ((_currentMapIndex == 1 || _currentMapIndex == 2) && direction > 0)
            position.x = Mathf.Min(position.x, rightClampX);

        Object_Moran.transform.position = position;

        if (Renderer_Moran != null)
            Renderer_Moran.flipX = direction < 0;

        PlayMoranState(isRunning ? _runningStateName : _walkingStateName, isRunning ? _runningAnimationSpeed : _walkingAnimationSpeed);

        if (_currentMapIndex == 0 && position.x >= rightEdgeX)
            StartCoroutine(ChangeMapRoutine(1, false));
        else if (_currentMapIndex == 1 && direction < 0 && position.x <= leftEdgeX)
            StartCoroutine(ChangeMapRoutine(0, true));
        else if (_currentMapIndex == 2 && direction < 0 && position.x <= leftEdgeX)
            StartCoroutine(ChangeMapRoutine(0, true));
    }

    private IEnumerator ChangeMapRoutine(int nextMapIndex, bool isEnterFromRight)
    {
        if (_isChangingMap)
            yield break;

        _isChangingMap = true;
        UpdateInteractionState();
        PlayMoranState(_idleStateName, 1f);

        yield return FadeRoutine(0f, 1f, _fadeOutSeconds);

        if (_blackHoldSeconds > 0f)
            yield return new WaitForSeconds(_blackHoldSeconds);

        _currentMapIndex = nextMapIndex;
        SetCurrentMapActive();
        PlaceMoranAtMapEntry(isEnterFromRight);
        FocusCameraOnCurrentMap();

        yield return FadeRoutine(1f, 0f, _fadeInSeconds);
        _isChangingMap = false;
        UpdateInteractionState();
    }

    private void SetCurrentMapActive()
    {
        if (Object_StageMap1 != null)
            Object_StageMap1.SetActive(_currentMapIndex == 0);

        if (Object_StageMap2 != null)
            Object_StageMap2.SetActive(_currentMapIndex == 1);

        if (Object_StageMap3 != null)
            Object_StageMap3.SetActive(_currentMapIndex == 2);

        if (Object_VillageChief != null)
            Object_VillageChief.SetActive(_currentMapIndex == 0);

        UpdateMarkerVisibility();
    }

    private void PlaceMoranAtMapEntry(bool isEnterFromRight)
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (Object_Moran == null || mapRenderer == null)
            return;

        Vector3 position = Object_Moran.transform.position;
        position.x = isEnterFromRight ? mapRenderer.bounds.max.x - _leftStartMargin : mapRenderer.bounds.min.x + _leftStartMargin;
        position.y = CalculateLaneY(mapRenderer);
        Object_Moran.transform.position = position;
    }

    private float CalculateMoranNormalizedMapX()
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (Object_Moran == null || mapRenderer == null)
            return 0f;

        float minX = mapRenderer.bounds.min.x + _leftStartMargin;
        float maxX = mapRenderer.bounds.max.x - _leftStartMargin;

        if (Mathf.Approximately(minX, maxX))
            return 0f;

        return Mathf.Clamp01(Mathf.InverseLerp(minX, maxX, Object_Moran.transform.position.x));
    }

    private void PlaceMoranAtMapNormalizedX(float normalizedX)
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (Object_Moran == null || mapRenderer == null)
            return;

        Vector3 position = Object_Moran.transform.position;
        float minX = mapRenderer.bounds.min.x + _leftStartMargin;
        float maxX = mapRenderer.bounds.max.x - _leftStartMargin;
        position.x = Mathf.Lerp(minX, maxX, Mathf.Clamp01(normalizedX));
        position.y = CalculateLaneY(mapRenderer);
        Object_Moran.transform.position = position;
    }

    private float CalculateLaneY(SpriteRenderer mapRenderer)
    {
        return Mathf.Lerp(mapRenderer.bounds.min.y, mapRenderer.bounds.max.y, Mathf.Clamp01(_moranLaneNormalizedHeight));
    }

    private SpriteRenderer GetCurrentMapRenderer()
    {
        GameObject mapObject = Object_StageMap1;

        if (_currentMapIndex == 1)
            mapObject = Object_StageMap2;
        else if (_currentMapIndex == 2)
            mapObject = Object_StageMap3;

        return RequestBestMapRenderer(mapObject);
    }

    private SpriteRenderer RequestBestMapRenderer(GameObject mapObject)
    {
        if (mapObject == null)
            return null;

        SpriteRenderer[] rendererArray = mapObject.GetComponentsInChildren<SpriteRenderer>(true);
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

            bestArea = area;
            bestRenderer = spriteRenderer;
        }

        return bestRenderer != null ? bestRenderer : mapObject.GetComponentInChildren<SpriteRenderer>(true);
    }

    private void FocusCameraOnCurrentMap()
    {
        if (Camera_Main == null)
            Camera_Main = Camera.main;

        SaveAndDisableCameraFollow();

        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (Camera_Main == null || mapRenderer == null)
            return;

        Camera_Main.orthographic = true;
        Bounds bounds = mapRenderer.bounds;
        Vector3 cameraPosition = bounds.center;
        cameraPosition.z = Camera_Main.transform.position.z;
        Camera_Main.transform.position = cameraPosition;

        float aspect = Mathf.Max(0.01f, Camera_Main.aspect);
        float sizeByHeight = bounds.extents.y;
        float sizeByWidth = bounds.extents.x / aspect;
        Camera_Main.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth) * 1.02f;
    }

    private void SaveAndDisableCameraFollow()
    {
        if (Camera_Main == null)
            Camera_Main = Camera.main;

        if (Camera_Follow == null && Camera_Main != null)
            Camera_Main.TryGetComponent(out Camera_Follow);

        if (Camera_Follow == null)
            return;

        if (!_hasSavedCameraFollowState)
        {
            _savedCameraFollowEnabled = Camera_Follow.enabled;
            _hasSavedCameraFollowState = true;
        }

        Camera_Follow.enabled = false;
    }

    private void RestoreCameraFollow()
    {
        if (!_hasSavedCameraFollowState || Camera_Follow == null)
            return;

        Camera_Follow.enabled = _savedCameraFollowEnabled;
        _hasSavedCameraFollowState = false;
    }

    private void SetMoranActive(bool isActive)
    {
        if (Object_Moran != null)
            Object_Moran.SetActive(isActive);
    }

    private bool PlayMoranState(string stateName, float speed, bool isForceReplay = false)
    {
        if (Animator_Moran == null || string.IsNullOrEmpty(stateName) || !Animator_Moran.gameObject.activeInHierarchy)
            return false;

        string finalStateName = HasMoranState(stateName) ? stateName : _idleStateName;

        if (!HasMoranState(finalStateName))
            return false;

        Animator_Moran.speed = speed;

        if (!isForceReplay && _currentAnimationStateName == finalStateName)
            return true;

        _currentAnimationStateName = finalStateName;

        if (finalStateName == _victoryStateName)
            RequestSetMoranTrigger(_victoryTriggerName);

        Animator_Moran.Play(finalStateName, 0, 0f);
        return true;
    }

    /// <summary>
    /// Animator Controller??Victory Trigger媛 ?덉쑝硫??④퍡 諛쒕룞?⑸땲??
    /// Game View?먯꽌??吏곸젒 Play? Transition Trigger瑜?媛숈씠 ?⑥꽌 Victory媛 Idle????씠吏 ?딄쾶 ?⑸땲??
    /// </summary>
    private void RequestSetMoranTrigger(string triggerName)
    {
        if (Animator_Moran == null || string.IsNullOrEmpty(triggerName))
            return;

        foreach (AnimatorControllerParameter parameter in Animator_Moran.parameters)
        {
            if (parameter == null || parameter.name != triggerName || parameter.type != AnimatorControllerParameterType.Trigger)
                continue;

            Animator_Moran.ResetTrigger(triggerName);
            Animator_Moran.SetTrigger(triggerName);
            return;
        }
    }

    private bool HasMoranState(string stateName)
    {
        if (Animator_Moran == null || string.IsNullOrEmpty(stateName))
            return false;

        int stateHash = Animator.StringToHash(stateName);

        for (int layerIndex = 0; layerIndex < Animator_Moran.layerCount; layerIndex++)
        {
            if (Animator_Moran.HasState(layerIndex, stateHash))
                return true;
        }

        return false;
    }

    private float GetMoranAnimationClipLength(string clipName, float fallbackSeconds)
    {
        if (Animator_Moran == null || Animator_Moran.runtimeAnimatorController == null || string.IsNullOrEmpty(clipName))
            return fallbackSeconds;

        foreach (AnimationClip clip in Animator_Moran.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == clipName)
                return Mathf.Max(0.1f, clip.length);
        }

        return fallbackSeconds;
    }

    private IEnumerator PlayStageNameRoutine()
    {
        if (Text_StageName == null)
            yield break;

        OO_Stage stageData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStageData(_stageDataId) : null;
        Text_StageName.text = stageData != null && !string.IsNullOrEmpty(stageData.Name) ? stageData.Name : "동쪽 마을";
        Text_StageName.gameObject.SetActive(true);

        yield return FadeTextRoutine(0f, 1f, _stageTitleFadeSeconds);
        yield return new WaitForSeconds(_stageTitleHoldSeconds);
        yield return FadeTextRoutine(1f, 0f, _stageTitleFadeSeconds);

        Text_StageName.gameObject.SetActive(false);
    }

    private IEnumerator FadeTextRoutine(float fromAlpha, float toAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color color = Text_StageName.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(fromAlpha, toAlpha, elapsedTime / Mathf.Max(0.01f, duration));
            Text_StageName.color = color;
            yield return null;
        }

        color.a = toAlpha;
        Text_StageName.color = color;
    }

    private void CreateFadeCanvasIfNeeded()
    {
        if (Canvas_Fade != null)
            return;

        if (ResolveFadeCanvasFromScene())
        {
            SetFadeAlpha(0f);
            return;
        }

        GameObject canvasObject = new GameObject("Canvas_Stage1Fade");
        canvasObject.transform.SetParent(transform, false);
        Canvas_Fade = canvasObject.AddComponent<Canvas>();
        Canvas_Fade.renderMode = RenderMode.ScreenSpaceOverlay;
        Canvas_Fade.overrideSorting = true;
        Canvas_Fade.sortingOrder = 5000;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("Image_Fade");
        imageObject.transform.SetParent(canvasObject.transform, false);
        Image_Fade = imageObject.AddComponent<Image>();
        Image_Fade.color = Color.black;

        RectTransform imageRect = imageObject.transform as RectTransform;
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;
        SetFadeAlpha(0f);
    }

    /// <summary>
    /// Stage1 臾대???誘몃━ ?볦씤 ?섏씠???뚰뭹??李얠븘 ?곌껐?⑸땲??
    /// ?곹솕 鍮꾩쑀濡쒕뒗 ?붿쟾 議곕챸???덈줈 留뚮뱶??寃껋씠 ?꾨땲?? 議곕챸????ㅼ튂?????붿쟾 ?μ튂瑜??먯떆?몄뿉 ?곌껐?섎뒗 ?④퀎?낅땲??
    /// </summary>
    private bool ResolveFadeCanvasFromScene()
    {
        GameObject canvasObject = RequestChildObjectByName(transform, "Canvas_Stage1Fade");

        if (canvasObject == null)
            return false;

        Canvas_Fade = canvasObject.GetComponent<Canvas>();
        Image_Fade = canvasObject.GetComponentInChildren<Image>(true);

        if (Canvas_Fade == null || Image_Fade == null)
        {
            Canvas_Fade = null;
            Image_Fade = null;
            return false;
        }

        ApplyStageCanvas(Canvas_Fade, 5000);
        return true;
    }

    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            SetFadeAlpha(Mathf.Lerp(fromAlpha, toAlpha, elapsedTime / Mathf.Max(0.01f, duration)));
            yield return null;
        }

        SetFadeAlpha(toAlpha);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (Image_Fade == null)
            return;

        Color color = Image_Fade.color;
        color.a = alpha;
        Image_Fade.color = color;
        Image_Fade.raycastTarget = alpha > 0.01f;
    }

    private void CreateClearCanvasIfNeeded()
    {
        if (Canvas_Clear != null)
            return;

        if (ResolveClearCanvasFromScene())
        {
            SetClearPanelActive(false);
            return;
        }

        GameObject canvasObject = new GameObject("Canvas_Stage1Clear");
        canvasObject.transform.SetParent(transform, false);
        Canvas_Clear = canvasObject.AddComponent<Canvas>();
        Canvas_Clear.renderMode = RenderMode.ScreenSpaceOverlay;
        Canvas_Clear.overrideSorting = true;
        Canvas_Clear.sortingOrder = 5200;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject panelObject = new GameObject("Panel_StageClear", typeof(RectTransform));
        panelObject.transform.SetParent(canvasObject.transform, false);
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.82f);

        RectTransform panelRect = panelObject.transform as RectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(760f, 320f);
        panelRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI titleText = CreateClearText(panelObject.transform, "Text_ClearTitle", _stageClearTitle, 42f, new Vector2(0f, 86f), new Vector2(680f, 70f));
        TextMeshProUGUI bodyText = CreateClearText(panelObject.transform, "Text_ClearMessage", _stageClearMessage, 28f, new Vector2(0f, 10f), new Vector2(680f, 110f));
        titleText.color = Color.white;
        bodyText.color = Color.white;

        Button_NextRoad = CreateClearButton(panelObject.transform);
        SetClearPanelActive(false);
    }

    /// <summary>
    /// Stage1 臾대???誘몃━ ?볦씤 ?꾨Т?꾩닔 ?⑤꼸??李얠븘 ?곌껐?⑸땲??
    /// Game View?먯꽌???꾨Т ?꾨즺 ????Canvas媛 耳쒖?怨? 踰꾪듉? ?ㅼ쓬 Road 洹몃９???щ뒗 ?먮? ?몄텧?⑸땲??
    /// </summary>
    private bool ResolveClearCanvasFromScene()
    {
        GameObject canvasObject = RequestChildObjectByName(transform, "Canvas_Stage1Clear");

        if (canvasObject == null)
            return false;

        Canvas_Clear = canvasObject.GetComponent<Canvas>();
        Button_NextRoad = canvasObject.GetComponentInChildren<Button>(true);

        if (Canvas_Clear == null || Button_NextRoad == null)
        {
            Canvas_Clear = null;
            Button_NextRoad = null;
            return false;
        }

        ApplyStageCanvas(Canvas_Clear, 5200);
        Button_NextRoad.onClick.RemoveListener(OnNextRoadButtonClicked);
        Button_NextRoad.onClick.AddListener(OnNextRoadButtonClicked);
        ApplyClearPanelText();
        return true;
    }

    /// <summary>
    /// Stage1 ?꾨Т?꾩닔 ?⑤꼸???꾩옱 蹂댁긽 臾멸뎄瑜?諛섏쁺?⑸땲??
    /// ?곹솕 鍮꾩쑀濡쒕뒗 怨듭뿰 留덉?留??먮쭑?먯뿉 ?ㅼ젣 吏湲됰맂 ?뚰뭹 ?대쫫???ㅼ떆 ?곸뼱 愿媛앹뿉寃?蹂댁뿬二쇰뒗 ?④퀎?낅땲??
    /// </summary>
    private void ApplyClearPanelText()
    {
        if (Canvas_Clear == null)
            return;

        TextMeshProUGUI[] textArray = Canvas_Clear.GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI text in textArray)
        {
            if (text == null)
                continue;

            if (text.name == "Text_ClearTitle")
                text.text = _stageClearTitle;
            else if (text.name == "Text_ClearMessage")
                text.text = _stageClearMessage;
        }
    }

    private void ApplyStageCanvas(Canvas targetCanvas, int sortingOrder)
    {
        if (targetCanvas == null)
            return;

        targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        targetCanvas.overrideSorting = true;
        targetCanvas.sortingOrder = sortingOrder;

        CanvasScaler canvasScaler = targetCanvas.GetComponent<CanvasScaler>();

        if (canvasScaler == null)
            return;

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;
    }

    private TextMeshProUGUI CreateClearText(Transform parentTransform, string objectName, string text, float fontSize, Vector2 position, Vector2 size)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parentTransform, false);

        RectTransform textRect = textObject.transform as RectTransform;
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = position;
        textRect.sizeDelta = size;

        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.textWrappingMode = TextWrappingModes.Normal;
        OOTechTMPFontUtility.ApplyProjectFont(textComponent);
        return textComponent;
    }

    private Button CreateClearButton(Transform parentTransform)
    {
        GameObject buttonObject = new GameObject("Button_NextRoad", typeof(RectTransform));
        buttonObject.transform.SetParent(parentTransform, false);

        RectTransform buttonRect = buttonObject.transform as RectTransform;
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -112f);
        buttonRect.sizeDelta = new Vector2(260f, 70f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(1f, 1f, 1f, 0.92f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(OnNextRoadButtonClicked);

        TextMeshProUGUI labelText = CreateClearText(buttonObject.transform, "Text_Label", "넘어가기", 30f, Vector2.zero, new Vector2(240f, 60f));
        labelText.color = Color.black;
        return button;
    }

    private void SetClearPanelActive(bool isActive)
    {
        if (Canvas_Clear != null)
            Canvas_Clear.gameObject.SetActive(isActive);
    }

    private void OnNextRoadButtonClicked()
    {
        RequestOpenNextRoadGroup();
    }

    private void RequestOpenNextRoadGroup()
    {
        if (OOTechUIManager.Inst != null)
        {
            GameObject nextGroup = RequestSceneObjectByName(_nextRoadGroupName);

            if (nextGroup != null)
                OOTechUIManager.Inst.RegisterUI(_nextRoadGroupName, nextGroup);

            OOTechUIManager.Inst.CloseUI(_currentGroupName);
            OOTechUIManager.Inst.OpenUI(_nextRoadGroupName);
            return;
        }

        GameObject nextGroupObject = RequestSceneObjectByName(_nextRoadGroupName);
        gameObject.SetActive(false);

        if (nextGroupObject != null)
            nextGroupObject.SetActive(true);
    }

    private string ResolveSpeakerName(string characterId, string fallbackName)
    {
        OO_Character characterData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetCharacterData(characterId) : null;
        return characterData != null && !string.IsNullOrEmpty(characterData.Name) ? characterData.Name : fallbackName;
    }

    private GameObject RequestSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

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
        if (rootTransform == null || string.IsNullOrEmpty(objectName))
            return null;

        if (rootTransform.name == objectName || rootTransform.name.Trim() == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = RequestChildObjectByName(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }
}


