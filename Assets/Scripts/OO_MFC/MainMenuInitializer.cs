using UnityEngine;

/// <summary>
/// MainMenuGroup이 Awake 될 때 UIManager에 자신을 등록하는 클래스
/// PDF 스타일에 따라 UI 등록을 담당합니다.
/// </summary>
public class MainMenuInitializer : MonoBehaviour
{
    private void Awake()
    {
        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.RegisterUI("MainMenuGroup", gameObject);
            Debug.Log("[MainMenuInitializer] MainMenuGroup 등록 완료");
        }
        else
        {
            Debug.LogError("[MainMenuInitializer] OOTechUIManager를 찾을 수 없습니다.");
        }

        // 등록 상태 확인용 (디버깅)
        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.PrintRegisteredUI();
        }
    }
}