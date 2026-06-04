// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechStage1SceneRepairEditor.cs
// - 역할: Unity Editor에서 수동으로 실행하는 씬 수리/스캐폴드 도구입니다.
// - 감독 관점: 공연 전에 무대를 정리하는 준비 도구이며, 실제 공연 중 배우가 쓰는 대본은 아닙니다.
// - 유지보수 포인트: 사용자 배치를 덮어쓸 수 있으므로 자동 실행하지 말고, 필요한 메뉴/배치모드에서만 실행합니다.
// =============================================================================
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Stage1Group 1차 플레이에 필요한 씬 배치를 수동으로 복구하는 에디터 도구입니다.
/// 사용자가 배치한 Stage1Group의 Moran은 유지하고, Senario1Group에는 별도 복사본만 되돌립니다.
/// </summary>
public static class OOTechStage1SceneRepairEditor
{
    private const string _scenePath = "Assets/Scenes/OO_MFC.unity";

    [MenuItem("Tools/OO MFC/Repair Stage1 Scene")]
    public static void RequestRepairStage1Scene()
    {
        Scene scene = EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Single);
        bool isChanged = RepairScene(scene);

        if (isChanged)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log("[OOTechStage1SceneRepairEditor] Stage1 scene repair complete.");
    }

    public static void RequestRepairStage1SceneBatch()
    {
        RequestRepairStage1Scene();
        EditorApplication.Exit(0);
    }

    [MenuItem("Tools/OO MFC/Repair Scene Group Boundaries")]
    public static void RequestRepairSceneGroupBoundaries()
    {
        Scene scene = EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Single);
        bool isChanged = RepairSceneGroupBoundaries(scene);

        if (isChanged)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("[OOTechStage1SceneRepairEditor] Scene group boundary repair complete.");
    }

    public static void RequestRepairSceneGroupBoundariesBatch()
    {
        RequestRepairSceneGroupBoundaries();
        EditorApplication.Exit(0);
    }

    private static bool RepairScene(Scene scene)
    {
        GameObject senarioGroup = FindRootObject(scene, "Senario1Group");
        GameObject stageGroup = FindRootObject(scene, "Stage1Group");

        if (senarioGroup == null || stageGroup == null)
        {
            Debug.LogWarning("[OOTechStage1SceneRepairEditor] Senario1Group or Stage1Group not found.");
            return false;
        }

        bool isChanged = false;
        isChanged |= EnsureMainCamera2D(scene);
        isChanged |= EnsureItemDefinitionSprites(scene);
        isChanged |= RestoreSenarioMoran(senarioGroup, stageGroup);
        isChanged |= RemoveStagePlaceholderController(stageGroup);
        isChanged |= EnsureStage1Controller(stageGroup);
        isChanged |= RemoveStagePlaceholderCanvas(stageGroup);
        isChanged |= EnsureStageNameObject(stageGroup);
        isChanged |= EnsureStage1BackgroundSprites(stageGroup);
        isChanged |= EnsureStage1MoranAnimator(stageGroup);
        isChanged |= EnsureStage1InitialMapState(stageGroup);
        isChanged |= EnsureSenarioMoranIdle(senarioGroup);
        isChanged |= EnsureRoadDialogueLayout(scene);
        return isChanged;
    }

    private static bool EnsureMainCamera2D(Scene scene)
    {
        Camera targetCamera = null;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            Camera[] cameraArray = rootObject.GetComponentsInChildren<Camera>(true);

            foreach (Camera camera in cameraArray)
            {
                if (camera == null)
                    continue;

                if (camera.CompareTag("MainCamera"))
                {
                    targetCamera = camera;
                    break;
                }

                if (targetCamera == null)
                    targetCamera = camera;
            }

            if (targetCamera != null && targetCamera.CompareTag("MainCamera"))
                break;
        }

        if (targetCamera == null)
            return false;

        bool isChanged = false;

        if (!targetCamera.orthographic)
        {
            targetCamera.orthographic = true;
            isChanged = true;
        }

        if (targetCamera.clearFlags != CameraClearFlags.SolidColor)
        {
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            isChanged = true;
        }

        if (targetCamera.backgroundColor != Color.black)
        {
            targetCamera.backgroundColor = Color.black;
            isChanged = true;
        }

        if (isChanged)
            EditorUtility.SetDirty(targetCamera);

        return isChanged;
    }

    private static bool EnsureItemDefinitionSprites(Scene scene)
    {
        bool isChanged = false;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            OOTechItemDefinitionObject[] definitionArray = rootObject.GetComponentsInChildren<OOTechItemDefinitionObject>(true);

            foreach (OOTechItemDefinitionObject definitionObject in definitionArray)
            {
                if (definitionObject == null)
                    continue;

                SerializedObject serializedDefinition = new SerializedObject(definitionObject);
                SerializedProperty itemDataIdProperty = serializedDefinition.FindProperty("_itemDataId");
                SerializedProperty spriteIconProperty = serializedDefinition.FindProperty("Sprite_Icon");

                if (itemDataIdProperty == null || spriteIconProperty == null)
                    continue;

                Sprite iconSprite = LoadKnownItemSprite(itemDataIdProperty.stringValue);

                if (iconSprite == null || spriteIconProperty.objectReferenceValue == iconSprite)
                    continue;

                spriteIconProperty.objectReferenceValue = iconSprite;
                serializedDefinition.ApplyModifiedProperties();
                definitionObject.RequestRefreshPreview();
                EditorUtility.SetDirty(definitionObject);
                isChanged = true;
            }
        }

        return isChanged;
    }

    private static Sprite LoadKnownItemSprite(string itemDataId)
    {
        string spritePath = string.Empty;

        if (itemDataId == "Ing_Rice_01")
            spritePath = "Assets/Images/Food/Rice.png";
        else if (itemDataId == "Ing_Veggie_01")
            spritePath = "Assets/Images/Food/Vegetable.png";
        else if (itemDataId == "Ing_Pumpkin_01")
            spritePath = "Assets/Images/Food/Pumpkin.png";
        else if (itemDataId == "OO_VegetableSoup_1")
            spritePath = "Assets/Images/Food/VegetableSoup.png";

        return string.IsNullOrEmpty(spritePath) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
    }

    private static bool RestoreSenarioMoran(GameObject senarioGroup, GameObject stageGroup)
    {
        GameObject senarioMoran = FindChildByName(senarioGroup.transform, "Moran");

        if (senarioMoran != null)
            return false;

        GameObject stageMoran = FindChildByName(stageGroup.transform, "Moran");

        if (stageMoran == null)
        {
            Debug.LogWarning("[OOTechStage1SceneRepairEditor] Stage1Group Moran not found. Cannot restore Senario1Group Moran.");
            return false;
        }

        GameObject restoredMoran = Object.Instantiate(stageMoran, senarioGroup.transform);
        restoredMoran.name = "Moran";
        restoredMoran.transform.localPosition = new Vector3(4.4f, -1.2f, 0f);
        restoredMoran.transform.localRotation = Quaternion.identity;
        restoredMoran.transform.localScale = stageMoran.transform.localScale;
        restoredMoran.SetActive(false);

        OOTechSceneObject sceneObject = restoredMoran.GetComponent<OOTechSceneObject>();

        if (sceneObject == null)
            sceneObject = restoredMoran.AddComponent<OOTechSceneObject>();

        sceneObject.RequestSetRoleId("Moran");

        Animator animator = restoredMoran.GetComponentInChildren<Animator>(true);

        if (animator != null)
            animator.Play("Moran_Idle", 0, 0f);

        return true;
    }

    private static bool EnsureStage1Controller(GameObject stageGroup)
    {
        bool isChanged = false;

        OOTechStage1GroupController controller = stageGroup.GetComponent<OOTechStage1GroupController>();

        if (controller == null)
        {
            stageGroup.AddComponent<OOTechStage1GroupController>();
            isChanged = true;
        }

        if (stageGroup.GetComponent<OOTechRoadHUDController>() == null)
        {
            stageGroup.AddComponent<OOTechRoadHUDController>();
            isChanged = true;
        }

        return isChanged;
    }

    private static bool RemoveStagePlaceholderController(GameObject stageGroup)
    {
        OOTechStagePlaceholderController placeholderController = stageGroup.GetComponent<OOTechStagePlaceholderController>();

        if (placeholderController == null)
            return false;

        Object.DestroyImmediate(placeholderController);
        return true;
    }

    private static bool RemoveStagePlaceholderCanvas(GameObject stageGroup)
    {
        GameObject placeholderCanvas = FindChildByName(stageGroup.transform, "Canvas_StagePlaceholder");

        if (placeholderCanvas == null)
            return false;

        Object.DestroyImmediate(placeholderCanvas);
        return true;
    }

    private static bool EnsureStageNameObject(GameObject stageGroup)
    {
        GameObject stageNameObject = FindChildByName(stageGroup.transform, "StageName");

        if (stageNameObject != null && stageNameObject.GetComponentInChildren<TextMeshProUGUI>(true) != null)
            return false;

        if (stageNameObject == null)
        {
            stageNameObject = new GameObject("StageName");
            stageNameObject.transform.SetParent(stageGroup.transform, false);
            stageNameObject.transform.localPosition = new Vector3(0f, 3.6f, 0f);
        }

        GameObject textObject = new GameObject("Text_StageName");
        textObject.transform.SetParent(stageNameObject.transform, false);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = "동쪽 마을";
        text.fontSize = 72f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        RectTransform rectTransform = textObject.transform as RectTransform;
        rectTransform.sizeDelta = new Vector2(900f, 140f);

        Canvas canvas = stageNameObject.GetComponent<Canvas>();

        if (canvas == null)
            canvas = stageNameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1600;

        if (stageNameObject.GetComponent<UnityEngine.UI.CanvasScaler>() == null)
            stageNameObject.AddComponent<UnityEngine.UI.CanvasScaler>();

        if (stageNameObject.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            stageNameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        stageNameObject.SetActive(true);
        return true;
    }

    private static bool EnsureStage1BackgroundSprites(GameObject stageGroup)
    {
        bool isChanged = false;
        isChanged |= AssignSpriteRendererSprite(stageGroup, "Stage1-1", "Assets/Images/Stages/Stage1-1.jpg");
        isChanged |= AssignSpriteRendererSprite(stageGroup, "Stage1-2", "Assets/Images/Stages/Stage1-2.jpg");
        return isChanged;
    }

    private static bool EnsureStage1MoranAnimator(GameObject stageGroup)
    {
        GameObject moranObject = FindChildByName(stageGroup.transform, "Moran");
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animation/Characters/Moran/Stage1Group.controller");
        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animation/Characters/Moran/Moran_Idle.anim");
        AnimationClip walkingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animation/Characters/Moran/Moran_isWalking.anim");
        AnimationClip runningClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animation/Characters/Moran/Moran_isRunning.anim");

        if (moranObject == null || controller == null)
            return false;

        bool isChanged = false;
        isChanged |= EnsureAnimatorState(controller, "Moran_idle", "Moran_Idle", idleClip, new Vector3(200f, 0f, 0f), true);
        isChanged |= EnsureAnimatorState(controller, "Moran_isWalking", "Moran_isWalkinganim", walkingClip, new Vector3(240f, 70f, 0f), false);
        isChanged |= EnsureAnimatorState(controller, "Moran_isRunning", string.Empty, runningClip, new Vector3(280f, 140f, 0f), false);

        Animator animator = moranObject.GetComponentInChildren<Animator>(true);

        if (animator == null)
        {
            animator = moranObject.AddComponent<Animator>();
            isChanged = true;
        }

        if (animator.runtimeAnimatorController != controller)
        {
            animator.runtimeAnimatorController = controller;
            isChanged = true;
        }

        if (!animator.enabled)
        {
            animator.enabled = true;
            isChanged = true;
        }

        if (isChanged)
        {
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(animator);
            EditorUtility.SetDirty(moranObject);
        }

        return isChanged;
    }

    private static bool EnsureAnimatorState(AnimatorController controller, string stateName, string legacyStateName, AnimationClip clip, Vector3 position, bool isDefault)
    {
        if (controller.layers == null || controller.layers.Length == 0)
            return false;

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

        if (stateMachine == null)
            return false;

        AnimatorState state = FindAnimatorState(stateMachine, stateName);

        if (state == null && !string.IsNullOrEmpty(legacyStateName))
            state = FindAnimatorState(stateMachine, legacyStateName);

        bool isChanged = false;

        if (state == null)
        {
            state = stateMachine.AddState(stateName, position);
            isChanged = true;
        }

        if (state.name != stateName)
        {
            state.name = stateName;
            isChanged = true;
        }

        if (clip != null && state.motion != clip)
        {
            state.motion = clip;
            isChanged = true;
        }

        if (isDefault && stateMachine.defaultState != state)
        {
            stateMachine.defaultState = state;
            isChanged = true;
        }

        if (isChanged)
            EditorUtility.SetDirty(state);

        return isChanged;
    }

    private static AnimatorState FindAnimatorState(AnimatorStateMachine stateMachine, string stateName)
    {
        if (stateMachine == null || string.IsNullOrEmpty(stateName))
            return null;

        ChildAnimatorState[] childStateArray = stateMachine.states;

        foreach (ChildAnimatorState childState in childStateArray)
        {
            if (childState.state != null && childState.state.name == stateName)
                return childState.state;
        }

        return null;
    }

    private static bool EnsureStage1InitialMapState(GameObject stageGroup)
    {
        GameObject stageMap1 = FindChildByName(stageGroup.transform, "Stage1-1");
        GameObject stageMap2 = FindChildByName(stageGroup.transform, "Stage1-2");
        bool isChanged = false;

        if (stageMap1 != null && !stageMap1.activeSelf)
        {
            stageMap1.SetActive(true);
            isChanged = true;
        }

        if (stageMap2 != null && stageMap2.activeSelf)
        {
            stageMap2.SetActive(false);
            isChanged = true;
        }

        return isChanged;
    }

    private static bool AssignSpriteRendererSprite(GameObject rootObject, string objectName, string spritePath)
    {
        GameObject targetObject = FindChildByName(rootObject.transform, objectName);
        Sprite targetSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

        if (targetObject == null || targetSprite == null)
            return false;

        SpriteRenderer spriteRenderer = targetObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = targetObject.AddComponent<SpriteRenderer>();

        bool isChanged = false;

        if (spriteRenderer.sprite != targetSprite)
        {
            spriteRenderer.sprite = targetSprite;
            isChanged = true;
        }

        if (spriteRenderer.sortingOrder != -1)
        {
            spriteRenderer.sortingOrder = -1;
            isChanged = true;
        }

        if (isChanged)
            EditorUtility.SetDirty(targetObject);

        return isChanged;
    }

    private static bool EnsureSenarioMoranIdle(GameObject senarioGroup)
    {
        GameObject moranObject = FindChildByName(senarioGroup.transform, "Moran");

        if (moranObject == null)
            return false;

        Animator animator = moranObject.GetComponentInChildren<Animator>(true);
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Images/Characters/Moran/Senario1Group.controller");

        if (controller == null)
            return false;

        if (animator == null)
            animator = moranObject.AddComponent<Animator>();

        bool isChanged = false;

        if (animator.runtimeAnimatorController != controller)
        {
            animator.runtimeAnimatorController = controller;
            isChanged = true;
        }

        if (!animator.enabled)
        {
            animator.enabled = true;
            isChanged = true;
        }

        if (animator.gameObject.activeInHierarchy)
            animator.Play("Moran_Idle", 0, 0f);

        EditorUtility.SetDirty(animator);
        return isChanged;
    }

    private static bool EnsureRoadDialogueLayout(Scene scene)
    {
        GameObject dialoguePanel = FindSceneObjectByName(scene, "DialoguePanel");

        if (dialoguePanel == null)
            return false;

        OOTechDialogueLayout dialogueLayout = dialoguePanel.GetComponent<OOTechDialogueLayout>();

        if (dialogueLayout == null)
            dialogueLayout = dialoguePanel.AddComponent<OOTechDialogueLayout>();

        SerializedObject serializedLayout = new SerializedObject(dialogueLayout);
        SerializedProperty roadPositionProperty = serializedLayout.FindProperty("_roadViewPanelAnchoredPosition");

        if (roadPositionProperty == null)
            return false;

        Vector2 targetPosition = new Vector2(0f, 155f);

        if (roadPositionProperty.vector2Value == targetPosition)
            return false;

        roadPositionProperty.vector2Value = targetPosition;
        serializedLayout.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(dialogueLayout);
        return true;
    }

    private static bool RepairSceneGroupBoundaries(Scene scene)
    {
        GameObject tutorialGroup = FindRootObject(scene, "Tutorial1Group");
        GameObject senarioGroup = FindRootObject(scene, "Senario1Group");

        if (tutorialGroup == null || senarioGroup == null)
        {
            Debug.LogWarning("[OOTechStage1SceneRepairEditor] Tutorial1Group or Senario1Group not found.");
            return false;
        }

        bool isChanged = false;
        isChanged |= RestoreSenarioObjectFromTutorial(tutorialGroup, senarioGroup, "Jaeik");
        isChanged |= RestoreSenarioObjectFromTutorial(tutorialGroup, senarioGroup, "S1_Object_1");
        return isChanged;
    }

    private static bool RestoreSenarioObjectFromTutorial(GameObject tutorialGroup, GameObject senarioGroup, string objectName)
    {
        GameObject wrongObject = FindDirectChildByName(tutorialGroup.transform, objectName);

        if (wrongObject == null)
            return false;

        GameObject existingSenarioObject = FindChildByName(senarioGroup.transform, objectName);

        if (existingSenarioObject != null)
        {
            Object.DestroyImmediate(wrongObject);
            Debug.Log($"[OOTechStage1SceneRepairEditor] Removed duplicated {objectName} from Tutorial1Group.");
            return true;
        }

        wrongObject.transform.SetParent(senarioGroup.transform, true);
        Debug.Log($"[OOTechStage1SceneRepairEditor] Restored {objectName} from Tutorial1Group to Senario1Group.");
        return true;
    }

    private static GameObject FindRootObject(Scene scene, string objectName)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            if (rootObject.name == objectName)
                return rootObject;
        }

        return null;
    }

    private static GameObject FindSceneObjectByName(Scene scene, string objectName)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = FindChildByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
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

    private static GameObject FindDirectChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
            return null;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform childTransform = rootTransform.GetChild(index);

            if (childTransform.name == objectName)
                return childTransform.gameObject;
        }

        return null;
    }
}
