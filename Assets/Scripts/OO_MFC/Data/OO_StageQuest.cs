// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OO_StageQuest.cs
// - 역할: JSON/Excel에서 로드되는 정적 데이터 한 행을 표현합니다.
// - 유지보수: 필드명은 JsonConverter 결과와 맞아야 하므로 이름 변경 시 Excel, JSON, GameDataManager 매핑을 함께 확인합니다.
// =============================================================================
using System;
using System.Collections.Generic;

/// <summary>
/// StageGroup 안에서 HUD 임무판에 표시할 퀘스트 데이터입니다.
/// 스테이지 감독은 이 역할표를 읽어 플레이어에게 다음 행동을 안내합니다.
/// </summary>
[Serializable]
public class OO_StageQuest : GameDataBase
{
    public string StageId;                     // 이 퀘스트가 속한 스테이지 ID
    public string Name;                        // 임무 제목
    public string Description;                 // 임무 설명
    public string ObjectiveType;               // TalkToNPC, CollectItem 같은 목표 타입
    public string ObjectiveId;                 // 목표 오브젝트 또는 데이터 ID
    public int RequiredCount;                  // 필요한 수량
    public List<string> RewardItemIds;         // 완료 보상 ID 목록
    public string NextGroupId;                 // 완료 후 이동할 그룹 ID
}
