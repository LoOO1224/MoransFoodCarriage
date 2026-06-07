// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechGameManager.cs
// - 역할: 여러 장면에서 함께 쓰는 공통 Manager입니다.
// - 감독 관점: 각 부서에 공통 창구를 열어 주는 제작 본부입니다.
// - 유지보수 포인트: 특정 장면의 세부 연출을 직접 처리하지 말고, 공통 조회/등록/요청 API만 유지합니다.
// =============================================================================
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
    private readonly HashSet<string> _clearedStageIdSet = new HashSet<string>();
    private readonly HashSet<string> _pendingWorldMapNewStageIdSet = new HashSet<string>();

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

    /// <summary>
    /// 스테이지 클리어 상태를 기록합니다.
    /// 영화 비유로는 월드맵 소품팀에게 "이 무대는 이미 끝난 공연"이라는 표식을 넘겨주는 장부입니다.
    /// </summary>
    public void MarkStageCleared(string stageId)
    {
        if (string.IsNullOrWhiteSpace(stageId))
            return;

        _clearedStageIdSet.Add(stageId);
        _pendingWorldMapNewStageIdSet.Add(stageId);
        Debug.Log($"[OOTechGameManager] Stage cleared: {stageId}");
    }

    /// <summary>
    /// 월드맵과 도감이 현재 스테이지 진행도를 조회할 때 사용합니다.
    /// Game View에서는 S1_1/S1_2 같은 초상화 교체 기준이 됩니다.
    /// </summary>
    public bool IsStageCleared(string stageId)
    {
        return !string.IsNullOrWhiteSpace(stageId) && _clearedStageIdSet.Contains(stageId);
    }

    /// <summary>
    /// 클리어 직후 월드맵이 바뀌었다는 NEW 표시가 필요한지 확인하고 소비합니다.
    /// 영화 비유로는 스테이지가 끝난 뒤 다음 로드맵에 도착했을 때만 새 지도를 한 번 펼치는 큐입니다.
    /// </summary>
    public bool ConsumePendingWorldMapNewBadge()
    {
        if (_pendingWorldMapNewStageIdSet.Count <= 0)
            return false;

        _pendingWorldMapNewStageIdSet.Clear();
        return true;
    }
}
