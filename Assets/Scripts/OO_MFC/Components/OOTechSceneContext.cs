using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 그룹 안의 역할표(OOTechSceneObject)를 모아 두는 컴포넌트입니다.
/// 감독이 모든 배우를 직접 붙잡지 않고 "Jaeik", "Moran" 같은 역할 이름으로 찾게 해줍니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechSceneContext : MonoBehaviour
{
    private readonly Dictionary<string, OOTechSceneObject> _sceneObjectDic =
        new Dictionary<string, OOTechSceneObject>();

    /// <summary>
    /// 씬이 시작되면 자식 배우들의 역할표를 미리 수집합니다.
    /// </summary>
    private void Awake()
    {
        CacheSceneObjects();
    }

    /// <summary>
    /// 자식 오브젝트의 역할표를 다시 읽어 Dictionary에 정리합니다.
    /// 새 배우를 추가한 뒤에도 이 메서드만 부르면 감독이 다시 찾을 수 있습니다.
    /// </summary>
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

    /// <summary>
    /// 역할 이름으로 해당 배우의 GameObject를 반환합니다.
    /// </summary>
    public GameObject GetRoleObject(string roleId)
    {
        OOTechSceneObject sceneObject = GetSceneObject(roleId);
        return sceneObject != null ? sceneObject.gameObject : null;
    }

    /// <summary>
    /// 역할 이름으로 해당 배우의 Transform을 반환합니다.
    /// </summary>
    public Transform GetRoleTransform(string roleId)
    {
        OOTechSceneObject sceneObject = GetSceneObject(roleId);
        return sceneObject != null ? sceneObject.transform : null;
    }

    /// <summary>
    /// 역할 이름으로 배우를 찾고, 그 배우에게 붙은 특정 컴포넌트를 반환합니다.
    /// </summary>
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
