using TMPro;
using UnityEditor;
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
        isChanged |= RestoreSenarioMoran(senarioGroup, stageGroup);
        isChanged |= RemoveStagePlaceholderController(stageGroup);
        isChanged |= EnsureStage1Controller(stageGroup);
        isChanged |= RemoveStagePlaceholderCanvas(stageGroup);
        isChanged |= EnsureStageNameObject(stageGroup);
        return isChanged;
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
