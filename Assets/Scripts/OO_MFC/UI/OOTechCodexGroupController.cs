// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechCodexGroupController.cs
// - 역할: UI 오브젝트 참조, 표시 갱신, 버튼 입력 연결을 담당합니다.
// - 유지보수: 씬 Hierarchy 이름으로 런타임 참조를 복구하는 코드가 많아 오브젝트 이름 변경에 주의합니다.
// =============================================================================
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// CodexGroup???쒖떆 諛⑹떇??愿由ы빀?덈떎.
/// ?꾩옱 諛쒗몴 踰꾩쟾?먯꽌???덈궡臾몃쭔 蹂댁뿬二쇰ŉ, 李??꾨Т 怨녹씠???대┃?섎㈃ ?덈궡李쎈쭔 ?レ뒿?덈떎.
/// CodexGroup???먮낯 諛곌꼍怨??뚯븘媛湲?踰꾪듉? ?좎??댁꽌, ?뚯븘媛湲곕뒗 BackButtonController媛 ?대떦?⑸땲??
/// </summary>
[DisallowMultipleComponent]
public class OOTechCodexGroupController : MonoBehaviour, IPointerClickHandler
{
    [Header("List")]
    [SerializeField] private ScrollRect Scroll_CodexList;
    [SerializeField] private RectTransform Rect_CodexContent;
    [SerializeField] private OOTechCodexEntrySlotView View_SlotTemplate;

    [Header("Detail")]
    [SerializeField] private RectTransform Rect_DetailPanel;
    [SerializeField] private TextMeshProUGUI Text_DetailTitle;
    [SerializeField] private TextMeshProUGUI Text_DetailCategory;
    [SerializeField] private TextMeshProUGUI Text_DetailDescription;
    [SerializeField] private Image Image_DetailPreview;
    [SerializeField] private ScrollRect Scroll_DetailDescription;

    [Header("Page")]
    [SerializeField] private Button Button_PreviousPage;
    [SerializeField] private Button Button_NextPage;
    [SerializeField] private TextMeshProUGUI Text_PageLabel;

    [Header("Announcement")]
    [SerializeField] private bool _isUseAnnouncementOnly = false;
    [SerializeField] private bool _isCloseAnnouncementByAnyClick = true;
    [SerializeField] private bool _isCloseCodexByOutsideClick = true;
    [SerializeField] private string _announcementTitle = "도감";
    [SerializeField] private string _announcementCategory = "발표 전 업데이트 예정";
    [TextArea(3, 8)]
    [SerializeField] private string _announcementDescription = "도감은 발표 전 업데이트 예정입니다.\n캐릭터, 음식, 재료, 나레이션, 스테이지 정보를 데이터 기반으로 정리합니다.";

    [Header("Layout")]
    [SerializeField] private float _codexListSlotHeight = 44f;
    [SerializeField] private float _codexListSlotSpacing = 2f;
    [SerializeField] private Vector2 _detailDescriptionPadding = new Vector2(18f, 18f);

    private readonly List<OOTechCodexEntrySlotView> _spawnedSlotViewList = new List<OOTechCodexEntrySlotView>();
    private GameObject Root_AnnouncementPanel;
    private TextMeshProUGUI Text_RuntimeDetailDescription;
    private RectTransform Rect_DetailDescriptionContent;
    private int _currentPageIndex;
    private int _openedFrame;
    private bool _isAnnouncementClosed;

    private void OnEnable()
    {
        _openedFrame = Time.frameCount;
        _isAnnouncementClosed = false;
        ResolveReferences();
        BindPageButtons();

        if (_isUseAnnouncementOnly)
        {
            RequestShowAnnouncementOnly();
            return;
        }

        RequestSetCodexPageNavigationActive(true);
        RequestSetCodexContentActive(true);
        RequestRefreshCodexList();
    }

    private void Update()
    {
        if (!_isUseAnnouncementOnly)
            HandlePageKeyboardInput();

        if (Time.frameCount <= _openedFrame + 1)
            return;

        if (CanCloseAnnouncementByClick() && Input.GetMouseButtonDown(0))
            RequestCloseAnnouncementPanel();

        if (!_isUseAnnouncementOnly && _isCloseCodexByOutsideClick && Input.GetMouseButtonDown(0) && !IsPointerInsideCodexContent(Input.mousePosition))
            RequestHideCodexContentOnly();
    }

