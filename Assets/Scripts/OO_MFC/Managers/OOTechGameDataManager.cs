using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OOTechGameDataManager : MonoBehaviour
{
    public static OOTechGameDataManager Inst { get; private set; }

    private void Awake()
    {
        if (Inst != null && Inst != this)
        {
            Destroy(gameObject);
            return;
        }
        Inst = this;
        DontDestroyOnLoad(gameObject);

        LoadAllData();
    }

    // ==================== 데이터 로드 ====================
    public void LoadAllData()
    {
        Debug.Log("[OOTechGameDataManager] 모든 데이터 로드 시작");

        // TODO: 실제 JSON 로드 로직은 나중에 구현
        // 현재는 빈 상태로 시작
    }

    // ==================== 데이터 조회 메서드 (예시) ====================
    public T GetData<T>(string id) where T : class
    {
        // TODO: 실제 데이터 반환 로직 구현
        Debug.LogWarning($"[GetData] 아직 구현되지 않음 - ID: {id}");
        return null;
    }

    // ==================== 나중에 추가할 메서드 예시 ====================
    // public IngredientData GetIngredientData(string id) { ... }
    // public RecipeData GetRecipeData(string id) { ... }
    // public StageData GetStageData(int stageId) { ... }
}