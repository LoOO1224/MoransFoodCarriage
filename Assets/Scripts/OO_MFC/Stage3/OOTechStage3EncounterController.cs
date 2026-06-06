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
    [Header("Data")]
    [SerializeField] private string _cueSheetDataId = "Stage3_CueSheet_01";

    [Header("Role Components")]
    [SerializeField] private OOTechSceneContext Context_Scene;
    [SerializeField] private OOTechStage3DialogueCue Cue_Dialogue;
    [SerializeField] private OOTechStage2CameraCue Cue_Camera;

    [Header("Group")]
    [SerializeField] private string _cookingGroupName = "CookingGroup";
    [SerializeField] private string _mainMenuGroupName = "MainMenuGroup";

    [Header("Clear UI")]
    [SerializeField] private string _stageClearCanvasName = "Canvas_Stage3Clear";
    [SerializeField] private string _stageClearTitle = "북쪽 숲 임무 완수";
    [SerializeField] private string _stageClearMessage = "산군이 약속의 뜻으로 당근 x10 묶음을 줬습니다. 다음 여정에 나설 준비가 끝났습니다.";

    [Header("Actor State")]
    [SerializeField] private string _mrJaeikThreateningStateName = "Mr.Jaeik_isThreatening";

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

        PrepareEncounterActors();
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

        HUD_Road = HUD_Road != null ? HUD_Road : FindAnyObjectByType<OOTechRoadHUDController>(FindObjectsInactive.Include);
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

        RequestFitCameraToEncounterBackground();
        Actor_Sangun = ResolveActor(Data_CueSheet.SangunRoleId);
        Actor_Moran = ResolveActor(string.IsNullOrEmpty(Data_CueSheet.MoranRoleId) ? "Moran" : Data_CueSheet.MoranRoleId);
        Actor_MrJaeik = ResolveActor(string.IsNullOrEmpty(Data_CueSheet.MrJaeikRoleId) ? "Mr.Jaeik" : Data_CueSheet.MrJaeikRoleId);

        RequestPlayActorState(Actor_Sangun, "Sangun_Idle", 1f, false);
        RequestPlayActorState(Actor_Moran, "Moran_isScared", 0.6f, true);
        RequestPlayActorState(Actor_MrJaeik, _mrJaeikThreateningStateName, 0.6f, true);
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
            return;

        Cue_Camera.RequestSaveAndDisableCameraFollow();
        Cue_Camera.RequestFocusCameraOnBounds(backgroundRenderer.bounds, 1f);
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

        if (selectedIndex == 0)
            yield return PlayDeathRoutine();
    }

    private IEnumerator PlayGiveHoneyCakeChoiceRoutine()
    {
        int selectedIndex = -1;
        yield return Cue_Dialogue.RequestShowChoiceAndWait(Data_CueSheet.GiveHoneyCakeChoiceId, delegate (int index)
        {
            selectedIndex = index;
        });

        if (selectedIndex != 0)
            yield break;

        if (!TryRemoveItem(Data_CueSheet.HoneyKoreanCakeItemId, 1))
        {
            Debug.LogWarning("[OOTechStage3EncounterController] Honey Korean Cake is missing.");
            yield break;
        }

        yield return Cue_Dialogue.RequestShowDialogueAndWait("character_Sangun_04");
        yield return PlayStageClearRoutine();
    }

    private IEnumerator PlayDeathRoutine()
    {
        RequestPlayActorState(Actor_Sangun, "Sangun_isAttacking", Data_CueSheet.AttackingAnimationSpeed > 0f ? Data_CueSheet.AttackingAnimationSpeed : 0.5f, true);
        Debug.LogWarning($"[OOTechStage3EncounterController] {Data_CueSheet.DeathMessage}");
        yield return new WaitForSeconds(1.2f);

        int selectedIndex = -1;
        yield return Cue_Dialogue.RequestShowChoiceAndWait(Data_CueSheet.DeathRetryChoiceId, delegate (int index)
        {
            selectedIndex = index;
        });

        if (selectedIndex == 0)
        {
            _hasInitialSequencePlayed = false;
            RequestStartEncounterCue();
            yield break;
        }

        RequestSwitchGroup(gameObject.name, _mainMenuGroupName);
    }

    private IEnumerator PlayStageClearRoutine()
    {
        _isQuestCleared = true;
        GiveClearReward();
        RefreshHUDInventory();
        RequestUpdateStageQuestAsComplete();
        yield return MoveSangunOffStageRoutine();
        RequestPlayActorState(Actor_Moran, "Moran_Victory", 1f, true);
        RequestPlayActorState(Actor_MrJaeik, "Mr.Jaeik_Victory", 1f, true);
        SetNextButtonActive(true);
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

        string questText = "산군을 위해 꿀떡을 만드세요.";
        OO_StageQuest questData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStageQuestData(Data_CueSheet.StageQuestId) : null;

        if (questData != null && !string.IsNullOrEmpty(questData.Description))
            questText = questData.Description;

        HUD_Road.RequestSetStageQuestMission(questText);
        HUD_Road.SetMissionNewBadgeActive(true);
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
        GameObject cookingGroup = FindSceneObjectByName(_cookingGroupName);
        OOTechGroupNavigationHistory.SetPreviousGroup(_cookingGroupName, gameObject.name);

        if (cookingGroup != null && OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.RegisterUI(_cookingGroupName, cookingGroup);

        RequestSwitchGroup(gameObject.name, _cookingGroupName);
    }

    private void RequestSwitchGroup(string currentGroupName, string nextGroupName)
    {
        if (OOTechUIManager.Inst != null)
        {
            GameObject nextGroup = FindSceneObjectByName(nextGroupName);

            if (nextGroup != null)
                OOTechUIManager.Inst.RegisterUI(nextGroupName, nextGroup);

            OOTechUIManager.Inst.CloseUI(currentGroupName);
            OOTechUIManager.Inst.OpenUI(nextGroupName);
            return;
        }

        GameObject currentGroup = FindSceneObjectByName(currentGroupName);
        GameObject nextGroupObject = FindSceneObjectByName(nextGroupName);

        if (currentGroup != null)
            currentGroup.SetActive(false);

        if (nextGroupObject != null)
            nextGroupObject.SetActive(true);
    }

    private OOTechStageActorMotion ResolveActor(string roleId)
    {
        if (Context_Scene == null || string.IsNullOrEmpty(roleId))
            return null;

        Transform actorTransform = Context_Scene.GetRoleTransform(roleId);
        return actorTransform != null ? actorTransform.GetComponent<OOTechStageActorMotion>() : null;
    }

    private bool RequestPlayActorState(OOTechStageActorMotion actor, string stateName, float speed, bool isForceReplay)
    {
        if (actor == null)
            return false;

        return actor.RequestPlayState(stateName, speed, isForceReplay);
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
        GameObject canvasObject = FindChildByName(transform, _stageClearCanvasName);

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
        GameObject targetObject = FindChildByName(parentTransform, objectName);

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
}
