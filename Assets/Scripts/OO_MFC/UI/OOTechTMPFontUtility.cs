// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechTMPFontUtility.cs
// - 역할: UI 오브젝트 참조, 표시 갱신, 버튼 입력 연결을 담당합니다.
// - 유지보수: 씬 Hierarchy 이름으로 런타임 참조를 복구하는 코드가 많아 오브젝트 이름 변경에 주의합니다.
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
    private const string _fontAssetPath = "Assets/Resources/Fonts/ChosunCentennial SDF.asset";
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
