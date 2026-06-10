// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: CookingResult.cs
// - 역할: CookingGroup의 조리 입력, 도구 판정, 인벤토리 연동을 나누어 담당합니다.
// - 유지보수: Stage3/Stage4 발표용 진행 보험이 섞여 있으므로 제거 전 실제 리허설 흐름을 반드시 확인합니다.
// =============================================================================
using System;

[Serializable]
public class CookingResult
{
    public bool IsSuccess;
    public string ResultItemId;
    public string FailReason;
}