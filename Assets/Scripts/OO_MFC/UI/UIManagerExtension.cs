using UnityEngine;

public static class UIManagerExtension
{
    // ==================== Main Menu 버튼 이벤트 ====================

    /// <summary>
    /// 시작하기 버튼 클릭 - 프롤로그로 이동
    /// </summary>
    public static void OnStartButtonClicked()
    {
        Debug.Log("[UIManagerExtension] 시작하기 버튼 클릭 → 프롤로그로 이동");

        // MainMenuGroup 닫기
        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.CloseUI("MainMenuGroup");
        }

        // TODO: PrologueGroup 열기 (다음 단계에서 구현)
        Debug.LogWarning("PrologueGroup 열기 로직은 아직 구현되지 않았습니다.");
    }

    /// <summary>
    /// 도감 버튼 클릭
    /// </summary>
    public static void OnCodexButtonClicked()
    {
        Debug.Log("[UIManagerExtension] 도감 버튼 클릭");
        // TODO: CodexGroup 열기
        Debug.LogWarning("CodexGroup 열기 로직은 아직 구현되지 않았습니다.");
    }

    /// <summary>
    /// 종료 버튼 클릭
    /// </summary>
    public static void OnExitButtonClicked()
    {
        Debug.Log("[UIManagerExtension] 종료 버튼 클릭 → 게임 종료");
        Application.Quit();
    }
}