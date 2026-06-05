// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCodexGroupController.cs
// - 역할: CodexGroup의 도감 목록/상세 패널을 관리합니다.
// - 영화 비유: 극장 로비의 프로그램북 담당자입니다. 지금은 프로그램북이 완성 전이라
//   관객에게 "발표 전 업데이트 예정" 안내판만 보여주고, 클릭하면 안내판만 치웁니다.
// - 유지보수 사인: 실제 도감 공개 시 _isUseAnnouncementOnly를 끄면 OO_Codex.json 목록 모드로 돌아갑니다.
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
/// CodexGroup의 표시 방식을 관리합니다.
/// 현재 발표 버전에서는 안내문만 보여주며, 창 아무 곳이나 클릭하면 안내창만 닫습니다.
/// CodexGroup의 원본 배경과 돌아가기 버튼은 유지해서, 돌아가기는 BackButtonController가 담당합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechCodexGroupController : MonoBehaviour, IPointerClickHandler
{
    [Header("List")]
    [SerializeField] private ScrollRect Scroll_CodexList;
    [SerializeField] private RectTransform Rect_CodexContent;
    [SerializeField] private OOTechCodexEntrySlotView View_SlotTemplate;

    [Header("Detail")]
    [SerializeField] private TextMeshProUGUI Text_DetailTitle;
    [SerializeField] private TextMeshProUGUI Text_DetailCategory;
    [SerializeField] private TextMeshProUGUI Text_DetailDescription;
    [SerializeField] private Image Image_DetailPreview;

    [Header("Announcement")]
    [SerializeField] private bool _isUseAnnouncementOnly = true;
    [SerializeField] private bool _isCloseAnnouncementByAnyClick = true;
    [SerializeField] private string _announcementTitle = "도감";
    [SerializeField] private string _announcementCategory = "발표 전 업데이트 예정";
    [TextArea(3, 8)]
    [SerializeField] private string _announcementDescription = "도감은 발표 전 업데이트 예정입니다.\n캐릭터, 음식, 재료, 지역 정보는 데이터가 확정된 뒤 OO_Codex.xlsx로 정리해 업데이트합니다.";

    private readonly List<OOTechCodexEntrySlotView> _spawnedSlotViewList = new List<OOTechCodexEntrySlotView>();
    private GameObject Root_AnnouncementPanel;
    private int _openedFrame;
    private bool _isAnnouncementClosed;

    private void OnEnable()
    {
        _openedFrame = Time.frameCount;
        _isAnnouncementClosed = false;
        ResolveReferences();

        if (_isUseAnnouncementOnly)
        {
            RequestShowAnnouncementOnly();
            return;
        }

        RequestRefreshCodexList();
    }

    private void Update()
    {
        if (!CanCloseAnnouncementByClick())
            return;

        if (Time.frameCount <= _openedFrame + 1)
            return;

        if (Input.GetMouseButtonDown(0))
            RequestCloseAnnouncementPanel();
    }

    /// <summary>
    /// 발표 전 안내 모드에서는 도감 창 아무 곳이나 클릭해도 안내창만 닫습니다.
    /// Game View에서는 배경 책장이 남고, 플레이어는 돌아가기 버튼으로 이전 무대에 복귀합니다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanCloseAnnouncementByClick())
            return;

        RequestCloseAnnouncementPanel();
    }

    /// <summary>
    /// 하이어라키에 미리 배치된 Codex UI 배우들을 이름으로 찾아 연결합니다.
    /// 감독이 매번 소품을 새로 만들지 않고, 무대 위에 놓인 소품에 역할표를 붙이는 단계입니다.
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
        Image_DetailPreview = ResolveImage(Image_DetailPreview, "Image_DetailPreview", "Image_Preview");
        Root_AnnouncementPanel = ResolveGameObject(Root_AnnouncementPanel, "Panel_CodexRoot");

        OOTechTMPFontUtility.ApplyProjectFont(Text_DetailTitle);
        OOTechTMPFontUtility.ApplyProjectFont(Text_DetailCategory);
        OOTechTMPFontUtility.ApplyProjectFont(Text_DetailDescription);
    }

    /// <summary>
    /// OO_Codex.json의 도감 목록을 스크롤 리스트에 다시 채웁니다.
    /// 발표 이후 실제 도감이 열릴 때 사용하는 목록 모드입니다.
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

        List<OO_Codex> codexDataList = OOTechGameDataManager.Inst != null
            ? OOTechGameDataManager.Inst.GetCodexDataList()
            : new List<OO_Codex>();

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

    /// <summary>
    /// 발표 전 도감은 목록을 열지 않고 안내문만 보여줍니다.
    /// Game View에서는 쌀 하나짜리 임시 목록 대신 "발표 전 업데이트 예정" 문구가 고정됩니다.
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
    /// 선택한 도감 항목의 제목, 분류, 설명, 이미지를 상세 패널에 보여줍니다.
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

    private GameObject ResolveGameObject(GameObject currentObject, string objectName)
    {
        if (currentObject != null)
            return currentObject;

        Transform targetTransform = FindChildByName(transform, objectName);
        return targetTransform != null ? targetTransform.gameObject : null;
    }

    private Transform FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(objectName))
            return null;

        if (rootTransform.name == objectName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = FindChildByName(rootTransform.GetChild(index), objectName);

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
