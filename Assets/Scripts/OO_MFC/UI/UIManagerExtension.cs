using UnityEngine;

/// <summary>
/// 신규 UI 이벤트들을 확장 메서드로 관리하는 정적 클래스입니다.
/// 모든 버튼 클릭 로직은 이곳에서 중앙 집중 관리하여 유지보수를 쉽게 합니다.
/// </summary>
public static class UIManagerExtension
{
    // ==================== Main Menu 버튼 이벤트 ====================

    /// <summary>
    /// 시작하기 버튼 클릭 시 호출됩니다.
    /// </summary>
    public static void OnStartButtonClicked()
    {
        Debug.Log("[UIManagerExtension] 시작하기 버튼 클릭");

        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.CloseUI("MainMenuGroup");
    }

    /// <summary>
    /// 도감 버튼 클릭 시 호출됩니다.
    /// </summary>
    public static void OnCodexButtonClicked()
    {
        Debug.Log("[UIManagerExtension] 도감 버튼 클릭 → CodexGroup 열기");

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.SwitchUI("MainMenuGroup", "CodexGroup");
        }
    }

    /// <summary>
    /// 종료 버튼 클릭 시 호출됩니다.
    /// </summary>
    public static void OnExitButtonClicked()
    {
        Debug.Log("[UIManagerExtension] 종료 버튼 클릭 → 게임 종료");
        Application.Quit();
    }

    // ==================== BackButton 이벤트 ====================

    /// <summary>
    /// CommonBackButton에서 호출되는 공용 뒤로가기 메서드
    /// </summary>
    public static void OnBackButtonClicked(string previousGroupName = "MainMenuGroup")
    {
        Debug.Log($"[UIManagerExtension] BackButton 클릭 → {previousGroupName}으로 돌아가기");

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.SwitchUI("CodexGroup", previousGroupName);
        }
    }
}