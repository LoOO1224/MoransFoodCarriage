using System;
using System.Collections.Generic;

[Serializable]
public class OOTechPlayerModel
{
    public string PlayerName = "Player";
    public int PlayerTotalExp = 0;
    public List<OOTechItemModel> Inventory = new List<OOTechItemModel>();
}