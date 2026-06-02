using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 아이템 데이터 ID를 가진 실제 하이어라키 오브젝트와 프리팹을 준비하는 에디터 전용 소품팀 도구입니다.
/// 감독은 생성된 ItemObjectLibraryGroup과 Prefabs/OO_MFC/Props/Items 안에서 이미지를 직접 교체할 수 있습니다.
/// </summary>
public static class OOTechItemObjectScaffoldEditor
{
    private const string _scenePath = "Assets/Scenes/OO_MFC.unity";
    private const string _firstRoadGroupName = "1st_Road_to_Stage1";
    private const string _itemLibraryGroupName = "ItemObjectLibraryGroup";
    private const string _itemCatalogManagerName = "ItemCatalogManager";
    private const string _cookingManagerName = "CookingManager";
    private const string _itemPrefabFolderPath = "Assets/Prefabs/OO_MFC/Props/Items";
    private const string _itemIconResourceFolderPath = "Assets/Resources/OO_MFC/Items";

    private static readonly ItemDefinitionScaffoldData[] _itemDefinitionDataArray =
    {
        new ItemDefinitionScaffoldData("Item_Rice_01", "Ing_Rice_01", "쌀", "OO_MFC/Items/Ing_Rice_01", new Vector3(-2f, -2.6f, 0f)),
        new ItemDefinitionScaffoldData("Item_Vegetable_01", "Ing_Pumpkin_01", "채소", "OO_MFC/Items/Ing_Pumpkin_01", new Vector3(-1.2f, -2.6f, 0f)),
        new ItemDefinitionScaffoldData("Item_VegetablePorridge_01", "OO_Cook_1", "채소죽", "OO_MFC/Items/OO_Cook_1", new Vector3(-0.4f, -2.6f, 0f))
    };

    private static readonly string[] _roadGroupNameArray =
    {
        "1st_Road_to_Stage1",
        "2nd_Road_to_Stage2",
        "3rd_Road_to_Stage3",
        "4th_Road_to_Stage4",
        "Final_Road_to_FinalStage"
    };

    [MenuItem("Tools/OO MFC/Build Item Object Library")]
    public static void RequestBuildItemObjectLibrary()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return;

        bool isChanged = BuildItemObjectLibrary(scene);

