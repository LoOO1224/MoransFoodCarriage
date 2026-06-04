// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechGroupNavigationHistory.cs
// - 역할: UI 표시와 입력 연결을 담당하는 UI 컴포넌트입니다.
// - 감독 관점: 관객에게 보이는 패널과 버튼의 무대 동선을 담당합니다.
// - 유지보수 포인트: 사용자가 직접 편집할 UI는 하이어라키/프리팹에 두고, 코드에서 즉석 생성하지 않습니다.
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

    public static string FindRootGroupName(Transform childTransform)
    {
        if (childTransform == null)
            return string.Empty;

        Transform rootTransform = childTransform;

        while (rootTransform.parent != null)
            rootTransform = rootTransform.parent;

        return rootTransform.name;
    }
}
