// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechCookingIngredientDragItem.cs
// - ??븷: ?붾━ ?쒖뒪?쒖쓽 ?낅젰, 議곕━?꾧뎄, ?덉떆???먯젙???대떦?섎뒗 ?ㅽ겕由쏀듃?낅땲??
// - 媛먮룆 愿?? 遺???λ㈃?먯꽌 ?щ즺? 議곕━?꾧뎄 諛곗슦媛 ?대뼡 ?쒖꽌濡?留뚮굹?붿? 愿由ы빀?덈떎.
// - ?좎?蹂댁닔 ?ъ씤?? ?щ즺 洹쒖튃? ?곗씠?곗? DropTarget ??븷?쒕줈 鍮쇨퀬, UI 諛곗튂??CookingUIGroup?먯꽌 吏곸젒 ?섏젙?⑸땲??
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ?몃깽?좊━ ?щ’???쒕옒洹?媛?ν븳 ?щ즺 ?뚰뭹?쇰줈 留뚮벊?덈떎.
/// ?뚮젅?댁뼱媛 ?щ’???뚯뼱 媛留덉넡???볦쑝硫?CookingGroupController媛 ?붾━瑜??먯젙?⑸땲??
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
    /// ?щ’???대뼡 ?щ즺瑜???쒗븯?붿? ?붾━ 而⑦듃濡ㅻ윭? ?④퍡 ?ㅼ젙?⑸땲??
    /// </summary>
    public void Setup(OOTechCookingGroupController controller, string itemDataId, string itemName, int itemCount)
    {
        Controller_Cooking = controller;
        _itemDataId = itemDataId;
        _itemName = itemName;
        _itemCount = Mathf.Max(0, itemCount);
        _dragQuantity = 1;

        if (Text_Label == null)
            Text_Label = GetComponentInChildren<TextMeshProUGUI>(true);

        ApplySlotLabel();
        ResolveIconDragArea();
    }

    /// <summary>
    /// ?쒕옒洹멸? ?쒖옉?섎㈃ ?먯뿉 ???щ즺泥섎읆 蹂댁씠???꾩떆 ?붿긽 UI瑜?留뚮벊?덈떎.
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
    /// 留덉슦???꾩튂瑜??곕씪 ?щ즺 ?붿긽???대룞?⑸땲??
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDraggingFromIcon || Rect_DragGhost == null)
            return;

        Rect_DragGhost.position = eventData.position;
    }

    /// <summary>
    /// ?쒕옒洹몃? ?볦? ?꾩튂媛 媛留덉넡?대㈃ ?щ즺 ?ъ엯???붿껌?섍퀬 ?붿긽???뺣━?⑸땲??
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
    /// Ctrl???꾨Ⅴ怨?留덉슦???좎쓣 ?뚮━硫??대쾲??吏묒쓣 ?щ즺 ?섎웾??議곗젅?⑸땲??
    /// ?곹솕 鍮꾩쑀濡쒕뒗 ?뚰뭹 ??媛쒕? ?ㅼ?, 媛숈? ?뚰뭹 ??媛쒕? ??踰덉뿉 ?ㅼ? 諛곗슦媛 ?먯뿉 伊먮뒗 媛쒖닔瑜??뺥븯???먯엯?덈떎.
    /// </summary>
    public void OnScroll(PointerEventData eventData)
    {
        if (Controller_Cooking != null && Controller_Cooking.IsStage3EncounterCooking())
        {
            _dragQuantity = 1;
            ApplySlotLabel();
            eventData?.Use();
            return;
        }

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
    /// ?щ’ ?덉쓽 Image_ItemIcon ?곸뿭???쒕옒洹??먯옟?대줈 李얠뒿?덈떎.
    /// ?곹솕濡?移섎㈃ ?щ즺 ?대쫫?쒓? ?꾨땲???ㅼ젣 ?뚯떇 ?뚰뭹??吏묒쓣 ?꾩튂瑜?李얜뒗 ?④퀎?낅땲??
    /// </summary>
    private void ResolveIconDragArea()
    {
        if (Rect_IconDragArea != null)
            return;

        Transform iconTransform = RequestChildObjectByName(transform, "Image_ItemIcon");
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

        string quantityText = _dragQuantity > 1 ? $" / 吏묎린 x{_dragQuantity}" : string.Empty;
        Text_Label.text = $"{_itemName} x{_itemCount}{quantityText}";
    }

    private Transform RequestChildObjectByName(Transform rootTransform, string childName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == childName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = RequestChildObjectByName(rootTransform.GetChild(index), childName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
    }
}

