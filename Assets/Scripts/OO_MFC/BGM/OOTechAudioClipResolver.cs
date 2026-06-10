using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Resolves BGM clips in the same order across gameplay scripts:
/// assigned Inspector clip, Resources path, then Editor-only asset path.
/// </summary>
public static class OOTechAudioClipResolver
{
    private const string MainMenuBGMResourcePath = "Audio/BGM/MainMenu_BGM";
    private const string MainMenuBGMEditorAssetPath = "Assets/Sounds/BGM/MainMenu_BGM.mp3";

    public static AudioClip Resolve(AudioClip assignedClip, string resourcePath, string editorAssetPath = null)
    {
        if (assignedClip != null)
            return assignedClip;

        AudioClip resourceClip = ResolveResource(resourcePath);

        if (resourceClip != null)
            return resourceClip;

#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(editorAssetPath))
            return AssetDatabase.LoadAssetAtPath<AudioClip>(editorAssetPath);
#endif

        return null;
    }

    public static AudioClip ResolveFromEditorAssetName(AudioClip assignedClip, string editorAssetPath)
    {
        return Resolve(assignedClip, CreateBGMResourcePathFromAssetPath(editorAssetPath), editorAssetPath);
    }

    public static AudioClip ResolveMainMenuBGM()
    {
        MainMenuBGMPlayer[] playerArray = Resources.FindObjectsOfTypeAll<MainMenuBGMPlayer>();

        foreach (MainMenuBGMPlayer player in playerArray)
        {
            if (player == null)
                continue;

            AudioClip clip = player.ResolveMainMenuBGMClip();

            if (clip != null)
                return clip;
        }

        return Resolve(null, MainMenuBGMResourcePath, MainMenuBGMEditorAssetPath);
    }

    private static AudioClip ResolveResource(string resourcePath)
    {
        return string.IsNullOrWhiteSpace(resourcePath) ? null : Resources.Load<AudioClip>(resourcePath);
    }

    private static string CreateBGMResourcePathFromAssetPath(string editorAssetPath)
    {
        if (string.IsNullOrWhiteSpace(editorAssetPath))
            return null;

        string fileName = Path.GetFileNameWithoutExtension(editorAssetPath);
        return string.IsNullOrWhiteSpace(fileName) ? null : $"Audio/BGM/{fileName}";
    }
}
