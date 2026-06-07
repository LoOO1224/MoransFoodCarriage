// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OO_SpeechBubble.cs
// - 역할: 캐릭터 머리 위 말풍선에 출력할 짧은 문장을 담는 static data 카드입니다.
// - 영화 비유: 대형 DialoguePanel 대본이 아니라, 배우가 움직이는 중 작게 속삭이는 애드리브 대본입니다.
// - 유지보수 포인트: 말풍선 문장은 코드에 박지 말고 OO_SpeechBubble.xlsx -> JSON으로 관리합니다.
// =============================================================================
using System;

/// <summary>
/// SpeechBubbleGroup/말풍선 뷰에서 사용하는 데이터입니다.
/// Game View에서는 지정한 오브젝트 머리 위 말풍선에 Text를 한 글자씩 출력합니다.
/// </summary>
[Serializable]
public class OO_SpeechBubble : GameDataBase
{
    public string Name;
    public string Description;
    public string SpeakerCharacterId;      // 말풍선을 말하는 캐릭터 ID
    public string Text;                    // 실제 말풍선 문장
    public float TypingSpeed = 1f;         // 글자 타이핑 속도 배율
    public float DisplaySeconds = 2f;      // 출력 후 유지 시간
    public bool IsLoop;                    // 잠든 토끼처럼 계속 반복할지 여부
    public bool IsWhisper;                 // 속마음/속삭임 연출 여부
}
