// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OO_Recipe.cs
// - 역할: JSON/Excel에서 로드되는 정적 데이터 한 행을 표현합니다.
// - 유지보수: 필드명은 JsonConverter 결과와 맞아야 하므로 이름 변경 시 Excel, JSON, GameDataManager 매핑을 함께 확인합니다.
// =============================================================================
using System;
using System.Collections.Generic;

[Serializable]
public class OO_Recipe : GameDataBase
{
    public string Name;                                      // 레시피 이름
    public string Description;                               // 요리 설명
    public string ResultItemId;                              // 완성 음식 ID
    public List<string> RequiredIngredients;                 // 기존 호환용 필요 재료 ID 목록
    public int MaxDuplicateCount = 1;                        // 기존 호환용 같은 재료 허용 횟수
    public string RequiredTool;                              // 기존 호환용 필요 도구 이름
    public List<string> RequiredIngredientIds;               // 실제 레시피 판정용 재료 ID 목록
    public List<int> RequiredIngredientCounts;               // 실제 레시피 판정용 재료 수량 목록
    public List<string> RequiredToolIds;                     // 각 재료가 올라가야 하는 조리도구 ID 목록
    public int ResultCount = 1;                              // 완성될 음식 수량
    public string QuantityGuideText;                         // 수량이 부족할 때 보여줄 안내 문구
}
