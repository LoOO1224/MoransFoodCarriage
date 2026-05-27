using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Codex BGM")]
    [SerializeField] private AudioClip _codexBGM;

    // ==================== 버튼 이벤트 ====================

    public void OnStartButtonClicked()
    {
        UIManagerExtension.OnStartButtonClicked();
    }

    public void OnCodexButtonClicked()
    {
        UIManagerExtension.OnCodexButtonClicked();

        if (OOTechSoundManager.Inst != null && _codexBGM != null)
            OOTechSoundManager.Inst.PlayBGM(_codexBGM);
    }

    public void OnExitButtonClicked()
    {
        UIManagerExtension.OnExitButtonClicked();
    }
}