// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechStage4GroupController.cs
// - 역할: Stage4 토끼/거북이 마지막 임무와 발표용 진행 보장을 담당합니다.
// - 유지보수: Moran 표시, SpeechBubble, CookingGroup 왕복 보험은 엔딩 진행에 직접 연결되므로 임의 삭제하지 않습니다.
// =============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class OOTechStage4GroupController : MonoBehaviour
{
    private const string DefaultCueSheetId = "Stage4_CueSheet_01";
    private const string Stage4_1Name = "Stage4_1Group";
    private const string Stage4_2Name = "Stage4_2Group";
    private const float StageLaneNormalizedHeight = 0.2f;
    private const float LargeStageBackgroundWidth = 40f;
    private const float LargeStageEdgeMargin = 120f;
    private const float SmallStageEdgeMargin = 18f;
    private const float EdgeReachTolerance = 2f;
    private const float EdgeRearmDistance = 48f;
    private const float Stage4_2VisibleStartViewportX = 0.12f;

    [Header("Data")]
    [SerializeField] private string _cueSheetDataId = DefaultCueSheetId;

    [Header("BGM")]
    [SerializeField] private AudioClip Clip_Stage4BGM;

    private static bool _isTurtleQuestAccepted;
    private static bool _isStopPointATriggered;
    private static bool _isRabbitCarrotCakeDialoguePlayed;
    private static bool _isRabbitSleeping;
    private static bool _isStage4Cleared;
    private static bool _hasStage4_2SavedMoranPosition;
    private static bool _isReturnFromStage4_2ToStage4_1;
    private static Vector3 _stage4_2SavedMoranPosition;

    private OO_Stage4CueSheet Data_CueSheet;
    private OOTechSceneContext Context_Scene;
    private OOTechStage3DialogueCue Cue_Dialogue;
    private OOTechRoadHUDController HUD_Road;
    private OOTechStageMoranFreeMoveController Controller_MoranMove;
    private OOTechStage4ClearPanelView View_ClearPanel;
    private Transform Transform_Moran;
    private Transform Transform_Turtle;
    private Transform Transform_Rabbit;
    private Transform Transform_RabbitSleeping;
    private Transform Transform_Stump;
    private Transform Transform_Stump2;
    private Transform Transform_StopPointA;
    private SpriteRenderer Renderer_Background;
    private Coroutine Coroutine_Sequence;
    private Coroutine Coroutine_ClearFailSafe;
    private bool _isRunningSequence;
    private bool _isStage4ClearTransitionRequested;
    private bool _isStage4_1RightExitArmed = true;
    private bool _isStage4_2LeftExitArmed;
    private Transform Transform_Stage4_2PresentationMoranClone;

    /// <summary>
    /// Stage4 洹몃９??耳쒖쭏 ???꾩옱 洹몃９ ?대쫫??留욌뒗 ?먮? 以鍮꾪빀?덈떎.
    /// Game View?먯꽌??Stage4_1?대㈃ 嫄곕턿???섏뒪?? Stage4_2?대㈃ ?좊겮 ?섏뒪?몃? ?댁뼱媛묐땲??
    /// </summary>
    private void OnEnable()
    {
        ResolveComponents();
        ResolveCueSheetData();
        RequestPlayStage4BGM();
        PrepareStageView();
        PrepareMoranControl(true);

        if (gameObject.name == ResolveStage4_1GroupName())
            PrepareStage4_1();
        else if (gameObject.name == ResolveStage4_2GroupName())
            PrepareStage4_2();
    }

    /// <summary>
    /// Stage4_2?먯꽌 遺?뚯쑝濡??섍???留덉?留??꾩튂瑜?湲곗뼲?섍린 ?꾪빐 鍮꾪솢??吏곸쟾??醫뚰몴瑜???ν빀?덈떎.
    /// </summary>
    private void OnDisable()
    {
        if (gameObject.name == ResolveStage4_2GroupName() && Transform_Moran != null)
        {
            _stage4_2SavedMoranPosition = Transform_Moran.position;
            _hasStage4_2SavedMoranPosition = true;
        }

        if (Coroutine_Sequence != null)
        {
            StopCoroutine(Coroutine_Sequence);
            Coroutine_Sequence = null;
        }

        if (Coroutine_ClearFailSafe != null)
        {
            StopCoroutine(Coroutine_ClearFailSafe);
            Coroutine_ClearFailSafe = null;
        }

        _isRunningSequence = false;
    }

    /// <summary>
    /// Stage4??E ?곹샇?묒슜怨??붾㈃ ???꾪솚??留??꾨젅???뺤씤?⑸땲??
    /// Controller???먯젙留??섍퀬, ?ㅼ젣 ?좊땲硫붿씠????щ뒗 媛???븷 而댄룷?뚰듃??留↔퉩?덈떎.
    /// </summary>
    private void Update()
    {
        RequestKeepPresentationInsuranceAlive();
        RequestKeepStage4_1RabbitVisible();

        if (_isRunningSequence)
            return;

        if (gameObject.name == ResolveStage4_1GroupName())
            UpdateStage4_1Input();
        else if (gameObject.name == ResolveStage4_2GroupName())
            UpdateStage4_2Input();
    }

    private void ResolveComponents()
    {
        if (Context_Scene == null)
            Context_Scene = GetComponent<OOTechSceneContext>();

        if (Context_Scene == null)
            Context_Scene = gameObject.AddComponent<OOTechSceneContext>();

        Context_Scene.CacheSceneObjects();

        if (Cue_Dialogue == null)
            Cue_Dialogue = GetComponent<OOTechStage3DialogueCue>();

        if (Cue_Dialogue == null)
            Cue_Dialogue = gameObject.AddComponent<OOTechStage3DialogueCue>();

        HUD_Road = GetComponent<OOTechRoadHUDController>();

        if (HUD_Road == null)
            HUD_Road = gameObject.AddComponent<OOTechRoadHUDController>();

        Transform_Moran = ResolveRoleTransform(ResolveMoranRoleId(), "Moran");

        if (gameObject.name == ResolveStage4_2GroupName() || !IsRenderableActor(Transform_Moran))
            Transform_Moran = RequestEnsureStage4_2PresentationMoran();

        Transform_Turtle = ResolveRoleTransform(ResolveTurtleRoleId(), "Turtle");
        Transform_Rabbit = ResolveRoleTransform(ResolveRabbitRoleId(), "Rabbit");
        Transform_RabbitSleeping = ResolveRoleTransform(ResolveSleepingRabbitRoleId(), "Rabbit_isSleeping");
        Transform_Stump = ResolveRoleTransform(ResolveStumpRoleId(), "Stump");
        Transform_Stump2 = ResolveRoleTransform(ResolveStump2RoleId(), "Stump2");
        Transform_StopPointA = ResolveRoleTransform(ResolveStopPointARoleId(), "StopPoint_A");
        Renderer_Background = ResolveBackgroundRenderer();

        if (Transform_Moran != null)
        {
            Controller_MoranMove = Transform_Moran.GetComponent<OOTechStageMoranFreeMoveController>();

            if (Controller_MoranMove == null)
                Controller_MoranMove = Transform_Moran.gameObject.AddComponent<OOTechStageMoranFreeMoveController>();
        }

        if (View_ClearPanel == null)
            View_ClearPanel = GetComponentInChildren<OOTechStage4ClearPanelView>(true);
    }

    private void ResolveCueSheetData()
    {
        Data_CueSheet = OOTechGameDataManager.Inst != null
            ? OOTechGameDataManager.Inst.GetStage4CueSheetData(_cueSheetDataId)
            : null;
    }

    private void PrepareStageView()
    {
        OOTechStageViewController stageViewController = GetComponent<OOTechStageViewController>();

        if (stageViewController != null)
        {
            string backgroundObjectName = gameObject.name == ResolveStage4_2GroupName() ? "Stage4_2Background" : "Stage4_1Background";
            stageViewController.Configure(backgroundObjectName, true);
            stageViewController.enabled = true;
        }

        if (HUD_Road != null)
        {
            HUD_Road.SetOwnerGroupName(gameObject.name);
            HUD_Road.PrepareHUD();
            HUD_Road.SetCookingUnlocked(true, false);
            HUD_Road.SetHUDVisible(true);
            RequestForcePresentationHUD();
        }

        RequestFitCameraToBackground();
    }

    private void PrepareStage4_1()
    {
        RequestSetActive(Transform_Turtle, true);
        RequestSetActive(Transform_Rabbit, !_isRabbitSleeping);
        RequestSetActive(Transform_RabbitSleeping, false);

        if (!_isTurtleQuestAccepted && !_isRabbitSleeping)
            RequestPlayActorState(Transform_Turtle, "Turtle_Idle", 1f, true);

        if (!_isRabbitSleeping)
        {
            RequestForceActorVisible(Transform_Rabbit, 2100);
            RequestPlayActorState(Transform_Rabbit, "Rabbit_isJoking", 1f, false);
            RequestHideSpeechBubble(Transform_Rabbit);
        }

        if (_isRabbitSleeping)
            RequestSetMission("토끼가 잠이 들었습니다. 거북이에게 돌아가세요!", true);
        else if (_isTurtleQuestAccepted)
            RequestSetMission("토끼를 찾으세요.", false);

        if (_isReturnFromStage4_2ToStage4_1)
        {
            PlaceMoranAtRightEdge();
            _isStage4_1RightExitArmed = false;
            _isReturnFromStage4_2ToStage4_1 = false;
        }
        else if (Transform_Moran != null && !_isTurtleQuestAccepted)
        {
            PlaceMoranAtLeftEdge();
            _isStage4_1RightExitArmed = true;
        }
        else
        {
            _isStage4_1RightExitArmed = true;
        }

        RequestForceActorVisible(Transform_Moran, 2200);
        RequestForcePresentationHUD();
    }

    private void RequestKeepStage4_1RabbitVisible()
    {
        if (gameObject.name != ResolveStage4_1GroupName() || _isRabbitSleeping || Transform_Rabbit == null)
            return;

        if (!Transform_Rabbit.gameObject.activeSelf || !IsRenderableActor(Transform_Rabbit))
        {
            RequestForceActorVisible(Transform_Rabbit, 2100);
            RequestPlayActorState(Transform_Rabbit, "Rabbit_isJoking", 1f, false);
        }
    }

    private void PrepareStage4_2()
    {
        _isStage4_2LeftExitArmed = false;
        RequestHideSpeechBubble(Transform_Rabbit);
        RequestHideSpeechBubble(Transform_RabbitSleeping);
        RequestSetActive(Transform_Turtle, false);
        RequestSetActive(Transform_Rabbit, !_isRabbitSleeping);
        RequestSetActive(Transform_RabbitSleeping, _isRabbitSleeping);
        RequestSetActive(Transform_Stump2, false);

        PlaceMoranAtStage4_2VisibleStart();

        RequestForceActorVisible(Transform_Moran, 2200);
        RequestKeepMoranInPresentationStartPositionIfNeeded();
        PrepareMoranControl(true);

        if (_isRabbitSleeping)
            RequestForceActorVisible(Transform_RabbitSleeping, 2100);
        else
            RequestForceActorVisible(Transform_Rabbit, 2100);

        if (_isStopPointATriggered && Transform_StopPointA != null)
            Transform_StopPointA.gameObject.SetActive(false);

        if (!_isRabbitSleeping)
        {
            RequestPrepareStage4_2CookingQuestHUD();
            StartManagedRoutine(PlayStage4_2IntroRoutine());
        }
        else
        {
            OOTechSpeechBubbleView sleepingBubbleView = ResolveSpeechBubbleView(Transform_RabbitSleeping);
            sleepingBubbleView?.RequestPlaySpeechBubble(ResolveRabbitSleepingBubbleId(), 1f);
            RequestSetMission("토끼가 잠이 들었습니다. 거북이에게 돌아가세요!", true);
        }
    }

    private void UpdateStage4_1Input()
    {
        if (Transform_Moran == null)
            return;

        bool isInteractionPressed = IsInteractionPressed();

        if (_isRabbitSleeping && !_isStage4Cleared && Transform_Turtle != null && IsNear(Transform_Moran, Transform_Turtle))
        {
            if (isInteractionPressed)
                StartManagedRoutine(PlayTurtleClearRoutine());

            return;
        }

        if (!_isTurtleQuestAccepted && Transform_Turtle != null && IsNear(Transform_Moran, Transform_Turtle) && isInteractionPressed)
        {
            StartManagedRoutine(PlayTurtleQuestRoutine());
            return;
        }

        if (_isTurtleQuestAccepted && !_isStage4_1RightExitArmed)
        {
            if (HasMoranMovedAwayFromRightEdge())
                _isStage4_1RightExitArmed = true;

            return;
        }

        if (_isTurtleQuestAccepted && _isStage4_1RightExitArmed && IsMoranAtRightEdge())
        {
            RequestSwitchGroup(gameObject.name, ResolveStage4_2GroupName());
        }
    }

    private void UpdateStage4_2Input()
    {
        if (Transform_Moran == null)
            return;

        if (!_isStage4_2LeftExitArmed)
        {
            if (HasMoranMovedAwayFromLeftEdge())
                _isStage4_2LeftExitArmed = true;
        }

        if (_isStage4_2LeftExitArmed && IsMoranAtLeftEdge())
        {
            _isReturnFromStage4_2ToStage4_1 = true;
            RequestSwitchGroup(gameObject.name, ResolveStage4_1GroupName());
            return;
        }

        if (!_isStopPointATriggered && Transform_StopPointA != null && Transform_StopPointA.gameObject.activeInHierarchy && IsNear(Transform_Moran, Transform_StopPointA))
        {
            StartManagedRoutine(PlayStopPointARoutine());
            return;
        }

        if (!_isRabbitCarrotCakeDialoguePlayed && HasItem(ResolveCarrotCakeItemId()))
        {
            StartManagedRoutine(PlayRabbitCarrotCakeReadyRoutine());
            return;
        }

        if (_isRabbitCarrotCakeDialoguePlayed && !_isRabbitSleeping)
            ClampMoranBeforeStump();

        if (_isRabbitCarrotCakeDialoguePlayed && !_isRabbitSleeping && Transform_Stump != null && IsNear(Transform_Moran, Transform_Stump) && IsInteractionPressed())
            StartManagedRoutine(PlayStumpCarrotCakeRoutine());
    }

    private IEnumerator PlayTurtleQuestRoutine()
    {
        PrepareMoranControl(false);
        RequestPlayActorState(Transform_Turtle, "Turtle_isBegging", 1f, true);
        yield return ShowDialogueListRoutine(ResolveTextList(ResolveTurtleDialogueIdList()));

        _isTurtleQuestAccepted = true;
        RequestSetMission("토끼를 찾으세요.", true);
        PrepareMoranControl(true);
    }

    private IEnumerator PlayStage4_2IntroRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        if (_isStopPointATriggered)
            yield break;

        yield return Cue_Dialogue.RequestShowDialogueAndWait(ResolveRabbitIntroDialogueId(), 3f);
    }

    private IEnumerator PlayStopPointARoutine()
    {
        PrepareMoranControl(false);
        _isStopPointATriggered = true;

        if (Transform_StopPointA != null)
            Transform_StopPointA.gameObject.SetActive(false);

        RequestPlayActorState(Transform_Rabbit, "Rabbit_isJoking", 1f, true);
        yield return Cue_Dialogue.RequestShowDialogueAndWait(ResolveRabbitStopDialogueId());
        RequestSetMission("토끼를 유혹하기 위한 당근전을 만드세요.", true);
        PrepareMoranControl(true);
    }

    private IEnumerator PlayRabbitCarrotCakeReadyRoutine()
    {
        PrepareMoranControl(false);
        _isRabbitCarrotCakeDialoguePlayed = true;
        RequestPlayActorState(Transform_Rabbit, "Rabbit_Idle", 1f, false);
        yield return ShowDialogueListRoutine(ResolveTextList(ResolveRabbitCarrotCakeDialogueIdList()));
        RequestSetMission("당근전을 토끼가 말한 나무터기에 올려두세요!", true);
        PrepareMoranControl(true);
    }

    private IEnumerator PlayStumpCarrotCakeRoutine()
    {
        if (!TryRemoveItem(ResolveCarrotCakeItemId(), 1))
            yield break;

        PrepareMoranControl(false);
        yield return FadeWaitRoutine(0.35f);
        RequestSetActive(Transform_Stump2, true);
        RequestSetActive(Transform_Stump, true);
        RequestRaiseStump2AboveStump();

        OOTechSpeechBubbleView speechBubbleView = ResolveSpeechBubbleView(Transform_Rabbit);
        speechBubbleView?.RequestPlaySpeechBubble("character_Rabbit_01", 1f);

        yield return MoveRabbitToStump2Routine();
        RequestSetActive(Transform_Stump2, false);
        RequestSetActive(Transform_Stump, true);
        RequestPlayActorState(Transform_Rabbit, "Rabbit_isEatting", 1f, true);

        List<string> speechIdList = ResolveTextList(ResolveRabbitSpeechBubbleIdList());

        for (int index = 1; index < speechIdList.Count; index++)
        {
            float speed = index >= 4 ? 1.8f : 1.3f;
            speechBubbleView?.RequestPlaySpeechBubble(speechIdList[index], speed);
            yield return new WaitForSeconds(index >= 4 ? 1.4f : 2.0f);
        }

        yield return new WaitForSeconds(5f);
        yield return FadeWaitRoutine(0.35f);
        RequestSetActive(Transform_Rabbit, false);
        RequestSetActive(Transform_RabbitSleeping, true);
        _isRabbitSleeping = true;

        OOTechSpeechBubbleView sleepingBubbleView = ResolveSpeechBubbleView(Transform_RabbitSleeping);
        sleepingBubbleView?.RequestPlaySpeechBubble(ResolveRabbitSleepingBubbleId(), 1f);
        RequestSetMission("토끼가 잠이 들었습니다. 거북이에게 돌아가세요!", true);
        PrepareMoranControl(true);
    }

    private IEnumerator PlayTurtleClearRoutine()
    {
        PrepareMoranControl(false);
        yield return ShowDialogueListRoutine(ResolveTextList(ResolveTurtleClearDialogueIdList()));
        _isStage4Cleared = true;
        OOTechGameManager.Inst?.MarkStageCleared("Stage4");
        RequestPlayActorState(Transform_Turtle, "Turtle_Victory", 1f, true);
        RequestPlayActorState(Transform_Moran, "Moran_Victory", 1f, true);
        ShowStage4ClearCanvas();
    }

    private IEnumerator ShowDialogueListRoutine(List<string> dialogueIdList)
    {
        foreach (string dialogueId in dialogueIdList)
        {
            if (string.IsNullOrEmpty(dialogueId))
                continue;

            yield return Cue_Dialogue.RequestShowDialogueAndWait(dialogueId);
        }
    }

    private IEnumerator MoveRabbitToStump2Routine()
    {
        if (Transform_Rabbit == null || Transform_Stump2 == null)
            yield break;

        RequestPlayActorState(Transform_Rabbit, "Rabbit_isRunning", 1f, true);
        float timeout = Data_CueSheet != null && Data_CueSheet.RabbitReachTimeoutSeconds > 0f ? Data_CueSheet.RabbitReachTimeoutSeconds : 3f;
        float speed = Data_CueSheet != null && Data_CueSheet.RabbitRunSpeed > 0f ? Data_CueSheet.RabbitRunSpeed : 480f;
        float elapsedTime = 0f;

        while (Vector2.Distance(Transform_Rabbit.position, Transform_Stump2.position) > 8f && elapsedTime < timeout)
        {
            elapsedTime += Time.deltaTime;
            Vector3 beforePosition = Transform_Rabbit.position;
            Transform_Rabbit.position = Vector3.MoveTowards(Transform_Rabbit.position, Transform_Stump2.position, speed * Time.deltaTime);
            UpdateFlipByDelta(Transform_Rabbit, Transform_Rabbit.position - beforePosition);
            yield return null;
        }

        Transform_Rabbit.position = Transform_Stump2.position;
    }

    private IEnumerator FadeWaitRoutine(float seconds)
    {
        yield return new WaitForSeconds(Mathf.Max(0.01f, seconds));
    }

    private void StartManagedRoutine(IEnumerator routine)
    {
        if (Coroutine_Sequence != null)
            StopCoroutine(Coroutine_Sequence);

        Coroutine_Sequence = StartCoroutine(ManagedRoutine(routine));
    }

    private IEnumerator ManagedRoutine(IEnumerator routine)
    {
        _isRunningSequence = true;
        yield return routine;
        _isRunningSequence = false;
        Coroutine_Sequence = null;
    }

    private void RequestSetMission(string missionText, bool isNew)
    {
        if (HUD_Road == null)
            HUD_Road = GetComponent<OOTechRoadHUDController>();

        if (HUD_Road == null)
            return;

        HUD_Road.RequestSetStageQuestMission(missionText);
        HUD_Road.SetMissionNewBadgeActive(isNew);
    }

    private void RequestPrepareStage4_2CookingQuestHUD()
    {
        if (HUD_Road == null)
            HUD_Road = GetComponent<OOTechRoadHUDController>();

        if (HUD_Road == null)
            return;

        HUD_Road.SetOwnerGroupName(gameObject.name);
        HUD_Road.PrepareHUD();
        HUD_Road.SetHUDVisible(true);
        HUD_Road.SetCookingUnlocked(true, true);
        HUD_Road.SetCookingNewBadgeActive(true);
        RequestSetMission(ResolveStage4CarrotCakeMissionText(), true);
        HUD_Road.RequestEnableStage4PresentationHUD(gameObject.name, ResolveStage4CarrotCakeMissionText(), "당근전 조리 가이드", ResolveStage4CarrotCakeGuideText());
    }

    private void RequestForcePresentationHUD()
    {
        if (HUD_Road == null)
            HUD_Road = GetComponent<OOTechRoadHUDController>();

        if (HUD_Road == null)
            return;

        string missionText = _isRabbitSleeping
            ? "토끼가 잠이 들었습니다. 거북이에게 돌아가세요!"
            : ResolveStage4CarrotCakeMissionText();

        HUD_Road.RequestEnableStage4PresentationHUD(gameObject.name, missionText, "당근전 조리 가이드", ResolveStage4CarrotCakeGuideText());
    }

    private void RequestKeepPresentationInsuranceAlive()
    {
        if (gameObject.name == ResolveStage4_2GroupName())
        {
            Transform_Moran = RequestEnsureStage4_2PresentationMoran();
            RequestForceActorVisible(Transform_Moran, 2200);
            RequestKeepMoranInPresentationStartPositionIfNeeded();
            PrepareMoranControl(true);
        }
        else if (gameObject.name == ResolveStage4_1GroupName())
        {
            RequestForceActorVisible(Transform_Moran, 2200);

            if (_isStage4Cleared)
            {
                PrepareMoranControl(false);
                RequestKeepStage4ClearVictoryActorsAlive();
            }
            else
            {
                PrepareMoranControl(true);
            }
        }

        if (_isStage4Cleared)
            HUD_Road?.RequestForceHideForCutScene();
        else
            RequestForcePresentationHUD();
    }

    private void RequestKeepStage4ClearVictoryActorsAlive()
    {
        if (!_isStage4Cleared)
            return;

        RequestForceActorVisible(Transform_Moran, 2200);
        RequestForceActorVisible(Transform_Turtle, 2100);
        RequestPlayActorState(Transform_Moran, "Moran_Victory", 1f, false);
        RequestPlayActorState(Transform_Turtle, "Turtle_Victory", 1f, false);
    }

    private Transform RequestEnsureStage4_2PresentationMoran()
    {
        Transform currentMoranTransform = ResolveRoleTransform(ResolveMoranRoleId(), "Moran");

        if (IsRenderableActor(currentMoranTransform))
            return currentMoranTransform;

        if (Transform_Stage4_2PresentationMoranClone != null)
            return Transform_Stage4_2PresentationMoranClone;

        Transform sourceMoranTransform = ResolveMoranFromStage4_1();

        if (sourceMoranTransform == null)
            return currentMoranTransform;

        Transform_Stage4_2PresentationMoranClone = Instantiate(sourceMoranTransform, transform);
        Transform_Stage4_2PresentationMoranClone.name = "Moran";
        Transform_Stage4_2PresentationMoranClone.SetAsLastSibling();
        RequestForceActorVisible(Transform_Stage4_2PresentationMoranClone, 2200);
        return Transform_Stage4_2PresentationMoranClone;
    }

    private Transform ResolveMoranFromStage4_1()
    {
        GameObject stage4_1Object = RequestSceneObjectByName(ResolveStage4_1GroupName());

        if (stage4_1Object == null)
            return ResolveSceneWideMoranSource();

        Transform moranTransform = RequestChildObjectByName(stage4_1Object.transform, "Moran");

        if (moranTransform != null)
            return moranTransform;

        OOTechSceneObject[] sceneObjectArray = stage4_1Object.GetComponentsInChildren<OOTechSceneObject>(true);

        foreach (OOTechSceneObject sceneObject in sceneObjectArray)
        {
            if (sceneObject != null && sceneObject.RoleId == ResolveMoranRoleId())
                return sceneObject.transform;
        }

        return ResolveSceneWideMoranSource();
    }

    private Transform ResolveSceneWideMoranSource()
    {
        string preferredGroupName = gameObject.name == ResolveStage4_1GroupName()
            ? ResolveStage4_2GroupName()
            : ResolveStage4_1GroupName();
        Transform preferredMoranTransform = ResolveMoranInGroupOnly(preferredGroupName);

        if (IsRenderableActor(preferredMoranTransform))
            return preferredMoranTransform;

        OOTechSceneObject[] sceneObjectArray = FindObjectsByType<OOTechSceneObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (OOTechSceneObject sceneObject in sceneObjectArray)
        {
            if (sceneObject == null || sceneObject.RoleId != ResolveMoranRoleId())
                continue;

            if (sceneObject.transform == Transform_Moran || sceneObject.transform.IsChildOf(transform) || IsBlockedMoranSource(sceneObject.transform))
                continue;

            if (IsRenderableActor(sceneObject.transform))
                return sceneObject.transform;
        }

        OOTechStageMoranFreeMoveController[] moranControllerArray = FindObjectsByType<OOTechStageMoranFreeMoveController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (OOTechStageMoranFreeMoveController moranController in moranControllerArray)
        {
            if (moranController == null || moranController.transform == Transform_Moran || moranController.transform.IsChildOf(transform) || IsBlockedMoranSource(moranController.transform))
                continue;

            if (IsRenderableActor(moranController.transform))
                return moranController.transform;
        }

        return null;
    }

    private Transform ResolveMoranInGroupOnly(string groupName)
    {
        GameObject groupObject = RequestSceneObjectByName(groupName);

        if (groupObject == null)
            return null;

        Transform moranTransform = RequestChildObjectByName(groupObject.transform, "Moran");

        if (IsRenderableActor(moranTransform))
            return moranTransform;

        OOTechSceneObject[] sceneObjectArray = groupObject.GetComponentsInChildren<OOTechSceneObject>(true);

        foreach (OOTechSceneObject sceneObject in sceneObjectArray)
        {
            if (sceneObject != null && sceneObject.RoleId == ResolveMoranRoleId() && IsRenderableActor(sceneObject.transform))
                return sceneObject.transform;
        }

        return null;
    }

    private bool IsBlockedMoranSource(Transform sourceTransform)
    {
        Transform currentTransform = sourceTransform;

        while (currentTransform != null)
        {
            string objectName = currentTransform.name;

            if (objectName == "FinalStageGroup"
                || objectName == "Final_Road_to_FinalStage"
                || objectName == "PreFinal_Narration"
                || objectName == "EpilogueGroup"
                || objectName == "EndingCreditGroup")
                return true;

            currentTransform = currentTransform.parent;
        }

        return false;
    }

    private bool IsRenderableActor(Transform actorTransform)
    {
        if (actorTransform == null)
            return false;

        SpriteRenderer spriteRenderer = actorTransform.GetComponentInChildren<SpriteRenderer>(true);
        return spriteRenderer != null && spriteRenderer.sprite != null;
    }

    private void RequestKeepMoranInPresentationStartPositionIfNeeded()
    {
        if (gameObject.name != ResolveStage4_2GroupName() || Transform_Moran == null || Renderer_Background == null)
            return;

        Bounds backgroundBounds = Renderer_Background.bounds;
        Vector3 moranPosition = Transform_Moran.position;
        bool isOutsideBackground =
            moranPosition.x < backgroundBounds.min.x ||
            moranPosition.x > backgroundBounds.max.x ||
            moranPosition.y < backgroundBounds.min.y ||
            moranPosition.y > backgroundBounds.max.y;

        if (isOutsideBackground || !IsMoranVisibleInMainCamera())
            PlaceMoranAtStage4_2VisibleStart();
    }

    private void PrepareMoranControl(bool isEnabled)
    {
        if ((Controller_MoranMove == null || Controller_MoranMove.transform != Transform_Moran) && Transform_Moran != null)
            Controller_MoranMove = Transform_Moran.GetComponent<OOTechStageMoranFreeMoveController>();

        if (Controller_MoranMove != null)
            Controller_MoranMove.enabled = isEnabled;
    }

    public void RequestResolveStage4_2CookingReturn(bool isKeepCurrentPositionPreferred)
    {
        if (gameObject.name != ResolveStage4_2GroupName())
            return;

        ResolveComponents();

        if (!_isStopPointATriggered)
        {
            _isStopPointATriggered = true;
            RequestSetMission("토끼를 유혹하기 위한 당근전을 만드세요.", true);
        }

        RequestSetActive(Transform_StopPointA, false);

        if (!isKeepCurrentPositionPreferred)
            PlaceMoranAtStage4_2CookingReturnFallback();

        RequestForceActorVisible(Transform_Moran, 2200);
        _isStage4_2LeftExitArmed = false;
        PrepareMoranControl(true);
    }

    public void RequestPlayRabbitCarrotCakeReadyIfAvailable()
    {
        if (gameObject.name != ResolveStage4_2GroupName())
            return;

        ResolveCueSheetData();
        ResolveComponents();

        if (_isRabbitCarrotCakeDialoguePlayed || !HasItem(ResolveCarrotCakeItemId()))
            return;

        StartManagedRoutine(PlayRabbitCarrotCakeReadyRoutine());
    }

    private void PlaceMoranAtStage4_2CookingReturnFallback()
    {
        if (Transform_Moran == null)
            return;

        if (_hasStage4_2SavedMoranPosition)
        {
            Transform_Moran.position = _stage4_2SavedMoranPosition;
            return;
        }

        if (Transform_StopPointA == null)
            return;

        Vector3 position = Transform_Moran.position;
        position.x = Transform_StopPointA.position.x;
        position.y = Transform_StopPointA.position.y;
        Transform_Moran.position = position;
    }

    private void PlaceMoranAtLeftEdge()
    {
        if (Transform_Moran == null || Renderer_Background == null)
            return;

        Bounds bounds = Renderer_Background.bounds;
        Vector3 position = Transform_Moran.position;
        position.x = bounds.min.x + ResolveStageEdgeMargin(bounds);
        position.y = Mathf.Lerp(bounds.min.y, bounds.max.y, StageLaneNormalizedHeight);
        Transform_Moran.position = position;
    }

    private void PlaceMoranAtStage4_2VisibleStart()
    {
        if (Transform_Moran == null || Renderer_Background == null)
            return;

        RequestFitCameraToBackground();

        Bounds bounds = Renderer_Background.bounds;
        Vector3 position = Transform_Moran.position;
        position.x = bounds.min.x + ResolveStageEdgeMargin(bounds);
        position.y = Mathf.Lerp(bounds.min.y, bounds.max.y, StageLaneNormalizedHeight);

        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            float cameraDepth = Mathf.Abs(mainCamera.transform.position.z - position.z);
            Vector3 cameraLeftPosition = mainCamera.ViewportToWorldPoint(new Vector3(Stage4_2VisibleStartViewportX, 0.5f, cameraDepth));
            position.x = cameraLeftPosition.x;
        }

        float edgeMargin = ResolveStageEdgeMargin(bounds);
        position.x = Mathf.Clamp(position.x, bounds.min.x + edgeMargin, bounds.max.x - edgeMargin);
        Transform_Moran.position = position;
    }

    private bool IsMoranVisibleInMainCamera()
    {
        if (Transform_Moran == null)
            return false;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return true;

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(Transform_Moran.position);
        return viewportPosition.z > 0f
            && viewportPosition.x >= 0.04f
            && viewportPosition.x <= 0.96f
            && viewportPosition.y >= 0.04f
            && viewportPosition.y <= 0.96f;
    }

    private void RequestFitCameraToBackground()
    {
        if (Renderer_Background == null)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        CameraFollowController cameraFollow = mainCamera.GetComponent<CameraFollowController>();

        if (cameraFollow != null)
            cameraFollow.enabled = false;

        Bounds bounds = Renderer_Background.bounds;
        float verticalSize = bounds.extents.y;
        float horizontalSize = bounds.extents.x / Mathf.Max(0.01f, mainCamera.aspect);

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);

        Vector3 cameraPosition = mainCamera.transform.position;
        cameraPosition.x = bounds.center.x;
        cameraPosition.y = bounds.center.y;
        mainCamera.transform.position = cameraPosition;
    }

    private void PlaceMoranAtRightEdge()
    {
        if (Transform_Moran == null || Renderer_Background == null)
            return;

        Bounds bounds = Renderer_Background.bounds;
        Vector3 position = Transform_Moran.position;
        position.x = bounds.max.x - ResolveStageEdgeMargin(bounds);
        position.y = Mathf.Lerp(bounds.min.y, bounds.max.y, StageLaneNormalizedHeight);
        Transform_Moran.position = position;
    }

    private bool IsMoranAtRightEdge()
    {
        if (Transform_Moran == null)
            return false;

        if (Renderer_Background != null)
        {
            Bounds bounds = Renderer_Background.bounds;
            return Transform_Moran.position.x >= bounds.max.x - ResolveStageEdgeMargin(bounds) - EdgeReachTolerance;
        }

        return Transform_Moran.position.x >= 8.8f;
    }

    private bool IsMoranAtLeftEdge()
    {
        if (Transform_Moran == null)
            return false;

        if (Renderer_Background != null)
        {
            Bounds bounds = Renderer_Background.bounds;
            return Transform_Moran.position.x <= bounds.min.x + ResolveStageEdgeMargin(bounds) + EdgeReachTolerance;
        }

        return Transform_Moran.position.x <= -8.8f;
    }

    private bool HasMoranMovedAwayFromRightEdge()
    {
        if (Transform_Moran == null || Renderer_Background == null)
            return true;

        Bounds bounds = Renderer_Background.bounds;
        return Transform_Moran.position.x < bounds.max.x - ResolveStageEdgeMargin(bounds) - EdgeRearmDistance;
    }

    private bool HasMoranMovedAwayFromLeftEdge()
    {
        if (Transform_Moran == null || Renderer_Background == null)
            return true;

        Bounds bounds = Renderer_Background.bounds;
        return Transform_Moran.position.x > bounds.min.x + ResolveStageEdgeMargin(bounds) + EdgeRearmDistance;
    }

    private float ResolveStageEdgeMargin(Bounds backgroundBounds)
    {
        float edgeMargin = backgroundBounds.size.x >= LargeStageBackgroundWidth ? LargeStageEdgeMargin : SmallStageEdgeMargin;
        SpriteRenderer moranRenderer = Transform_Moran != null ? Transform_Moran.GetComponentInChildren<SpriteRenderer>(true) : null;

        if (moranRenderer != null)
            edgeMargin = Mathf.Max(edgeMargin, moranRenderer.bounds.extents.x * 0.35f);

        return Mathf.Min(edgeMargin, Mathf.Max(0.01f, backgroundBounds.extents.x - 0.01f));
    }

    private void ClampMoranBeforeStump()
    {
        if (Transform_Moran == null || Transform_Stump == null)
            return;

        Vector3 position = Transform_Moran.position;
        position.x = Mathf.Min(position.x, Transform_Stump.position.x - 12f);
        Transform_Moran.position = position;
    }

    private bool IsNear(Transform firstTransform, Transform secondTransform)
    {
        if (firstTransform == null || secondTransform == null)
            return false;

        float distance = Data_CueSheet != null && Data_CueSheet.InteractionDistance > 0f ? Data_CueSheet.InteractionDistance : 95f;

        if (Vector2.Distance(firstTransform.position, secondTransform.position) <= distance)
            return true;

        float boundsGap = CalculateRendererBoundsGap(firstTransform, secondTransform);
        return boundsGap <= distance;
    }

    private bool IsInteractionPressed()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.E))
            return true;
