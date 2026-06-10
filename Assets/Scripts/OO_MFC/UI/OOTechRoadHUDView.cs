// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechRoadHUDView.cs
// - 역할: UI 오브젝트 참조, 표시 갱신, 버튼 입력 연결을 담당합니다.
// - 유지보수: 씬 Hierarchy 이름으로 런타임 참조를 복구하는 코드가 많아 오브젝트 이름 변경에 주의합니다.
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUDUIGroup ?덉뿉 諛곗튂??踰꾪듉, ?⑤꼸, NEW 諛곗?瑜???踰덉뿉 ?ㅺ퀬 ?덈뒗 View 而댄룷?뚰듃?낅땲??
/// 媛먮룆? ??View瑜??듯빐 ?ㅼ젣 ???뚰뭹??留뚯?怨? ?뺤쟻 HUD ?ㅻ툕?앺듃???ъ슜?먭? 吏곸젒 ?몄쭛?????덉뒿?덈떎.
/// </summary>
[DisallowMultipleComponent]
public class OOTechRoadHUDView : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private GameObject Root_BottomBar;
    [SerializeField] private GameObject Root_InventoryPanel;
    [SerializeField] private GameObject Root_MissionPanel;

    [Header("Buttons")]
    [SerializeField] private Button Button_MainMenu;
    [SerializeField] private Button Button_Inventory;
    [SerializeField] private Button Button_Codex;
    [SerializeField] private Button Button_Mission;
    [SerializeField] private Button Button_Cooking;
    [SerializeField] private Button Button_WorldMap;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI Text_MissionContent;
    [SerializeField] private TextMeshProUGUI Text_InventoryNewBadge;
    [SerializeField] private TextMeshProUGUI Text_CodexNewBadge;
    [SerializeField] private TextMeshProUGUI Text_MissionNewBadge;
    [SerializeField] private TextMeshProUGUI Text_WorldMapNewBadge;
    [SerializeField] private TextMeshProUGUI Text_CookingLabel;
    [SerializeField] private TextMeshProUGUI Text_InventoryQuantityGuide;

    [Header("Images")]
    [SerializeField] private Image Image_CookingButton;

    [Header("Inventory")]
    [SerializeField] private ScrollRect Scroll_InventorySlots;
    [SerializeField] private RectTransform Rect_InventoryContent;
    [SerializeField] private GameObject Slot_InventoryItemTemplate;

    [Header("Guide")]
    [SerializeField] private GameObject Root_GuideOverlay;
    [SerializeField] private RectTransform Rect_FocusArrow;
    [SerializeField] private RectTransform Rect_GuideTextPanel;
    [SerializeField] private TextMeshProUGUI Text_GuideTitle;
    [SerializeField] private TextMeshProUGUI Text_GuideBody;
    [SerializeField] private Button Button_GuideNext;

    [Header("Confirm Popup")]
    [SerializeField] private GameObject Root_MainMenuConfirmPopup;
    [SerializeField] private TextMeshProUGUI Text_MainMenuConfirmMessage;
    [SerializeField] private Button Button_MainMenuConfirmYes;
    [SerializeField] private Button Button_MainMenuConfirmNo;

    public GameObject BottomBar => Root_BottomBar;
    public GameObject InventoryPanel => Root_InventoryPanel;
    public GameObject MissionPanel => Root_MissionPanel;
    public Button MainMenuButton => Button_MainMenu;
    public Button InventoryButton => Button_Inventory;
    public Button CodexButton => Button_Codex;
    public Button MissionButton => Button_Mission;
    public Button CookingButton => Button_Cooking;
    public Button WorldMapButton => Button_WorldMap;
    public TextMeshProUGUI MissionContentText => Text_MissionContent;
    public TextMeshProUGUI InventoryNewBadgeText => Text_InventoryNewBadge;
    public TextMeshProUGUI CodexNewBadgeText => Text_CodexNewBadge;
    public TextMeshProUGUI MissionNewBadgeText => Text_MissionNewBadge;
    public TextMeshProUGUI WorldMapNewBadgeText => Text_WorldMapNewBadge;
    public TextMeshProUGUI CookingLabelText => Text_CookingLabel;
    public TextMeshProUGUI InventoryQuantityGuideText => Text_InventoryQuantityGuide;
    public Image CookingButtonImage => Image_CookingButton;
    public ScrollRect InventoryScrollRect => Scroll_InventorySlots;
    public RectTransform InventoryContentRect => Rect_InventoryContent;
    public GameObject InventorySlotTemplate => Slot_InventoryItemTemplate;
    public GameObject GuideOverlay => Root_GuideOverlay;
    public RectTransform FocusArrowRect => Rect_FocusArrow;
    public RectTransform GuideTextPanelRect => Rect_GuideTextPanel;
    public TextMeshProUGUI GuideTitleText => Text_GuideTitle;
    public TextMeshProUGUI GuideBodyText => Text_GuideBody;
    public Button GuideNextButton => Button_GuideNext;
    public GameObject MainMenuConfirmPopup => Root_MainMenuConfirmPopup;
    public TextMeshProUGUI MainMenuConfirmMessageText => Text_MainMenuConfirmMessage;
    public Button MainMenuConfirmYesButton => Button_MainMenuConfirmYes;
    public Button MainMenuConfirmNoButton => Button_MainMenuConfirmNo;

    /// <summary>
    /// ?몄뒪?숉꽣 李몄“媛 鍮꾩뼱 ?덉쓣 ???먯떇 ?대쫫?쇰줈 HUD ?뚰뭹???ㅼ떆 ?곌껐?⑸땲??
    /// Game View?먯꽌???몃깽?좊━, ?꾧컧, ?꾨Т, ?붾━?섍린, ?붾뱶留?踰꾪듉???ш린???≫옓?덈떎.
    /// </summary>
    public void ResolveReferences()
    {
        Root_BottomBar = ResolveGameObject(Root_BottomBar, "Panel_RoadBottomHUD");
        Root_InventoryPanel = ResolveGameObject(Root_InventoryPanel, "Panel_Inventory", "Panel_InventoryUI");
        Root_MissionPanel = ResolveGameObject(Root_MissionPanel, "Panel_Mission", "Panel_MissionUI");

        Button_MainMenu = ResolveButton(Button_MainMenu, "Button_MainMenu", "Button_MainMenuUI");
        Button_Inventory = ResolveButton(Button_Inventory, "Button_Inventory", "Button_InventoryUI");
        Button_Codex = ResolveButton(Button_Codex, "Button_Codex", "Button_CodexUI");
        Button_Mission = ResolveButton(Button_Mission, "Button_Mission", "Button_MissionUI");
        Button_Cooking = ResolveButton(Button_Cooking, "Button_Cooking", "Button_CookingUI");
        Button_WorldMap = ResolveButton(Button_WorldMap, "Button_WorldMap", "Button_WorldMapUI");

        Text_MissionContent = ResolveText(Text_MissionContent, "Text_MissionContent");
        Text_InventoryNewBadge = ResolveText(Text_InventoryNewBadge, "NewBadge_Inventory", "Text_InventoryNewBadge");
        Text_CodexNewBadge = ResolveText(Text_CodexNewBadge, "NewBadge_Codex", "Text_CodexNewBadge");
        Text_MissionNewBadge = ResolveText(Text_MissionNewBadge, "NewBadge_Mission", "Text_MissionNewBadge");
        Text_WorldMapNewBadge = ResolveText(Text_WorldMapNewBadge, "NewBadge_WorldMap", "Text_WorldMapNewBadge");
        Text_InventoryQuantityGuide = ResolveText(Text_InventoryQuantityGuide, "Text_InventoryQuantityGuide");

        if (Button_Cooking != null)
        {
            Image_CookingButton = Image_CookingButton != null ? Image_CookingButton : Button_Cooking.GetComponent<Image>();
            Text_CookingLabel = Text_CookingLabel != null ? Text_CookingLabel : Button_Cooking.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (Scroll_InventorySlots == null)
        {
            Transform scrollTransform = RequestChildObjectByName(transform, "Scroll_InventorySlots");
            Scroll_InventorySlots = scrollTransform != null ? scrollTransform.GetComponent<ScrollRect>() : null;
        }

        if (Rect_InventoryContent == null)
        {
            Transform contentTransform = RequestChildObjectByName(transform, "Content");
            Rect_InventoryContent = contentTransform as RectTransform;
        }

        Slot_InventoryItemTemplate = ResolveGameObject(Slot_InventoryItemTemplate, "Slot_InventoryItemTemplate");

        Root_GuideOverlay = ResolveGameObject(Root_GuideOverlay, "Panel_HUDFocusGuide");
        Rect_FocusArrow = ResolveRect(Rect_FocusArrow, "Text_FocusArrow");
        Rect_GuideTextPanel = ResolveRect(Rect_GuideTextPanel, "Panel_GuideText");
        Text_GuideTitle = ResolveText(Text_GuideTitle, "Text_GuideTitle");
        Text_GuideBody = ResolveText(Text_GuideBody, "Text_GuideBody");
        Button_GuideNext = ResolveButton(Button_GuideNext, "Button_GuideNext");

        Root_MainMenuConfirmPopup = ResolveGameObject(Root_MainMenuConfirmPopup, "Panel_MainMenuConfirm");
        Text_MainMenuConfirmMessage = ResolveText(Text_MainMenuConfirmMessage, "Text_Message");
        Button_MainMenuConfirmYes = ResolveButton(Button_MainMenuConfirmYes, "Button_Yes");
        Button_MainMenuConfirmNo = ResolveButton(Button_MainMenuConfirmNo, "Button_No");
    }

    private GameObject ResolveGameObject(GameObject currentObject, string objectName)
    {
        if (currentObject != null)
            return currentObject;

        Transform targetTransform = RequestChildObjectByName(transform, objectName);
        return targetTransform != null ? targetTransform.gameObject : null;
    }

    private GameObject ResolveGameObject(GameObject currentObject, string primaryName, string fallbackName)
    {
        GameObject targetObject = ResolveGameObject(currentObject, primaryName);
        return targetObject != null ? targetObject : ResolveGameObject(currentObject, fallbackName);
    }

    private Button ResolveButton(Button currentButton, string objectName)
    {
        if (currentButton != null)
            return currentButton;

        Transform targetTransform = RequestChildObjectByName(transform, objectName);
        return targetTransform != null ? targetTransform.GetComponent<Button>() : null;
    }

    private Button ResolveButton(Button currentButton, string primaryName, string fallbackName)
    {
        Button targetButton = ResolveButton(currentButton, primaryName);
        return targetButton != null ? targetButton : ResolveButton(currentButton, fallbackName);
    }

    private RectTransform ResolveRect(RectTransform currentRect, string objectName)
    {
        if (currentRect != null)
            return currentRect;

        Transform targetTransform = RequestChildObjectByName(transform, objectName);
        return targetTransform as RectTransform;
    }

    private TextMeshProUGUI ResolveText(TextMeshProUGUI currentText, string objectName)
    {
        if (currentText != null)
            return currentText;

        Transform targetTransform = RequestChildObjectByName(transform, objectName);
        return targetTransform != null ? targetTransform.GetComponent<TextMeshProUGUI>() : null;
    }

    private TextMeshProUGUI ResolveText(TextMeshProUGUI currentText, string primaryName, string fallbackName)
    {
        TextMeshProUGUI targetText = ResolveText(currentText, primaryName);
        return targetText != null ? targetText : ResolveText(currentText, fallbackName);
    }

    private Transform RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
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
}

