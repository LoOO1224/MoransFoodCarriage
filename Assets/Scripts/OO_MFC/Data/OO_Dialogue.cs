using System;
using System.Collections.Generic;

[Serializable]
public class OO_Dialogue : GameDataBase
{
    public string SpeakerName;                    // 화자 이름
    public string Text;                           // 대사 내용
    public string NextDialogueId;                 // 다음 대화 ID
    public List<string> SelectionNameList;        // 선택지 목록
    public List<string> SelectionDialogueIdList;  // 선택지 이동 ID 목록
    public string TexturePath;                    // 캐릭터 일러스트 경로
    public string VoicePath;                      // 음성 파일 경로
}