    /// <summary>
    /// 諛쒗몴 ???덈궡 紐⑤뱶?먯꽌???꾧컧 李??꾨Т 怨녹씠???대┃?대룄 ?덈궡李쎈쭔 ?レ뒿?덈떎.
    /// Game View?먯꽌??諛곌꼍 梨낆옣???④퀬, ?뚮젅?댁뼱???뚯븘媛湲?踰꾪듉?쇰줈 ?댁쟾 臾대???蹂듦??⑸땲??
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (CanCloseAnnouncementByClick())
            RequestCloseAnnouncementPanel();
    }

    /// <summary>
    /// ?섏씠?대씪?ㅼ뿉 誘몃━ 諛곗튂??Codex UI 諛곗슦?ㅼ쓣 ?대쫫?쇰줈 李얠븘 ?곌껐?⑸땲??
    /// 媛먮룆??留ㅻ쾲 ?뚰뭹???덈줈 留뚮뱾吏 ?딄퀬, 臾대? ?꾩뿉 ?볦씤 ?뚰뭹????븷?쒕? 遺숈씠???④퀎?낅땲??
    /// </summary>
    private void ResolveReferences()
    {
        if (Scroll_CodexList == null)
            Scroll_CodexList = GetComponentInChildren<ScrollRect>(true);

        if (Rect_CodexContent == null && Scroll_CodexList != null)
            Rect_CodexContent = Scroll_CodexList.content;

        if (View_SlotTemplate == null)
            View_SlotTemplate = GetComponentInChildren<OOTechCodexEntrySlotView>(true);

        Text_DetailTitle = ResolveText(Text_DetailTitle, "Text_DetailTitle", "Text_Title");
        Text_DetailCategory = ResolveText(Text_DetailCategory, "Text_DetailCategory", "Text_Category");
        Text_DetailDescription = ResolveText(Text_DetailDescription, "Text_DetailDescription", "Text_Description");
        Rect_DetailPanel = ResolveDetailPanel(Rect_DetailPanel);
        Image_DetailPreview = ResolveImage(Image_DetailPreview, "Image_DetailPreview", "Image_Preview");
        Button_PreviousPage = ResolveButton(Button_PreviousPage, "Button_PreviousPage", "Button_PagePrev");
        Button_NextPage = ResolveButton(Button_NextPage, "Button_NextPage", "Button_PageNext");
        Text_PageLabel = ResolveText(Text_PageLabel, "Text_PageLabel", "Text_Page");
        Root_AnnouncementPanel = ResolveGameObject(Root_AnnouncementPanel, "Panel_CodexRoot");

        OOTechTMPFontUtility.ApplyProjectFont(Text_DetailTitle);
        OOTechTMPFontUtility.ApplyProjectFont(Text_DetailCategory);
        OOTechTMPFontUtility.ApplyProjectFont(Text_DetailDescription);
        OOTechTMPFontUtility.ApplyProjectFont(Text_PageLabel);

        EnsureCodexListScrollLayout();
        EnsureDetailDescriptionScrollLayout();
    }

    /// <summary>
    /// OO_Codex.json???꾧컧 紐⑸줉???ㅽ겕濡?由ъ뒪?몄뿉 ?ㅼ떆 梨꾩썎?덈떎.
    /// 諛쒗몴 ?댄썑 ?ㅼ젣 ?꾧컧???대┫ ???ъ슜?섎뒗 紐⑸줉 紐⑤뱶?낅땲??
    /// </summary>
    public void RequestRefreshCodexList()
    {
        if (Rect_CodexContent == null || View_SlotTemplate == null)
        {
            Debug.LogWarning("[OOTechCodexGroupController] Codex list objects are missing. Check Scroll_CodexList and Slot template.");
            return;
        }

        if (Scroll_CodexList != null)
            Scroll_CodexList.gameObject.SetActive(true);

        ClearSlotViews();

        List<OO_Codex> codexDataList = CreateCurrentPageCodexDataList();

        foreach (OO_Codex codexData in codexDataList)
            CreateSlotView(codexData);

        if (View_SlotTemplate != null)
            View_SlotTemplate.gameObject.SetActive(false);

        Canvas.ForceUpdateCanvases();

        if (Scroll_CodexList != null)
            Scroll_CodexList.verticalNormalizedPosition = 1f;

        if (codexDataList.Count > 0)
            RequestSelectCodex(codexDataList[0]);
        else
            RequestSelectCodex(null);
    }

    private void CreateSlotView(OO_Codex codexData)
    {
        OOTechCodexEntrySlotView slotView = Instantiate(View_SlotTemplate, Rect_CodexContent);
        slotView.name = $"Slot_Codex_{codexData.Id}";
        slotView.RequestSetup(codexData, RequestSelectCodex);
        slotView.RequestSetSlotHeight(_codexListSlotHeight);
        _spawnedSlotViewList.Add(slotView);
    }

    private void ClearSlotViews()
    {
        foreach (OOTechCodexEntrySlotView slotView in _spawnedSlotViewList)
        {
            if (slotView != null)
                Destroy(slotView.gameObject);
        }

        _spawnedSlotViewList.Clear();
    }

    private void BindPageButtons()
    {
        if (Button_PreviousPage != null)
        {
            Button_PreviousPage.onClick.RemoveListener(RequestPreviousPage);
            Button_PreviousPage.onClick.AddListener(RequestPreviousPage);
        }

        if (Button_NextPage != null)
        {
            Button_NextPage.onClick.RemoveListener(RequestNextPage);
            Button_NextPage.onClick.AddListener(RequestNextPage);
        }
    }

    private void RequestPreviousPage()
    {
        _currentPageIndex = Mathf.Max(0, _currentPageIndex - 1);
        RequestSetCodexContentActive(true);
        RequestRefreshCodexList();
    }

    private void RequestNextPage()
    {
        _currentPageIndex = Mathf.Min(4, _currentPageIndex + 1);
        RequestSetCodexContentActive(true);
        RequestRefreshCodexList();
    }

    private void HandlePageKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            RequestPreviousPage();

        if (Input.GetKeyDown(KeyCode.RightArrow))
            RequestNextPage();
    }

    private List<OO_Codex> CreateCurrentPageCodexDataList()
    {
        List<OO_Codex> dataList = new List<OO_Codex>();

        if (OOTechGameDataManager.Inst == null)
            return dataList;

        if (_currentPageIndex == 0)
            AppendCharacterPage(dataList);
        else if (_currentPageIndex == 1)
            AppendCookPage(dataList);
        else if (_currentPageIndex == 2)
            AppendIngredientPage(dataList);
        else if (_currentPageIndex == 3)
            AppendNarrationPage(dataList);
        else
            AppendStagePage(dataList);

        if (Text_PageLabel != null)
            Text_PageLabel.text = ResolveCurrentPageName();

        return dataList;
    }

    private void AppendCharacterPage(List<OO_Codex> dataList)
    {
        foreach (OO_Character characterData in OOTechGameDataManager.Inst.GetCharacterDataList())
        {
            dataList.Add(new OO_Codex
            {
                Id = characterData.Id,
                Category = "캐릭터",
                Title = string.IsNullOrEmpty(characterData.Name) ? characterData.Id : characterData.Name,
                Description = ResolveReadableDescription("캐릭터", characterData.Id, characterData.Name, characterData.Description),
                ImagePath = characterData.ProfileImagePath
            });
        }
    }

    private void AppendCookPage(List<OO_Codex> dataList)
    {
        foreach (OO_Cook cookData in OOTechGameDataManager.Inst.GetCookDataList())
        {
            dataList.Add(new OO_Codex
            {
                Id = cookData.Id,
                Category = "요리",
                Title = string.IsNullOrEmpty(cookData.Name) ? cookData.Id : cookData.Name,
                Description = ResolveReadableDescription("요리", cookData.Id, cookData.Name, CreateCombinedDescription(cookData.Description, cookData.EffectDescription)),
                ImagePath = cookData.IconPath
            });
        }
    }

    private void AppendIngredientPage(List<OO_Codex> dataList)
    {
        foreach (OO_Ingredient ingredientData in OOTechGameDataManager.Inst.GetIngredientDataList())
        {
            dataList.Add(new OO_Codex
            {
                Id = ingredientData.Id,
                Category = "재료",
                Title = string.IsNullOrEmpty(ingredientData.Name) ? ingredientData.Id : ingredientData.Name,
                Description = ResolveReadableDescription("재료", ingredientData.Id, ingredientData.Name, ingredientData.Description),
                ImagePath = ingredientData.IconPath
            });
        }
    }

    private void AppendNarrationPage(List<OO_Codex> dataList)
    {
        foreach (OO_Narration narrationData in OOTechGameDataManager.Inst.GetNarrationDataList())
        {
            dataList.Add(new OO_Codex
            {
                Id = narrationData.Id,
                Category = "나레이션",
                Title = string.IsNullOrEmpty(narrationData.Title) ? narrationData.Id : narrationData.Title,
                Description = ResolveReadableDescription("스토리", narrationData.Id, narrationData.Title, narrationData.NarrationTexts != null && narrationData.NarrationTexts.Count > 0 ? string.Join("\n", narrationData.NarrationTexts) : string.Empty),
                ImagePath = narrationData.BackgroundImagePaths != null && narrationData.BackgroundImagePaths.Count > 0 ? narrationData.BackgroundImagePaths[0] : string.Empty
            });
        }
    }

    private void AppendStagePage(List<OO_Codex> dataList)
    {
        foreach (OO_Stage stageData in OOTechGameDataManager.Inst.GetStageDataList())
        {
            dataList.Add(new OO_Codex
            {
                Id = stageData.Id,
                Category = "스테이지",
                Title = string.IsNullOrEmpty(stageData.Name) ? stageData.Id : stageData.Name,
                Description = ResolveReadableDescription("스테이지", stageData.Id, stageData.Name, CreateCombinedDescription(stageData.Description, stageData.QuestDescription)),
                ImagePath = stageData.BackgroundImagePath
            });
        }
    }

    private string ResolveCurrentPageName()
    {
        if (_currentPageIndex == 0)
            return "캐릭터";

        if (_currentPageIndex == 1)
            return "음식";

        if (_currentPageIndex == 2)
            return "재료";

        if (_currentPageIndex == 3)
            return "스토리";

        return "스테이지";
    }

    private string CreateCombinedDescription(string firstText, string secondText)
    {
        string first = string.IsNullOrEmpty(firstText) ? string.Empty : firstText;
        string second = string.IsNullOrEmpty(secondText) ? string.Empty : secondText;

        if (string.IsNullOrEmpty(first) && string.IsNullOrEmpty(second))
            return "설명 준비 중입니다.";

        if (string.IsNullOrEmpty(first))
            return second;

        if (string.IsNullOrEmpty(second))
            return first;

        return first + "\n\n" + second;
    }

    private string ResolveReadableDescription(string category, string id, string title, string rawDescription)
    {
        if (IsReadableDescription(rawDescription))
            return rawDescription;

        return ResolvePresentationFallbackDescription(category, id, title);
    }

    private bool IsReadableDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string trimmedValue = value.Trim();

        if (trimmedValue == "설명 준비 중입니다.")
            return false;

        int koreanCount = 0;

        foreach (char character in trimmedValue)
        {
            if (character >= '\uAC00' && character <= '\uD7A3')
                koreanCount++;
        }

        return koreanCount >= 2;
    }

    private string ResolvePresentationFallbackDescription(string category, string id, string title)
    {
        string key = ((title ?? string.Empty) + "|" + (id ?? string.Empty)).ToLowerInvariant();

        if (key.Contains("모란") || key.Contains("moran"))
            return "모란은 이야기의 중심을 이끄는 주인공입니다.\n푸드카리지를 타고 각 지역을 지나며 재료를 모으고, 요리를 완성해 사람들과 동물들의 문제를 풀어갑니다.";

        if (key.Contains("춘양"))
            return "춘양은 프롤로그와 여정 초반에 등장하는 화자입니다.\n따뜻한 말투로 모란의 길을 열어 주며, 플레이어가 세계관과 첫 임무를 이해하도록 안내합니다.";

        if (key.Contains("춘장"))
            return "춘장은 마을의 중심에 서 있는 인물입니다.\n귀한 쌀과 백성들의 사정을 둘러싼 이야기를 통해 첫 번째 여정의 목표를 선명하게 만들어 줍니다.";

        if (key.Contains("재익") || key.Contains("jaeik"))
            return "재익군은 모란과 함께 여정을 밀고 나가는 동료입니다.\n위험한 상황에서도 끝까지 곁을 지키며, 산군과의 조우 이후 승리 장면에서 모란과 함께 기쁨을 나눕니다.";

        if (key.Contains("산군") || key.Contains("sangun"))
            return "산군은 깊은 숲에서 만나는 강력한 존재입니다.\n꿀떡을 통해 마음을 돌릴 수 있으며, 스테이지3의 긴장과 해결을 담당하는 핵심 캐릭터입니다.";

        if (key.Contains("토끼") || key.Contains("rabbit"))
            return "토끼는 마지막 여정에서 모란을 놀리며 등장하는 장난기 많은 캐릭터입니다.\n당근전으로 유혹해 잠들게 해야 하며, 거북이에게 돌아가는 마지막 임무의 열쇠가 됩니다.";

        if (key.Contains("거북") || key.Contains("turtle"))
            return "거북이는 Stage4의 의뢰를 이끄는 인물입니다.\n모란에게 토끼를 찾도록 부탁하고, 마지막 보고를 통해 모든 스테이지 임무 완료로 이어 줍니다.";

        if (key.Contains("연산") || key.Contains("yeonsan") || key.Contains("yansan"))
            return "연산군자는 최종 장면에서 마주하는 인물입니다.\n음식과 이야기가 끝내 닿는 마지막 대상으로, 엔딩 직전의 긴장과 해소를 담당합니다.";

        if (key.Contains("당근전"))
            return "당근전은 떡과 당근으로 당근전분을 만든 뒤, 가마솥에 넣어 완성하는 Stage4 핵심 음식입니다.\n토끼를 유혹하고 마지막 임무를 진행하기 위해 반드시 필요한 요리입니다.";

        if (key.Contains("꿀떡"))
            return "꿀떡은 떡과 꿀을 조합해 만드는 음식입니다.\n산군의 마음을 돌리는 데 쓰이며, Stage3의 해결 조건이 되는 중요한 음식입니다.";

        if (key.Contains("떡"))
            return "떡은 쌀을 절구로 찧어 만드는 기본 음식입니다.\n꿀떡과 당근전분의 재료가 되므로 Stage3 이후 요리 진행에서 계속 중요한 역할을 합니다.";

        if (category == "스토리")
            return "이 항목은 모란의 여정을 구성하는 이야기 장면입니다.\n대사와 나레이션을 통해 지역의 분위기, 임무의 목적, 다음 장면으로 넘어가는 이유를 전달합니다.";

        if (category == "스테이지")
            return "이 스테이지는 모란의 푸드카리지 여정 중 하나입니다.\n재료 수집, 요리 제작, 인물과의 상호작용을 통해 다음 목적지로 이어지는 진행 구간입니다.";

        if (category == "요리")
            return "이 음식은 여정 중 수집한 재료를 조합해 만드는 결과물입니다.\n인벤토리에 들어간 뒤 임무 진행, 보상 교환, 캐릭터 설득에 사용됩니다.";

        if (category == "재료")
            return "이 재료는 요리를 만들기 위한 기본 아이템입니다.\n인벤토리에 보관되며, 조합 패널이나 조리도구에 넣어 새로운 음식으로 바꿀 수 있습니다.";

        return "이 항목은 모란의 푸드카리지 여정에 등장하는 도감 데이터입니다.\n게임 진행 중 만나는 인물, 장소, 음식, 사건을 정리해 플레이어가 다시 읽어볼 수 있게 합니다.";
    }

    /// <summary>
    /// 諛쒗몴 ???꾧컧? 紐⑸줉???댁? ?딄퀬 ?덈궡臾몃쭔 蹂댁뿬以띾땲??
    /// Game View?먯꽌??? ?섎굹吏쒕━ ?꾩떆 紐⑸줉 ???"諛쒗몴 ???낅뜲?댄듃 ?덉젙" 臾멸뎄媛 怨좎젙?⑸땲??
    /// </summary>
    private void RequestShowAnnouncementOnly()
    {
        ClearSlotViews();
        _isAnnouncementClosed = false;

        if (Root_AnnouncementPanel != null)
            Root_AnnouncementPanel.SetActive(true);

        if (View_SlotTemplate != null)
            View_SlotTemplate.gameObject.SetActive(false);

        if (Scroll_CodexList != null)
            Scroll_CodexList.gameObject.SetActive(false);

        if (Text_DetailTitle != null)
            Text_DetailTitle.text = _announcementTitle;

        if (Text_DetailCategory != null)
            Text_DetailCategory.text = _announcementCategory;

        if (Text_DetailDescription != null)
            Text_DetailDescription.text = _announcementDescription;

        RefreshDetailDescriptionScrollPosition(_announcementDescription);
        RequestForceDetailTextsVisible(null, _announcementDescription);

        if (Image_DetailPreview != null)
        {
            Image_DetailPreview.sprite = null;
            Image_DetailPreview.enabled = true;
        }

        Debug.Log("[OOTechCodexGroupController] Codex list is locked for presentation. Click anywhere to close.");
    }

    private bool CanCloseAnnouncementByClick()
    {
        return _isUseAnnouncementOnly &&
               _isCloseAnnouncementByAnyClick &&
               !_isAnnouncementClosed &&
               gameObject.activeInHierarchy &&
               (Root_AnnouncementPanel == null || Root_AnnouncementPanel.activeInHierarchy);
    }

    private void RequestCloseAnnouncementPanel()
    {
        if (_isAnnouncementClosed)
            return;

        _isAnnouncementClosed = true;

        if (Root_AnnouncementPanel != null)
            Root_AnnouncementPanel.SetActive(false);

        Debug.Log("[OOTechCodexGroupController] Announcement panel closed. Codex background and return button stay active.");
    }

    /// <summary>
    /// ?좏깮???꾧컧 ??ぉ???쒕ぉ, 遺꾨쪟, ?ㅻ챸, ?대?吏瑜??곸꽭 ?⑤꼸??蹂댁뿬以띾땲??
    /// </summary>
    private void RequestSelectCodex(OO_Codex codexData)
    {
        string descriptionText = codexData != null
            ? ResolveReadableDescription(ResolveDisplayCategory(codexData.Category), codexData.Id, codexData.Title, codexData.Description)
            : "등록된 도감 데이터가 없습니다.";

        if (Text_DetailTitle != null)
            Text_DetailTitle.text = codexData != null ? codexData.Title : "도감";

        if (Text_DetailCategory != null)
            Text_DetailCategory.text = codexData != null ? ResolveDisplayCategory(codexData.Category) : ResolveCurrentPageName();

        if (Text_DetailDescription != null)
            Text_DetailDescription.text = descriptionText;

        RefreshDetailDescriptionScrollPosition(descriptionText);

        if (Image_DetailPreview != null)
        {
            Image_DetailPreview.sprite = null;
            Image_DetailPreview.enabled = true;
        }

        RequestForceDetailTextsVisible(codexData, descriptionText);
    }

    private string ResolveDisplayCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return ResolveCurrentPageName();

        if (category == "Character")
            return "캐릭터";

        if (category == "Food" || category == "Cook")
            return "음식";

        if (category == "Ingredient")
            return "재료";

        if (category == "Story" || category == "Narration")
            return "스토리";

        if (category == "Region" || category == "Stage")
            return "스테이지";

        return category;
    }

    private void EnsureCodexListScrollLayout()
    {
        if (Scroll_CodexList == null)
            return;

        Scroll_CodexList.horizontal = false;
        Scroll_CodexList.vertical = true;
        Scroll_CodexList.movementType = ScrollRect.MovementType.Clamped;

        RectTransform scrollRect = Scroll_CodexList.transform as RectTransform;
        RectTransform viewportRect = Scroll_CodexList.viewport;

        if (viewportRect == null)
        {
            Transform viewportTransform = RequestChildObjectByName(Scroll_CodexList.transform, "Viewport");
            viewportRect = viewportTransform as RectTransform;
        }

        if (viewportRect != null)
        {
            Scroll_CodexList.viewport = viewportRect;
            RectMask2D viewportMask = viewportRect.GetComponent<RectMask2D>();

            if (viewportMask == null)
                viewportMask = viewportRect.gameObject.AddComponent<RectMask2D>();
        }

        if (Rect_CodexContent == null)
            Rect_CodexContent = Scroll_CodexList.content;

        if (Rect_CodexContent == null && viewportRect != null)
        {
            GameObject contentObject = new GameObject("Content", typeof(RectTransform));
            contentObject.transform.SetParent(viewportRect, false);
            Rect_CodexContent = contentObject.GetComponent<RectTransform>();
            Scroll_CodexList.content = Rect_CodexContent;
        }

        if (Rect_CodexContent == null)
            return;

        Rect_CodexContent.anchorMin = new Vector2(0f, 1f);
        Rect_CodexContent.anchorMax = new Vector2(1f, 1f);
        Rect_CodexContent.pivot = new Vector2(0.5f, 1f);
        Rect_CodexContent.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup layoutGroup = Rect_CodexContent.GetComponent<VerticalLayoutGroup>();

        if (layoutGroup == null)
            layoutGroup = Rect_CodexContent.gameObject.AddComponent<VerticalLayoutGroup>();

        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.spacing = _codexListSlotSpacing;
        layoutGroup.padding = new RectOffset(0, 0, 0, 0);

        ContentSizeFitter sizeFitter = Rect_CodexContent.GetComponent<ContentSizeFitter>();

        if (sizeFitter == null)
            sizeFitter = Rect_CodexContent.gameObject.AddComponent<ContentSizeFitter>();

        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        if (scrollRect != null && viewportRect != null)
        {
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
        }
    }

    private void EnsureDetailDescriptionScrollLayout()
    {
        if (Image_DetailPreview == null)
            return;

        RectTransform previewRect = Image_DetailPreview.transform as RectTransform;

        if (previewRect == null)
            return;

        Image_DetailPreview.enabled = true;
        Image_DetailPreview.raycastTarget = false;
        previewRect.SetAsFirstSibling();

        if (Image_DetailPreview.sprite == null)
            Image_DetailPreview.color = new Color(1f, 0.94f, 0.72f, 0.82f);

        RectMask2D mask = Image_DetailPreview.GetComponent<RectMask2D>();

        if (mask == null)
            mask = Image_DetailPreview.gameObject.AddComponent<RectMask2D>();

        Transform existingContentTransform = RequestChildObjectByName(previewRect, "Content_DetailDescription");
        RectTransform contentRect = existingContentTransform as RectTransform;

        if (contentRect == null)
        {
            GameObject contentObject = new GameObject("Content_DetailDescription", typeof(RectTransform));
            contentObject.transform.SetParent(previewRect, false);
            contentRect = contentObject.GetComponent<RectTransform>();
        }

        Rect_DetailDescriptionContent = contentRect;

        if (Scroll_DetailDescription == null)
            Scroll_DetailDescription = Image_DetailPreview.GetComponent<ScrollRect>();

        if (Scroll_DetailDescription == null)
            Scroll_DetailDescription = Image_DetailPreview.gameObject.AddComponent<ScrollRect>();

        Text_RuntimeDetailDescription = ResolveOrCreateRuntimeDetailDescription(contentRect);

        Scroll_DetailDescription.viewport = previewRect;
        Scroll_DetailDescription.content = contentRect;
        Scroll_DetailDescription.horizontal = false;
        Scroll_DetailDescription.vertical = true;
        Scroll_DetailDescription.movementType = ScrollRect.MovementType.Clamped;

        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.offsetMin = new Vector2(_detailDescriptionPadding.x, 0f);
        contentRect.offsetMax = new Vector2(-_detailDescriptionPadding.x, 0f);

        ContentSizeFitter contentSizeFitter = contentRect.GetComponent<ContentSizeFitter>();

        if (contentSizeFitter == null)
            contentSizeFitter = contentRect.gameObject.AddComponent<ContentSizeFitter>();

        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        RectTransform textRect = Text_RuntimeDetailDescription.rectTransform;
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = new Vector2(0f, -_detailDescriptionPadding.y);
        textRect.offsetMin = new Vector2(0f, textRect.offsetMin.y);
        textRect.offsetMax = new Vector2(0f, textRect.offsetMax.y);

        ApplyDetailDescriptionTextStyle(Text_RuntimeDetailDescription);
        Text_RuntimeDetailDescription.gameObject.SetActive(true);
        Text_RuntimeDetailDescription.transform.SetAsLastSibling();

        if (Text_DetailDescription != null && Text_DetailDescription != Text_RuntimeDetailDescription && Text_DetailDescription.transform.parent != contentRect)
            Text_DetailDescription.gameObject.SetActive(false);
    }

    private TextMeshProUGUI ResolveOrCreateRuntimeDetailDescription(RectTransform contentRect)
    {
        if (contentRect == null)
            return Text_DetailDescription;

        Transform existingTextTransform = RequestChildObjectByName(contentRect, "Text_DetailDescription_Runtime");
        TextMeshProUGUI runtimeText = existingTextTransform != null ? existingTextTransform.GetComponent<TextMeshProUGUI>() : null;

        if (runtimeText != null)
            return runtimeText;

        GameObject textObject = new GameObject("Text_DetailDescription_Runtime", typeof(RectTransform));
        textObject.transform.SetParent(contentRect, false);
        runtimeText = textObject.AddComponent<TextMeshProUGUI>();
        OOTechTMPFontUtility.ApplyProjectFont(runtimeText);
        return runtimeText;
    }

    private void ApplyDetailDescriptionTextStyle(TextMeshProUGUI targetText)
    {
        if (targetText == null)
            return;

        targetText.textWrappingMode = TextWrappingModes.Normal;
        targetText.overflowMode = TextOverflowModes.Overflow;
        targetText.alignment = TextAlignmentOptions.TopLeft;
        targetText.raycastTarget = false;
        targetText.color = new Color(0.04f, 0.035f, 0.025f, 1f);
        targetText.fontSize = 27f;
        targetText.lineSpacing = 12f;
        targetText.margin = Vector4.zero;
        targetText.gameObject.SetActive(true);
        OOTechTMPFontUtility.ApplyProjectFont(targetText);
    }

    private void RefreshDetailDescriptionScrollPosition(string descriptionText = null)
    {
        EnsureDetailDescriptionScrollLayout();

        TextMeshProUGUI visibleDescriptionText = Text_RuntimeDetailDescription != null ? Text_RuntimeDetailDescription : Text_DetailDescription;

        if (visibleDescriptionText != null && descriptionText != null)
            visibleDescriptionText.text = descriptionText;

        if (visibleDescriptionText != null)
        {
            ApplyDetailDescriptionTextStyle(visibleDescriptionText);
            visibleDescriptionText.ForceMeshUpdate();
        }

        Canvas.ForceUpdateCanvases();

        if (Scroll_DetailDescription != null)
        {
            RectTransform contentRect = Scroll_DetailDescription.content;

            if (contentRect != null && visibleDescriptionText != null)
            {
                RectTransform textRect = visibleDescriptionText.rectTransform;
                float preferredHeight = Mathf.Max(180f, visibleDescriptionText.preferredHeight + _detailDescriptionPadding.y * 2f);
                contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
                textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight - _detailDescriptionPadding.y * 2f);
            }

            Scroll_DetailDescription.verticalNormalizedPosition = 1f;
        }
    }

    private void RequestForceDetailTextsVisible(OO_Codex codexData, string descriptionText = null)
    {
        if (Text_DetailTitle != null)
        {
            Text_DetailTitle.gameObject.SetActive(true);
            Text_DetailTitle.text = codexData != null ? codexData.Title : string.IsNullOrWhiteSpace(Text_DetailTitle.text) ? "도감" : Text_DetailTitle.text;
            Text_DetailTitle.color = new Color(0.05f, 0.04f, 0.03f, 1f);
            Text_DetailTitle.alignment = TextAlignmentOptions.Center;
            Text_DetailTitle.raycastTarget = false;
            OOTechTMPFontUtility.ApplyProjectFont(Text_DetailTitle);
            Text_DetailTitle.transform.SetAsLastSibling();
        }

        if (Text_DetailCategory != null)
        {
            Text_DetailCategory.gameObject.SetActive(true);
            Text_DetailCategory.text = codexData != null ? ResolveDisplayCategory(codexData.Category) : string.IsNullOrWhiteSpace(Text_DetailCategory.text) ? ResolveCurrentPageName() : Text_DetailCategory.text;
            Text_DetailCategory.color = new Color(0.08f, 0.07f, 0.04f, 1f);
            Text_DetailCategory.alignment = TextAlignmentOptions.Center;
            Text_DetailCategory.raycastTarget = false;
            OOTechTMPFontUtility.ApplyProjectFont(Text_DetailCategory);
            Text_DetailCategory.transform.SetAsLastSibling();
        }

        if (Text_DetailDescription != null)
        {
            Text_DetailDescription.gameObject.SetActive(true);
            Text_DetailDescription.text = descriptionText ?? (codexData != null ? codexData.Description : string.IsNullOrWhiteSpace(Text_DetailDescription.text) ? "등록된 도감 데이터가 없습니다." : Text_DetailDescription.text);
            ApplyDetailDescriptionTextStyle(Text_DetailDescription);
        }

        if (Text_RuntimeDetailDescription != null)
        {
            Text_RuntimeDetailDescription.text = descriptionText ?? (codexData != null ? codexData.Description : Text_RuntimeDetailDescription.text);
            ApplyDetailDescriptionTextStyle(Text_RuntimeDetailDescription);
            Text_RuntimeDetailDescription.transform.SetAsLastSibling();
        }

        Canvas.ForceUpdateCanvases();
    }

    private TextMeshProUGUI ResolveText(TextMeshProUGUI currentText, params string[] nameArray)
    {
        if (currentText != null)
            return currentText;

        TextMeshProUGUI[] textArray = GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (string textName in nameArray)
        {
            foreach (TextMeshProUGUI text in textArray)
            {
                if (text != null && text.name == textName)
                    return text;
            }
        }

        return null;
    }

    private Image ResolveImage(Image currentImage, params string[] nameArray)
    {
        if (currentImage != null)
            return currentImage;

        Image[] imageArray = GetComponentsInChildren<Image>(true);

        foreach (string imageName in nameArray)
        {
            foreach (Image image in imageArray)
            {
                if (image != null && image.name == imageName)
                    return image;
            }
        }

        return null;
    }

    private RectTransform ResolveDetailPanel(RectTransform currentRect)
    {
        if (currentRect != null)
            return currentRect;

        if (Text_DetailDescription != null && Text_DetailDescription.transform.parent != null)
            return Text_DetailDescription.transform.parent as RectTransform;

        if (Text_DetailTitle != null && Text_DetailTitle.transform.parent != null)
            return Text_DetailTitle.transform.parent as RectTransform;

        return null;
    }

    /// <summary>
    /// 도감 책장 안쪽을 눌렀는지 확인합니다.
    /// 영화 비유로는 관객이 책장을 넘기는 중인지, 무대 바깥을 눌러 퇴장하려는지 구분하는 문지기입니다.
    /// </summary>
    private bool IsPointerInsideCodexContent(Vector2 screenPosition)
    {
        if (Scroll_CodexList != null && IsPointInsideRect(Scroll_CodexList.transform as RectTransform, screenPosition))
            return true;

        if (Rect_DetailPanel != null && IsPointInsideRect(Rect_DetailPanel, screenPosition))
            return true;

        if (Button_PreviousPage != null && IsPointInsideRect(Button_PreviousPage.transform as RectTransform, screenPosition))
            return true;

        if (Button_NextPage != null && IsPointInsideRect(Button_NextPage.transform as RectTransform, screenPosition))
            return true;

        if (Text_PageLabel != null && IsPointInsideRect(Text_PageLabel.transform as RectTransform, screenPosition))
            return true;

        return false;
    }

    private bool IsPointInsideRect(RectTransform targetRect, Vector2 screenPosition)
    {
        return targetRect != null && RectTransformUtility.RectangleContainsScreenPoint(targetRect, screenPosition, null);
    }

    /// <summary>
    /// 도감 바깥을 클릭했을 때 이전 그룹으로 복귀합니다.
    /// BackButton이 실패해도 UIManager와 NavigationHistory를 한 번 더 확인하는 보험입니다.
    /// </summary>
    public void RequestCloseCodexGroupByButton()
    {
        RequestCloseCodexGroup();
    }

    private void RequestCloseCodexGroup()
    {
        string previousGroupName = OOTechGroupNavigationHistory.GetPreviousGroup(gameObject.name, "MainMenuGroup");

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.CloseUI(gameObject.name);

            if (!string.IsNullOrEmpty(previousGroupName))
                OOTechUIManager.Inst.OpenUI(previousGroupName);

            return;
        }

        gameObject.SetActive(false);

        GameObject previousGroupObject = OOTechSceneQuery.RequestSceneObjectByName(previousGroupName);

        if (previousGroupObject != null)
            previousGroupObject.SetActive(true);
    }

    /// <summary>
    /// 도감의 목록/설명 창만 닫고, 배경과 돌아가기 버튼은 유지합니다.
    /// Game View에서는 도감 창 바깥을 눌렀을 때 플레이어가 갇히지 않고 배경을 볼 수 있습니다.
    /// </summary>
    private void RequestHideCodexContentOnly()
    {
        bool isAlreadyHidden = true;

        if (Scroll_CodexList != null && Scroll_CodexList.gameObject.activeSelf)
            isAlreadyHidden = false;

        if (Rect_DetailPanel != null && Rect_DetailPanel.gameObject.activeSelf)
            isAlreadyHidden = false;

        if (isAlreadyHidden)
            return;

        if (Scroll_CodexList != null)
            Scroll_CodexList.gameObject.SetActive(false);

        if (Rect_DetailPanel != null)
            Rect_DetailPanel.gameObject.SetActive(false);

        Debug.Log("[OOTechCodexGroupController] Codex content panels hidden by outside click.");
    }

    private void RequestSetCodexContentActive(bool isActive)
    {
        if (Scroll_CodexList != null)
            Scroll_CodexList.gameObject.SetActive(isActive);

        if (Rect_DetailPanel != null)
            Rect_DetailPanel.gameObject.SetActive(isActive);
    }

    /// <summary>
    /// 도감 페이지 이동 UI만 켜둡니다.
    /// Game View에서는 목록을 숨긴 뒤에도 페이지 버튼을 눌러 다시 내용을 열 수 있어야 합니다.
    /// </summary>
    private void RequestSetCodexPageNavigationActive(bool isActive)
    {
        if (Button_PreviousPage != null)
            Button_PreviousPage.gameObject.SetActive(isActive);

        if (Button_NextPage != null)
            Button_NextPage.gameObject.SetActive(isActive);

        if (Text_PageLabel != null)
            Text_PageLabel.gameObject.SetActive(isActive);
    }

    private Button ResolveButton(Button currentButton, params string[] nameArray)
    {
        if (currentButton != null)
            return currentButton;

        Button[] buttonArray = GetComponentsInChildren<Button>(true);

        foreach (string buttonName in nameArray)
        {
            foreach (Button button in buttonArray)
            {
                if (button != null && button.name == buttonName)
                    return button;
            }
        }

        return null;
    }

    private GameObject ResolveGameObject(GameObject currentObject, string objectName)
    {
        if (currentObject != null)
            return currentObject;

        Transform targetTransform = RequestChildObjectByName(transform, objectName);
        return targetTransform != null ? targetTransform.gameObject : null;
    }

    private Transform RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(objectName))
            return null;

        if (rootTransform.name == objectName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = RequestChildObjectByName(rootTransform.GetChild(index), objectName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
    }

    private Sprite ResolveSprite(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return null;

        string normalizedPath = NormalizeResourcesPath(imagePath);
        Sprite sprite = Resources.Load<Sprite>(normalizedPath);

        if (sprite != null)
            return sprite;

        Texture2D texture = Resources.Load<Texture2D>(normalizedPath);

        if (texture != null)
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));

#if UNITY_EDITOR
        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);

        if (sprite != null)
            return sprite;

        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath + ".png");

        if (sprite != null)
            return sprite;

        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath + ".jpg");

        if (sprite != null)
            return sprite;
#endif

        return null;
    }

    private string NormalizeResourcesPath(string rawPath)
    {
        string normalizedPath = (rawPath ?? string.Empty).Trim().Replace("\\", "/");

        if (normalizedPath.StartsWith("Assets/Resources/"))
            normalizedPath = normalizedPath.Substring("Assets/Resources/".Length);
        else if (normalizedPath.StartsWith("Resources/"))
            normalizedPath = normalizedPath.Substring("Resources/".Length);
        else if (normalizedPath.StartsWith("Assets/"))
            normalizedPath = normalizedPath.Substring("Assets/".Length);

        int extensionIndex = normalizedPath.LastIndexOf('.');

        if (extensionIndex >= 0)
            normalizedPath = normalizedPath.Substring(0, extensionIndex);

        return normalizedPath;
    }
}

