// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: MainMenuController.cs
// - 역할: 메인 메뉴 버튼 흐름을 담당하는 시작 장면 Controller입니다.
// - 감독 관점: 첫 화면에서 관객이 어느 장면으로 들어갈지 안내하는 안내 데스크입니다.
// - 유지보수 포인트: 버튼 배치는 UI 오브젝트에서 직접 수정하고, 이 스크립트는 클릭 후 이동만 담당합니다.
// =============================================================================
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
