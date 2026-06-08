using System;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class OOTechStage4BatchValidator
{
    private const string ScenePath = "Assets/Scenes/OO_MFC.unity";

    public static void Run()
    {
        EditorSceneManager.OpenScene(ScenePath);

        GameObject stage4_1Group = FindSceneObject("Stage4_1Group");
        GameObject stage4_2Group = FindSceneObject("Stage4_2Group");
        GameObject moranObject = FindActorObject(stage4_1Group.transform, "Moran", "Moran");
        GameObject turtleObject = FindChildObject(stage4_1Group.transform, "Turtle");
        GameObject backgroundObject = FindChildObject(stage4_1Group.transform, "Stage4_1Background");
        GameObject stage4_2MoranObject = FindActorObject(stage4_2Group.transform, "Moran", "Moran");
        GameObject stage4_2BackgroundObject = FindChildObject(stage4_2Group.transform, "Stage4_2Background");
        GameObject stopPointAObject = FindChildObject(stage4_2Group.transform, "StopPoint_A");
        GameObject rabbitObject = FindChildObject(stage4_2Group.transform, "Rabbit");
        GameObject sleepingRabbitObject = FindChildObject(stage4_2Group.transform, "Rabbit_isSleeping");
        GameObject preFinalGroup = FindSceneObject("PreFinal_Narration");
        GameObject preFinalCutScene = FindChildObject(preFinalGroup.transform, "PreFinalCutScene");
        GameObject finalStageGroup = FindSceneObject("FinalStageGroup");
        GameObject epilogueGroup = FindSceneObject("EpilogueGroup");
        GameObject epilogueCutScene = FindChildObject(epilogueGroup.transform, "EpilogueCutScene");
        GameObject endingCreditGroup = FindSceneObject("EndingCreditGroup");
        GameObject mainMenuGroup = FindSceneObject("MainMenuGroup");
        GameObject cookingGroup = FindSceneObject("CookingGroup");
        GameObject fourthRoadGroup = FindSceneObject("4th_Road_to_Stage4");
        GameObject canvasEndingCreditObject = FindChildObject(endingCreditGroup.transform, "Canvas_EndingCredit");
        GameObject creditRootObject = FindChildObject(endingCreditGroup.transform, "CreditRoot");
        GameObject textCreditObject = FindChildObject(endingCreditGroup.transform, "Text_Credit");
        GameObject logoObject = FindChildObject(endingCreditGroup.transform, "LOGO");
        GameObject returnMainMenuButtonObject = FindChildObject(endingCreditGroup.transform, "Button_ReturnMainMenu");

        OOTechStageMoranFreeMoveController moveController = moranObject != null ? moranObject.GetComponent<OOTechStageMoranFreeMoveController>() : null;
        OOTechStage4GroupController stageController = stage4_1Group != null ? stage4_1Group.GetComponent<OOTechStage4GroupController>() : null;
        OOTechStage4GroupController stage4_2Controller = stage4_2Group != null ? stage4_2Group.GetComponent<OOTechStage4GroupController>() : null;
        SpriteRenderer backgroundRenderer = backgroundObject != null ? backgroundObject.GetComponent<SpriteRenderer>() : null;
        SpriteRenderer stage4_2BackgroundRenderer = stage4_2BackgroundObject != null ? stage4_2BackgroundObject.GetComponent<SpriteRenderer>() : null;
        OOTechEndingCreditController endingCreditController = endingCreditGroup != null ? endingCreditGroup.GetComponent<OOTechEndingCreditController>() : null;
        MainMenuController mainMenuController = mainMenuGroup != null ? mainMenuGroup.GetComponent<MainMenuController>() : null;
        OOTechCookingGroupController cookingController = cookingGroup != null ? cookingGroup.GetComponent<OOTechCookingGroupController>() : null;
        OOTechRoadHUDController fourthRoadHUDController = fourthRoadGroup != null ? fourthRoadGroup.GetComponent<OOTechRoadHUDController>() : null;

        if (stageController != null)
        {
            InvokePrivate(stageController, "ResolveComponents");
            Transform resolvedMoranTransform = GetPrivateField<Transform>(stageController, "Transform_Moran");

            if (resolvedMoranTransform != null)
                moranObject = resolvedMoranTransform.gameObject;
        }

        if (stage4_2Controller != null)
        {
            InvokePrivate(stage4_2Controller, "ResolveComponents");
            Transform resolvedStage4_2MoranTransform = GetPrivateField<Transform>(stage4_2Controller, "Transform_Moran");

            if (resolvedStage4_2MoranTransform != null)
                stage4_2MoranObject = resolvedStage4_2MoranTransform.gameObject;
        }

        moveController = moranObject != null ? moranObject.GetComponent<OOTechStageMoranFreeMoveController>() : null;

        Require(moranObject != null, "Stage4_1 Moran is missing.");
        Require(moveController != null, "Stage4_1 Moran movement controller is missing.");
        Require(stageController != null, "Stage4_1 controller is missing.");
        Require(stage4_2Controller != null, "Stage4_2 controller is missing.");
        Require(mainMenuController != null, "MainMenuController is missing.");
        Require(backgroundRenderer != null, "Stage4_1 background renderer is missing.");
        Require(stage4_2MoranObject != null, "Stage4_2 Moran is missing.");
        Require(stage4_2BackgroundRenderer != null, "Stage4_2 background renderer is missing.");
        Require(stopPointAObject != null, "Stage4_2 StopPoint_A is missing.");
        Require(turtleObject != null && turtleObject.GetComponent<OOTechSceneObject>() != null, "Stage4_1 Turtle role marker is missing.");
        RequireRabbitSpeechBubbleObjects(rabbitObject, sleepingRabbitObject);
        Require(preFinalGroup.GetComponent<OOTechCutSceneNarrationController>() != null, "PreFinal_Narration cutscene controller is missing.");
        Require(HasCutSceneVisual(preFinalCutScene), "PreFinalCutScene background is missing.");
        RequireFinalStageObjects(finalStageGroup);
        Require(epilogueGroup.GetComponent<OOTechCutSceneNarrationController>() != null, "EpilogueGroup cutscene controller is missing.");
        Require(HasCutSceneVisual(epilogueCutScene), "EpilogueCutScene background is missing.");
        Require(endingCreditController != null, "EndingCreditGroup controller is missing.");
        Require(cookingController != null, "CookingGroup controller is missing.");
        Require(fourthRoadHUDController != null, "4th_Road_to_Stage4 HUD controller is missing.");
        RequireEndingCreditObjects(canvasEndingCreditObject, creditRootObject, textCreditObject, logoObject, returnMainMenuButtonObject);
        string resolvedCreditText = InvokePrivate<string>(endingCreditController, "ResolveCreditText");
        Require(resolvedCreditText.Contains("UnityBasic_6 by DaniTech"), "Ending credit text did not repair the serialized UnityBasic line.");
        RequireStage4CueSheetData();
        RequireFinalCueSheetData();
        RequireFinalDialogueData();

        InvokePrivate(stageController, "ResolveComponents");
        InvokePrivate(stage4_2Controller, "ResolveComponents");
        InvokePrivate(moveController, "ResolveComponents");
        Require(GetPrivateField<bool>(mainMenuController, "_isShowDeveloperSkipButtons"), "MainMenu old developer skip buttons must be restored.");
        Require(GetPrivateField<string>(mainMenuController, "_developerRoad1GroupName") == "1st_Road_to_Stage1", "MainMenu DEV Road 1 target is not 1st_Road_to_Stage1.");
        InvokePrivate(mainMenuController, "PrepareDeveloperSkipButtons");
        InvokePrivate(mainMenuController, "PrepareDeveloperStage3Button");
        RequireMainMenuDeveloperSkipButtons(mainMenuGroup);
        RequireMainMenuStage3DeveloperButton(mainMenuGroup);
        float walkSpeed = InvokePrivate<float>(moveController, "ResolveEffectiveMoveSpeed", false);
        float runSpeed = InvokePrivate<float>(moveController, "ResolveEffectiveMoveSpeed", true);

        Require(backgroundRenderer.bounds.size.x > 1000f, $"Stage4_1 background width is too small: {backgroundRenderer.bounds.size.x}");
        Require(walkSpeed >= 100f, $"Stage4_1 effective walk speed is too slow: {walkSpeed}");
        Require(runSpeed >= 200f, $"Stage4_1 effective run speed is too slow: {runSpeed}");

        Transform moranTransform = moranObject.transform;
        Transform turtleTransform = turtleObject.transform;
        Vector3 originalMoranPosition = moranTransform.position;
        Transform stage4_2MoranTransform = stage4_2MoranObject.transform;
        Vector3 originalStage4_2MoranPosition = stage4_2MoranTransform.position;

        try
        {
            moranTransform.position = turtleTransform.position + Vector3.left * 80f;
            bool isNearTurtle = InvokePrivate<bool>(stageController, "IsNear", moranTransform, turtleTransform);
            Require(isNearTurtle, "Stage4_1 Moran/Turtle interaction range check failed.");

            InvokePrivate(stageController, "PlaceMoranAtRightEdge");
            bool isStage4_1RightEdgeReachable = InvokePrivate<bool>(stageController, "IsMoranAtRightEdge");
            Require(isStage4_1RightEdgeReachable, "Stage4_1 right edge transition check failed.");

            InvokePrivate(stage4_2Controller, "PlaceMoranAtStage4_2VisibleStart");
            InvokePrivate(stage4_2Controller, "RequestForceActorVisible", stage4_2MoranTransform, 2200);
            RequireRenderableActor(stage4_2MoranObject, "Stage4_2 Moran", 2200);
            RequireActorVisibleToMainCamera(stage4_2MoranObject, "Stage4_2 Moran");
            InvokePrivate(stage4_2Controller, "RequestPrepareStage4_2CookingQuestHUD");
            RequireStage4_2HUD(stage4_2Group);
            RequireStage4PresentationMoranInsurance(stage4_2Controller, stage4_2MoranObject);
            RequireStage4PresentationHUDCanLock(fourthRoadHUDController, "4th_Road_to_Stage4");
            InvokePrivate(stage4_2Controller, "PlaceMoranAtLeftEdge");
            bool isStage4_2LeftEdgeReachable = InvokePrivate<bool>(stage4_2Controller, "IsMoranAtLeftEdge");
            Require(isStage4_2LeftEdgeReachable, "Stage4_2 left edge return check failed.");

            RequireSpeechBubbleCanHide(rabbitObject, "Rabbit");
            RequireSpeechBubbleCanHide(sleepingRabbitObject, "Rabbit_isSleeping");
            RequireStage4CookingRecipeUI(cookingController);

            stopPointAObject.SetActive(true);
            stage4_2Controller.RequestResolveStage4_2CookingReturn(true);
            Require(!stopPointAObject.activeSelf, "Stage4_2 cooking return did not disable StopPoint_A.");
        }
        finally
        {
            moranTransform.position = originalMoranPosition;
            stage4_2MoranTransform.position = originalStage4_2MoranPosition;
        }

        Debug.Log($"[OOTechStage4BatchValidator] PASS Stage4 movement/interact/roundtrip and final route check. width={backgroundRenderer.bounds.size.x:0.##}, stage4_2Width={stage4_2BackgroundRenderer.bounds.size.x:0.##}, walk={walkSpeed:0.##}, run={runSpeed:0.##}");
    }

    private static void RequireRenderableActor(GameObject actorObject, string label, int minimumSortingOrder)
    {
        Require(actorObject != null, $"{label} is missing.");
        Require(actorObject.activeSelf, $"{label} object is inactive.");
        Require(actorObject.activeInHierarchy, $"{label} parent chain is inactive.");
        Require(Mathf.Abs(actorObject.transform.localScale.x) > 0.0001f && Mathf.Abs(actorObject.transform.localScale.y) > 0.0001f, $"{label} scale is zero.");

        SpriteRenderer spriteRenderer = actorObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = actorObject.GetComponentInChildren<SpriteRenderer>(true);

        Require(spriteRenderer != null && spriteRenderer.sprite != null, $"{label} sprite renderer is missing.");
        Require(spriteRenderer.enabled && !spriteRenderer.forceRenderingOff, $"{label} sprite renderer is disabled.");
        Require(spriteRenderer.color.a > 0.9f, $"{label} sprite alpha is transparent.");
        Require(spriteRenderer.sortingLayerName == "Characters", $"{label} sorting layer is not Characters: {spriteRenderer.sortingLayerName}");
        Require(spriteRenderer.sortingOrder >= minimumSortingOrder, $"{label} sorting order is too low: {spriteRenderer.sortingOrder}");
    }

    private static void RequireActorVisibleToMainCamera(GameObject actorObject, string label)
    {
        Require(actorObject != null, $"{label} is missing.");

        Camera mainCamera = Camera.main;
        Require(mainCamera != null, "Main camera is missing.");

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(actorObject.transform.position);
        Require(viewportPosition.z > 0f, $"{label} is behind the main camera.");
        Require(viewportPosition.x >= 0.04f && viewportPosition.x <= 0.96f, $"{label} is outside the camera horizontally: {viewportPosition.x:0.###}");
        Require(viewportPosition.y >= 0.04f && viewportPosition.y <= 0.96f, $"{label} is outside the camera vertically: {viewportPosition.y:0.###}");
    }

    private static void RequireStage4_2HUD(GameObject stage4_2Group)
    {
        OOTechRoadHUDController hudController = stage4_2Group.GetComponent<OOTechRoadHUDController>();
        Require(hudController != null, "Stage4_2 HUD controller was not prepared.");
        Require(hudController.IsCookingUnlocked, "Stage4_2 cooking button is not unlocked.");
        Require(hudController.IsStage4PresentationHUDLockedOpen, "Stage4_2 presentation HUD lock is not active.");

        string missionText = GetPrivateField<string>(hudController, "_stageQuestMissionText");
        TextMeshProUGUI guideTitleText = GetPrivateField<TextMeshProUGUI>(hudController, "Text_GuideTitle");
        TextMeshProUGUI guideBodyText = GetPrivateField<TextMeshProUGUI>(hudController, "Text_GuideBody");

        Require(!string.IsNullOrWhiteSpace(missionText) && missionText.Contains("당근전"), "Stage4_2 mission did not switch to carrot cake quest.");
        Require(guideTitleText != null && guideTitleText.text.Contains("당근전"), "Stage4_2 tutorial guide title is missing carrot cake text.");
        Require(guideBodyText != null && guideBodyText.text.Contains("당근"), "Stage4_2 tutorial guide body is missing carrot recipe guidance.");
    }

    private static void RequireStage4PresentationMoranInsurance(OOTechStage4GroupController stage4_2Controller, GameObject stage4_2MoranObject)
    {
        bool originalActiveState = stage4_2MoranObject.activeSelf;
        stage4_2MoranObject.SetActive(false);

        try
        {
            InvokePrivate(stage4_2Controller, "RequestKeepPresentationInsuranceAlive");
            Transform resolvedMoran = GetPrivateField<Transform>(stage4_2Controller, "Transform_Moran");
            Require(resolvedMoran != null, "Stage4_2 presentation Moran insurance did not resolve Moran.");
            RequireRenderableActor(resolvedMoran.gameObject, "Stage4_2 insured Moran", 2200);
            RequireActorVisibleToMainCamera(resolvedMoran.gameObject, "Stage4_2 insured Moran");
        }
        finally
        {
            stage4_2MoranObject.SetActive(originalActiveState);
        }
    }

    private static void RequireStage4PresentationHUDCanLock(OOTechRoadHUDController hudController, string ownerGroupName)
    {
        hudController.RequestEnableStage4PresentationHUD(
            ownerGroupName,
            "북쪽 농경지대로 가세요.",
            "당근전 조리 가이드",
            "떡과 당근으로 당근전분을 만들고, 당근전분을 가마솥에 넣어 당근전을 완성하세요.");

        GameObject inventoryPanel = GetPrivateField<GameObject>(hudController, "Root_InventoryPanel");
        GameObject missionPanel = GetPrivateField<GameObject>(hudController, "Root_MissionPanel");

        Require(hudController.IsStage4PresentationHUDLockedOpen, $"{ownerGroupName} presentation HUD lock is not active.");
        Require(hudController.IsCookingUnlocked, $"{ownerGroupName} cooking button is not unlocked.");
        Require(inventoryPanel != null && !inventoryPanel.activeSelf, $"{ownerGroupName} inventory panel should stay user-toggleable outside CookingGroup.");
        Require(missionPanel != null && !missionPanel.activeSelf, $"{ownerGroupName} mission panel should stay user-toggleable outside CookingGroup.");
    }

    private static void RequireStage4CookingRecipeUI(OOTechCookingGroupController cookingController)
    {
        OOTechGroupNavigationHistory.SetPreviousGroup("CookingGroup", "Stage4_2Group");
        InvokePrivate(cookingController, "PrepareCookingView");
        InvokePrivate(cookingController, "RequestPrepareStage4RecipeCombineUIIfNeeded");
        InvokePrivate(cookingController, "RequestOpenCookingSupportHUDIfNeeded");

        Button combineButton = GetPrivateField<Button>(cookingController, "Button_HoneyCakeCombine");
        GameObject combinePanel = GetPrivateField<GameObject>(cookingController, "Root_HoneyCakeCombinePanel");
        TextMeshProUGUI guideText = GetPrivateField<TextMeshProUGUI>(cookingController, "Text_HoneyCakeGuide");
        OOTechRoadHUDController stage4CookingHUD = ResolveStage4CookingSupportHUD();
        bool isUnlocked = InvokePrivate<bool>(cookingController, "IsRecipeCombineUnlocked");

        Require(isUnlocked, "Stage4 cooking recipe combine is not unlocked.");
        Require(combineButton != null && combineButton.gameObject.activeSelf && combineButton.interactable, "Stage4 cooking recipe combine button is not visible.");
        Require(combinePanel != null && combinePanel.activeSelf, "Stage4 cooking recipe combine panel is not open.");
        Require(stage4CookingHUD != null && stage4CookingHUD.IsStage4CookingSupportLockedOpen, "Stage4 cooking support HUD is not locked open.");
        Require(stage4CookingHUD != null && stage4CookingHUD.IsStage4PersistentGuideLockedOpen, "Stage4 cooking guide is not persistent.");
        Require(guideText != null && guideText.text.Contains("당근"), "Stage4 cooking recipe guide text is not data-driven for carrot cake.");
    }

    private static OOTechRoadHUDController ResolveStage4CookingSupportHUD()
    {
        OOTechRoadHUDController[] hudControllerArray = UnityEngine.Object.FindObjectsByType<OOTechRoadHUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (OOTechRoadHUDController hudController in hudControllerArray)
        {
            if (hudController != null && hudController.IsStage4CookingSupportLockedOpen)
                return hudController;
        }

        return null;
    }

    private static void RequireSpeechBubbleCanHide(GameObject actorObject, string label)
    {
        GameObject speechBubbleObject = FindChildObject(actorObject.transform, "SpeechBubble");
        Require(speechBubbleObject != null, $"{label} SpeechBubble child is missing.");

        OOTechSpeechBubbleView speechBubbleView = speechBubbleObject.GetComponent<OOTechSpeechBubbleView>();

        if (speechBubbleView == null)
            speechBubbleView = speechBubbleObject.AddComponent<OOTechSpeechBubbleView>();

        speechBubbleObject.SetActive(true);
        speechBubbleView.RequestPrepareHidden();
        Require(!speechBubbleObject.activeSelf, $"{label} SpeechBubble did not hide before cue playback.");

        speechBubbleObject.SetActive(true);
        speechBubbleView.RequestPlaySpeechBubble("character_Rabbit_01", 1f);

        GameObject overlayCanvasObject = FindChildObject(speechBubbleObject.transform, "Canvas_ScreenSpeechBubbleText");
        GameObject overlayTextObject = FindChildObject(speechBubbleObject.transform, "Text_ScreenSpeechBubbleBody");
        TextMeshProUGUI overlayText = overlayTextObject != null ? overlayTextObject.GetComponent<TextMeshProUGUI>() : null;
        Canvas overlayCanvas = overlayCanvasObject != null ? overlayCanvasObject.GetComponent<Canvas>() : null;

        Require(overlayCanvas != null, $"{label} screen overlay speech canvas is missing.");
        Require(overlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay, $"{label} screen overlay speech canvas is not overlay mode.");
        Require(overlayCanvas.overrideSorting && overlayCanvas.sortingOrder >= 9999, $"{label} screen overlay speech canvas sorting is too low.");
        Require(overlayText != null, $"{label} screen overlay speech text is missing.");
        Require(overlayText.enabled && overlayText.gameObject.activeSelf, $"{label} screen overlay speech text is inactive.");
        Require(overlayText.text.Contains("\uB2F9\uADFC\uC804"), $"{label} screen overlay speech text did not receive Rabbit data.");
        Require(overlayText.color.a > 0.9f && overlayText.color.r < 0.1f && overlayText.color.g < 0.1f && overlayText.color.b < 0.1f, $"{label} screen overlay speech text is not solid black.");

        speechBubbleView.RequestPrepareHidden();
    }

    private static void RequireMainMenuStage3DeveloperButton(GameObject mainMenuGroup)
    {
        GameObject buttonObject = FindChildObject(mainMenuGroup.transform, "Button_DevSkip_Stage3");
        Require(buttonObject != null, "MainMenu DEV Stage 3 button is missing.");
        Require(buttonObject.GetComponent<Button>() != null, "MainMenu DEV Stage 3 button component is missing.");

        RectTransform rootRect = buttonObject.transform.parent as RectTransform;
        Require(rootRect != null, "MainMenu DEV Stage 3 root rect is missing.");
        Require(Mathf.Approximately(rootRect.anchorMin.x, 1f) && Mathf.Approximately(rootRect.anchorMax.x, 1f), "MainMenu DEV Stage 3 root is not anchored to the right.");
        Require(Mathf.Approximately(rootRect.anchorMin.y, 1f) && Mathf.Approximately(rootRect.anchorMax.y, 1f), "MainMenu DEV Stage 3 root is not anchored to the top.");

        TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
        Require(label != null && label.text.Contains("Stage 3"), "MainMenu DEV Stage 3 button label is invalid.");
    }

    private static void RequireMainMenuDeveloperSkipButtons(GameObject mainMenuGroup)
    {
        RequireDeveloperButton(mainMenuGroup, "Button_DevSkip_Road1", "Road 1");
        RequireDeveloperButton(mainMenuGroup, "Button_DevSkip_Stage1", "Stage 1");
        RequireDeveloperButton(mainMenuGroup, "Button_DevSkip_Road2", "Road 2");
    }

    private static void RequireDeveloperButton(GameObject mainMenuGroup, string objectName, string labelFragment)
    {
        GameObject buttonObject = FindChildObject(mainMenuGroup.transform, objectName);
        Require(buttonObject != null, $"MainMenu {objectName} is missing.");
        Require(buttonObject.GetComponent<Button>() != null, $"MainMenu {objectName} button component is missing.");

        TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
        Require(label != null && label.text.Contains(labelFragment), $"MainMenu {objectName} label is invalid.");
    }

    private static void RequireFinalStageObjects(GameObject finalStageGroup)
    {
        Require(finalStageGroup != null, "FinalStageGroup is missing.");
        Require(finalStageGroup.GetComponent<OOTechFinalStageController>() != null, "FinalStageGroup controller is missing.");
        Require(finalStageGroup.GetComponent<OOTechStage3DialogueCue>() != null, "FinalStageGroup dialogue cue is missing.");
        Require(FindChildObject(finalStageGroup.transform, "FinalStageBackground") != null, "FinalStageBackground is missing.");
        Require(FindChildObject(finalStageGroup.transform, "Moran") != null, "FinalStage Moran is missing.");
        Require(FindChildObject(finalStageGroup.transform, "YeonSanJa") != null, "FinalStage YeonSanJa is missing.");
        Require(FindChildObject(finalStageGroup.transform, "End_Point") != null, "FinalStage End_Point is missing.");
    }

    private static bool HasCutSceneVisual(GameObject cutSceneObject)
    {
        if (cutSceneObject == null)
            return false;

        SpriteRenderer spriteRenderer = cutSceneObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && spriteRenderer.sprite != null)
            return true;

        Image image = cutSceneObject.GetComponent<Image>();
        return image != null && image.sprite != null;
    }

    private static void RequireRabbitSpeechBubbleObjects(GameObject rabbitObject, GameObject sleepingRabbitObject)
    {
        Require(rabbitObject != null, "Stage4_2 Rabbit is missing.");
        Require(sleepingRabbitObject != null, "Stage4_2 Rabbit_isSleeping is missing.");
        Require(FindChildObject(rabbitObject.transform, "SpeechBubble") != null, "Rabbit SpeechBubble child is missing.");
        Require(FindChildObject(sleepingRabbitObject.transform, "SpeechBubble") != null, "Rabbit_isSleeping SpeechBubble child is missing.");
    }

    private static void RequireEndingCreditObjects(
        GameObject canvasEndingCreditObject,
        GameObject creditRootObject,
        GameObject textCreditObject,
        GameObject logoObject,
        GameObject returnMainMenuButtonObject)
    {
        Require(canvasEndingCreditObject != null && canvasEndingCreditObject.GetComponent<Canvas>() != null, "Canvas_EndingCredit is missing.");
        Require(creditRootObject != null && creditRootObject.GetComponent<RectTransform>() != null, "CreditRoot is missing.");
        Require(textCreditObject != null && textCreditObject.GetComponent<TextMeshProUGUI>() != null, "Text_Credit is missing.");
        Require(returnMainMenuButtonObject != null && returnMainMenuButtonObject.GetComponent<Button>() != null, "Button_ReturnMainMenu is missing.");
        Require(logoObject != null && logoObject.GetComponent<RectTransform>() != null, "LOGO is missing.");
        Require(logoObject.GetComponent<Image>() != null && logoObject.GetComponent<Image>().sprite != null, "LOGO UI Image sprite is missing.");
        Require(logoObject.GetComponent<SpriteRenderer>() != null && logoObject.GetComponent<SpriteRenderer>().sprite != null, "LOGO SpriteRenderer source sprite is missing.");
        Require(logoObject.GetComponent<Animator>() != null && logoObject.GetComponent<Animator>().runtimeAnimatorController != null, "LOGO Animator controller is missing.");
        Require(AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animation/Logo/OOLOGO_3_7.controller") != null, "OOLOGO_3_7 animator controller asset is missing.");
        Require(AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animation/Logo/PCROOMS_.anim") != null, "PCROOMS_ animation clip is missing.");
    }

    private static void RequireFinalCueSheetData()
    {
        TextAsset finalCueSheetAsset = Resources.Load<TextAsset>("JsonOutput/OO_FinalCueSheet");

        Require(finalCueSheetAsset != null, "OO_FinalCueSheet resource is missing.");
        Require(finalCueSheetAsset.text.Contains("\"PreFinalGroupName\": \"PreFinal_Narration\""), "Final cue sheet PreFinalGroupName is invalid.");
        Require(finalCueSheetAsset.text.Contains("\"FinalStageGroupName\": \"FinalStageGroup\""), "Final cue sheet FinalStageGroupName is invalid.");
        Require(finalCueSheetAsset.text.Contains("\"EpilogueGroupName\": \"EpilogueGroup\""), "Final cue sheet EpilogueGroupName is invalid.");
        Require(finalCueSheetAsset.text.Contains("\"EndingCreditGroupName\": \"EndingCreditGroup\""), "Final cue sheet EndingCreditGroupName is invalid.");
        Require(finalCueSheetAsset.text.Contains("\"PreFinalNarrationId\": \"narration_prologue_08\""), "Final cue sheet PreFinalNarrationId is invalid.");
        Require(finalCueSheetAsset.text.Contains("\"EpilogueNarrationId\": \"narration_Epilogue_01\""), "Final cue sheet EpilogueNarrationId is invalid.");
        Require(finalCueSheetAsset.text.Contains("\"FinalOpeningDialogueId\": \"character_YeonSanJa_01\""), "Final cue sheet FinalOpeningDialogueId is invalid.");
        Require(finalCueSheetAsset.text.Contains("\"FinalHappyDialogueIdList\": \"character_YeonSanJa_02|character_YeonSanJa_03\""), "Final cue sheet FinalHappyDialogueIdList is invalid.");
    }

    private static void RequireStage4CueSheetData()
    {
        TextAsset stage4CueSheetAsset = Resources.Load<TextAsset>("JsonOutput/OO_Stage4CueSheet");
        TextAsset speechBubbleAsset = Resources.Load<TextAsset>("JsonOutput/OO_SpeechBubble");
        TextAsset recipeAsset = Resources.Load<TextAsset>("JsonOutput/OO_Recipe");
        TextAsset stageQuestAsset = Resources.Load<TextAsset>("JsonOutput/OO_StageQuest");

        Require(stage4CueSheetAsset != null, "OO_Stage4CueSheet resource is missing.");
        Require(speechBubbleAsset != null, "OO_SpeechBubble resource is missing.");
        Require(recipeAsset != null, "OO_Recipe resource is missing.");
        Require(stageQuestAsset != null, "OO_StageQuest resource is missing.");
        Require(stage4CueSheetAsset.text.Contains("\"PreFinalGroupName\": \"PreFinal_Narration\""), "Stage4 cue sheet PreFinalGroupName is invalid.");
        Require(stage4CueSheetAsset.text.Contains("\"RabbitSpeechBubbleIdList\": \"character_Rabbit_01|character_Rabbit_02|character_Rabbit_03|character_Rabbit_04|character_Rabbit_05\""), "Rabbit speech bubble cue list is invalid.");
        Require(stage4CueSheetAsset.text.Contains("\"RabbitSleepingBubbleId\": \"character_Rabbit_06\""), "Rabbit sleeping bubble cue is invalid.");
        Require(stage4CueSheetAsset.text.Contains("\"StageQuestId\": \"Stage4__Quest_01\""), "Stage4 carrot cake quest id is invalid.");
        Require(recipeAsset.text.Contains("\"ResultItemId\": \"OO_CarrotStarch_1\"") && recipeAsset.text.Contains("\"RequiredToolIds\": \"RecipeMix\""), "Carrot starch RecipeMix data is missing.");
        Require(recipeAsset.text.Contains("\"ResultItemId\": \"OO_CarrotCake_1\"") && recipeAsset.text.Contains("\"RequiredToolIds\": \"Cauldron\""), "Carrot cake Cauldron data is missing.");
        Require(stageQuestAsset.text.Contains("\"Id\": \"Stage4__Quest_01\"") && stageQuestAsset.text.Contains("당근전"), "Stage4 carrot cake quest data is missing.");

        for (int index = 1; index <= 6; index++)
            Require(speechBubbleAsset.text.Contains($"\"Id\": \"character_Rabbit_{index:00}\""), $"Rabbit speech bubble data missing: character_Rabbit_{index:00}");

        Require(speechBubbleAsset.text.Contains("\"Id\": \"character_Rabbit_06\"") && speechBubbleAsset.text.Contains("\"IsLoop\": true"), "Rabbit sleeping speech bubble must be looped.");
    }

    private static void RequireFinalDialogueData()
    {
        TextAsset dialogueAsset = Resources.Load<TextAsset>("JsonOutput/OO_Dialogue");
        TextAsset narrationAsset = Resources.Load<TextAsset>("JsonOutput/OO_Narration");

        Require(dialogueAsset != null, "OO_Dialogue resource is missing.");
        Require(narrationAsset != null, "OO_Narration resource is missing.");
        Require(narrationAsset.text.Contains("\"Id\": \"narration_prologue_08\""), "PreFinal narration data is missing.");
        Require(narrationAsset.text.Contains("\"Id\": \"narration_Epilogue_01\""), "Epilogue narration data is missing.");
        Require(dialogueAsset.text.Contains("\"Id\": \"character_YeonSanJa_01\""), "YeonSanJa opening dialogue data is missing.");
        Require(dialogueAsset.text.Contains("\"Id\": \"character_YeonSanJa_02\""), "YeonSanJa happy dialogue 02 data is missing.");
        Require(dialogueAsset.text.Contains("\"Id\": \"character_YeonSanJa_03\""), "YeonSanJa happy dialogue 03 data is missing.");
    }

    private static GameObject FindSceneObject(string objectName)
    {
        foreach (GameObject rootObject in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            GameObject foundObject = FindChildObject(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        throw new InvalidOperationException($"Scene object not found: {objectName}");
    }

    private static GameObject FindChildObject(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = FindChildObject(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private static GameObject FindActorObject(Transform rootTransform, string objectName, string roleId)
    {
        GameObject foundObject = FindChildObject(rootTransform, objectName);

        if (foundObject != null)
            return foundObject;

        if (rootTransform == null || string.IsNullOrEmpty(roleId))
            return FindSceneWideActorObject(objectName, roleId);

        OOTechSceneObject[] sceneObjectArray = rootTransform.GetComponentsInChildren<OOTechSceneObject>(true);

        foreach (OOTechSceneObject sceneObject in sceneObjectArray)
        {
            if (sceneObject != null && sceneObject.RoleId == roleId)
                return sceneObject.gameObject;
        }

        return FindSceneWideActorObject(objectName, roleId);
    }

    private static GameObject FindSceneWideActorObject(string objectName, string roleId)
    {
        OOTechSceneObject[] sceneObjectArray = UnityEngine.Object.FindObjectsByType<OOTechSceneObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (OOTechSceneObject sceneObject in sceneObjectArray)
        {
            if (sceneObject != null && sceneObject.RoleId == roleId)
                return sceneObject.gameObject;
        }

        OOTechStageMoranFreeMoveController[] moranControllerArray = UnityEngine.Object.FindObjectsByType<OOTechStageMoranFreeMoveController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (OOTechStageMoranFreeMoveController moranController in moranControllerArray)
        {
            if (moranController != null && moranController.name == objectName)
                return moranController.gameObject;
        }

        return null;
    }

    private static void InvokePrivate(object targetObject, string methodName, params object[] argumentArray)
    {
        MethodInfo methodInfo = targetObject.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Require(methodInfo != null, $"Private method not found: {targetObject.GetType().Name}.{methodName}");
        methodInfo.Invoke(targetObject, argumentArray);
    }

    private static T InvokePrivate<T>(object targetObject, string methodName, params object[] argumentArray)
    {
        MethodInfo methodInfo = targetObject.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Require(methodInfo != null, $"Private method not found: {targetObject.GetType().Name}.{methodName}");
        return (T)methodInfo.Invoke(targetObject, argumentArray);
    }

    private static T GetPrivateField<T>(object targetObject, string fieldName)
    {
        FieldInfo fieldInfo = targetObject.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Require(fieldInfo != null, $"Private field not found: {targetObject.GetType().Name}.{fieldName}");
        return (T)fieldInfo.GetValue(targetObject);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
