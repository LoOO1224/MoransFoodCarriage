// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OO_Recipe.cs
// - 역할: 엑셀/JSON에서 읽어오는 static data의 그릇입니다.
// - 감독 관점: 기획자가 써 둔 설정표를 배우가 읽을 수 있는 대본 카드로 바꾸는 역할입니다.
// - 유지보수 포인트: 게임 중 변하는 값은 여기에 넣지 말고 Model에 둡니다. JsonUtility 호환 때문에 public field를 허용합니다.
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
