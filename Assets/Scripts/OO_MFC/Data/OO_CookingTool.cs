// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OO_CookingTool.cs
// - 역할: 엑셀/JSON에서 읽어오는 조리도구 static data의 그릇입니다.
// - 감독 관점: 가마솥과 도마 배우에게 붙는 역할표입니다. 어떤 소품을 받을지 여기 적어 둡니다.
// - 유지보수 포인트: 새 조리도구를 추가할 때 Controller를 고치지 말고 OO_CookingTool.xlsx에 행을 추가합니다.
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
