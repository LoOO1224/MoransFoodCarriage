using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Senario1Group에서 카메라가 누구를 비추는지에 따라 BGM을 전환합니다.
/// 인스펙터 AudioClip이 비어 있으면 에디터 플레이 중에 경로로 한 번 더 찾아봅니다.
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

    /// <summary>
    /// Scenario1 무대가 처음 열리면 재익에게 초점이 맞은 BGM으로 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        PlayJaeikFocusedBGM();
    }

    /// <summary>
    /// Scenario1 무대가 닫히면 현재 BGM을 정리합니다.
    /// </summary>
    private void OnDisable()
    {
        if (OOTechSoundManager.Inst != null)
            OOTechSoundManager.Inst.StopBGM();
    }

    /// <summary>
    /// 카메라가 재익/재익군을 비출 때 쓰는 음악 큐입니다.
    /// </summary>
    public void PlayJaeikFocusedBGM()
    {
        AudioClip bgmClip = ResolveClip(_jaeikFocusedBGM, _jaeikFocusedBGMAssetPath);

        if (bgmClip == null)
            bgmClip = _senario1BGM;

        PlayBGM(bgmClip, "Jaeik focused");
    }

    /// <summary>
    /// 카메라가 춘양을 비출 때 쓰는 음악 큐입니다.
    /// </summary>
    public void PlayChunyangFocusedBGM()
    {
        AudioClip bgmClip = ResolveClip(_chunyangFocusedBGM, _chunyangFocusedBGMAssetPath);

        if (bgmClip == null)
            bgmClip = _senario1BGM;

        PlayBGM(bgmClip, "Chunyang focused");
    }

    /// <summary>
    /// SoundManager에게 실제 BGM 재생을 요청합니다.
    /// </summary>
    private void PlayBGM(AudioClip bgmClip, string label)
    {
        if (OOTechSoundManager.Inst == null || bgmClip == null)
            return;

        OOTechSoundManager.Inst.PlayBGM(bgmClip, true);
        Debug.Log($"[Senario1_BGMPlayer] Play {label} BGM: {bgmClip.name}");
    }

    /// <summary>
    /// 인스펙터 클립을 우선 사용하고, 에디터에서는 경로 기반 클립을 보조로 사용합니다.
    /// </summary>
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
