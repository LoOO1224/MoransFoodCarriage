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
