// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OO_CookingTool.cs
// - 역할: JSON/Excel에서 로드되는 정적 데이터 한 행을 표현합니다.
// - 유지보수: 필드명은 JsonConverter 결과와 맞아야 하므로 이름 변경 시 Excel, JSON, GameDataManager 매핑을 함께 확인합니다.
// =============================================================================
using System;
using System.Collections.Generic;

[Serializable]
public class OO_CookingTool : GameDataBase
{
    public string Name;                                      // 조리도구 이름
    public string Description;                               // 조리도구 설명
    public List<string> AcceptedIngredientIds;               // 받을 수 있는 재료 ID 목록
    public float DropAreaPadding = 1.18f;                    // 드롭 판정 보정값
    public string GuideTutorialId;                           // 이 도구를 설명할 튜토리얼 ID
    public string ToolRoleId;                                // 무대에서 찾을 역할 ID
}
