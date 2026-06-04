// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechRoadHUDUIGroupEditorBuilder.cs
// - 역할: Unity Editor에서 수동으로 실행하는 씬 수리/스캐폴드 도구입니다.
// - 감독 관점: 공연 전에 무대를 정리하는 준비 도구이며, 실제 공연 중 배우가 쓰는 대본은 아닙니다.
// - 유지보수 포인트: 사용자 배치를 덮어쓸 수 있으므로 자동 실행하지 말고, 필요한 메뉴/배치모드에서만 실행합니다.
// =============================================================================
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class OOTechRoadHUDUIGroupEditorBuilder
{
    private const string _sharedHUDSystemGroupName = "HUDSystemGroup";
    private const string _sharedHUDGroupName = "HUDUIGroup";

    private static readonly string[] _hudOwnerGroupNameArray =
    {
        "1st_Road_to_Stage1",
        "2nd_Road_to_Stage2",
        "3rd_Road_to_Stage3",
        "4th_Road_to_Stage4",
        "Final_Road_to_FinalStage",
        "Stage1Group",
        "Stage2Group",
        "Stage3Group",
        "Stage4Group",
        "FinalStageGroup"
    };

    [MenuItem("Tools/OO MFC/Create Editable HUDUIGroup")]
    public static void CreateEditableHUDUIGroupForActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return;

        bool isChanged = RepairSceneHUDUIGroups(scene);

        if (isChanged)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
    }

    /// <summary>
    /// 배치모드에서 공용 HUD와 각 그룹 보조 UI를 생성하고 씬을 저장합니다.
    /// </summary>
    public static void RequestRepairHUDUIGroupsBatch()
    {
        Scene scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/OO_MFC.unity", UnityEditor.SceneManagement.OpenSceneMode.Single);
        bool isChanged = RepairSceneHUDUIGroups(scene);

        if (isChanged)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[OOTechRoadHUDUIGroupEditorBuilder] HUD UI groups repaired.");
    }

    public static bool RepairSceneHUDUIGroups(Scene scene)
    {
        if (!scene.IsValid())
            return false;

        bool isChanged = false;
        isChanged |= EnsureSharedHUDUIGroup(scene);

        foreach (string groupName in _hudOwnerGroupNameArray)
        {
            GameObject ownerGroup = FindSceneObjectByName(scene, groupName);

            if (ownerGroup == null)
                continue;

            isChanged |= EnsureHUDUIGroup(ownerGroup);

            if (IsRoadGroupName(groupName))
                isChanged |= EnsureRoadFadeCanvas(ownerGroup);

            if (IsStageGroupName(groupName))
                isChanged |= EnsureStagePlaceholderUI(ownerGroup);
        }

        isChanged |= EnsureOverlayReturnButton(scene, "WorldMapGroup");
        isChanged |= EnsureOverlayReturnButton(scene, "CookingGroup");
        isChanged |= EnsureOverlayReturnButton(scene, "CodexGroup");
        isChanged |= EnsureWorldMapGuideUI(scene);
        isChanged |= EnsureCookingUIGroup(scene);
        isChanged |= EnsureEpilogueUI(scene);
        return isChanged;
    }

    private static bool EnsureSharedHUDUIGroup(Scene scene)
    {
        bool isChanged = false;
        GameObject hudSystemObject = FindSceneObjectByName(scene, _sharedHUDSystemGroupName);

        if (hudSystemObject == null)
        {
            hudSystemObject = new GameObject(_sharedHUDSystemGroupName);
            SceneManager.MoveGameObjectToScene(hudSystemObject, scene);
            isChanged = true;
        }

        if (!hudSystemObject.activeSelf)
        {
            hudSystemObject.SetActive(true);
            isChanged = true;
        }

        GameObject hudObject = FindDirectChild(hudSystemObject.transform, _sharedHUDGroupName);

        if (hudObject == null)
        {
            hudObject = new GameObject(_sharedHUDGroupName, typeof(RectTransform));
            hudObject.transform.SetParent(hudSystemObject.transform, false);
            isChanged = true;
        }

        if (hudObject.activeSelf)
        {
            hudObject.SetActive(false);
            isChanged = true;
        }

        isChanged |= ConfigureHUDUIGroup(hudObject);
        return isChanged;
    }

    private static bool EnsureHUDUIGroup(GameObject ownerGroup)
    {
        bool isChanged = false;
        GameObject hudObject = FindDirectChild(ownerGroup.transform, "HUDUIGroup");

        if (hudObject == null)
        {
            hudObject = FindDirectChild(ownerGroup.transform, "RoadHUDCanvas");

            if (hudObject != null)
            {
                hudObject.name = "HUDUIGroup";
                isChanged = true;
            }
        }

        if (hudObject == null)
        {
            hudObject = new GameObject("HUDUIGroup", typeof(RectTransform));
            hudObject.transform.SetParent(ownerGroup.transform, false);
            isChanged = true;
        }

        isChanged |= ConfigureHUDUIGroup(hudObject);
        return isChanged;
    }

    private static bool ConfigureHUDUIGroup(GameObject hudObject)
    {
        bool isChanged = false;
        RectTransform hudRect = hudObject.transform as RectTransform;

        if (hudRect == null)
        {
            hudRect = hudObject.AddComponent<RectTransform>();
            isChanged = true;
        }

        StretchToParent(hudRect);

        Canvas canvas = hudObject.GetComponent<Canvas>();

        if (canvas == null)
        {
            canvas = hudObject.AddComponent<Canvas>();
            isChanged = true;
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1200;

        CanvasScaler canvasScaler = hudObject.GetComponent<CanvasScaler>();

        if (canvasScaler == null)
        {
            canvasScaler = hudObject.AddComponent<CanvasScaler>();
            isChanged = true;
        }

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;

        if (hudObject.GetComponent<GraphicRaycaster>() == null)
        {
            hudObject.AddComponent<GraphicRaycaster>();
            isChanged = true;
        }

        CanvasGroup canvasGroup = hudObject.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = hudObject.AddComponent<CanvasGroup>();
            isChanged = true;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        OOTechRoadHUDView hudView = hudObject.GetComponent<OOTechRoadHUDView>();

        if (hudView == null)
        {
            hudView = hudObject.AddComponent<OOTechRoadHUDView>();
            isChanged = true;
        }

        isChanged |= EnsureHUDButtonSet(hudObject.transform);
        isChanged |= EnsureInventoryPanel(hudObject.transform);
        isChanged |= EnsureMissionPanel(hudObject.transform);
        isChanged |= EnsureHUDGuideOverlay(hudObject.transform);
        isChanged |= EnsureMainMenuConfirmPopup(hudObject.transform);
        hudView.ResolveReferences();
        EditorUtility.SetDirty(hudView);
        return isChanged;
    }

    private static bool IsRoadGroupName(string groupName)
    {
        return groupName == "1st_Road_to_Stage1" ||
               groupName == "2nd_Road_to_Stage2" ||
               groupName == "3rd_Road_to_Stage3" ||
               groupName == "4th_Road_to_Stage4" ||
               groupName == "Final_Road_to_FinalStage";
    }

    private static bool IsStageGroupName(string groupName)
    {
        return groupName == "Stage1Group" ||
               groupName == "Stage2Group" ||
               groupName == "Stage3Group" ||
               groupName == "Stage4Group" ||
               groupName == "FinalStageGroup";
    }

    private static bool EnsureRoadFadeCanvas(GameObject roadGroup)
    {
        bool isChanged = false;
        bool isCanvasCreated = FindDirectChild(roadGroup.transform, "RoadMapFadeCanvas") == null;
        GameObject canvasObject = EnsureUIObject(roadGroup.transform, "RoadMapFadeCanvas", ref isChanged);
        EnsureCanvasComponents(canvasObject, 5000, ref isChanged);

        if (isCanvasCreated)
        {
            StretchToParent(canvasObject.transform as RectTransform);
            canvasObject.SetActive(false);
        }

        bool isImageCreated = FindDirectChild(canvasObject.transform, "Image_FadeOverlay") == null;
        GameObject imageObject = EnsurePanel(canvasObject.transform, "Image_FadeOverlay", Color.black, ref isChanged);
        RectTransform imageRect = imageObject.transform as RectTransform;

        if (isImageCreated)
        {
            StretchToParent(imageRect);
            Image image = imageObject.GetComponent<Image>();

            if (image != null)
            {
                Color color = Color.black;
                color.a = 0f;
                image.color = color;
                image.raycastTarget = false;
            }
        }

        return isChanged;
    }

    private static bool EnsureStagePlaceholderUI(GameObject stageGroup)
    {
        bool isChanged = false;
        bool isCanvasCreated = FindDirectChild(stageGroup.transform, "Canvas_StagePlaceholder") == null;
        GameObject canvasObject = EnsureUIObject(stageGroup.transform, "Canvas_StagePlaceholder", ref isChanged);
        EnsureCanvasComponents(canvasObject, 900, ref isChanged);

        if (isCanvasCreated)
            StretchToParent(canvasObject.transform as RectTransform);

        bool isBackgroundCreated = FindDirectChild(canvasObject.transform, "Image_WhiteBackground") == null;
        GameObject backgroundObject = EnsurePanel(canvasObject.transform, "Image_WhiteBackground", Color.white, ref isChanged);

        if (isBackgroundCreated)
        {
            StretchToParent(backgroundObject.transform as RectTransform);
            Image backgroundImage = backgroundObject.GetComponent<Image>();

            if (backgroundImage != null)
                backgroundImage.raycastTarget = false;
        }

        bool isButtonCreated = FindDirectChild(canvasObject.transform, "Button_NextStage") == null;
        Button nextButton = EnsureButton(canvasObject.transform, "Button_NextStage", "넘어가기", new Vector2(-36f, -36f), new Vector2(260f, 72f), ref isChanged);
        RectTransform buttonRect = nextButton.transform as RectTransform;

        if (isButtonCreated)
        {
            buttonRect.anchorMin = new Vector2(1f, 1f);
            buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.pivot = new Vector2(1f, 1f);
            buttonRect.anchoredPosition = new Vector2(-36f, -36f);
            buttonRect.sizeDelta = new Vector2(260f, 72f);
        }

        return isChanged;
    }

    private static bool EnsureOverlayReturnButton(Scene scene, string groupName)
    {
        GameObject groupObject = FindSceneObjectByName(scene, groupName);

        if (groupObject == null)
            return false;

        bool isChanged = false;
        bool isReturnCanvasCreated = FindDirectChild(groupObject.transform, "Canvas_RuntimeReturn") == null;
        GameObject canvasObject = EnsureUIObject(groupObject.transform, "Canvas_RuntimeReturn", ref isChanged);
        EnsureCanvasComponents(canvasObject, 1700, ref isChanged);

        if (isReturnCanvasCreated)
            StretchToParent(canvasObject.transform as RectTransform);

        bool isButtonCreated = FindDirectChild(canvasObject.transform, "Button_RuntimeReturn") == null;
        Button returnButton = EnsureButton(canvasObject.transform, "Button_RuntimeReturn", "돌아가기", Vector2.zero, new Vector2(220f, 64f), ref isChanged);
        RectTransform buttonRect = returnButton.transform as RectTransform;

        if (isButtonCreated)
        {
            buttonRect.anchorMin = new Vector2(1f, 1f);
            buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.pivot = new Vector2(1f, 1f);
            buttonRect.anchoredPosition = new Vector2(-36f, -36f);
            buttonRect.sizeDelta = new Vector2(220f, 64f);
        }

        return isChanged;
    }

    private static bool EnsureWorldMapGuideUI(Scene scene)
    {
        GameObject worldMapGroup = FindSceneObjectByName(scene, "WorldMapGroup");

        if (worldMapGroup == null)
            return false;

        bool isChanged = false;

        if (worldMapGroup.GetComponent<OOTechWorldMapOverlayController>() == null)
        {
            worldMapGroup.AddComponent<OOTechWorldMapOverlayController>();
            isChanged = true;
        }

        bool isGuideRootCreated = FindDirectChild(worldMapGroup.transform, "WorldMapGuideUIGroup") == null;
        GameObject guideRoot = EnsureUIObject(worldMapGroup.transform, "WorldMapGuideUIGroup", ref isChanged);
        EnsureCanvasComponents(guideRoot, 1680, ref isChanged);

        if (isGuideRootCreated)
        {
            StretchToParent(guideRoot.transform as RectTransform);
            guideRoot.SetActive(false);
        }

        OOTechWorldMapOverlayView guideView = guideRoot.GetComponent<OOTechWorldMapOverlayView>();

        if (guideView == null)
        {
            guideView = guideRoot.AddComponent<OOTechWorldMapOverlayView>();
            isChanged = true;
        }

        bool isBubbleCreated = FindDirectChild(guideRoot.transform, "Panel_WorldMapDragGuide") == null;
        GameObject bubbleObject = EnsurePanel(guideRoot.transform, "Panel_WorldMapDragGuide", new Color(1f, 1f, 1f, 0.96f), ref isChanged);
        RectTransform bubbleRect = bubbleObject.transform as RectTransform;

        if (isBubbleCreated)
        {
            bubbleRect.anchorMin = new Vector2(0f, 0f);
            bubbleRect.anchorMax = new Vector2(0f, 0f);
            bubbleRect.pivot = new Vector2(0f, 0f);
            bubbleRect.anchoredPosition = new Vector2(36f, 36f);
            bubbleRect.sizeDelta = new Vector2(620f, 158f);
        }

        EnsureWorldMapGuideChildren(bubbleObject.transform, ref isChanged);
        guideView.ResolveReferences();
        EditorUtility.SetDirty(guideView);
        return isChanged;
    }

    private static void EnsureWorldMapGuideChildren(Transform bubbleRoot, ref bool isChanged)
    {
        bool isTitleCreated = FindDirectChild(bubbleRoot, "Text_GuideTitle") == null;
        TextMeshProUGUI titleText = EnsureText(bubbleRoot, "Text_GuideTitle", "월드맵", 28, FontStyles.Bold, Color.black, ref isChanged);

        if (isTitleCreated)
        {
            RectTransform titleRect = titleText.transform as RectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
            titleRect.sizeDelta = new Vector2(-36f, 40f);
        }

        bool isBodyCreated = FindDirectChild(bubbleRoot, "Text_GuideBody") == null;
        TextMeshProUGUI bodyText = EnsureText(bubbleRoot, "Text_GuideBody", "좌클릭을 누른 채 맵을 움직여 볼 수 있습니다.", 23, FontStyles.Normal, Color.black, ref isChanged);

        if (isBodyCreated)
        {
            RectTransform bodyRect = bodyText.transform as RectTransform;
            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.offsetMin = new Vector2(28f, 58f);
            bodyRect.offsetMax = new Vector2(-138f, -52f);
            bodyText.alignment = TextAlignmentOptions.TopLeft;
        }

        EnsureButton(bubbleRoot, "Button_GuideClose", "확인", new Vector2(-24f, 22f), new Vector2(108f, 46f), ref isChanged);
    }

    private static bool EnsureCookingUIGroup(Scene scene)
    {
        GameObject cookingGroup = FindSceneObjectByName(scene, "CookingGroup");

        if (cookingGroup == null)
            return false;

        bool isChanged = false;
        isChanged |= NormalizeCookingGroupTransform(cookingGroup);

        if (cookingGroup.GetComponent<OOTechCookingGroupController>() == null)
        {
            cookingGroup.AddComponent<OOTechCookingGroupController>();
            isChanged = true;
        }

        bool isCookingRootCreated = FindDirectChild(cookingGroup.transform, "CookingUIGroup") == null;
        GameObject cookingRoot = EnsureUIObject(cookingGroup.transform, "CookingUIGroup", ref isChanged);
        EnsureCanvasComponents(cookingRoot, 1260, ref isChanged);
        EnsureCookingCanvasRoot(cookingRoot.transform as RectTransform, ref isChanged);

        if (isCookingRootCreated)
            StretchToParent(cookingRoot.transform as RectTransform);

        OOTechCookingGroupView cookingView = cookingRoot.GetComponent<OOTechCookingGroupView>();

        if (cookingView == null)
        {
            cookingView = cookingRoot.AddComponent<OOTechCookingGroupView>();
            isChanged = true;
        }

        EnsureCookingCauldronDropArea(cookingRoot.transform, ref isChanged);
        EnsureCookingGuide(cookingRoot.transform, ref isChanged);
        EnsureCookingStatus(cookingRoot.transform, ref isChanged);
        EnsureCookingDragGhostTemplate(cookingRoot.transform, ref isChanged);
        cookingView.ResolveReferences();
        EditorUtility.SetDirty(cookingView);
        return isChanged;
    }

    private static bool NormalizeCookingGroupTransform(GameObject cookingGroup)
    {
        if (cookingGroup == null)
            return false;

        Transform groupTransform = cookingGroup.transform;
        Vector3 groupOffset = groupTransform.localPosition;
        bool isChanged = false;

        if (groupOffset.sqrMagnitude > 0.01f)
        {
            for (int index = 0; index < groupTransform.childCount; index++)
            {
                Transform childTransform = groupTransform.GetChild(index);

                if (childTransform is RectTransform)
                    continue;

                childTransform.localPosition += groupOffset;
            }

            groupTransform.localPosition = Vector3.zero;
            isChanged = true;
        }

        if (groupTransform.localRotation != Quaternion.identity)
        {
            groupTransform.localRotation = Quaternion.identity;
            isChanged = true;
        }

        if (groupTransform.localScale != Vector3.one)
        {
            groupTransform.localScale = Vector3.one;
            isChanged = true;
        }

        return isChanged;
    }

    private static void EnsureCookingCanvasRoot(RectTransform cookingRootRect, ref bool isChanged)
    {
        if (cookingRootRect == null)
            return;

        if (cookingRootRect.localScale != Vector3.one)
        {
            cookingRootRect.localScale = Vector3.one;
            isChanged = true;
        }

        if (cookingRootRect.anchorMin != Vector2.zero || cookingRootRect.anchorMax != Vector2.one)
        {
            cookingRootRect.anchorMin = Vector2.zero;
            cookingRootRect.anchorMax = Vector2.one;
            isChanged = true;
        }

        if (cookingRootRect.pivot != new Vector2(0.5f, 0.5f))
        {
            cookingRootRect.pivot = new Vector2(0.5f, 0.5f);
            isChanged = true;
        }

        if (cookingRootRect.offsetMin != Vector2.zero || cookingRootRect.offsetMax != Vector2.zero)
        {
            cookingRootRect.offsetMin = Vector2.zero;
            cookingRootRect.offsetMax = Vector2.zero;
            isChanged = true;
        }
    }

    private static bool EnsureEpilogueUI(Scene scene)
    {
        GameObject epilogueGroup = FindSceneObjectByName(scene, "EpilogueGroup");

        if (epilogueGroup == null)
            return false;

        bool isChanged = false;
        bool isCanvasCreated = FindDirectChild(epilogueGroup.transform, "Canvas_EpiloguePlaceholder") == null;
        GameObject canvasObject = EnsureUIObject(epilogueGroup.transform, "Canvas_EpiloguePlaceholder", ref isChanged);
        EnsureCanvasComponents(canvasObject, 950, ref isChanged);

        if (isCanvasCreated)
            StretchToParent(canvasObject.transform as RectTransform);

        bool isBackgroundCreated = FindDirectChild(canvasObject.transform, "Image_WhiteBackground") == null;
        GameObject backgroundObject = EnsurePanel(canvasObject.transform, "Image_WhiteBackground", Color.white, ref isChanged);

        if (isBackgroundCreated)
        {
            StretchToParent(backgroundObject.transform as RectTransform);
            Image backgroundImage = backgroundObject.GetComponent<Image>();

            if (backgroundImage != null)
                backgroundImage.raycastTarget = false;
        }

        bool isCreditsButtonCreated = FindDirectChild(canvasObject.transform, "Button_ShowEndingCredits") == null;
        Button creditsButton = EnsureButton(canvasObject.transform, "Button_ShowEndingCredits", "엔딩 크레딧 보기", new Vector2(-36f, -36f), new Vector2(320f, 76f), ref isChanged);

        if (isCreditsButtonCreated)
            ApplyTopRightButtonLayout(creditsButton.transform as RectTransform, new Vector2(320f, 76f));

        bool isCreditsPanelCreated = FindDirectChild(canvasObject.transform, "Panel_EndingCredits") == null;
        GameObject creditsPanel = EnsurePanel(canvasObject.transform, "Panel_EndingCredits", new Color(0f, 0f, 0f, 0.88f), ref isChanged);

        if (isCreditsPanelCreated)
        {
            StretchToParent(creditsPanel.transform as RectTransform);
            creditsPanel.SetActive(false);
        }

        bool isCreditsTextCreated = FindDirectChild(creditsPanel.transform, "Text_EndingCredits") == null;
        TextMeshProUGUI creditsText = EnsureText(creditsPanel.transform, "Text_EndingCredits", "엔딩 크레딧", 46, FontStyles.Bold, Color.white, ref isChanged);

        if (isCreditsTextCreated)
        {
            RectTransform creditsTextRect = creditsText.transform as RectTransform;
            creditsTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            creditsTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            creditsTextRect.pivot = new Vector2(0.5f, 0.5f);
            creditsTextRect.anchoredPosition = new Vector2(0f, 90f);
            creditsTextRect.sizeDelta = new Vector2(700f, 120f);
        }

        bool isReturnButtonCreated = FindDirectChild(creditsPanel.transform, "Button_ReturnMainMenu") == null;
        Button returnButton = EnsureButton(creditsPanel.transform, "Button_ReturnMainMenu", "메인메뉴 돌아가기", new Vector2(-36f, -36f), new Vector2(320f, 76f), ref isChanged);

        if (isReturnButtonCreated)
            ApplyTopRightButtonLayout(returnButton.transform as RectTransform, new Vector2(320f, 76f));

        return isChanged;
    }

    private static void ApplyTopRightButtonLayout(RectTransform buttonRect, Vector2 sizeDelta)
    {
        if (buttonRect == null)
            return;

        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.anchoredPosition = new Vector2(-36f, -36f);
        buttonRect.sizeDelta = sizeDelta;
    }

    private static void EnsureCookingCauldronDropArea(Transform cookingRoot, ref bool isChanged)
    {
        bool isDropAreaCreated = FindDirectChild(cookingRoot, "Rect_CauldronDropArea") == null;
        GameObject dropAreaObject = EnsurePanel(cookingRoot, "Rect_CauldronDropArea", new Color(1f, 1f, 1f, 0f), ref isChanged);
        RectTransform dropAreaRect = dropAreaObject.transform as RectTransform;

        if (isDropAreaCreated)
        {
            dropAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
            dropAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
            dropAreaRect.pivot = new Vector2(0.5f, 0.5f);
            dropAreaRect.sizeDelta = new Vector2(360f, 260f);
        }

        Image dropAreaImage = dropAreaObject.GetComponent<Image>();

        if (dropAreaImage != null)
            dropAreaImage.raycastTarget = false;

        bool isPotTextCreated = FindDirectChild(dropAreaObject.transform, "Text_PotContent") == null;
        TextMeshProUGUI potText = EnsureText(dropAreaObject.transform, "Text_PotContent", "비어 있음", 24, FontStyles.Bold, Color.white, ref isChanged);

        if (isPotTextCreated)
            StretchToParent(potText.transform as RectTransform);
    }

    private static void EnsureCookingGuide(Transform cookingRoot, ref bool isChanged)
    {
        bool isGuideCreated = FindDirectChild(cookingRoot, "Panel_CauldronGuide") == null;
        GameObject guideObject = EnsurePanel(cookingRoot, "Panel_CauldronGuide", new Color(1f, 1f, 1f, 0.95f), ref isChanged);
        RectTransform guideRect = guideObject.transform as RectTransform;

        if (isGuideCreated)
        {
            guideRect.anchorMin = new Vector2(0.5f, 0.5f);
            guideRect.anchorMax = new Vector2(0.5f, 0.5f);
            guideRect.pivot = new Vector2(0f, 0.5f);
            guideRect.anchoredPosition = new Vector2(280f, 170f);
            guideRect.sizeDelta = new Vector2(520f, 210f);
            guideObject.SetActive(false);
        }

        EnsureGuideTextChildren(guideObject.transform, ref isChanged);
        EnsureButton(guideObject.transform, "Button_GuideConfirm", "확인", new Vector2(0f, 28f), new Vector2(150f, 48f), ref isChanged);
        EnsureCookingArrow(cookingRoot, "Text_InventoryGuideArrow", new Vector2(270f, 250f), ref isChanged);
        EnsureCookingArrow(cookingRoot, "Text_CauldronGuideArrow", new Vector2(0f, 210f), ref isChanged);
    }

    private static void EnsureCookingArrow(Transform cookingRoot, string objectName, Vector2 anchoredPosition, ref bool isChanged)
    {
        bool isArrowCreated = FindDirectChild(cookingRoot, objectName) == null;
        TextMeshProUGUI arrowText = EnsureText(cookingRoot, objectName, "▼", 64, FontStyles.Bold, new Color(1f, 0.88f, 0.05f, 1f), ref isChanged);
        RectTransform arrowRect = arrowText.transform as RectTransform;

        if (isArrowCreated)
        {
            arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
            arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.anchoredPosition = anchoredPosition;
            arrowRect.sizeDelta = new Vector2(120f, 90f);
            arrowText.gameObject.SetActive(false);
        }
    }

    private static void EnsureCookingStatus(Transform cookingRoot, ref bool isChanged)
    {
        bool isStatusCreated = FindDirectChild(cookingRoot, "Panel_CookingStatus") == null;
        GameObject statusObject = EnsurePanel(cookingRoot, "Panel_CookingStatus", new Color(1f, 1f, 1f, 0.92f), ref isChanged);
        RectTransform statusRect = statusObject.transform as RectTransform;

        if (isStatusCreated)
        {
            statusRect.anchorMin = new Vector2(0.5f, 0f);
            statusRect.anchorMax = new Vector2(0.5f, 0f);
            statusRect.pivot = new Vector2(0.5f, 0f);
            statusRect.anchoredPosition = new Vector2(0f, 44f);
            statusRect.sizeDelta = new Vector2(880f, 86f);
        }

        bool isStatusTextCreated = FindDirectChild(statusObject.transform, "Text_Status") == null;
        TextMeshProUGUI statusText = EnsureText(statusObject.transform, "Text_Status", "재료를 가마솥으로 끌어다 놓으세요.", 28, FontStyles.Bold, Color.black, ref isChanged);

        if (isStatusTextCreated)
            StretchToParent(statusText.transform as RectTransform);
    }

    private static void EnsureCookingDragGhostTemplate(Transform cookingRoot, ref bool isChanged)
    {
        bool isGhostCreated = FindDirectChild(cookingRoot, "Slot_DragGhostTemplate") == null;
        GameObject ghostObject = EnsurePanel(cookingRoot, "Slot_DragGhostTemplate", new Color(1f, 0.92f, 0.35f, 0.92f), ref isChanged);
        RectTransform ghostRect = ghostObject.transform as RectTransform;

        if (isGhostCreated)
        {
            ghostRect.anchorMin = new Vector2(0.5f, 0.5f);
            ghostRect.anchorMax = new Vector2(0.5f, 0.5f);
            ghostRect.pivot = new Vector2(0.5f, 0.5f);
            ghostRect.sizeDelta = new Vector2(180f, 54f);
            ghostObject.SetActive(false);
        }

        if (ghostObject.GetComponent<CanvasGroup>() == null)
        {
            CanvasGroup canvasGroup = ghostObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            isChanged = true;
        }

        bool isIconCreated = FindDirectChild(ghostObject.transform, "Image_ItemIcon") == null;
        GameObject iconObject = EnsurePanel(ghostObject.transform, "Image_ItemIcon", new Color(1f, 1f, 1f, 0.18f), ref isChanged);
        RectTransform iconRect = iconObject.transform as RectTransform;

        if (isIconCreated)
        {
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(10f, 0f);
            iconRect.sizeDelta = new Vector2(38f, 38f);
        }

        Image iconImage = iconObject.GetComponent<Image>();

        if (iconImage != null)
            iconImage.raycastTarget = false;

        bool isLabelCreated = FindDirectChild(ghostObject.transform, "Text_Label") == null;
        TextMeshProUGUI labelText = EnsureText(ghostObject.transform, "Text_Label", "재료", 22, FontStyles.Bold, Color.black, ref isChanged);

        if (isLabelCreated || isIconCreated)
        {
            RectTransform labelRect = labelText.transform as RectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(58f, 0f);
            labelRect.offsetMax = new Vector2(-12f, 0f);
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
        }
    }

    private static void EnsureCanvasComponents(GameObject canvasObject, int sortingOrder, ref bool isChanged)
    {
        Canvas canvas = canvasObject.GetComponent<Canvas>();

        if (canvas == null)
        {
            canvas = canvasObject.AddComponent<Canvas>();
            isChanged = true;
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();

        if (canvasScaler == null)
        {
            canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            isChanged = true;
        }

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObject.AddComponent<GraphicRaycaster>();
            isChanged = true;
        }
    }

    private static bool EnsureHUDButtonSet(Transform hudRoot)
    {
        bool isChanged = false;
        EnsureButton(hudRoot, "Button_MainMenu", "메인 메뉴", new Vector2(-835f, 75f), new Vector2(170f, 58f), ref isChanged);
        EnsureButton(hudRoot, "Button_Inventory", "인벤토리", new Vector2(-520f, 75f), new Vector2(220f, 66f), ref isChanged);
        EnsureButton(hudRoot, "Button_Codex", "도감", new Vector2(-260f, 75f), new Vector2(220f, 66f), ref isChanged);
        EnsureButton(hudRoot, "Button_Mission", "임무", new Vector2(0f, 75f), new Vector2(220f, 66f), ref isChanged);
        EnsureButton(hudRoot, "Button_Cooking", "요리하기", new Vector2(260f, 75f), new Vector2(220f, 66f), ref isChanged);
        EnsureButton(hudRoot, "Button_WorldMap", "월드맵", new Vector2(520f, 75f), new Vector2(220f, 66f), ref isChanged);

        EnsureBadge(hudRoot, "NewBadge_Inventory", new Vector2(-415f, 116f), ref isChanged);
        EnsureBadge(hudRoot, "NewBadge_Codex", new Vector2(-155f, 116f), ref isChanged);
        EnsureBadge(hudRoot, "NewBadge_Mission", new Vector2(105f, 116f), ref isChanged);
        return isChanged;
    }

    private static bool EnsureInventoryPanel(Transform hudRoot)
    {
        bool isChanged = false;
        bool isInventoryPanelCreated = FindDirectChild(hudRoot, "Panel_Inventory") == null;
        GameObject inventoryPanel = EnsurePanel(hudRoot, "Panel_Inventory", new Color(0.04f, 0.04f, 0.04f, 0.94f), ref isChanged);
        RectTransform panelRect = inventoryPanel.transform as RectTransform;

        if (isInventoryPanelCreated)
        {
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0f, 0f);
            panelRect.pivot = new Vector2(0f, 0f);
            panelRect.anchoredPosition = new Vector2(28f, 170f);
            panelRect.sizeDelta = new Vector2(460f, 330f);
            inventoryPanel.SetActive(false);
        }

        bool isTitleCreated = FindDirectChild(inventoryPanel.transform, "Text_InventoryTitle") == null;
        TextMeshProUGUI titleText = EnsureText(inventoryPanel.transform, "Text_InventoryTitle", "인벤토리", 28, FontStyles.Bold, Color.white, ref isChanged);
        RectTransform titleRect = titleText.transform as RectTransform;

        if (isTitleCreated)
        {
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -16f);
            titleRect.sizeDelta = new Vector2(-32f, 44f);
        }

        bool isScrollCreated = FindDirectChild(inventoryPanel.transform, "Scroll_InventorySlots") == null;
        GameObject scrollObject = EnsureUIObject(inventoryPanel.transform, "Scroll_InventorySlots", ref isChanged);
        RectTransform scrollRectTransform = scrollObject.transform as RectTransform;

        if (isScrollCreated)
        {
            scrollRectTransform.anchorMin = new Vector2(0f, 0f);
            scrollRectTransform.anchorMax = new Vector2(1f, 1f);
            scrollRectTransform.offsetMin = new Vector2(18f, 18f);
            scrollRectTransform.offsetMax = new Vector2(-18f, -72f);
        }

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();

        if (scrollRect == null)
        {
            scrollRect = scrollObject.AddComponent<ScrollRect>();
            isChanged = true;
        }

        scrollRect.horizontal = false;

        bool isViewportCreated = FindDirectChild(scrollObject.transform, "Viewport") == null;
        GameObject viewportObject = EnsurePanel(scrollObject.transform, "Viewport", new Color(1f, 1f, 1f, 0.04f), ref isChanged);
        RectTransform viewportRect = viewportObject.transform as RectTransform;

        if (isViewportCreated)
            StretchToParent(viewportRect);

        Mask mask = viewportObject.GetComponent<Mask>();

        if (mask == null)
        {
            mask = viewportObject.AddComponent<Mask>();
            isChanged = true;
        }

        mask.showMaskGraphic = false;

        bool isContentCreated = FindDirectChild(viewportObject.transform, "Content") == null;
        GameObject contentObject = EnsureUIObject(viewportObject.transform, "Content", ref isChanged);
        RectTransform contentRect = contentObject.transform as RectTransform;

        if (isContentCreated)
        {
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 260f);
        }

        VerticalLayoutGroup layoutGroup = contentObject.GetComponent<VerticalLayoutGroup>();

        if (layoutGroup == null)
        {
            layoutGroup = contentObject.AddComponent<VerticalLayoutGroup>();
            isChanged = true;
        }

        if (isContentCreated)
        {
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.spacing = 10f;
        }

        isChanged |= EnsureInventorySlotTemplate(contentObject.transform);

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        return isChanged;
    }

    private static bool EnsureInventorySlotTemplate(Transform contentRoot)
    {
        bool isChanged = false;
        bool isTemplateCreated = FindDirectChild(contentRoot, "Slot_InventoryItemTemplate") == null;
        GameObject slotObject = EnsurePanel(contentRoot, "Slot_InventoryItemTemplate", new Color(0.18f, 0.18f, 0.18f, 0.96f), ref isChanged);

        if (isTemplateCreated)
        {
            RectTransform slotRect = slotObject.transform as RectTransform;
            slotRect.sizeDelta = new Vector2(0f, 58f);
            slotObject.SetActive(false);
        }

        OOTechInventorySlotView slotView = slotObject.GetComponent<OOTechInventorySlotView>();

        if (slotView == null)
        {
            slotView = slotObject.AddComponent<OOTechInventorySlotView>();
            isChanged = true;
        }

        if (slotObject.GetComponent<OOTechCookingIngredientDragItem>() == null)
        {
            slotObject.AddComponent<OOTechCookingIngredientDragItem>();
            isChanged = true;
        }

        bool isIconCreated = FindDirectChild(slotObject.transform, "Image_ItemIcon") == null;
        GameObject iconObject = EnsurePanel(slotObject.transform, "Image_ItemIcon", new Color(1f, 1f, 1f, 0.16f), ref isChanged);
        RectTransform iconRect = iconObject.transform as RectTransform;

        if (isIconCreated)
        {
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(12f, 0f);
            iconRect.sizeDelta = new Vector2(42f, 42f);
        }

        Image iconImage = iconObject.GetComponent<Image>();

        if (iconImage != null)
            iconImage.raycastTarget = false;

        bool isLabelCreated = FindDirectChild(slotObject.transform, "Text_Label") == null;
        TextMeshProUGUI labelText = EnsureText(slotObject.transform, "Text_Label", "아이템 x0", 22, FontStyles.Bold, Color.white, ref isChanged);

        if (isLabelCreated || isIconCreated)
        {
            RectTransform labelRect = labelText.transform as RectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(70f, 0f);
            labelRect.offsetMax = new Vector2(-14f, 0f);
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
        }

        slotView.ResolveReferences();
        EditorUtility.SetDirty(slotView);
        return isChanged;
    }

    private static bool EnsureMissionPanel(Transform hudRoot)
    {
        bool isChanged = false;
        bool isMissionPanelCreated = FindDirectChild(hudRoot, "Panel_Mission") == null;
        GameObject missionPanel = EnsurePanel(hudRoot, "Panel_Mission", new Color(0.04f, 0.04f, 0.04f, 0.94f), ref isChanged);
        RectTransform panelRect = missionPanel.transform as RectTransform;

        if (isMissionPanelCreated || IsLegacyCenteredMissionPanelLayout(panelRect))
        {
            ApplyMissionPanelBottomRightLayout(panelRect);
            isChanged = true;
        }

        if (isMissionPanelCreated)
            missionPanel.SetActive(false);

        bool isMissionTextCreated = FindDirectChild(missionPanel.transform, "Text_MissionContent") == null;
        TextMeshProUGUI missionText = EnsureText(missionPanel.transform, "Text_MissionContent", "현재 임무\n○ 배고픈 모란과 동료들을 위해 요리하세요.\n○ 동쪽의 마을로 가시오", 24, FontStyles.Normal, Color.white, ref isChanged);
        RectTransform textRect = missionText.transform as RectTransform;

        if (isMissionTextCreated)
        {
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(24f, 24f);
            textRect.offsetMax = new Vector2(-24f, -24f);
            missionText.alignment = TextAlignmentOptions.MidlineLeft;
        }

        return isChanged;
    }

    private static bool IsLegacyCenteredMissionPanelLayout(RectTransform panelRect)
    {
        if (panelRect == null)
            return false;

        bool isCenterBottomAnchor = Mathf.Abs(panelRect.anchorMin.x - 0.5f) <= 0.01f
            && Mathf.Abs(panelRect.anchorMax.x - 0.5f) <= 0.01f
            && Mathf.Abs(panelRect.anchorMin.y) <= 0.01f
            && Mathf.Abs(panelRect.anchorMax.y) <= 0.01f;
        bool isCenterPosition = Mathf.Abs(panelRect.anchoredPosition.x) <= 2f;
        return isCenterBottomAnchor && isCenterPosition;
    }

    private static void ApplyMissionPanelBottomRightLayout(RectTransform panelRect)
    {
        if (panelRect == null)
            return;

        panelRect.anchorMin = new Vector2(1f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0f);
        panelRect.pivot = new Vector2(1f, 0f);
        panelRect.anchoredPosition = new Vector2(-36f, 132f);
        panelRect.sizeDelta = new Vector2(560f, 220f);
    }

    private static bool EnsureHUDGuideOverlay(Transform hudRoot)
    {
        bool isChanged = false;
        bool isOverlayCreated = FindDirectChild(hudRoot, "Panel_HUDFocusGuide") == null;
        GameObject overlayObject = EnsureUIObject(hudRoot, "Panel_HUDFocusGuide", ref isChanged);
        RectTransform overlayRect = overlayObject.transform as RectTransform;

        if (isOverlayCreated)
        {
            StretchToParent(overlayRect);
            overlayObject.SetActive(false);
        }

        bool isArrowCreated = FindDirectChild(overlayObject.transform, "Text_FocusArrow") == null;
        TextMeshProUGUI arrowText = EnsureText(overlayObject.transform, "Text_FocusArrow", "▼", 64, FontStyles.Bold, new Color(1f, 0.86f, 0.1f, 1f), ref isChanged);
        RectTransform arrowRect = arrowText.transform as RectTransform;

        if (isArrowCreated)
        {
            arrowRect.anchorMin = new Vector2(0.5f, 0f);
            arrowRect.anchorMax = new Vector2(0.5f, 0f);
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.sizeDelta = new Vector2(90f, 72f);
        }

        bool isPanelCreated = FindDirectChild(overlayObject.transform, "Panel_GuideText") == null;
        GameObject panelObject = EnsurePanel(overlayObject.transform, "Panel_GuideText", new Color(1f, 1f, 1f, 0.96f), ref isChanged);
        RectTransform panelRect = panelObject.transform as RectTransform;

        if (isPanelCreated)
        {
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.sizeDelta = new Vector2(430f, 210f);
        }

        EnsureGuideTextChildren(panelObject.transform, ref isChanged);
        return isChanged;
    }

    private static bool EnsureGuideTextChildren(Transform panelRoot, ref bool isChanged)
    {
        bool isTitleCreated = FindDirectChild(panelRoot, "Text_GuideTitle") == null;
        TextMeshProUGUI titleText = EnsureText(panelRoot, "Text_GuideTitle", "튜토리얼", 26, FontStyles.Bold, Color.black, ref isChanged);

        if (isTitleCreated)
        {
            RectTransform titleRect = titleText.transform as RectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
            titleRect.sizeDelta = new Vector2(-36f, 42f);
        }

        bool isBodyCreated = FindDirectChild(panelRoot, "Text_GuideBody") == null;
        TextMeshProUGUI bodyText = EnsureText(panelRoot, "Text_GuideBody", "기능 안내", 22, FontStyles.Normal, Color.black, ref isChanged);

        if (isBodyCreated)
        {
            RectTransform bodyRect = bodyText.transform as RectTransform;
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.offsetMin = new Vector2(28f, 66f);
            bodyRect.offsetMax = new Vector2(-28f, -68f);
            bodyText.alignment = TextAlignmentOptions.TopLeft;
        }

        EnsureButton(panelRoot, "Button_GuideNext", "다음", new Vector2(0f, 32f), new Vector2(150f, 52f), ref isChanged);
        return isChanged;
    }

    private static bool EnsureMainMenuConfirmPopup(Transform hudRoot)
    {
        bool isChanged = false;
        bool isPopupCreated = FindDirectChild(hudRoot, "Panel_MainMenuConfirm") == null;
        GameObject popupObject = EnsurePanel(hudRoot, "Panel_MainMenuConfirm", new Color(0.04f, 0.04f, 0.04f, 0.96f), ref isChanged);
        RectTransform popupRect = popupObject.transform as RectTransform;

        if (isPopupCreated)
        {
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);
            popupRect.anchoredPosition = Vector2.zero;
            popupRect.sizeDelta = new Vector2(560f, 260f);
            popupObject.SetActive(false);
        }

        bool isMessageCreated = FindDirectChild(popupObject.transform, "Text_Message") == null;
        TextMeshProUGUI messageText = EnsureText(popupObject.transform, "Text_Message", "정말 메인 메뉴로 돌아가시겠습니까?", 30, FontStyles.Bold, Color.white, ref isChanged);

        if (isMessageCreated)
        {
            RectTransform messageRect = messageText.transform as RectTransform;
            messageRect.anchorMin = new Vector2(0.5f, 1f);
            messageRect.anchorMax = new Vector2(0.5f, 1f);
            messageRect.pivot = new Vector2(0.5f, 1f);
            messageRect.anchoredPosition = new Vector2(0f, -46f);
            messageRect.sizeDelta = new Vector2(500f, 90f);
        }

        EnsureButton(popupObject.transform, "Button_Yes", "예", new Vector2(-110f, -72f), new Vector2(160f, 62f), ref isChanged);
        EnsureButton(popupObject.transform, "Button_No", "아니오", new Vector2(110f, -72f), new Vector2(160f, 62f), ref isChanged);
        return isChanged;
    }

    private static GameObject EnsurePanel(Transform parent, string objectName, Color color, ref bool isChanged)
    {
        GameObject panelObject = EnsureUIObject(parent, objectName, ref isChanged);
        Image image = panelObject.GetComponent<Image>();

        if (image == null)
        {
            image = panelObject.AddComponent<Image>();
            image.color = color;
            isChanged = true;
        }

        return panelObject;
    }

    private static Button EnsureButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 sizeDelta, ref bool isChanged)
    {
        bool isCreated = FindDirectChild(parent, objectName) == null;
        GameObject buttonObject = EnsurePanel(parent, objectName, new Color(0.22f, 0.22f, 0.22f, 0.96f), ref isChanged);
        RectTransform buttonRect = buttonObject.transform as RectTransform;

        if (isCreated)
        {
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = anchoredPosition;
            buttonRect.sizeDelta = sizeDelta;
        }

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
            isChanged = true;
        }

        button.targetGraphic = buttonObject.GetComponent<Image>();
        TextMeshProUGUI buttonText = EnsureText(buttonObject.transform, "Text_Label", label, 24, FontStyles.Bold, Color.white, ref isChanged);
        StretchToParent(buttonText.transform as RectTransform);
        return button;
    }

    private static TextMeshProUGUI EnsureBadge(Transform parent, string objectName, Vector2 anchoredPosition, ref bool isChanged)
    {
        bool isCreated = FindDirectChild(parent, objectName) == null;
        TextMeshProUGUI badgeText = EnsureText(parent, objectName, "NEW", 24, FontStyles.Bold, Color.yellow, ref isChanged);
        RectTransform badgeRect = badgeText.transform as RectTransform;

        if (isCreated)
        {
            badgeRect.anchorMin = new Vector2(0.5f, 0f);
            badgeRect.anchorMax = new Vector2(0.5f, 0f);
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.anchoredPosition = anchoredPosition;
            badgeRect.sizeDelta = new Vector2(88f, 36f);
            badgeText.gameObject.SetActive(false);
        }

        return badgeText;
    }

    private static TextMeshProUGUI EnsureText(Transform parent, string objectName, string text, int fontSize, FontStyles fontStyle, Color color, ref bool isChanged)
    {
        GameObject textObject = EnsureUIObject(parent, objectName, ref isChanged);
        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();

        if (textComponent == null)
        {
            textComponent = textObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = fontStyle;
            textComponent.color = color;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.textWrappingMode = TextWrappingModes.Normal;
            textComponent.raycastTarget = false;
            OOTechTMPFontUtility.ApplyProjectFont(textComponent);
            isChanged = true;
        }

        return textComponent;
    }

    private static GameObject EnsureUIObject(Transform parent, string objectName, ref bool isChanged)
    {
        GameObject uiObject = FindDirectChild(parent, objectName);

        if (uiObject != null)
            return uiObject;

        uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        isChanged = true;
        return uiObject;
    }

    private static GameObject FindSceneObjectByName(Scene scene, string objectName)
    {
        GameObject[] objectArray = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject targetObject in objectArray)
        {
            if (targetObject == null || targetObject.name != objectName)
                continue;

            if (!targetObject.scene.IsValid() || targetObject.scene.path != scene.path)
                continue;

            return targetObject;
        }

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = FindChildByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private static GameObject FindDirectChild(Transform parent, string objectName)
    {
        if (parent == null)
            return null;

        Transform childTransform = parent.Find(objectName);
        return childTransform != null ? childTransform.gameObject : null;
    }

    private static GameObject FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
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

    private static void StretchToParent(RectTransform rectTransform)
    {
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }
}
