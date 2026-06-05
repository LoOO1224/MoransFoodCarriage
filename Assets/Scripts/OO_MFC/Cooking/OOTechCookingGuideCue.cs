// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCookingGuideCue.cs
// - 역할: CookingGroup의 가마솥/도마 튜토리얼 문장을 데이터에서 꺼내는 안내 큐 담당입니다.
// - 영화 비유: 부엌 장면에서 관객에게 어떤 소품을 봐야 하는지 알려 주는 진행 멘트 담당입니다.
// - 유지보수 포인트: UI 배치는 View/Controller가 유지하고, 안내 문장 선택은 이 컴포넌트가 맡습니다.
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
