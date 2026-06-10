// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OO_DialogueGroup.cs
// - 역할: JSON/Excel에서 로드되는 정적 데이터 한 행을 표현합니다.
// - 유지보수: 필드명은 JsonConverter 결과와 맞아야 하므로 이름 변경 시 Excel, JSON, GameDataManager 매핑을 함께 확인합니다.
// =============================================================================
using System;
using System.Collections.Generic;

[Serializable]
public class OO_DialogueGroup : GameDataBase
{
    public string Name;                         // 대화 그룹 이름
    public string Description;                  // 대화 그룹 설명
    public List<string> SpeakerCharacterIdList; // 동시에 말하는 캐릭터 ID 목록
    public List<string> SpeakerNameList;        // 캐릭터 데이터가 없을 때 사용할 화자 이름 목록
    public string Text;                         // 동시에 출력할 공통 대사
    public List<string> DialogueIdList;         // 기존 단일 대사 ID 묶음
    public string NextDialogueGroupId;          // 다음 대화 그룹 ID
}
