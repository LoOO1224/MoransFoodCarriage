// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCookingDropFlow.cs
// - 역할: 화면 좌표가 어떤 조리도구 영역인지 판정한 결과를 작게 정리합니다.
// - 감독 관점: 재료 배우가 가마솥으로 들어갈지, 도마로 올라갈지 입구에서 안내하는 무대 스태프입니다.
// - 유지보수 포인트: 포인터 판정 자체는 각 조리도구 컴포넌트가 담당하고, 이 클래스는 흐름만 정리합니다.
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
