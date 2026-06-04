// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OO_Tutorial.cs
// - 역할: 엑셀/JSON에서 읽어오는 static data의 그릇입니다.
// - 감독 관점: 기획자가 써 둔 설정표를 배우가 읽을 수 있는 대본 카드로 바꾸는 역할입니다.
// - 유지보수 포인트: 게임 중 변하는 값은 여기에 넣지 말고 Model에 둡니다. JsonUtility 호환 때문에 public field를 허용합니다.
// =============================================================================
using System;

[Serializable]
public class OO_Tutorial : GameDataBase
{
    public string Name;                        // 엑셀에서 사용하는 표시 이름
    public string Title;                       // 튜토리얼 제목
    public string Description;                 // 튜토리얼 설명
    public string TargetStageId;               // 해당 튜토리얼이 나오는 스테이지
    public string TriggerCondition;            // 트리거 조건 (FirstCooking 등)
    public string DialogueGroupId;             // 보여줄 대화 그룹 ID
    public string SkillList;                   // 엑셀 공통 컬럼 호환용
    public string UseWeaponId;                 // 엑셀 공통 컬럼 호환용
    public string BasicCostumeId;              // 엑셀 공통 컬럼 호환용
}