        if (isChanged)
            EditorSceneManager.MarkSceneDirty(scene);
    }

    /// <summary>
    /// Unity batchmode에서 호출하는 진입점입니다.
    /// </summary>
    public static void RequestBuildItemObjectLibraryBatch()
    {
        Scene scene = EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Single);
        bool isChanged = BuildItemObjectLibrary(scene);

        if (isChanged)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[OOTechItemObjectScaffoldEditor] Item object library build complete.");
    }

    private static bool BuildItemObjectLibrary(Scene scene)
    {
        if (!scene.IsValid())
            return false;

        bool isChanged = false;
        EnsurePrefabFolder(_itemPrefabFolderPath);
        EnsurePrefabFolder(_itemIconResourceFolderPath);
        isChanged |= EnsureItemCatalogManager(scene);
        isChanged |= EnsureCookingManager(scene);
        GameObject itemLibraryGroup = EnsureItemLibraryGroup(scene, ref isChanged);

        foreach (ItemDefinitionScaffoldData data in _itemDefinitionDataArray)
        {
            GameObject itemObject = EnsureItemDefinitionObject(itemLibraryGroup.transform, data, ref isChanged);
            SaveItemPrefab(itemObject, data);
        }

        foreach (string roadGroupName in _roadGroupNameArray)
            isChanged |= EnsureMFCStartPoint(scene, roadGroupName);

        isChanged |= OOTechRoadHUDUIGroupEditorBuilder.RepairSceneHUDUIGroups(scene);
        return isChanged;
    }

    private static bool EnsureItemCatalogManager(Scene scene)
    {
        GameObject managerObject = FindSceneObjectByName(scene, _itemCatalogManagerName);

        if (managerObject == null)
        {
            managerObject = new GameObject(_itemCatalogManagerName);
            SceneManager.MoveGameObjectToScene(managerObject, scene);
        }

        if (managerObject.GetComponent<OOTechItemCatalogManager>() != null)
            return false;

        managerObject.AddComponent<OOTechItemCatalogManager>();
        EditorUtility.SetDirty(managerObject);
        return true;
    }

    private static bool EnsureCookingManager(Scene scene)
    {
        GameObject managerObject = FindSceneObjectByName(scene, _cookingManagerName);

        if (managerObject == null)
        {
            managerObject = new GameObject(_cookingManagerName);
            SceneManager.MoveGameObjectToScene(managerObject, scene);
        }

        if (managerObject.GetComponent<OOTechCookingManager>() != null)
            return false;

        managerObject.AddComponent<OOTechCookingManager>();
        EditorUtility.SetDirty(managerObject);
        return true;
    }

    private static GameObject EnsureItemLibraryGroup(Scene scene, ref bool isChanged)
    {
        GameObject firstRoadGroup = FindSceneObjectByName(scene, _firstRoadGroupName);
        Transform parentTransform = firstRoadGroup != null ? firstRoadGroup.transform : null;
        GameObject itemLibraryGroup = parentTransform != null ? FindDirectChild(parentTransform, _itemLibraryGroupName) : FindSceneObjectByName(scene, _itemLibraryGroupName);

        if (itemLibraryGroup != null)
            return itemLibraryGroup;

        itemLibraryGroup = new GameObject(_itemLibraryGroupName);

        if (parentTransform != null)
            itemLibraryGroup.transform.SetParent(parentTransform, false);
        else
            SceneManager.MoveGameObjectToScene(itemLibraryGroup, scene);

        itemLibraryGroup.transform.localPosition = Vector3.zero;
        isChanged = true;
        return itemLibraryGroup;
    }

    private static GameObject EnsureItemDefinitionObject(Transform parentTransform, ItemDefinitionScaffoldData data, ref bool isChanged)
    {
        GameObject itemObject = FindDirectChild(parentTransform, data.ObjectName);

        if (itemObject == null)
        {
            itemObject = new GameObject(data.ObjectName);
            itemObject.transform.SetParent(parentTransform, false);
            itemObject.transform.localPosition = data.LocalPosition;
            itemObject.SetActive(false);
            isChanged = true;
        }

        SpriteRenderer spriteRenderer = itemObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = itemObject.AddComponent<SpriteRenderer>();
            isChanged = true;
        }

        OOTechItemDefinitionObject definitionObject = itemObject.GetComponent<OOTechItemDefinitionObject>();

        if (definitionObject == null)
        {
            definitionObject = itemObject.AddComponent<OOTechItemDefinitionObject>();
            isChanged = true;
        }

        definitionObject.RequestSetupDefinition(data.ItemDataId, data.DisplayName, data.IconResourcePath);
        EditorUtility.SetDirty(definitionObject);
        EditorUtility.SetDirty(itemObject);
        return itemObject;
    }

    private static bool EnsureMFCStartPoint(Scene scene, string roadGroupName)
    {
        GameObject roadGroup = FindSceneObjectByName(scene, roadGroupName);

        if (roadGroup == null)
            return false;

        bool isChanged = false;
        GameObject startPointObject = FindDirectChild(roadGroup.transform, "MFC_StartPoint");
        GameObject mfcObject = FindChildByName(roadGroup.transform, "MFC");

        if (startPointObject == null)
        {
            startPointObject = new GameObject("MFC_StartPoint");
            startPointObject.transform.SetParent(roadGroup.transform, false);
            startPointObject.transform.position = mfcObject != null ? mfcObject.transform.position : roadGroup.transform.position;
            isChanged = true;
        }

        OOTechSceneObject sceneObject = startPointObject.GetComponent<OOTechSceneObject>();

        if (sceneObject == null)
        {
            sceneObject = startPointObject.AddComponent<OOTechSceneObject>();
            isChanged = true;
        }

        SerializedObject serializedSceneObject = new SerializedObject(sceneObject);
        isChanged |= SetStringProperty(serializedSceneObject, "_roleId", "MFCStartPoint");

        OOTechRoadToStage1Controller roadController = roadGroup.GetComponent<OOTechRoadToStage1Controller>();

        if (roadController != null)
        {
            SerializedObject serializedController = new SerializedObject(roadController);
            isChanged |= SetObjectProperty(serializedController, "Transform_MFCStartPoint", startPointObject.transform);
            isChanged |= SetBoolProperty(serializedController, "_isUseMFCStartPointOnFirstMap", true);
        }

        if (isChanged)
            EditorUtility.SetDirty(startPointObject);

        return isChanged;
    }

    private static void SaveItemPrefab(GameObject itemObject, ItemDefinitionScaffoldData data)
    {
        if (itemObject == null)
            return;

        string prefabPath = $"{_itemPrefabFolderPath}/{data.ObjectName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(itemObject, prefabPath);
    }

    private static void EnsurePrefabFolder(string folderPath)
    {
        string[] folderPartArray = folderPath.Split('/');
        string currentPath = folderPartArray[0];

        for (int index = 1; index < folderPartArray.Length; index++)
        {
            string nextPath = $"{currentPath}/{folderPartArray[index]}";

            if (!AssetDatabase.IsValidFolder(nextPath))
                AssetDatabase.CreateFolder(currentPath, folderPartArray[index]);

            currentPath = nextPath;
        }
    }

    private static bool SetStringProperty(SerializedObject serializedObject, string propertyName, string value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
            return false;

        if (property.stringValue == value)
            return false;

        property.stringValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }

    private static bool SetBoolProperty(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
            return false;

        if (property.boolValue == value)
            return false;

        property.boolValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }

    private static bool SetObjectProperty(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
            return false;

        if (property.objectReferenceValue == value)
            return false;

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        return true;
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

    private static GameObject FindDirectChild(Transform parentTransform, string childName)
    {
        if (parentTransform == null)
            return null;

        for (int index = 0; index < parentTransform.childCount; index++)
        {
            Transform childTransform = parentTransform.GetChild(index);

            if (childTransform.name == childName)
                return childTransform.gameObject;
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

    private readonly struct ItemDefinitionScaffoldData
    {
        public readonly string ObjectName;
        public readonly string ItemDataId;
        public readonly string DisplayName;
        public readonly string IconResourcePath;
        public readonly Vector3 LocalPosition;

        public ItemDefinitionScaffoldData(string objectName, string itemDataId, string displayName, string iconResourcePath, Vector3 localPosition)
        {
            ObjectName = objectName;
            ItemDataId = itemDataId;
            DisplayName = displayName;
            IconResourcePath = iconResourcePath;
            LocalPosition = localPosition;
        }
    }
}
