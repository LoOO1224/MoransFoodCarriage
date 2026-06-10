// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechGroupNavigationHistory.cs
// - 역할: UI 오브젝트 참조, 표시 갱신, 버튼 입력 연결을 담당합니다.
// - 유지보수: 씬 Hierarchy 이름으로 런타임 참조를 복구하는 코드가 많아 오브젝트 이름 변경에 주의합니다.
// =============================================================================
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Remembers which group opened an overlay group such as CodexGroup or WorldMapGroup.
/// This keeps the back button returning to the previous stage instead of always
/// sending the player back to MainMenuGroup.
/// </summary>
public static class OOTechGroupNavigationHistory
{
    private static readonly Dictionary<string, string> _previousGroupDic = new Dictionary<string, string>();

    public static void SetPreviousGroup(string openedGroupName, string previousGroupName)
    {
        if (string.IsNullOrEmpty(openedGroupName) || string.IsNullOrEmpty(previousGroupName))
            return;

        _previousGroupDic[openedGroupName] = previousGroupName;
    }

    public static string GetPreviousGroup(string openedGroupName, string defaultGroupName)
    {
        if (!string.IsNullOrEmpty(openedGroupName) &&
            _previousGroupDic.TryGetValue(openedGroupName, out string previousGroupName) &&
            !string.IsNullOrEmpty(previousGroupName))
        {
            return previousGroupName;
        }

        return defaultGroupName;
    }

    public static string RequestRootGroupName(Transform childTransform)
    {
        if (childTransform == null)
            return string.Empty;

        Transform rootTransform = childTransform;

        while (rootTransform.parent != null)
            rootTransform = rootTransform.parent;

        return rootTransform.name;
    }
}

