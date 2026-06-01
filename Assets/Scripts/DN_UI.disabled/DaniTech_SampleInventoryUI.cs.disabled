using System.Collections.Generic;
using UnityEngine;

// 관리주체 역할
public class DaniTech_SampleInventoryUI : DaniTechUIBase
{
    [SerializeField] private GameObject Prefab_Slot;
    [SerializeField] private Transform Transform_UISlotRoot;
    [SerializeField] private DaniTechUIButton Button_UseSelectItem;
    [SerializeField] private DaniTechUIButton Button_CloseSelf;
    [SerializeField] private DaniTechUIButton Button_CloseSelfAllArea;

    private Dictionary<long, DaniTech_SampleInventorySlotUI> _itemSlotList = new Dictionary<long, DaniTech_SampleInventorySlotUI>();
    private long _currentSelectedItemUniqueId;


    private void OnEnable()
    {
        Button_UseSelectItem.BindOnClickButtonEvent(OnClick_UseSelectItem, true);
        Button_CloseSelf.BindOnClickButtonEvent(OnClick_ClosePopup);
        Button_CloseSelfAllArea.BindOnClickButtonEvent(OnClick_ClosePopup);
        SetInventoryItemSlotOnEnable();

        ActiveUseSelectItemButton(false);
    }


    private void OnDisable()
    {
        // 소멸이니까 나중에 신경써주셔도 되요
        // _itemSlotList.Clear();
        // Destroy

        Button_UseSelectItem.UnBindAllOnClickButtonEvent();
    }


    private void SetInventoryItemSlotOnEnable()
    {
        // 슬롯 정리 - 혹시 오픈 시점에 다른 슬롯들이 있다면 제거하자
        if(_itemSlotList.Count > 0)
        {
            foreach(var slot in _itemSlotList){
                DestroyImmediate(slot.Value.gameObject);
            }
            _itemSlotList.Clear();
        }

        //인벤오픈 1-1) 인벤토리가 열릴때 플레이어가 보유한 모든 아이템을 출력하는 로직을 넣어봅시다
        var itemList = DaniTechGameManager.Inst.GetPlayerItemList();
        if(itemList == null || itemList.Count == 0)
        {
            Debug.LogWarning("보유한 아이템이 없습니다!");
            return;
        }

        foreach (var itemModel in itemList) 
        {
            CreateSlot(itemModel.ItemUniqueId, itemModel.ItemDataId, itemModel.ItemStackCount);
        }
    }

    public void OnClick_ClosePopup()
    {
        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.DNInventory);
    }


    public void OnClick_UseSelectItem()
    {
        RequestSelectedUseItem();
    }

    private void RequestSelectedUseItem()
    {
        // 게임 매니저에 아이템 제거를 요청!
        bool isItemRemoved = DaniTechGameManager.Inst.RequestUseItem(_currentSelectedItemUniqueId);
        if(isItemRemoved == true)
        {
            RemoveItemSlot(_currentSelectedItemUniqueId);
            _currentSelectedItemUniqueId = 0;
            ActiveUseSelectItemButton(false);
        }
    }

    private void ActiveUseSelectItemButton(bool isActive)
    {
        Button_UseSelectItem.gameObject.SetActive(isActive);
    }

    private void RemoveItemSlot(long removedItemUniqueId)
    {
        // 저장정보에서 먼저! 아이템이 제거된 후에!!!
        // 그 다음에 슬롯을 제거해야 한다
        if(_itemSlotList.ContainsKey(removedItemUniqueId) == false)
        {
            Debug.LogError("이상합니다! 제거가 된 아이템을 슬롯을 찾을수가 없네요!");
            return;
        }

        var slotComponent = _itemSlotList[removedItemUniqueId];
        _itemSlotList.Remove(removedItemUniqueId);
        Destroy(slotComponent.gameObject);
    }


    // 나중에 규모가 커지면 이렇게 파라미터를 일일이 받는게 아니라, Model 받아올 수도 있다.
    // private void CreateSlot(DaniTechItemModel itemModel)
    private void CreateSlot(long itemUniqueId, string itemDataId, int itemStackCount)
    {
        // 1-1 수동 SetParant가 뒤에 지금은 자동으로 해주고 있다
        var gObj = Instantiate(Prefab_Slot, Transform_UISlotRoot);
        if (gObj == null) return;

        // 1-2 자식 슬롯의 컴포넌트를 가져온다 -> 위에 게임오브젝트는 스크립트가 아직 아니므로
        var slotComponent = gObj.GetComponent<DaniTech_SampleInventorySlotUI>();
        if(slotComponent == null) return;


        // 1-3 여기서 slotComponent가지고 뭔가를 하는 겁니다!
        slotComponent.InitSlot(itemUniqueId, itemDataId, itemStackCount);
        slotComponent.gameObject.name = $"ItemSlot : {slotComponent.SlotItemUniqueId}";

        // 1-4 중복체크 해주면 좋긴 하지만, 일단 쉽게 컴포넌트(컴포넌트로 게임오브젝트는 받을 수 있으므로)를 보관해보자
        _itemSlotList.Add(slotComponent.SlotItemUniqueId, slotComponent);

        slotComponent.BindSlotSelectEvent(OnChildSlotSelected);
    }


    private void OnChildSlotSelected(long selectedItemUniqueId)
    {
        foreach(var slotKv in _itemSlotList)
        {
            var slot = slotKv.Value;
            bool isSlotSelected = (selectedItemUniqueId == slot.SlotItemUniqueId);
            slot.ChangeSelectedState(isSlotSelected);

            if(isSlotSelected == true)
            {
                _currentSelectedItemUniqueId = slot.SlotItemUniqueId;
                ActiveUseSelectItemButton(slot.IsUsableItem);
            }
        }
        Debug.LogWarning($"자식 슬롯 {selectedItemUniqueId} 선택됨!");
    }



}
