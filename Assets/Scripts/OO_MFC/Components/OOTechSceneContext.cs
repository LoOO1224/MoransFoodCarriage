using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Collects role components once when a group opens.
/// A group controller can ask for a role instead of depending on each concrete
/// GameObject slot. Future groups can reuse the same component without knowing
/// anything about Senario1Group.
/// </summary>
[DisallowMultipleComponent]
public class OOTechSceneContext : MonoBehaviour
{
    private readonly Dictionary<string, OOTechSceneObject> _sceneObjectDic =
        new Dictionary<string, OOTechSceneObject>();

    private void Awake()
    {
        CacheSceneObjects();
    }

    public void CacheSceneObjects()
    {
        _sceneObjectDic.Clear();

        OOTechSceneObject[] sceneObjectArray = GetComponentsInChildren<OOTechSceneObject>(true);

        foreach (OOTechSceneObject sceneObject in sceneObjectArray)
        {
            if (sceneObject == null || string.IsNullOrWhiteSpace(sceneObject.RoleId))
                continue;

            if (_sceneObjectDic.ContainsKey(sceneObject.RoleId))
            {
                Debug.LogWarning($"[OOTechSceneContext] Duplicate role ignored: {sceneObject.RoleId}", sceneObject);
                continue;
            }

            _sceneObjectDic.Add(sceneObject.RoleId, sceneObject);
        }
    }

    public GameObject GetRoleObject(string roleId)
    {
        OOTechSceneObject sceneObject = GetSceneObject(roleId);
        return sceneObject != null ? sceneObject.gameObject : null;
    }

    public Transform GetRoleTransform(string roleId)
    {
        OOTechSceneObject sceneObject = GetSceneObject(roleId);
        return sceneObject != null ? sceneObject.transform : null;
    }

    public T GetRoleComponent<T>(string roleId) where T : Component
    {
        OOTechSceneObject sceneObject = GetSceneObject(roleId);
        return sceneObject != null ? sceneObject.GetRoleComponent<T>() : null;
    }

    private OOTechSceneObject GetSceneObject(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            return null;

        _sceneObjectDic.TryGetValue(roleId, out OOTechSceneObject sceneObject);
        return sceneObject;
    }
}
