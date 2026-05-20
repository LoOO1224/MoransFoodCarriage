using System.Collections.Generic;
using UnityEngine;

public class OOTechCookingManager : MonoBehaviour
{
    public static OOTechCookingManager Inst { get; private set; }

    private void Awake()
    {
        if (Inst != null && Inst != this)
        {
            Destroy(gameObject);
            return;
        }
        Inst = this;
        DontDestroyOnLoad(gameObject);
    }

    // ==================== 요리 시도 ====================
    public CookingResult TryCook(List<string> ingredientIds)
    {
        Debug.Log("[OOTechCookingManager] 요리 시도");

        // TODO: 실제 레시피 판정 로직 구현 예정
        // 현재는 항상 실패로 처리 (테스트용)
        return new CookingResult
        {
            IsSuccess = false,
            ResultItemId = null,
            FailReason = "아직 구현되지 않음"
        };
    }

    // ==================== 도구 해금 확인 ====================
    public bool IsToolUnlocked(string toolId)
    {
        // TODO: 실제 해금 여부 확인 로직
        return true; // 임시로 항상 true
    }
}