using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public enum EGameBookCategory
{
    None = 0,
    ItemCategory,
    MonsterCategory,
    HarvestCategory
}

public class DaniTech_GameBookUI : DaniTechUIBase
{
    [Header("동적 생성할 프리팹")]
    [SerializeField] private GameObject Prefab_Slot; // 게임 오브젝트이지만, 프리팹이라는 단어를 명시!!

    [Header("디테일 정보 영역")]
    [SerializeField] private Image Image_MainIcon;
    [SerializeField] private Text Text_MainName;
    [SerializeField] private Text Text_Description;

    [Header("상단 카테고리")]
    [SerializeField] private DaniTechUIButton Button_ItemCategory;
    [SerializeField] private DaniTechUIButton Button_MonsterCategory;
    [SerializeField] private DaniTechUIButton Button_HarvestCategory;

    [SerializeField] private DaniTechUIButton Button_CloseUI;

    //[Header("부가 정보")]
    //[SerializeField] private GameObject Layout_SubInfoSkill; // 그 안에 있는 UI요소를 직접 하나하나 껐다 켰다 하는게 아니라!, 그 레이아웃의 대표 오브젝트만 껏다 켰다하는게 압도적으로 편하다

    [Header("슬롯 리스트 영역")]
    [SerializeField] private Transform Transform_SlotRoot; // 스크롤뷰에 슬롯이 생성될 수 있게 위치를 미리 지정해줍시다

    private Dictionary<string, DaniTech_GameBookSlotUI> _slotList = new Dictionary<string, DaniTech_GameBookSlotUI>();

    private void OnEnable()
    {
        // 이 UI가 열릴때 스스로, 기본적으로 아이템 도감 안에 있는 모든~~~ 데이터를 불러온다
        OnClick_ItemCategory();

        Button_CloseUI.BindOnClickButtonEvent(OnClick_CloseGameBookUI);
        Button_ItemCategory.BindOnClickButtonEvent(OnClick_ItemCategory);
        Button_MonsterCategory.BindOnClickButtonEvent(OnClick_MonsterCategory);
        Button_HarvestCategory.BindOnClickButtonEvent(OnClick_HarvestCategory);
    }

    private void OnDisable()
    {
        DestroyAndClearSlotList();
    }

    private void DestroyAndClearSlotList()
    {
        if (_slotList.Count > 0)
        {
            foreach (var slotKv in _slotList)
            {
                var slot = slotKv.Value; // 컴포넌트인데, 얘로 gameObject를 받아올 수 있다!
                // DestroyImmediate(slot);
                DestroyImmediate(slot.gameObject);
            }

            _slotList.Clear();
        }
    }

