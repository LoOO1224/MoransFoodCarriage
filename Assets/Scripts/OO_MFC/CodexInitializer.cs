using UnityEngine;

/// <summary>
/// CodexGroup이 Awake 될 때 UIManager에 자신을 등록하는 클래스
/// </summary>
public class CodexInitializer : MonoBehaviour
{
    private void Awake()
    {
        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.RegisterUI("CodexGroup", gameObject);
            Debug.Log("[CodexInitializer] CodexGroup 등록 완료");
        }
        else
        {
            Debug.LogError("[CodexInitializer] OOTechUIManager를 찾을 수 없습니다.");
        }

        // 등록 상태 확인용 (디버깅)
        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.PrintRegisteredUI();
        }
    }
}