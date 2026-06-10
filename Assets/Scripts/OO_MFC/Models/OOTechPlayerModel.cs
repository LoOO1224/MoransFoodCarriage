// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechPlayerModel.cs
// - 역할: 플레이 중 보관되는 런타임 상태 값을 담는 모델입니다.
// - 유지보수: 외부에서 직접 컬렉션 구조를 바꾸기보다 GameManager 계열 API를 통해 상태를 갱신합니다.
// =============================================================================
using System;
using System.Collections.Generic;

[Serializable]
public class OOTechPlayerModel
{
    public string PlayerName = "Player";
    public int PlayerTotalExp = 0;
    public List<OOTechItemModel> Inventory = new List<OOTechItemModel>();
}