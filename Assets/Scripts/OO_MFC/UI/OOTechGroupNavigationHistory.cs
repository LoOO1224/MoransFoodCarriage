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
