// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechCookingDropFlow.cs
// - 역할: CookingGroup의 조리 입력, 도구 판정, 인벤토리 연동을 나누어 담당합니다.
// - 유지보수: Stage3/Stage4 발표용 진행 보험이 섞여 있으므로 제거 전 실제 리허설 흐름을 반드시 확인합니다.
// =============================================================================

public enum OOTechCookingDropToolType
{
    None,
    Cauldron,
    Cuttingboard,
    Julgu
}

public class OOTechCookingDropFlow
{
    /// <summary>
    /// 가마솥과 도마 판정 결과를 하나의 드롭 타입으로 정리합니다.
    /// </summary>
    public OOTechCookingDropToolType RequestResolveDropTool(bool isInsideCauldron, bool isInsideCuttingboard)
    {
        if (isInsideCauldron)
            return OOTechCookingDropToolType.Cauldron;

        if (isInsideCuttingboard)
            return OOTechCookingDropToolType.Cuttingboard;

        return OOTechCookingDropToolType.None;
    }

    /// <summary>
    /// 가마솥, 도마, 절구 판정 결과를 하나의 드롭 타입으로 정리합니다.
    /// Stage3부터는 쌀을 절구에 넣어 떡을 만들 수 있으므로 세 번째 조리도구를 지원합니다.
    /// </summary>
    public OOTechCookingDropToolType RequestResolveDropTool(bool isInsideCauldron, bool isInsideCuttingboard, bool isInsideJulgu)
    {
        if (isInsideCauldron)
            return OOTechCookingDropToolType.Cauldron;

        if (isInsideCuttingboard)
            return OOTechCookingDropToolType.Cuttingboard;

        if (isInsideJulgu)
            return OOTechCookingDropToolType.Julgu;

        return OOTechCookingDropToolType.None;
    }
}
