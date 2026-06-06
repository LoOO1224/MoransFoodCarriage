// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OO_Stage3CueSheet.cs
// - 역할: Stage3Group과 EncounterGroup의 대사, 선택지, 보상, 이동 큐를 담는 정적 데이터 카드입니다.
// - 영화 비유: 산군 장면의 콘티와 큐시트입니다. 감독은 이 표를 읽고 배우 컴포넌트에게 순서만 지시합니다.
// - 유지보수 포인트: 대사 ID, 선택지 ID, 보상 수량은 코드에 박지 않고 엑셀 -> JSON -> GameDataManager로 관리합니다.
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
    public string StageGroupId;                    // Stage3Group 이름
    public string EncounterGroupId;                // EncounterGroup 이름
    public string MFCRoleId;                       // 마차 역할 ID
    public string SangunRoleId;                    // 산군 역할 ID
    public string MoranRoleId;                     // EncounterGroup Moran 역할 ID
    public string MrJaeikRoleId;                   // EncounterGroup Mr.Jaeik 역할 ID
    public string EntryPointAId;                   // 마차 도착 지점

    public string SangunFirstDialogueId;           // Stage3Group 첫 산군 대사
    public string EncounterSangunDialogueId;       // EncounterGroup 산군 대사
    public string EncounterMoranDialogueId;        // EncounterGroup Moran 대사
    public string EncounterQuestDialogueId;        // EncounterGroup 퀘스트 요구 대사
    public string StageQuestId;                    // Stage3 임무 데이터 ID

    public string JulguToolId;                     // 절구 도구 ID
    public string JulguTutorialId;                 // 절구 안내 튜토리얼 ID
    public string KoreanCakeItemId;                // 떡 아이템 ID
    public string HoneyIngredientId;               // 꿀 재료 ID
    public string HoneyKoreanCakeItemId;           // 꿀떡 아이템 ID
    public string CakeOnlyChoiceId;                // 떡만 있을 때 선택지
    public string MakeHoneyCakeChoiceId;           // 떡+꿀 조합 선택지
    public string GiveHoneyCakeChoiceId;           // 꿀떡 전달 선택지
    public string DeathRetryChoiceId;              // 사망 후 재시도 선택지
    public List<string> ClearRewardItemIdList;     // 클리어 보상 아이템 ID 목록
    public List<int> ClearRewardCountList;         // 클리어 보상 수량 목록
    public string NextRoadGroupName;               // 다음 RoadGroup 이름

    public float SangunStartScaleRatio;            // 산군 시작 크기 비율
    public float SangunThreateningScaleRatio;      // 위협 애니메이션 시작 크기 비율
    public float SangunAppearSeconds;              // 산군 등장 시간
    public float ThreateningAnimationSpeed;        // 위협 애니메이션 속도
    public float AttackingAnimationSpeed;          // 공격 애니메이션 속도
    public float EncounterWaitSeconds;             // EncounterGroup 첫 대사 전 대기 시간
    public string DeathMessage;                    // 실패 연출 문구
}
