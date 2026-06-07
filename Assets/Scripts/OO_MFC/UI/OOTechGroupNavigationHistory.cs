// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechGroupNavigationHistory.cs
// - ??븷: UI ?쒖떆? ?낅젰 ?곌껐???대떦?섎뒗 UI 而댄룷?뚰듃?낅땲??
// - 媛먮룆 愿?? 愿媛앹뿉寃?蹂댁씠???⑤꼸怨?踰꾪듉??臾대? ?숈꽑???대떦?⑸땲??
// - ?좎?蹂댁닔 ?ъ씤?? ?ъ슜?먭? 吏곸젒 ?몄쭛??UI???섏씠?대씪???꾨━?뱀뿉 ?먭퀬, 肄붾뱶?먯꽌 利됱꽍 ?앹꽦?섏? ?딆뒿?덈떎.
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

