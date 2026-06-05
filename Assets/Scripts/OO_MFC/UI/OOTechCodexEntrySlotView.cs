// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCodexEntrySlotView.cs
// - 역할: CodexGroup 스크롤 목록의 도감 항목 한 줄을 표시합니다.
// - 영화 비유: 프로그램북 목차의 한 줄입니다. 감독이 아니라, 자기 제목과 클릭 신호만 담당합니다.
// - 유지보수 사인: 데이터 로딩은 GameDataManager가 맡고, 이 View는 받은 데이터만 화면에 보여줍니다.
// =============================================================================
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 도감 목록 슬롯 한 칸입니다.
/// Game View에서는 카테고리와 제목을 표시하고, 클릭되면 CodexGroupController에 선택 신호를 보냅니다.
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
    /// 슬롯에 도감 데이터를 배치합니다.
    /// 영화로 치면 목차 카드에 오늘 소개할 배우 이름을 꽂아 넣는 단계입니다.
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
            Text_Title = FindText("Text_Title", "Text_Label");

        if (Text_Category == null)
            Text_Category = FindText("Text_Category");

        OOTechTMPFontUtility.ApplyProjectFont(Text_Category);
        OOTechTMPFontUtility.ApplyProjectFont(Text_Title);
    }

    private TextMeshProUGUI FindText(params string[] nameArray)
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
