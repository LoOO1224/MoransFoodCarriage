using System;
using System.Collections.Generic;

[Serializable]
public class DialogueData : GameDataBase
{
    public string SpeakerName;                    // 대사하는 사람 이름 (예: 모란, 촌장, 몽령)
    public string Text;                           // 실제 대사 내용
    public string NextDialogueId;                 // 다음 대화 ID (없으면 null)
    public List<string> SelectionNameList;        // 선택지 텍스트 목록
    public List<string> SelectionDialogueIdList;  // 선택지 선택 시 이동할 대화 ID 목록
    public string TexturePath;                    // 캐릭터 일러스트 경로
    public string VoicePath;                      // 음성 파일 경로
}