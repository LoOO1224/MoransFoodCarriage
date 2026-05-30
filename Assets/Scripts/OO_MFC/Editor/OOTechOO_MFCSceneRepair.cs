using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Repairs only missing OO_MFC scene references after instructor sample files are imported.
/// Existing inspector assignments are preserved.
/// </summary>
[InitializeOnLoad]
public static class OOTechOO_MFCSceneRepair
{
    private const string _targetSceneName = "OO_MFC";
    private const string _mrJaeikIdleClipPath = "Assets/Animation/Characters/Mr.Jaeik/Mr.Jaeik_Idle.anim";
    private const string _chunyangIdleClipPath = "Assets/Animation/Characters/ChunYang/Chunyang_Idle.anim";
    private static readonly string[] _mainFlowGroupNameArray =
    {
        "MainMenuGroup",
        "CodexGroup",
        "Prologue1Group",
        "Prologue2Group",
        "Tutorial1Group",
        "Senario1Group",
        "WorldMapGroup"
    };

    static OOTechOO_MFCSceneRepair()
    {
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.delayCall -= RepairActiveScene;
        EditorApplication.delayCall += RepairActiveScene;
        EditorApplication.delayCall -= FrameActiveMainMenuInSceneView;
        EditorApplication.delayCall += FrameActiveMainMenuInSceneView;
    }

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

    private static void FrameActiveMainMenuInSceneView()
    {
        FrameMainMenuInSceneView(SceneManager.GetActiveScene());
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode openSceneMode)
    {
        RepairScene(scene);
        EditorApplication.delayCall += () => FrameMainMenuInSceneView(scene);
    }

    private static void RepairScene(Scene scene)
    {
        if (!scene.IsValid() || scene.name != _targetSceneName)
            return;

        bool isChanged = OpenMainMenuIfNoFlowGroupActive(scene);

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
    /// Scene View is a free editor camera. This command seats the editor camera in front of
    /// the main-menu stage so the director can compare the composition with Game View quickly.
    /// </summary>
    private static void FrameMainMenuInSceneView(Scene scene)
    {
        if (!scene.IsValid() || scene.name != _targetSceneName)
            return;

        GameObject mainMenuGroup = null;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            if (rootObject.name == "MainMenuGroup")
            {
                mainMenuGroup = rootObject;
                break;
            }
        }

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
}
