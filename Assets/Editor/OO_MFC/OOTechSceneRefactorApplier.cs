using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class OOTechSceneRefactorApplier
{
    private const string ScenePath = "Assets/Scenes/OO_MFC.unity";

    private static readonly string[] RoadGroupNameArray =
    {
        "1st_Road_to_Stage1",
        "2nd_Road_to_Stage2",
        "3rd_Road_to_Stage3",
        "4th_Road_to_Stage4"
    };

    public static void ApplyRoadComponentSplit()
    {
        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(ScenePath);
        int addedCount = 0;

        foreach (string roadGroupName in RoadGroupNameArray)
        {
            GameObject roadGroupObject = FindSceneObject(roadGroupName);

            if (roadGroupObject == null)
            {
                Debug.LogWarning($"[OOTechSceneRefactorApplier] Road group missing: {roadGroupName}");
                continue;
            }

            OOTechRoadAutoMoveFailSafe autoMoveFailSafe = roadGroupObject.GetComponent<OOTechRoadAutoMoveFailSafe>();

            if (autoMoveFailSafe == null)
            {
                autoMoveFailSafe = roadGroupObject.AddComponent<OOTechRoadAutoMoveFailSafe>();
                addedCount++;
            }

            autoMoveFailSafe.enabled = false;

            OOTechTargetStageBGMPlayer targetStageBGMPlayer = roadGroupObject.GetComponent<OOTechTargetStageBGMPlayer>();

            if (targetStageBGMPlayer == null)
            {
                targetStageBGMPlayer = roadGroupObject.AddComponent<OOTechTargetStageBGMPlayer>();
                addedCount++;
            }

            OOTechRoadMoveInputReader moveInputReader = roadGroupObject.GetComponent<OOTechRoadMoveInputReader>();

            if (moveInputReader == null)
            {
                moveInputReader = roadGroupObject.AddComponent<OOTechRoadMoveInputReader>();
                addedCount++;
            }

            OOTechRoadToStage1Controller roadController = roadGroupObject.GetComponent<OOTechRoadToStage1Controller>();

            if (roadController != null)
                AssignRoadControllerReferences(roadController, moveInputReader, autoMoveFailSafe, targetStageBGMPlayer);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log($"[OOTechSceneRefactorApplier] Applied road component split. Added components: {addedCount}");
    }

    private static void AssignRoadControllerReferences(
        OOTechRoadToStage1Controller roadController,
        OOTechRoadMoveInputReader moveInputReader,
        OOTechRoadAutoMoveFailSafe autoMoveFailSafe,
        OOTechTargetStageBGMPlayer targetStageBGMPlayer)
    {
        SerializedObject serializedController = new SerializedObject(roadController);

        SerializedProperty moveInputProperty = serializedController.FindProperty("MoveInput_Reader");
        SerializedProperty autoMoveProperty = serializedController.FindProperty("AutoMove_FailSafe");
        SerializedProperty bgmPlayerProperty = serializedController.FindProperty("TargetStage_BGMPlayer");

        if (moveInputProperty != null)
            moveInputProperty.objectReferenceValue = moveInputReader;

        if (autoMoveProperty != null)
            autoMoveProperty.objectReferenceValue = autoMoveFailSafe;

        if (bgmPlayerProperty != null)
            bgmPlayerProperty.objectReferenceValue = targetStageBGMPlayer;

        serializedController.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject FindSceneObject(string objectName)
    {
        foreach (GameObject rootObject in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            GameObject foundObject = FindChildObject(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
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
}
