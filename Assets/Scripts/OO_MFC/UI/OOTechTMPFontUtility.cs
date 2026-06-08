// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechTMPFontUtility.cs
// - 역할: UI 표시와 입력 연결을 담당하는 UI 컴포넌트입니다.
// - 감독 관점: 관객에게 보이는 패널과 버튼의 무대 동선을 담당합니다.
// - 유지보수 포인트: 사용자가 직접 편집할 UI는 하이어라키/프리팹에 두고, 코드에서 즉석 생성하지 않습니다.
// =============================================================================
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Applies the shared project TMP font to runtime-created Korean UI text.
/// The font asset is the subtitle typeface for this production, like the
/// common caption style printed on every shot.
/// </summary>
public static class OOTechTMPFontUtility
{
    private const string _fontAssetPath = "Assets/Fonts/ChosunCentennial SDF.asset";
    private const string _fontResourcePath = "ChosunCentennial SDF";
    private const string _fontResourcePathInFolder = "Fonts/ChosunCentennial SDF";

    private static TMP_FontAsset Font_Project;

    public static TMP_FontAsset GetProjectFont()
    {
        if (Font_Project != null)
            return Font_Project;

        Font_Project = Resources.Load<TMP_FontAsset>(_fontResourcePath);

        if (Font_Project == null)
            Font_Project = Resources.Load<TMP_FontAsset>(_fontResourcePathInFolder);

#if UNITY_EDITOR
        if (Font_Project == null)
            Font_Project = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(_fontAssetPath);
#endif

        return Font_Project;
    }

    public static void ApplyProjectFont(TextMeshProUGUI text)
    {
        if (text == null)
            return;

        TMP_FontAsset fontAsset = GetProjectFont();

        if (fontAsset != null)
            text.font = fontAsset;
    }
}
