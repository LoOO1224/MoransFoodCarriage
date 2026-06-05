// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OO_CookingCueSheet.cs
// - 역할: CookingGroup의 공통 연출/가이드 값을 담는 static data의 그릇입니다.
// - 감독 관점: 부엌 장면의 조명, 카메라, 안내 화살표 타이밍을 적어 둔 큐시트입니다.
// - 유지보수 포인트: 숫자와 문구를 코드에 박지 않고 OO_CookingCueSheet.xlsx에서 조정합니다.
// =============================================================================
using System;

[Serializable]
public class OO_CookingCueSheet : GameDataBase
{
    public string CauldronTutorialId;                        // 가마솥 튜토리얼 ID
    public string CuttingboardTutorialId;                    // 도마 튜토리얼 ID
    public string CauldronToolId;                            // 가마솥 도구 데이터 ID
    public string CuttingboardToolId;                        // 도마 도구 데이터 ID
    public int SortingOrder = 1260;                          // Cooking UI 캔버스 정렬 순서
    public float ReferenceResolutionWidth = 1920f;           // UI 기준 해상도 너비
    public float ReferenceResolutionHeight = 1080f;          // UI 기준 해상도 높이
    public float CameraPadding = 1.04f;                      // 부엌 배경을 카메라에 맞출 때 여백
    public float GuideArrowBlinkSpeed = 6f;                  // 가이드 화살표 깜빡임 속도
    public float GuideArrowMinimumAlpha = 0.25f;             // 가이드 화살표 최소 투명도
    public float NewBadgeBlinkSpeed = 7f;                    // NEW 뱃지 깜빡임 속도
    public float NewBadgeMinimumAlpha = 0.25f;               // NEW 뱃지 최소 투명도
    public float DragGhostIconWidth = 88f;                   // 드래그 아이콘 너비
    public float DragGhostIconHeight = 88f;                  // 드래그 아이콘 높이
    public string EmptyPotText;                              // 아직 재료가 없을 때 표시할 문구
    public string DefaultStatusText;                         // 기본 안내 상태 문구
}
