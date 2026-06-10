// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OO_Stage3CueSheet.cs
// - 역할: JSON/Excel에서 로드되는 정적 데이터 한 행을 표현합니다.
// - 유지보수: 필드명은 JsonConverter 결과와 맞아야 하므로 이름 변경 시 Excel, JSON, GameDataManager 매핑을 함께 확인합니다.
// =============================================================================
using System;
using System.Collections.Generic;

/// <summary>
/// Stage3 산군 장면 큐시트 데이터입니다.
/// Game View에서는 마차 입장, 산군 등장, EncounterGroup 복귀 선택지, 최종 보상 흐름의 기준값으로 쓰입니다.
/// </summary>
[Serializable]
public class OO_Stage3CueSheet : GameDataBase
{
    public string StageId;                         // Stage 데이터 ID
    public string RoadGroupId;                     // 이전 RoadGroup 이름
    public string RoadMissionDataId;               // Road mission data ID
    public string RoadMissionFallbackText;         // Road mission fallback text
    public string StageGroupId;                    // Stage3Group 이름
    public string EncounterGroupId;                // EncounterGroup 이름
    public string MFCRoleId;                       // 마차 역할 ID
    public string SangunRoleId;                    // 산군 역할 ID
    public string MoranRoleId;                     // EncounterGroup Moran 역할 ID
    public string MrJaeikRoleId;                   // EncounterGroup Mr.Jaeik 역할 ID
    public string EntryPointAId;                   // 마차 도착 지점 ID

    public string SangunFirstDialogueId;           // Stage3Group 첫 산군 대사 ID
    public string EncounterSangunDialogueId;       // EncounterGroup 첫 산군 대사 ID
    public string EncounterMoranDialogueId;        // EncounterGroup Moran 대사 ID
    public string EncounterQuestDialogueId;        // EncounterGroup 퀘스트 요구 대사 ID
    public string ClearDialogueId;                 // 꿀떡 전달 후 임무 완료 전에 재생할 산군 대사 ID
    public string StageQuestId;                    // Stage3 임무 데이터 ID

    public string JulguToolId;                     // 절구 도구 ID
    public string JulguTutorialId;                 // 절구 안내 튜토리얼 ID
    public string KoreanCakeItemId;                // 떡 아이템 ID
    public string HoneyIngredientId;               // 꿀 재료 ID
    public string HoneyKoreanCakeItemId;           // 꿀떡 아이템 ID
    public string CakeOnlyChoiceId;                // 떡만 있을 때 선택지 ID
    public string MakeHoneyCakeChoiceId;           // 떡+꿀 조합 선택지 ID
    public string GiveHoneyCakeChoiceId;           // 꿀떡 전달 선택지 ID
    public string DeathRetryChoiceId;              // 실패 후 재시도 선택지 ID
    public List<string> ClearRewardItemIdList;     // 클리어 보상 아이템 ID 목록
    public List<int> ClearRewardCountList;         // 클리어 보상 수량 목록
    public string NextRoadGroupName;               // 다음 RoadGroup 이름

    public float SangunStartScaleRatio;            // 산군 시작 크기 비율
    public float SangunThreateningScaleRatio;      // 산군 위협 애니메이션 시작 크기 비율
    public float SangunAppearSeconds;              // 산군 등장 시간
    public float ThreateningAnimationSpeed;        // 위협 애니메이션 속도
    public float AttackingAnimationSpeed;          // 공격 애니메이션 속도
    public float EncounterWaitSeconds;             // EncounterGroup 첫 대사 전 대기 시간
    public string DeathMessage;                    // 실패 연출 문구
}
