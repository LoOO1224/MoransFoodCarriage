// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCookingToolResolver.cs
// - 역할: OO_CookingTool 데이터를 실제 Cauldron/Cuttingboard 컴포넌트에 적용합니다.
// - 감독 관점: 조리도구 배우에게 오늘 공연의 역할표를 붙여 주는 조감독입니다.
// - 유지보수 포인트: 도구별 허용 재료는 코드 배열보다 OO_CookingTool.xlsx를 우선합니다.
// =============================================================================
using UnityEngine;

public class OOTechCookingToolResolver
{
    /// <summary>
    /// 조리도구 데이터가 있으면 DropTarget 컴포넌트에 적용하고, 없으면 기존 fallback을 유지합니다.
    /// </summary>
    public OO_CookingTool RequestSetupTool(OOTechCookingToolDropTarget toolTarget, string toolId, string fallbackName, string[] fallbackAcceptedIngredientIdArray, float fallbackPadding)
    {
        OO_CookingTool toolData = null;

        if (OOTechGameDataManager.Inst != null)
            toolData = OOTechGameDataManager.Inst.GetCookingToolData(toolId);

        string displayName = toolData != null && !string.IsNullOrEmpty(toolData.Name) ? toolData.Name : fallbackName;
        string[] acceptedIngredientIdArray = toolData != null && toolData.AcceptedIngredientIds != null && toolData.AcceptedIngredientIds.Count > 0
            ? toolData.AcceptedIngredientIds.ToArray()
            : fallbackAcceptedIngredientIdArray;
        float dropAreaPadding = toolData != null && toolData.DropAreaPadding > 0f ? toolData.DropAreaPadding : fallbackPadding;

        if (toolTarget != null)
            toolTarget.RequestSetupTool(toolId, displayName, acceptedIngredientIdArray, dropAreaPadding);

        return toolData;
    }
}
