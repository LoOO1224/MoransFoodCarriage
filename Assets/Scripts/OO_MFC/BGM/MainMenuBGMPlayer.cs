// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: MainMenuBGMPlayer.cs
// - 역할: 특정 그룹 또는 장면에서 BGM을 재생하는 음향 큐 스크립트입니다.
// - 감독 관점: 장면이 켜질 때 어떤 음악을 틀지 알려 주는 음향 스태프입니다.
// - 유지보수 포인트: 사운드 전환 규칙이 커지면 SoundManager로 옮기고, 이 스크립트는 AudioClip 참조와 재생 요청만 유지합니다.
// =============================================================================
using UnityEngine;

/// <summary>
/// MainMenuGroup이 켜져 있는 동안 메인 메뉴 BGM을 재생합니다.
/// Game View에서는 첫 화면의 음악 큐를 담당하는 음향 스태프입니다.
/// </summary>
public class MainMenuBGMPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _mainMenuBGM;

    /// <summary>
    /// 메인 메뉴 무대가 열리면 지정된 BGM을 재생합니다.
    /// </summary>
    private void OnEnable()
    {
        if (OOTechSoundManager.Inst != null && _mainMenuBGM != null)
        {
            OOTechSoundManager.Inst.PlayBGM(_mainMenuBGM);
        }
    }

    /// <summary>
    /// 메인 메뉴 무대가 닫히면 다음 장면 음악과 겹치지 않도록 BGM을 멈춥니다.
    /// </summary>
    private void OnDisable()
    {
        if (OOTechSoundManager.Inst != null)
        {
            OOTechSoundManager.Inst.StopBGM();
        }
    }
}
