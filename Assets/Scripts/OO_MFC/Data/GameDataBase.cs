// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: GameDataBase.cs
// - 역할: 엑셀/JSON에서 읽어오는 static data의 그릇입니다.
// - 감독 관점: 기획자가 써 둔 설정표를 배우가 읽을 수 있는 대본 카드로 바꾸는 역할입니다.
// - 유지보수 포인트: 게임 중 변하는 값은 여기에 넣지 말고 Model에 둡니다. JsonUtility 호환 때문에 public field를 허용합니다.
// =============================================================================
using System;

/// <summary>
/// OO_MFC JSON data classes share this small base ticket.
/// Think of it as the label on every prop and script page: the managers can
/// find each data row by Id without depending on the disabled class examples.
/// </summary>
[Serializable]
public class GameDataBase
{
    public string Id;
}
