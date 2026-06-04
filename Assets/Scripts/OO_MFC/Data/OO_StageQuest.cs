// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OO_StageQuest.cs
// - 역할: 엑셀/JSON에서 읽어오는 static data의 그릇입니다.
// - 감독 관점: 기획자가 써 둔 설정표를 배우가 읽을 수 있는 대본 카드로 바꾸는 역할입니다.
// - 유지보수 포인트: 게임 중 변하는 값은 여기에 넣지 말고 Model에 둡니다. JsonUtility 호환 때문에 public field를 허용합니다.
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
