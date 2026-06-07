// =============================================================================
// OO_MFC Editor Repair
// - 스크립트: OOTechStage3EncounterRepairEditor.cs
// - 역할: 배치모드에서 EncounterGroup 배우의 전용 Sprite 루프 플레이어를 세팅하고 프레임 품질을 검사합니다.
// - 영화 비유: 리허설 전에 스태프가 배우 컷 사진의 크기/중심점/필름 번호가 맞는지 검사하고 무대에 붙입니다.
// =============================================================================
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class OOTechStage3EncounterRepairEditor
{
    private const string ScenePath = "Assets/Scenes/OO_MFC.unity";
    private const string MoranClipPath = "Assets/Animation/Characters/Moran/Moran_isScared.anim";
    private const string MrJaeikClipPath = "Assets/Animation/Characters/Mr.Jaeik/Mr.Jaeik_isThreatening.anim";
    private const string Stage3BackgroundSpritePath = "Assets/Images/Stages/Stage3.png";

    /// <summary>
    /// 배치모드에서 호출하는 수리 진입점입니다.
    /// EncounterGroup의 Moran/Mr.Jaeik에게 Animator 우회용 Sprite 프레임 플레이어를 붙이고 씬을 저장합니다.
    /// </summary>
    public static void RepairStage3EncounterLoopPlayers()
    {
        EditorSceneManager.OpenScene(ScenePath);

        GameObject encounterGroup = FindSceneObjectByName("EncounterGroup");

        if (encounterGroup == null)
        {
            Debug.LogError("[OOTechStage3EncounterRepairEditor] EncounterGroup not found.");
            return;
        }

        RepairActor(encounterGroup.transform, "Moran", "Moran_isScared", MoranClipPath, 0.6f);
        RepairActor(encounterGroup.transform, "Mr.Jaeik", "Mr_Jaeik_isThreatening", MrJaeikClipPath, 0.6f);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("[OOTechStage3EncounterRepairEditor] Stage3 Encounter loop player repair completed.");
    }

    /// <summary>
    /// EncounterGroup의 배경과 중복 Mr.Jaeik만 수리합니다.
    /// 사용자가 새로 배치한 Moran/Mr.Jaeik 애니메이션 세팅은 건드리지 않습니다.
    /// </summary>
    public static void RepairEncounterGroupVisualState()
    {
        EditorSceneManager.OpenScene(ScenePath);

        GameObject encounterGroup = FindSceneObjectByName("EncounterGroup");

        if (encounterGroup == null)
        {
            Debug.LogError("[OOTechStage3EncounterRepairEditor] EncounterGroup not found.");
            return;
        }

        RepairEncounterBackground(encounterGroup.transform);
        AssignEncounterBackgroundSprite(encounterGroup, AssetDatabase.LoadAssetAtPath<Sprite>(Stage3BackgroundSpritePath));
        RemoveDuplicateMrJaeik(encounterGroup.transform);
        RemoveStage3GroupMrJaeikArtifacts(encounterGroup.transform);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("[OOTechStage3EncounterRepairEditor] EncounterGroup visual repair completed.");
    }

    private static void RepairEncounterBackground(Transform encounterRoot)
    {
        Transform backgroundTransform = FindChildByName(encounterRoot, "Stage3Background");

        if (backgroundTransform == null)
        {
            GameObject backgroundObject = new GameObject("Stage3Background");
            backgroundTransform = backgroundObject.transform;
            backgroundTransform.SetParent(encounterRoot, false);
            Debug.LogWarning("[OOTechStage3EncounterRepairEditor] EncounterGroup Stage3Background was missing. Created new background object.");
        }

        SpriteRenderer backgroundRenderer = backgroundTransform.GetComponent<SpriteRenderer>();

        if (backgroundRenderer == null)
            backgroundRenderer = backgroundTransform.gameObject.AddComponent<SpriteRenderer>();

        Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Stage3BackgroundSpritePath);

        if (backgroundSprite != null)
            backgroundRenderer.sprite = backgroundSprite;
        else
            Debug.LogWarning($"[OOTechStage3EncounterRepairEditor] Stage3 background sprite not found: {Stage3BackgroundSpritePath}");

        backgroundTransform.gameObject.SetActive(true);
        backgroundRenderer.enabled = true;
        backgroundRenderer.forceRenderingOff = false;
        backgroundRenderer.color = Color.white;
        backgroundRenderer.sortingLayerName = "Background";
        backgroundRenderer.sortingOrder = -1000;
        backgroundTransform.localPosition = Vector3.zero;
        backgroundTransform.localRotation = Quaternion.identity;

        if (backgroundRenderer.sprite != null)
        {
            float targetWidth = 1920f;
            float targetHeight = 1080f;
            Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;

            if (spriteSize.x > 0f && spriteSize.y > 0f)
            {
                float scaleX = targetWidth / spriteSize.x;
                float scaleY = targetHeight / spriteSize.y;
                backgroundTransform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
        }

        EditorUtility.SetDirty(backgroundTransform.gameObject);
        Debug.Log($"[OOTechStage3EncounterRepairEditor] Encounter background repaired. Sprite={backgroundRenderer.sprite?.name}, Scale={backgroundTransform.localScale}");
    }

    private static void AssignEncounterBackgroundSprite(GameObject encounterGroup, Sprite backgroundSprite)
    {
        if (encounterGroup == null || backgroundSprite == null)
            return;

        OOTechStage3EncounterController controller = encounterGroup.GetComponent<OOTechStage3EncounterController>();

        if (controller == null)
            controller = encounterGroup.AddComponent<OOTechStage3EncounterController>();

        SerializedObject serializedObject = new SerializedObject(controller);
        SerializedProperty spriteProperty = serializedObject.FindProperty("Sprite_Stage3Background");

        if (spriteProperty == null)
        {
            Debug.LogWarning("[OOTechStage3EncounterRepairEditor] Sprite_Stage3Background serialized field not found.");
            return;
        }

        spriteProperty.objectReferenceValue = backgroundSprite;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(controller);
        Debug.Log("[OOTechStage3EncounterRepairEditor] Encounter controller Stage3 background sprite reference assigned.");
    }

    private static void RemoveDuplicateMrJaeik(Transform encounterRoot)
    {
        List<Transform> mrJaeikList = FindActorTransformList(encounterRoot, "Mr.Jaeik");

        if (mrJaeikList.Count <= 1)
        {
            Debug.Log($"[OOTechStage3EncounterRepairEditor] Mr.Jaeik duplicate count OK: {mrJaeikList.Count}");
            return;
        }

        Transform moranTransform = FindPreferredRoleTransform(encounterRoot, "Moran", null);
        Transform keepTransform = FindPreferredActorTransform(encounterRoot, "Mr.Jaeik", moranTransform);

        foreach (Transform mrJaeikTransform in mrJaeikList)
        {
            if (mrJaeikTransform == null || mrJaeikTransform == keepTransform)
                continue;

            Debug.LogWarning($"[OOTechStage3EncounterRepairEditor] Removing duplicate Mr.Jaeik: {GetHierarchyPath(mrJaeikTransform)} / Pos={mrJaeikTransform.position}");
            Object.DestroyImmediate(mrJaeikTransform.gameObject);
        }

        if (keepTransform != null)
            Debug.Log($"[OOTechStage3EncounterRepairEditor] Kept Mr.Jaeik: {GetHierarchyPath(keepTransform)} / Pos={keepTransform.position}");
    }

    private static void RemoveStage3GroupMrJaeikArtifacts(Transform encounterRoot)
    {
        GameObject stage3GroupObject = FindSceneObjectByName("Stage3Group");

        if (stage3GroupObject == null)
            return;

        Transform[] transformArray = stage3GroupObject.GetComponentsInChildren<Transform>(true);
        List<Transform> removeTransformList = new List<Transform>();

        foreach (Transform targetTransform in transformArray)
        {
            if (targetTransform == null || targetTransform == stage3GroupObject.transform)
                continue;

            if (!IsMrJaeikRelatedPath(targetTransform))
                continue;

            if (IsChildOfAny(targetTransform, removeTransformList))
                continue;

            removeTransformList.Add(targetTransform);
        }

        foreach (Transform targetTransform in removeTransformList)
        {
            Debug.LogWarning($"[OOTechStage3EncounterRepairEditor] Removing Stage3Group Mr.Jaeik artifact: {GetHierarchyPath(targetTransform)}");
            Object.DestroyImmediate(targetTransform.gameObject);
        }
    }

    private static List<Transform> FindActorTransformList(Transform rootTransform, string actorName)
    {
        List<Transform> transformList = FindRoleTransformList(rootTransform, actorName);
        Transform[] childTransformArray = rootTransform.GetComponentsInChildren<Transform>(true);

        foreach (Transform childTransform in childTransformArray)
        {
            if (childTransform == null)
                continue;

            bool isExactActorName = childTransform.name == actorName;
            bool isActorComponent = childTransform.GetComponent<OOTechStageActorMotion>() != null || childTransform.GetComponent<OOTechSceneObject>() != null;

            if (!isExactActorName && !(isActorComponent && childTransform.name.Contains(actorName)))
                continue;

            if (!transformList.Contains(childTransform))
                transformList.Add(childTransform);
        }

        return transformList;
    }

    private static List<Transform> FindRoleTransformList(Transform rootTransform, string roleId)
    {
        List<Transform> transformList = new List<Transform>();
        OOTechSceneObject[] sceneObjectArray = rootTransform.GetComponentsInChildren<OOTechSceneObject>(true);

        foreach (OOTechSceneObject sceneObject in sceneObjectArray)
        {
            if (sceneObject != null && sceneObject.RoleId == roleId)
                transformList.Add(sceneObject.transform);
        }

        return transformList;
    }

    private static Transform FindPreferredRoleTransform(Transform rootTransform, string roleId, Transform anchorTransform)
    {
        List<Transform> roleTransformList = FindRoleTransformList(rootTransform, roleId);
        return FindClosestActiveTransform(roleTransformList, anchorTransform);
    }

    private static Transform FindPreferredActorTransform(Transform rootTransform, string actorName, Transform anchorTransform)
    {
        List<Transform> actorTransformList = FindActorTransformList(rootTransform, actorName);
        return FindClosestActiveTransform(actorTransformList, anchorTransform);
    }

    private static Transform FindClosestActiveTransform(List<Transform> transformList, Transform anchorTransform)
    {
        Transform bestTransform = null;
        float bestScore = float.MaxValue;

        foreach (Transform roleTransform in transformList)
        {
            if (roleTransform == null)
                continue;

            float score = anchorTransform != null ? Vector3.SqrMagnitude(roleTransform.position - anchorTransform.position) : 0f;

            if (roleTransform.gameObject.activeInHierarchy)
                score -= 100000f;

            if (score >= bestScore)
                continue;

            bestScore = score;
            bestTransform = roleTransform;
        }

        return bestTransform;
    }

    private static string GetHierarchyPath(Transform targetTransform)
    {
        if (targetTransform == null)
            return string.Empty;

        string path = targetTransform.name;
        Transform currentTransform = targetTransform.parent;

        while (currentTransform != null)
        {
            path = currentTransform.name + "/" + path;
            currentTransform = currentTransform.parent;
        }

        return path;
    }

    private static bool IsMrJaeikRelatedPath(Transform targetTransform)
    {
        Transform currentTransform = targetTransform;

        while (currentTransform != null)
        {
            string objectName = currentTransform.name;

            if (objectName.Contains("Mr.Jaeik") || objectName.Contains("Mr_Jaeik"))
                return true;

            currentTransform = currentTransform.parent;
        }

        return false;
    }

    private static bool IsChildOfAny(Transform targetTransform, List<Transform> parentTransformList)
    {
        foreach (Transform parentTransform in parentTransformList)
        {
            if (targetTransform.IsChildOf(parentTransform))
                return true;
        }

        return false;
    }

    private static void RepairActor(Transform encounterRoot, string actorName, string stateName, string clipPath, float speed)
    {
        Transform actorTransform = FindChildByName(encounterRoot, actorName);
        AnimationClip sourceClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);

        if (actorTransform == null)
        {
            Debug.LogError($"[OOTechStage3EncounterRepairEditor] Actor not found: {actorName}");
            return;
        }

        if (sourceClip == null)
        {
            Debug.LogError($"[OOTechStage3EncounterRepairEditor] Clip not found: {clipPath}");
            return;
        }

        OOTechEncounterLoopSpritePlayer loopPlayer = actorTransform.GetComponent<OOTechEncounterLoopSpritePlayer>();

        if (loopPlayer == null)
            loopPlayer = actorTransform.gameObject.AddComponent<OOTechEncounterLoopSpritePlayer>();

        loopPlayer.RequestEditorSetupFromClip(stateName, sourceClip, speed);
        LogSpriteFrameImportState(actorName, sourceClip);
        EditorUtility.SetDirty(actorTransform.gameObject);
    }

    private static void LogSpriteFrameImportState(string actorName, AnimationClip sourceClip)
    {
        List<Sprite> spriteList = ExtractSpriteList(sourceClip);

        if (spriteList.Count == 0)
        {
            Debug.LogWarning($"[OOTechStage3EncounterRepairEditor] No sprite frames: {actorName}");
            return;
        }

        float firstPixelsPerUnit = spriteList[0].pixelsPerUnit;
        Vector2 firstPivot = spriteList[0].pivot;
        Rect firstRect = spriteList[0].rect;
        bool hasDifferentPixelsPerUnit = false;
        bool hasDifferentPivot = false;
        bool hasDifferentRectSize = false;
        bool hasEmptyFrame = false;

        foreach (Sprite sprite in spriteList)
        {
            if (sprite == null)
            {
                hasEmptyFrame = true;
                continue;
            }

            if (!Mathf.Approximately(sprite.pixelsPerUnit, firstPixelsPerUnit))
                hasDifferentPixelsPerUnit = true;

            if ((sprite.pivot - firstPivot).sqrMagnitude > 0.01f)
                hasDifferentPivot = true;

            if (!Mathf.Approximately(sprite.rect.width, firstRect.width) || !Mathf.Approximately(sprite.rect.height, firstRect.height))
                hasDifferentRectSize = true;
        }

        Debug.Log($"[OOTechStage3EncounterRepairEditor] {actorName} frame check. Count={spriteList.Count}, PPU={firstPixelsPerUnit}, DifferentPPU={hasDifferentPixelsPerUnit}, DifferentPivot={hasDifferentPivot}, DifferentRectSize={hasDifferentRectSize}, EmptyFrame={hasEmptyFrame}");
    }

    private static List<Sprite> ExtractSpriteList(AnimationClip sourceClip)
    {
        List<Sprite> spriteList = new List<Sprite>();
        EditorCurveBinding[] bindingArray = AnimationUtility.GetObjectReferenceCurveBindings(sourceClip);

        foreach (EditorCurveBinding binding in bindingArray)
        {
            if (binding.type != typeof(SpriteRenderer) || binding.propertyName != "m_Sprite")
                continue;

            ObjectReferenceKeyframe[] keyframeArray = AnimationUtility.GetObjectReferenceCurve(sourceClip, binding);

            foreach (ObjectReferenceKeyframe keyframe in keyframeArray)
                spriteList.Add(keyframe.value as Sprite);
        }

        return spriteList;
    }

    private static GameObject FindSceneObjectByName(string objectName)
    {
        foreach (GameObject rootObject in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform foundTransform = FindChildByName(rootObject.transform, objectName);

            if (foundTransform != null)
                return foundTransform.gameObject;
        }

        return null;
    }

    private static Transform FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == objectName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = FindChildByName(rootTransform.GetChild(index), objectName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
    }
}
#endif
