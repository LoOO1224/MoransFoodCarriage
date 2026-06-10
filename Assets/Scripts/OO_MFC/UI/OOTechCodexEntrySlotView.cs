// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechCodexEntrySlotView.cs
// - 역할: UI 오브젝트 참조, 표시 갱신, 버튼 입력 연결을 담당합니다.
// - 유지보수: 씬 Hierarchy 이름으로 런타임 참조를 복구하는 코드가 많아 오브젝트 이름 변경에 주의합니다.
// =============================================================================
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ?꾧컧 紐⑸줉 ?щ’ ??移몄엯?덈떎.
/// Game View?먯꽌??移댄뀒怨좊━? ?쒕ぉ???쒖떆?섍퀬, ?대┃?섎㈃ CodexGroupController???좏깮 ?좏샇瑜?蹂대깄?덈떎.
/// </summary>
[DisallowMultipleComponent]
public class OOTechCodexEntrySlotView : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private TextMeshProUGUI Text_Category;
    [SerializeField] private TextMeshProUGUI Text_Title;
    [SerializeField] private Image Image_Background;
    [SerializeField] private Button Button_Select;

    [Header("Style")]
    [SerializeField] private Color _slotBackgroundColor = new Color(0.68f, 0.84f, 0.54f, 0.82f);
    [SerializeField] private Color _slotTextColor = new Color(0.08f, 0.07f, 0.04f, 1f);
    [SerializeField] private float _slotHeight = 44f;

    private OO_Codex _codexData;
    private Action<OO_Codex> _onSelected;

    /// <summary>
    /// ?щ’???꾧컧 ?곗씠?곕? 諛곗튂?⑸땲??
    /// ?곹솕濡?移섎㈃ 紐⑹감 移대뱶???ㅻ뒛 ?뚭컻??諛곗슦 ?대쫫??苑귥븘 ?ｋ뒗 ?④퀎?낅땲??
    /// </summary>
    public void RequestSetup(OO_Codex codexData, Action<OO_Codex> onSelected)
    {
        ResolveReferences();
        _codexData = codexData;
        _onSelected = onSelected;

        if (Text_Category != null)
        {
            Text_Category.text = string.Empty;
            Text_Category.gameObject.SetActive(false);
        }

        if (Text_Title != null)
        {
            Text_Title.text = codexData != null ? codexData.Title : string.Empty;
            Text_Title.color = _slotTextColor;
            Text_Title.alignment = TextAlignmentOptions.MidlineLeft;
        }

        ApplySlotBackgroundStyle();
        ApplySlotLayout();

        BindButtonEvent();
        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        UnbindButtonEvent();
    }

    private void ResolveReferences()
    {
        if (Button_Select == null)
            Button_Select = GetComponent<Button>();

        if (Button_Select == null)
            Button_Select = gameObject.AddComponent<Button>();

        if (Image_Background == null)
            Image_Background = GetComponent<Image>();

        if (Image_Background == null && Button_Select != null)
            Image_Background = Button_Select.GetComponent<Image>();

        if (Image_Background == null)
            Image_Background = gameObject.AddComponent<Image>();

        if (Text_Title == null)
            Text_Title = RequestText("Text_Title", "Text_Label");

        if (Text_Title == null)
            Text_Title = CreateTitleText();

        if (Text_Category == null)
            Text_Category = RequestText("Text_Category");

        OOTechTMPFontUtility.ApplyProjectFont(Text_Category);
        OOTechTMPFontUtility.ApplyProjectFont(Text_Title);
    }

    /// <summary>
    /// 도감 목록 한 줄을 연녹색 표지처럼 정리합니다.
    /// Game View에서는 반복되는 "캐릭터" 꼬리표 없이 이름만 읽기 쉬운 줄로 보입니다.
    /// </summary>
    private void ApplySlotBackgroundStyle()
    {
        if (Image_Background == null)
            return;

        Image_Background.color = _slotBackgroundColor;
        Image_Background.type = Image.Type.Sliced;
        Image_Background.raycastTarget = true;
    }

    /// <summary>
    /// 도감 한 줄 카드가 서로 겹치지 않도록 최소 높이를 고정합니다.
    /// Game View에서는 캐릭터 이름들이 한 덩어리로 쌓이지 않고, 각각 클릭 가능한 캡슐 줄로 보입니다.
    /// </summary>
    private void ApplySlotLayout()
    {
        RectTransform rectTransform = transform as RectTransform;

        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0f, rectTransform.anchorMin.y);
            rectTransform.anchorMax = new Vector2(1f, rectTransform.anchorMax.y);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _slotHeight);
        }

        LayoutElement layoutElement = GetComponent<LayoutElement>();

        if (layoutElement == null)
            layoutElement = gameObject.AddComponent<LayoutElement>();

        layoutElement.minHeight = _slotHeight;
        layoutElement.preferredHeight = _slotHeight;
        layoutElement.flexibleHeight = 0f;

        if (Text_Title != null)
        {
            RectTransform titleRect = Text_Title.rectTransform;
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(12f, 0f);
            titleRect.offsetMax = new Vector2(-12f, 0f);
            Text_Title.textWrappingMode = TextWrappingModes.NoWrap;
            Text_Title.overflowMode = TextOverflowModes.Ellipsis;
            Text_Title.fontSize = Mathf.Clamp(Text_Title.fontSize <= 0f ? 28f : Text_Title.fontSize, 22f, 30f);
            Text_Title.color = _slotTextColor;
            Text_Title.raycastTarget = false;
            Text_Title.gameObject.SetActive(true);
            Text_Title.transform.SetAsLastSibling();
        }
    }

    private TextMeshProUGUI CreateTitleText()
    {
        GameObject textObject = new GameObject("Text_Title", typeof(RectTransform));
        textObject.transform.SetParent(transform, false);
        TextMeshProUGUI titleText = textObject.AddComponent<TextMeshProUGUI>();
        OOTechTMPFontUtility.ApplyProjectFont(titleText);
        return titleText;
    }

    public void RequestSetSlotHeight(float slotHeight)
    {
        _slotHeight = Mathf.Max(28f, slotHeight);
        ApplySlotLayout();
    }

    private TextMeshProUGUI RequestText(params string[] nameArray)
    {
        TextMeshProUGUI[] textArray = GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (string textName in nameArray)
        {
            foreach (TextMeshProUGUI text in textArray)
            {
                if (text != null && text.name == textName)
                    return text;
            }
        }

        return textArray.Length > 0 ? textArray[0] : null;
    }

    private void BindButtonEvent()
    {
        if (Button_Select == null)
            return;

        Button_Select.onClick.RemoveListener(OnSelectButtonClicked);
        Button_Select.onClick.AddListener(OnSelectButtonClicked);
    }

    private void UnbindButtonEvent()
    {
        if (Button_Select == null)
            return;

        Button_Select.onClick.RemoveListener(OnSelectButtonClicked);
    }

    private void OnSelectButtonClicked()
    {
        _onSelected?.Invoke(_codexData);
    }
}

