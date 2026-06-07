// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechInventorySlotView.cs
// - ??븷: UI ?섏씠?대씪?ㅼ쓽 踰꾪듉, ?대?吏, ?띿뒪??李몄“瑜?紐⑥븘 ??View 而댄룷?뚰듃?낅땲??
// - 媛먮룆 愿?? 臾대? ???뚰뭹 ?꾩튂?쒖엯?덈떎. ?먮떒?섏? ?딄퀬 ?뚰뭹??蹂댁뿬 二쇰뒗 ?쇰쭔 留≪뒿?덈떎.
// - ?좎?蹂댁닔 ?ъ씤?? 踰꾪듉 ?숈옉 ?먮떒, ?곗씠??濡쒕뵫, 洹몃９ ?꾪솚 濡쒖쭅? Controller??Manager???〓땲??
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ?몃깽?좊━ ?щ’ ??移몄쓽 ?쒖떆 ?띿뒪?몃? ?대떦?⑸땲??
/// Game View?먯꽌??"? x2" 媛숈? ?щ’ 臾멸뎄瑜?蹂댁뿬二쇰뒗 ?묒? ?뚰뭹?낅땲??
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
    /// ?щ’ ?덉쓽 TMP ?띿뒪?몃? 李얠븘 ??ν빀?덈떎.
    /// </summary>
    public void ResolveReferences()
    {
        if (Text_Label == null)
            Text_Label = GetComponentInChildren<TextMeshProUGUI>(true);

        if (Image_Icon == null)
        {
            Transform iconTransform = RequestChildObjectByName(transform, "Image_ItemIcon");
            Image_Icon = iconTransform != null ? iconTransform.GetComponent<Image>() : null;
        }
    }

    /// <summary>
    /// ?щ’???쒖떆???꾩씠???대쫫怨??섎웾 臾멸뎄瑜??곸슜?⑸땲??
    /// </summary>
    public void RequestSetupText(string labelText)
    {
        ResolveReferences();

        if (Text_Label != null)
            Text_Label.text = labelText;
    }

    /// <summary>
    /// ?щ’???꾩씠???꾩씠肄섍낵 ?대쫫/?섎웾 臾멸뎄瑜??④퍡 ?곸슜?⑸땲??
    /// </summary>
    public void RequestSetupItem(string itemDataId, string labelText, Sprite iconSprite)
    {
        RequestSetupText(labelText);
        RequestApplySlotLayout();
        RequestApplyIconView(iconSprite);
    }

    /// <summary>
    /// ?몃깽?좊━ ?щ’ ?덉뿉???뺤궗媛곹삎 ?뚯떇 ?대?吏? ?대쫫/?섎웾 ?띿뒪?멸? 寃뱀튂吏 ?딄쾶 諛곗튂?⑸땲??
    /// ?곹솕濡?移섎㈃ ?щ즺 ?ъ쭊? ?뚰뭹 移몄뿉, ?대쫫?쒕뒗 洹????ㅻ챸 移몄뿉 遺숈뿬 ?먮뒗 ?쇱엯?덈떎.
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
    /// JSON IconPath濡?遺덈윭??Sprite瑜??щ’??Image_ItemIcon 諛곗슦?먭쾶 ?꾨떖?⑸땲??
    /// ?꾩씠肄섏씠 ?꾩쭅 ?놁쓣 ?뚮뒗 ItemCatalogManager媛 ?꾩떆 鍮??꾩씠肄섏쓣 ?섍꺼 ?뚮젅?댁뼱媛 鍮??먮━瑜??뚯븘蹂????덇쾶 ?⑸땲??
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

