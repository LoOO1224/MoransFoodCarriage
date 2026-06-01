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

    private static TMP_FontAsset Font_Project;

    public static TMP_FontAsset GetProjectFont()
    {
        if (Font_Project != null)
            return Font_Project;

        Font_Project = Resources.Load<TMP_FontAsset>(_fontResourcePath);

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
