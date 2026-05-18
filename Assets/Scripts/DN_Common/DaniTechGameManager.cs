using System.Collections.Generic;
using UnityEngine;

public class DaniTechGameManager : MonoBehaviour
{
    public static DaniTechGameManager Inst { get; set; }

    // 플레이 중에 저장되어야 하는 정보들이 있는 위치
    private DaniTechPlayerModel _playerModel = new DaniTechPlayerModel();

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        LoadSaveData();
    }

    public void SaveData()
    {
        DaniTechNetworkManager.Inst.RequstSaveData(_playerModel);
    }

    public void SaveAndEndGame()
    {
        SaveData();
        Application.Quit();
    }

    private void LoadSaveData()
    {
        _playerModel = DaniTechNetworkManager.Inst.RequstLoadSaveData();
    }

    public void IncreasePlayerExp(int exp)
    {
        // 추후에 한곳에서 관리할 수 있게 익스텐션으로 빼도 된다
        _playerModel.PlayerTotalExp += exp;
    }
}
