// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechEndingCreditController.cs
// - ??븷: EndingCreditGroup?먯꽌 寃? 諛곌꼍, ?ㅽ겕濡??щ젅?? PCROOMS 濡쒓퀬 ?좊땲硫붿씠?? 硫붿씤 硫붾돱 蹂듦?瑜??대떦?⑸땲??
// - ?곹솕 鍮꾩쑀: ?곹솕媛 ?앸궃 ???щ젅???먮쭑怨??쒖옉??濡쒓퀬瑜??щ━???곸쁺 湲곗궗?낅땲??
// - ?좎?蹂댁닔 ?ъ씤?? ?щ젅??臾멸뎄??諛쒗몴 吏곸쟾 ?섏젙 媛?ν븯?꾨줉 ??怨녹뿉 紐⑥븘 ?먭퀬, 濡쒓퀬 ?ㅻ툕?앺듃??LOGO ?먯떇???곗꽑 ?ъ궗?⑺빀?덈떎.
// =============================================================================
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OOTechEndingCreditController : MonoBehaviour
{
    [Header("Credit")]
    [SerializeField] private float _scrollSpeed = 68f;
    [SerializeField] private float _logoPauseSeconds = 2.4f;
    [SerializeField] private string _mainMenuGroupName = "MainMenuGroup";

    [TextArea(12, 30)]
    [SerializeField] private string _creditText =
        "Game Development\nChris Wooyoung Cheon\n\n" +
        "Based Project\nUnityBasic_6 by DaniTech\n\n" +
        "Development Period\nMay 18 ~ June 9, 2026\n\n" +
        "AI Coding Assistance\nGemini, ChatGPT, Grok\n\n" +
        "Art Director\nChris W. Cheon\n" +
        "(All images created with Gemini, ChatGPT, https://www.autosprite.io/, https://www.genspark.ai)\n\n" +
        "Sound Director\nChris W. Cheon\n" +
        "(Sound from Mureka.ai)\n\n" +
        "Story\nChris W. Cheon\n\n" +
        "Special Thanks\n단단's 이타심\n송준호 조교님\nDaniel Cho\n\n" +
        "Thank you for playing!";

    private RectTransform Rect_CreditRoot;
    private TextMeshProUGUI Text_Credit;
    private RectTransform Rect_Logo;
    private Animator Animator_Logo;
    private Button Button_MainMenu;
    private bool _isLogoPlayed;
    private bool _isPausedForLogo;
    private bool _isFinished;

    /// <summary>
    /// ?붾뵫 ?щ젅?㏃씠 ?대┫ ???꾩슂??UI瑜??뺣━?섍퀬 泥섏쓬 ?꾩튂?먯꽌 ?ㅽ겕濡ㅼ쓣 ?쒖옉?⑸땲??
    /// </summary>
    private void OnEnable()
    {
        PrepareCreditView();
        ResetCreditState();
    }

    /// <summary>
    /// ?щ젅??猷⑦듃瑜??꾨줈 ?吏곸씠怨? 濡쒓퀬媛 以묒븰???ㅻ㈃ ?좉퉸 ?뺤? ???좊땲硫붿씠?섏쓣 ??踰??ъ깮?⑸땲??
    /// </summary>
    private void Update()
    {
        if (_isFinished || _isPausedForLogo || Rect_CreditRoot == null)
            return;

        Rect_CreditRoot.anchoredPosition += Vector2.up * _scrollSpeed * Time.deltaTime;

        if (!_isLogoPlayed && Rect_Logo != null && Mathf.Abs(GetLogoScreenYFromCenter()) < 80f)
        {
            StartCoroutine(PlayLogoPauseRoutine());
            return;
        }

        if (Rect_CreditRoot.anchoredPosition.y > 1750f)
            ShowMainMenuButton();
    }

    private IEnumerator PlayLogoPauseRoutine()
    {
        _isLogoPlayed = true;
        _isPausedForLogo = true;

        if (Animator_Logo != null)
        {
            Animator_Logo.enabled = true;
            Animator_Logo.Play("PCROOMS_", 0, 0f);
        }

        yield return new WaitForSeconds(_logoPauseSeconds);
        _isPausedForLogo = false;
    }

    private void PrepareCreditView()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            mainCamera.orthographic = true;
            mainCamera.transform.position = new Vector3(0f, 0f, mainCamera.transform.position.z);
            CameraFollowController followController = mainCamera.GetComponent<CameraFollowController>();

            if (followController != null)
                followController.enabled = false;
        }

        Canvas canvas = GetComponentInChildren<Canvas>(true);

        if (canvas == null)
        {
            Debug.LogError("[OOTechEndingCreditController] Canvas_EndingCredit is missing. Run final route scene repair before play.");
            enabled = false;
            return;
        }

        canvas.gameObject.SetActive(true);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 9000;

        Image backgroundImage = canvas.GetComponent<Image>();

        if (backgroundImage == null)
            backgroundImage = canvas.gameObject.AddComponent<Image>();

        backgroundImage.color = Color.black;

        Rect_CreditRoot = ResolveExistingRect(canvas.transform, "CreditRoot");
        Text_Credit = ResolveExistingText(Rect_CreditRoot, "Text_Credit", _creditText, 38f, new Vector2(1120f, 1120f));
        Rect_Logo = ResolveExistingLogo(Rect_CreditRoot);
        Button_MainMenu = ResolveExistingMainMenuButton(canvas.transform);

        if (Button_MainMenu != null)
            Button_MainMenu.gameObject.SetActive(false);
    }

    private void ResetCreditState()
    {
        _isLogoPlayed = false;
        _isPausedForLogo = false;
        _isFinished = false;

        if (Rect_CreditRoot != null)
            Rect_CreditRoot.anchoredPosition = new Vector2(0f, -760f);

        if (Text_Credit != null)
            Text_Credit.text = _creditText;
    }

    private RectTransform ResolveExistingRect(Transform parentTransform, string objectName)
    {
        Transform foundTransform = RequestChildObjectByName(parentTransform, objectName);

        if (foundTransform == null)
        {
            Debug.LogError($"[OOTechEndingCreditController] Missing RectTransform object: {objectName}");
            return null;
        }

        return foundTransform as RectTransform;
    }

    private TextMeshProUGUI ResolveExistingText(Transform parentTransform, string objectName, string text, float fontSize, Vector2 size)
    {
        if (parentTransform == null)
            return null;

        Transform foundTransform = RequestChildObjectByName(parentTransform, objectName);
        TextMeshProUGUI textComponent = foundTransform != null ? foundTransform.GetComponent<TextMeshProUGUI>() : null;

        if (textComponent == null)
        {
            Debug.LogError($"[OOTechEndingCreditController] Missing TextMeshProUGUI object: {objectName}");
            return null;
        }

        RectTransform rectTransform = textComponent.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = size;
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;
        OOTechTMPFontUtility.ApplyProjectFont(textComponent);
        return textComponent;
    }

    private RectTransform ResolveExistingLogo(Transform parentTransform)
    {
        if (parentTransform == null)
            return null;

        Transform logoTransform = RequestChildObjectByName(transform, "LOGO");

        if (logoTransform == null)
        {
            Debug.LogWarning("[OOTechEndingCreditController] LOGO object is missing. Credit scroll continues without logo.");
            return null;
        }

        logoTransform.SetParent(parentTransform, false);
        RectTransform logoRect = logoTransform as RectTransform;

        if (logoRect == null)
            return null;

        logoRect.anchorMin = new Vector2(0.5f, 0.5f);
        logoRect.anchorMax = new Vector2(0.5f, 0.5f);
        logoRect.anchoredPosition = new Vector2(0f, -230f);
        logoRect.sizeDelta = new Vector2(420f, 220f);

        Animator_Logo = logoTransform.GetComponent<Animator>();
        return logoRect;
    }

    private Button ResolveExistingMainMenuButton(Transform parentTransform)
    {
        Transform foundTransform = RequestChildObjectByName(parentTransform, "Button_ReturnMainMenu");
        Button button = foundTransform != null ? foundTransform.GetComponent<Button>() : null;

        if (button == null)
        {
            Debug.LogError("[OOTechEndingCreditController] Button_ReturnMainMenu is missing.");
            return null;
        }

        TextMeshProUGUI label = ResolveExistingText(button.transform, "Text_Label", "메인 메뉴로 돌아가기", 34f, button.GetComponent<RectTransform>().sizeDelta);

        if (label != null)
            label.color = Color.black;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ReturnToMainMenu);
        return button;
    }

    private void ShowMainMenuButton()
    {
        _isFinished = true;

        if (Button_MainMenu != null)
            Button_MainMenu.gameObject.SetActive(true);
    }

    private void ReturnToMainMenu()
    {
        if (OOTechUIManager.Inst == null)
            return;

        OOTechUIManager.Inst.CloseUI(gameObject.name);
        OOTechUIManager.Inst.OpenUI(_mainMenuGroupName);
    }

    private float GetLogoScreenYFromCenter()
    {
        if (Rect_Logo == null)
            return 9999f;

        Vector3[] cornerArray = new Vector3[4];
        Rect_Logo.GetWorldCorners(cornerArray);
        float centerY = (cornerArray[0].y + cornerArray[2].y) * 0.5f;
        return centerY - Screen.height * 0.5f;
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
}

