// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCookingRecipeService.cs
// - 역할: OO_Recipe 데이터로 요리 가능 여부, 필요 수량, 소비 수량을 판정합니다.
// - 감독 관점: 배우가 올린 재료 소품이 대본의 레시피와 맞는지 확인하는 조리 연출 조감독입니다.
// - 유지보수 포인트: 새 음식은 이 코드가 아니라 OO_Recipe.xlsx에 행을 추가해 확장합니다.
// =============================================================================
using System.Collections.Generic;
using UnityEngine;

public class OOTechCookingRecipeService
{
    /// <summary>
    /// 결과 음식 ID로 레시피 데이터를 찾습니다.
    /// </summary>
    public OO_Recipe RequestFindRecipeByResultItemId(string resultItemId)
    {
        if (OOTechGameDataManager.Inst == null || string.IsNullOrEmpty(resultItemId))
            return null;

        List<OO_Recipe> recipeList = OOTechGameDataManager.Inst.GetRecipeDataList();

        foreach (OO_Recipe recipeData in recipeList)
        {
            if (recipeData != null && recipeData.ResultItemId == resultItemId)
                return recipeData;
        }

        return null;
    }

    /// <summary>
    /// 지금 재료를 하나 더 올려도 어떤 레시피의 중간 단계로 유효한지 확인합니다.
    /// </summary>
    public bool RequestCanAcceptIngredient(string itemDataId, OOTechCookingIngredientSelectionModel selectionModel)
    {
        if (selectionModel == null || string.IsNullOrEmpty(itemDataId))
            return false;

        if (selectionModel.SelectedIngredientAmountDic.ContainsKey(itemDataId))
            return true;

        List<OO_Recipe> recipeList = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetRecipeDataList() : new List<OO_Recipe>();

        foreach (OO_Recipe recipeData in recipeList)
        {
            if (RequestIsPartialMatch(recipeData, selectionModel, itemDataId))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 레시피에 필요한 수량이 모두 올라왔는지 확인하고 완성 수량을 반환합니다.
    /// </summary>
    public int RequestCalculateResultCount(string resultItemId, OOTechCookingIngredientSelectionModel selectionModel)
    {
        OO_Recipe recipeData = RequestFindRecipeByResultItemId(resultItemId);

        if (recipeData == null)
            return RequestCalculateFallbackResultCount(resultItemId, selectionModel);

        if (!RequestCanCompleteRecipe(recipeData, selectionModel))
            return 0;

        return Mathf.Max(1, recipeData.ResultCount);
    }

    /// <summary>
    /// 완성에 실제로 소비할 재료 수량표를 만듭니다.
    /// </summary>
    public Dictionary<string, int> RequestCreateUsedIngredientAmountDic(string resultItemId, int resultCount, OOTechCookingIngredientSelectionModel selectionModel)
    {
        Dictionary<string, int> usedAmountDic = new Dictionary<string, int>();
        OO_Recipe recipeData = RequestFindRecipeByResultItemId(resultItemId);

        if (recipeData != null)
        {
            List<string> ingredientIdList = RequestGetRequiredIngredientIdList(recipeData);
            List<int> ingredientCountList = RequestGetRequiredIngredientCountList(recipeData, ingredientIdList.Count);

            for (int index = 0; index < ingredientIdList.Count; index++)
                usedAmountDic[ingredientIdList[index]] = Mathf.Max(1, ingredientCountList[index]);

            return usedAmountDic;
        }

        if (selectionModel == null)
            return usedAmountDic;

        foreach (string ingredientId in selectionModel.SelectedIngredientIdList)
            usedAmountDic[ingredientId] = 1;

        return usedAmountDic;
    }

    /// <summary>
    /// 수량이 부족할 때 안내 문구를 데이터에서 가져옵니다.
    /// </summary>
    public string RequestGetQuantityGuide(string resultItemId)
    {
        OO_Recipe recipeData = RequestFindRecipeByResultItemId(resultItemId);

        if (recipeData != null && !string.IsNullOrEmpty(recipeData.QuantityGuideText))
            return recipeData.QuantityGuideText;

        return "재료 수량이 부족합니다. 슬롯 위에서 Ctrl을 누른 채 마우스 휠로 집기 수량을 조절하세요.";
    }

    private bool RequestIsPartialMatch(OO_Recipe recipeData, OOTechCookingIngredientSelectionModel selectionModel, string nextIngredientId)
    {
        if (recipeData == null || selectionModel == null)
            return false;

        List<string> ingredientIdList = RequestGetRequiredIngredientIdList(recipeData);
        List<int> ingredientCountList = RequestGetRequiredIngredientCountList(recipeData, ingredientIdList.Count);
        Dictionary<string, int> requiredCountDic = RequestCreateRequiredCountDic(ingredientIdList, ingredientCountList);
        Dictionary<string, int> candidateCountDic = new Dictionary<string, int>(selectionModel.SelectedIngredientAmountDic);

        if (!candidateCountDic.ContainsKey(nextIngredientId))
            candidateCountDic[nextIngredientId] = 0;

        candidateCountDic[nextIngredientId]++;

        foreach (KeyValuePair<string, int> pair in candidateCountDic)
        {
            if (!requiredCountDic.TryGetValue(pair.Key, out int requiredCount))
                return false;

            if (pair.Value > requiredCount)
                return false;
        }

        return true;
    }

    private bool RequestCanCompleteRecipe(OO_Recipe recipeData, OOTechCookingIngredientSelectionModel selectionModel)
    {
        if (recipeData == null || selectionModel == null)
            return false;

        List<string> ingredientIdList = RequestGetRequiredIngredientIdList(recipeData);
        List<int> ingredientCountList = RequestGetRequiredIngredientCountList(recipeData, ingredientIdList.Count);

        for (int index = 0; index < ingredientIdList.Count; index++)
        {
            string ingredientId = ingredientIdList[index];
            int requiredCount = Mathf.Max(1, ingredientCountList[index]);

            if (selectionModel.RequestGetIngredientAmount(ingredientId) < requiredCount)
                return false;
        }

        return true;
    }

    private List<string> RequestGetRequiredIngredientIdList(OO_Recipe recipeData)
    {
        if (recipeData.RequiredIngredientIds != null && recipeData.RequiredIngredientIds.Count > 0)
            return recipeData.RequiredIngredientIds;

        return recipeData.RequiredIngredients != null ? recipeData.RequiredIngredients : new List<string>();
    }

    private List<int> RequestGetRequiredIngredientCountList(OO_Recipe recipeData, int requiredCount)
    {
        if (recipeData.RequiredIngredientCounts != null && recipeData.RequiredIngredientCounts.Count > 0)
            return recipeData.RequiredIngredientCounts;

        List<int> fallbackCountList = new List<int>();

        for (int index = 0; index < requiredCount; index++)
            fallbackCountList.Add(1);

        return fallbackCountList;
    }

    private Dictionary<string, int> RequestCreateRequiredCountDic(List<string> ingredientIdList, List<int> ingredientCountList)
    {
        Dictionary<string, int> requiredCountDic = new Dictionary<string, int>();

        for (int index = 0; index < ingredientIdList.Count; index++)
        {
            string ingredientId = ingredientIdList[index];

            if (string.IsNullOrEmpty(ingredientId))
                continue;

            int amount = index < ingredientCountList.Count ? ingredientCountList[index] : 1;
            requiredCountDic[ingredientId] = Mathf.Max(1, amount);
        }

        return requiredCountDic;
    }

    private int RequestCalculateFallbackResultCount(string resultItemId, OOTechCookingIngredientSelectionModel selectionModel)
    {
        if (selectionModel == null)
            return 0;

        if (resultItemId == "OO_KimchiStew_1")
            return selectionModel.RequestGetIngredientAmount("Ing_Kimch_01") >= 10 && selectionModel.RequestGetIngredientAmount("Ing_ChiliPepper_01") >= 10 ? 1 : 0;

        if (resultItemId == "OO_PumpkinSoup_1")
            return selectionModel.RequestGetIngredientAmount("Ing_Rice_01") >= 10 && selectionModel.RequestGetIngredientAmount("Ing_Pumpkin_01") >= 10 ? 10 : 0;

        if (resultItemId == "OO_VegetableSoup_1")
            return selectionModel.RequestGetIngredientAmount("Ing_Rice_01") >= 1 && selectionModel.RequestGetIngredientAmount("Ing_Veggie_01") >= 1 ? 1 : 0;

        if (resultItemId == "OO_KoreanCake_1")
            return selectionModel.RequestGetIngredientAmount("Ing_Rice_01") >= 1 ? 1 : 0;

        if (resultItemId == "OO_CarrotStarch_1")
            return selectionModel.RequestGetIngredientAmount("Ing_Carrot_01") >= 1 && selectionModel.RequestGetIngredientAmount("OO_KoreanCake_1") >= 1 ? 1 : 0;

        if (resultItemId == "OO_CarrotCake_1")
            return selectionModel.RequestGetIngredientAmount("OO_CarrotStarch_1") >= 1 ? 1 : 0;

        return 1;
    }
}
