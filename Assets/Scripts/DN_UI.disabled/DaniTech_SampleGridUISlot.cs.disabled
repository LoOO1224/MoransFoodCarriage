using System;
using UnityEngine;
using UnityEngine.UI;

public class DaniTech_SampleGridUISlot : MonoBehaviour
{
    [SerializeField] private DaniTechUIButton Button_SelectSlot;
    [SerializeField] private Image Image_Icon;
    [SerializeField] private Image Image_Selected;

    // 2-1) 기존의 버튼 onclick과 다르게, 이번엔 슬롯이 눌러지면, 우리가 직접 선언한 델리게이트를 통해 부모에게 알려줘야한다.
    private event Action<int> OnSlotSelected;

    // 1-2 클릭했을 때, 나를 관리하는 제 3자가 내가 누구인지 알 수 있도록 보내주는 용도로 사용한다
    public int SlotInstanceId { get; private set; }

    private void OnEnable()
    {
        // 3-2) 다른예제들과 마찬가지로 에디터에서 버튼 이벤트를 수동으로 등록해야했던 것을 이렇게 등록 가능
        Button_SelectSlot.BindOnClickButtonEvent(OnClick_SelectSlot);

        if(Image_Selected != null)
        {
            Image_Selected.gameObject.SetActive(false);
        }
    }

    private void ExtraSetRandomIcon()
    {
        // + 이 로직은 추후에 데이터드리븐을 통해서 Sprite 이름을 받아오면 됩니다!
        int randomIdx = UnityEngine.Random.Range(0, 4);

        string spriteName = string.Empty;
        switch (randomIdx)
        {
            case 0:
                spriteName = "2D/SampleImg_pug_1";
                break;
            case 1:
                spriteName = "2D/Background_Img";
                break;
            case 2:
                spriteName = "2D/SampleImg_pug_3";
                break;
            case 3:
                spriteName = "2D/SampleImg_NineSlice_1";
                break;
        }

        var sprite = DaniTechGameUtil.LoadSpriteCanBeNull(spriteName);
        if(sprite == null) return;

        Image_Icon.sprite = sprite;
    }


    // 1-1) 슬롯이 생성될 때 호출되어 ID가 부여되고, 이를 관리하는 대상이 슬롯이 눌러졌는지 응답 받을 수 있게 이벤트를 등록
    public void InitSlotOnCreate(int slotInstanceId, Action<int> onClickCallback)
    {
        SlotInstanceId = slotInstanceId;

        // 2-2) 이 슬롯에 다른 여러 UI에서 이벤트 구독을 해야한다면 +=으로 바꿔도 된다
        OnSlotSelected = onClickCallback;

        ExtraSetRandomIcon();
    }

    // 3-1) 버튼 클릭할때 관리 객체에 내가 눌렸다는 것을 알려주자
    public void OnClick_SelectSlot()
    {
        OnSlotSelected?.Invoke(SlotInstanceId);
    }


    // 4-1) 내가 직접 선택 상태를 바꾸는게 아니라! 이렇게 수동으로 내 관리 주체에 의해서 바뀌어야 한다!
        // Shift + F12를 눌러 꼭 이 메서드가 어디서 호출되는지 확인하자!
    public void ChangeSelectedState(bool isSelected)
    {
        Image_Selected.gameObject.SetActive(isSelected);
    }
}
