// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OO_Character.cs
// - 역할: 엑셀/JSON에서 읽어오는 static data의 그릇입니다.
// - 감독 관점: 기획자가 써 둔 설정표를 배우가 읽을 수 있는 대본 카드로 바꾸는 역할입니다.
// - 유지보수 포인트: 게임 중 변하는 값은 여기에 넣지 말고 Model에 둡니다. JsonUtility 호환 때문에 public field를 허용합니다.
// =============================================================================
using System;
using System.Collections.Generic;

[Serializable]
public class OO_Character : GameDataBase
{
    public string Name;                    // ĳ ̸
    public string Description;             // ĳ   丮 
    public string ProfileImagePath;        //  ̹ 
    public string BasicCostumeId;          // ⺻ ǻ ID
    public List<string> DefaultSkillList;  // ⺻ ų 
}