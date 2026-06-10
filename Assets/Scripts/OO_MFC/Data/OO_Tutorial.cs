// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OO_Tutorial.cs
// - 역할: JSON/Excel에서 로드되는 정적 데이터 한 행을 표현합니다.
// - 유지보수: 필드명은 JsonConverter 결과와 맞아야 하므로 이름 변경 시 Excel, JSON, GameDataManager 매핑을 함께 확인합니다.
// =============================================================================
using System;

[Serializable]
public class OO_Tutorial : GameDataBase
{
    public string Name;                        // 엑셀에서 사용하는 표시 이름
    public string Title;                       // 튜토리얼 제목
    public string Description;                 // 튜토리얼 설명
    public string TargetStageId;               // 해당 튜토리얼이 나오는 스테이지
    public string TriggerCondition;            // 트리거 조건 (FirstCooking 등)
    public string DialogueGroupId;             // 보여줄 대화 그룹 ID
    public string SkillList;                   // 엑셀 공통 컬럼 호환용
    public string UseWeaponId;                 // 엑셀 공통 컬럼 호환용
    public string BasicCostumeId;              // 엑셀 공통 컬럼 호환용
}
