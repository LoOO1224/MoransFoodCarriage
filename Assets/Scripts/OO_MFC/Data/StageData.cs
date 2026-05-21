using System;
using System.Collections.Generic;

[Serializable]
public class StageData : GameDataBase
{
    public string Name;
    public int StageNumber;
    public string Description;
    public string RequiredPreviousStageId;     // 이전 스테이지 ID (없으면 null)
    public List<string> RewardItemIds;         // 클리어 보상 아이템 ID 목록
    public string StartDialogueGroupId;        // 시작 시 재생할 대화 그룹 ID
}