#endif

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.eKey.wasPressedThisFrame;
#else
        return false;
#endif
    }

    private float CalculateRendererBoundsGap(Transform firstTransform, Transform secondTransform)
    {
        SpriteRenderer firstRenderer = firstTransform != null ? firstTransform.GetComponentInChildren<SpriteRenderer>(true) : null;
        SpriteRenderer secondRenderer = secondTransform != null ? secondTransform.GetComponentInChildren<SpriteRenderer>(true) : null;

        if (firstRenderer == null || secondRenderer == null)
            return float.PositiveInfinity;

        Bounds firstBounds = firstRenderer.bounds;
        Bounds secondBounds = secondRenderer.bounds;
        float xGap = Mathf.Max(0f, Mathf.Max(secondBounds.min.x - firstBounds.max.x, firstBounds.min.x - secondBounds.max.x));
        float yGap = Mathf.Max(0f, Mathf.Max(secondBounds.min.y - firstBounds.max.y, firstBounds.min.y - secondBounds.max.y));

        return new Vector2(xGap, yGap).magnitude;
    }

    private void RequestPlayActorState(Transform actorTransform, string stateName, float speed, bool isForce)
    {
        if (actorTransform == null || string.IsNullOrEmpty(stateName))
            return;

        Animator animator = actorTransform.GetComponentInChildren<Animator>(true);
        SpriteRenderer spriteRenderer = actorTransform.GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.forceRenderingOff = false;
        }

        if (animator == null)
            return;

        animator.enabled = true;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.speed = Mathf.Max(0.01f, speed);

        int stateHash = Animator.StringToHash(stateName);

        if (animator.HasState(0, stateHash))
            animator.Play(stateHash, 0, isForce ? 0f : animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
        else
            Debug.LogWarning($"[OOTechStage4GroupController] Animator state missing: {actorTransform.name}/{stateName}");
    }

    private OOTechSpeechBubbleView ResolveSpeechBubbleView(Transform actorTransform)
    {
        if (actorTransform == null)
            return null;

        OOTechSpeechBubbleView view = actorTransform.GetComponentInChildren<OOTechSpeechBubbleView>(true);

        if (view != null)
            return view;

        Transform speechBubbleTransform = RequestChildObjectByName(actorTransform, "SpeechBubble");

        if (speechBubbleTransform == null)
            return null;

        return speechBubbleTransform.gameObject.AddComponent<OOTechSpeechBubbleView>();
    }

    private void RequestHideSpeechBubble(Transform actorTransform)
    {
        OOTechSpeechBubbleView speechBubbleView = ResolveSpeechBubbleView(actorTransform);
        speechBubbleView?.RequestPrepareHidden();
    }

    private void RequestPlayStage4BGM()
    {
        AudioClip clip = OOTechAudioClipResolver.Resolve(Clip_Stage4BGM, "Audio/BGM/Stage4_BGM");

        if (clip != null && OOTechSoundManager.Inst != null)
            OOTechSoundManager.Inst.PlayBGM(clip, true);
    }

    private void ShowStage4ClearCanvas()
    {
        _isStage4ClearTransitionRequested = false;
        HUD_Road?.RequestForceHideForCutScene();

        if (TryShowStage4CompleteTutorialGuide())
        {
            StartStage4ClearFailSafe();
            return;
        }

        if (View_ClearPanel == null)
            View_ClearPanel = GetComponentInChildren<OOTechStage4ClearPanelView>(true);

        OO_Tutorial tutorialData = OOTechGameDataManager.Inst != null
            ? OOTechGameDataManager.Inst.GetTutorialData(ResolveNextTutorialNarrationId())
            : null;
        string bodyText = tutorialData != null && !string.IsNullOrEmpty(tutorialData.Description)
            ? tutorialData.Description
            : "모든 스테이지 임무를 완수했습니다. 마지막 이야기로 이동합니다.";

        if (View_ClearPanel != null)
        {
            View_ClearPanel.RequestShow("모든 스테이지 임무 완료!", bodyText, delegate
            {
                RequestSwitchToPreFinalFromStage4Clear();
            });
            RequestForceCanvasVisible(View_ClearPanel.gameObject, 6500);
            StartStage4ClearFailSafe();
            return;
        }

        Debug.LogWarning("[OOTechStage4GroupController] Canvas_Stage4Clear is missing. Switching to PreFinal as a fail-safe.");
        RequestSwitchToPreFinalFromStage4Clear();
    }

    private bool TryShowStage4CompleteTutorialGuide()
    {
        GameObject tutorialGuideGroup = RequestSceneObjectByName("TutorialGuideGroup");

        if (tutorialGuideGroup == null)
            return false;

        OOTechTutorialGuideUI tutorialGuideUI = tutorialGuideGroup.GetComponentInChildren<OOTechTutorialGuideUI>(true);

        if (tutorialGuideUI == null)
            return false;

        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.RegisterUI("TutorialGuideGroup", tutorialGuideGroup);

        tutorialGuideGroup.SetActive(true);
        tutorialGuideUI.gameObject.SetActive(true);
        RequestForceCanvasVisible(tutorialGuideGroup, 6400);

        OO_Tutorial tutorialData = OOTechGameDataManager.Inst != null
            ? OOTechGameDataManager.Inst.GetTutorialData(ResolveNextTutorialNarrationId())
            : null;

        if (tutorialData != null)
        {
            OO_Tutorial stage4CompleteTutorialData = new OO_Tutorial
            {
                Id = tutorialData.Id,
                Name = tutorialData.Name,
                Title = "모든 스테이지 임무 완료!",
                Description = string.IsNullOrWhiteSpace(tutorialData.Description)
                    ? "모든 스테이지 임무를 완수했습니다. 마지막 이야기로 이동합니다."
                    : tutorialData.Description,
                TargetStageId = tutorialData.TargetStageId,
                TriggerCondition = tutorialData.TriggerCondition,
                DialogueGroupId = tutorialData.DialogueGroupId,
                SkillList = tutorialData.SkillList,
                UseWeaponId = tutorialData.UseWeaponId,
                BasicCostumeId = tutorialData.BasicCostumeId
            };

            tutorialGuideUI.ShowGuide(stage4CompleteTutorialData, delegate
            {
                tutorialGuideUI.SetTitleEmphasisActive(false);
                tutorialGuideUI.CloseGuide();
                RequestSwitchToPreFinalFromStage4Clear();
            });
        }
        else
        {
            OO_Narration fallbackNarration = new OO_Narration
            {
                Id = ResolveNextTutorialNarrationId(),
                Title = "모든 스테이지 임무 완료!",
                NarrationTexts = new List<string> { "모든 스테이지 임무를 완수했습니다. 마지막 이야기로 이동합니다." }
            };

            tutorialGuideUI.ShowGuide(fallbackNarration, delegate
            {
                tutorialGuideUI.SetTitleEmphasisActive(false);
                tutorialGuideUI.CloseGuide();
                RequestSwitchToPreFinalFromStage4Clear();
            });
        }

        tutorialGuideUI.SetTitleEmphasisActive(true);
        return true;
    }

    private void StartStage4ClearFailSafe()
    {
        if (Coroutine_ClearFailSafe != null)
            StopCoroutine(Coroutine_ClearFailSafe);

        Coroutine_ClearFailSafe = StartCoroutine(Stage4ClearFailSafeRoutine());
    }

    private IEnumerator Stage4ClearFailSafeRoutine()
    {
        yield return new WaitForSecondsRealtime(10f);

        if (_isStage4ClearTransitionRequested)
            yield break;

        Debug.LogWarning("[OOTechStage4GroupController] Stage4 clear panel did not advance. Moving to PreFinal by fail-safe.");
        RequestSwitchToPreFinalFromStage4Clear();
    }

    private void RequestSwitchToPreFinalFromStage4Clear()
    {
        if (_isStage4ClearTransitionRequested)
            return;

        _isStage4ClearTransitionRequested = true;

        if (Coroutine_ClearFailSafe != null)
        {
            StopCoroutine(Coroutine_ClearFailSafe);
            Coroutine_ClearFailSafe = null;
        }

        RequestSwitchGroup(gameObject.name, ResolvePreFinalGroupName());
    }

    private Transform ResolveRoleTransform(string roleId, string fallbackName)
    {
        if (Context_Scene != null)
        {
            Transform roleTransform = Context_Scene.GetRoleTransform(roleId);

            if (roleTransform != null)
                return roleTransform;
        }

        Transform foundTransform = RequestChildObjectByName(transform, roleId);

        if (foundTransform != null)
            return foundTransform;

        return RequestChildObjectByName(transform, fallbackName);
    }

    private SpriteRenderer ResolveBackgroundRenderer()
    {
        SpriteRenderer[] rendererArray = GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer bestRenderer = null;
        SpriteRenderer largestRenderer = null;
        float bestArea = 0f;
        float largestArea = 0f;

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
                continue;

            string objectName = spriteRenderer.gameObject.name;

            float area = Mathf.Abs(spriteRenderer.bounds.size.x * spriteRenderer.bounds.size.y);

            if (area > largestArea)
            {
                largestRenderer = spriteRenderer;
                largestArea = area;
            }

            if (!objectName.Contains("Background") && !objectName.Contains("Backound"))
                continue;

            if (area <= bestArea)
                continue;

            bestRenderer = spriteRenderer;
            bestArea = area;
        }

        return bestRenderer != null ? bestRenderer : largestRenderer;
    }

    private void RequestSwitchGroup(string closingGroupName, string openingGroupName)
    {
        if (string.IsNullOrEmpty(openingGroupName))
            return;

        GameObject closingObject = RequestSceneObjectByName(closingGroupName);
        GameObject openingObject = RequestSceneObjectByName(openingGroupName);
        PrepareOpeningGroupController(openingObject, openingGroupName);

        if (OOTechUIManager.Inst != null)
        {
            if (closingObject != null)
                OOTechUIManager.Inst.RegisterUI(closingGroupName, closingObject);

            if (openingObject != null)
                OOTechUIManager.Inst.RegisterUI(openingGroupName, openingObject);

            OOTechUIManager.Inst.CloseUI(closingGroupName);

            if (OOTechUIManager.Inst.OpenUI(openingGroupName))
                return;
        }

        if (openingObject != null)
            openingObject.SetActive(true);

        if (closingObject != null)
            closingObject.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    private void PrepareOpeningGroupController(GameObject openingObject, string openingGroupName)
    {
        if (openingObject == null || openingGroupName != ResolvePreFinalGroupName())
            return;

        if (openingObject.GetComponent<OOTechCutSceneNarrationController>() == null)
            openingObject.AddComponent<OOTechCutSceneNarrationController>();
    }

    private void RequestSetActive(Transform targetTransform, bool isActive)
    {
        if (targetTransform != null)
            targetTransform.gameObject.SetActive(isActive);
    }

    private void RequestForceActorVisible(Transform actorTransform, int minimumSortingOrder)
    {
        if (actorTransform == null)
            return;

        actorTransform.gameObject.SetActive(true);
        RequestActivateParentChain(actorTransform);

        Vector3 localScale = actorTransform.localScale;

        if (Mathf.Abs(localScale.x) < 0.0001f)
            localScale.x = 1f;

        if (Mathf.Abs(localScale.y) < 0.0001f)
            localScale.y = 1f;

        if (Mathf.Abs(localScale.z) < 0.0001f)
            localScale.z = 1f;

        actorTransform.localScale = localScale;

        SpriteRenderer[] rendererArray = actorTransform.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null)
                continue;

            spriteRenderer.enabled = true;
            spriteRenderer.forceRenderingOff = false;
            spriteRenderer.sortingLayerName = "Characters";

            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;

            if (minimumSortingOrder > 0 && spriteRenderer.sortingOrder < minimumSortingOrder)
                spriteRenderer.sortingOrder = minimumSortingOrder;
        }

        Animator animator = actorTransform.GetComponentInChildren<Animator>(true);

        if (animator != null)
        {
            animator.enabled = true;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }
    }

    private void RequestRaiseStump2AboveStump()
    {
        if (Transform_Stump2 == null)
            return;

        int baseSortingOrder = 2400;
        SpriteRenderer stumpRenderer = Transform_Stump != null ? Transform_Stump.GetComponentInChildren<SpriteRenderer>(true) : null;

        if (stumpRenderer != null)
            baseSortingOrder = Mathf.Max(baseSortingOrder, stumpRenderer.sortingOrder + 5);

        SpriteRenderer[] stump2RendererArray = Transform_Stump2.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer spriteRenderer in stump2RendererArray)
        {
            if (spriteRenderer == null)
                continue;

            spriteRenderer.enabled = true;
            spriteRenderer.forceRenderingOff = false;
            spriteRenderer.sortingLayerName = "Characters";
            spriteRenderer.sortingOrder = Mathf.Max(spriteRenderer.sortingOrder, baseSortingOrder);

            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }

    private void RequestForceCanvasVisible(GameObject rootObject, int sortingOrder)
    {
        if (rootObject == null)
            return;

        Canvas[] canvasArray = rootObject.GetComponentsInChildren<Canvas>(true);

        foreach (Canvas canvas in canvasArray)
        {
            if (canvas == null)
                continue;

            canvas.gameObject.SetActive(true);
            canvas.overrideSorting = true;
            canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, sortingOrder);
        }
    }

    private void RequestActivateParentChain(Transform targetTransform)
    {
        Transform currentTransform = targetTransform;

        while (currentTransform != null)
        {
            if (!currentTransform.gameObject.activeSelf)
                currentTransform.gameObject.SetActive(true);

            if (currentTransform == transform)
                break;

            currentTransform = currentTransform.parent;
        }
    }

    private bool HasItem(string itemId)
    {
        return OOTechGameManager.Inst != null && OOTechGameManager.Inst.GetItemCount(itemId) > 0;
    }

    private bool TryRemoveItem(string itemId, int count)
    {
        return OOTechGameManager.Inst != null && OOTechGameManager.Inst.RemoveItem(itemId, count);
    }

    private void UpdateFlipByDelta(Transform actorTransform, Vector3 delta)
    {
        if (actorTransform == null || Mathf.Abs(delta.x) <= 0.001f)
            return;

        SpriteRenderer spriteRenderer = actorTransform.GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
            spriteRenderer.flipX = delta.x < 0f;
    }

    private List<string> ResolveTextList(string rawText)
    {
        List<string> resultList = new List<string>();

        if (string.IsNullOrWhiteSpace(rawText))
            return resultList;

        string[] splitArray = rawText.Split('|');

        foreach (string value in splitArray)
        {
            if (!string.IsNullOrWhiteSpace(value))
                resultList.Add(value.Trim());
        }

        return resultList;
    }

    private string ResolveStage4CarrotCakeMissionText()
    {
        OO_StageQuest questData = OOTechGameDataManager.Inst != null
            ? OOTechGameDataManager.Inst.GetStageQuestData(ResolveStageQuestId())
            : null;

        if (questData != null && IsReadableStage4CarrotCakeText(questData.Description))
            return questData.Description;

        return "토끼를 유혹하기 위한 당근전을 만드세요.";
    }

    private string ResolveStage4CarrotCakeGuideText()
    {
        string starchGuide = ResolveRecipeGuideByResultItemId(
            ResolveCarrotStarchItemId(),
            "레시피 조합 버튼을 누른 뒤 떡 1개와 당근 1개를 조합 칸에 올려 당근전분을 만드세요.");
        string carrotCakeGuide = ResolveRecipeGuideByResultItemId(
            ResolveCarrotCakeItemId(),
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

            if (IsReadableStage4CarrotCakeText(recipeData.QuantityGuideText))
                return recipeData.QuantityGuideText;

            if (IsReadableStage4CarrotCakeText(recipeData.Description))
                return recipeData.Description;
        }

        return fallbackText;
    }

    private bool IsReadableStage4CarrotCakeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return value.Contains("당근") || value.Contains("떡") || value.Contains("가마솥") || value.Contains("레시피");
    }

    private string ResolveStage4_1GroupName() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.Stage4_1GroupName) ? Data_CueSheet.Stage4_1GroupName : Stage4_1Name;
    private string ResolveStage4_2GroupName() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.Stage4_2GroupName) ? Data_CueSheet.Stage4_2GroupName : Stage4_2Name;
    private string ResolvePreFinalGroupName() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.PreFinalGroupName) ? Data_CueSheet.PreFinalGroupName : "PreFinal_Narration";
    private string ResolveMoranRoleId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.MoranRoleId) ? Data_CueSheet.MoranRoleId : "Moran";
    private string ResolveTurtleRoleId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.TurtleRoleId) ? Data_CueSheet.TurtleRoleId : "Turtle";
    private string ResolveRabbitRoleId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.RabbitRoleId) ? Data_CueSheet.RabbitRoleId : "Rabbit";
    private string ResolveSleepingRabbitRoleId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.SleepingRabbitRoleId) ? Data_CueSheet.SleepingRabbitRoleId : "Rabbit_isSleeping";
    private string ResolveStumpRoleId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.StumpRoleId) ? Data_CueSheet.StumpRoleId : "Stump";
    private string ResolveStump2RoleId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.Stump2RoleId) ? Data_CueSheet.Stump2RoleId : "Stump2";
    private string ResolveStopPointARoleId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.StopPointARoleId) ? Data_CueSheet.StopPointARoleId : "StopPoint_A";
    private string ResolveTurtleDialogueIdList() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.TurtleDialogueIdList) ? Data_CueSheet.TurtleDialogueIdList : "character_Turtle_01|character_Turtle_02";
    private string ResolveTurtleClearDialogueIdList() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.TurtleClearDialogueIdList) ? Data_CueSheet.TurtleClearDialogueIdList : "character_Turtle_03|character_Turtle_04";
    private string ResolveRabbitIntroDialogueId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.RabbitIntroDialogueId) ? Data_CueSheet.RabbitIntroDialogueId : "character_Rabbit_01";
    private string ResolveRabbitStopDialogueId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.RabbitStopDialogueId) ? Data_CueSheet.RabbitStopDialogueId : "character_Rabbit_02";
    private string ResolveRabbitCarrotCakeDialogueIdList() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.RabbitCarrotCakeDialogueIdList) ? Data_CueSheet.RabbitCarrotCakeDialogueIdList : "character_Rabbit_03|character_Rabbit_04";
    private string ResolveRabbitSpeechBubbleIdList() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.RabbitSpeechBubbleIdList) ? Data_CueSheet.RabbitSpeechBubbleIdList : "character_Rabbit_01|character_Rabbit_02|character_Rabbit_03|character_Rabbit_04|character_Rabbit_05";
    private string ResolveRabbitSleepingBubbleId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.RabbitSleepingBubbleId) ? Data_CueSheet.RabbitSleepingBubbleId : "character_Rabbit_06";
    private string ResolveCarrotCakeItemId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.CarrotCakeItemId) ? Data_CueSheet.CarrotCakeItemId : "OO_CarrotCake_1";
    private string ResolveCarrotStarchItemId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.CarrotStarchItemId) ? Data_CueSheet.CarrotStarchItemId : "OO_CarrotStarch_1";
    private string ResolveStageQuestId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.StageQuestId) ? Data_CueSheet.StageQuestId : "Stage4__Quest_01";
    private string ResolveNextTutorialNarrationId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.NextTutorialNarrationId) ? Data_CueSheet.NextTutorialNarrationId : "narration_tutorial_15";

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

    private GameObject RequestSceneObjectByName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

        foreach (GameObject rootObject in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform foundTransform = RequestChildObjectByName(rootObject.transform, objectName);

            if (foundTransform != null)
                return foundTransform.gameObject;
        }

        return null;
    }
}
