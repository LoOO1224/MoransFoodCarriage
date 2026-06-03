using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// OO_MFC 씬의 누락된 참조를 필요할 때만 수동으로 복구하는 에디터 도구입니다.
/// 자동 실행을 막아 사용자가 직접 배치한 UI와 오브젝트가 씬을 열 때마다 되돌아가지 않게 합니다.
/// </summary>
public static class OOTechOO_MFCSceneRepair
{
    private const string _targetSceneName = "OO_MFC";
    private const string _templateRoadGroupName = "1st_Road_to_Stage1";
    private const string _mrJaeikIdleClipPath = "Assets/Animation/Characters/Mr.Jaeik/Mr.Jaeik_Idle.anim";
    private const string _chunyangIdleClipPath = "Assets/Animation/Characters/ChunYang/Chunyang_Idle.anim";

    private static readonly RoadStageFlowData[] _roadStageFlowDataArray =
    {
        new RoadStageFlowData("1st_Road_to_Stage1", "Stage1Group"),
        new RoadStageFlowData("2nd_Road_to_Stage2", "Stage2Group"),
        new RoadStageFlowData("3rd_Road_to_Stage3", "Stage3Group"),
        new RoadStageFlowData("4th_Road_to_Stage4", "Stage4Group"),
        new RoadStageFlowData("Final_Road_to_FinalStage", "FinalStageGroup")
    };

    private static readonly StageNextFlowData[] _stageNextFlowDataArray =
    {
        new StageNextFlowData("Stage1Group", "2nd_Road_to_Stage2", "넘어가기"),
        new StageNextFlowData("Stage2Group", "3rd_Road_to_Stage3", "넘어가기"),
        new StageNextFlowData("Stage3Group", "4th_Road_to_Stage4", "넘어가기"),
        new StageNextFlowData("Stage4Group", "Final_Road_to_FinalStage", "넘어가기"),
        new StageNextFlowData("FinalStageGroup", "EpilogueGroup", "넘어가기")
    };

    private static readonly string[] _mainFlowGroupNameArray =
    {
        "MainMenuGroup",
        "CodexGroup",
        "Prologue1Group",
        "Prologue2Group",
        "Tutorial1Group",
        "Senario1Group",
        "WorldMapGroup",
        "1st_Road_to_Stage1",
        "2nd_Road_to_Stage2",
        "3rd_Road_to_Stage3",
        "4th_Road_to_Stage4",
        "Final_Road_to_FinalStage",
        "CookingGroup",
        "Stage1Group",
        "Stage2Group",
        "Stage3Group",
        "Stage4Group",
        "FinalStageGroup",
        "EpilogueGroup"
    };

    [MenuItem("Tools/OO MFC/Repair Scenario1 References")]
    private static void RepairActiveScene()
    {
        RepairScene(SceneManager.GetActiveScene());
    }

    [MenuItem("Tools/OO MFC/Frame Main Menu In Scene View")]
    private static void FrameMainMenuInSceneView()
    {
        FrameMainMenuInSceneView(SceneManager.GetActiveScene());
    }

    private static void RepairScene(Scene scene)
    {
        if (!scene.IsValid() || scene.name != _targetSceneName)
            return;

        bool isChanged = RenameLegacyRootGroup(scene, "Frist_Road_to_Stage1", "1st_Road_to_Stage1");
        isChanged |= OpenMainMenuIfNoFlowGroupActive(scene);
        isChanged |= RepairDialogueBackdrop(scene);
        isChanged |= RepairRoadStageRelay(scene);
        isChanged |= RepairTransparentScenarioCollider(scene);
        isChanged |= RepairEpilogue(scene);
        isChanged |= OOTechRoadHUDUIGroupEditorBuilder.RepairSceneHUDUIGroups(scene);

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            OOTechSenario1Controller[] controllerArray = rootObject.GetComponentsInChildren<OOTechSenario1Controller>(true);

            foreach (OOTechSenario1Controller controller in controllerArray)
                isChanged |= RepairSenario1Controller(controller);
        }

