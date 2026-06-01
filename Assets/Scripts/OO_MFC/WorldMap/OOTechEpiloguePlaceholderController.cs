using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Temporary epilogue screen.
/// It first offers ending credits, then returns the audience to the main menu.
/// </summary>
[DisallowMultipleComponent]
public class OOTechEpiloguePlaceholderController : MonoBehaviour
{
    [SerializeField] private string _currentGroupName = "EpilogueGroup";
    [SerializeField] private string _mainMenuGroupName = "MainMenuGroup";
    [SerializeField] private int _sortingOrder = 950;

    private GameObject Root_Canvas;
    private GameObject Root_Credits;
    private Button Button_Credits;
    private Button Button_ReturnMainMenu;

    private void OnEnable()
    {
        PrepareView();
        SetCreditsViewActive(false);
    }

    private void PrepareView()
    {
        if (Root_Canvas != null)
            return;

        Root_Canvas = new GameObject("Canvas_EpiloguePlaceholder", typeof(RectTransform));
        Root_Canvas.transform.SetParent(transform, false);

        Canvas canvas = Root_Canvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = _sortingOrder;

        CanvasScaler canvasScaler = Root_Canvas.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;

        Root_Canvas.AddComponent<GraphicRaycaster>();
        CreateWhiteBackground();
        CreateCreditsButton();
        CreateCreditsView();
    }

    private void CreateWhiteBackground()
    {
        GameObject backgroundObject = CreateUIObject("Image_WhiteBackground", Root_Canvas.transform);
        StretchFull(backgroundObject.transform as RectTransform);

        Image image = backgroundObject.AddComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = false;
    }

    private void CreateCreditsButton()
    {
        Button_Credits = CreateButton("Button_ShowEndingCredits", Root_Canvas.transform, "엔딩 크레딧 보기");
        Button_Credits.onClick.AddListener(OnCreditsButtonClicked);
    }

    private void CreateCreditsView()
    {
        Root_Credits = CreateUIObject("Panel_EndingCredits", Root_Canvas.transform);
        RectTransform creditsRect = Root_Credits.transform as RectTransform;
        StretchFull(creditsRect);

        Image image = Root_Credits.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.88f);

        TextMeshProUGUI creditsText = CreateText("Text_EndingCredits", Root_Credits.transform, "엔딩 크레딧", 46, FontStyles.Bold, Color.white);
        RectTransform creditsTextRect = creditsText.transform as RectTransform;
        creditsTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        creditsTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        creditsTextRect.pivot = new Vector2(0.5f, 0.5f);
        creditsTextRect.anchoredPosition = new Vector2(0f, 90f);
        creditsTextRect.sizeDelta = new Vector2(700f, 120f);

        Button_ReturnMainMenu = CreateButton("Button_ReturnMainMenu", Root_Credits.transform, "메인메뉴 돌아가기");
        Button_ReturnMainMenu.onClick.AddListener(OnReturnMainMenuButtonClicked);
    }

    private void OnCreditsButtonClicked()
    {
        SetCreditsViewActive(true);
    }

    private void OnReturnMainMenuButtonClicked()
    {
        GameObject currentGroupObject = FindSceneObjectByName(_currentGroupName);
        GameObject mainMenuGroupObject = FindSceneObjectByName(_mainMenuGroupName);

        if (OOTechUIManager.Inst != null)
        {
            if (currentGroupObject != null)
                OOTechUIManager.Inst.RegisterUI(_currentGroupName, currentGroupObject);

            if (mainMenuGroupObject != null)
                OOTechUIManager.Inst.RegisterUI(_mainMenuGroupName, mainMenuGroupObject);

            OOTechUIManager.Inst.CloseUI(_currentGroupName);

            if (OOTechUIManager.Inst.OpenUI(_mainMenuGroupName))
                return;
        }

        if (currentGroupObject != null)
            currentGroupObject.SetActive(false);

        if (mainMenuGroupObject != null)
            mainMenuGroupObject.SetActive(true);
    }

    private void SetCreditsViewActive(bool isActive)
    {
        if (Root_Credits != null)
            Root_Credits.SetActive(isActive);

        if (Button_Credits != null)
            Button_Credits.gameObject.SetActive(!isActive);
    }

    private Button CreateButton(string objectName, Transform parent, string text)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        RectTransform buttonRect = buttonObject.transform as RectTransform;
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.anchoredPosition = new Vector2(-36f, -36f);
        buttonRect.sizeDelta = new Vector2(320f, 76f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI label = CreateText("Text_Button", buttonObject.transform, text, 28, FontStyles.Bold, Color.white);
        StretchFull(label.transform as RectTransform);
        return button;
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
