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

    private event Action<long> OnSelectEvent;

    public long SlotItemUniqueId { get; private set; }
    public bool IsUsableItem { get; private set; }

    private void OnEnable()
    {
        Image_Selected.gameObject.SetActive(false);
        Button_Slot.BindOnClickButtonEvent(OnClick_SelectItem);
    }

    public void SetIcon(string itemDataId, int itemCount)
    {
        var itemData = DaniTechGameDataManager.Instance.GetDNItemData(itemDataId);
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

        IsUsableItem = (string.IsNullOrEmpty(itemData.UseItemType) == false);

        // + Addressable을 적용하면서 비동기로 바뀌었다
        //DaniTechResourceManager.Inst.LoadSprite(iconPath, (sprite) => {
        //    Image_Icon.sprite = sprite;
        //});

        DaniTechGameUtil.LoadAndSetSpriteImage(Image_Icon, iconPath).Forget();

        //var sprite = GameUtil.LoadSpriteCanBeNull(iconPath);
        //if(sprite == null)
        //{
        //    Debug.LogWarning($"Sprite를 불러올 수 없습니다! 경로:{iconPath}");
        //    return;
        //}
        //Image_Icon.sprite = sprite;

        Text_StackCount.text = $"{itemCount}";
    }

    private void OnDisable()
    {
        OnSelectEvent = null;
    }

    public void InitSlot(long slotUniqueId, string itemDataId, int itemStackCount)
    {
        SlotItemUniqueId = slotUniqueId;
        SetIcon(itemDataId, itemStackCount);
        // Text_StackCount.text = slotInstanceId.ToString();
    }

    public void OnClick_SelectItem()
    {
        // 부모한테 알려주자
        OnSelectEvent?.Invoke(SlotItemUniqueId);


        Debug.Log($"{SlotItemUniqueId}눌러졌다");
        // 나중에 툴팁, 팝업 다 여기서 띄워주면 된다
    }

    public void BindSlotSelectEvent(Action<long> onSelectEvent)
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
