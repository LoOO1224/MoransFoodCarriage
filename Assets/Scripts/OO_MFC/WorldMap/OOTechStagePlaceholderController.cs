using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Temporary stage screen used until each stage receives its full quest scene.
/// The controller is the stage manager: it shows a simple white set, keeps the
/// shared HUD alive, and opens the next road or epilogue group when cued.
/// </summary>
[DisallowMultipleComponent]
public class OOTechStagePlaceholderController : MonoBehaviour
{
    [Header("Group Transition")]
    [SerializeField] private string _currentGroupName;
    [SerializeField] private string _nextGroupName;
    [SerializeField] private string _buttonText = "넘어가기";

    [Header("View")]
    [SerializeField] private int _sortingOrder = 900;
    [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);

    private GameObject Root_Canvas;
    private Button Button_Next;
    private TextMeshProUGUI Text_Button;
    private OOTechRoadHUDController HUD_Shared;

    public void Configure(string currentGroupName, string nextGroupName, string buttonText)
    {
        _currentGroupName = currentGroupName;
        _nextGroupName = nextGroupName;

        if (!string.IsNullOrEmpty(buttonText))
            _buttonText = buttonText;

        if (Text_Button != null)
            Text_Button.text = _buttonText;
    }

    private void OnEnable()
    {
        NormalizeButtonTextIfNeeded();
        PrepareView();
        PrepareSharedHUD();
        BindButton();
    }

    private void OnDisable()
    {
        UnbindButton();

        if (HUD_Shared != null)
            HUD_Shared.SetHUDVisible(false);
    }

    private void PrepareView()
    {
        if (Root_Canvas != null)
            return;

        Root_Canvas = new GameObject("Canvas_StagePlaceholder", typeof(RectTransform));
        Root_Canvas.transform.SetParent(transform, false);

        Canvas canvas = Root_Canvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = _sortingOrder;

        CanvasScaler canvasScaler = Root_Canvas.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = _referenceResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;

        Root_Canvas.AddComponent<GraphicRaycaster>();
        CreateWhiteBackground();
        CreateNextButton();
    }

    private void NormalizeButtonTextIfNeeded()
    {
        if (string.IsNullOrWhiteSpace(_buttonText) || _buttonText.Contains("?"))
            _buttonText = "넘어가기";
    }

    private void CreateWhiteBackground()
    {
        GameObject backgroundObject = CreateUIObject("Image_WhiteBackground", Root_Canvas.transform);
        RectTransform backgroundRect = backgroundObject.transform as RectTransform;
        StretchFull(backgroundRect);

        Image image = backgroundObject.AddComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = false;
    }

    private void CreateNextButton()
    {
        GameObject buttonObject = CreateUIObject("Button_NextStage", Root_Canvas.transform);
        RectTransform buttonRect = buttonObject.transform as RectTransform;
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.anchoredPosition = new Vector2(-36f, -36f);
        buttonRect.sizeDelta = new Vector2(260f, 72f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);

        Button_Next = buttonObject.AddComponent<Button>();
        Button_Next.targetGraphic = image;

        Text_Button = CreateText("Text_Button", buttonObject.transform, _buttonText, 28, FontStyles.Bold, Color.white);
        StretchFull(Text_Button.transform as RectTransform);
    }

    private void BindButton()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(OnNextButtonClicked);
        Button_Next.onClick.AddListener(OnNextButtonClicked);
    }

    private void PrepareSharedHUD()
    {
        if (HUD_Shared == null)
            HUD_Shared = GetComponent<OOTechRoadHUDController>();

        if (HUD_Shared == null)
            HUD_Shared = gameObject.AddComponent<OOTechRoadHUDController>();

        HUD_Shared.SetOwnerGroupName(_currentGroupName);
        HUD_Shared.PrepareHUD();
        HUD_Shared.SetHUDVisible(true);
    }

    private void UnbindButton()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(OnNextButtonClicked);
    }

    private void OnNextButtonClicked()
    {
        RequestSwitchSceneGroup(_currentGroupName, _nextGroupName);
    }

    private bool RequestSwitchSceneGroup(string closingGroupName, string openingGroupName)
    {
        GameObject closingGroupObject = FindSceneObjectByName(closingGroupName);
        GameObject openingGroupObject = FindSceneObjectByName(openingGroupName);

        if (openingGroupObject == null)
            openingGroupObject = CreateRuntimeRoadGroupFromTemplate(openingGroupName);

        if (OOTechUIManager.Inst != null)
        {
            if (closingGroupObject != null)
                OOTechUIManager.Inst.RegisterUI(closingGroupName, closingGroupObject);

            if (openingGroupObject != null)
                OOTechUIManager.Inst.RegisterUI(openingGroupName, openingGroupObject);

            OOTechUIManager.Inst.CloseUI(closingGroupName);

            if (OOTechUIManager.Inst.OpenUI(openingGroupName))
                return true;
        }

        if (closingGroupObject != null)
            closingGroupObject.SetActive(false);

        if (openingGroupObject == null)
        {
            Debug.LogWarning($"[OOTechStagePlaceholderController] Next group not found: {openingGroupName}");
            return false;
        }

        openingGroupObject.SetActive(true);
        return true;
    }

    private GameObject CreateRuntimeRoadGroupFromTemplate(string roadGroupName)
    {
        string targetStageGroupName = GetTargetStageGroupName(roadGroupName);

        if (string.IsNullOrEmpty(targetStageGroupName))
            return null;

        GameObject templateRoadGroup = FindSceneObjectByName("1st_Road_to_Stage1");

        if (templateRoadGroup == null)
            return null;

        GameObject roadGroup = Instantiate(templateRoadGroup);
        roadGroup.name = roadGroupName;
        roadGroup.SetActive(false);

        OOTechRoadToStage1Controller roadController = roadGroup.GetComponent<OOTechRoadToStage1Controller>();

        if (roadController == null)
            roadController = roadGroup.AddComponent<OOTechRoadToStage1Controller>();

        roadController.ConfigureRoadFlow(roadGroupName, targetStageGroupName);
        return roadGroup;
    }

    private string GetTargetStageGroupName(string roadGroupName)
    {
        if (roadGroupName == "2nd_Road_to_Stage2")
            return "Stage2Group";

        if (roadGroupName == "3rd_Road_to_Stage3")
            return "Stage3Group";

        if (roadGroupName == "4th_Road_to_Stage4")
            return "Stage4Group";

        if (roadGroupName == "Final_Road_to_FinalStage")
            return "FinalStageGroup";

        return string.Empty;
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
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    private GameObject FindSceneObjectByName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

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
