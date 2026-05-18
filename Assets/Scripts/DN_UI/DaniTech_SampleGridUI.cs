using System.Collections.Generic;
using UnityEngine;

public class DaniTech_SampleGridUI : MonoBehaviour
{
    [SerializeField] private GameObject Prefab_GridSlot; // 동적생성할 슬롯의 대상이 되는 프리팹을 등록한다
    [SerializeField] private Transform Transform_SlotRoot; // 동적생성되는 슬롯이 들어가야하는 스크롤뷰의 Content 위치다!
    [SerializeField] private DaniTechUIButton Button_TestAddNewCard;

    // 이 UI에서 추후에 해당 슬롯을 확인할 수 있게 자료구조에 보관하자!
    private Dictionary<int, DaniTech_SampleGridUISlot> _gridSlotList = new Dictionary<int, DaniTech_SampleGridUISlot>();
    private int _generatedKey = 0;


    private void OnEnable()
    {
        ClearSlot();
        Button_TestAddNewCard.BindOnClickButtonEvent(OnClick_AddNewCard);
    }

    private void OnDisable()
    {
        ClearSlot();
    }

    public void ClearSlot()
    {
        // + 개선작업 : UI가 오픈될때 혹시 잔 슬롯이 남아있다면 클리어해준다
        foreach(var slotKv in _gridSlotList)
        {
            Destroy(slotKv.Value);
        }
        _gridSlotList.Clear();

        // 혹시 등록 안된 슬롯이 추가로 남아있다면 제거하자
        for (int i = Transform_SlotRoot.childCount - 1; i >= 0; i--)
        {
            GameObject child = Transform_SlotRoot.GetChild(i).gameObject;
            DestroyImmediate(child);
        }
    }

    public void OnClick_AddNewCard()
    {
        // + 클릭이 눌러졌을때 슬롯을 생성해보자!
        CreateSlot();

    }

    private void CreateSlot()
    {
        var slotObj = Instantiate(Prefab_GridSlot, Transform_SlotRoot);
        if (slotObj == null) return;

        // + 이 컴포넌트(DaniTech_SampleGridUISlot)는 여러분들이 직접 만든 슬롯 UI로 바꾸면 됩니다
        var slotComponent = slotObj.GetComponent<DaniTech_SampleGridUISlot>();
        if(slotComponent == null) return;

        _generatedKey++;
        _gridSlotList.Add(_generatedKey, slotComponent);
        // + 자식이 눌려졌다는 것을 알 수 있게(콜백) 내 메서드(함수)를 넘겨주자
        slotComponent.InitSlotOnCreate(_generatedKey, OnChildSlotSelected);

    }

    private void OnChildSlotSelected(int slotInstanceId)
    {
        foreach(var slotKv in _gridSlotList)
        {
            var slot = slotKv.Value;
            bool isSelected = (slot.SlotInstanceId == slotInstanceId);
            slot.ChangeSelectedState(isSelected);
        }

        Debug.Log($"자식 {slotInstanceId}가 눌러짐");
    }
}
