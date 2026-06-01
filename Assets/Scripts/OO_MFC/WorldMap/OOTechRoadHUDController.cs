using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds and controls the shared lower HUD for road and stage groups.
/// Think of this as the production desk under the stage: inventory, codex,
/// mission, cooking, and world map all live in the same fixed control row.
/// </summary>
[DisallowMultipleComponent]
public class OOTechRoadHUDController : MonoBehaviour
{
    [Header("Group Names")]
    [SerializeField] private string _ownerGroupName;
    [SerializeField] private string _worldMapGroupName = "WorldMapGroup";
    [SerializeField] private string _cookingGroupName = "CookingGroup";
    [SerializeField] private string _codexGroupName = "CodexGroup";

    [Header("Inventory")]
    [SerializeField] private bool _isPrepareDefaultInventoryItem = true;
    [SerializeField] private string _defaultInventoryItemId = "ingredient_rice";
    [SerializeField] private int _defaultInventoryItemCount = 1;

    [Header("Canvas")]
    [SerializeField] private int _sortingOrder = 1200;
    [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);

    private GameObject Root_HUD;
    private Canvas Canvas_HUD;
    private GameObject Root_BottomBar;
    private GameObject Root_InventoryPanel;
    private GameObject Root_MissionPanel;
    private TextMeshProUGUI Text_InventoryContent;
    private TextMeshProUGUI Text_InventoryNewBadge;
    private TextMeshProUGUI Text_MissionContent;
    private bool _isDefaultInventoryPrepared;

    public void SetOwnerGroupName(string ownerGroupName)
    {
        _ownerGroupName = ownerGroupName;
    }

    public void PrepareHUD()
    {
        if (string.IsNullOrEmpty(_ownerGroupName))
            _ownerGroupName = gameObject.name;

        PrepareDefaultInventoryItem();
        CreateHUDCanvasIfNeeded();
        CreateBottomBarIfNeeded();
        CreateInventoryPanelIfNeeded();
        CreateMissionPanelIfNeeded();
        RefreshInventoryView();
        SetInventoryPanelActive(false);
        SetMissionPanelActive(false);
        SetNewBadgeActive(true);
    }

    public void SetHUDVisible(bool isVisible)
    {
        if (Root_HUD != null)
            Root_HUD.SetActive(isVisible);
    }

    private void PrepareDefaultInventoryItem()
    {
        if (!_isPrepareDefaultInventoryItem || _isDefaultInventoryPrepared)
            return;

        if (OOTechGameManager.Inst == null)
            return;

        List<OOTechItemModel> itemList = OOTechGameManager.Inst.GetPlayerItemList();

        foreach (OOTechItemModel item in itemList)
        {
            if (item != null && item.ItemDataId == _defaultInventoryItemId)
            {
                _isDefaultInventoryPrepared = true;
                return;
            }
        }

        OOTechGameManager.Inst.AddItem(_defaultInventoryItemId, Mathf.Max(1, _defaultInventoryItemCount));
        _isDefaultInventoryPrepared = true;
    }

    private void CreateHUDCanvasIfNeeded()
    {
        if (Root_HUD != null)
            return;

        Root_HUD = new GameObject("RoadHUDCanvas", typeof(RectTransform));
        Root_HUD.transform.SetParent(transform, false);

        Canvas_HUD = Root_HUD.AddComponent<Canvas>();
        Canvas_HUD.renderMode = RenderMode.ScreenSpaceOverlay;
        Canvas_HUD.overrideSorting = true;
        Canvas_HUD.sortingOrder = _sortingOrder;

        CanvasScaler canvasScaler = Root_HUD.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = _referenceResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;

        Root_HUD.AddComponent<GraphicRaycaster>();
    }

    private void CreateBottomBarIfNeeded()
    {
        if (Root_BottomBar != null)
            return;

        Root_BottomBar = CreatePanel("Panel_RoadBottomHUD", Root_HUD.transform, new Color(0.08f, 0.08f, 0.08f, 0.86f));
        RectTransform barRect = Root_BottomBar.transform as RectTransform;
        barRect.anchorMin = new Vector2(0f, 0f);
        barRect.anchorMax = new Vector2(1f, 0f);
        barRect.pivot = new Vector2(0.5f, 0f);
        barRect.anchoredPosition = Vector2.zero;
        barRect.sizeDelta = new Vector2(0f, 150f);

        CreateButton("Button_InventoryUI", Root_BottomBar.transform, new Vector2(-520f, 75f), "인벤토리", OnInventoryButtonClicked);
        CreateButton("Button_CodexUI", Root_BottomBar.transform, new Vector2(-260f, 75f), "도감", OnCodexButtonClicked);
        CreateButton("Button_MissionUI", Root_BottomBar.transform, new Vector2(0f, 75f), "임무", OnMissionButtonClicked);
        CreateButton("Button_CookingUI", Root_BottomBar.transform, new Vector2(260f, 75f), "요리하기", OnCookingButtonClicked);
        CreateButton("Button_WorldMapUI", Root_BottomBar.transform, new Vector2(520f, 75f), "월드맵", OnWorldMapButtonClicked);

        Text_InventoryNewBadge = CreateText("Text_InventoryNewBadge", Root_BottomBar.transform, "NEW", 24, FontStyles.Bold, Color.yellow);
        RectTransform badgeRect = Text_InventoryNewBadge.transform as RectTransform;
        badgeRect.anchorMin = new Vector2(0.5f, 0f);
        badgeRect.anchorMax = new Vector2(0.5f, 0f);
        badgeRect.pivot = new Vector2(0.5f, 0.5f);
        badgeRect.anchoredPosition = new Vector2(-415f, 115f);
        badgeRect.sizeDelta = new Vector2(88f, 36f);
    }

