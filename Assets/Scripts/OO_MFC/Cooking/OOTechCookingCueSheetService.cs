// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechCookingCueSheetService.cs
// - 역할: CookingGroup의 조리 입력, 도구 판정, 인벤토리 연동을 나누어 담당합니다.
// - 유지보수: Stage3/Stage4 발표용 진행 보험이 섞여 있으므로 제거 전 실제 리허설 흐름을 반드시 확인합니다.
// =============================================================================
using UnityEngine;

public class OOTechCookingCueSheetService
{
    /// <summary>
    /// CookingGroup 큐시트를 ID로 조회합니다. 없으면 null을 돌려 기존 인스펙터 값을 안전망으로 쓰게 합니다.
    /// </summary>
    public OO_CookingCueSheet RequestGetCueSheetData(string cueSheetId)
    {
        if (OOTechGameDataManager.Inst == null)
            return null;

        return OOTechGameDataManager.Inst.GetCookingCueSheetData(cueSheetId);
    }

    /// <summary>
    /// 큐시트의 드래그 아이콘 크기를 Vector2로 바꿔 줍니다.
    /// Game View에서는 플레이어가 집어 든 재료 아이콘의 실제 표시 크기로 쓰입니다.
    /// </summary>
    public Vector2 RequestGetDragGhostSize(OO_CookingCueSheet cueSheetData, Vector2 fallbackSize)
    {
        if (cueSheetData == null)
            return fallbackSize;

        float width = cueSheetData.DragGhostIconWidth > 0f ? cueSheetData.DragGhostIconWidth : fallbackSize.x;
        float height = cueSheetData.DragGhostIconHeight > 0f ? cueSheetData.DragGhostIconHeight : fallbackSize.y;
        return new Vector2(width, height);
    }

    /// <summary>
    /// 큐시트의 기준 해상도를 Vector2로 바꿔 줍니다.
    /// </summary>
    public Vector2 RequestGetReferenceResolution(OO_CookingCueSheet cueSheetData, Vector2 fallbackResolution)
    {
        if (cueSheetData == null)
            return fallbackResolution;

        float width = cueSheetData.ReferenceResolutionWidth > 0f ? cueSheetData.ReferenceResolutionWidth : fallbackResolution.x;
        float height = cueSheetData.ReferenceResolutionHeight > 0f ? cueSheetData.ReferenceResolutionHeight : fallbackResolution.y;
        return new Vector2(width, height);
    }
}
