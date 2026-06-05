// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCookingIngredientDragItem.cs
// - 역할: 요리 시스템의 입력, 조리도구, 레시피 판정을 담당하는 스크립트입니다.
// - 감독 관점: 부엌 장면에서 재료와 조리도구 배우가 어떤 순서로 만나는지 관리합니다.
// - 유지보수 포인트: 재료 규칙은 데이터와 DropTarget 역할표로 빼고, UI 배치는 CookingUIGroup에서 직접 수정합니다.
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 슬롯을 드래그 가능한 재료 소품으로 만듭니다.
/// 플레이어가 슬롯을 끌어 가마솥에 놓으면 CookingGroupController가 요리를 판정합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechCookingIngredientDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    private OOTechCookingGroupController Controller_Cooking;
    private string _itemDataId;
    private string _itemName;
    private int _itemCount;
    private int _dragQuantity = 1;
    private TextMeshProUGUI Text_Label;
    private RectTransform Rect_IconDragArea;
    private RectTransform Rect_DragGhost;
    private bool _isDraggingFromIcon;

    /// <summary>
    /// 슬롯이 어떤 재료를 대표하는지 요리 컨트롤러와 함께 설정합니다.
    /// </summary>
    public void Setup(OOTechCookingGroupController controller, string itemDataId, string itemName, int itemCount)
    {
        Controller_Cooking = controller;
        _itemDataId = itemDataId;
        _itemName = itemName;
        _itemCount = Mathf.Max(0, itemCount);
        _dragQuantity = Mathf.Clamp(_dragQuantity, 1, Mathf.Max(1, _itemCount));

        if (Text_Label == null)
            Text_Label = GetComponentInChildren<TextMeshProUGUI>(true);

        ApplySlotLabel();
        ResolveIconDragArea();
    }

    /// <summary>
    /// 드래그가 시작되면 손에 든 재료처럼 보이는 임시 잔상 UI를 만듭니다.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Controller_Cooking == null)
            return;

        _isDraggingFromIcon = IsPointerInsideIcon(eventData);

        if (!_isDraggingFromIcon)
            return;

        Rect_DragGhost = Controller_Cooking.CreateDragGhost(_itemDataId, eventData.position, _dragQuantity);
    }

    /// <summary>
    /// 마우스 위치를 따라 재료 잔상을 이동합니다.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDraggingFromIcon || Rect_DragGhost == null)
            return;

        Rect_DragGhost.position = eventData.position;
    }

    /// <summary>
    /// 드래그를 놓은 위치가 가마솥이면 재료 투입을 요청하고 잔상을 정리합니다.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isDraggingFromIcon && Controller_Cooking != null)
            Controller_Cooking.RequestDropIngredientAtPosition(_itemDataId, eventData.position, _dragQuantity);

        if (Rect_DragGhost != null)
            Destroy(Rect_DragGhost.gameObject);

        Rect_DragGhost = null;
        _isDraggingFromIcon = false;
    }

    /// <summary>
    /// Ctrl을 누르고 마우스 휠을 돌리면 이번에 집을 재료 수량을 조절합니다.
    /// 영화 비유로는 소품 한 개를 들지, 같은 소품 열 개를 한 번에 들지 배우가 손에 쥐는 개수를 정하는 큐입니다.
    /// </summary>
    public void OnScroll(PointerEventData eventData)
    {
        if (eventData == null || !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
            return;

        if (_itemCount <= 0)
            return;

        int delta = eventData.scrollDelta.y > 0f ? 1 : -1;
        _dragQuantity = Mathf.Clamp(_dragQuantity + delta, 1, _itemCount);
        ApplySlotLabel();
        eventData.Use();

        if (Controller_Cooking != null)
            Controller_Cooking.RequestShowCookingStatus($"{_itemName} 집기 수량: {_dragQuantity}개");
    }

    /// <summary>
    /// 슬롯 안의 Image_ItemIcon 영역을 드래그 손잡이로 찾습니다.
    /// 영화로 치면 재료 이름표가 아니라 실제 음식 소품을 집을 위치를 찾는 단계입니다.
    /// </summary>
    private void ResolveIconDragArea()
    {
        if (Rect_IconDragArea != null)
            return;

        Transform iconTransform = FindChildByName(transform, "Image_ItemIcon");
        Rect_IconDragArea = iconTransform as RectTransform;
    }

    private bool IsPointerInsideIcon(PointerEventData eventData)
    {
        ResolveIconDragArea();

        if (Rect_IconDragArea == null)
            return true;

        Camera eventCamera = eventData != null ? eventData.pressEventCamera : null;
        return eventData != null && RectTransformUtility.RectangleContainsScreenPoint(Rect_IconDragArea, eventData.position, eventCamera);
    }

    private void ApplySlotLabel()
    {
        if (Text_Label == null)
            return;

        string quantityText = _dragQuantity > 1 ? $" / 집기 x{_dragQuantity}" : string.Empty;
        Text_Label.text = $"{_itemName} x{_itemCount}{quantityText}";
    }

    private Transform FindChildByName(Transform rootTransform, string childName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == childName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = FindChildByName(rootTransform.GetChild(index), childName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
    }
}
