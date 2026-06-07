// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechStage4GroupController.cs
// - ??븷: Stage4_1Group怨?Stage4_2Group??嫄곕턿???좊겮 ?먯떆?몃? 吏?섑빀?덈떎.
// - ?곹솕 鍮꾩쑀: ???ㅽ겕由쏀듃??臾대?媛먮룆?낅땲?? 嫄곕턿?? ?좊겮, 留먰뭾?? HUD瑜?吏곸젒 留뚮뱾吏 ?딄퀬
//   "吏湲????, "吏湲??대룞", "吏湲??꾨Т 媛깆떊" 媛숈? ?먮쭔 ?꾨떖?⑸땲??
// - ?좎?蹂댁닔 ?ъ씤?? ???留먰뭾???꾩씠??ID??OO_Stage4CueSheet? 湲곗〈 JSON?먯꽌 ?쎄퀬,
//   諛곗슦 ?ㅻ툕?앺듃??OOTechSceneObject ??븷?쒕줈 李얠뒿?덈떎.
// =============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class OOTechStage4GroupController : MonoBehaviour
{
    private const string DefaultCueSheetId = "Stage4_CueSheet_01";
    private const string Stage4_1Name = "Stage4_1Group";
    private const string Stage4_2Name = "Stage4_2Group";

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
    private bool _isRunningSequence;

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

        _isRunningSequence = false;
    }

    /// <summary>
    /// Stage4??E ?곹샇?묒슜怨??붾㈃ ???꾪솚??留??꾨젅???뺤씤?⑸땲??
    /// Controller???먯젙留??섍퀬, ?ㅼ젣 ?좊땲硫붿씠????щ뒗 媛???븷 而댄룷?뚰듃??留↔퉩?덈떎.
    /// </summary>
    private void Update()
    {
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
        Transform_Moran = ResolveRoleTransform(ResolveMoranRoleId(), "Moran");
        Transform_Turtle = ResolveRoleTransform(ResolveTurtleRoleId(), "Turtle");
        Transform_Rabbit = ResolveRoleTransform(ResolveRabbitRoleId(), "Rabbit");
        Transform_RabbitSleeping = ResolveRoleTransform(ResolveSleepingRabbitRoleId(), "Rabbit_isSleeping");
        Transform_Stump = ResolveRoleTransform(ResolveStumpRoleId(), "Stump");
        Transform_Stump2 = ResolveRoleTransform(ResolveStump2RoleId(), "Stump2");
        Transform_StopPointA = ResolveRoleTransform(ResolveStopPointARoleId(), "StopPoint_A");
        Renderer_Background = ResolveBackgroundRenderer();

        if (Transform_Moran != null)
            Controller_MoranMove = Transform_Moran.GetComponent<OOTechStageMoranFreeMoveController>();

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
            stageViewController.enabled = true;

        if (HUD_Road != null)
        {
            HUD_Road.SetOwnerGroupName(gameObject.name);
            HUD_Road.PrepareHUD();
            HUD_Road.SetCookingUnlocked(true, false);
            HUD_Road.SetHUDVisible(true);
        }
    }

    private void PrepareStage4_1()
    {
        RequestSetActive(Transform_Turtle, true);
        RequestSetActive(Transform_Rabbit, false);
        RequestSetActive(Transform_RabbitSleeping, false);

        if (_isRabbitSleeping)
            RequestSetMission("토끼가 잠이 들었습니다. 거북이에게 돌아가세요!", true);
        else if (_isTurtleQuestAccepted)
            RequestSetMission("토끼를 찾으세요.", false);

        if (_isReturnFromStage4_2ToStage4_1)
        {
            PlaceMoranAtRightEdge();
            _isReturnFromStage4_2ToStage4_1 = false;
        }
        else if (Transform_Moran != null && !_isTurtleQuestAccepted)
        {
            PlaceMoranAtLeftEdge();
        }
    }

    private void PrepareStage4_2()
    {
        RequestSetActive(Transform_Turtle, false);
        RequestSetActive(Transform_Rabbit, !_isRabbitSleeping);
        RequestSetActive(Transform_RabbitSleeping, _isRabbitSleeping);
        RequestSetActive(Transform_Stump2, false);

        if (_hasStage4_2SavedMoranPosition && Transform_Moran != null)
            Transform_Moran.position = _stage4_2SavedMoranPosition;
        else
            PlaceMoranAtLeftEdge();

        if (_isStopPointATriggered && Transform_StopPointA != null)
            Transform_StopPointA.gameObject.SetActive(false);

        if (!_isRabbitSleeping)
            StartManagedRoutine(PlayStage4_2IntroRoutine());
        else
            RequestSetMission("토끼가 잠이 들었습니다. 거북이에게 돌아가세요!", true);
    }

    private void UpdateStage4_1Input()
    {
        if (Transform_Moran == null)
            return;

        if (_isRabbitSleeping && !_isStage4Cleared && Transform_Turtle != null && IsNear(Transform_Moran, Transform_Turtle))
        {
            if (Input.GetKeyDown(KeyCode.E))
                StartManagedRoutine(PlayTurtleClearRoutine());

            return;
        }

        if (!_isTurtleQuestAccepted && Transform_Turtle != null && IsNear(Transform_Moran, Transform_Turtle) && Input.GetKeyDown(KeyCode.E))
        {
            StartManagedRoutine(PlayTurtleQuestRoutine());
            return;
        }

        if (_isTurtleQuestAccepted && IsMoranAtRightEdge())
            RequestSwitchGroup(gameObject.name, ResolveStage4_2GroupName());
    }

    private void UpdateStage4_2Input()
    {
        if (Transform_Moran == null)
            return;

        if (IsMoranAtLeftEdge())
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

        if (_isRabbitCarrotCakeDialoguePlayed && !_isRabbitSleeping && Transform_Stump != null && IsNear(Transform_Moran, Transform_Stump) && Input.GetKeyDown(KeyCode.E))
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

        yield return Cue_Dialogue.RequestShowDialogueAndWait(ResolveRabbitIntroDialogueId());
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

    private void PrepareMoranControl(bool isEnabled)
    {
        if (Controller_MoranMove == null && Transform_Moran != null)
            Controller_MoranMove = Transform_Moran.GetComponent<OOTechStageMoranFreeMoveController>();

        if (Controller_MoranMove != null)
            Controller_MoranMove.enabled = isEnabled;
    }

    private void PlaceMoranAtLeftEdge()
    {
        if (Transform_Moran == null || Renderer_Background == null)
            return;

        Bounds bounds = Renderer_Background.bounds;
        Vector3 position = Transform_Moran.position;
        position.x = bounds.min.x + 120f;
        position.y = Mathf.Lerp(bounds.min.y, bounds.max.y, 0.2f);
        Transform_Moran.position = position;
    }

    private void PlaceMoranAtRightEdge()
    {
        if (Transform_Moran == null || Renderer_Background == null)
            return;

        Bounds bounds = Renderer_Background.bounds;
        Vector3 position = Transform_Moran.position;
        position.x = bounds.max.x - 120f;
        position.y = Mathf.Lerp(bounds.min.y, bounds.max.y, 0.2f);
        Transform_Moran.position = position;
    }

    private bool IsMoranAtRightEdge()
    {
        if (Transform_Moran == null || Renderer_Background == null)
            return false;

        return Transform_Moran.position.x >= Renderer_Background.bounds.max.x - 18f;
    }

    private bool IsMoranAtLeftEdge()
    {
        if (Transform_Moran == null || Renderer_Background == null)
            return false;

        return Transform_Moran.position.x <= Renderer_Background.bounds.min.x + 18f;
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
        return Vector2.Distance(firstTransform.position, secondTransform.position) <= distance;
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

    private void RequestPlayStage4BGM()
    {
        AudioClip clip = Clip_Stage4BGM;

        if (clip == null)
            clip = Resources.Load<AudioClip>("Audio/BGM/Stage4_BGM");

        if (clip != null && OOTechSoundManager.Inst != null)
            OOTechSoundManager.Inst.PlayBGM(clip, true);
    }

    private void ShowStage4ClearCanvas()
    {
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
            View_ClearPanel.RequestShow("모든 임무 완수!", bodyText, delegate
            {
                RequestSwitchGroup(gameObject.name, ResolvePreFinalGroupName());
            });
            return;
        }

        Debug.LogWarning("[OOTechStage4GroupController] Canvas_Stage4Clear is missing. Switching to PreFinal as a fail-safe.");
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
        float bestArea = 0f;

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
                continue;

            string objectName = spriteRenderer.gameObject.name;

            if (!objectName.Contains("Background") && !objectName.Contains("Backound"))
                continue;

            float area = Mathf.Abs(spriteRenderer.bounds.size.x * spriteRenderer.bounds.size.y);

            if (area <= bestArea)
                continue;

            bestRenderer = spriteRenderer;
            bestArea = area;
        }

        return bestRenderer;
    }

    private void RequestSwitchGroup(string closingGroupName, string openingGroupName)
    {
        if (string.IsNullOrEmpty(openingGroupName))
            return;

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.CloseUI(closingGroupName);
            OOTechUIManager.Inst.OpenUI(openingGroupName);
            return;
        }

        GameObject openingObject = RequestSceneObjectByName(openingGroupName);

        if (openingObject != null)
            openingObject.SetActive(true);

        gameObject.SetActive(false);
    }

    private void RequestSetActive(Transform targetTransform, bool isActive)
    {
        if (targetTransform != null)
            targetTransform.gameObject.SetActive(isActive);
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

