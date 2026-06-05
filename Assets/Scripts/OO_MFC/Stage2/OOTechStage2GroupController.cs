// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechStage2GroupController.cs
// - 역할: Stage2Group의 큐시트 순서만 지휘하는 Controller입니다.
// - 영화 비유: 무대감독은 "세 배우 입장 -> 악덕오리 대사 -> 몽룡 등장 -> 요리 임무 -> 퇴장" 큐만 부릅니다.
// - 유지보수 포인트: 배우 이동은 OOTechStageActorMotion, UI는 HUD/Dialog, 데이터는 GameDataManager가 맡습니다.
// =============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Stage2Group의 전체 진행 순서를 관리합니다.
/// Controller는 장면 순서를 지휘하고, 실제 배우 연기는 각 오브젝트의 역할 컴포넌트가 수행합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechStage2GroupController : MonoBehaviour
{
    [Header("Scene Context")]
    [SerializeField] private OOTechSceneContext Context_Scene;
    [SerializeField] private OOTechRoadHUDController HUD_Road;
    [SerializeField] private AudioClip _stage2BGM;
    [SerializeField] private AudioClip _arrivedBGM;

    [Header("Cue Sheet")]
    [SerializeField] private string _cueSheetDataId = "Stage2_CueSheet_01";

    private string _moranRoleId = "Moran";
    private string _mrJaeikRoleId = "Mr.Jaeik";
    private string _chunyangRoleId = "Chunyang";
    private string _greedyDuckRoleId = "GreedyDuck";
    private string _leeMongRyongRoleId = "LeeMongRyong";
    private string _backgroundRoleId = "Stage2Background";
    private string _arriveEffectRoleId = "ArriveEffect";
    private string _entryPointAId = "EntryPoint_A";
    private string _entryPointBId = "EntryPoint_B";
    private string _entryPointCId = "EntryPoint_C";
    private string _entryPointDId = "EntryPoint_D";
    private string _entryPointEId = "EntryPoint_E";
    private string _tempColliderRoleId = "Collider_Temp";
    private string _roadMissionDataId = "Stage2_Road_Quest_01";
    private string _roadMissionFallbackText = "서쪽 도시에 가 탐관오리의 자택을 방문하세요.";
    private string _stageQuestDataId = "Stage2_Quest_01";
    private string _greedyDuckFirstDialogueId = "character_GreedyDuck_01";
    private string _greedyDuckSecondDialogueId = "character_GreedyDuck_02";
    private string _leeMongRyongFirstDialogueId = "character_LeeMongRyong_01";
    private string _leeMongRyongSecondDialogueId = "character_LeeMongRyong_02";
    private string _moranQuestDialogueId = "character_Moran_07";
    private string _greedyDuckFinalDialogueId = "character_GreedyDuck_03";
    private string[] _endingDialogueIdArray =
    {
        "character_LeeMongRyong_03",
        "character_LeeMongRyong_04",
        "character_LeeMongRyong_05",
        "character_Chunyang_09"
    };
    private string _kimchiStewCookId = "OO_KimchiStew_1";
    private string _honeyIngredientId = "Ing_Honey_01";
    private int _honeyRewardCount = 10;
    private readonly List<string> _stageClearRewardItemIdList = new List<string> { "Ing_Honey_01", "OO_KoreanCake_1", "Ing_Rice_01" };
    private readonly List<int> _stageClearRewardCountList = new List<int> { 10, 10, 10 };
    private string _nextRoadGroupName = "3rd_Road_to_Stage3";
    private string _placeholderCanvasName = "Canvas_StagePlaceholder";
    private string _nextButtonName = "Button_NextStage";
    private float _entryMoveSpeed = 145f;
    private float _greedyDuckEscapeSpeed = 820f;
    private float _minimumGreedyDuckEscapeSpeed = 720f;
    private float _greedyDuckExitTimeoutSeconds = 2.4f;
    private float _greedyDuckEscapeAnimationSpeed = 1.8f;
    private float _forcedDialogueSeconds = 5f;
    private float _finalDuckDialogueSeconds = 3f;
    private float _arriveEffectSeconds = 3.4f;
    private float _arriveEffectAnimationSpeed = 0.5f;
    private float _cameraMoveSeconds = 0.6f;
    private float _cameraZoomSize = 230f;
    private string _greedyDuckFallbackObjectName = "GreedyDuck";
    private float _greedyDuckInteractionDistance = 190f;
    private int _greedyDuckVisibleSortingOrder = 70;
    private float _greedyDuckVisibilityCheckInterval = 0.2f;
    private KeyCode _interactionKey = KeyCode.E;
    private string _stage2BGMAssetPath = "Assets/Sounds/BGM/2_Road__Stage2_BGM.mp3";
    private string _arrivedBGMAssetPath = "Assets/Sounds/BGM/Arrived_BGM.mp3";

    private OOTechStageActorMotion Actor_Moran;
    private OOTechStageActorMotion Actor_MrJaeik;
    private OOTechStageActorMotion Actor_Chunyang;
    private OOTechStageActorMotion Actor_GreedyDuck;
    private OOTechStageActorMotion Actor_LeeMongRyong;
    private OOTechNPCInteractionActor Actor_GreedyDuckInteraction;
    private OOTechVisibleSpriteGuard Guard_GreedyDuck;
    private OOTechStage2DialogueCue Cue_Dialogue;
    private OOTechStage2CameraCue Cue_Camera;
    private OOTechStage2RewardService Service_Reward;
    private OOTechStage2ClearCue Cue_Clear;
    private GameObject Object_ArriveEffect;
    private GameObject Object_TempCollider;
    private GameObject Object_ClearCanvas;
    private Button Button_NextStage;
    private Bounds _stageBounds;
    private bool _isCuePlaying;
    private bool _isPlayerPhase;
    private bool _isStageClear;
    private bool _isStageClearRewardListLoadedFromData;
    private float _nextGreedyDuckVisibilityCheckTime;
    private Coroutine Coroutine_Victory;
    private OO_Stage2CueSheet Data_CueSheet;

    /// <summary>
    /// Stage2Group이 켜지면 모든 배우의 역할표를 찾고 큐시트를 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        ResolveCueSheetData();
        ApplyCueSheetData();
        ResolveReferences();
        PrepareStageView();
        PrepareHUD();
        PrepareInteraction();
        StartStage2Cue();
    }

    /// <summary>
    /// Stage2Group이 꺼질 때 입력, 마커, 코루틴, 카메라 상태를 정리합니다.
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
        Coroutine_Victory = null;
        _isCuePlaying = false;
        _isPlayerPhase = false;
        _isStageClear = false;
        _isStageClearRewardListLoadedFromData = false;
        SetGreedyDuckInteractionEnabled(false);
        RequestSetTempColliderActive(false);
        SetClearButtonActive(false);
        if (Cue_Dialogue != null)
            Cue_Dialogue.RequestCloseDialogue();

        if (Cue_Camera != null)
            Cue_Camera.RequestRestoreCameraFollow();
    }

    /// <summary>
    /// 플레이어 조작 단계에서 김치찌개 보유 여부와 GreedyDuck 상호작용 가능 상태를 갱신합니다.
    /// </summary>
    private void Update()
    {
        if (!_isPlayerPhase || _isStageClear)
        {
            MaintainGreedyDuckVisibility();
            return;
        }

        bool hasStew = OOTechGameManager.Inst != null && OOTechGameManager.Inst.GetItemCount(_kimchiStewCookId) > 0;
        SetGreedyDuckInteractionEnabled(hasStew);
        MaintainGreedyDuckVisibility();
    }

    private void ResolveReferences()
    {
        Context_Scene = Context_Scene != null ? Context_Scene : GetComponent<OOTechSceneContext>();
        HUD_Road = HUD_Road != null ? HUD_Road : GetComponent<OOTechRoadHUDController>();
        Cue_Dialogue = Cue_Dialogue != null ? Cue_Dialogue : GetComponent<OOTechStage2DialogueCue>();
        Cue_Camera = Cue_Camera != null ? Cue_Camera : GetComponent<OOTechStage2CameraCue>();
        Service_Reward = Service_Reward != null ? Service_Reward : GetComponent<OOTechStage2RewardService>();
        Cue_Clear = Cue_Clear != null ? Cue_Clear : GetComponent<OOTechStage2ClearCue>();

        if (Context_Scene != null)
            Context_Scene.CacheSceneObjects();

        Actor_Moran = ResolveActor(_moranRoleId);
        Actor_MrJaeik = ResolveActor(_mrJaeikRoleId);
        Actor_Chunyang = ResolveActor(_chunyangRoleId);
        Actor_GreedyDuck = ResolveGreedyDuckActor();
        Guard_GreedyDuck = ResolveGreedyDuckGuard();
        Actor_LeeMongRyong = ResolveActor(_leeMongRyongRoleId);
        Object_ArriveEffect = ResolveRoleObject(_arriveEffectRoleId);
        Object_TempCollider = ResolveRoleObject(_tempColliderRoleId);
        
        if (Cue_Camera != null)
            Cue_Camera.RequestResolveCamera();

        ResolveStageBounds();
        ResolveClearButton();
    }

    private OOTechStageActorMotion ResolveActor(string roleId)
    {
        OOTechStageActorMotion actor = Context_Scene != null ? Context_Scene.GetRoleComponent<OOTechStageActorMotion>(roleId) : null;

        if (actor != null)
            return actor;

        GameObject actorObject = ResolveRoleObject(roleId);
        return actorObject != null ? actorObject.GetComponent<OOTechStageActorMotion>() : null;
    }

    private OOTechStageActorMotion ResolveGreedyDuckActor()
    {
        OOTechStageActorMotion actor = ResolveActor(_greedyDuckRoleId);

        if (actor != null)
            return actor;

        GameObject fallbackObject = FindChildByName(transform, _greedyDuckFallbackObjectName);

        if (fallbackObject == null)
            return null;

        return fallbackObject.GetComponent<OOTechStageActorMotion>();
    }

    private OOTechVisibleSpriteGuard ResolveGreedyDuckGuard()
    {
        GameObject greedyDuckObject = ResolveRoleObject(_greedyDuckRoleId);

        if (greedyDuckObject == null)
            greedyDuckObject = FindChildByName(transform, _greedyDuckFallbackObjectName);

        return greedyDuckObject != null ? greedyDuckObject.GetComponent<OOTechVisibleSpriteGuard>() : null;
    }

    private GameObject ResolveRoleObject(string roleId)
    {
        if (Context_Scene != null)
        {
            GameObject roleObject = Context_Scene.GetRoleObject(roleId);

            if (roleObject != null)
                return roleObject;
        }

        return FindChildByName(transform, roleId);
    }

    private Transform ResolveRoleTransform(string roleId)
    {
        GameObject roleObject = ResolveRoleObject(roleId);
        return roleObject != null ? roleObject.transform : null;
    }

    private void ResolveStageBounds()
    {
        GameObject backgroundObject = ResolveRoleObject(_backgroundRoleId);
        SpriteRenderer backgroundRenderer = backgroundObject != null ? backgroundObject.GetComponentInChildren<SpriteRenderer>(true) : null;
        _stageBounds = backgroundRenderer != null ? backgroundRenderer.bounds : new Bounds(new Vector3(960f, 540f, 0f), new Vector3(1920f, 1080f, 0f));

        if (Actor_Moran != null)
            Actor_Moran.RequestSetMovementBounds(_stageBounds);
    }

    /// <summary>
    /// 배경 전체가 카메라에 들어오도록 맞추고, 기존 따라가기 카메라는 잠시 끕니다.
    /// </summary>
    private void PrepareStageView()
    {
        if (Cue_Camera != null)
            Cue_Camera.RequestSaveAndDisableCameraFollow();

        FocusCameraOnBounds(_stageBounds, 1.02f);
        SetArriveEffectActive(false);
        RequestSetTempColliderActive(false);
        RestoreGreedyDuckView();
        SetClearButtonActive(false);

        if (Actor_Moran != null)
            Actor_Moran.RequestSetPlayerInputEnabled(false);

        if (Actor_MrJaeik != null)
            Actor_MrJaeik.RequestSetPlayerInputEnabled(false);

        if (Actor_Chunyang != null)
            Actor_Chunyang.RequestSetPlayerInputEnabled(false);
    }

    private void PrepareHUD()
    {
        if (HUD_Road == null)
            return;

        HUD_Road.SetOwnerGroupName(gameObject.name);
        HUD_Road.PrepareHUD();
        HUD_Road.SetCookingUnlocked(true);
        HUD_Road.RequestSetRoadMissionText(RequestResolveStageQuestDescription(_roadMissionDataId, _roadMissionFallbackText), true);
    }

    private void PrepareInteraction()
    {
        GameObject greedyDuckObject = ResolveRoleObject(_greedyDuckRoleId);

        if (greedyDuckObject == null)
            return;

        Actor_GreedyDuckInteraction = greedyDuckObject.GetComponent<OOTechNPCInteractionActor>();

        if (Actor_GreedyDuckInteraction == null)
            Actor_GreedyDuckInteraction = greedyDuckObject.AddComponent<OOTechNPCInteractionActor>();

        Actor_GreedyDuckInteraction.InteractionRequested -= OnGreedyDuckInteractionRequested;
        Actor_GreedyDuckInteraction.InteractionRequested += OnGreedyDuckInteractionRequested;
        Actor_GreedyDuckInteraction.RequestSetup(Actor_Moran != null ? Actor_Moran.transform : null, null);
        Actor_GreedyDuckInteraction.RequestSetInteractionRule(_greedyDuckInteractionDistance, Vector3.zero, _interactionKey);
        Actor_GreedyDuckInteraction.RequestSetOneShot(true);

        SetGreedyDuckInteractionEnabled(false);
    }

    private void StartStage2Cue()
    {
        if (_isCuePlaying)
            return;

        StartCoroutine(PlayStage2CueRoutine());
    }

    /// <summary>
    /// OO_Stage2CueSheet 데이터에서 Stage2 공연 큐시트를 찾습니다.
    /// JSON이 아직 컨버팅되지 않았으면 기존 fallback 값으로 공연을 이어갑니다.
    /// </summary>
    private void ResolveCueSheetData()
    {
        Data_CueSheet = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStage2CueSheetData(_cueSheetDataId) : null;

        if (Data_CueSheet == null)
            Debug.LogWarning($"[OOTechStage2GroupController] Stage2CueSheet data missing: {_cueSheetDataId}. Fallback cue will be used.");
    }

    /// <summary>
    /// 엑셀 큐시트 값을 Stage2Controller의 실행 변수에 적용합니다.
    /// 영화 비유로는 오늘 공연 큐시트에 적힌 배우 이름표, 대사 번호, 조명 시간을 무대감독 책상에 펼쳐 놓는 단계입니다.
    /// </summary>
    private void ApplyCueSheetData()
    {
        if (Data_CueSheet == null)
            return;

        ApplyTextIfValid(ref _moranRoleId, Data_CueSheet.MoranRoleId);
        ApplyTextIfValid(ref _mrJaeikRoleId, Data_CueSheet.MrJaeikRoleId);
        ApplyTextIfValid(ref _chunyangRoleId, Data_CueSheet.ChunyangRoleId);
        ApplyTextIfValid(ref _greedyDuckRoleId, Data_CueSheet.GreedyDuckRoleId);
        ApplyTextIfValid(ref _leeMongRyongRoleId, Data_CueSheet.LeeMongRyongRoleId);
        ApplyTextIfValid(ref _backgroundRoleId, Data_CueSheet.BackgroundRoleId);
        ApplyTextIfValid(ref _arriveEffectRoleId, Data_CueSheet.ArriveEffectRoleId);
        ApplyTextIfValid(ref _entryPointAId, Data_CueSheet.EntryPointAId);
        ApplyTextIfValid(ref _entryPointBId, Data_CueSheet.EntryPointBId);
        ApplyTextIfValid(ref _entryPointCId, Data_CueSheet.EntryPointCId);
        ApplyTextIfValid(ref _entryPointDId, Data_CueSheet.EntryPointDId);
        ApplyTextIfValid(ref _entryPointEId, Data_CueSheet.EntryPointEId);
        ApplyTextIfValid(ref _tempColliderRoleId, Data_CueSheet.TempColliderRoleId);
        ApplyTextIfValid(ref _roadMissionDataId, Data_CueSheet.RoadMissionDataId);
        ApplyTextIfValid(ref _roadMissionFallbackText, Data_CueSheet.RoadMissionFallbackText);
        ApplyTextIfValid(ref _stageQuestDataId, Data_CueSheet.StageQuestDataId);
        ApplyTextIfValid(ref _greedyDuckFirstDialogueId, Data_CueSheet.GreedyDuckFirstDialogueId);
        ApplyTextIfValid(ref _greedyDuckSecondDialogueId, Data_CueSheet.GreedyDuckSecondDialogueId);
        ApplyTextIfValid(ref _leeMongRyongFirstDialogueId, Data_CueSheet.LeeMongRyongFirstDialogueId);
        ApplyTextIfValid(ref _leeMongRyongSecondDialogueId, Data_CueSheet.LeeMongRyongSecondDialogueId);
        ApplyTextIfValid(ref _moranQuestDialogueId, Data_CueSheet.MoranQuestDialogueId);
        ApplyTextIfValid(ref _greedyDuckFinalDialogueId, Data_CueSheet.GreedyDuckFinalDialogueId);
        ApplyTextIfValid(ref _kimchiStewCookId, Data_CueSheet.KimchiStewCookId);
        ApplyTextIfValid(ref _honeyIngredientId, Data_CueSheet.HoneyIngredientId);
        ApplyTextIfValid(ref _nextRoadGroupName, Data_CueSheet.NextRoadGroupName);
        ApplyTextIfValid(ref _placeholderCanvasName, Data_CueSheet.PlaceholderCanvasName);
        ApplyTextIfValid(ref _nextButtonName, Data_CueSheet.NextButtonName);
        ApplyTextIfValid(ref _greedyDuckFallbackObjectName, Data_CueSheet.GreedyDuckFallbackObjectName);
        ApplyKeyIfValid(ref _interactionKey, Data_CueSheet.InteractionKey);

        if (Data_CueSheet.EndingDialogueIdList != null && Data_CueSheet.EndingDialogueIdList.Count > 0)
            _endingDialogueIdArray = Data_CueSheet.EndingDialogueIdList.ToArray();

        ApplyIntIfPositive(ref _honeyRewardCount, Data_CueSheet.HoneyRewardCount);
        ApplyRewardListIfValid(Data_CueSheet);
        ApplyLegacyHoneyRewardFallback();
        ApplyIntIfPositive(ref _greedyDuckVisibleSortingOrder, Data_CueSheet.GreedyDuckVisibleSortingOrder);
        ApplyFloatIfPositive(ref _entryMoveSpeed, Data_CueSheet.EntryMoveSpeed);
        ApplyFloatIfPositive(ref _greedyDuckEscapeSpeed, Data_CueSheet.GreedyDuckEscapeSpeed);
        ApplyFloatIfPositive(ref _greedyDuckExitTimeoutSeconds, Data_CueSheet.GreedyDuckExitTimeoutSeconds);
        ApplyFloatIfPositive(ref _greedyDuckEscapeAnimationSpeed, Data_CueSheet.GreedyDuckEscapeAnimationSpeed);
        ApplyFloatIfPositive(ref _forcedDialogueSeconds, Data_CueSheet.ForcedDialogueSeconds);
        ApplyFloatIfPositive(ref _finalDuckDialogueSeconds, Data_CueSheet.FinalDuckDialogueSeconds);
        ApplyFloatIfPositive(ref _arriveEffectSeconds, Data_CueSheet.ArriveEffectSeconds);
        ApplyFloatIfPositive(ref _arriveEffectAnimationSpeed, Data_CueSheet.ArriveEffectAnimationSpeed);
        ApplyFloatIfPositive(ref _cameraMoveSeconds, Data_CueSheet.CameraMoveSeconds);
        ApplyFloatIfPositive(ref _cameraZoomSize, Data_CueSheet.CameraZoomSize);
        ApplyFloatIfPositive(ref _greedyDuckInteractionDistance, Data_CueSheet.GreedyDuckInteractionDistance);
        ApplyFloatIfPositive(ref _greedyDuckVisibilityCheckInterval, Data_CueSheet.GreedyDuckVisibilityCheckInterval);
        _greedyDuckEscapeSpeed = Mathf.Max(_greedyDuckEscapeSpeed, _minimumGreedyDuckEscapeSpeed);
        _greedyDuckEscapeAnimationSpeed = Mathf.Max(1f, _greedyDuckEscapeAnimationSpeed);
        _greedyDuckExitTimeoutSeconds = Mathf.Max(0.5f, _greedyDuckExitTimeoutSeconds);
    }

    private void ApplyTextIfValid(ref string targetText, string sourceText)
    {
        if (!string.IsNullOrWhiteSpace(sourceText))
            targetText = sourceText;
    }

    private void ApplyIntIfPositive(ref int targetValue, int sourceValue)
    {
        if (sourceValue > 0)
            targetValue = sourceValue;
    }

    private void ApplyFloatIfPositive(ref float targetValue, float sourceValue)
    {
        if (sourceValue > 0f)
            targetValue = sourceValue;
    }

    private void ApplyKeyIfValid(ref KeyCode targetKey, string keyText)
    {
        if (string.IsNullOrWhiteSpace(keyText))
            return;

        if (Enum.TryParse(keyText, true, out KeyCode parsedKey))
            targetKey = parsedKey;
    }

    /// <summary>
    /// Stage2 클리어 보상을 데이터 목록으로 갱신합니다.
    /// 영화로 치면 마지막 커튼콜 뒤 관객에게 나눠줄 선물 목록을 큐시트에서 읽는 단계입니다.
    /// </summary>
    private void ApplyRewardListIfValid(OO_Stage2CueSheet cueSheetData)
    {
        if (cueSheetData == null || cueSheetData.StageClearRewardItemIdList == null || cueSheetData.StageClearRewardItemIdList.Count == 0)
            return;

        _isStageClearRewardListLoadedFromData = true;
        _stageClearRewardItemIdList.Clear();
        _stageClearRewardCountList.Clear();

        for (int index = 0; index < cueSheetData.StageClearRewardItemIdList.Count; index++)
        {
            string itemId = cueSheetData.StageClearRewardItemIdList[index];

            if (string.IsNullOrWhiteSpace(itemId))
                continue;

            int count = 1;

            if (cueSheetData.StageClearRewardCountList != null && index < cueSheetData.StageClearRewardCountList.Count)
                count = Mathf.Max(1, cueSheetData.StageClearRewardCountList[index]);

            _stageClearRewardItemIdList.Add(itemId);
            _stageClearRewardCountList.Add(count);
        }
    }

    private void ApplyLegacyHoneyRewardFallback()
    {
        if (_isStageClearRewardListLoadedFromData)
            return;

        if (string.IsNullOrWhiteSpace(_honeyIngredientId) || _stageClearRewardItemIdList.Count == 0)
            return;

        _stageClearRewardItemIdList[0] = _honeyIngredientId;

        if (_stageClearRewardCountList.Count > 0)
            _stageClearRewardCountList[0] = Mathf.Max(1, _honeyRewardCount);
    }

    /// <summary>
    /// Stage2 큐시트 본문입니다.
    /// 각 줄은 감독 큐이고, 실제 이동/애니메이션/UI 출력은 역할 컴포넌트와 매니저에게 요청합니다.
    /// </summary>
    private IEnumerator PlayStage2CueRoutine()
    {
        _isCuePlaying = true;
        _isPlayerPhase = false;
        RequestPlayStage2BGM();
        PlaceEntryActorsOffScreen();

        yield return MoveOpeningActorsRoutine();
        yield return PlayGreedyDuckFirstDialogueRoutine();
        yield return MoveLeeMongRyongEntryRoutine();
        yield return PlayGreedyDuckSecondDialogueRoutine();
        yield return PlayArrivalEffectRoutine();
        yield return PlayLeeMongRyongOpeningRoutine();
        yield return PlayQuestDialogueRoutine();

        StartPlayerQuestPhase();
        _isCuePlaying = false;
    }

    private void RequestPlayStage2BGM()
    {
        AudioClip bgmClip = ResolveAudioClip(_stage2BGM, _stage2BGMAssetPath);

        if (bgmClip != null && OOTechSoundManager.Inst != null)
            OOTechSoundManager.Inst.PlayBGM(bgmClip, true);
    }

    /// <summary>
    /// 이몽룡 등장 이후의 BGM으로 교체합니다.
    /// 영화로 치면 주인공 등장의 테마 음악을 새로 큐하는 장면입니다.
    /// </summary>
    private void RequestPlayArrivedBGM()
    {
        AudioClip bgmClip = ResolveAudioClip(_arrivedBGM, _arrivedBGMAssetPath);

        if (bgmClip != null && OOTechSoundManager.Inst != null)
            OOTechSoundManager.Inst.PlayBGM(bgmClip, true);
    }

    private AudioClip ResolveAudioClip(AudioClip assignedClip, string assetPath)
    {
        if (assignedClip != null)
            return assignedClip;

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(assetPath))
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
#endif

        return null;
    }

    private void PlaceEntryActorsOffScreen()
    {
        PlaceActorAtEntryStart(Actor_Moran, ResolveRoleTransform(_entryPointAId), 360f);
        PlaceActorAtEntryStart(Actor_MrJaeik, ResolveRoleTransform(_entryPointBId), 440f);
        PlaceActorAtEntryStart(Actor_Chunyang, ResolveRoleTransform(_entryPointCId), 520f);
        PlaceActorAtEntryStart(Actor_LeeMongRyong, ResolveRoleTransform(_entryPointDId), 360f);

        if (Actor_LeeMongRyong != null)
            Actor_LeeMongRyong.gameObject.SetActive(false);
    }

    private void PlaceActorAtEntryStart(OOTechStageActorMotion actor, Transform targetTransform, float leftOffset)
    {
        if (actor == null || targetTransform == null)
            return;

        actor.gameObject.SetActive(true);
        Vector3 position = targetTransform.position;
        position.x = _stageBounds.min.x - Mathf.Max(80f, leftOffset);
        actor.transform.position = position;
        actor.RequestPlayIdle();
    }

    private IEnumerator MoveOpeningActorsRoutine()
    {
        int arrivalCount = 0;
        StartActorMove(Actor_Moran, ResolveRoleTransform(_entryPointAId), delegate { arrivalCount++; });
        StartActorMove(Actor_MrJaeik, ResolveRoleTransform(_entryPointBId), delegate { arrivalCount++; });
        StartActorMove(Actor_Chunyang, ResolveRoleTransform(_entryPointCId), delegate { arrivalCount++; });

        yield return new WaitUntil(() => arrivalCount >= 3);
    }

    private void StartActorMove(OOTechStageActorMotion actor, Transform targetTransform, Action onArrived)
    {
        if (actor == null || targetTransform == null)
        {
            onArrived?.Invoke();
            return;
        }

        StartCoroutine(MoveActorAndNotifyRoutine(actor, targetTransform, onArrived));
    }

    private IEnumerator MoveActorAndNotifyRoutine(OOTechStageActorMotion actor, Transform targetTransform, Action onArrived)
    {
        yield return actor.MoveToTargetRoutine(targetTransform, _entryMoveSpeed);
        onArrived?.Invoke();
    }

    private IEnumerator PlayGreedyDuckFirstDialogueRoutine()
    {
        if (Actor_GreedyDuck != null)
            Actor_GreedyDuck.RequestPlayState("GreedyDuck_isAngry", 1f, true);

        yield return ShowDialogueAndWait(_greedyDuckFirstDialogueId);
    }

    private IEnumerator MoveLeeMongRyongEntryRoutine()
    {
        if (Actor_LeeMongRyong == null)
            yield break;

        Actor_LeeMongRyong.gameObject.SetActive(true);
        RequestPlayArrivedBGM();
        yield return Actor_LeeMongRyong.MoveToTargetRoutine(ResolveRoleTransform(_entryPointDId), _entryMoveSpeed);
    }

    private IEnumerator PlayGreedyDuckSecondDialogueRoutine()
    {
        if (Actor_GreedyDuck != null)
            Actor_GreedyDuck.RequestPlayState("GreedyDuck_isAngry", 1f, true);

        yield return ShowDialogueForSeconds(_greedyDuckSecondDialogueId, _forcedDialogueSeconds);
    }

    private IEnumerator PlayArrivalEffectRoutine()
    {
        if (Actor_GreedyDuck != null)
            Actor_GreedyDuck.RequestPlayState("GreedyDuck_isScared", 1f, true);

        FocusCameraOnBounds(_stageBounds, 1.02f);
        SetArriveEffectActive(true);
        yield return new WaitForSeconds(_arriveEffectSeconds);
        SetArriveEffectActive(false);

        if (Actor_LeeMongRyong != null)
            yield return FocusCameraOnActorRoutine(Actor_LeeMongRyong.transform, _cameraZoomSize, _cameraMoveSeconds);
    }

    private IEnumerator PlayLeeMongRyongOpeningRoutine()
    {
        PlayReactionActors(true);
        yield return ShowDialogueAndWait(_leeMongRyongFirstDialogueId);

        if (Actor_LeeMongRyong != null)
        {
            Actor_LeeMongRyong.RequestPlayState("LeeMongRyong_isArrived", 0.7f, true);
            yield return new WaitForSeconds(GetAnimationClipLength(Actor_LeeMongRyong, "LeeMongRyong_isArrived", 1.1f) / 0.7f);
            Actor_LeeMongRyong.RequestPlayState("LeeMongRyong_idle2", 1f, true);
        }

        FocusCameraOnBounds(_stageBounds, 1.02f);
        yield return ShowDialogueAndWait(_leeMongRyongSecondDialogueId);
        PlayReactionActors(false);
    }

    private IEnumerator PlayQuestDialogueRoutine()
    {
        yield return ShowDialogueAndWait(_moranQuestDialogueId);
        if (Service_Reward != null)
            Service_Reward.RequestUpdateStageQuest(HUD_Road, _stageQuestDataId, "김치와 청양고추로 김치찌개를 만들고 악덕오리에게 가져가세요.");
    }

    private void StartPlayerQuestPhase()
    {
        _isPlayerPhase = true;

        if (Actor_Moran != null)
            Actor_Moran.RequestSetPlayerInputEnabled(true);
    }

    private void OnGreedyDuckInteractionRequested(OOTechNPCInteractionActor interactionActor)
    {
        if (!_isPlayerPhase || _isStageClear)
            return;

        if (OOTechGameManager.Inst == null || OOTechGameManager.Inst.GetItemCount(_kimchiStewCookId) <= 0)
        {
            if (HUD_Road != null)
                HUD_Road.SetMissionNewBadgeActive(true);

            interactionActor.RequestResetInteraction();
            return;
        }

        StartCoroutine(PlayGreedyDuckClearRoutine());
    }

    /// <summary>
    /// 김치찌개를 받은 GreedyDuck이 불타며 퇴장하고 Stage2를 완료하는 마지막 큐입니다.
    /// </summary>
    private IEnumerator PlayGreedyDuckClearRoutine()
    {
        _isStageClear = true;
        _isPlayerPhase = false;
        SetGreedyDuckInteractionEnabled(false);

        if (Actor_Moran != null)
            Actor_Moran.RequestSetPlayerInputEnabled(false);

        OOTechGameManager.Inst.RemoveItem(_kimchiStewCookId, 1);

        yield return ShowDialogueForSeconds(_greedyDuckFinalDialogueId, _finalDuckDialogueSeconds);

        if (Actor_GreedyDuck != null)
        {
            yield return FocusCameraOnActorRoutine(Actor_GreedyDuck.transform, _cameraZoomSize, _cameraMoveSeconds);
            Transform escapeTransform = ResolveRoleTransform(_entryPointEId);

            if (Cue_Clear != null)
                yield return Cue_Clear.RequestPlayGreedyDuckExitRoutine(Actor_GreedyDuck, escapeTransform, Object_TempCollider, _greedyDuckEscapeSpeed, _greedyDuckExitTimeoutSeconds, _greedyDuckEscapeAnimationSpeed);
        }

        if (Actor_LeeMongRyong != null)
            yield return FocusCameraOnActorRoutine(Actor_LeeMongRyong.transform, _cameraZoomSize, _cameraMoveSeconds);

        Debug.Log("[OOTechStage2GroupController] GreedyDuck exit complete. Ending dialogue cue starts.");
        yield return PlayEndingDialogueRoutine();
        Debug.Log("[OOTechStage2GroupController] Ending dialogue cue complete. Stage2 clear reward starts.");
        CompleteStage2();
    }

    private IEnumerator PlayEndingDialogueRoutine()
    {
        for (int index = 0; index < _endingDialogueIdArray.Length; index++)
        {
            string dialogueId = _endingDialogueIdArray[index];

            if (dialogueId == "character_Chunyang_09" && Actor_Chunyang != null)
                Actor_Chunyang.RequestPlayReaction(1f);

            Debug.Log($"[OOTechStage2GroupController] Ending dialogue requested: {dialogueId}");
            yield return ShowDialogueAndWait(dialogueId);
        }
    }

    private void CompleteStage2()
    {
        Debug.Log("[OOTechStage2GroupController] Stage2 complete. Reward, victory loop, clear button enabled.");

        if (Service_Reward != null)
            Service_Reward.RequestGiveStageClearRewardList(HUD_Road, _stageClearRewardItemIdList, _stageClearRewardCountList);

        Coroutine_Victory = StartCoroutine(PlayMoranVictoryLoopRoutine());
        SetClearButtonActive(true);
        FocusCameraOnBounds(_stageBounds, 1.02f);
    }

    private IEnumerator PlayMoranVictoryLoopRoutine()
    {
        while (_isStageClear && Actor_Moran != null && gameObject.activeInHierarchy)
        {
            Actor_Moran.RequestPlayVictory(1f);
            yield return new WaitForSeconds(GetAnimationClipLength(Actor_Moran, "Moran_Victory", 0.8f));
        }
    }

    private IEnumerator ShowDialogueAndWait(string dialogueId)
    {
        if (Cue_Dialogue != null)
            yield return Cue_Dialogue.RequestShowDialogueAndWait(dialogueId);
    }

    private IEnumerator ShowDialogueForSeconds(string dialogueId, float seconds)
    {
        if (Cue_Dialogue != null)
            yield return Cue_Dialogue.RequestShowDialogueForSeconds(dialogueId, seconds);
    }

    /// <summary>
    /// OO_StageQuest에서 임무 설명을 꺼냅니다.
    /// 무대감독은 대사를 직접 쓰지 않고, 기획팀이 적어 둔 큐시트 문장을 읽어 HUD에 넘깁니다.
    /// </summary>
    private string RequestResolveStageQuestDescription(string stageQuestDataId, string fallbackText)
    {
        return Service_Reward != null ? Service_Reward.RequestResolveStageQuestDescription(stageQuestDataId, fallbackText) : fallbackText;
    }

    private void PlayReactionActors(bool isActive)
    {
        if (isActive)
        {
            if (Actor_Moran != null && !Actor_Moran.RequestPlayState("Moran_isDancing", 1f, true))
                Actor_Moran.RequestPlayReaction(1f);

            if (Actor_Chunyang != null)
                Actor_Chunyang.RequestPlayReaction(1f);

            if (Actor_MrJaeik != null)
                Actor_MrJaeik.RequestPlayReaction(1f);

            return;
        }

        if (Actor_Moran != null)
            Actor_Moran.RequestPlayIdle();

        if (Actor_Chunyang != null)
            Actor_Chunyang.RequestPlayIdle();

        if (Actor_MrJaeik != null)
            Actor_MrJaeik.RequestPlayIdle();
    }

    private IEnumerator FocusCameraOnActorRoutine(Transform targetTransform, float targetSize, float duration)
    {
        if (Cue_Camera != null)
            yield return Cue_Camera.RequestFocusCameraOnActorRoutine(targetTransform, targetSize, duration);
    }

    private void FocusCameraOnBounds(Bounds bounds, float padding)
    {
        if (Cue_Camera != null)
            Cue_Camera.RequestFocusCameraOnBounds(bounds, padding);
    }

    private void SetArriveEffectActive(bool isActive)
    {
        if (Object_ArriveEffect == null)
            return;

        Object_ArriveEffect.SetActive(isActive);
        Animator effectAnimator = Object_ArriveEffect.GetComponentInChildren<Animator>(true);

        if (effectAnimator != null && isActive)
        {
            effectAnimator.speed = Mathf.Max(0.01f, _arriveEffectAnimationSpeed);
            effectAnimator.Play(0, 0, 0f);
        }
    }

    private IEnumerator BlinkAndHideRoutine(GameObject targetObject)
    {
        if (Cue_Clear != null)
            yield return Cue_Clear.RequestBlinkAndHideRoutine(targetObject);
    }

    private void SetGreedyDuckInteractionEnabled(bool isEnabled)
    {
        if (Actor_GreedyDuckInteraction != null)
            Actor_GreedyDuckInteraction.RequestSetInteractable(isEnabled);
    }

    /// <summary>
    /// Stage2가 다시 열릴 때 GreedyDuck 배우를 보이는 상태로 복구합니다.
    /// 이전 리허설에서 퇴장하며 꺼진 조명과 의상을 다시 켜서 첫 장면에 탐관오리가 보이게 합니다.
    /// </summary>
    private void RestoreGreedyDuckView()
    {
        GameObject greedyDuckObject = Actor_GreedyDuck != null ? Actor_GreedyDuck.gameObject : ResolveRoleObject(_greedyDuckRoleId);

        if (greedyDuckObject == null)
            greedyDuckObject = FindChildByName(transform, _greedyDuckFallbackObjectName);

        if (greedyDuckObject == null)
        {
            Debug.LogWarning("[OOTechStage2GroupController] GreedyDuck restore failed: object not found.");
            return;
        }

        greedyDuckObject.SetActive(true);

        if (Actor_GreedyDuck == null)
            Actor_GreedyDuck = greedyDuckObject.GetComponent<OOTechStageActorMotion>();

        SpriteRenderer[] rendererArray = greedyDuckObject.GetComponentsInChildren<SpriteRenderer>(true);
        bool hasVisibleRenderer = false;

        foreach (SpriteRenderer renderer in rendererArray)
        {
            if (renderer == null)
                continue;

            hasVisibleRenderer = true;
            renderer.enabled = true;
            renderer.sortingLayerName = "Characters";
            renderer.sortingOrder = Mathf.Max(renderer.sortingOrder, _greedyDuckVisibleSortingOrder);
            Color color = renderer.color;
            color.a = 1f;
            renderer.color = color;
        }

        if (!hasVisibleRenderer)
            Debug.LogWarning("[OOTechStage2GroupController] GreedyDuck restore warning: SpriteRenderer not found.", greedyDuckObject);

        Animator animator = greedyDuckObject.GetComponentInChildren<Animator>(true);

        if (animator != null)
        {
            animator.enabled = true;
            animator.speed = 1f;
        }

        if (Actor_GreedyDuck != null)
            Actor_GreedyDuck.RequestPlayIdle();

        Guard_GreedyDuck = Guard_GreedyDuck != null ? Guard_GreedyDuck : greedyDuckObject.GetComponent<OOTechVisibleSpriteGuard>();

        if (Guard_GreedyDuck != null)
            Guard_GreedyDuck.RequestEnsureVisible();

        Debug.Log($"[OOTechStage2GroupController] GreedyDuck visible restore complete. Active={greedyDuckObject.activeInHierarchy}, RendererCount={rendererArray.Length}, SortingOrder={_greedyDuckVisibleSortingOrder}");
    }

    /// <summary>
    /// Stage2 클리어 전에는 GreedyDuck이 게임 진행의 핵심 배우라 주기적으로 표시 상태를 확인합니다.
    /// 영화로 치면 탐관오리 배우가 무대에서 사라지면 다음 장면이 막히므로, 조명 담당에게 계속 상태 체크를 맡기는 안전장치입니다.
    /// </summary>
    private void MaintainGreedyDuckVisibility()
    {
        if (_isStageClear || Time.unscaledTime < _nextGreedyDuckVisibilityCheckTime)
            return;

        _nextGreedyDuckVisibilityCheckTime = Time.unscaledTime + Mathf.Max(0.05f, _greedyDuckVisibilityCheckInterval);

        if (Guard_GreedyDuck == null)
            Guard_GreedyDuck = ResolveGreedyDuckGuard();

        if (Guard_GreedyDuck != null)
            Guard_GreedyDuck.RequestEnsureVisible();
    }

    /// <summary>
    /// GreedyDuck이 EntryPoint_E로 퇴장할 때만 임시 발판 Collider를 켜고, 퇴장 후 다시 끕니다.
    /// 영화로 치면 배우가 무대 밖으로 안전하게 나가도록 잠깐 놓는 이동용 받침대입니다.
    /// </summary>
    private void RequestSetTempColliderActive(bool isActive)
    {
        if (Object_TempCollider == null)
            Object_TempCollider = ResolveRoleObject(_tempColliderRoleId);

        if (Cue_Clear != null)
            Cue_Clear.RequestSetTempColliderActive(Object_TempCollider, isActive);
    }

    private void ResolveClearButton()
    {
        GameObject canvasObject = FindChildByName(transform, _placeholderCanvasName);

        if (canvasObject == null)
            return;

        Object_ClearCanvas = canvasObject;
        Button_NextStage = FindChildByName(canvasObject.transform, _nextButtonName)?.GetComponent<Button>();

        if (Button_NextStage == null)
            Button_NextStage = canvasObject.GetComponentInChildren<Button>(true);

        if (Button_NextStage != null)
        {
            Button_NextStage.onClick.RemoveListener(OnNextStageButtonClicked);
            Button_NextStage.onClick.AddListener(OnNextStageButtonClicked);
        }
    }

    private void SetClearButtonActive(bool isActive)
    {
        if (Object_ClearCanvas != null)
            Object_ClearCanvas.SetActive(isActive);
    }

    private void OnNextStageButtonClicked()
    {
        if (OOTechUIManager.Inst != null)
        {
            GameObject nextGroup = FindSceneObjectByName(_nextRoadGroupName);

            if (nextGroup != null)
                OOTechUIManager.Inst.RegisterUI(_nextRoadGroupName, nextGroup);

            OOTechUIManager.Inst.CloseUI(gameObject.name);
            OOTechUIManager.Inst.OpenUI(_nextRoadGroupName);
            return;
        }

        GameObject nextGroupObject = FindSceneObjectByName(_nextRoadGroupName);
        gameObject.SetActive(false);

        if (nextGroupObject != null)
            nextGroupObject.SetActive(true);
    }

    private float GetAnimationClipLength(OOTechStageActorMotion actor, string clipName, float fallbackSeconds)
    {
        Animator animator = actor != null ? actor.GetComponentInChildren<Animator>(true) : null;

        if (animator == null || animator.runtimeAnimatorController == null)
            return fallbackSeconds;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == clipName)
                return Mathf.Max(0.1f, clip.length);
        }

        return fallbackSeconds;
    }

    private GameObject FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(objectName))
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

    private GameObject FindSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = FindChildByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }
}
