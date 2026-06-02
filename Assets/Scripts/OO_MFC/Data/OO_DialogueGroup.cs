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
