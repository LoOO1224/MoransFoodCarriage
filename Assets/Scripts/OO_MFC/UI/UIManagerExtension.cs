using UnityEngine;

/// <summary>
/// UIManagerExtension
/// 버튼 이벤트처럼 콘텐츠별로 추가되는 UI 흐름을 모아두는 확장 클래스입니다.
/// UIManager는 Open / Close의 핵심 기능만 담당하고,
/// 실제 화면 전환 시나리오는 이 클래스에서 조합합니다.
/// </summary>
public static class UIManagerExtension
{
    // ==================== Main Menu 버튼 이벤트 ====================

    /// <summary>
    /// 시작하기 버튼 클릭 시 메인 메뉴를 닫고 프롤로그와 대화창을 엽니다.
    /// 프롤로그 내부의 실제 나레이션 시작은 PrologueController.OnEnable에서 처리합니다.
    /// </summary>
    public static void OnStartButtonClicked()
    {
        Debug.Log("[UIManagerExtension] 시작하기 버튼 클릭 → Prologue1Group 열기");

        if (OOTechUIManager.Inst == null)
        {
            Debug.LogError("[UIManagerExtension] OOTechUIManager를 찾을 수 없습니다.");
            return;
        }

        OOTechUIManager.Inst.SwitchUI("MainMenuGroup", "Prologue1Group");
        OOTechUIManager.Inst.OpenUI("DialogueGroup");
    }

    /// <summary>
    /// 도감 버튼 클릭 시 메인 메뉴를 닫고 도감 UI를 엽니다.
    /// </summary>
    public static void OnCodexButtonClicked()
    {
        Debug.Log("[UIManagerExtension] 도감 버튼 클릭 → CodexGroup 열기");

        if (OOTechUIManager.Inst == null)
        {
            Debug.LogError("[UIManagerExtension] OOTechUIManager를 찾을 수 없습니다.");
            return;
        }

        OOTechUIManager.Inst.SwitchUI("MainMenuGroup", "CodexGroup");
    }

    /// <summary>
    /// 종료 버튼 클릭 시 게임을 종료합니다.
    /// </summary>
    public static void OnExitButtonClicked()
    {
        Debug.Log("[UIManagerExtension] 종료 버튼 클릭 → 게임 종료");
        Application.Quit();
    }

    // ==================== BackButton 버튼 이벤트 ====================

    /// <summary>
    /// 공용 뒤로가기 버튼 클릭 시 이전 그룹으로 돌아갑니다.
    /// 현재는 CodexGroup에서 MainMenuGroup으로 돌아가는 용도로 사용합니다.
    /// </summary>
    public static void OnBackButtonClicked(string previousGroupName = "MainMenuGroup")
    {
        Debug.Log($"[UIManagerExtension] BackButton 클릭 → {previousGroupName}으로 돌아가기");

        if (OOTechUIManager.Inst == null)
        {
            Debug.LogError("[UIManagerExtension] OOTechUIManager를 찾을 수 없습니다.");
            return;
        }

        OOTechUIManager.Inst.SwitchUI("CodexGroup", previousGroupName);
    }
}
