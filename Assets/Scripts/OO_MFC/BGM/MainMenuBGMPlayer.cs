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
