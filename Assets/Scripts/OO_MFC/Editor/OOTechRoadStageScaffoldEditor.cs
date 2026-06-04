// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechRoadStageScaffoldEditor.cs
// - 역할: Unity Editor에서 수동으로 실행하는 씬 수리/스캐폴드 도구입니다.
// - 감독 관점: 공연 전에 무대를 정리하는 준비 도구이며, 실제 공연 중 배우가 쓰는 대본은 아닙니다.
// - 유지보수 포인트: 사용자 배치를 덮어쓸 수 있으므로 자동 실행하지 말고, 필요한 메뉴/배치모드에서만 실행합니다.
// =============================================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Editor-only stage builder for the repeated road-to-stage structure.
/// It duplicates the first road set as the template, assigns each duplicate its
/// destination stage, registers scene groups, and saves road prefabs for later reuse.
/// </summary>
public static class OOTechRoadStageScaffoldEditor
{
    private const string _prefabFolderPath = "Assets/Prefabs/OO_MFC/Groups/RoadGroups";
    private const string _templateRoadGroupName = "1st_Road_to_Stage1";

    private static readonly RoadStageScaffoldData[] _roadStageScaffoldDataArray =
    {
        new RoadStageScaffoldData("2nd_Road_to_Stage2", "Stage2Group"),
        new RoadStageScaffoldData("3rd_Road_to_Stage3", "Stage3Group"),
        new RoadStageScaffoldData("4th_Road_to_Stage4", "Stage4Group"),
        new RoadStageScaffoldData("Final_Road_to_FinalStage", "FinalStageGroup")
    };

    [MenuItem("Tools/OO MFC/Build Road Stage Skeletons")]
    private static void RequestBuildRoadStageSkeletons()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return;

        GameObject templateRoadGroup = FindSceneObjectByName(scene, _templateRoadGroupName);

        if (templateRoadGroup == null)
        {
            Debug.LogError($"[OOTechRoadStageScaffoldEditor] Template road group not found: {_templateRoadGroupName}");
            return;
        }

        EnsurePrefabFolder();

        foreach (RoadStageScaffoldData scaffoldData in _roadStageScaffoldDataArray)
        {
            GameObject roadGroup = EnsureRoadGroup(scene, templateRoadGroup, scaffoldData);
            GameObject stageGroup = EnsureStageGroup(scene, scaffoldData.StageGroupName);

            RegisterUIGroup(scaffoldData.RoadGroupName, roadGroup);
            RegisterUIGroup(scaffoldData.StageGroupName, stageGroup);
            SaveRoadPrefab(roadGroup);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[OOTechRoadStageScaffoldEditor] Road and stage skeletons are ready.");
    }

    private static GameObject EnsureRoadGroup(Scene scene, GameObject templateRoadGroup, RoadStageScaffoldData scaffoldData)
    {
        GameObject roadGroup = FindSceneObjectByName(scene, scaffoldData.RoadGroupName);

        if (roadGroup != null)
        {
            ConfigureRoadGroup(roadGroup, scaffoldData);
            return roadGroup;
        }

        roadGroup = Object.Instantiate(templateRoadGroup);
        roadGroup.name = scaffoldData.RoadGroupName;
        roadGroup.SetActive(false);
        Undo.RegisterCreatedObjectUndo(roadGroup, $"Create {scaffoldData.RoadGroupName}");
        ConfigureRoadGroup(roadGroup, scaffoldData);
        return roadGroup;
    }

