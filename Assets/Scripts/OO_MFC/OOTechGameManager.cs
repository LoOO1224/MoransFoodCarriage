using System.Collections.Generic;
using UnityEngine;

public class OOTechGameManager : MonoBehaviour
{
    public static OOTechGameManager Inst { get; private set; }

    [SerializeField] private OOTechPlayerModel _playerModel = new OOTechPlayerModel();

    private void Awake()
    {
        if (Inst != null && Inst != this)
        {
            Destroy(gameObject);
            return;
        }
        Inst = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadInitialData();
    }

    private void LoadInitialData()
    {
        Debug.Log("[OOTechGameManager] 초기 데이터 로드 완료");
    }

    public void AddItem(string itemDataId, int count)
    {
        if (_playerModel == null) return;

        long uniqueId = System.DateTime.UtcNow.Ticks;
        var newItem = new OOTechItemModel();
        newItem.ItemUniqueId = uniqueId;
        newItem.ItemDataId = itemDataId;
        newItem.ItemStackCount = count;

        _playerModel.Inventory.Add(newItem);
        Debug.Log($"[아이템 추가] {itemDataId} x{count} 추가됨");
    }

    public List<OOTechItemModel> GetPlayerItemList()
    {
        return _playerModel?.Inventory ?? new List<OOTechItemModel>();
    }

    // ==================== 저장/로드 (나중에 OOTechNetworkManager를 만들 때 다시 활성화하기) ====================
    /*
    public void SaveData()
    {
        if (OOTechNetworkManager.Inst != null)
            OOTechNetworkManager.Inst.RequstSaveData(_playerModel);
    }

    public void LoadSaveData()
    {
        if (OOTechNetworkManager.Inst != null)
            _playerModel = OOTechNetworkManager.Inst.RequstLoadSaveData();
    }
    */

    public void ChangeStage(int stageId)
    {
        Debug.Log($"[스테이지 변경] {stageId}번 스테이지로 이동");
    }

    public OOTechPlayerModel GetPlayerModel()
    {
        return _playerModel;
    }
}