        if (isChanged)
            EditorSceneManager.MarkSceneDirty(scene);
    }

    private static bool OpenMainMenuIfNoFlowGroupActive(Scene scene)
    {
        GameObject mainMenuGroup = null;
        bool isFlowGroupActive = false;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            if (!IsMainFlowGroup(rootObject.name))
                continue;

            if (rootObject.name == "MainMenuGroup")
                mainMenuGroup = rootObject;

            if (rootObject.activeSelf)
                isFlowGroupActive = true;
        }

        if (isFlowGroupActive || mainMenuGroup == null)
            return false;

        mainMenuGroup.SetActive(true);
        return true;
    }

    private static bool IsMainFlowGroup(string objectName)
    {
        foreach (string flowGroupName in _mainFlowGroupNameArray)
        {
            if (flowGroupName == objectName)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Scene View is a free editor camera. This optional command seats the editor
    /// camera in front of MainMenuGroup for a quick director-monitor check.
    /// </summary>
    private static void FrameMainMenuInSceneView(Scene scene)
    {
        if (!scene.IsValid() || scene.name != _targetSceneName)
            return;

        GameObject mainMenuGroup = FindSceneObjectByName(scene, "MainMenuGroup");

        if (mainMenuGroup == null || !mainMenuGroup.activeSelf)
            return;

        Selection.activeGameObject = mainMenuGroup;

        if (SceneView.lastActiveSceneView == null)
            return;

        if (TryCalculateRectTransformBounds(mainMenuGroup, out Bounds bounds))
            SceneView.lastActiveSceneView.Frame(bounds, false);
        else
            SceneView.lastActiveSceneView.FrameSelected(false);

        SceneView.lastActiveSceneView.Repaint();
    }

    private static bool TryCalculateRectTransformBounds(GameObject rootObject, out Bounds bounds)
    {
        bounds = default;
        RectTransform[] rectTransformArray = rootObject.GetComponentsInChildren<RectTransform>(true);
        Vector3[] cornerArray = new Vector3[4];
        bool isInitialized = false;

        foreach (RectTransform rectTransform in rectTransformArray)
        {
            rectTransform.GetWorldCorners(cornerArray);

            foreach (Vector3 corner in cornerArray)
            {
                if (!isInitialized)
                {
                    bounds = new Bounds(corner, Vector3.zero);
                    isInitialized = true;
                    continue;
                }

                bounds.Encapsulate(corner);
            }
        }

        return isInitialized;
    }

    private static bool RepairSenario1Controller(OOTechSenario1Controller controller)
    {
        if (controller == null)
            return false;

        SerializedObject serializedController = new SerializedObject(controller);
        bool isChanged = false;

        isChanged |= AssignAnimationClipIfMissing(serializedController, "Clip_MrJaeikIdle", _mrJaeikIdleClipPath);
        isChanged |= AssignAnimationClipIfMissing(serializedController, "Clip_ChunyangIdle", _chunyangIdleClipPath);
        isChanged |= CorrectLegacyStringValue(serializedController, "_chunyangIdleStateName", "ChunYang_Idle", "Chunyang_Idle");

        if (isChanged)
            serializedController.ApplyModifiedPropertiesWithoutUndo();

        return isChanged;
    }

    private static bool RenameLegacyRootGroup(Scene scene, string legacyName, string newName)
    {
        GameObject legacyGroup = FindSceneObjectByName(scene, legacyName);

        if (legacyGroup == null)
            return false;

        legacyGroup.name = newName;
        return true;
    }

    private static bool RepairDialogueBackdrop(Scene scene)
    {
        GameObject dialoguePanel = FindSceneObjectByName(scene, "DialoguePanel");

        if (dialoguePanel == null || dialoguePanel.GetComponent<OOTechDialogueSpeakerNameBackdrop>() != null)
            return false;

        dialoguePanel.AddComponent<OOTechDialogueSpeakerNameBackdrop>();
        return true;
    }

    private static bool RepairRoadStageRelay(Scene scene)
    {
        bool isChanged = false;
        GameObject templateRoadGroup = FindSceneObjectByName(scene, _templateRoadGroupName);

        foreach (RoadStageFlowData flowData in _roadStageFlowDataArray)
        {
            GameObject roadGroup = EnsureRoadGroup(scene, flowData, templateRoadGroup, ref isChanged);

            if (roadGroup != null)
            {
                OOTechRoadToStage1Controller roadController = roadGroup.GetComponent<OOTechRoadToStage1Controller>();

                if (roadController == null)
                {
                    roadController = roadGroup.AddComponent<OOTechRoadToStage1Controller>();
                    isChanged = true;
                }

                SerializedObject serializedRoad = new SerializedObject(roadController);
                isChanged |= SetStringProperty(serializedRoad, "_currentGroupName", flowData.RoadGroupName);
                isChanged |= SetStringProperty(serializedRoad, "_targetStageGroupName", flowData.StageGroupName);
                serializedRoad.ApplyModifiedPropertiesWithoutUndo();

                if (roadGroup.GetComponent<OOTechRoadHUDController>() == null)
                {
                    roadGroup.AddComponent<OOTechRoadHUDController>();
                    isChanged = true;
                }

                if (roadGroup.GetComponent<OOTechTutorial2Controller>() == null)
                {
                    roadGroup.AddComponent<OOTechTutorial2Controller>();
                    isChanged = true;
                }
            }

            GameObject stageGroup = EnsureRootGroup(scene, flowData.StageGroupName, ref isChanged);
            isChanged |= RegisterUIGroup(scene, flowData.RoadGroupName, roadGroup);
            isChanged |= RegisterUIGroup(scene, flowData.StageGroupName, stageGroup);
        }

        foreach (StageNextFlowData flowData in _stageNextFlowDataArray)
        {
            GameObject stageGroup = EnsureRootGroup(scene, flowData.StageGroupName, ref isChanged);
            OOTechStagePlaceholderController stageController = stageGroup.GetComponent<OOTechStagePlaceholderController>();

            if (stageController == null)
            {
                stageController = stageGroup.AddComponent<OOTechStagePlaceholderController>();
                isChanged = true;
            }

            stageController.Configure(flowData.StageGroupName, flowData.NextGroupName, flowData.ButtonText);

            if (stageGroup.GetComponent<OOTechRoadHUDController>() == null)
            {
                stageGroup.AddComponent<OOTechRoadHUDController>();
                isChanged = true;
            }

            isChanged |= RegisterUIGroup(scene, flowData.StageGroupName, stageGroup);
        }

        return isChanged;
    }

    private static GameObject EnsureRoadGroup(Scene scene, RoadStageFlowData flowData, GameObject templateRoadGroup, ref bool isChanged)
    {
        GameObject roadGroup = FindSceneObjectByName(scene, flowData.RoadGroupName);

        if (roadGroup == null)
        {
            if (templateRoadGroup == null)
                return null;

            roadGroup = UnityEngine.Object.Instantiate(templateRoadGroup);
            roadGroup.name = flowData.RoadGroupName;
            roadGroup.SetActive(false);
            isChanged = true;
        }
        else if (templateRoadGroup != null && roadGroup != templateRoadGroup)
        {
            isChanged |= RestoreMissingRoadTemplateChildren(roadGroup, templateRoadGroup);
        }

        return roadGroup;
    }

    private static bool RestoreMissingRoadTemplateChildren(GameObject roadGroup, GameObject templateRoadGroup)
    {
        bool isChanged = false;

        for (int index = 0; index < templateRoadGroup.transform.childCount; index++)
        {
            Transform templateChild = templateRoadGroup.transform.GetChild(index);

            if (!ShouldCopyRoadTemplateChild(templateChild.name))
                continue;

            if (roadGroup.transform.Find(templateChild.name) != null)
                continue;

            GameObject copiedChild = UnityEngine.Object.Instantiate(templateChild.gameObject, roadGroup.transform);
            copiedChild.name = templateChild.name;
            isChanged = true;
        }

        return isChanged;
    }

    private static bool ShouldCopyRoadTemplateChild(string childName)
    {
        return childName != "RoadHUDCanvas" && childName != "HUDUIGroup" && childName != "RoadMapFadeCanvas";
    }

    private static bool RepairTransparentScenarioCollider(Scene scene)
    {
        bool isChanged = false;
        GameObject senario1Group = FindSceneObjectByName(scene, "Senario1Group");

        if (senario1Group == null)
            return false;

        Transform[] childTransformArray = senario1Group.GetComponentsInChildren<Transform>(true);

        foreach (Transform childTransform in childTransformArray)
        {
            if (childTransform == null || !childTransform.name.StartsWith("Coliider_"))
                continue;

            SpriteRenderer spriteRenderer = childTransform.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && spriteRenderer.enabled)
            {
                spriteRenderer.enabled = false;
                isChanged = true;
            }
        }

        return isChanged;
    }

    private static bool RepairEpilogue(Scene scene)
    {
        bool isChanged = false;
        GameObject epilogueGroup = EnsureRootGroup(scene, "EpilogueGroup", ref isChanged);
        OOTechEpiloguePlaceholderController epilogueController = epilogueGroup.GetComponent<OOTechEpiloguePlaceholderController>();

        if (epilogueController == null)
        {
            epilogueGroup.AddComponent<OOTechEpiloguePlaceholderController>();
            isChanged = true;
        }

        isChanged |= RegisterUIGroup(scene, "EpilogueGroup", epilogueGroup);
        return isChanged;
    }

    private static GameObject EnsureRootGroup(Scene scene, string groupName, ref bool isChanged)
    {
        GameObject groupObject = FindSceneObjectByName(scene, groupName);

        if (groupObject != null)
            return groupObject;

        groupObject = new GameObject(groupName);
        groupObject.SetActive(false);
        isChanged = true;
        return groupObject;
    }

    private static bool RegisterUIGroup(Scene scene, string groupName, GameObject groupObject)
    {
        if (string.IsNullOrEmpty(groupName) || groupObject == null)
            return false;

        OOTechUIManager uiManager = FindUIManager(scene);

        if (uiManager == null)
            return false;

        SerializedObject serializedUIManager = new SerializedObject(uiManager);
        SerializedProperty groupArrayProperty = serializedUIManager.FindProperty("UIGroup_InitialArray");

        if (groupArrayProperty == null || !groupArrayProperty.isArray)
            return false;

        for (int index = 0; index < groupArrayProperty.arraySize; index++)
        {
            SerializedProperty groupProperty = groupArrayProperty.GetArrayElementAtIndex(index);
            SerializedProperty nameProperty = groupProperty.FindPropertyRelative("Name");
            SerializedProperty objectProperty = groupProperty.FindPropertyRelative("Group");

            if (nameProperty == null || nameProperty.stringValue != groupName)
                continue;

            if (objectProperty == null || objectProperty.objectReferenceValue == groupObject)
                return false;

            objectProperty.objectReferenceValue = groupObject;
            serializedUIManager.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        int newIndex = groupArrayProperty.arraySize;
        groupArrayProperty.InsertArrayElementAtIndex(newIndex);
        SerializedProperty newGroupProperty = groupArrayProperty.GetArrayElementAtIndex(newIndex);
        newGroupProperty.FindPropertyRelative("Name").stringValue = groupName;
        newGroupProperty.FindPropertyRelative("Group").objectReferenceValue = groupObject;
        serializedUIManager.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }

    private static OOTechUIManager FindUIManager(Scene scene)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            OOTechUIManager uiManager = rootObject.GetComponentInChildren<OOTechUIManager>(true);

            if (uiManager != null)
                return uiManager;
        }

        return null;
    }

    private static bool SetStringProperty(SerializedObject serializedObject, string propertyName, string value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null || property.stringValue == value)
            return false;

        property.stringValue = value;
        return true;
    }

    private static GameObject FindSceneObjectByName(Scene scene, string objectName)
    {
        if (!scene.IsValid() || string.IsNullOrEmpty(objectName))
            return null;

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

    private static bool AssignAnimationClipIfMissing(SerializedObject serializedController, string propertyName, string assetPath)
    {
        SerializedProperty clipProperty = serializedController.FindProperty(propertyName);

        if (clipProperty == null || clipProperty.objectReferenceValue != null)
            return false;

        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);

        if (idleClip == null)
        {
            Debug.LogWarning($"[OOTechOO_MFCSceneRepair] Idle clip not found: {assetPath}");
            return false;
        }

        clipProperty.objectReferenceValue = idleClip;
        return true;
    }

    private static bool CorrectLegacyStringValue(SerializedObject serializedController, string propertyName, string legacyValue, string correctedValue)
    {
        SerializedProperty stringProperty = serializedController.FindProperty(propertyName);

        if (stringProperty == null || stringProperty.stringValue != legacyValue)
            return false;

        stringProperty.stringValue = correctedValue;
        return true;
    }

    private readonly struct RoadStageFlowData
    {
        public readonly string RoadGroupName;
        public readonly string StageGroupName;

        public RoadStageFlowData(string roadGroupName, string stageGroupName)
        {
            RoadGroupName = roadGroupName;
            StageGroupName = stageGroupName;
        }
    }

    private readonly struct StageNextFlowData
    {
        public readonly string StageGroupName;
        public readonly string NextGroupName;
        public readonly string ButtonText;

        public StageNextFlowData(string stageGroupName, string nextGroupName, string buttonText)
        {
            StageGroupName = stageGroupName;
            NextGroupName = nextGroupName;
            ButtonText = buttonText;
        }
    }
}
