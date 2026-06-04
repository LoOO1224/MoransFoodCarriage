// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OO_Stage.cs
// - 역할: 엑셀/JSON에서 읽어오는 static data의 그릇입니다.
// - 감독 관점: 기획자가 써 둔 설정표를 배우가 읽을 수 있는 대본 카드로 바꾸는 역할입니다.
// - 유지보수 포인트: 게임 중 변하는 값은 여기에 넣지 말고 Model에 둡니다. JsonUtility 호환 때문에 public field를 허용합니다.
// =============================================================================
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
