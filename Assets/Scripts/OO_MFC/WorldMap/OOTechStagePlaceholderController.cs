// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechStagePlaceholderController.cs
// - ??븷: 濡쒕뱶留? ?붾뱶留? ?ㅽ뀒?댁? ?꾪솚 ?먮쫫???대떦?섎뒗 ?λ㈃ Controller?낅땲??
// - 媛먮룆 愿?? 湲??꾩쓽 ?λ㈃ ?꾪솚 ?먯떆?몃? ?ㅺ퀬 ?덈뒗 臾대?媛먮룆?낅땲??
// - ?좎?蹂댁닔 ?ъ씤?? 諛곌꼍/踰꾪듉/罹먮┃??諛곗튂???ㅻ툕?앺듃? View媛 留↔퀬, ???ㅽ겕由쏀듃???쒖꽌 吏?섎쭔 留≪븘???⑸땲??
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 媛?StageGroup???몃? ?섏뒪?멸? ?ㅼ뼱?ㅺ린 ?꾧퉴吏 ?곕뒗 ?꾩떆 臾대? 而⑦듃濡ㅻ윭?낅땲??
/// ??諛곌꼍 ?명듃, 怨듭슜 HUD, ?ㅼ쓬 RoadGroup?쇰줈 ?섏뼱媛??踰꾪듉 ?먮? 愿由ы빀?덈떎.
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

    /// <summary>
    /// ?먮뵒??蹂댁닔 ?ㅽ겕由쏀듃媛 Stage 踰덊샇??留욎떠 ?꾩옱/?ㅼ쓬 洹몃９ ?대쫫???명똿?????ъ슜?⑸땲??
    /// </summary>
    public void Configure(string currentGroupName, string nextGroupName, string buttonText)
    {
        _currentGroupName = currentGroupName;
        _nextGroupName = nextGroupName;

        if (!string.IsNullOrEmpty(buttonText))
            _buttonText = buttonText;

        if (Text_Button != null)
            Text_Button.text = _buttonText;
    }

    /// <summary>
    /// StageGroup??耳쒖?硫??꾩떆 臾대? UI? HUD瑜?以鍮꾪빀?덈떎.
    /// </summary>
    private void OnEnable()
    {
        NormalizeButtonTextIfNeeded();
        PrepareStageBackground();
        PrepareView();
        PrepareSharedHUD();
        BindButton();
    }

    /// <summary>
    /// StageGroup??爰쇱쭏 ??踰꾪듉 ?대깽?몄? HUD ?쒖떆瑜??뺣━?⑸땲??
    /// </summary>
    private void OnDisable()
    {
        UnbindButton();

        if (HUD_Shared != null)
            HUD_Shared.SetHUDVisible(false);
    }

    /// <summary>
    /// ?ъ뿉 諛곗튂??Canvas_StagePlaceholder?먯꽌 踰꾪듉 ?뚰뭹??李얠븘 ?곌껐?⑸땲??
    /// </summary>
    private void PrepareView()
    {
        if (Root_Canvas == null)
            Root_Canvas = RequestChildObjectByName(transform, "Canvas_StagePlaceholder");

        if (Root_Canvas == null)
        {
            Debug.LogWarning($"[OOTechStagePlaceholderController] {gameObject.name} needs Canvas_StagePlaceholder as a child object.");
            return;
        }

        Canvas canvas = Root_Canvas.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = _sortingOrder;
        }

        GameObject buttonObject = RequestChildObjectByName(Root_Canvas.transform, "Button_NextStage");
        Button_Next = buttonObject != null ? buttonObject.GetComponent<Button>() : null;
        Text_Button = buttonObject != null ? buttonObject.GetComponentInChildren<TextMeshProUGUI>(true) : null;

        if (Text_Button != null)
            Text_Button.text = _buttonText;
    }

    /// <summary>
    /// StageGroup??耳쒖쭏 ???ㅼ젣 諛곌꼍留됱쓣 癒쇱? ?щ━怨? ?곗깋 ?꾩떆 諛곌꼍留됱? ?대┰?덈떎.
    /// ?곹솕 臾대?濡?蹂대㈃ Stage2Background???ㅼ젣 ?명듃?닿퀬 Image_WhiteBackground???꾩떆 由ы뿀??泥쒕쭑?낅땲??
    /// </summary>
    private void PrepareStageBackground()
    {
        GameObject stageBackgroundObject = RequestStageBackgroundObject();

        if (stageBackgroundObject == null)
            return;

        stageBackgroundObject.SetActive(true);
        NormalizeStageBackgroundView(stageBackgroundObject);
        DisablePlaceholderWhiteBackground();
        RequestFitCameraToStageBackground(stageBackgroundObject);

        Debug.Log($"[OOTechStagePlaceholderController] Stage background enabled: {gameObject.name} -> {stageBackgroundObject.name}");
    }

    /// <summary>
    /// ?꾩옱 StageGroup ?대쫫??留욌뒗 諛곌꼍 ?ㅻ툕?앺듃瑜?李얠뒿?덈떎.
    /// ?? Stage2Group 臾대??먯꽌??Stage2Background 諛곗슦瑜?李얠뒿?덈떎.
    /// </summary>
    private GameObject RequestStageBackgroundObject()
    {
        if (HasRenderableStageBackground(gameObject))
            return gameObject;

        string groupName = string.IsNullOrEmpty(_currentGroupName) ? gameObject.name : _currentGroupName;
        string expectedBackgroundName = groupName.Replace("Group", "Background");
        GameObject stageBackgroundObject = RequestChildObjectByName(transform, expectedBackgroundName);

        if (stageBackgroundObject != null)
            return stageBackgroundObject;

        string typoSafeBackgroundName = groupName.Replace("Group", "Backound");
        stageBackgroundObject = RequestChildObjectByName(transform, typoSafeBackgroundName);

        if (stageBackgroundObject != null)
            return stageBackgroundObject;

        return RequestFirstRealBackgroundChild(transform);
    }

    /// <summary>
    /// Stage3Group泥섎읆 洹몃９ 猷⑦듃 ?먯껜??諛곌꼍 Image瑜?遺숈씤 寃쎌슦???ㅼ젣 諛곌꼍 諛곗슦濡??몄젙?⑸땲??
    /// 媛먮룆??臾대? 踰??먯껜??洹몃┝??遺숈뿬???곹솴?대?濡? 蹂꾨룄 ?먯떇 ?뚰뭹???놁뼱??諛곌꼍?쇰줈 ?ъ슜?⑸땲??
    /// </summary>
    private bool HasRenderableStageBackground(GameObject targetObject)
    {
        if (targetObject == null)
            return false;

        Image backgroundImage = targetObject.GetComponent<Image>();

        if (backgroundImage != null && backgroundImage.sprite != null)
            return true;

        SpriteRenderer backgroundRenderer = targetObject.GetComponent<SpriteRenderer>();
        return backgroundRenderer != null && backgroundRenderer.sprite != null;
    }

    /// <summary>
    /// ?대쫫??議곌툑 ?щ씪??Background/Backound媛 遺숈? ?ㅼ젣 諛곌꼍 ?먯떇??李얠뒿?덈떎.
    /// Image_WhiteBackground???꾩떆留됱씠???ㅼ젣 諛곌꼍?쇰줈 痍④툒?섏? ?딆뒿?덈떎.
    /// </summary>
    private GameObject RequestFirstRealBackgroundChild(Transform rootTransform)
    {
        if (rootTransform == null)
            return null;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform childTransform = rootTransform.GetChild(index);

            if (childTransform == null)
                continue;

            string childName = childTransform.name;
            bool isBackgroundName = childName.Contains("Background") || childName.Contains("Backound");
            bool isPlaceholderWhiteBackground = childName == "Image_WhiteBackground";

            if (isBackgroundName && !isPlaceholderWhiteBackground)
                return childTransform.gameObject;

            GameObject foundObject = RequestFirstRealBackgroundChild(childTransform);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    /// <summary>
    /// Stage 諛곌꼍 UI媛 Game View ?꾩껜??留욌룄濡?Canvas? RectTransform???뺣━?⑸땲??
    /// 媛먮룆???붾㈃鍮꾨? 諛붽퓭??諛곌꼍留됱씠 1920x1080 湲곗??쇰줈 臾대? ?ㅻ? 苑?梨꾩슦寃??섎뒗 ?덉쟾?μ튂?낅땲??
    /// </summary>
    private void NormalizeStageBackgroundView(GameObject stageBackgroundObject)
    {
        SpriteRenderer backgroundRenderer = stageBackgroundObject.GetComponent<SpriteRenderer>();

        if (backgroundRenderer != null)
        {
            NormalizeStageSpriteBackgroundView(stageBackgroundObject, backgroundRenderer);
            return;
        }

        RectTransform rectTransform = stageBackgroundObject.GetComponent<RectTransform>();
        Image backgroundImage = stageBackgroundObject.GetComponent<Image>();

        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        Canvas canvas = stageBackgroundObject.GetComponent<Canvas>();

        if (canvas == null && backgroundImage != null)
            canvas = stageBackgroundObject.AddComponent<Canvas>();

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = _sortingOrder - 100;
        }

        CanvasScaler canvasScaler = stageBackgroundObject.GetComponent<CanvasScaler>();

        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = _referenceResolution;
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        if (backgroundImage != null)
            backgroundImage.raycastTarget = false;
    }

    /// <summary>
    /// SpriteRenderer濡?留뚮뱺 Stage 諛곌꼍??1920x1080 ?붾뱶 臾대???留욎땅?덈떎.
    /// ?곹솕濡?移섎㈃ UI 泥쒕쭑???꾨땲???ㅼ젣 諛곌꼍 ?명듃?쇱꽌, 臾대? 以묒븰???볤퀬 ?붾㈃ ?ш린留뚰겮 ?ㅼ썙???⑸땲??
    /// </summary>
    private void NormalizeStageSpriteBackgroundView(GameObject stageBackgroundObject, SpriteRenderer backgroundRenderer)
    {
        Canvas canvas = stageBackgroundObject.GetComponent<Canvas>();

        if (canvas != null)
            canvas.enabled = false;

        CanvasScaler canvasScaler = stageBackgroundObject.GetComponent<CanvasScaler>();

        if (canvasScaler != null)
            canvasScaler.enabled = false;

        Image backgroundImage = stageBackgroundObject.GetComponent<Image>();

        if (backgroundImage != null)
            backgroundImage.enabled = false;

        Transform backgroundTransform = stageBackgroundObject.transform;
        const float targetWidth = 1920f;
        const float targetHeight = 1080f;

        backgroundTransform.localPosition = new Vector3(targetWidth * 0.5f, targetHeight * 0.5f, 0f);
        backgroundTransform.localRotation = Quaternion.identity;

        if (backgroundRenderer.sprite != null)
        {
            Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;
            float scaleX = targetWidth / Mathf.Max(0.01f, spriteSize.x);
            float scaleY = targetHeight / Mathf.Max(0.01f, spriteSize.y);

            backgroundTransform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        backgroundRenderer.enabled = true;
        backgroundRenderer.sortingLayerName = "Background";
        backgroundRenderer.sortingOrder = -1;

        Color color = backgroundRenderer.color;

        if (color.a <= 0.01f)
        {
            color.a = 1f;
            backgroundRenderer.color = color;
        }
    }

    /// <summary>
    /// ?ㅼ젣 Stage 諛곌꼍???덉쓣 ?뚮뒗 ?곗깋 ?꾩떆 諛곌꼍??爰쇱꽌 諛곌꼍 ?대?吏瑜?媛由ъ? ?딄쾶 ?⑸땲??
    /// </summary>
    private void DisablePlaceholderWhiteBackground()
    {
        if (Root_Canvas == null)
            Root_Canvas = RequestChildObjectByName(transform, "Canvas_StagePlaceholder");

        if (Root_Canvas == null)
            return;

        GameObject whiteBackgroundObject = RequestChildObjectByName(Root_Canvas.transform, "Image_WhiteBackground");

        if (whiteBackgroundObject != null)
            whiteBackgroundObject.SetActive(false);
    }

    /// <summary>
    /// 踰꾪듉 ?띿뒪?멸? 源⑥쭊 ?곹깭?쇰㈃ 湲곕낯 ?쒓? ?띿뒪?몃줈 蹂듦뎄?⑸땲??
    /// </summary>
    private void NormalizeButtonTextIfNeeded()
    {
        if (string.IsNullOrWhiteSpace(_buttonText) || _buttonText.Contains("?"))
            _buttonText = "넘어가기";
    }

    /// <summary>
    /// ?섏뼱媛湲?踰꾪듉???ㅼ쓬 洹몃９ ?대룞 ?먯뿉 ?곌껐?⑸땲??
    /// </summary>
    private void BindButton()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(OnNextButtonClicked);
        Button_Next.onClick.AddListener(OnNextButtonClicked);
        Button_Next.gameObject.SetActive(true);
        Button_Next.interactable = true;
    }

    /// <summary>
    /// Stage holder媛 ?대━硫?諛곌꼍 ?꾩껜媛 諛붾줈 蹂댁씠?꾨줉 移대찓?쇰? ??대뱶?룹쑝濡?留욎땅?덈떎.
    /// ?꾩떆 臾대??쇰룄 愿媛앹뿉寃뚮뒗 寃? ?щ갚蹂대떎 ?꾩껜 諛곌꼍??癒쇱? 蹂댁뿬???⑸땲??
    /// </summary>
    private void RequestFitCameraToStageBackground(GameObject stageBackgroundObject)
    {
        if (stageBackgroundObject == null)
            return;

        SpriteRenderer backgroundRenderer = stageBackgroundObject.GetComponent<SpriteRenderer>();

        if (backgroundRenderer == null || backgroundRenderer.sprite == null)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        CameraFollowController cameraFollow = mainCamera.GetComponent<CameraFollowController>();

        if (cameraFollow != null)
            cameraFollow.enabled = false;

        mainCamera.orthographic = true;

        Bounds backgroundBounds = backgroundRenderer.bounds;
        float verticalSize = backgroundBounds.extents.y;
        float horizontalSize = backgroundBounds.extents.x / Mathf.Max(0.01f, mainCamera.aspect);

        mainCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);

        Vector3 cameraPosition = mainCamera.transform.position;
        cameraPosition.x = backgroundBounds.center.x;
        cameraPosition.y = backgroundBounds.center.y;
        mainCamera.transform.position = cameraPosition;
    }

    /// <summary>
    /// StageGroup?먯꽌???몃깽?좊━/?꾨Т ?뺤씤??媛?ν븯?꾨줉 怨듭슜 HUD瑜?耳?땲??
    /// </summary>
    private void PrepareSharedHUD()
    {
        if (HUD_Shared == null)
            HUD_Shared = GetComponent<OOTechRoadHUDController>();

        if (HUD_Shared == null)
        {
            Debug.LogWarning($"[OOTechStagePlaceholderController] {gameObject.name} needs OOTechRoadHUDController attached in the scene.");
            return;
        }

        HUD_Shared.SetOwnerGroupName(_currentGroupName);
        HUD_Shared.PrepareHUD();
        HUD_Shared.SetCookingUnlocked(true);
        HUD_Shared.SetHUDVisible(true);
    }

    private void UnbindButton()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(OnNextButtonClicked);
    }

    /// <summary>
    /// 踰꾪듉 ?대┃ ???꾩옱 StageGroup???リ퀬 ?ㅼ쓬 RoadGroup ?먮뒗 EpilogueGroup???쎈땲??
    /// </summary>
    private void OnNextButtonClicked()
    {
        RequestSwitchSceneGroup(_currentGroupName, _nextGroupName);
    }

    /// <summary>
    /// UIManager ?깅줉 ?곹깭瑜??곗꽑 ?ъ슜?섍퀬, ?ㅽ뙣?섎㈃ ???ㅻ툕?앺듃 ?쒖꽦?붾줈 洹몃９???꾪솚?⑸땲??
    /// </summary>
    private bool RequestSwitchSceneGroup(string closingGroupName, string openingGroupName)
    {
        GameObject closingGroupObject = RequestSceneObjectByName(closingGroupName);
        GameObject openingGroupObject = RequestSceneObjectByName(openingGroupName);

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

    private GameObject RequestSceneObjectByName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return null;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = RequestChildObjectByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private GameObject RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = RequestChildObjectByName(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }
}


