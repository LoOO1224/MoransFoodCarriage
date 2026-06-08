// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: MainMenuController.cs
// - ??븷: MainMenuGroup??踰꾪듉 ?낅젰??諛쏆븘 ?ㅼ쓬 臾대?瑜??щ뒗 ?쒖옉 ?붾㈃ Controller?낅땲??
// - ?곹솕 鍮꾩쑀: 泥??곸쁺愿 濡쒕퉬?먯꽌 愿媛앹씠 蹂명렪, ?꾧컧, 醫낅즺, 媛쒕컻 由ы뿀???λ㈃??怨좊Ⅴ???덈궡 ?곗뒪?ъ엯?덈떎.
// - ?좎?蹂댁닔 ?ъ씤?? ?ㅼ젣 ?붾㈃ ?꾪솚? UIManager???붿껌?섍퀬, ???대옒?ㅻ뒗 踰꾪듉 ?먮쭔 ?꾨떖?⑸땲??
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// MainMenuGroup??踰꾪듉 ?대깽?몃? ?대떦?⑸땲??
/// Game View?먯꽌???뚮젅?댁뼱媛 泥섏쓬 留뚮굹??硫붾돱 踰꾪듉怨?媛쒕컻?먯슜 鍮좊Ⅸ 吏꾩엯 踰꾪듉???쒓났?⑸땲??
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Codex BGM")]
    [SerializeField] private AudioClip _codexBGM;

    [Header("Developer Skip")]
    [SerializeField] private bool _isShowDeveloperSkipButtons = true;
    [SerializeField] private string _developerRoad1GroupName = "1st_Road_to_Stage1";
    [SerializeField] private string _developerStage1GroupName = "Stage1Group";
    [SerializeField] private string _developerRoad2GroupName = "2nd_Road_to_Stage2";
    [SerializeField] private bool _isShowDeveloperStage3Button = true;
    [SerializeField] private string _developerStage3GroupName = "Stage3Group";
    [SerializeField] private bool _isShowDeveloperStage4Button = true;
    [SerializeField] private string _developerStage4GroupName = "Stage4_1Group";
    [SerializeField] private string _developerStage4KoreanCakeItemId = "OO_KoreanCake_1";
    [SerializeField] private string _developerStage4CarrotItemId = "Ing_Carrot_01";
    [SerializeField] private int _developerStage4ItemCount = 10;
    [SerializeField] private Vector2 _developerButtonStartPosition = new Vector2(32f, -32f);
    [SerializeField] private Vector2 _developerButtonSize = new Vector2(280f, 54f);
    [SerializeField] private float _developerButtonSpacing = 12f;
    [SerializeField] private Vector2 _developerStage3ButtonOffset = new Vector2(-32f, -32f);
    [SerializeField] private Vector2 _developerStage3ButtonSize = new Vector2(240f, 54f);

    private RectTransform Root_DeveloperSkipPanel;
    private RectTransform Root_DeveloperStage3Panel;

    private readonly string[] _exclusiveGroupNameArray =
    {
        "MainMenuGroup",
        "CodexGroup",
        "Prologue1Group",
        "Prologue2Group",
        "Tutorial1Group",
        "Senario1Group",
        "WorldMapGroup",
        "1st_Road_to_Stage1",
        "2nd_Road_to_Stage2",
        "3rd_Road_to_Stage3",
        "4th_Road_to_Stage4",
        "Final_Road_to_FinalStage",
        "CookingGroup",
        "Stage1Group",
        "Stage2Group",
        "Stage3Group",
        "Stage4_1Group",
        "Stage4_2Group",
        "Stage4Group",
        "FinalStageGroup",
        "EpilogueGroup",
        "DialogueGroup",
        "TutorialGuideGroup"
    };

    /// <summary>
    /// 硫붾돱媛 以鍮꾨맆 ??媛쒕컻?먯슜 由ы뿀??踰꾪듉??媛숈씠 以鍮꾪빀?덈떎.
    /// 媛먮룆 鍮꾩쑀濡쒕뒗 蹂?怨듭뿰 踰꾪듉 ?놁뿉 ?뱀젙 ?λ㈃?쇰줈 諛붾줈 媛??由ы뿀????踰꾪듉??遺숈씠???④퀎?낅땲??
    /// </summary>
    private void Awake()
    {
        PrepareDeveloperSkipButtons();
        PrepareDeveloperStage3Button();
    }

    /// <summary>
    /// MainMenuGroup???ㅼ떆 耳쒖쭏 ??媛쒕컻??踰꾪듉 ?쒖떆 ?곹깭瑜??좎??⑸땲??
    /// </summary>
    private void OnEnable()
    {
        if (Root_DeveloperSkipPanel != null)
            Root_DeveloperSkipPanel.gameObject.SetActive(_isShowDeveloperSkipButtons);

        if (Root_DeveloperStage3Panel != null)
            Root_DeveloperStage3Panel.gameObject.SetActive(_isShowDeveloperStage3Button || _isShowDeveloperStage4Button);
    }

    // ==================== 湲곕낯 硫붾돱 踰꾪듉 ====================

    /// <summary>
    /// ?쒖옉 踰꾪듉???꾨Ⅴ硫?Prologue1Group?쇰줈 吏꾪뻾?⑸땲??
    /// </summary>
    public void OnStartButtonClicked()
    {
        UIManagerExtension.OnStartButtonClicked();
    }

    /// <summary>
    /// ?꾧컧 踰꾪듉???꾨Ⅴ硫?CodexGroup???닿퀬 ?꾧컧 BGM???덉쑝硫??ъ깮?⑸땲??
    /// </summary>
    public void OnCodexButtonClicked()
    {
        UIManagerExtension.OnCodexButtonClicked();

        if (OOTechSoundManager.Inst != null && _codexBGM != null)
            OOTechSoundManager.Inst.PlayBGM(_codexBGM);
    }

    /// <summary>
    /// 醫낅즺 踰꾪듉???꾨Ⅴ硫?寃뚯엫 醫낅즺 ?붿껌???꾨떖?⑸땲??
    /// </summary>
    public void OnExitButtonClicked()
    {
        UIManagerExtension.OnExitButtonClicked();
    }

    // ==================== 媛쒕컻?먯슜 諛붾줈媛湲?踰꾪듉 ====================

    /// <summary>
    /// 媛쒕컻 ?뚯뒪?몄슜 諛붾줈媛湲?踰꾪듉 3媛쒕? MainMenuGroup ?덉뿉 以鍮꾪빀?덈떎.
    /// Game View?먯꽌??鍮④컯, 二쇳솴, ?몃옉 踰꾪듉?쇰줈 濡쒕뱶1, ?ㅽ뀒?댁?1, 濡쒕뱶2??諛붾줈 ?ㅼ뼱媛묐땲??
    /// </summary>
    private void PrepareDeveloperSkipButtons()
    {
        if (!_isShowDeveloperSkipButtons)
            return;

        Canvas canvas = GetComponentInChildren<Canvas>(true);

        if (canvas == null)
            canvas = CreateDeveloperSkipCanvas();

        Root_DeveloperSkipPanel = RequestOrCreateDeveloperSkipRoot(canvas.transform);
        ClearDeveloperButtonListenerArray();

        CreateDeveloperSkipButton("Button_DevSkip_Road1", "DEV Road 1", new Color(0.85f, 0.06f, 0.06f, 0.92f), 0, delegate
        {
            RequestDeveloperSkipToGroup(_developerRoad1GroupName);
        });

        CreateDeveloperSkipButton("Button_DevSkip_Stage1", "DEV Stage 1", new Color(1f, 0.45f, 0.02f, 0.92f), 1, delegate
        {
            RequestDeveloperSkipToGroup(_developerStage1GroupName);
        });

        CreateDeveloperSkipButton("Button_DevSkip_Road2", "DEV Road 2", new Color(1f, 0.9f, 0.05f, 0.92f), 2, delegate
        {
            RequestDeveloperSkipToGroup(_developerRoad2GroupName);
        });

        Root_DeveloperSkipPanel.gameObject.SetActive(true);
    }

    private void PrepareDeveloperStage3Button()
    {
        if (!_isShowDeveloperStage3Button && !_isShowDeveloperStage4Button)
            return;

        Canvas canvas = GetComponentInChildren<Canvas>(true);

        if (canvas == null)
            canvas = CreateDeveloperSkipCanvas();

        Root_DeveloperStage3Panel = RequestOrCreateDeveloperStage3Root(canvas.transform);
        if (_isShowDeveloperStage3Button)
            CreateDeveloperTopRightButton("Button_DevSkip_Stage3", "DEV Stage 3", new Color(0.05f, 0.16f, 0.32f, 0.92f), 0, delegate
            {
                RequestDeveloperSkipToGroup(_developerStage3GroupName);
            });

        if (_isShowDeveloperStage4Button)
            CreateDeveloperTopRightButton("Button_DevSkip_Stage4", "DEV Stage 4", new Color(0.16f, 0.36f, 0.12f, 0.94f), _isShowDeveloperStage3Button ? 1 : 0, delegate
            {
                PrepareDeveloperStage4Inventory();
                RequestDeveloperSkipToGroup(_developerStage4GroupName);
            });

        Root_DeveloperStage3Panel.gameObject.SetActive(true);
    }

    private Canvas CreateDeveloperSkipCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas_MainMenuDeveloperSkip");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 20000;

        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private RectTransform RequestOrCreateDeveloperSkipRoot(Transform canvasTransform)
    {
        GameObject rootObjectInCanvas = OOTechSceneQuery.RequestChildObjectByName(canvasTransform, "Panel_DeveloperSkipButtons");
        Transform rootTransform = rootObjectInCanvas != null ? rootObjectInCanvas.transform : null;

        if (rootTransform != null)
            return rootTransform as RectTransform;

        GameObject rootObject = new GameObject("Panel_DeveloperSkipButtons");
        rootObject.transform.SetParent(canvasTransform, false);

        RectTransform rootRect = rootObject.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.anchoredPosition = _developerButtonStartPosition;
        rootRect.sizeDelta = new Vector2(_developerButtonSize.x, (_developerButtonSize.y + _developerButtonSpacing) * 3f);
        return rootRect;
    }

    private RectTransform RequestOrCreateDeveloperStage3Root(Transform canvasTransform)
    {
        GameObject rootObjectInCanvas = OOTechSceneQuery.RequestChildObjectByName(canvasTransform, "Panel_DeveloperStage3Button");
        Transform rootTransform = rootObjectInCanvas != null ? rootObjectInCanvas.transform : null;

        if (rootTransform != null)
        {
            RectTransform existingRootRect = rootTransform as RectTransform;
            ApplyDeveloperStageShortcutRootSize(existingRootRect);
            return rootTransform as RectTransform;
        }

        GameObject rootObject = new GameObject("Panel_DeveloperStage3Button");
        rootObject.transform.SetParent(canvasTransform, false);

        RectTransform rootRect = rootObject.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(1f, 1f);
        rootRect.anchorMax = new Vector2(1f, 1f);
        rootRect.pivot = new Vector2(1f, 1f);
        rootRect.anchoredPosition = _developerStage3ButtonOffset;
        ApplyDeveloperStageShortcutRootSize(rootRect);
        return rootRect;
    }

    private void ApplyDeveloperStageShortcutRootSize(RectTransform rootRect)
    {
        if (rootRect == null)
            return;

        int buttonCount = (_isShowDeveloperStage3Button ? 1 : 0) + (_isShowDeveloperStage4Button ? 1 : 0);
        rootRect.sizeDelta = new Vector2(_developerStage3ButtonSize.x, (_developerStage3ButtonSize.y + _developerButtonSpacing) * Mathf.Max(1, buttonCount) - _developerButtonSpacing);
    }

    private void ClearDeveloperButtonListenerArray()
    {
        if (Root_DeveloperSkipPanel == null)
            return;

        Button[] buttonArray = Root_DeveloperSkipPanel.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttonArray)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }
    }

    private void CreateDeveloperSkipButton(string objectName, string labelText, Color color, int index, UnityAction clickAction)
    {
        GameObject existingButtonObject = OOTechSceneQuery.RequestChildObjectByName(Root_DeveloperSkipPanel, objectName);
        Transform buttonTransform = existingButtonObject != null ? existingButtonObject.transform : null;
        GameObject buttonObject = buttonTransform != null ? buttonTransform.gameObject : new GameObject(objectName);
        buttonObject.transform.SetParent(Root_DeveloperSkipPanel, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();

        if (buttonRect == null)
            buttonRect = buttonObject.AddComponent<RectTransform>();

        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(0f, 1f);
        buttonRect.pivot = new Vector2(0f, 1f);
        buttonRect.anchoredPosition = new Vector2(0f, -index * (_developerButtonSize.y + _developerButtonSpacing));
        buttonRect.sizeDelta = _developerButtonSize;

        Image buttonImage = buttonObject.GetComponent<Image>();

        if (buttonImage == null)
            buttonImage = buttonObject.AddComponent<Image>();

        buttonImage.color = color;

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
            button = buttonObject.AddComponent<Button>();

        button.targetGraphic = buttonImage;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(clickAction);

        TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);

        if (label == null)
            label = CreateDeveloperButtonLabel(buttonObject.transform);

        label.text = labelText;
    }

    private void CreateDeveloperTopRightButton(string objectName, string labelText, Color color, int index, UnityAction clickAction)
    {
        GameObject existingButtonObject = OOTechSceneQuery.RequestChildObjectByName(Root_DeveloperStage3Panel, objectName);
        Transform buttonTransform = existingButtonObject != null ? existingButtonObject.transform : null;
        GameObject buttonObject = buttonTransform != null ? buttonTransform.gameObject : new GameObject(objectName);
        buttonObject.transform.SetParent(Root_DeveloperStage3Panel, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();

        if (buttonRect == null)
            buttonRect = buttonObject.AddComponent<RectTransform>();

        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(0.5f, 1f);
        buttonRect.anchoredPosition = new Vector2(0f, -index * (_developerStage3ButtonSize.y + _developerButtonSpacing));
        buttonRect.sizeDelta = new Vector2(0f, _developerStage3ButtonSize.y);

        Image buttonImage = buttonObject.GetComponent<Image>();

        if (buttonImage == null)
            buttonImage = buttonObject.AddComponent<Image>();

        buttonImage.color = color;

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
            button = buttonObject.AddComponent<Button>();

        button.targetGraphic = buttonImage;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(clickAction);

        TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);

        if (label == null)
            label = CreateDeveloperButtonLabel(buttonObject.transform);

        label.text = labelText;
        label.color = Color.white;
    }

    private void PrepareDeveloperStage4Inventory()
    {
        EnsureDeveloperInventoryCount(_developerStage4KoreanCakeItemId, _developerStage4ItemCount);
        EnsureDeveloperInventoryCount(_developerStage4CarrotItemId, _developerStage4ItemCount);
    }

    private void EnsureDeveloperInventoryCount(string itemDataId, int targetCount)
    {
        if (OOTechGameManager.Inst == null || string.IsNullOrEmpty(itemDataId))
            return;

        int currentCount = OOTechGameManager.Inst.GetItemCount(itemDataId);
        int addCount = Mathf.Max(0, targetCount - currentCount);

        if (addCount > 0)
            OOTechGameManager.Inst.AddItem(itemDataId, addCount);
    }

    private TextMeshProUGUI CreateDeveloperButtonLabel(Transform parentTransform)
    {
        GameObject labelObject = new GameObject("Text_Label");
        labelObject.transform.SetParent(parentTransform, false);

        RectTransform labelRect = labelObject.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 26f;
        label.color = Color.black;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    /// <summary>
    /// 媛쒕컻 由ы뿀??踰꾪듉???뚮━硫??꾩옱 洹몃９?ㅼ쓣 ?リ퀬 紐⑺몴 洹몃９留?耳?땲??
    /// MainMenuGroup?먯꽌 Prologue瑜?嫄대꼫?곌퀬 諛붾줈 ?뱀젙 臾대?濡?吏꾩엯?????ъ슜?⑸땲??
    /// </summary>
    private void RequestDeveloperSkipToGroup(string targetGroupName)
    {
        if (OOTechUIManager.Inst == null)
        {
            Debug.LogError("[MainMenuController] Developer skip failed. OOTechUIManager is missing.");
            return;
        }

        foreach (string groupName in _exclusiveGroupNameArray)
        {
            if (groupName == targetGroupName)
                continue;

            OOTechUIManager.Inst.CloseUI(groupName);
        }

        OOTechUIManager.Inst.OpenUI(targetGroupName);
        Debug.Log($"[MainMenuController] Developer skip opened: {targetGroupName}");
    }
}

