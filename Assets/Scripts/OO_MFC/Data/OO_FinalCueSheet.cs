// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OO_FinalCueSheet.cs
// - 역할: PreFinal, FinalStage, Epilogue, EndingCredit에서 쓰는 데이터 ID와 그룹 이름을 담습니다.
// - 영화 비유: 마지막 막의 콜시트입니다. 컷신, 최종 보스, 엔딩 크레딧의 호출 순서를 한 장에 적어 둡니다.
// - 유지보수 포인트: YeonSanJa 네이밍 혼용은 이 데이터에서 한 번 정리하고, 코드는 fallback만 둡니다.
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
