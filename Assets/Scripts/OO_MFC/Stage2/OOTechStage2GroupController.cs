// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechStage2GroupController.cs
// - ??븷: Stage2Group???먯떆???쒖꽌留?吏?섑븯??Controller?낅땲??
// - ?곹솕 鍮꾩쑀: 臾대?媛먮룆? "??諛곗슦 ?낆옣 -> ?낅뜒?ㅻ━ ???-> 紐쎈！ ?깆옣 -> ?붾━ ?꾨Т -> ?댁옣" ?먮쭔 遺由낅땲??
// - ?좎?蹂댁닔 ?ъ씤?? 諛곗슦 ?대룞? OOTechStageActorMotion, UI??HUD/Dialog, ?곗씠?곕뒗 GameDataManager媛 留≪뒿?덈떎.
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
/// Stage2Group???꾩껜 吏꾪뻾 ?쒖꽌瑜?愿由ы빀?덈떎.
/// Controller???λ㈃ ?쒖꽌瑜?吏?섑븯怨? ?ㅼ젣 諛곗슦 ?곌린??媛??ㅻ툕?앺듃????븷 而댄룷?뚰듃媛 ?섑뻾?⑸땲??
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
    private string _roadMissionFallbackText = "?쒖そ ?꾩떆??媛 ?먭??ㅻ━???먰깮??諛⑸Ц?섏꽭??";
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
    private string _stageClearCanvasName = "Canvas_Stage2Clear";
    private string _stageClearTitle = "?쒖そ ?꾩떆 ?꾨Т ?꾩닔";
    private string _stageClearMessage = "?대そ?뱀씠 ?ъ젙??蹂댄꺃?쇰줈 ? 10 媛留덈땲? 轅 10 ?⑥?瑜?嫄대꽭?듬땲?? ?ㅼ쓬 湲몃줈 ?섏꽕 以鍮꾧? ?앸궗?듬땲??";
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
    /// Stage2Group??耳쒖?硫?紐⑤뱺 諛곗슦????븷?쒕? 李얘퀬 ?먯떆?몃? ?쒖옉?⑸땲??
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
    /// Stage2Group??爰쇱쭏 ???낅젰, 留덉빱, 肄붾（?? 移대찓???곹깭瑜??뺣━?⑸땲??
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
    /// ?뚮젅?댁뼱 議곗옉 ?④퀎?먯꽌 源移섏컡媛?蹂댁쑀 ?щ?? GreedyDuck ?곹샇?묒슜 媛???곹깭瑜?媛깆떊?⑸땲??
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

        GameObject fallbackObject = RequestChildObjectByName(transform, _greedyDuckFallbackObjectName);

        if (fallbackObject == null)
            return null;

        return fallbackObject.GetComponent<OOTechStageActorMotion>();
    }

    private OOTechVisibleSpriteGuard ResolveGreedyDuckGuard()
    {
        GameObject greedyDuckObject = ResolveRoleObject(_greedyDuckRoleId);

        if (greedyDuckObject == null)
            greedyDuckObject = RequestChildObjectByName(transform, _greedyDuckFallbackObjectName);

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

        return RequestChildObjectByName(transform, roleId);
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
    /// 諛곌꼍 ?꾩껜媛 移대찓?쇱뿉 ?ㅼ뼱?ㅻ룄濡?留욎텛怨? 湲곗〈 ?곕씪媛湲?移대찓?쇰뒗 ?좎떆 ?뺣땲??
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
    /// OO_Stage2CueSheet ?곗씠?곗뿉??Stage2 怨듭뿰 ?먯떆?몃? 李얠뒿?덈떎.
    /// JSON???꾩쭅 而⑤쾭?낅릺吏 ?딆븯?쇰㈃ 湲곗〈 fallback 媛믪쑝濡?怨듭뿰???댁뼱媛묐땲??
    /// </summary>
    private void ResolveCueSheetData()
    {
        Data_CueSheet = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStage2CueSheetData(_cueSheetDataId) : null;

        if (Data_CueSheet == null)
            Debug.LogWarning($"[OOTechStage2GroupController] Stage2CueSheet data missing: {_cueSheetDataId}. Fallback cue will be used.");
    }

    /// <summary>
    /// ?묒? ?먯떆??媛믪쓣 Stage2Controller???ㅽ뻾 蹂?섏뿉 ?곸슜?⑸땲??
    /// ?곹솕 鍮꾩쑀濡쒕뒗 ?ㅻ뒛 怨듭뿰 ?먯떆?몄뿉 ?곹엺 諛곗슦 ?대쫫?? ???踰덊샇, 議곕챸 ?쒓컙??臾대?媛먮룆 梨낆긽???쇱퀜 ?볥뒗 ?④퀎?낅땲??
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
    /// Stage2 ?대━??蹂댁긽???곗씠??紐⑸줉?쇰줈 媛깆떊?⑸땲??
    /// ?곹솕濡?移섎㈃ 留덉?留?而ㅽ듉肄???愿媛앹뿉寃??섎닠以??좊Ъ 紐⑸줉???먯떆?몄뿉???쎈뒗 ?④퀎?낅땲??
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
    /// Stage2 ?먯떆??蹂몃Ц?낅땲??
    /// 媛?以꾩? 媛먮룆 ?먯씠怨? ?ㅼ젣 ?대룞/?좊땲硫붿씠??UI 異쒕젰? ??븷 而댄룷?뚰듃? 留ㅻ땲??먭쾶 ?붿껌?⑸땲??
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
    /// ?대そ猷??깆옣 ?댄썑??BGM?쇰줈 援먯껜?⑸땲??
    /// ?곹솕濡?移섎㈃ 二쇱씤怨??깆옣???뚮쭏 ?뚯븙???덈줈 ?먰븯???λ㈃?낅땲??
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
            Service_Reward.RequestUpdateStageQuest(HUD_Road, _stageQuestDataId, "源移섏? 泥?뼇怨좎텛濡?源移섏컡媛쒕? 留뚮뱾怨??낅뜒?ㅻ━?먭쾶 媛?멸??몄슂.");
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
    /// 源移섏컡媛쒕? 諛쏆? GreedyDuck??遺덊?硫??댁옣?섍퀬 Stage2瑜??꾨즺?섎뒗 留덉?留??먯엯?덈떎.
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
    /// OO_StageQuest?먯꽌 ?꾨Т ?ㅻ챸??爰쇰깄?덈떎.
    /// 臾대?媛먮룆? ??щ? 吏곸젒 ?곗? ?딄퀬, 湲고쉷????곸뼱 ???먯떆??臾몄옣???쎌뼱 HUD???섍퉩?덈떎.
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
    /// Stage2媛 ?ㅼ떆 ?대┫ ??GreedyDuck 諛곗슦瑜?蹂댁씠???곹깭濡?蹂듦뎄?⑸땲??
    /// ?댁쟾 由ы뿀?ㅼ뿉???댁옣?섎ŉ 爰쇱쭊 議곕챸怨??섏긽???ㅼ떆 耳쒖꽌 泥??λ㈃???먭??ㅻ━媛 蹂댁씠寃??⑸땲??
    /// </summary>
    private void RestoreGreedyDuckView()
    {
        GameObject greedyDuckObject = Actor_GreedyDuck != null ? Actor_GreedyDuck.gameObject : ResolveRoleObject(_greedyDuckRoleId);

        if (greedyDuckObject == null)
            greedyDuckObject = RequestChildObjectByName(transform, _greedyDuckFallbackObjectName);

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
    /// Stage2 ?대━???꾩뿉??GreedyDuck??寃뚯엫 吏꾪뻾???듭떖 諛곗슦??二쇨린?곸쑝濡??쒖떆 ?곹깭瑜??뺤씤?⑸땲??
    /// ?곹솕濡?移섎㈃ ?먭??ㅻ━ 諛곗슦媛 臾대??먯꽌 ?щ씪吏硫??ㅼ쓬 ?λ㈃??留됲엳誘濡? 議곕챸 ?대떦?먭쾶 怨꾩냽 ?곹깭 泥댄겕瑜?留↔린???덉쟾?μ튂?낅땲??
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
    /// GreedyDuck??EntryPoint_E濡??댁옣???뚮쭔 ?꾩떆 諛쒗뙋 Collider瑜?耳쒓퀬, ?댁옣 ???ㅼ떆 ?뺣땲??
    /// ?곹솕濡?移섎㈃ 諛곗슦媛 臾대? 諛뽰쑝濡??덉쟾?섍쾶 ?섍??꾨줉 ?좉퉸 ?볥뒗 ?대룞??諛쏆묠??낅땲??
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
        GameObject canvasObject = RequestChildObjectByName(transform, _stageClearCanvasName);

        if (canvasObject == null)
            canvasObject = RequestChildObjectByName(transform, _placeholderCanvasName);

        if (canvasObject == null)
            return;

        Object_ClearCanvas = canvasObject;
        Button_NextStage = RequestChildObjectByName(canvasObject.transform, _nextButtonName)?.GetComponent<Button>();

        if (Button_NextStage == null)
            Button_NextStage = canvasObject.GetComponentInChildren<Button>(true);

        if (Button_NextStage != null)
        {
            Button_NextStage.onClick.RemoveListener(OnNextStageButtonClicked);
            Button_NextStage.onClick.AddListener(OnNextStageButtonClicked);
        }

        ApplyStage2ClearPanelText();
    }

    private void SetClearButtonActive(bool isActive)
    {
        if (Object_ClearCanvas != null)
            Object_ClearCanvas.SetActive(isActive);
    }

    /// <summary>
    /// Stage2 ?꾨Т?꾩닔 ?⑤꼸??蹂댁긽 臾멸뎄瑜?諛섏쁺?⑸땲??
    /// Stage1???붾뵫 ?먮쭑?먭낵 媛숈? ??븷?대ŉ, Game View?먯꽌??蹂댁긽 ?덈궡? ?섏뼱媛湲?踰꾪듉???④퍡 蹂댁뿬以띾땲??
    /// </summary>
    private void ApplyStage2ClearPanelText()
    {
        if (Object_ClearCanvas == null)
            return;

        Canvas canvas = Object_ClearCanvas.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 5200;
        }

        CanvasScaler canvasScaler = Object_ClearCanvas.GetComponent<CanvasScaler>();

        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        RectTransform clearPanelRect = RequestChildObjectByName(Object_ClearCanvas.transform, "Panel_StageClear")?.GetComponent<RectTransform>();

        if (clearPanelRect != null)
        {
            clearPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            clearPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            clearPanelRect.pivot = new Vector2(0.5f, 0.5f);
            clearPanelRect.anchoredPosition = new Vector2(0f, 36f);
            clearPanelRect.sizeDelta = new Vector2(1380f, 430f);
            clearPanelRect.localScale = Vector3.one;
        }

        Image clearPanelImage = clearPanelRect != null ? clearPanelRect.GetComponent<Image>() : null;

        if (clearPanelImage != null)
        {
            clearPanelImage.color = new Color(0f, 0f, 0f, 0.58f);
            clearPanelImage.raycastTarget = true;
        }

        TextMeshProUGUI[] textArray = Object_ClearCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI text in textArray)
        {
            if (text == null)
                continue;

            OOTechTMPFontUtility.ApplyProjectFont(text);

            if (text.name == "Text_ClearTitle")
            {
                text.text = _stageClearTitle;
                text.fontSize = 42f;
                text.color = Color.white;
            }
            else if (text.name == "Text_ClearMessage")
            {
                text.text = _stageClearMessage;
                text.fontSize = 28f;
                text.color = Color.white;
            }
            else if (text.name == "Text_Label")
            {
                text.text = "넘어가기";
                text.fontSize = 30f;
                text.color = Color.black;
            }
        }
    }

    private void OnNextStageButtonClicked()
    {
        if (OOTechUIManager.Inst != null)
        {
            GameObject nextGroup = RequestSceneObjectByName(_nextRoadGroupName);

            if (nextGroup != null)
                OOTechUIManager.Inst.RegisterUI(_nextRoadGroupName, nextGroup);

            OOTechUIManager.Inst.CloseUI(gameObject.name);
            OOTechUIManager.Inst.OpenUI(_nextRoadGroupName);
            return;
        }

        GameObject nextGroupObject = RequestSceneObjectByName(_nextRoadGroupName);
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

    private GameObject RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(objectName))
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
}

