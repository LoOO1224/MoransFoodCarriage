// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: MainMenuBGMPlayer.cs
// - 역할: 지정된 구간의 배경음악 재생을 SoundManager에 요청합니다.
// - 유지보수: 같은 BGM을 이어 재생해야 하는 구간에서는 중복 재시작 여부를 확인합니다.
// =============================================================================
using UnityEngine;

/// <summary>
/// MainMenuGroup이 켜져 있는 동안 메인 메뉴 BGM을 재생합니다.
/// Game View에서는 첫 화면의 음악 큐를 담당하는 음향 스태프입니다.
/// </summary>
public class MainMenuBGMPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _mainMenuBGM;
    [SerializeField] private string _mainMenuBGMAssetPath = "Assets/Sounds/BGM/MainMenu_BGM.mp3";

    /// <summary>
    /// 메인 메뉴 무대가 열리면 지정된 BGM을 재생합니다.
    /// </summary>
    private void OnEnable()
    {
        AudioClip bgmClip = ResolveMainMenuBGMClip();

        if (OOTechSoundManager.Inst != null && bgmClip != null)
            OOTechSoundManager.Inst.PlayBGM(bgmClip);
    }

    /// <summary>
    /// 메인 메뉴 무대가 닫히면 다음 장면 음악과 겹치지 않도록 BGM을 멈춥니다.
    /// </summary>
    private void OnDisable()
    {
        AudioClip bgmClip = ResolveMainMenuBGMClip();

        if (OOTechSoundManager.Inst != null && bgmClip != null)
            OOTechSoundManager.Inst.StopBGM(bgmClip);
    }

    public AudioClip ResolveMainMenuBGMClip()
    {
        return OOTechAudioClipResolver.Resolve(_mainMenuBGM, "Audio/BGM/MainMenu_BGM", _mainMenuBGMAssetPath);
    }
}
