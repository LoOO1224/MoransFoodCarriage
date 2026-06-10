// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OO_CookingCueSheet.cs
// - 역할: JSON/Excel에서 로드되는 정적 데이터 한 행을 표현합니다.
// - 유지보수: 필드명은 JsonConverter 결과와 맞아야 하므로 이름 변경 시 Excel, JSON, GameDataManager 매핑을 함께 확인합니다.
// =============================================================================
using System;

[Serializable]
public class OO_CookingCueSheet : GameDataBase
{
    public string CauldronTutorialId;                        // 가마솥 튜토리얼 ID
    public string CuttingboardTutorialId;                    // 도마 튜토리얼 ID
    public string JulguTutorialId;                           // 절구 튜토리얼 ID
    public string CauldronToolId;                            // 가마솥 도구 데이터 ID
    public string CuttingboardToolId;                        // 도마 도구 데이터 ID
    public string JulguToolId;                               // 절구 도구 데이터 ID
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
