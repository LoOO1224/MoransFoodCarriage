using System;
using System.Collections.Generic;

/// <summary>
/// StageGroup의 이름, 설명, 배경, 시작 대화, 다음 이동 정보를 담는 정적 데이터입니다.
/// Game View에서는 스테이지 입장 타이틀과 도감/임무 설명의 원본 대본으로 사용됩니다.
/// </summary>
[Serializable]
public class OO_Stage : GameDataBase
{
    public string Name;                        // 스테이지 이름
    public int StageNumber;                    // 스테이지 번호
    public string Description;                 // 스테이지 설명
    public string BackgroundImagePath;         // 배경 이미지 경로
    public string BGMPath;                     // 배경 음악 경로
    public string RequiredPreviousStageId;     // 선행 스테이지 ID
    public List<string> RewardItemIds;         // 클리어 보상 아이템 ID 목록
    public string StartDialogueGroupId;        // 시작 대화 그룹 ID
    public string QuestTitle;                  // 메인 임무 제목
    public string QuestDescription;            // 메인 임무 설명
    public string RequiredCookId;              // 완료에 필요한 음식 ID
    public string NextRoadGroupId;             // 완료 후 이동할 RoadGroup ID
}
