using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 슬롯을 드래그 가능한 재료 소품으로 만듭니다.
/// 플레이어가 슬롯을 끌어 가마솥에 놓으면 CookingGroupController가 요리를 판정합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechCookingIngredientDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private OOTechCookingGroupController Controller_Cooking;
    private string _itemDataId;
    private TextMeshProUGUI Text_Label;
    private RectTransform Rect_DragGhost;

    /// <summary>
    /// 슬롯이 어떤 재료를 대표하는지 요리 컨트롤러와 함께 설정합니다.
    /// </summary>
    public void Setup(OOTechCookingGroupController controller, string itemDataId, string itemName, int itemCount)
    {
        Controller_Cooking = controller;
        _itemDataId = itemDataId;

        if (Text_Label == null)
            Text_Label = GetComponentInChildren<TextMeshProUGUI>(true);

        if (Text_Label != null)
            Text_Label.text = $"{itemName} x{itemCount}";
    }

    /// <summary>
    /// 드래그가 시작되면 손에 든 재료처럼 보이는 임시 잔상 UI를 만듭니다.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Controller_Cooking == null)
            return;

        Rect_DragGhost = Controller_Cooking.CreateDragGhost(_itemDataId, eventData.position);
    }

    /// <summary>
    /// 마우스 위치를 따라 재료 잔상을 이동합니다.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (Rect_DragGhost != null)
            Rect_DragGhost.position = eventData.position;
    }

    /// <summary>
    /// 드래그를 놓은 위치가 가마솥이면 재료 투입을 요청하고 잔상을 정리합니다.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (Controller_Cooking != null)
            Controller_Cooking.RequestDropIngredientAtPosition(_itemDataId, eventData.position);

        if (Rect_DragGhost != null)
            Destroy(Rect_DragGhost.gameObject);

        Rect_DragGhost = null;
    }
}
