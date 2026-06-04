// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechStage1GroupController.cs
// - 역할: 로드맵, 월드맵, 스테이지 전환 흐름을 담당하는 장면 Controller입니다.
// - 감독 관점: 길 위의 장면 전환 큐시트를 들고 있는 무대감독입니다.
// - 유지보수 포인트: 배경/버튼/캐릭터 배치는 오브젝트와 View가 맡고, 이 스크립트는 순서 지휘만 맡아야 합니다.
// =============================================================================
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Stage1Group의 첫 플레이 흐름을 담당합니다.
/// Moran 배우의 좌우 이동, Stage1-1/Stage1-2 배경 전환, 스테이지 이름 연출, 임무 갱신을 지휘합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechStage1GroupController : MonoBehaviour
{
    // 읽는 순서:
    // 1. OnEnable: Stage1-1을 켜고 Moran, HUD, StageName, 카메라를 준비합니다.
    // 2. Update/MoveMoran: A/D 이동, Shift 달리기, 좌우 flip, 맵 끝 판정을 처리합니다.
    // 3. ChangeMapRoutine: Stage1-1 <-> Stage1-2를 검은 화면 페이드로 전환합니다.
    // 4. FocusCameraOnCurrentMap: 현재 배경 스프라이트가 GameView에 들어오도록 카메라를 맞춥니다.
    // 5. PlayStageNameRoutine: OO_Stage.json의 Name을 위쪽 StageName 텍스트로 보여줍니다.
    // 유지보수 주의:
    // - Stage 배경 이미지는 Stage1-1, Stage1-2 오브젝트 SpriteRenderer에서 직접 바꿉니다.
    // - 이 스크립트는 Stage1의 큐시트만 맡고, Stage2 이후는 같은 구조를 복제/확장합니다.
    // - 카메라가 검은 화면을 보이면 먼저 Main Camera Orthographic과 CameraFollow 상태를 확인합니다.

    [Header("Data")]
    [SerializeField] private string _stageDataId = "OO_Stage_1";
    [SerializeField] private string _stageQuestDataId = "Stage1_Quest_01";
    [SerializeField] private string _currentGroupName = "Stage1Group";

    [Header("Scene Object Names")]
    [SerializeField] private string _stageMap1Name = "Stage1-1";
    [SerializeField] private string _stageMap2Name = "Stage1-2";
    [SerializeField] private string _moranObjectName = "Moran";
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
    [SerializeField] private float _walkingAnimationSpeed = 0.75f;
    [SerializeField] private float _runningAnimationSpeed = 1.5f;

    [Header("Transition")]
    [SerializeField] private float _fadeOutSeconds = 0.35f;
    [SerializeField] private float _blackHoldSeconds = 0.08f;
    [SerializeField] private float _fadeInSeconds = 0.4f;

    [Header("Stage Title")]
    [SerializeField] private float _stageTitleFadeSeconds = 0.6f;
    [SerializeField] private float _stageTitleHoldSeconds = 1.1f;

    [Header("Entry Tutorial")]
    [SerializeField] private string _tutorialGuideGroupName = "TutorialGuideGroup";
    [SerializeField] private string _entryTutorialId = "narration_tutorial_13";

    [Header("Village Chief Interaction")]
    [SerializeField] private string _villageChiefObjectName = "VillageChief";
    [SerializeField] private string _villageChiefPromptObjectName = "Ebutton_VillageChief";
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private string _villageChiefDialogueId = "character_VillageChief_01";
    [SerializeField] private string _villageChiefNextQuestDataId = "Stage1_Quest_02";
    [SerializeField] private KeyCode _villageChiefInteractionKey = KeyCode.E;
    [SerializeField] private float _villageChiefInteractionDistance = 2.4f;
    [SerializeField] private Vector3 _villageChiefPromptOffset = new Vector3(0f, 1.45f, 0f);
    [SerializeField] private bool _isVillageChiefDefaultFacingLeft = false;

    private GameObject Object_StageMap1;
    private GameObject Object_StageMap2;
    private GameObject Object_Moran;
    private GameObject Object_VillageChief;
    private GameObject Object_VillageChiefPrompt;
    private SpriteRenderer Renderer_Moran;
    private SpriteRenderer Renderer_VillageChief;
    private Animator Animator_Moran;
    private TextMeshProUGUI Text_StageName;
    private OOTechNPCInteractionActor Actor_VillageChief;
    private OOTechTutorialGuideUI UI_TutorialGuide;
    private DialogueUI UI_Dialogue;
    private OOTechRoadHUDController HUD_Road;
    private Camera Camera_Main;
    private CameraFollowController Camera_Follow;
    private Canvas Canvas_Fade;
    private Image Image_Fade;
    private int _currentMapIndex;
    private bool _isChangingMap;
    private bool _isInputLocked;
    private bool _isVillageChiefDialoguePlaying;
    private bool _hasSavedCameraFollowState;
    private bool _savedCameraFollowEnabled;
    private string _currentAnimationStateName;

    /// <summary>
    /// Stage1Group이 켜질 때 배우와 배경을 첫 장면 기준으로 준비합니다.
    /// </summary>
    private void OnEnable()
    {
        ResolveReferences();
        SaveAndDisableCameraFollow();
        DisablePlaceholderCanvas();
        PrepareHUDMission();
        PrepareStage();
        PrepareVillageChiefInteraction();
        StartCoroutine(PlayStageNameRoutine());
        RequestShowEntryTutorial();
    }

    /// <summary>
    /// Stage1Group이 꺼질 때 애니메이션과 페이드 상태를 정리합니다.
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
        _isChangingMap = false;
        _isInputLocked = false;
        _isVillageChiefDialoguePlaying = false;
        SetFadeAlpha(0f);
        PlayMoranState(_idleStateName, 1f);
        ReleaseVillageChiefInteraction();
        RestoreCameraFollow();
    }

    /// <summary>
    /// 플레이어 입력에 따라 Moran을 좌우로 이동시키고 맵 경계 전환을 판정합니다.
    /// </summary>
    private void Update()
    {
        UpdateVillageChiefLookDirection();

        if (_isChangingMap || _isInputLocked || _isVillageChiefDialoguePlaying || Object_Moran == null)
        {
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

        bool isRunning = Input.GetKey(_runKey);
        MoveMoran(direction, isRunning);
    }

    private void ResolveReferences()
    {
        Object_StageMap1 = Object_StageMap1 != null ? Object_StageMap1 : FindChildByName(transform, _stageMap1Name);
        Object_StageMap2 = Object_StageMap2 != null ? Object_StageMap2 : FindChildByName(transform, _stageMap2Name);
        Object_Moran = Object_Moran != null ? Object_Moran : FindChildByName(transform, _moranObjectName);
        Object_VillageChief = Object_VillageChief != null ? Object_VillageChief : FindChildByName(transform, _villageChiefObjectName);
        Object_VillageChiefPrompt = Object_VillageChiefPrompt != null ? Object_VillageChiefPrompt : FindChildByName(transform, _villageChiefPromptObjectName);

        if (Object_Moran != null)
        {
            Renderer_Moran = Renderer_Moran != null ? Renderer_Moran : Object_Moran.GetComponentInChildren<SpriteRenderer>(true);
            Animator_Moran = Animator_Moran != null ? Animator_Moran : Object_Moran.GetComponentInChildren<Animator>(true);
        }

        if (Object_VillageChief != null)
            Renderer_VillageChief = Renderer_VillageChief != null ? Renderer_VillageChief : Object_VillageChief.GetComponentInChildren<SpriteRenderer>(true);

        HUD_Road = HUD_Road != null ? HUD_Road : GetComponent<OOTechRoadHUDController>();
        Camera_Main = Camera_Main != null ? Camera_Main : Camera.main;

        if (Camera_Follow == null && Camera_Main != null)
            Camera_Main.TryGetComponent(out Camera_Follow);

        ResolveStageNameText();
        CreateFadeCanvasIfNeeded();
    }

    private void ResolveStageNameText()
    {
        if (Text_StageName != null)
            return;

        GameObject stageNameObject = FindChildByName(transform, _stageNameObjectName);

        if (stageNameObject == null)
            stageNameObject = FindSceneObjectByName(_stageNameObjectName);

        Text_StageName = stageNameObject != null ? stageNameObject.GetComponentInChildren<TextMeshProUGUI>(true) : null;

        if (Text_StageName != null)
            Text_StageName.gameObject.SetActive(false);
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
            tutorialGroup = FindSceneObjectByName(_tutorialGuideGroupName);

        if (tutorialGroup == null)
            return null;

        tutorialGroup.SetActive(true);
        return tutorialGroup.GetComponentInChildren<OOTechTutorialGuideUI>(true);
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
            dialogueGroup = FindSceneObjectByName(_dialogueGroupName);

        if (dialogueGroup == null)
            return null;

        dialogueGroup.SetActive(true);
        return dialogueGroup.GetComponentInChildren<DialogueUI>(true);
    }

    private void CloseDialogueUI()
    {
        if (OOTechUIManager.Inst != null && OOTechUIManager.Inst.CloseUI(_dialogueGroupName))
            return;

        GameObject dialogueGroup = FindSceneObjectByName(_dialogueGroupName);

        if (dialogueGroup != null)
            dialogueGroup.SetActive(false);
    }

    private void DisablePlaceholderCanvas()
    {
        GameObject placeholderCanvas = FindChildByName(transform, _placeholderCanvasName);

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

        OO_StageQuest questData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStageQuestData(_stageQuestDataId) : null;
        string questText = questData != null && !string.IsNullOrEmpty(questData.Description) ? questData.Description : "이 마을의 촌장을 만나세요";
        HUD_Road.RequestSetStageQuestMission(questText);
    }

    private void PrepareStage()
    {
        _currentMapIndex = 0;
        SetCurrentMapActive();
        SetMoranActive(true);
        PlaceMoranAtMapEntry(false);
        FocusCameraOnCurrentMap();
        PlayMoranState(_idleStateName, 1f);
        SetFadeAlpha(0f);
    }

    /// <summary>
    /// Stage1 입장 직후 튜토리얼 안내를 열고, 안내가 끝날 때까지 Moran 조작을 잠급니다.
    /// 무대 비유로는 첫 장면 시작 전에 관객에게 관람 포인트를 알려주는 안내 방송입니다.
    /// </summary>
    private void RequestShowEntryTutorial()
    {
        _isInputLocked = true;
        SetVillageChiefInteractable(false);

        OO_Tutorial tutorialData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetTutorialData(_entryTutorialId) : null;

        if (tutorialData == null)
        {
            Debug.LogWarning($"[OOTechStage1GroupController] Tutorial data is missing: {_entryTutorialId}");
            FinishEntryTutorial();
            return;
        }

        UI_TutorialGuide = ResolveTutorialGuideUI();

        if (UI_TutorialGuide == null)
        {
            Debug.LogWarning("[OOTechStage1GroupController] TutorialGuideGroup is missing.");
            FinishEntryTutorial();
            return;
        }

        UI_TutorialGuide.ShowGuide(tutorialData, FinishEntryTutorial);
    }

    /// <summary>
    /// 입장 튜토리얼이 끝나면 Moran 조작과 촌장 E 상호작용을 다시 엽니다.
    /// </summary>
    private void FinishEntryTutorial()
    {
        _isInputLocked = false;
        SetVillageChiefInteractable(true);
    }

    /// <summary>
    /// 촌장 배우에게 NPC 상호작용 역할표를 연결합니다.
    /// Controller는 순서만 지휘하고, 거리/E 입력 감지는 Actor 컴포넌트가 맡습니다.
    /// </summary>
    private void PrepareVillageChiefInteraction()
    {
        if (Object_VillageChief == null)
            return;

        Actor_VillageChief = Object_VillageChief.GetComponent<OOTechNPCInteractionActor>();

        if (Actor_VillageChief == null)
        {
            Debug.LogWarning("[OOTechStage1GroupController] VillageChief needs OOTechNPCInteractionActor component.");
            return;
        }

        Actor_VillageChief.InteractionRequested -= OnVillageChiefInteractionRequested;
        Actor_VillageChief.InteractionRequested += OnVillageChiefInteractionRequested;
        Actor_VillageChief.RequestSetup(Object_Moran != null ? Object_Moran.transform : null, Object_VillageChiefPrompt);
        Actor_VillageChief.RequestSetInteractionData(_villageChiefDialogueId, _villageChiefNextQuestDataId);
        Actor_VillageChief.RequestSetInteractionRule(_villageChiefInteractionDistance, _villageChiefPromptOffset, _villageChiefInteractionKey);
        Actor_VillageChief.RequestSetInteractable(false);
    }

    private void ReleaseVillageChiefInteraction()
    {
        if (Actor_VillageChief == null)
            return;

        Actor_VillageChief.InteractionRequested -= OnVillageChiefInteractionRequested;
        Actor_VillageChief.RequestSetInteractable(false);
    }

    private void SetVillageChiefInteractable(bool isInteractable)
    {
        if (Actor_VillageChief == null)
            return;

        Actor_VillageChief.RequestSetInteractable(isInteractable);
    }

    private void OnVillageChiefInteractionRequested(OOTechNPCInteractionActor interactionActor)
    {
        if (_isVillageChiefDialoguePlaying)
            return;

        StartCoroutine(PlayVillageChiefDialogueRoutine(interactionActor));
    }

    /// <summary>
    /// 촌장과 상호작용하면 DialogueGroup을 열어 OO_Dialogue 데이터를 보여준 뒤 StageQuest를 갱신합니다.
    /// 무대감독은 "촌장 대사 후 새 임무"라는 큐시트 순서만 담당합니다.
    /// </summary>
    private IEnumerator PlayVillageChiefDialogueRoutine(OOTechNPCInteractionActor interactionActor)
    {
        _isInputLocked = true;
        _isVillageChiefDialoguePlaying = true;
        SetVillageChiefInteractable(false);
        PlayMoranState(_idleStateName, 1f);

        string dialogueDataId = interactionActor != null && !string.IsNullOrEmpty(interactionActor.DialogueDataId) ? interactionActor.DialogueDataId : _villageChiefDialogueId;
        OO_Dialogue dialogueData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetDialogueData(dialogueDataId) : null;

        if (dialogueData != null)
            yield return ShowDialogueDataAndWait(dialogueData);
        else
            Debug.LogWarning($"[OOTechStage1GroupController] Dialogue data is missing: {dialogueDataId}");

        string nextQuestDataId = interactionActor != null && !string.IsNullOrEmpty(interactionActor.NextStageQuestDataId) ? interactionActor.NextStageQuestDataId : _villageChiefNextQuestDataId;
        RequestUpdateStageQuest(nextQuestDataId);

        _isVillageChiefDialoguePlaying = false;
        _isInputLocked = false;
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

    /// <summary>
    /// OO_StageQuest 데이터로 임무 HUD를 갱신합니다.
    /// 텍스트는 코드에 박지 않고 JSON 큐시트에서 가져옵니다.
    /// </summary>
    private void RequestUpdateStageQuest(string stageQuestDataId)
    {
        if (string.IsNullOrEmpty(stageQuestDataId))
            return;

        _stageQuestDataId = stageQuestDataId;
        OO_StageQuest questData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStageQuestData(_stageQuestDataId) : null;
        string questText = questData != null && !string.IsNullOrEmpty(questData.Description) ? questData.Description : _stageQuestDataId;

        if (HUD_Road != null)
            HUD_Road.RequestSetStageQuestMission(questText);
    }

    private void MoveMoran(int direction, bool isRunning)
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (mapRenderer == null)
            return;

        float speed = isRunning ? _runSpeed : _walkSpeed;
        Vector3 position = Object_Moran.transform.position;
        position.x += direction * speed * Time.deltaTime;
        position.y = CalculateLaneY(mapRenderer);

        float leftX = mapRenderer.bounds.min.x + _leftStartMargin;
        float rightX = mapRenderer.bounds.max.x - _edgeExitMargin;

        if (_currentMapIndex == 0)
            position.x = Mathf.Max(position.x, leftX);

        Object_Moran.transform.position = position;

        if (Renderer_Moran != null)
            Renderer_Moran.flipX = direction < 0;

        PlayMoranState(isRunning ? _runningStateName : _walkingStateName, isRunning ? _runningAnimationSpeed : _walkingAnimationSpeed);

        if (_currentMapIndex == 0 && position.x >= rightX)
            StartCoroutine(ChangeMapRoutine(1));
        else if (_currentMapIndex == 1 && position.x <= mapRenderer.bounds.min.x + _edgeExitMargin)
            StartCoroutine(ChangeMapRoutine(0));
    }

    private IEnumerator ChangeMapRoutine(int nextMapIndex)
    {
        if (_isChangingMap)
            yield break;

        _isChangingMap = true;
        PlayMoranState(_idleStateName, 1f);

        yield return FadeRoutine(0f, 1f, _fadeOutSeconds);

        if (_blackHoldSeconds > 0f)
            yield return new WaitForSeconds(_blackHoldSeconds);

        _currentMapIndex = nextMapIndex;
        SetCurrentMapActive();
        PlaceMoranAtMapEntry(nextMapIndex == 0);
        FocusCameraOnCurrentMap();

        yield return FadeRoutine(1f, 0f, _fadeInSeconds);
        _isChangingMap = false;
    }

    private void SetCurrentMapActive()
    {
        if (Object_StageMap1 != null)
            Object_StageMap1.SetActive(_currentMapIndex == 0);

        if (Object_StageMap2 != null)
            Object_StageMap2.SetActive(_currentMapIndex == 1);

        SetVillageChiefVisibleForCurrentMap();
    }

    /// <summary>
    /// VillageChief 배우는 Stage1-1 마을 장면에만 등장시킵니다.
    /// 영화로 치면 촌장은 마을 입구 세트의 배우라서, Stage1-2로 화면 전환되면 무대 뒤로 퇴장합니다.
    /// </summary>
    private void SetVillageChiefVisibleForCurrentMap()
    {
        if (Object_VillageChief == null)
            return;

        bool isVillageChiefVisible = _currentMapIndex == 0;
        Object_VillageChief.SetActive(isVillageChiefVisible);
        UpdateVillageChiefLookDirection();

        if (Actor_VillageChief == null)
            return;

        bool isInteractionOpen = isVillageChiefVisible && !_isInputLocked && !_isVillageChiefDialoguePlaying;
        Actor_VillageChief.RequestSetInteractable(isInteractionOpen);
    }

    /// <summary>
    /// 촌장 배우가 Moran 배우 쪽을 바라보도록 좌우 방향을 갱신합니다.
    /// 영화로 치면 대사를 기다리는 배우가 상대 배우 위치에 맞춰 고개 방향을 맞추는 동선 정리입니다.
    /// </summary>
    private void UpdateVillageChiefLookDirection()
    {
        if (_currentMapIndex != 0)
            return;

        if (Object_Moran == null || Object_VillageChief == null || Renderer_VillageChief == null)
            return;

        if (!Object_VillageChief.activeInHierarchy)
            return;

        bool isMoranOnRight = Object_Moran.transform.position.x > Object_VillageChief.transform.position.x;
        Renderer_VillageChief.flipX = _isVillageChiefDefaultFacingLeft ? isMoranOnRight : !isMoranOnRight;
    }

    private void PlaceMoranAtMapEntry(bool isFromLeftEdge)
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (Object_Moran == null || mapRenderer == null)
            return;

        Vector3 position = Object_Moran.transform.position;
        position.x = isFromLeftEdge ? mapRenderer.bounds.max.x - _leftStartMargin : mapRenderer.bounds.min.x + _leftStartMargin;
        position.y = CalculateLaneY(mapRenderer);
        Object_Moran.transform.position = position;
    }

    private float CalculateLaneY(SpriteRenderer mapRenderer)
    {
        return Mathf.Lerp(mapRenderer.bounds.min.y, mapRenderer.bounds.max.y, Mathf.Clamp01(_moranLaneNormalizedHeight));
    }

    private SpriteRenderer GetCurrentMapRenderer()
    {
        GameObject mapObject = _currentMapIndex == 0 ? Object_StageMap1 : Object_StageMap2;
        return FindBestMapRenderer(mapObject);
    }

    private SpriteRenderer FindBestMapRenderer(GameObject mapObject)
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

        if (Camera_Main.orthographic)
        {
            float aspect = Mathf.Max(0.01f, Camera_Main.aspect);
            float sizeByHeight = bounds.extents.y;
            float sizeByWidth = bounds.extents.x / aspect;
            Camera_Main.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth) * 1.02f;
        }
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

    private void PlayMoranState(string stateName, float speed)
    {
        if (Animator_Moran == null || string.IsNullOrEmpty(stateName))
            return;

        if (!Animator_Moran.gameObject.activeInHierarchy)
            return;

        Animator_Moran.speed = speed;

        if (_currentAnimationStateName == stateName)
            return;

        _currentAnimationStateName = stateName;
        Animator_Moran.Play(stateName, 0, 0f);
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

    private GameObject FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == objectName || rootTransform.name.Trim() == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = FindChildByName(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }
}
