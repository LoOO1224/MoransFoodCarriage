using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    // ==================== Main Menu 버튼 이벤트 ====================

    public void OnStartButtonClicked()
    {
        UIManagerExtension.OnStartButtonClicked();
    }

    public void OnCodexButtonClicked()
    {
        UIManagerExtension.OnCodexButtonClicked();
    }

    public void OnExitButtonClicked()
    {
        UIManagerExtension.OnExitButtonClicked();
    }
}