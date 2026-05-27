using UnityEngine;

public class CodexBackButtonInitializer : MonoBehaviour
{
    private void Awake()
    {
        BackButtonController backButton = GetComponent<BackButtonController>();
        if (backButton != null)
        {
            backButton.SetPreviousGroup("MainMenuGroup");
            Debug.Log("[CodexBackButtonInitializer] BackButton에 MainMenuGroup 등록 완료");
        }
        else
        {
            Debug.LogError("[CodexBackButtonInitializer] BackButtonController를 찾을 수 없습니다.");
        }
    }
}