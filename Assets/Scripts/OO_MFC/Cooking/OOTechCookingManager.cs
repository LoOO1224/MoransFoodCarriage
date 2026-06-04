// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCookingManager.cs
// - 역할: 요리 시스템의 입력, 조리도구, 레시피 판정을 담당하는 스크립트입니다.
// - 감독 관점: 부엌 장면에서 재료와 조리도구 배우가 어떤 순서로 만나는지 관리합니다.
// - 유지보수 포인트: 재료 규칙은 데이터와 DropTarget 역할표로 빼고, UI 배치는 CookingUIGroup에서 직접 수정합니다.
// =============================================================================
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 요리 레시피 판정을 담당하는 매니저입니다.
/// Game View에서는 가마솥에 들어온 재료 조합이 어떤 음식으로 완성되는지 결정합니다.
/// </summary>
public class OOTechCookingManager : MonoBehaviour
{
    public static OOTechCookingManager Inst { get; private set; }

    /// <summary>
    /// 하나만 존재하는 요리 매니저로 등록합니다.
    /// </summary>
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

    /// <summary>
    /// 투입된 재료 ID 목록을 Recipe 데이터와 비교해 요리 성공/실패 결과를 반환합니다.
    /// </summary>
    public CookingResult TryCook(List<string> ingredientIds)
    {
        Debug.Log("[OOTechCookingManager] 요리 시도");

        if (ingredientIds == null || ingredientIds.Count == 0)
        {
            return new CookingResult
            {
                IsSuccess = false,
                ResultItemId = null,
                FailReason = "재료가 비어 있음"
            };
        }

        OO_Recipe recipeData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.FindRecipeByIngredientList(ingredientIds) : null;

        if (recipeData != null && !string.IsNullOrEmpty(recipeData.ResultItemId))
        {
            return new CookingResult
            {
                IsSuccess = true,
                ResultItemId = recipeData.ResultItemId,
                FailReason = string.Empty
            };
        }

        if (IsFallbackPumpkinPorridgeRecipe(ingredientIds))
        {
            return new CookingResult
            {
                IsSuccess = true,
                ResultItemId = "OO_PumpkinSoup_1",
                FailReason = string.Empty
            };
        }

        if (IsFallbackVegetablePorridgeRecipe(ingredientIds))
        {
            return new CookingResult
            {
                IsSuccess = true,
                ResultItemId = "OO_VegetableSoup_1",
                FailReason = string.Empty
            };
        }

        return new CookingResult
        {
            IsSuccess = false,
            ResultItemId = null,
            FailReason = "맞는 레시피가 없음"
        };
    }

    /// <summary>
    /// 요리 도구 잠금 여부를 확인합니다. 지금은 1차 구현이라 모든 도구를 허용합니다.
    /// </summary>
    public bool IsToolUnlocked(string toolId)
    {
        return true;
    }

    /// <summary>
    /// Recipe 데이터가 비어 있어도 튜토리얼 채소죽은 플레이 가능하도록 남겨둔 안전망입니다.
    /// </summary>
    private bool IsFallbackVegetablePorridgeRecipe(List<string> ingredientIds)
    {
        if (ingredientIds == null || ingredientIds.Count != 2)
            return false;

        return ingredientIds.Contains("Ing_Rice_01") &&
               ingredientIds.Contains("Ing_Veggie_01");
    }

    /// <summary>
    /// Recipe 데이터가 아직 덜 들어왔어도 Stage1 호박죽 제작이 막히지 않게 하는 안전망입니다.
    /// 데이터가 완성되면 OO_Recipe.json 규칙이 우선 적용되고, 이 fallback은 뒤에서 받쳐 줍니다.
    /// </summary>
    private bool IsFallbackPumpkinPorridgeRecipe(List<string> ingredientIds)
    {
        if (ingredientIds == null || ingredientIds.Count != 2)
            return false;

        return ingredientIds.Contains("Ing_Rice_01") &&
               ingredientIds.Contains("Ing_Pumpkin_01");
    }
}