    private void CreateInventoryPanelIfNeeded()
    {
        if (Root_InventoryPanel != null)
            return;

        Root_InventoryPanel = CreatePanel("Panel_InventoryUI", Root_HUD.transform, new Color(0.04f, 0.04f, 0.04f, 0.94f));
        RectTransform panelRect = Root_InventoryPanel.transform as RectTransform;
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 0f);
        panelRect.pivot = new Vector2(0f, 0f);
        panelRect.anchoredPosition = new Vector2(28f, 170f);
        panelRect.sizeDelta = new Vector2(460f, 330f);

        TextMeshProUGUI titleText = CreateText("Text_InventoryTitle", Root_InventoryPanel.transform, "인벤토리", 28, FontStyles.Bold, Color.white);
        RectTransform titleRect = titleText.transform as RectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -16f);
        titleRect.sizeDelta = new Vector2(-32f, 44f);

        GameObject scrollObject = CreateUIObject("Scroll_InventorySlots", Root_InventoryPanel.transform);
        RectTransform scrollRectTransform = scrollObject.transform as RectTransform;
        scrollRectTransform.anchorMin = new Vector2(0f, 0f);
        scrollRectTransform.anchorMax = new Vector2(1f, 1f);
        scrollRectTransform.offsetMin = new Vector2(18f, 18f);
        scrollRectTransform.offsetMax = new Vector2(-18f, -72f);

        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        GameObject viewportObject = CreatePanel("Viewport", scrollObject.transform, new Color(1f, 1f, 1f, 0.04f));
        RectTransform viewportRect = viewportObject.transform as RectTransform;
        StretchFull(viewportRect);
        viewportObject.AddComponent<Mask>().showMaskGraphic = false;

        GameObject contentObject = CreateUIObject("Content", viewportObject.transform);
        RectTransform contentRect = contentObject.transform as RectTransform;
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 260f);

        Text_InventoryContent = CreateText("Text_InventoryContent", contentObject.transform, string.Empty, 24, FontStyles.Normal, Color.white);
        RectTransform contentTextRect = Text_InventoryContent.transform as RectTransform;
        StretchFull(contentTextRect);
        Text_InventoryContent.alignment = TextAlignmentOptions.TopLeft;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
    }

    private void CreateMissionPanelIfNeeded()
    {
        if (Root_MissionPanel != null)
            return;

        Root_MissionPanel = CreatePanel("Panel_MissionUI", Root_HUD.transform, new Color(0.04f, 0.04f, 0.04f, 0.94f));
        RectTransform panelRect = Root_MissionPanel.transform as RectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 170f);
        panelRect.sizeDelta = new Vector2(520f, 220f);

        Text_MissionContent = CreateText("Text_MissionContent", Root_MissionPanel.transform, "현재 임무\n스테이지 입구까지 이동하세요.", 24, FontStyles.Normal, Color.white);
        RectTransform textRect = Text_MissionContent.transform as RectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 24f);
        textRect.offsetMax = new Vector2(-24f, -24f);
        Text_MissionContent.alignment = TextAlignmentOptions.MidlineLeft;
    }

    private void RefreshInventoryView()
    {
        if (Text_InventoryContent == null)
            return;

        List<OOTechItemModel> itemList = OOTechGameManager.Inst != null ? OOTechGameManager.Inst.GetPlayerItemList() : new List<OOTechItemModel>();

        if (itemList.Count == 0)
        {
            Text_InventoryContent.text = $"쌀 x{Mathf.Max(1, _defaultInventoryItemCount)}";
            return;
        }

        string inventoryText = string.Empty;

        foreach (OOTechItemModel item in itemList)
        {
            if (item == null)
                continue;

            string itemName = item.ItemDataId == _defaultInventoryItemId ? "쌀" : item.ItemDataId;
            inventoryText += $"{itemName} x{item.ItemStackCount}\n";
        }

        Text_InventoryContent.text = string.IsNullOrEmpty(inventoryText) ? "비어 있음" : inventoryText.TrimEnd();
    }

    private void OnInventoryButtonClicked()
    {
        RefreshInventoryView();
        SetInventoryPanelActive(Root_InventoryPanel != null && !Root_InventoryPanel.activeSelf);
        SetMissionPanelActive(false);
        SetNewBadgeActive(false);
    }

    private void OnCodexButtonClicked()
    {
        RequestOpenSceneGroup(_codexGroupName);
    }

    private void OnMissionButtonClicked()
    {
        SetMissionPanelActive(Root_MissionPanel != null && !Root_MissionPanel.activeSelf);
        SetInventoryPanelActive(false);
    }

    private void OnCookingButtonClicked()
    {
        RequestOpenSceneGroup(_cookingGroupName);
    }

    private void OnWorldMapButtonClicked()
    {
        RequestOpenSceneGroup(_worldMapGroupName);
    }

    private void SetInventoryPanelActive(bool isActive)
    {
        if (Root_InventoryPanel != null)
            Root_InventoryPanel.SetActive(isActive);
    }

    private void SetMissionPanelActive(bool isActive)
    {
        if (Root_MissionPanel != null)
            Root_MissionPanel.SetActive(isActive);
    }

    private void SetNewBadgeActive(bool isActive)
    {
        if (Text_InventoryNewBadge != null)
            Text_InventoryNewBadge.gameObject.SetActive(isActive);
    }

    private bool RequestOpenSceneGroup(string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
            return false;

        GameObject groupObject = FindSceneObjectByName(groupName);
        string previousGroupName = GetOwnerGroupName();
        OOTechGroupNavigationHistory.SetPreviousGroup(groupName, previousGroupName);

        if (OOTechUIManager.Inst != null && groupObject != null)
        {
            OOTechUIManager.Inst.RegisterUI(groupName, groupObject);

            if (OOTechUIManager.Inst.OpenUI(groupName))
            {
                PrepareOverlayReturnButton(groupObject, groupName, previousGroupName);
                return true;
            }
        }

        if (OOTechUIManager.Inst != null && OOTechUIManager.Inst.OpenUI(groupName))
        {
            PrepareOverlayReturnButton(OOTechUIManager.Inst.GetCreatedUI(groupName), groupName, previousGroupName);
            return true;
        }

        if (groupObject == null)
        {
            Debug.LogWarning($"[OOTechRoadHUDController] Scene group not found: {groupName}");
            return false;
        }

        groupObject.SetActive(true);
        PrepareOverlayReturnButton(groupObject, groupName, previousGroupName);
        return true;
    }

    private string GetOwnerGroupName()
    {
        if (!string.IsNullOrEmpty(_ownerGroupName))
            return _ownerGroupName;

        return gameObject.name;
    }

    private void PrepareOverlayReturnButton(GameObject groupObject, string currentGroupName, string previousGroupName)
    {
        if (groupObject == null)
            return;

        BackButtonController[] backButtonArray = groupObject.GetComponentsInChildren<BackButtonController>(true);

        foreach (BackButtonController backButton in backButtonArray)
            backButton.SetPreviousGroup(previousGroupName);

        if (FindChildByName(groupObject.transform, "Button_RuntimeReturn") != null)
            return;

        GameObject canvasObject = new GameObject("Canvas_RuntimeReturn", typeof(RectTransform));
        canvasObject.transform.SetParent(groupObject.transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = _sortingOrder + 10;

        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = _referenceResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        Button returnButton = CreateButton("Button_RuntimeReturn", canvasObject.transform, Vector2.zero, "돌아가기", delegate
        {
            if (OOTechUIManager.Inst == null)
                return;

            OOTechUIManager.Inst.CloseUI(currentGroupName);
            OOTechUIManager.Inst.OpenUI(previousGroupName);
        });

        RectTransform rectTransform = returnButton.transform as RectTransform;
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = new Vector2(-36f, -36f);
        rectTransform.sizeDelta = new Vector2(220f, 64f);
    }

    private Button CreateButton(string objectName, Transform parent, Vector2 anchoredPosition, string text, UnityAction clickAction)
    {
        GameObject buttonObject = CreatePanel(objectName, parent, new Color(0.22f, 0.22f, 0.22f, 0.96f));
        RectTransform rectTransform = buttonObject.transform as RectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(220f, 66f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        button.onClick.AddListener(clickAction);

        TextMeshProUGUI buttonText = CreateText("Text_Label", buttonObject.transform, text, 24, FontStyles.Bold, Color.white);
        StretchFull(buttonText.transform as RectTransform);
        return button;
    }

    private GameObject CreatePanel(string objectName, Transform parent, Color color)
    {
        GameObject panelObject = CreateUIObject(objectName, parent);
        Image image = panelObject.AddComponent<Image>();
        image.color = color;
        return panelObject;
    }

    private TextMeshProUGUI CreateText(string objectName, Transform parent, string text, int fontSize, FontStyles fontStyle, Color color)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = fontStyle;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = color;
        textComponent.raycastTarget = false;
        OOTechTMPFontUtility.ApplyProjectFont(textComponent);
        return textComponent;
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        uiObject.layer = parent != null ? parent.gameObject.layer : gameObject.layer;
        return uiObject;
    }

    private void StretchFull(RectTransform rectTransform)
    {
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    private GameObject FindSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return null;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = FindChildByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private GameObject FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = FindChildByName(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }
}
