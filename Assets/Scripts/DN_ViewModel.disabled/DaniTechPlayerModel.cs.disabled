using System;
using System.Collections.Generic;
using UnityEngine;

// 2-1) 플레이어 데이터에 습득 아이템을 저장하기 위해 미리 만듦
[Serializable]
public class DaniTechItemModel
{
    public long ItemUniqueId;
    public string ItemDataId;
    public int ItemStackCount;
}

// 1) 플레이어 데이터를 만들어보자
// 1-1) JsonUtility로 직렬화하려면, Mono를 상속받지 않도록 주의하자!
[Serializable]
public class DaniTechPlayerModel
{
    public string PlayerName;
    public int PlayerTotalExp;
    public string LastMapDataId;
    public Vector3 LastMapPosition;

    public List<DaniTechItemModel> ItemList = new List<DaniTechItemModel>();
}
