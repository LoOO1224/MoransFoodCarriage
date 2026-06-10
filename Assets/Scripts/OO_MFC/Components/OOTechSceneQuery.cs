// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechSceneQuery.cs
// - 역할: 여러 그룹에서 재사용하는 씬 컴포넌트/검색 보조 기능입니다.
// - 유지보수: 역할 ID와 씬 오브젝트 이름 기반 검색을 보조하므로 공용 호출 범위를 확인합니다.
// =============================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 현재 활성 씬의 루트 오브젝트를 기준으로만 컴포넌트와 오브젝트를 조회합니다.
/// Game View에서는 비활성 그룹까지 포함해도 현재 씬 내부만 훑기 때문에 조회 범위가 명확합니다.
/// </summary>
public static class OOTechSceneQuery
{
    /// <summary>
    /// 현재 씬에 배치된 특정 컴포넌트들을 수집합니다.
    /// 감독이 "이 무대 안에 같은 역할표를 단 배우가 누구인지" 확인할 때 쓰는 공통 목록표입니다.
    /// </summary>
    public static List<T> RequestCollectComponents<T>(bool isIncludeInactive = true) where T : Component
    {
        List<T> componentList = new List<T>();
        Scene activeScene = SceneManager.GetActiveScene();

        if (!activeScene.IsValid())
            return componentList;

        foreach (GameObject rootObject in activeScene.GetRootGameObjects())
        {
            if (rootObject == null)
                continue;

            componentList.AddRange(rootObject.GetComponentsInChildren<T>(isIncludeInactive));
        }

        return componentList;
    }

    /// <summary>
    /// 현재 씬에서 조건에 맞는 첫 번째 컴포넌트를 반환합니다.
    /// 하나만 필요한 공용 배우를 찾을 때 쓰되, 가능하면 각 그룹이 자기 자식 컴포넌트를 먼저 참조하게 합니다.
    /// </summary>
    public static T RequestFirstComponent<T>(System.Predicate<T> predicate = null, bool isIncludeInactive = true) where T : Component
    {
        List<T> componentList = RequestCollectComponents<T>(isIncludeInactive);

        for (int index = 0; index < componentList.Count; index++)
        {
            T component = componentList[index];

            if (component == null)
                continue;

            if (predicate == null || predicate(component))
                return component;
        }

        return null;
    }

    /// <summary>
    /// 현재 씬 루트부터 이름이 같은 오브젝트를 찾습니다.
    /// UIManager나 HUD가 씬에 미리 놓인 그룹을 연결할 때만 사용하는 제한적 조회입니다.
    /// </summary>
    public static GameObject RequestSceneObjectByName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

        Scene activeScene = SceneManager.GetActiveScene();

        if (!activeScene.IsValid())
            return null;

        foreach (GameObject rootObject in activeScene.GetRootGameObjects())
        {
            GameObject foundObject = RequestChildObjectByName(rootObject != null ? rootObject.transform : null, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    /// <summary>
    /// 특정 부모 아래에서 이름이 같은 자식 오브젝트를 찾습니다.
    /// 무대감독이 전체 극장이 아니라 한 세트장 안에서만 소품을 찾는 방식입니다.
    /// </summary>
    public static GameObject RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(objectName))
            return null;

        if (rootTransform.name == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = RequestChildObjectByName(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }
}
