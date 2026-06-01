using Cysharp.Threading.Tasks;
using NUnit.Framework.Interfaces;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DaniTech_GameBookSlotUI : MonoBehaviour
{
    [Header("슬롯 기본 정보")]
    [SerializeField] private Image Image_MainIcon;
    [SerializeField] private Text Text_MainName;
    [SerializeField] private GameObject GObj_Selected; // 왜 이미지가 아니라 GameObject -> 활성/비활성화 기능으로만 사용할거라서
    [SerializeField] private DaniTechUIButton Button_SlotClick;

    private event Action<string, EGameBookCategory> _onClickSlot;

    private string _slotDataId; // 슬롯이 자기가 살아있는 동안 어떤 슬롯인지 DataId 보관
    private EGameBookCategory _curSlotCategory;

    public string GetSlotDataId()
    {
        return _slotDataId;
    }

    private void OnEnable()
    {
        // 우리가 그냥 평소에 쓰던 버튼 클릭해줄려고 하는 것 - 큰 의미가 있지 않다
        Button_SlotClick.BindOnClickButtonEvent(OnClick_GameBookSlot);
    }

    public void OnClick_GameBookSlot()
    {
        // 요게 오히려 중요! 자식이 눌러졌는데, 부모한테 알림!
        _onClickSlot?.Invoke(_slotDataId, _curSlotCategory);
    }

    private void OnDisable()
    {
        _onClickSlot = null;
    }

    private void SetSlotUI(string dataName, string iconPath)
    {
        Text_MainName.text = dataName; // 이름 반영

        if (string.IsNullOrEmpty(iconPath) == false) // 있다
        {
            // 이건 잘 만들어 둔거니까 묻지마 사용...< Image에 아이콘, Spitre리소스 불러와서 표기해줄때
            DaniTechGameUtil.LoadAndSetSpriteImage(Image_MainIcon, iconPath).Forget();
        }
    }

    public void InitSlot(string dataId, EGameBookCategory curCategory, Action<string, EGameBookCategory> onClickCallback /*TableType*/) // TODO : 카테고리에 따라 다른 데이터를 받아올 수 있도록 구별할 파라미터를 추가해서 필요는 있다
    {
        if(curCategory == EGameBookCategory.ItemCategory)
        {
            var itemData = DaniTechGameDataManager.Instance.GetDNItemData(dataId);
            if (itemData == null) return;

            SetSlotUI(itemData.Name, itemData.IconPath);
        }
        else if(curCategory == EGameBookCategory.MonsterCategory)
        {
            var monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(dataId);
            if (monsterData == null) return;

            SetSlotUI(monsterData.Name, monsterData.IconPath);
        }
        else if(curCategory == EGameBookCategory.HarvestCategory)
        {
            var fieldObjectData = DaniTechGameDataManager.Instance.GetDNFieldObjectData(dataId);
            if (fieldObjectData == null) return;

            SetSlotUI(fieldObjectData.Name, fieldObjectData.IconPath);
        }


        // 데이터를 잘 받아왔으면, 보관해두자
        _slotDataId = dataId;
        _curSlotCategory = curCategory;
        _onClickSlot += onClickCallback; // 이벤트 등록 완료
    }

    public void SetSelectedUI(bool isSelect)
    {
        GObj_Selected.SetActive(isSelect);
    }
}
