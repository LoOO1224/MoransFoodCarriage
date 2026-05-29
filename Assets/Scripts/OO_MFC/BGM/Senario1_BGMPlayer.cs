using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Plays and switches Senario1Group-focused BGM tracks.
/// AudioClip fields can be assigned in the inspector; editor asset paths are used as a fallback during Play Mode.
/// </summary>
public class Senario1_BGMPlayer : MonoBehaviour
{
    [Header("Fallback BGM")]
    [SerializeField] private AudioClip _senario1BGM;

    [Header("Focused BGM")]
    [SerializeField] private AudioClip _jaeikFocusedBGM;
    [SerializeField] private AudioClip _chunyangFocusedBGM;

    [Header("Editor Asset Fallback")]
    [SerializeField] private string _jaeikFocusedBGMAssetPath = "Assets/Sounds/BGM/Senario1Group_JaeikCameraFocused_BGM.mp3";
    [SerializeField] private string _chunyangFocusedBGMAssetPath = "Assets/Sounds/BGM/Senario1Group_Chunyang_CameraFocused_BGM.mp3";

    private void OnEnable()
    {
        PlayJaeikFocusedBGM();
    }

    private void OnDisable()
    {
        if (OOTechSoundManager.Inst != null)
            OOTechSoundManager.Inst.StopBGM();
    }

    public void PlayJaeikFocusedBGM()
    {
        AudioClip bgmClip = ResolveClip(_jaeikFocusedBGM, _jaeikFocusedBGMAssetPath);

        if (bgmClip == null)
            bgmClip = _senario1BGM;

        PlayBGM(bgmClip, "Jaeik focused");
    }

    public void PlayChunyangFocusedBGM()
    {
        AudioClip bgmClip = ResolveClip(_chunyangFocusedBGM, _chunyangFocusedBGMAssetPath);

        if (bgmClip == null)
            bgmClip = _senario1BGM;

        PlayBGM(bgmClip, "Chunyang focused");
    }

    private void PlayBGM(AudioClip bgmClip, string label)
    {
        if (OOTechSoundManager.Inst == null || bgmClip == null)
            return;

        OOTechSoundManager.Inst.PlayBGM(bgmClip, true);
        Debug.Log($"[Senario1_BGMPlayer] Play {label} BGM: {bgmClip.name}");
    }

    private AudioClip ResolveClip(AudioClip assignedClip, string editorAssetPath)
    {
        if (assignedClip != null)
            return assignedClip;

#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(editorAssetPath))
            return AssetDatabase.LoadAssetAtPath<AudioClip>(editorAssetPath);
#endif

        return null;
    }
}
