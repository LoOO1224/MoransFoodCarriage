// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechItemModel.cs
// - 역할: 플레이 중 보관되는 런타임 상태 값을 담는 모델입니다.
// - 유지보수: 외부에서 직접 컬렉션 구조를 바꾸기보다 GameManager 계열 API를 통해 상태를 갱신합니다.
// =============================================================================
using System;

[Serializable]
public class OOTechItemModel
{
    public long ItemUniqueId;
    public string ItemDataId;
    public int ItemStackCount;
}