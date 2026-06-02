using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUDUIGroup 안에 배치된 버튼, 패널, NEW 배지를 한 번에 들고 있는 View 컴포넌트입니다.
/// 감독은 이 View를 통해 실제 씬 소품을 만지고, 정적 HUD 오브젝트는 사용자가 직접 편집할 수 있습니다.
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
    [SerializeField] private TextMeshProUGUI Text_CookingLabel;

    [Header("Images")]
    [SerializeField] private Image Image_CookingButton;

    [Header("Inventory")]
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
    public TextMeshProUGUI CookingLabelText => Text_CookingLabel;
    public Image CookingButtonImage => Image_CookingButton;
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
    /// 인스펙터 참조가 비어 있을 때 자식 이름으로 HUD 소품을 다시 연결합니다.
    /// Game View에서는 인벤토리, 도감, 임무, 요리하기, 월드맵 버튼이 여기서 잡힙니다.
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

        if (Button_Cooking != null)
        {
            Image_CookingButton = Image_CookingButton != null ? Image_CookingButton : Button_Cooking.GetComponent<Image>();
            Text_CookingLabel = Text_CookingLabel != null ? Text_CookingLabel : Button_Cooking.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (Rect_InventoryContent == null)
        {
            Transform contentTransform = FindChildByName(transform, "Content");
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

        Transform targetTransform = FindChildByName(transform, objectName);
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

        Transform targetTransform = FindChildByName(transform, objectName);
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

        Transform targetTransform = FindChildByName(transform, objectName);
        return targetTransform as RectTransform;
    }

    private TextMeshProUGUI ResolveText(TextMeshProUGUI currentText, string objectName)
    {
        if (currentText != null)
            return currentText;

        Transform targetTransform = FindChildByName(transform, objectName);
        return targetTransform != null ? targetTransform.GetComponent<TextMeshProUGUI>() : null;
    }

    private TextMeshProUGUI ResolveText(TextMeshProUGUI currentText, string primaryName, string fallbackName)
    {
        TextMeshProUGUI targetText = ResolveText(currentText, primaryName);
        return targetText != null ? targetText : ResolveText(currentText, fallbackName);
    }

    private Transform FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
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
}
