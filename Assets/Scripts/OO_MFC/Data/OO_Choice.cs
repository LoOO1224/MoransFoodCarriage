// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OO_Choice.cs
// - 역할: JSON/Excel에서 로드되는 정적 데이터 한 행을 표현합니다.
// - 유지보수: 필드명은 JsonConverter 결과와 맞아야 하므로 이름 변경 시 Excel, JSON, GameDataManager 매핑을 함께 확인합니다.
// =============================================================================
using System;
using System.Collections.Generic;

/// <summary>
/// DialogueGroup을 재사용하되, 선택지가 필요한 순간에만 ChoicePanel을 켜기 위한 데이터입니다.
/// Game View에서는 DialoguePanel 본문 아래에 예/아니오 버튼이 붙어 플레이어 선택을 받습니다.
/// </summary>
[Serializable]
public class OO_Choice : GameDataBase
{
    public string SpeakerId;                       // 말하는 화자 ID
    public string SpeakerName;                     // 화면에 표시할 화자 이름
    public string PromptText;                      // 선택지 앞에 보여줄 문장
    public string ChoiceMode;                      // YesNo, Multiple 같은 선택 방식
    public int OptionCount;                        // 선택지 개수
    public List<string> OptionKeyList;             // Y, N 같은 키 힌트
    public List<string> OptionTextList;            // 수확한다, 그만둔다 같은 버튼 문구
    public List<string> ResultTypeList;            // AddItem, Close 같은 결과 타입
    public List<string> ResultValueList;           // 결과에 필요한 데이터 ID
    public List<int> ResultCountList;              // 결과 수량
    public List<string> NextDialogueIdList;        // 선택 후 이어질 대사 ID
    public List<string> NextChoiceIdList;          // 선택 후 이어질 선택지 ID
    public List<string> StageQuestIdList;          // 선택 후 갱신할 임무 ID
    public string Memo;                            // 기획 메모
}
