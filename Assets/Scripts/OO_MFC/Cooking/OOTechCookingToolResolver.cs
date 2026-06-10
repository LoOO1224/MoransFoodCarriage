// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechCookingToolResolver.cs
// - 역할: CookingGroup의 조리 입력, 도구 판정, 인벤토리 연동을 나누어 담당합니다.
// - 유지보수: Stage3/Stage4 발표용 진행 보험이 섞여 있으므로 제거 전 실제 리허설 흐름을 반드시 확인합니다.
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
