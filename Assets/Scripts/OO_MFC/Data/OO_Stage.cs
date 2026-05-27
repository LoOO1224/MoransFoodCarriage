using System;
using System.Collections.Generic;

[Serializable]
public class OO_Stage : GameDataBase
{
    public string Name;                        // 스테이지 이름 (예: 동쪽의 굶주린 마을)
    public int StageNumber;                    // 스테이지 번호
    public string Description;                 // 스테이지 설명
    public string BackgroundImagePath;         // 배경 이미지 경로
    public string BGMPath;                     // 배경 음악 경로
    public string RequiredPreviousStageId;     // 선행 스테이지 ID
    public List<string> RewardItemIds;         // 클리어 보상 아이템 ID 목록
    public string StartDialogueGroupId;        // 시작 대화 그룹 ID
}