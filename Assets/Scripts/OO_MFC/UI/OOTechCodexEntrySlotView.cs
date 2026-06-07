// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechCodexEntrySlotView.cs
// - ??븷: CodexGroup ?ㅽ겕濡?紐⑸줉???꾧컧 ??ぉ ??以꾩쓣 ?쒖떆?⑸땲??
// - ?곹솕 鍮꾩쑀: ?꾨줈洹몃옩遺?紐⑹감????以꾩엯?덈떎. 媛먮룆???꾨땲?? ?먭린 ?쒕ぉ怨??대┃ ?좏샇留??대떦?⑸땲??
// - ?좎?蹂댁닔 ?ъ씤: ?곗씠??濡쒕뵫? GameDataManager媛 留↔퀬, ??View??諛쏆? ?곗씠?곕쭔 ?붾㈃??蹂댁뿬以띾땲??
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
    [SerializeField] private Button Button_Select;

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
            Text_Category.text = codexData != null ? codexData.Category : string.Empty;

        if (Text_Title != null)
            Text_Title.text = codexData != null ? codexData.Title : string.Empty;

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

        if (Text_Title == null)
            Text_Title = RequestText("Text_Title", "Text_Label");

        if (Text_Category == null)
            Text_Category = RequestText("Text_Category");

        OOTechTMPFontUtility.ApplyProjectFont(Text_Category);
        OOTechTMPFontUtility.ApplyProjectFont(Text_Title);
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

