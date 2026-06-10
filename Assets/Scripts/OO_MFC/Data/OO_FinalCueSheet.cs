// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OO_FinalCueSheet.cs
// - 역할: JSON/Excel에서 로드되는 정적 데이터 한 행을 표현합니다.
// - 유지보수: 필드명은 JsonConverter 결과와 맞아야 하므로 이름 변경 시 Excel, JSON, GameDataManager 매핑을 함께 확인합니다.
// =============================================================================
using System;

/// <summary>
/// 최종 구간 큐시트 데이터입니다.
/// Game View에서는 PreFinal 나레이션부터 EndingCredit 메인 메뉴 복귀까지 이 값을 기준으로 진행됩니다.
/// </summary>
[Serializable]
public class OO_FinalCueSheet : GameDataBase
{
    public string Name;
    public string Description;
    public string PreFinalGroupName;
    public string FinalStageGroupName;
    public string EpilogueGroupName;
    public string EndingCreditGroupName;
    public string MainMenuGroupName;

    public string PreFinalNarrationId;
    public string EpilogueNarrationId;
    public string FinalOpeningDialogueId;
    public string FinalHappyDialogueIdList;

    public string MoranRoleId;
    public string YeonSanJaRoleId;
    public string EndPointRoleId;
    public string FinalStageBGMPath;

    public float MoranMoveSpeed = 180f;
    public float MoranStuckFallbackSeconds = 1.5f;
    public float MoranEndScale = 0.6f;
    public float CameraZoomSize = 280f;
    public float YeonSanJaEatingSpeed = 0.5f;
}
