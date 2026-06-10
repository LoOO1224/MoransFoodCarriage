// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechCookingGuideCue.cs
// - 역할: CookingGroup의 조리 입력, 도구 판정, 인벤토리 연동을 나누어 담당합니다.
// - 유지보수: Stage3/Stage4 발표용 진행 보험이 섞여 있으므로 제거 전 실제 리허설 흐름을 반드시 확인합니다.
// =============================================================================
using UnityEngine;

/// <summary>
/// OO_Tutorial 데이터에서 CookingGroup 가이드 문장을 꺼냅니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechCookingGuideCue : MonoBehaviour
{
    /// <summary>
    /// 튜토리얼 데이터 ID로 제목과 설명을 꺼냅니다.
    /// 데이터가 비어 있으면 리허설용 fallback 문장을 반환합니다.
    /// </summary>
    public void RequestGetGuideData(string tutorialId, string fallbackTitle, string fallbackDescription, out string title, out string description)
    {
        OO_Tutorial tutorialData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetTutorialData(tutorialId) : null;
        title = tutorialData != null && !string.IsNullOrEmpty(tutorialData.Title) ? tutorialData.Title : string.Empty;

        if (string.IsNullOrEmpty(title) && tutorialData != null)
            title = tutorialData.Name;

        if (string.IsNullOrEmpty(title))
            title = fallbackTitle;

        description = tutorialData != null && !string.IsNullOrEmpty(tutorialData.Description)
            ? tutorialData.Description
            : fallbackDescription;
    }
}
