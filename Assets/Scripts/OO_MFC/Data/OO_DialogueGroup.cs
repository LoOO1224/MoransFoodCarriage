// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OO_DialogueGroup.cs
// - 역할: 엑셀/JSON에서 읽어오는 static data의 그릇입니다.
// - 감독 관점: 기획자가 써 둔 설정표를 배우가 읽을 수 있는 대본 카드로 바꾸는 역할입니다.
// - 유지보수 포인트: 게임 중 변하는 값은 여기에 넣지 말고 Model에 둡니다. JsonUtility 호환 때문에 public field를 허용합니다.
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
