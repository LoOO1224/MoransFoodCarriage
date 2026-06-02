using UnityEngine;

/// <summary>
/// MainMenuGroup의 시작, 도감, 종료 버튼을 받는 컨트롤러입니다.
/// Game View에서는 플레이어가 첫 화면에서 어떤 무대로 들어갈지 선택하는 입구 역할입니다.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Codex BGM")]
    [SerializeField] private AudioClip _codexBGM;

    // ==================== 버튼 이벤트 ====================

    /// <summary>
    /// 시작 버튼을 누르면 첫 진행 그룹으로 이동합니다.
    /// </summary>
    public void OnStartButtonClicked()
    {
        UIManagerExtension.OnStartButtonClicked();
    }

    /// <summary>
    /// 도감 버튼을 누르면 CodexGroup을 열고 도감 전용 BGM이 있으면 재생합니다.
    /// </summary>
    public void OnCodexButtonClicked()
    {
        UIManagerExtension.OnCodexButtonClicked();

        if (OOTechSoundManager.Inst != null && _codexBGM != null)
            OOTechSoundManager.Inst.PlayBGM(_codexBGM);
    }

    /// <summary>
    /// 종료 버튼을 누르면 게임 종료 요청을 전달합니다.
    /// </summary>
    public void OnExitButtonClicked()
    {
        UIManagerExtension.OnExitButtonClicked();
    }
}
