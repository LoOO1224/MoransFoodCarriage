using System;

[Serializable]
public class OO_Tutorial : GameDataBase
{
    public string Title;                       // 튜토리얼 제목
    public string Description;                 // 튜토리얼 설명
    public string TargetStageId;               // 해당 튜토리얼이 나오는 스테이지
    public string TriggerCondition;            // 트리거 조건 (FirstCooking 등)
    public string DialogueGroupId;             // 보여줄 대화 그룹 ID
}