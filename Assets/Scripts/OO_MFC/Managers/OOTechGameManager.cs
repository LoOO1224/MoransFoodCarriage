using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 런타임 진행 데이터(Model)를 관리하는 게임 매니저입니다.
/// Game View에서는 인벤토리에 재료가 들어오고 빠지는 소품 창고 역할을 합니다.
/// </summary>
public class OOTechGameManager : MonoBehaviour
{
    public static OOTechGameManager Inst { get; private set; }

    [SerializeField] private OOTechPlayerModel _playerModel = new OOTechPlayerModel();

    /// <summary>
    /// 씬 전환 후에도 유지되는 단일 게임 매니저로 등록합니다.
    /// </summary>
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

    /// <summary>
    /// 시작 시 저장 데이터나 기본 진행 데이터를 준비합니다.
    /// </summary>
    private void Start()
    {
        LoadInitialData();
    }

    /// <summary>
    /// 초기 런타임 데이터를 준비합니다. 현재는 1차 구현이라 로그만 남깁니다.
    /// </summary>
    private void LoadInitialData()
    {
        Debug.Log("[OOTechGameManager] 초기 데이터 로드 완료");
    }

    /// <summary>
    /// 인벤토리에 아이템을 추가합니다. 같은 데이터 ID가 있으면 스택 수량만 늘립니다.
    /// </summary>
    public void AddItem(string itemDataId, int count)
    {
        if (_playerModel == null || string.IsNullOrEmpty(itemDataId))
            return;

        int addCount = Mathf.Max(1, count);

        foreach (OOTechItemModel itemModel in _playerModel.Inventory)
        {
            if (itemModel == null || itemModel.ItemDataId != itemDataId)
                continue;

            itemModel.ItemStackCount += addCount;
            Debug.Log($"[아이템 추가] {itemDataId} x{addCount} 누적됨");
            return;
        }

        OOTechItemModel newItem = new OOTechItemModel
        {
            ItemUniqueId = System.DateTime.UtcNow.Ticks,
            ItemDataId = itemDataId,
            ItemStackCount = addCount
        };

        _playerModel.Inventory.Add(newItem);
        Debug.Log($"[아이템 추가] {itemDataId} x{addCount} 추가됨");
    }

    /// <summary>
    /// 인벤토리에서 아이템 수량을 차감합니다. 요리 재료를 가마솥에 넣을 때 사용합니다.
    /// </summary>
    public bool RemoveItem(string itemDataId, int count)
    {
        if (_playerModel == null || string.IsNullOrEmpty(itemDataId))
            return false;

        int removeCount = Mathf.Max(1, count);

        for (int index = 0; index < _playerModel.Inventory.Count; index++)
        {
            OOTechItemModel itemModel = _playerModel.Inventory[index];

            if (itemModel == null || itemModel.ItemDataId != itemDataId)
                continue;

            if (itemModel.ItemStackCount < removeCount)
                return false;

            itemModel.ItemStackCount -= removeCount;

            if (itemModel.ItemStackCount <= 0)
                _playerModel.Inventory.RemoveAt(index);

            Debug.Log($"[아이템 제거] {itemDataId} x{removeCount} 제거됨");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 인벤토리에 해당 아이템이 몇 개 있는지 반환합니다.
    /// </summary>
    public int GetItemCount(string itemDataId)
    {
        if (_playerModel == null || string.IsNullOrEmpty(itemDataId))
            return 0;

        foreach (OOTechItemModel itemModel in _playerModel.Inventory)
        {
            if (itemModel != null && itemModel.ItemDataId == itemDataId)
                return itemModel.ItemStackCount;
        }

        return 0;
    }

    /// <summary>
    /// HUD와 요리 UI가 표시할 전체 인벤토리 목록을 반환합니다.
    /// </summary>
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

    /// <summary>
    /// 스테이지 번호 변경을 기록합니다. 실제 저장/로드 연동 전까지는 로그 큐로 사용합니다.
    /// </summary>
    public void ChangeStage(int stageId)
    {
        Debug.Log($"[스테이지 변경] {stageId}번 스테이지로 이동");
    }

    /// <summary>
    /// 플레이어 모델 전체를 반환합니다. 저장 시스템과 연결할 때 사용합니다.
    /// </summary>
    public OOTechPlayerModel GetPlayerModel()
    {
        return _playerModel;
    }
}
