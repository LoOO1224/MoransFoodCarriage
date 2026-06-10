// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechCookingIngredientSelectionModel.cs
// - 역할: CookingGroup의 조리 입력, 도구 판정, 인벤토리 연동을 나누어 담당합니다.
// - 유지보수: Stage3/Stage4 발표용 진행 보험이 섞여 있으므로 제거 전 실제 리허설 흐름을 반드시 확인합니다.
// =============================================================================
using System.Collections.Generic;
using UnityEngine;

public class OOTechCookingIngredientSelectionModel
{
    private readonly List<string> _selectedIngredientIdList = new List<string>();
    private readonly Dictionary<string, int> _selectedIngredientAmountDic = new Dictionary<string, int>();
    private readonly Dictionary<string, string> _selectedIngredientToolIdDic = new Dictionary<string, string>();

    public List<string> SelectedIngredientIdList => _selectedIngredientIdList;
    public Dictionary<string, int> SelectedIngredientAmountDic => _selectedIngredientAmountDic;
    public Dictionary<string, string> SelectedIngredientToolIdDic => _selectedIngredientToolIdDic;

    /// <summary>
    /// 조리도구 위에 올라간 재료와 수량을 기록합니다.
    /// Game View에서는 플레이어가 드래그 앤 드롭한 소품이 무대 위에 올라왔다는 기록입니다.
    /// </summary>
    public void RequestRegisterIngredient(string itemDataId, int itemQuantity)
    {
        RequestRegisterIngredient(itemDataId, itemQuantity, string.Empty);
    }

    /// <summary>
    /// 조리도구 위에 올라간 재료, 수량, 도구 ID를 함께 기록합니다.
    /// 쌀은 가마솥에도 들어갈 수 있고 절구에도 들어갈 수 있으므로, 위치 기록이 레시피 판정의 핵심입니다.
    /// </summary>
    public void RequestRegisterIngredient(string itemDataId, int itemQuantity, string toolId)
    {
        if (string.IsNullOrEmpty(itemDataId))
            return;

        if (!_selectedIngredientIdList.Contains(itemDataId))
            _selectedIngredientIdList.Add(itemDataId);

        if (!_selectedIngredientAmountDic.ContainsKey(itemDataId))
            _selectedIngredientAmountDic[itemDataId] = 0;

        _selectedIngredientAmountDic[itemDataId] += Mathf.Max(1, itemQuantity);

        if (!string.IsNullOrEmpty(toolId))
            _selectedIngredientToolIdDic[itemDataId] = toolId;
    }

    /// <summary>
    /// 특정 재료가 현재 몇 개 올라가 있는지 반환합니다.
    /// </summary>
    public int RequestGetIngredientAmount(string itemDataId)
    {
        if (string.IsNullOrEmpty(itemDataId))
            return 0;

        return _selectedIngredientAmountDic.TryGetValue(itemDataId, out int amount) ? amount : 0;
    }

    /// <summary>
    /// 특정 재료가 마지막으로 올라간 조리도구 ID를 반환합니다.
    /// Game View에서는 쌀이 가마솥에 들어간 것인지, 절구에 들어간 것인지 구분하는 판정값입니다.
    /// </summary>
    public string RequestGetIngredientToolId(string itemDataId)
    {
        if (string.IsNullOrEmpty(itemDataId))
            return string.Empty;

        return _selectedIngredientToolIdDic.TryGetValue(itemDataId, out string toolId) ? toolId : string.Empty;
    }

    /// <summary>
    /// 완성 후 조리대 기록을 비웁니다.
    /// </summary>
    public void RequestClear()
    {
        _selectedIngredientIdList.Clear();
        _selectedIngredientAmountDic.Clear();
        _selectedIngredientToolIdDic.Clear();
    }
}
