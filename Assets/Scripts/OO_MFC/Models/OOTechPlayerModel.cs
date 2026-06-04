// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechPlayerModel.cs
// - 역할: 플레이 중 변하고 저장될 수 있는 runtime model 데이터입니다.
// - 감독 관점: 관객이 플레이하면서 바꾼 상태를 기록하는 제작 노트입니다.
// - 유지보수 포인트: 엑셀에서 고정되는 설명/이름은 Data에 두고, 수량/보유 여부/진행도처럼 변하는 값만 여기에 둡니다.
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