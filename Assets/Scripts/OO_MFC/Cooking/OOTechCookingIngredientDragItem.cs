using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 슬롯을 드래그 가능한 재료 소품으로 만듭니다.
/// 플레이어가 슬롯을 끌어 가마솥에 놓으면 CookingGroupController가 요리를 판정합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechCookingIngredientDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Preview")]
    [SerializeField] private float _hoverPreviewScale = 1.5f;

    private OOTechCookingGroupController Controller_Cooking;
    private string _itemDataId;
    private TextMeshProUGUI Text_Label;
    private RectTransform Rect_Item;
    private RectTransform Rect_DragGhost;
    private Vector3 _originScale = Vector3.one;
    private bool _isOriginScaleCached;

    /// <summary>
    /// 슬롯이 어떤 재료를 대표하는지 요리 컨트롤러와 함께 설정합니다.
    /// </summary>
    public void Setup(OOTechCookingGroupController controller, string itemDataId, string itemName, int itemCount)
    {
        Controller_Cooking = controller;
        _itemDataId = itemDataId;
        CacheOriginScale();

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

        SetPreviewScale(true);
        Rect_DragGhost = Controller_Cooking.CreateDragGhost(_itemDataId, eventData.position);

        if (Rect_DragGhost != null)
            Rect_DragGhost.localScale = Vector3.one * GetPreviewScale();
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
        SetPreviewScale(false);
    }

    /// <summary>
    /// 마우스를 올리면 재료 아이콘이 크게 보여서 어떤 아이템인지 바로 알아볼 수 있게 합니다.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetPreviewScale(true);
    }

    /// <summary>
    /// 마우스가 슬롯을 떠나면 원래 인벤토리 크기로 되돌립니다.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (Rect_DragGhost != null)
            return;

        SetPreviewScale(false);
    }

    private void CacheOriginScale()
    {
        if (_isOriginScaleCached)
            return;

        Rect_Item = transform as RectTransform;
        _originScale = transform.localScale;
        _isOriginScaleCached = true;
    }

    private void SetPreviewScale(bool isPreview)
    {
        CacheOriginScale();

        transform.localScale = isPreview ? _originScale * GetPreviewScale() : _originScale;

        if (Rect_Item != null)
            Rect_Item.SetAsLastSibling();
    }

    private float GetPreviewScale()
    {
        return Mathf.Clamp(_hoverPreviewScale, 1f, 1.5f);
    }
}
