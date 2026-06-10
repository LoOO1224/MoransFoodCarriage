// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OO_Ingredient.cs
// - 역할: JSON/Excel에서 로드되는 정적 데이터 한 행을 표현합니다.
// - 유지보수: 필드명은 JsonConverter 결과와 맞아야 하므로 이름 변경 시 Excel, JSON, GameDataManager 매핑을 함께 확인합니다.
// =============================================================================
using System;

[Serializable]
public class OO_Ingredient : GameDataBase
{
    public string Name;                        //  ̸
    public string Description;                 //  
    public string IconPath;                    //  ̹ 
    public string Grade;                       //  (Ϲ, , , ȭ)
    public int MaxStackCount;                  // ִ ø 
}