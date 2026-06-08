// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechStage3EncounterController.cs
// - 역할: EncounterGroup의 산군 퀘스트 복귀 판정과 클리어 큐를 지휘합니다.
// - 영화 비유: 감독은 "대사 시작, 부엌으로 이동, 꿀떡 판정, 산군 퇴장, 다음 막 버튼"만 부릅니다.
// - 유지보수 포인트: 선택지 문장과 보상은 OO_Stage3CueSheet/OO_Choice/OO_StageQuest 데이터에서 읽습니다.
// =============================================================================
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// EncounterGroup의 Stage3 산군 퀘스트를 담당합니다.
/// Game View에서는 첫 대사 후 CookingGroup으로 이동하고, 복귀 시 인벤토리 상태에 따라 선택지를 표시합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechStage3EncounterController : MonoBehaviour
{
    private const float EncounterBackgroundWorldWidth = 1920f;
    private const float EncounterBackgroundWorldHeight = 1080f;

    [Header("Data")]
    [SerializeField] private string _cueSheetDataId = "Stage3_CueSheet_01";

    [Header("Role Components")]
    [SerializeField] private OOTechSceneContext Context_Scene;
    [SerializeField] private OOTechStage3DialogueCue Cue_Dialogue;
    [SerializeField] private OOTechStage2CameraCue Cue_Camera;

    [Header("Group")]
    [SerializeField] private string _cookingGroupName = "CookingGroup";
    [SerializeField] private string _mainMenuGroupName = "MainMenuGroup";

    [Header("Background")]
    [SerializeField] private Sprite Sprite_Stage3Background;

    [Header("Clear UI")]
    [SerializeField] private string _stageClearCanvasName = "Canvas_Stage3Clear";
    [SerializeField] private string _stageClearTitle = "북쪽 숲 임무 완수";
    [SerializeField] private string _stageClearMessage = "산군이 약속의 뜻으로 당근 x10 묶음을 줬습니다. 다음 여정에 나설 준비가 끝났습니다.";

    [Header("Actor State")]
    [SerializeField] private string _mrJaeikThreateningStateName = "Mr_Jaeik_isThreatening";

    private OO_Stage3CueSheet Data_CueSheet;
    private OOTechRoadHUDController HUD_Road;
    private OOTechStageActorMotion Actor_Sangun;
    private OOTechStageActorMotion Actor_Moran;
    private OOTechStageActorMotion Actor_MrJaeik;
    private GameObject Object_ClearCanvas;
    private Button Button_NextStage;
    private Coroutine Coroutine_Sequence;
    private bool _hasInitialSequencePlayed;
    private bool _isQuestCleared;

    /// <summary>
    /// EncounterGroup이 열린 동안 뒤 무대가 다시 켜지면 Game View에 도플갱어가 찍힙니다.
    /// 촬영 중 계속 체크해서 Stage3 배경만 무대 뒤에 남도록 보험을 겁니다.
    /// </summary>
    private void LateUpdate()
    {
        if (!gameObject.activeInHierarchy)
            return;

        RequestCloseStage3GroupBehindEncounter();
        RequestPrepareEncounterBackground();
        RequestFitCameraToEncounterBackground();
        RequestDisableForeignMrJaeikRenderers();
        RequestKeepStage3ClearVictoryActorsAlive();
    }

    /// <summary>
    /// EncounterGroup이 열릴 때 초기 대사 또는 복귀 판정을 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        RequestStartEncounterCue();
    }

    private void OnDisable()
    {
        if (Coroutine_Sequence != null)
        {
            StopCoroutine(Coroutine_Sequence);
            Coroutine_Sequence = null;
        }
    }

    /// <summary>
    /// 외부에서 EncounterGroup을 다시 리허설할 때 사용하는 시작 메서드입니다.
    /// </summary>
    public void RequestStartEncounterCue()
    {
        ResolveComponents();
        ResolveCueSheetData();

        if (Coroutine_Sequence != null)
            StopCoroutine(Coroutine_Sequence);

        Coroutine_Sequence = StartCoroutine(PlayEncounterRoutine());
    }

    private IEnumerator PlayEncounterRoutine()
    {
        if (Data_CueSheet == null)
        {
            Debug.LogWarning("[OOTechStage3EncounterController] Stage3 cue sheet missing.");
            yield break;
        }

        RequestCloseStage3GroupBehindEncounter();
        PrepareEncounterActors();
        RequestCloseInventoryForReturnedEncounter();
        SetNextButtonActive(false);

        if (!_hasInitialSequencePlayed)
        {
            yield return PlayInitialDialogueRoutine();
            _hasInitialSequencePlayed = true;
            RequestOpenCookingGroup();
            Coroutine_Sequence = null;
            yield break;
        }

        yield return EvaluateReturnedInventoryRoutine();
        Coroutine_Sequence = null;
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

        if (Cue_Camera == null)
            Cue_Camera = GetComponent<OOTechStage2CameraCue>();

        if (Cue_Camera == null)
            Cue_Camera = gameObject.AddComponent<OOTechStage2CameraCue>();

        HUD_Road = HUD_Road != null ? HUD_Road : OOTechSceneQuery.RequestFirstComponent<OOTechRoadHUDController>(
            delegate (OOTechRoadHUDController targetHUD)
            {
                return targetHUD != null && targetHUD.gameObject.activeInHierarchy;
            },
            true);
        Button_NextStage = Button_NextStage != null ? Button_NextStage : ResolveNextStageButton();
        Object_ClearCanvas = Object_ClearCanvas != null ? Object_ClearCanvas : ResolveClearCanvasObject();
    }

    private void ResolveCueSheetData()
    {
        Data_CueSheet = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStage3CueSheetData(_cueSheetDataId) : null;
    }

    private void PrepareEncounterActors()
    {
        if (Data_CueSheet == null || Context_Scene == null)
            return;

        RequestPrepareEncounterBackground();
        RequestFitCameraToEncounterBackground();
        Actor_Sangun = ResolveActor(Data_CueSheet.SangunRoleId);
        Actor_Moran = ResolveActor(string.IsNullOrEmpty(Data_CueSheet.MoranRoleId) ? "Moran" : Data_CueSheet.MoranRoleId);
        Actor_MrJaeik = ResolveActor(string.IsNullOrEmpty(Data_CueSheet.MrJaeikRoleId) ? "Mr.Jaeik" : Data_CueSheet.MrJaeikRoleId);

        RequestDisableDuplicateRoleActors(Data_CueSheet.SangunRoleId, Actor_Sangun);
        RequestDisableDuplicateRoleActors(string.IsNullOrEmpty(Data_CueSheet.MoranRoleId) ? "Moran" : Data_CueSheet.MoranRoleId, Actor_Moran);
        RequestDisableDuplicateRoleActors(string.IsNullOrEmpty(Data_CueSheet.MrJaeikRoleId) ? "Mr.Jaeik" : Data_CueSheet.MrJaeikRoleId, Actor_MrJaeik);
        RequestDisableDuplicateNamedActors("Mr.Jaeik", Actor_MrJaeik);

        RequestPlayActorState(Actor_Sangun, "Sangun_Idle", 1f, false);

        if (_isQuestCleared)
        {
            RequestPlayStage3ClearVictoryActors(true);
            return;
        }

        RequestPlayActorState(Actor_Moran, "Moran_isScared", 0.6f, false);
        RequestPlayActorState(Actor_MrJaeik, _mrJaeikThreateningStateName, 0.6f, false);
    }

    /// <summary>
    /// EncounterGroup이 켜졌는데 이전 Road/Stage 그룹이 뒤에서 살아 있으면 이전 배우가 카메라에 도플갱어처럼 보입니다.
    /// 산군 Encounter 무대가 열릴 때 이전 무대는 확실히 암전시킵니다.
    /// </summary>
    private void RequestCloseStage3GroupBehindEncounter()
    {
        string[] previousGroupNameArray =
        {
            "Stage2Group",
            "Stage3Group",
            "2nd_Road_to_Stage2",
            "3rd_Road_to_Stage3"
        };

        foreach (string previousGroupName in previousGroupNameArray)
        {
            GameObject previousGroupObject = RequestSceneObjectByName(previousGroupName);

            if (previousGroupObject == null || previousGroupObject == gameObject)
                continue;

            if (!previousGroupObject.activeSelf)
                continue;

            previousGroupObject.SetActive(false);
            Debug.LogWarning($"[OOTechStage3EncounterController] Closed previous group behind EncounterGroup: {previousGroupName}");
        }
    }

    /// <summary>
    /// EncounterGroup 배경 배우를 다시 켜고 화면 뒤쪽에 고정합니다.
    /// Game View에서 검은 화면이 나오면 보통 배경 SpriteRenderer가 꺼져 있거나 카메라가 다른 배경을 잡은 상태입니다.
    /// </summary>
    private void RequestPrepareEncounterBackground()
    {
        SpriteRenderer backgroundRenderer = ResolveBestBackgroundRendererIncludingInactive();

        if (backgroundRenderer == null)
        {
            Debug.LogWarning("[OOTechStage3EncounterController] Encounter background renderer is missing.");
            return;
        }

        backgroundRenderer.gameObject.SetActive(true);
        backgroundRenderer.enabled = true;
        backgroundRenderer.forceRenderingOff = false;
        backgroundRenderer.color = Color.white;
        backgroundRenderer.sortingLayerName = "Background";
        backgroundRenderer.sortingOrder = -1000;
        RequestApplyStage3BackgroundSprite(backgroundRenderer);
        RequestFitBackgroundRendererToEncounterStage(backgroundRenderer);
    }

    /// <summary>
    /// 배경 배우가 실수로 다른 스프라이트를 들고 있으면 검은 화면과 거대한 캐릭터 조각처럼 보입니다.
    /// 무대 세트 담당자가 항상 Stage3.png 소품만 들게 고정합니다.
    /// </summary>
    private void RequestApplyStage3BackgroundSprite(SpriteRenderer backgroundRenderer)
    {
        if (backgroundRenderer == null || Sprite_Stage3Background == null)
            return;

        if (backgroundRenderer.sprite == Sprite_Stage3Background)
            return;

        backgroundRenderer.sprite = Sprite_Stage3Background;
        Debug.LogWarning("[OOTechStage3EncounterController] Encounter background sprite corrected to Stage3.png.");
    }

    /// <summary>
    /// EncounterGroup 안에서 같은 역할표를 단 배우가 둘 이상 켜져 있으면 같은 위치에서 두 스프라이트가 번갈아 보여 깜빡임처럼 보입니다.
    /// 선택된 주연 배우만 남기고, 같은 그룹 안의 중복 배우만 끕니다.
    /// </summary>
    private void RequestDisableDuplicateRoleActors(string roleId, OOTechStageActorMotion selectedActor)
    {
        if (string.IsNullOrEmpty(roleId) || selectedActor == null)
            return;

        OOTechSceneObject[] sceneObjectArray = GetComponentsInChildren<OOTechSceneObject>(true);

        foreach (OOTechSceneObject sceneObject in sceneObjectArray)
        {
            if (sceneObject == null || sceneObject.RoleId != roleId)
                continue;

            if (sceneObject.transform == selectedActor.transform || sceneObject.transform.IsChildOf(selectedActor.transform))
                continue;

            sceneObject.gameObject.SetActive(false);
            Debug.LogWarning($"[OOTechStage3EncounterController] Disabled duplicate Encounter actor role={roleId}, object={sceneObject.gameObject.name}.");
        }
    }

    /// <summary>
    /// 사용자가 새로 배치한 배우와 예전 배우가 같은 이름으로 남아 있으면 RoleId가 없어도 카메라에 도플갱어처럼 잡힙니다.
    /// 감독은 선택된 배우만 남기고, 같은 EncounterGroup 안의 같은 이름 배우는 무대 뒤로 내립니다.
    /// </summary>
    private void RequestDisableDuplicateNamedActors(string actorName, OOTechStageActorMotion selectedActor)
    {
        if (string.IsNullOrEmpty(actorName) || selectedActor == null)
            return;

        Transform[] transformArray = GetComponentsInChildren<Transform>(true);

        foreach (Transform actorTransform in transformArray)
        {
            if (actorTransform == null || actorTransform.name != actorName)
                continue;

            if (actorTransform == selectedActor.transform || actorTransform.IsChildOf(selectedActor.transform))
                continue;

            actorTransform.gameObject.SetActive(false);
            Debug.LogWarning($"[OOTechStage3EncounterController] Disabled duplicate Encounter actor name={actorName}, object={actorTransform.gameObject.name}.");
        }
    }

    /// <summary>
    /// EncounterGroup의 선택 배우가 아닌 Mr.Jaeik 렌더러가 다른 그룹에서 살아 있으면 카메라에 도플갱어가 찍힙니다.
    /// 배우를 삭제하지 않고 Renderer/Animator만 꺼서 다음 무대 데이터는 보존합니다.
    /// </summary>
    private void RequestDisableForeignMrJaeikRenderers()
    {
        if (Actor_MrJaeik == null)
            return;

        List<SpriteRenderer> rendererArray = OOTechSceneQuery.RequestCollectComponents<SpriteRenderer>(true);

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null || spriteRenderer.transform.IsChildOf(Actor_MrJaeik.transform))
                continue;

            if (!spriteRenderer.gameObject.activeInHierarchy)
                continue;

            if (!IsMrJaeikRelatedPath(spriteRenderer.transform))
                continue;

            if (!spriteRenderer.enabled && spriteRenderer.forceRenderingOff)
                continue;

            spriteRenderer.enabled = false;
            spriteRenderer.forceRenderingOff = true;

            Animator animator = spriteRenderer.GetComponent<Animator>();

            if (animator != null)
                animator.enabled = false;

            Debug.LogWarning($"[OOTechStage3EncounterController] Hidden foreign Mr.Jaeik renderer: {GetHierarchyPath(spriteRenderer.transform)}");
        }
    }

    /// <summary>
    /// EncounterGroup 배경 전체가 Game View에 들어오도록 카메라를 맞춥니다.
    /// 이전 Stage3 카메라 줌이 남아 배경이 아주 작게 보이는 문제를 막는 촬영 큐입니다.
    /// </summary>
    private void RequestFitCameraToEncounterBackground()
    {
        if (Cue_Camera == null)
            return;

        SpriteRenderer backgroundRenderer = ResolveBestBackgroundRenderer();

        if (backgroundRenderer == null)
            backgroundRenderer = ResolveBestBackgroundRendererIncludingInactive();

        if (backgroundRenderer == null)
            return;

        backgroundRenderer.gameObject.SetActive(true);
        backgroundRenderer.enabled = true;
        RequestApplyStage3BackgroundSprite(backgroundRenderer);
        RequestFitBackgroundRendererToEncounterStage(backgroundRenderer);
        Cue_Camera.RequestSaveAndDisableCameraFollow();
        Cue_Camera.RequestFocusCameraOnBounds(backgroundRenderer.bounds, 1f);
    }

    /// <summary>
    /// EncounterGroup은 큰 무대 좌표계로 배치되어 있으므로 배경도 1920x1080 월드 크기에 맞춥니다.
    /// 배경이 19.2x10.8처럼 작게 저장되면 카메라가 그 작은 포스터를 전체 무대라고 착각해 확대 촬영합니다.
    /// </summary>
    private void RequestFitBackgroundRendererToEncounterStage(SpriteRenderer backgroundRenderer)
    {
        if (backgroundRenderer == null || backgroundRenderer.sprite == null)
            return;

        Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            return;

        float scaleX = EncounterBackgroundWorldWidth / spriteSize.x;
        float scaleY = EncounterBackgroundWorldHeight / spriteSize.y;
        Transform backgroundTransform = backgroundRenderer.transform;
        backgroundTransform.localPosition = Vector3.zero;
        backgroundTransform.localRotation = Quaternion.identity;
        backgroundTransform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    private SpriteRenderer ResolveBestBackgroundRenderer()
    {
        SpriteRenderer[] rendererArray = GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer bestRenderer = null;
        float bestArea = 0f;

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null || !spriteRenderer.enabled)
                continue;

            string objectName = spriteRenderer.gameObject.name;
            bool isBackgroundName = objectName.Contains("Background") || objectName.Contains("Backound") || spriteRenderer.transform == transform;

            if (!isBackgroundName)
                continue;

            float area = spriteRenderer.bounds.size.x * spriteRenderer.bounds.size.y;

            if (area > bestArea)
            {
                bestRenderer = spriteRenderer;
                bestArea = area;
            }
        }

        return bestRenderer;
    }

    private SpriteRenderer ResolveBestBackgroundRendererIncludingInactive()
    {
        SpriteRenderer[] rendererArray = GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer bestRenderer = null;
        float bestArea = 0f;

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
                continue;

            string objectName = spriteRenderer.gameObject.name;
            bool isBackgroundName = objectName.Contains("Stage3Background") || objectName.Contains("Background") || objectName.Contains("Backound");

            if (!isBackgroundName)
                continue;

            float area = spriteRenderer.bounds.size.x * spriteRenderer.bounds.size.y;

            if (area > bestArea)
            {
                bestRenderer = spriteRenderer;
                bestArea = area;
            }
        }

        return bestRenderer;
    }

    private bool IsMrJaeikRelatedPath(Transform targetTransform)
    {
        Transform currentTransform = targetTransform;

        while (currentTransform != null)
        {
            string objectName = currentTransform.name;

            if (objectName.Contains("Mr.Jaeik") || objectName.Contains("Mr_Jaeik"))
                return true;

            currentTransform = currentTransform.parent;
        }

        return false;
    }

    private string GetHierarchyPath(Transform targetTransform)
    {
        if (targetTransform == null)
            return string.Empty;

        string path = targetTransform.name;
        Transform currentTransform = targetTransform.parent;

        while (currentTransform != null)
        {
            path = currentTransform.name + "/" + path;
            currentTransform = currentTransform.parent;
        }

        return path;
    }

    private IEnumerator PlayInitialDialogueRoutine()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, Data_CueSheet.EncounterWaitSeconds));

        yield return Cue_Dialogue.RequestShowDialogueAndWait(Data_CueSheet.EncounterSangunDialogueId);
        yield return Cue_Dialogue.RequestShowDialogueAndWait(Data_CueSheet.EncounterMoranDialogueId);
        yield return Cue_Dialogue.RequestShowDialogueAndWait(Data_CueSheet.EncounterQuestDialogueId);

        RequestUpdateStageQuest();
    }

    private IEnumerator EvaluateReturnedInventoryRoutine()
    {
        if (_isQuestCleared || OOTechGameManager.Inst == null)
            yield break;

        if (OOTechGameManager.Inst.GetItemCount(Data_CueSheet.HoneyKoreanCakeItemId) > 0)
        {
            yield return PlayGiveHoneyCakeChoiceRoutine();
            yield break;
        }

        if (OOTechGameManager.Inst.GetItemCount(Data_CueSheet.KoreanCakeItemId) > 0 &&
            OOTechGameManager.Inst.GetItemCount(Data_CueSheet.HoneyIngredientId) > 0)
        {
            yield return PlayMakeHoneyCakeChoiceRoutine();
            yield break;
        }

        if (OOTechGameManager.Inst.GetItemCount(Data_CueSheet.KoreanCakeItemId) > 0)
            yield return PlayCakeOnlyChoiceRoutine();
    }

    private IEnumerator PlayMakeHoneyCakeChoiceRoutine()
    {
        int selectedIndex = -1;
        yield return Cue_Dialogue.RequestShowChoiceAndWait(Data_CueSheet.MakeHoneyCakeChoiceId, delegate (int index)
        {
            selectedIndex = index;
        });

        if (selectedIndex != 0)
            yield break;

        if (!TryRemoveItem(Data_CueSheet.KoreanCakeItemId, 1) || !TryRemoveItem(Data_CueSheet.HoneyIngredientId, 1))
        {
            Debug.LogWarning("[OOTechStage3EncounterController] Honey cake material missing during choice.");
            yield break;
        }

        AddItem(Data_CueSheet.HoneyKoreanCakeItemId, 1);
        RefreshHUDInventory();
        yield return PlayGiveHoneyCakeChoiceRoutine();
    }

    private IEnumerator PlayCakeOnlyChoiceRoutine()
    {
        int selectedIndex = -1;
        yield return Cue_Dialogue.RequestShowChoiceAndWait(Data_CueSheet.CakeOnlyChoiceId, delegate (int index)
        {
            selectedIndex = index;
        });

        RequestCloseInventoryAfterChoiceDelay();

        if (selectedIndex == 0)
            yield return PlayDeathRoutine();
        else
            RequestOpenCookingGroup();
    }

    private IEnumerator PlayGiveHoneyCakeChoiceRoutine()
    {
        int selectedIndex = -1;
        yield return Cue_Dialogue.RequestShowChoiceAndWait(Data_CueSheet.GiveHoneyCakeChoiceId, delegate (int index)
        {
            selectedIndex = index;
        });

        RequestCloseInventoryAfterChoiceDelay();

        if (selectedIndex != 0)
        {
            yield return PlayDeathRoutine();
            yield break;
        }

        if (!TryRemoveItem(Data_CueSheet.HoneyKoreanCakeItemId, 1))
        {
            Debug.LogWarning("[OOTechStage3EncounterController] Honey Korean Cake is missing.");
            yield break;
        }

        string clearDialogueId = !string.IsNullOrEmpty(Data_CueSheet.ClearDialogueId)
            ? Data_CueSheet.ClearDialogueId
            : "character_Sangun_04";
        yield return Cue_Dialogue.RequestShowDialogueAndWait(clearDialogueId);
        yield return PlayStageClearRoutine();
    }

    /// <summary>
    /// 선택지 이후 인벤토리 패널이 다음 대사 위를 덮지 않도록 잠깐 뒤 닫습니다.
    /// CookingGroup에 다시 들어갈 때는 HUD가 다시 인벤토리를 열어 주므로 조리 진행은 막히지 않습니다.
    /// </summary>
    private void RequestCloseInventoryAfterChoiceDelay()
    {
        if (HUD_Road == null)
            return;

        HUD_Road.RequestCloseInventoryPanelAfterDelay(1.5f);
    }

    private IEnumerator PlayDeathRoutine()
    {
        RequestPlayActorState(Actor_Sangun, "Sangun_isAttacking", Data_CueSheet.AttackingAnimationSpeed > 0f ? Data_CueSheet.AttackingAnimationSpeed : 0.5f, true);
        Debug.LogWarning($"[OOTechStage3EncounterController] {Data_CueSheet.DeathMessage}");
        GameObject deathOverlayObject = ResolveOrCreateDeathOverlay();
        deathOverlayObject.SetActive(true);
        yield return new WaitForSeconds(1.2f);

        int selectedIndex = -1;
        yield return Cue_Dialogue.RequestShowChoiceAndWait(Data_CueSheet.DeathRetryChoiceId, delegate (int index)
        {
            selectedIndex = index;
        });

        deathOverlayObject.SetActive(false);

        if (selectedIndex == 0)
        {
            _hasInitialSequencePlayed = false;
            RequestStartEncounterCue();
            yield break;
        }

        RequestSwitchGroup(gameObject.name, _mainMenuGroupName);
    }

    /// <summary>
    /// 산군에게 떡만 줬을 때 화면 전체를 붉게 물들이는 실패 연출판입니다.
    /// Game View에서는 감독이 조명을 붉게 바꾸고 "YOU DIE" 자막을 잠깐 띄우는 장면입니다.
    /// </summary>
    private GameObject ResolveOrCreateDeathOverlay()
    {
        GameObject overlayObject = RequestChildObjectByName(transform, "Canvas_Stage3DeathOverlay");

        if (overlayObject == null)
            overlayObject = new GameObject("Canvas_Stage3DeathOverlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        overlayObject.transform.SetParent(transform, false);
        RequestRepairFullScreenRect(overlayObject.GetComponent<RectTransform>());

        Canvas canvas = overlayObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 5300;

        CanvasScaler canvasScaler = overlayObject.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = ResolveOrCreateUIObject(overlayObject.transform, "Panel_DeathOverlay");
        RequestRepairFullScreenRect(panelObject.GetComponent<RectTransform>());

        Image panelImage = panelObject.GetComponent<Image>();

        if (panelImage == null)
            panelImage = panelObject.AddComponent<Image>();

        panelImage.color = new Color(0.55f, 0f, 0f, 0.42f);
        panelImage.raycastTarget = false;

        TextMeshProUGUI deathText = ResolveOrCreateText(panelObject.transform, "Text_DeathMessage", Vector2.zero, new Vector2(900f, 120f));
        deathText.text = string.IsNullOrEmpty(Data_CueSheet.DeathMessage) ? "YOU DIE" : Data_CueSheet.DeathMessage;
        deathText.fontSize = 72f;
        deathText.fontStyle = FontStyles.Bold;
        deathText.color = Color.white;

        overlayObject.SetActive(false);
        return overlayObject;
    }

    private IEnumerator PlayStageClearRoutine()
    {
        _isQuestCleared = true;
        RequestPlayStage3ClearVictoryActors(true);
        yield return MoveSangunOffStageRoutine();

        GiveClearReward();
        RefreshHUDInventory();
        RequestUpdateStageQuestAsComplete();

        if (OOTechGameManager.Inst != null)
            OOTechGameManager.Inst.MarkStageCleared("Stage3");

        SetNextButtonActive(true);
        RequestPlayStage3ClearVictoryActors(false);
    }

    private void RequestKeepStage3ClearVictoryActorsAlive()
    {
        if (!_isQuestCleared)
            return;

        RequestPlayStage3ClearVictoryActors(false);
    }

    private void RequestPlayStage3ClearVictoryActors(bool isForceReplay)
    {
        RequestPlayStage3VictoryState(Actor_Moran, "Moran_Victory", null, isForceReplay);
        RequestPlayStage3VictoryState(Actor_MrJaeik, "Mr.Jaeik_Victory", "Mr_Jaeik_Victory", isForceReplay);
    }

    private void RequestPlayStage3VictoryState(OOTechStageActorMotion actor, string primaryStateName, string fallbackStateName, bool isForceReplay)
    {
        if (actor == null)
            return;

        actor.RequestForceVisibleRenderer();

        if (isForceReplay && actor.RequestPlayVictory(1f))
            return;

        if (RequestPlayActorState(actor, primaryStateName, 1f, isForceReplay))
            return;

        if (!string.IsNullOrEmpty(fallbackStateName))
            RequestPlayActorState(actor, fallbackStateName, 1f, isForceReplay);
    }

    private IEnumerator MoveSangunOffStageRoutine()
    {
        if (Actor_Sangun == null)
            yield break;

        Vector3 targetPosition = Actor_Sangun.transform.position + Vector3.right * 1400f;
        yield return Actor_Sangun.MoveToWorldPositionRoutine(targetPosition, 220f, "Sangun_isWalking", 1f);
        Actor_Sangun.gameObject.SetActive(false);
    }

    private void GiveClearReward()
    {
        if (Data_CueSheet == null || Data_CueSheet.ClearRewardItemIdList == null)
            return;

        for (int index = 0; index < Data_CueSheet.ClearRewardItemIdList.Count; index++)
        {
            string itemId = Data_CueSheet.ClearRewardItemIdList[index];
            int count = Data_CueSheet.ClearRewardCountList != null && index < Data_CueSheet.ClearRewardCountList.Count
                ? Mathf.Max(1, Data_CueSheet.ClearRewardCountList[index])
                : 1;
            AddItem(itemId, count);
        }
    }

    private void RequestUpdateStageQuest()
    {
        if (HUD_Road == null)
            return;

        string questText = ResolveStageQuestDescription(Data_CueSheet != null ? Data_CueSheet.StageQuestId : null, "떡 1개와 꿀 1개로 꿀떡을 만드세요.");

        HUD_Road.RequestSetStageQuestMission(questText);
        HUD_Road.SetMissionNewBadgeActive(true);
    }

    private string ResolveStageQuestDescription(string stageQuestDataId, string fallbackText)
    {
        OO_StageQuest questData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStageQuestData(stageQuestDataId) : null;

        if (questData != null && !string.IsNullOrWhiteSpace(questData.Description))
            return questData.Description;

        return fallbackText;
    }

    private void RequestUpdateStageQuestAsComplete()
    {
        if (HUD_Road == null)
            return;

        HUD_Road.RequestSetStageQuestMission("Stage3 임무 완료: 다음 길로 이동하세요.");
        HUD_Road.SetMissionNewBadgeActive(true);
    }

    private void RequestOpenCookingGroup()
    {
        GameObject cookingGroup = RequestSceneObjectByName(_cookingGroupName);
        OOTechGroupNavigationHistory.SetPreviousGroup(_cookingGroupName, gameObject.name);

        if (cookingGroup != null && OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.RegisterUI(_cookingGroupName, cookingGroup);
            PrepareCookingBackButtons(cookingGroup);
        }
        else if (cookingGroup != null)
        {
            PrepareCookingBackButtons(cookingGroup);
        }

        RequestSwitchGroup(gameObject.name, _cookingGroupName);
    }

    /// <summary>
    /// EncounterGroup에서 열린 부엌의 돌아가기 버튼은 반드시 EncounterGroup으로 돌아오게 지정합니다.
    /// Game View에서는 Stage2Group 같은 이전 로드로 튀지 않고 산군 장면으로 복귀합니다.
    /// </summary>
    private void PrepareCookingBackButtons(GameObject cookingGroup)
    {
        if (cookingGroup == null)
            return;

        BackButtonController[] backButtonArray = cookingGroup.GetComponentsInChildren<BackButtonController>(true);

        foreach (BackButtonController backButton in backButtonArray)
            backButton.SetPreviousGroup(gameObject.name);

        Button runtimeReturnButton = ResolveRuntimeReturnButton(cookingGroup);

        if (runtimeReturnButton == null)
            return;

        runtimeReturnButton.onClick.RemoveAllListeners();
        runtimeReturnButton.onClick.AddListener(OnCookingReturnToEncounterClicked);
    }

    private Button ResolveRuntimeReturnButton(GameObject cookingGroup)
    {
        if (cookingGroup == null)
            return null;

        GameObject returnButtonObject = RequestChildObjectByName(cookingGroup.transform, "Button_RuntimeReturn");

        if (returnButtonObject == null)
            return null;

        return returnButtonObject.GetComponent<Button>();
    }

    /// <summary>
    /// Stage3 Encounter 전용 부엌 복귀 버튼입니다.
    /// 이전 Road/Stage 기록이 남아 있어도 CookingGroup을 닫고 반드시 EncounterGroup을 다시 엽니다.
    /// </summary>
    private void OnCookingReturnToEncounterClicked()
    {
        OOTechGroupNavigationHistory.SetPreviousGroup(_cookingGroupName, gameObject.name);

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.CloseUI(_cookingGroupName);
            OOTechUIManager.Inst.OpenUI(gameObject.name);
            RequestCloseInventoryForReturnedEncounter();
            return;
        }

        GameObject cookingGroup = RequestSceneObjectByName(_cookingGroupName);

        if (cookingGroup != null)
            cookingGroup.SetActive(false);

        gameObject.SetActive(true);
        RequestCloseInventoryForReturnedEncounter();
    }

    private void RequestCloseInventoryForReturnedEncounter()
    {
        if (HUD_Road == null)
            return;

        HUD_Road.RequestForceCloseInventoryPanelForEncounterReturn();
    }

    private void RequestSwitchGroup(string currentGroupName, string nextGroupName)
    {
        if (OOTechUIManager.Inst != null)
        {
            GameObject nextGroup = RequestSceneObjectByName(nextGroupName);

            if (nextGroup != null)
                OOTechUIManager.Inst.RegisterUI(nextGroupName, nextGroup);

            OOTechUIManager.Inst.CloseUI(currentGroupName);
            OOTechUIManager.Inst.OpenUI(nextGroupName);
            return;
        }

        GameObject currentGroup = RequestSceneObjectByName(currentGroupName);
        GameObject nextGroupObject = RequestSceneObjectByName(nextGroupName);

        if (currentGroup != null)
            currentGroup.SetActive(false);

        if (nextGroupObject != null)
            nextGroupObject.SetActive(true);
    }

    private OOTechStageActorMotion ResolveActor(string roleId)
    {
        if (Context_Scene == null || string.IsNullOrEmpty(roleId))
            return null;

        Transform anchorTransform = Actor_Moran != null ? Actor_Moran.transform : null;
        Transform actorTransform = ResolvePreferredActorTransform(roleId, anchorTransform);
        return actorTransform != null ? actorTransform.GetComponent<OOTechStageActorMotion>() : null;
    }

    /// <summary>
    /// 같은 역할표가 둘 이상 있으면 Moran과 가장 가까운 배우를 우선합니다.
    /// 사용자가 새로 배치한 Mr.Jaeik이 Moran 근처에 있고, 예전 먼 배우가 남아 있는 경우를 막는 선택 규칙입니다.
    /// </summary>
    private Transform ResolvePreferredActorTransform(string roleId, Transform anchorTransform)
    {
        OOTechSceneObject[] sceneObjectArray = GetComponentsInChildren<OOTechSceneObject>(true);
        Transform fallbackTransform = Context_Scene.GetRoleTransform(roleId);

        if (sceneObjectArray == null || sceneObjectArray.Length == 0)
            return fallbackTransform;

        Transform bestTransform = null;
        float bestScore = float.MaxValue;

        foreach (OOTechSceneObject sceneObject in sceneObjectArray)
        {
            if (sceneObject == null || sceneObject.RoleId != roleId)
                continue;

            if (!sceneObject.gameObject.activeInHierarchy && sceneObject.transform != fallbackTransform)
                continue;

            float score = 0f;

            if (anchorTransform != null && sceneObject.transform != anchorTransform)
                score = Vector3.SqrMagnitude(sceneObject.transform.position - anchorTransform.position);

            if (sceneObject.gameObject.activeInHierarchy)
                score -= 100000f;

            if (score >= bestScore)
                continue;

            bestScore = score;
            bestTransform = sceneObject.transform;
        }

        return bestTransform != null ? bestTransform : fallbackTransform;
    }

    private bool RequestPlayActorState(OOTechStageActorMotion actor, string stateName, float speed, bool isForceReplay)
    {
        if (actor == null)
            return false;

        OOTechEncounterLoopSpritePlayer loopSpritePlayer = actor.GetComponent<OOTechEncounterLoopSpritePlayer>();

        if (loopSpritePlayer == null)
            loopSpritePlayer = actor.GetComponentInChildren<OOTechEncounterLoopSpritePlayer>(true);

        if (loopSpritePlayer == null)
            loopSpritePlayer = actor.GetComponentInParent<OOTechEncounterLoopSpritePlayer>(true);

        if (!isForceReplay && loopSpritePlayer != null && loopSpritePlayer.RequestPlayLoopState(stateName, speed))
            return true;

        if (!isForceReplay && loopSpritePlayer != null)
        {
            Debug.LogWarning($"[OOTechStage3EncounterController] Encounter loop player exists but did not accept state. actor={actor.gameObject.name}, state={stateName}");
            return false;
        }

        if (!isForceReplay)
            return actor.RequestPlayStableLoopState(stateName, speed);

        actor.RequestForceVisibleRenderer();
        return actor.RequestPlayState(stateName, speed, true);
    }

    private void AddItem(string itemId, int count)
    {
        if (OOTechGameManager.Inst == null || string.IsNullOrEmpty(itemId) || count <= 0)
            return;

        OOTechGameManager.Inst.AddItem(itemId, count);
    }

    private bool TryRemoveItem(string itemId, int count)
    {
        if (OOTechGameManager.Inst == null || string.IsNullOrEmpty(itemId) || count <= 0)
            return false;

        return OOTechGameManager.Inst.RemoveItem(itemId, count);
    }

    private void RefreshHUDInventory()
    {
        if (HUD_Road == null)
            return;

        HUD_Road.RequestRefreshInventoryView();
        HUD_Road.SetInventoryNewBadgeActive(true);
    }

    private Button ResolveNextStageButton()
    {
        Transform[] transformArray = GetComponentsInChildren<Transform>(true);

        foreach (Transform targetTransform in transformArray)
        {
            if (targetTransform != null && targetTransform.name == "Button_NextStage")
                return targetTransform.GetComponent<Button>();
        }

        Button fallbackButton = GetComponentInChildren<Button>(true);

        if (fallbackButton != null)
            return fallbackButton;

        return ResolveOrCreateStage3ClearButton();
    }

    private void SetNextButtonActive(bool isActive)
    {
        if (Button_NextStage == null)
            return;

        if (Object_ClearCanvas == null)
            Object_ClearCanvas = ResolveClearCanvasObject();

        if (Object_ClearCanvas != null)
        {
            Object_ClearCanvas.SetActive(isActive);

            if (isActive)
                ApplyStage3ClearPanelText();
        }

        Button_NextStage.gameObject.SetActive(isActive);
        Button_NextStage.onClick.RemoveListener(OnNextStageButtonClicked);

        if (isActive)
            Button_NextStage.onClick.AddListener(OnNextStageButtonClicked);
    }

    private void OnNextStageButtonClicked()
    {
        string nextGroupName = Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.NextRoadGroupName)
            ? Data_CueSheet.NextRoadGroupName
            : "4th_Road_to_Stage4";

        RequestSwitchGroup(gameObject.name, nextGroupName);
    }

    /// <summary>
    /// Stage3 임무완수 패널에 북쪽 숲 보상 문구를 반영합니다.
    /// Game View에서는 Stage1/Stage2와 같은 "임무 완수 자막판"으로 보입니다.
    /// </summary>
    private void ApplyStage3ClearPanelText()
    {
        if (Object_ClearCanvas == null)
            return;

        RectTransform canvasRect = Object_ClearCanvas.GetComponent<RectTransform>();
        RequestRepairFullScreenRect(canvasRect);

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

        GameObject panelObject = ResolveOrCreateUIObject(Object_ClearCanvas.transform, "Panel_StageClear");
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        RequestRepairFullScreenRect(panelRect);

        Image panelImage = panelObject.GetComponent<Image>();

        if (panelImage == null)
            panelImage = panelObject.AddComponent<Image>();

        panelImage.color = new Color(0f, 0f, 0f, 0f);
        panelImage.raycastTarget = true;

        TextMeshProUGUI titleText = ResolveOrCreateText(panelObject.transform, "Text_ClearTitle", new Vector2(0f, 160f), new Vector2(1200f, 80f));
        titleText.text = _stageClearTitle;
        titleText.fontSize = 44f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;

        TextMeshProUGUI messageText = ResolveOrCreateText(panelObject.transform, "Text_ClearMessage", new Vector2(0f, 58f), new Vector2(1320f, 120f));
        messageText.text = _stageClearMessage;
        messageText.fontSize = 30f;
        messageText.fontStyle = FontStyles.Normal;
        messageText.color = Color.white;
        messageText.textWrappingMode = TextWrappingModes.Normal;

        TextMeshProUGUI labelText = Button_NextStage != null ? Button_NextStage.GetComponentInChildren<TextMeshProUGUI>(true) : null;

        if (labelText != null)
        {
            OOTechTMPFontUtility.ApplyProjectFont(labelText);
            labelText.text = "넘어가기";
            labelText.fontSize = 30f;
            labelText.color = Color.black;
        }
    }

    private GameObject ResolveClearCanvasObject()
    {
        if (Button_NextStage == null)
            return null;

        Canvas canvas = Button_NextStage.GetComponentInParent<Canvas>(true);
        return canvas != null ? canvas.gameObject : Button_NextStage.transform.parent != null ? Button_NextStage.transform.parent.gameObject : null;
    }

    private Button ResolveOrCreateStage3ClearButton()
    {
        GameObject canvasObject = RequestChildObjectByName(transform, _stageClearCanvasName);

        if (canvasObject == null)
        {
            canvasObject = new GameObject(_stageClearCanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
        }

        Object_ClearCanvas = canvasObject;
        RequestRepairFullScreenRect(canvasObject.GetComponent<RectTransform>());

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 5200;

        CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = ResolveOrCreateUIObject(canvasObject.transform, "Panel_StageClear");
        RequestRepairFullScreenRect(panelObject.GetComponent<RectTransform>());

        Image panelImage = panelObject.GetComponent<Image>();

        if (panelImage == null)
            panelImage = panelObject.AddComponent<Image>();

        panelImage.color = new Color(0f, 0f, 0f, 0f);
        panelImage.raycastTarget = true;

        GameObject buttonObject = ResolveOrCreateUIObject(panelObject.transform, "Button_NextStage");
        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
            button = buttonObject.AddComponent<Button>();

        Image buttonImage = buttonObject.GetComponent<Image>();

        if (buttonImage == null)
            buttonImage = buttonObject.AddComponent<Image>();

        buttonImage.color = new Color(1f, 1f, 1f, 0.94f);
        buttonImage.raycastTarget = true;
        RequestSetCenterRect(buttonObject.GetComponent<RectTransform>(), new Vector2(0f, -128f), new Vector2(320f, 84f));

        TextMeshProUGUI labelText = ResolveOrCreateText(buttonObject.transform, "Text_Label", Vector2.zero, new Vector2(300f, 72f));
        labelText.text = "넘어가기";
        labelText.fontSize = 30f;
        labelText.color = Color.black;

        canvasObject.SetActive(false);
        return button;
    }

    private GameObject ResolveOrCreateUIObject(Transform parentTransform, string objectName)
    {
        GameObject targetObject = RequestChildObjectByName(parentTransform, objectName);

        if (targetObject != null)
            return targetObject;

        targetObject = new GameObject(objectName, typeof(RectTransform));
        targetObject.transform.SetParent(parentTransform, false);
        return targetObject;
    }

    private TextMeshProUGUI ResolveOrCreateText(Transform parentTransform, string objectName, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject textObject = ResolveOrCreateUIObject(parentTransform, objectName);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();

        if (text == null)
            text = textObject.AddComponent<TextMeshProUGUI>();

        RectTransform rectTransform = text.GetComponent<RectTransform>();
        RequestSetCenterRect(rectTransform, anchoredPosition, sizeDelta);

        OOTechTMPFontUtility.ApplyProjectFont(text);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private void RequestRepairFullScreenRect(RectTransform rectTransform)
    {
        if (rectTransform == null)
            return;

        rectTransform.localScale = Vector3.one;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
    }

    private void RequestSetCenterRect(RectTransform rectTransform, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        if (rectTransform == null)
            return;

        rectTransform.localScale = Vector3.one;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
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
}

