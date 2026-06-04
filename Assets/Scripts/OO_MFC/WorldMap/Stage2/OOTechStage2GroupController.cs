// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechStage2GroupController.cs
// - 역할: Stage2Group의 큐시트 순서만 지휘하는 Controller입니다.
// - 영화 비유: 무대감독은 "세 배우 입장 -> 악덕오리 대사 -> 몽룡 등장 -> 요리 임무 -> 퇴장" 큐만 부릅니다.
// - 유지보수 포인트: 배우 이동은 OOTechStageActorMotion, UI는 HUD/Dialog, 데이터는 GameDataManager가 맡습니다.
// =============================================================================
using System;
using System.Collections;
using TMPro;
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

    [Header("Role Id")]
    [SerializeField] private string _moranRoleId = "Moran";
    [SerializeField] private string _mrJaeikRoleId = "Mr.Jaeik";
    [SerializeField] private string _chunyangRoleId = "Chunyang";
    [SerializeField] private string _greedyDuckRoleId = "GreedyDuck";
    [SerializeField] private string _leeMongRyongRoleId = "LeeMongRyong";
    [SerializeField] private string _backgroundRoleId = "Stage2Background";
    [SerializeField] private string _arriveEffectRoleId = "ArriveEffect";
    [SerializeField] private string _entryPointAId = "EntryPoint_A";
    [SerializeField] private string _entryPointBId = "EntryPoint_B";
    [SerializeField] private string _entryPointCId = "EntryPoint_C";
    [SerializeField] private string _entryPointDId = "EntryPoint_D";
    [SerializeField] private string _entryPointEId = "EntryPoint_E";
    [SerializeField] private string _tempColliderRoleId = "Collider_Temp";

    [Header("Data Id")]
    [SerializeField] private string _roadMissionDataId = "Stage2_Road_Quest_01";
    [SerializeField] private string _roadMissionFallbackText = "서쪽 도시에 가 탐관오리의 자택을 방문하세요.";
    [SerializeField] private string _stageQuestDataId = "Stage2_Quest_01";
    [SerializeField] private string _greedyDuckFirstDialogueId = "character_GreedyDuck_01";
    [SerializeField] private string _greedyDuckSecondDialogueId = "character_GreedyDuck_02";
    [SerializeField] private string _leeMongRyongFirstDialogueId = "character_LeeMongRyong_01";
    [SerializeField] private string _leeMongRyongSecondDialogueId = "character_LeeMongRyong_02";
    [SerializeField] private string _moranQuestDialogueId = "character_Moran_07";
    [SerializeField] private string _greedyDuckFinalDialogueId = "character_GreedyDuck_03";
    [SerializeField] private string[] _endingDialogueIdArray =
    {
        "character_LeeMongRyong_03",
        "character_LeeMongRyong_04",
        "character_LeeMongRyong_05",
        "character_Chunyang_09"
    };

    [Header("Inventory")]
    [SerializeField] private string _kimchiStewCookId = "OO_KimchiStew_1";
    [SerializeField] private string _honeyIngredientId = "Ing_Honey_01";
    [SerializeField] private int _honeyRewardCount = 10;

    [Header("Flow")]
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private string _nextRoadGroupName = "3rd_Road_to_Stage3";
    [SerializeField] private string _placeholderCanvasName = "Canvas_StagePlaceholder";
    [SerializeField] private string _nextButtonName = "Button_NextStage";
    [SerializeField] private float _entryMoveSpeed = 145f;
    [SerializeField] private float _greedyDuckEscapeSpeed = 360f;
    [SerializeField] private float _forcedDialogueSeconds = 4f;
    [SerializeField] private float _finalDuckDialogueSeconds = 3f;
    [SerializeField] private float _arriveEffectSeconds = 3.4f;
    [SerializeField] private float _arriveEffectAnimationSpeed = 0.5f;
    [SerializeField] private float _cameraMoveSeconds = 0.6f;
    [SerializeField] private float _cameraZoomSize = 230f;

    [Header("Marker")]
    [SerializeField] private string _markerText = "\u25BC";
    [SerializeField] private string _greedyDuckMarkerObjectName = "Marker_GreedyDuck_InteractionArrow";
    [SerializeField] private Color _markerColor = Color.red;
    [SerializeField] private Vector3 _greedyDuckMarkerOffset = new Vector3(0f, 95f, 0f);
    [SerializeField] private float _greedyDuckInteractionDistance = 190f;
    [SerializeField] private KeyCode _interactionKey = KeyCode.E;

    private OOTechStageActorMotion Actor_Moran;
    private OOTechStageActorMotion Actor_MrJaeik;
    private OOTechStageActorMotion Actor_Chunyang;
    private OOTechStageActorMotion Actor_GreedyDuck;
    private OOTechStageActorMotion Actor_LeeMongRyong;
    private OOTechNPCInteractionActor Actor_GreedyDuckInteraction;
    private OOTechWorldInteractionMarker Marker_GreedyDuck;
    private GameObject Object_ArriveEffect;
    private GameObject Object_TempCollider;
    private GameObject Object_ClearCanvas;
    private Button Button_NextStage;
    private DialogueUI UI_Dialogue;
    private Camera Camera_Main;
    private CameraFollowController Camera_Follow;
    private Bounds _stageBounds;
    private bool _isCuePlaying;
    private bool _isPlayerPhase;
    private bool _isStageClear;
    private bool _hasSavedCameraFollowState;
    private bool _savedCameraFollowEnabled;
    private Coroutine Coroutine_Victory;

    /// <summary>
    /// Stage2Group이 켜지면 모든 배우의 역할표를 찾고 큐시트를 시작합니다.
    /// </summary>
    private void OnEnable()
    {
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
        SetGreedyDuckInteractionEnabled(false);
        RequestSetTempColliderActive(false);
        SetClearButtonActive(false);
        RestoreCameraFollow();
    }

    /// <summary>
    /// 플레이어 조작 단계에서 김치찌개 보유 여부와 GreedyDuck 상호작용 가능 상태를 갱신합니다.
    /// </summary>
    private void Update()
    {
        if (!_isPlayerPhase || _isStageClear)
            return;

        bool hasStew = OOTechGameManager.Inst != null && OOTechGameManager.Inst.GetItemCount(_kimchiStewCookId) > 0;
        SetGreedyDuckInteractionEnabled(hasStew);
    }

    private void ResolveReferences()
    {
        Context_Scene = Context_Scene != null ? Context_Scene : GetComponent<OOTechSceneContext>();
        HUD_Road = HUD_Road != null ? HUD_Road : GetComponent<OOTechRoadHUDController>();

        if (Context_Scene != null)
            Context_Scene.CacheSceneObjects();

        Actor_Moran = ResolveActor(_moranRoleId);
        Actor_MrJaeik = ResolveActor(_mrJaeikRoleId);
        Actor_Chunyang = ResolveActor(_chunyangRoleId);
        Actor_GreedyDuck = ResolveActor(_greedyDuckRoleId);
        Actor_LeeMongRyong = ResolveActor(_leeMongRyongRoleId);
        Object_ArriveEffect = ResolveRoleObject(_arriveEffectRoleId);
        Object_TempCollider = ResolveRoleObject(_tempColliderRoleId);
        Camera_Main = Camera.main;

        if (Camera_Main != null && Camera_Follow == null)
            Camera_Main.TryGetComponent(out Camera_Follow);

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
        SaveAndDisableCameraFollow();
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
        HUD_Road.RequestSetRoadMissionText(ResolveStageQuestDescription(_roadMissionDataId, _roadMissionFallbackText), true);
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

        GameObject markerObject = FindChildByName(greedyDuckObject.transform, _greedyDuckMarkerObjectName);

        if (markerObject != null)
        {
            Marker_GreedyDuck = markerObject.GetComponent<OOTechWorldInteractionMarker>();

            if (Marker_GreedyDuck == null)
                Marker_GreedyDuck = markerObject.AddComponent<OOTechWorldInteractionMarker>();

            Marker_GreedyDuck.RequestSetup(greedyDuckObject.transform, _greedyDuckMarkerOffset, _markerText, _markerColor);
        }
        else
        {
            Debug.LogWarning($"[OOTechStage2GroupController] Marker object missing: {_greedyDuckMarkerObjectName}");
        }

        SetGreedyDuckInteractionEnabled(false);
    }

    private void StartStage2Cue()
    {
        if (_isCuePlaying)
            return;

        StartCoroutine(PlayStage2CueRoutine());
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
        if (_stage2BGM != null && OOTechSoundManager.Inst != null)
            OOTechSoundManager.Inst.PlayBGM(_stage2BGM, true);
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
        yield return ShowDialogueAndWait(_leeMongRyongFirstDialogueId);

        PlayReactionActors(true);

        if (Actor_LeeMongRyong != null)
        {
            Actor_LeeMongRyong.RequestPlayState("LeeMongRyong_isArrived", 0.7f, true);
            yield return new WaitForSeconds(GetAnimationClipLength(Actor_LeeMongRyong, "LeeMongRyong_isArrived", 1.1f) / 0.7f);
            Actor_LeeMongRyong.RequestPlayState("LeeMongRyong_idle2", 1f, true);
        }

        yield return ShowDialogueAndWait(_leeMongRyongSecondDialogueId);
        PlayReactionActors(false);
    }

    private IEnumerator PlayQuestDialogueRoutine()
    {
        yield return ShowDialogueAndWait(_moranQuestDialogueId);
        RequestUpdateStageQuest(_stageQuestDataId, "김치와 청양고추로 김치찌개를 만들고 악덕오리에게 가져가세요.");
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
            Vector3 escapePosition = escapeTransform != null ? escapeTransform.position : Actor_GreedyDuck.transform.position + Vector3.right * 500f;
            RequestSetTempColliderActive(true);
            yield return Actor_GreedyDuck.MoveToWorldPositionRoutine(escapePosition, _greedyDuckEscapeSpeed, "GreedyDuck_isBurning");
            yield return BlinkAndHideRoutine(Actor_GreedyDuck.gameObject);
            RequestSetTempColliderActive(false);
        }

        if (Actor_LeeMongRyong != null)
            yield return FocusCameraOnActorRoutine(Actor_LeeMongRyong.transform, _cameraZoomSize, _cameraMoveSeconds);

        yield return PlayEndingDialogueRoutine();
        CompleteStage2();
    }

    private IEnumerator PlayEndingDialogueRoutine()
    {
        for (int index = 0; index < _endingDialogueIdArray.Length; index++)
        {
            string dialogueId = _endingDialogueIdArray[index];

            if (dialogueId == "character_Chunyang_09" && Actor_Chunyang != null)
                Actor_Chunyang.RequestPlayReaction(1f);

            yield return ShowDialogueAndWait(dialogueId);
        }
    }

    private void CompleteStage2()
    {
        if (OOTechGameManager.Inst != null)
            OOTechGameManager.Inst.AddItem(_honeyIngredientId, _honeyRewardCount);

        if (HUD_Road != null)
        {
            HUD_Road.RequestRefreshInventoryView();
            HUD_Road.SetInventoryNewBadgeActive(true);
            HUD_Road.RequestSetStageQuestMission("Stage2 임무 완료: 다음 길로 이동하세요.");
            HUD_Road.SetMissionNewBadgeActive(true);
        }

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
        UI_Dialogue = ResolveDialogueUI();

        if (UI_Dialogue == null)
            yield break;

        OO_Dialogue dialogueData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetDialogueData(dialogueId) : null;

        if (dialogueData == null)
        {
            Debug.LogWarning($"[OOTechStage2GroupController] Dialogue data missing: {dialogueId}");
            yield break;
        }

        bool isDone = false;
        UI_Dialogue.RequestRoadViewLayout();
        UI_Dialogue.ShowDialogue(dialogueData, delegate { isDone = true; });
        yield return new WaitUntil(() => isDone);
        CloseDialogueUI();
    }

    private IEnumerator ShowDialogueForSeconds(string dialogueId, float seconds)
    {
        UI_Dialogue = ResolveDialogueUI();

        if (UI_Dialogue == null)
            yield break;

        OO_Dialogue dialogueData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetDialogueData(dialogueId) : null;

        if (dialogueData == null)
        {
            Debug.LogWarning($"[OOTechStage2GroupController] Timed dialogue data missing: {dialogueId}");
            yield break;
        }

        bool isDone = false;
        UI_Dialogue.RequestRoadViewLayout();
        UI_Dialogue.ShowDialogue(dialogueData, delegate { isDone = true; });

        float elapsedTime = 0f;

        while (!isDone && elapsedTime < seconds)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        CloseDialogueUI();
    }

    private DialogueUI ResolveDialogueUI()
    {
        if (UI_Dialogue != null)
            return UI_Dialogue;

        GameObject dialogueObject = OOTechUIManager.Inst != null ? OOTechUIManager.Inst.GetCreatedUI(_dialogueGroupName) : null;

        if (dialogueObject == null)
            dialogueObject = FindSceneObjectByName(_dialogueGroupName);

        if (dialogueObject == null)
            return null;

        dialogueObject.SetActive(true);
        UI_Dialogue = dialogueObject.GetComponentInChildren<DialogueUI>(true);
        return UI_Dialogue;
    }

    private void CloseDialogueUI()
    {
        if (UI_Dialogue != null)
            UI_Dialogue.CloseDialogue();

        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.CloseUI(_dialogueGroupName);
    }

    private void RequestUpdateStageQuest(string stageQuestDataId, string fallbackText)
    {
        string questText = ResolveStageQuestDescription(stageQuestDataId, fallbackText);

        if (HUD_Road != null)
        {
            HUD_Road.RequestSetStageQuestMission(questText);
            HUD_Road.SetMissionNewBadgeActive(true);
        }
    }

    /// <summary>
    /// OO_StageQuest에서 임무 설명을 꺼냅니다.
    /// 무대감독은 대사를 직접 쓰지 않고, 기획팀이 적어 둔 큐시트 문장을 읽어 HUD에 넘깁니다.
    /// </summary>
    private string ResolveStageQuestDescription(string stageQuestDataId, string fallbackText)
    {
        OO_StageQuest questData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStageQuestData(stageQuestDataId) : null;

        if (questData != null && !string.IsNullOrWhiteSpace(questData.Description))
            return questData.Description;

        return fallbackText;
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
        if (Camera_Main == null || targetTransform == null)
            yield break;

        Vector3 startPosition = Camera_Main.transform.position;
        Vector3 endPosition = targetTransform.position;
        endPosition.z = startPosition.z;
        float startSize = Camera_Main.orthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / Mathf.Max(0.01f, duration));
            Camera_Main.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            Camera_Main.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }
    }

    private void FocusCameraOnBounds(Bounds bounds, float padding)
    {
        if (Camera_Main == null)
            return;

        Camera_Main.orthographic = true;
        Vector3 cameraPosition = bounds.center;
        cameraPosition.z = Camera_Main.transform.position.z;
        Camera_Main.transform.position = cameraPosition;
        float aspect = Mathf.Max(0.01f, Camera_Main.aspect);
        float sizeByHeight = bounds.extents.y;
        float sizeByWidth = bounds.extents.x / aspect;
        Camera_Main.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth) * Mathf.Max(1f, padding);
    }

    private void SaveAndDisableCameraFollow()
    {
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
        if (targetObject == null)
            yield break;

        SpriteRenderer renderer = targetObject.GetComponentInChildren<SpriteRenderer>(true);

        for (int index = 0; index < 4; index++)
        {
            if (renderer != null)
                renderer.enabled = !renderer.enabled;

            yield return new WaitForSeconds(0.12f);
        }

        if (renderer != null)
            renderer.enabled = false;

        targetObject.SetActive(false);
    }

    private void SetGreedyDuckInteractionEnabled(bool isEnabled)
    {
        if (Actor_GreedyDuckInteraction != null)
            Actor_GreedyDuckInteraction.RequestSetInteractable(isEnabled);

        if (Marker_GreedyDuck != null)
            Marker_GreedyDuck.RequestSetVisible(isEnabled);
    }

    /// <summary>
    /// Stage2가 다시 열릴 때 GreedyDuck 배우를 보이는 상태로 복구합니다.
    /// 이전 리허설에서 퇴장하며 꺼진 조명과 의상을 다시 켜서 첫 장면에 탐관오리가 보이게 합니다.
    /// </summary>
    private void RestoreGreedyDuckView()
    {
        if (Actor_GreedyDuck == null)
            return;

        Actor_GreedyDuck.gameObject.SetActive(true);

        SpriteRenderer[] rendererArray = Actor_GreedyDuck.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in rendererArray)
        {
            if (renderer == null)
                continue;

            renderer.enabled = true;
            Color color = renderer.color;
            color.a = 1f;
            renderer.color = color;
            renderer.sortingOrder = Mathf.Max(renderer.sortingOrder, 25);
        }

        Animator animator = Actor_GreedyDuck.GetComponentInChildren<Animator>(true);

        if (animator != null)
        {
            animator.enabled = true;
            animator.speed = 1f;
        }

        Actor_GreedyDuck.RequestPlayIdle();
    }

    /// <summary>
    /// GreedyDuck이 EntryPoint_E로 퇴장할 때만 임시 발판 Collider를 켜고, 퇴장 후 다시 끕니다.
    /// 영화로 치면 배우가 무대 밖으로 안전하게 나가도록 잠깐 놓는 이동용 받침대입니다.
    /// </summary>
    private void RequestSetTempColliderActive(bool isActive)
    {
        if (Object_TempCollider == null)
            Object_TempCollider = ResolveRoleObject(_tempColliderRoleId);

        if (Object_TempCollider == null)
            return;

        Object_TempCollider.SetActive(isActive);
        Collider2D[] colliderArray = Object_TempCollider.GetComponentsInChildren<Collider2D>(true);

        foreach (Collider2D collider in colliderArray)
        {
            if (collider != null)
                collider.enabled = isActive;
        }
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
