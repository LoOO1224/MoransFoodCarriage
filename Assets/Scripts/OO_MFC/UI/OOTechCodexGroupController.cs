// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechCodexGroupController.cs
// - ??븷: CodexGroup???꾧컧 紐⑸줉/?곸꽭 ?⑤꼸??愿由ы빀?덈떎.
// - ?곹솕 鍮꾩쑀: 洹뱀옣 濡쒕퉬???꾨줈洹몃옩遺??대떦?먯엯?덈떎. 吏湲덉? ?꾨줈洹몃옩遺곸씠 ?꾩꽦 ?꾩씠??//   愿媛앹뿉寃?"諛쒗몴 ???낅뜲?댄듃 ?덉젙" ?덈궡?먮쭔 蹂댁뿬二쇨퀬, ?대┃?섎㈃ ?덈궡?먮쭔 移섏썎?덈떎.
// - ?좎?蹂댁닔 ?ъ씤: ?ㅼ젣 ?꾧컧 怨듦컻 ??_isUseAnnouncementOnly瑜??꾨㈃ OO_Codex.json 紐⑸줉 紐⑤뱶濡??뚯븘媛묐땲??
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

    private readonly List<OOTechCodexEntrySlotView> _spawnedSlotViewList = new List<OOTechCodexEntrySlotView>();
    private GameObject Root_AnnouncementPanel;
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
            RequestCloseCodexGroup();
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
        RequestRefreshCodexList();
    }

    private void RequestNextPage()
    {
        _currentPageIndex = Mathf.Min(4, _currentPageIndex + 1);
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
            Text_PageLabel.text = $"{_currentPageIndex + 1}/5 {ResolveCurrentPageName()}";

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
                Description = string.IsNullOrEmpty(characterData.Description) ? "설명 준비 중입니다." : characterData.Description,
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
                Description = CreateCombinedDescription(cookData.Description, cookData.EffectDescription),
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
                Description = string.IsNullOrEmpty(ingredientData.Description) ? "설명 준비 중입니다." : ingredientData.Description,
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
                Description = narrationData.NarrationTexts != null && narrationData.NarrationTexts.Count > 0 ? string.Join("\n", narrationData.NarrationTexts) : "설명 준비 중입니다.",
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
                Description = CreateCombinedDescription(stageData.Description, stageData.QuestDescription),
                ImagePath = stageData.BackgroundImagePath
            });
        }
    }

    private string ResolveCurrentPageName()
    {
        if (_currentPageIndex == 0)
            return "캐릭터";

        if (_currentPageIndex == 1)
            return "요리";

        if (_currentPageIndex == 2)
            return "재료";

        if (_currentPageIndex == 3)
            return "나레이션";

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

        if (Image_DetailPreview != null)
        {
            Image_DetailPreview.sprite = null;
            Image_DetailPreview.enabled = false;
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
        if (Text_DetailTitle != null)
            Text_DetailTitle.text = codexData != null ? codexData.Title : "도감";

        if (Text_DetailCategory != null)
            Text_DetailCategory.text = codexData != null ? codexData.Category : string.Empty;

        if (Text_DetailDescription != null)
            Text_DetailDescription.text = codexData != null ? codexData.Description : "등록된 도감 데이터가 없습니다.";

        if (Image_DetailPreview != null)
        {
            Image_DetailPreview.sprite = codexData != null ? ResolveSprite(codexData.ImagePath) : null;
            Image_DetailPreview.enabled = Image_DetailPreview.sprite != null;
        }
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

