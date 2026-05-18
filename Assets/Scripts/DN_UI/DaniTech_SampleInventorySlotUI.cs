using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

// 1: n으로 n의 관계에 있는 슬롯 (자식)
public class DaniTech_SampleInventorySlotUI : MonoBehaviour
{
    [SerializeField] private Text Text_StackCount;
    [SerializeField] private DaniTechUIButton Button_Slot;
    [SerializeField] private Image Image_Icon;
    [SerializeField] private Image Image_Frame;
    [SerializeField] private Image Image_Selected;

    private event Action<int> OnSelectEvent;

    public int SlotInstanceId { get; private set; }

    private void OnEnable()
    {
        Image_Selected.gameObject.SetActive(false);
        Button_Slot.BindOnClickButtonEvent(OnClick_SelectItem);
        RandomTestItemSetIcon();
    }

    private void RandomTestItemSetIcon()
    {
        // 데이터 연동 테스트 전용 - 추후에 제거될 메서드
        int randomIdx = UnityEngine.Random.Range(0, 3);

        string temporalItemDataId = string.Empty;
        switch (randomIdx)
        {
            case 0:
                temporalItemDataId = "Item_DummyBox_1";
                break;
            case 1:
                temporalItemDataId = "Item_Gold_1";
                break;
            case 2:
                temporalItemDataId = "Item_Equip_Hat_1";
                break;
        }

        SetIcon(temporalItemDataId);
    }

    private void SetIcon(string itemDataId)
    {
        var itemData = GameDataManager.Instance.GetDNItemData(itemDataId);
        if (itemData == null)
        {
            Debug.LogWarning($"Item 데이터를 불러올 수 없습니다! 경로:{itemDataId}");
            return;
        }

        string iconPath = itemData.IconPath;
        if (string.IsNullOrEmpty(iconPath) == true)
        {
            Debug.LogWarning($"Item 데이터에 아이콘 경로가 존재하지 않습니다.");
            return;
        }

        // + Addressable을 적용하면서 비동기로 바뀌었다
        //DaniTechResourceManager.Inst.LoadSprite(iconPath, (sprite) => {
        //    Image_Icon.sprite = sprite;
        //});

        GameUtil.LoadAndSetSpriteImage(Image_Icon, iconPath).Forget();

        //var sprite = GameUtil.LoadSpriteCanBeNull(iconPath);
        //if(sprite == null)
        //{
        //    Debug.LogWarning($"Sprite를 불러올 수 없습니다! 경로:{iconPath}");
        //    return;
        //}
        //Image_Icon.sprite = sprite;
    }

    private void OnDisable()
    {
        OnSelectEvent = null;
    }

    public void InitSlot(int slotInstanceId)
    {
        SlotInstanceId = slotInstanceId;
        Text_StackCount.text = slotInstanceId.ToString();
    }

    public void OnClick_SelectItem()
    {
        // 부모한테 알려주자
        OnSelectEvent?.Invoke(SlotInstanceId);


        Debug.Log($"{SlotInstanceId}눌러졌다");
        // 나중에 툴팁, 팝업 다 여기서 띄워주면 된다
    }

    public void BindSlotSelectEvent(Action<int> onSelectEvent)
    {
        // 얘는 부모 하나만 콜백이벤트 등록하면 된다.
        OnSelectEvent = onSelectEvent;
    }

    // 수동태로 자신의 상태UI를 변경합니다
    public void ChangeSelectedState(bool isSelected)
    {
        Image_Selected.gameObject.SetActive(isSelected);
    }

}