    private static void ConfigureRoadGroup(GameObject roadGroup, RoadStageScaffoldData scaffoldData)
    {
        if (roadGroup == null)
            return;

        OOTechRoadToStage1Controller controller = roadGroup.GetComponent<OOTechRoadToStage1Controller>();

        if (controller == null)
            controller = roadGroup.AddComponent<OOTechRoadToStage1Controller>();

        SerializedObject serializedController = new SerializedObject(controller);
        SetStringProperty(serializedController, "_currentGroupName", scaffoldData.RoadGroupName);
        SetStringProperty(serializedController, "_targetStageGroupName", scaffoldData.StageGroupName);
        serializedController.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject EnsureStageGroup(Scene scene, string stageGroupName)
    {
        GameObject stageGroup = FindSceneObjectByName(scene, stageGroupName);

        if (stageGroup == null)
        {
            stageGroup = new GameObject(stageGroupName);
            stageGroup.SetActive(false);
            Undo.RegisterCreatedObjectUndo(stageGroup, $"Create {stageGroupName}");
        }

        CreateWhiteStageBackgroundIfNeeded(stageGroup);
        return stageGroup;
    }

    private static void CreateWhiteStageBackgroundIfNeeded(GameObject stageGroup)
    {
        if (stageGroup == null || stageGroup.transform.Find("Canvas_StageWhiteBackground") != null)
            return;

        GameObject canvasObject = new GameObject("Canvas_StageWhiteBackground", typeof(RectTransform));
        canvasObject.transform.SetParent(stageGroup.transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = -1000;

        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;

        GameObject imageObject = new GameObject("Image_WhiteStageBackground", typeof(RectTransform));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform imageRect = imageObject.transform as RectTransform;
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        Image image = imageObject.AddComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = false;
    }

    private static void RegisterUIGroup(string groupName, GameObject groupObject)
    {
        if (string.IsNullOrEmpty(groupName) || groupObject == null)
            return;

        OOTechUIManager uiManager = FindUIManager();

        if (uiManager == null)
            return;

        SerializedObject serializedUIManager = new SerializedObject(uiManager);
        SerializedProperty groupArrayProperty = serializedUIManager.FindProperty("UIGroup_InitialArray");

        if (groupArrayProperty == null || !groupArrayProperty.isArray)
            return;

        for (int index = 0; index < groupArrayProperty.arraySize; index++)
        {
            SerializedProperty groupProperty = groupArrayProperty.GetArrayElementAtIndex(index);
            SerializedProperty nameProperty = groupProperty.FindPropertyRelative("Name");
            SerializedProperty objectProperty = groupProperty.FindPropertyRelative("Group");

            if (nameProperty == null || nameProperty.stringValue != groupName)
                continue;

            if (objectProperty != null)
                objectProperty.objectReferenceValue = groupObject;

            serializedUIManager.ApplyModifiedPropertiesWithoutUndo();
            return;
        }

        int newIndex = groupArrayProperty.arraySize;
        groupArrayProperty.InsertArrayElementAtIndex(newIndex);

        SerializedProperty newGroupProperty = groupArrayProperty.GetArrayElementAtIndex(newIndex);
        newGroupProperty.FindPropertyRelative("Name").stringValue = groupName;
        newGroupProperty.FindPropertyRelative("Group").objectReferenceValue = groupObject;
        serializedUIManager.ApplyModifiedPropertiesWithoutUndo();
    }

    private static OOTechUIManager FindUIManager()
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            OOTechUIManager uiManager = rootObject.GetComponentInChildren<OOTechUIManager>(true);

            if (uiManager != null)
                return uiManager;
        }

        return null;
    }

    private static void SaveRoadPrefab(GameObject roadGroup)
    {
        if (roadGroup == null)
            return;

        string prefabPath = $"{_prefabFolderPath}/{roadGroup.name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(roadGroup, prefabPath);
    }

    private static void EnsurePrefabFolder()
    {
        string[] folderPartArray = _prefabFolderPath.Split('/');
        string currentPath = folderPartArray[0];

        for (int index = 1; index < folderPartArray.Length; index++)
        {
            string nextPath = $"{currentPath}/{folderPartArray[index]}";

            if (!AssetDatabase.IsValidFolder(nextPath))
                AssetDatabase.CreateFolder(currentPath, folderPartArray[index]);

            currentPath = nextPath;
        }
    }

    private static void SetStringProperty(SerializedObject serializedObject, string propertyName, string value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property != null)
            property.stringValue = value;
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

    private readonly struct RoadStageScaffoldData
    {
        public readonly string RoadGroupName;
        public readonly string StageGroupName;

        public RoadStageScaffoldData(string roadGroupName, string stageGroupName)
        {
            RoadGroupName = roadGroupName;
            StageGroupName = stageGroupName;
        }
    }
}
