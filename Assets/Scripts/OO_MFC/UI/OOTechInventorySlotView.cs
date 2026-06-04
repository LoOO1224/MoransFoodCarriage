// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechInventorySlotView.cs
// - 역할: UI 하이어라키의 버튼, 이미지, 텍스트 참조를 모아 둔 View 컴포넌트입니다.
// - 감독 관점: 무대 위 소품 위치표입니다. 판단하지 않고 소품을 보여 주는 일만 맡습니다.
// - 유지보수 포인트: 버튼 동작 판단, 데이터 로딩, 그룹 전환 로직은 Controller나 Manager에 둡니다.
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 슬롯 한 칸의 표시 텍스트를 담당합니다.
/// Game View에서는 "쌀 x2" 같은 슬롯 문구를 보여주는 작은 소품입니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechInventorySlotView : MonoBehaviour
{
    [Header("Slot Layout")]
    [SerializeField] private Vector2 _iconSize = new Vector2(72f, 72f);
    [SerializeField] private float _iconRightPadding = 16f;
    [SerializeField] private float _textLeftPadding = 18f;
    [SerializeField] private float _textRightGapFromIcon = 12f;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI Text_Label;

    [Header("Icon")]
    [SerializeField] private Image Image_Icon;

    /// <summary>
    /// 슬롯 안의 TMP 텍스트를 찾아 저장합니다.
    /// </summary>
    public void ResolveReferences()
    {
        if (Text_Label == null)
            Text_Label = GetComponentInChildren<TextMeshProUGUI>(true);

        if (Image_Icon == null)
        {
            Transform iconTransform = FindChildByName(transform, "Image_ItemIcon");
            Image_Icon = iconTransform != null ? iconTransform.GetComponent<Image>() : null;
        }
    }

    /// <summary>
    /// 슬롯에 표시할 아이템 이름과 수량 문구를 적용합니다.
    /// </summary>
    public void RequestSetupText(string labelText)
    {
        ResolveReferences();

        if (Text_Label != null)
            Text_Label.text = labelText;
    }

    /// <summary>
    /// 슬롯에 아이템 아이콘과 이름/수량 문구를 함께 적용합니다.
    /// </summary>
    public void RequestSetupItem(string itemDataId, string labelText, Sprite iconSprite)
    {
        RequestSetupText(labelText);
        RequestApplySlotLayout();
        RequestApplyIconView(iconSprite);
    }

    /// <summary>
    /// 인벤토리 슬롯 안에서 정사각형 음식 이미지와 이름/수량 텍스트가 겹치지 않게 배치합니다.
    /// 영화로 치면 재료 사진은 소품 칸에, 이름표는 그 옆 설명 칸에 붙여 두는 일입니다.
    /// </summary>
    private void RequestApplySlotLayout()
    {
        if (Image_Icon != null && Image_Icon.transform is RectTransform iconRect)
        {
            iconRect.anchorMin = new Vector2(1f, 0.5f);
            iconRect.anchorMax = new Vector2(1f, 0.5f);
            iconRect.pivot = new Vector2(1f, 0.5f);
            iconRect.anchoredPosition = new Vector2(-_iconRightPadding, 0f);
            iconRect.sizeDelta = _iconSize;
        }

        if (Text_Label != null && Text_Label.transform is RectTransform textRect)
        {
            float textRightPadding = _iconRightPadding + _iconSize.x + _textRightGapFromIcon;

            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(_textLeftPadding, 0f);
            textRect.offsetMax = new Vector2(-textRightPadding, 0f);
            Text_Label.alignment = TextAlignmentOptions.MidlineLeft;
            Text_Label.textWrappingMode = TextWrappingModes.NoWrap;
        }
    }

    /// <summary>
    /// JSON IconPath로 불러온 Sprite를 슬롯의 Image_ItemIcon 배우에게 전달합니다.
    /// 아이콘이 없을 때는 슬롯 배경을 건드리지 않고 텍스트만 유지합니다.
    /// </summary>
    private void RequestApplyIconView(Sprite iconSprite)
    {
        if (Image_Icon == null)
            return;

        Image_Icon.gameObject.SetActive(true);
        Image_Icon.sprite = iconSprite;
        Image_Icon.enabled = iconSprite != null;
        Image_Icon.preserveAspect = true;
        Image_Icon.raycastTarget = false;

        Color iconColor = Color.white;
        iconColor.a = iconSprite != null ? 1f : 0f;
        Image_Icon.color = iconColor;
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