    public void OnClick_CloseGameBookUI()
    {
        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.DNGameBookUI);
    }

    public void OnClick_ItemCategory()
    {
        SetGameBookLayoutByCategory(EGameBookCategory.ItemCategory);
    }

    public void OnClick_MonsterCategory()
    {
        SetGameBookLayoutByCategory(EGameBookCategory.MonsterCategory);
    }

    public void OnClick_HarvestCategory()
    {
        SetGameBookLayoutByCategory(EGameBookCategory.HarvestCategory);
    }

    private void SetGameBookLayoutByCategory(EGameBookCategory category)
    {
        DestroyAndClearSlotList();

        switch (category)
        {
            case EGameBookCategory.ItemCategory:
                ReadItemListAndCreateSlot();
                break;
            case EGameBookCategory.MonsterCategory:
                ReadMonsterListAndCreateSlot();
                break;
            case EGameBookCategory.HarvestCategory:
                ReadHarvestListAndCreateSlot();
                break;
            default:
                break;
        }
    }

    private void ReadItemListAndCreateSlot()
    {
        // 데이터를 읽어와서 순회(foreach)를 돌면서, 아이템들을 도감 리스트에 표기
        var dataList = DaniTechGameDataManager.Instance.ItemDataList;
        foreach(var dataKv in dataList)
        {
            var data = dataKv.Value;
            if (data == null) continue; // 데이터가 Null일수 있으니 체크

            CreateGameBookSlot(data.Id, EGameBookCategory.ItemCategory);
        }

        SelectFirstSlot();
    }

    private void ReadMonsterListAndCreateSlot()
    {
        var dataList = DaniTechGameDataManager.Instance.MonsterDataList;
        foreach (var dataKv in dataList)
        {
            var data = dataKv.Value;
            if (data == null) continue; 

            CreateGameBookSlot(data.Id, EGameBookCategory.MonsterCategory);
        }

        SelectFirstSlot();
    }

    private void ReadHarvestListAndCreateSlot()
    {
        var dataList = DaniTechGameDataManager.Instance.FieldObjectDataList;
        foreach (var dataKv in dataList)
        {
            var data = dataKv.Value;
            if (data == null) continue;

            if (data.FieldObjectType != "Harvest") continue;

            CreateGameBookSlot(data.Id, EGameBookCategory.HarvestCategory);
        }

        SelectFirstSlot();
    }

    private void SelectFirstSlot()
    {
        if (_slotList.Count > 0)
        {
            foreach (var slotKv in _slotList)
            {
                var slot = slotKv.Value;
                slot.OnClick_GameBookSlot();
            }
        }
    }

    // 슬롯 1개만 제대로 생성해주는 로직 역할을 맡는 메서드
    private void CreateGameBookSlot(string dataId, EGameBookCategory curCategory)
    {
        var gObj = Instantiate(Prefab_Slot, Transform_SlotRoot);
        if (gObj == null) return;

        // 게임 오브젝트는 동적 생성이 됬다
        var slotComponent = gObj.GetComponent<DaniTech_GameBookSlotUI>();
        if (slotComponent == null) return;

        // 동적 생성된 자식 슬롯(게임오브젝트) 안에 있는 컴포넌트도 잘 가져왔다
        slotComponent.InitSlot(dataId, curCategory, OnClickChildSlotSelected);
        _slotList.Add(dataId, slotComponent);
    }

    private void SetDetailInfoUI(string dataName, string dataDescription = "", string iconPath = "")
    {
        // Text_SellingPrice.text = currentSelectedData.SellingPrice;
        // Text_SellingPrice.gameObject.SetActive(currentSelectedData.SellingPrice > 0);
        Text_MainName.text = dataName;
        Text_Description.text = dataDescription;

        if (string.IsNullOrEmpty(iconPath) == false)
        {
            DaniTechGameUtil.LoadAndSetSpriteImage(Image_MainIcon, iconPath).Forget();
        }
        Image_MainIcon.gameObject.SetActive(string.IsNullOrEmpty(iconPath) == false);
    }

    private void OnClickChildSlotSelected(string slotDataId, EGameBookCategory selectedSlotCategory)
    {
        if(selectedSlotCategory == EGameBookCategory.ItemCategory)
        {
            var currentSelectedData = DaniTechGameDataManager.Instance.GetDNItemData(slotDataId);
            if (currentSelectedData == null) return;
            SetDetailInfoUI(currentSelectedData.Name, currentSelectedData.Description, currentSelectedData.IconPath);
        }
        else if(selectedSlotCategory == EGameBookCategory.MonsterCategory)
        {
            var currentSelectedData = DaniTechGameDataManager.Instance.GetDNMonsterData(slotDataId);
            if (currentSelectedData == null) return;
            SetDetailInfoUI(currentSelectedData.Name, currentSelectedData.Description, currentSelectedData.IconPath);
        }
        else if(selectedSlotCategory == EGameBookCategory.HarvestCategory)
        {
            var currentSelectedData = DaniTechGameDataManager.Instance.GetDNFieldObjectData(slotDataId);
            if (currentSelectedData == null) return;
            SetDetailInfoUI(currentSelectedData.Name, currentSelectedData.Description, currentSelectedData.IconPath);

        }

        foreach (var slotKv in _slotList)
        {
            var slot = slotKv.Value;
            var dataId = slot.GetSlotDataId();
            slot.SetSelectedUI(slotDataId == dataId);
        }
    }
}
