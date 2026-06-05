// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OO_Stage2CueSheet.cs
// - 역할: Stage2Group의 역할 ID, 대사 ID, 이동 속도, 연출 시간을 담는 정적 데이터 카드입니다.
// - 영화 비유: 무대감독이 배우를 직접 붙잡지 않고, 오늘 공연 큐시트에 적힌 순서와 속도만 읽는 구조입니다.
// - 유지보수 포인트: 게임 중 변하는 값이 아니라 기획표에서 내려오는 값이므로 엑셀 -> JSON -> GameDataManager 흐름으로 관리합니다.
// =============================================================================
using System;
using System.Collections.Generic;

/// <summary>
/// Stage2Group 큐시트 정적 데이터입니다.
/// Game View에서는 배우 역할 ID, 대사 ID, 이동 속도, 카메라/이펙트 시간을 Controller에 공급합니다.
/// </summary>
[Serializable]
public class OO_Stage2CueSheet : GameDataBase
{
    public string StageId;                         // Stage 데이터 ID
    public string MoranRoleId;                     // Moran 배우 역할 ID
    public string MrJaeikRoleId;                   // Mr.Jaeik 배우 역할 ID
    public string ChunyangRoleId;                  // Chunyang 배우 역할 ID
    public string GreedyDuckRoleId;                // GreedyDuck 배우 역할 ID
    public string LeeMongRyongRoleId;              // LeeMongRyong 배우 역할 ID
    public string BackgroundRoleId;                // 배경 역할 ID
    public string ArriveEffectRoleId;              // 등장 이펙트 역할 ID

    public string EntryPointAId;                   // Moran 입장 목표 지점
    public string EntryPointBId;                   // Mr.Jaeik 입장 목표 지점
    public string EntryPointCId;                   // Chunyang 입장 목표 지점
    public string EntryPointDId;                   // LeeMongRyong 입장/초점 지점
    public string EntryPointEId;                   // GreedyDuck 퇴장 목표 지점
    public string TempColliderRoleId;              // GreedyDuck 퇴장용 임시 발판 Collider 역할 ID

    public string RoadMissionDataId;               // Stage2 진입 전 Road 임무 ID
    public string RoadMissionFallbackText;         // Road 임무 fallback 문장
    public string StageQuestDataId;                // Stage2 조리/전달 임무 ID

    public string GreedyDuckFirstDialogueId;       // 첫 탐관오리 대사
    public string GreedyDuckSecondDialogueId;      // LeeMongRyong 등장 전 탐관오리 대사
    public string LeeMongRyongFirstDialogueId;     // LeeMongRyong 첫 대사
    public string LeeMongRyongSecondDialogueId;    // LeeMongRyong 퀘스트 안내 대사
    public string MoranQuestDialogueId;            // Moran 퀘스트 수락 대사
    public string GreedyDuckFinalDialogueId;       // 김치찌개 전달 후 탐관오리 대사
    public List<string> EndingDialogueIdList;      // 엔딩 대사 ID 목록

    public string KimchiStewCookId;                // 완료에 필요한 음식 ID
    public string HoneyIngredientId;               // Stage2 보상 재료 ID
    public int HoneyRewardCount;                   // 보상 수량
    public List<string> StageClearRewardItemIdList;// Stage2 클리어 보상 ID 목록
    public List<int> StageClearRewardCountList;    // Stage2 클리어 보상 수량 목록

    public string NextRoadGroupName;               // 다음 RoadGroup 이름
    public string PlaceholderCanvasName;           // 넘어가기 버튼 Canvas 이름
    public string NextButtonName;                  // 넘어가기 버튼 이름

    public float EntryMoveSpeed;                   // 배우 입장 속도
    public float GreedyDuckEscapeSpeed;            // 탐관오리 퇴장 이동 속도
    public float GreedyDuckExitTimeoutSeconds;     // 탐관오리 퇴장 대기 최대 시간
    public float GreedyDuckEscapeAnimationSpeed;   // 탐관오리 퇴장 애니메이션 속도
    public float ForcedDialogueSeconds;            // 강제 종료 대사 시간
    public float FinalDuckDialogueSeconds;         // 최종 탐관오리 대사 시간
    public float ArriveEffectSeconds;              // 등장 이펙트 지속 시간
    public float ArriveEffectAnimationSpeed;       // 등장 이펙트 Animator 속도
    public float CameraMoveSeconds;                // 카메라 이동 시간
    public float CameraZoomSize;                   // 카메라 클로즈업 크기

    public string GreedyDuckFallbackObjectName;    // GreedyDuck fallback 오브젝트 이름
    public float GreedyDuckInteractionDistance;    // GreedyDuck 상호작용 거리
    public int GreedyDuckVisibleSortingOrder;      // GreedyDuck 최소 SortingOrder
    public float GreedyDuckVisibilityCheckInterval;// GreedyDuck 표시 보정 체크 주기
    public string InteractionKey;                  // 상호작용 키 이름
}
