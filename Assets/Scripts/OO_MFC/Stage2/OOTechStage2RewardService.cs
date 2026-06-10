// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechStage2RewardService.cs
// - 역할: Stage2 연출, 대사, 보상, 카메라 큐를 분리해 처리합니다.
// - 유지보수: 큐시트 데이터와 씬 배치 오브젝트가 함께 맞아야 하므로 데이터 ID와 역할 오브젝트를 같이 확인합니다.
// =============================================================================
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stage2의 퀘스트 문장과 보상 처리를 분리한 서비스 컴포넌트입니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechStage2RewardService : MonoBehaviour
{
    /// <summary>
    /// OO_StageQuest에서 임무 설명을 읽어 HUD에 표시합니다.
    /// </summary>
    public void RequestUpdateStageQuest(OOTechRoadHUDController roadHUD, string stageQuestDataId, string fallbackText)
    {
        string questText = RequestResolveStageQuestDescription(stageQuestDataId, fallbackText);

        if (roadHUD == null)
            return;

        roadHUD.RequestSetStageQuestMission(questText);
        roadHUD.SetMissionNewBadgeActive(true);
    }

    /// <summary>
    /// Stage2 클리어 보상을 인벤토리에 넣고 HUD를 갱신합니다.
    /// </summary>
    public void RequestGiveStageClearReward(OOTechRoadHUDController roadHUD, string rewardItemId, int rewardCount)
    {
        RequestGiveStageClearRewardList(roadHUD, new List<string> { rewardItemId }, new List<int> { rewardCount });
    }

    /// <summary>
    /// Stage2 클리어 보상을 목록 단위로 지급합니다.
    /// 영화로 치면 보상 담당자가 큐시트의 선물 목록을 보고 꿀, 한과, 쌀을 한 번에 창고에 넣는 장면입니다.
    /// </summary>
    public void RequestGiveStageClearRewardList(OOTechRoadHUDController roadHUD, List<string> rewardItemIdList, List<int> rewardCountList)
    {
        if (OOTechGameManager.Inst != null && rewardItemIdList != null)
        {
            for (int index = 0; index < rewardItemIdList.Count; index++)
            {
                string rewardItemId = rewardItemIdList[index];

                if (string.IsNullOrWhiteSpace(rewardItemId))
                    continue;

                int rewardCount = 1;

                if (rewardCountList != null && index < rewardCountList.Count)
                    rewardCount = Mathf.Max(1, rewardCountList[index]);

                OOTechGameManager.Inst.AddItem(rewardItemId, rewardCount);
            }
        }

        if (roadHUD == null)
            return;

        roadHUD.RequestRefreshInventoryView();
        roadHUD.SetInventoryNewBadgeActive(true);
        roadHUD.RequestSetStageQuestMission("Stage2 임무 완료: 다음 길로 이동하세요.");
        roadHUD.SetMissionNewBadgeActive(true);
    }

    /// <summary>
    /// Road/Stage 임무 문장을 데이터에서 꺼냅니다.
    /// 데이터가 비어 있으면 리허설용 fallback 문장을 사용합니다.
    /// </summary>
    public string RequestResolveStageQuestDescription(string stageQuestDataId, string fallbackText)
    {
        OO_StageQuest questData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStageQuestData(stageQuestDataId) : null;

        if (questData != null && !string.IsNullOrWhiteSpace(questData.Description))
            return questData.Description;

        return fallbackText;
    }
}
