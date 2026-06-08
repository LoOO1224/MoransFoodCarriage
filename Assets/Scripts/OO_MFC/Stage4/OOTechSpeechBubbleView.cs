// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechSpeechBubbleView.cs
// - 역할: Rabbit 머리 위 말풍선 안의 텍스트를 OO_SpeechBubble 데이터로 타이핑 출력합니다.
// - 영화 비유: 배우 머리 위에 붙은 작은 자막 담당 스태프입니다. 감독은 대본 ID만 넘기고,
//   실제 글자 타이밍과 반복 표시는 이 컴포넌트가 처리합니다.
// - 유지보수 포인트: 말풍선 문장은 코드에 박지 말고 OO_SpeechBubble.xlsx/JSON에서 관리합니다.
// =============================================================================
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OOTechSpeechBubbleView : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private TextMeshProUGUI Text_Body;
    [SerializeField] private Canvas Canvas_Bubble;
    [SerializeField] private ScrollRect Scroll_Body;
    [SerializeField] private TextMeshProUGUI Text_ForcedBody;
    [SerializeField] private TextMeshProUGUI Text_OverlayBody;

    [Header("Typing")]
    [SerializeField] private float _baseCharacterInterval = 0.018f;
    [SerializeField] private bool _isHiddenOnAwake = true;
    [SerializeField] private Vector2 _overlayScreenOffset = new Vector2(0f, 8f);
    [SerializeField] private Vector2 _overlayTextSize = new Vector2(260f, 96f);

    private Coroutine Coroutine_Type;
    private bool _isLooping;
    private RectTransform Rect_OverlayCanvas;

    /// <summary>
    /// 말풍선이 켜질 때 필요한 텍스트와 Canvas를 찾아 둡니다.
    /// Game View에서는 Rabbit이 움직여도 이 오브젝트가 자식으로 따라가며 텍스트만 보이게 됩니다.
    /// </summary>
    private void Awake()
    {
        ResolveReferences();
        PrepareTransparentScrollView();

        if (_isHiddenOnAwake)
            RequestPrepareHidden();
    }

    private void LateUpdate()
    {
        UpdateOverlayTextPosition();
    }

    /// <summary>
    /// 지정한 말풍선 ID를 데이터에서 읽어 타이핑합니다.
    /// 반복 말풍선이면 끝난 뒤 같은 문장을 다시 시작합니다.
    /// </summary>
    public void RequestPlaySpeechBubble(string speechBubbleId, float speedMultiplier = 1f)
    {
        ResolveReferences();

        OO_SpeechBubble speechBubbleData = OOTechGameDataManager.Inst != null
            ? OOTechGameDataManager.Inst.GetSpeechBubbleData(speechBubbleId)
            : null;

        bool isRabbitSpeechBubble = !string.IsNullOrEmpty(speechBubbleId) && speechBubbleId.StartsWith("character_Rabbit_");
        string bodyText = isRabbitSpeechBubble
            ? ResolveHardcodedSpeechBubbleText(speechBubbleId)
            : speechBubbleData != null && IsReadableSpeechBubbleText(speechBubbleData.Text)
                ? speechBubbleData.Text
                : ResolveHardcodedSpeechBubbleText(speechBubbleId);
        bool isLoop = speechBubbleId == "character_Rabbit_06" || (speechBubbleData != null && speechBubbleData.IsLoop);
        float dataSpeed = speechBubbleData != null && speechBubbleData.TypingSpeed > 0f
            ? speechBubbleData.TypingSpeed
            : 1f;

        RequestPlayText(bodyText, dataSpeed * Mathf.Max(0.01f, speedMultiplier), isLoop);
    }

    /// <summary>
    /// 외부에서 직접 문장을 넘길 때 사용하는 fallback API입니다.
    /// 데이터가 아직 없을 때도 리허설이 막히지 않게 합니다.
    /// </summary>
    public void RequestPlayText(string bodyText, float speedMultiplier = 1f, bool isLoop = false)
    {
        ResolveReferences();

        if (Text_Body == null)
            return;

        gameObject.SetActive(true);
        RequestActivateViewObjects();
        PrepareTransparentScrollView();

        if (Coroutine_Type != null)
            StopCoroutine(Coroutine_Type);

        _isLooping = isLoop;
        SetVisibleSpeechText(bodyText);
        Coroutine_Type = StartCoroutine(PlayTypingRoutine(bodyText, speedMultiplier));
    }

    private string ResolveHardcodedSpeechBubbleText(string speechBubbleId)
    {
        if (speechBubbleId == "character_Rabbit_01")
            return "\uC5B4\uB77C? \uC800\uAC74... \uB2F9\uADFC\uC804?";

        if (speechBubbleId == "character_Rabbit_02")
            return "\uD765, \uC5B4\uB514 \uB9DB\uB9CC \uBCF4\uACA0\uC5B4!";

        if (speechBubbleId == "character_Rabbit_03")
            return "\uBC14\uC0AD\uD558\uACE0 \uACE0\uC18C\uD558\uC796\uC544!";

        if (speechBubbleId == "character_Rabbit_04")
            return "\uC774\uB7F0 \uB9DB\uC774\uBA74 \uC7A0\uAE50 \uC26C\uC5B4\uB3C4 \uB418\uACA0\uC5B4...";

        if (speechBubbleId == "character_Rabbit_05")
            return "\uC544... \uBC30\uBD80\uB974\uB2E4...";

        if (speechBubbleId == "character_Rabbit_06")
            return "\uCFE8... \uCFE8...";

        switch (speechBubbleId)
        {
            case "character_Rabbit_01":
                return "어라? 저건... 당근전?";
            case "character_Rabbit_02":
                return "흥, 어디 맛만 보겠어!";
            case "character_Rabbit_03":
                return "바삭하고 고소하잖아!";
            case "character_Rabbit_04":
                return "이런 맛이면 잠깐 쉬어도 되겠어...";
            case "character_Rabbit_05":
                return "아... 배부르다...";
            case "character_Rabbit_06":
                return "쿨... 쿨...";
            default:
                return string.IsNullOrEmpty(speechBubbleId) ? string.Empty : speechBubbleId;
        }
    }

    private bool IsReadableSpeechBubbleText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string trimmedValue = value.Trim();

        if (trimmedValue.Contains("???"))
            return false;

        return true;
    }

    private void RequestActivateViewObjects()
    {
        if (Canvas_Bubble != null)
        {
            Canvas_Bubble.gameObject.SetActive(true);
            Canvas_Bubble.enabled = true;
        }

        if (Scroll_Body != null)
            Scroll_Body.gameObject.SetActive(true);

        if (Text_Body != null)
        {
            Text_Body.gameObject.SetActive(true);
            Text_Body.enabled = true;
        }

        if (Text_ForcedBody != null)
        {
            Text_ForcedBody.gameObject.SetActive(true);
            Text_ForcedBody.enabled = true;
            Text_ForcedBody.transform.SetAsLastSibling();
        }

        if (Text_OverlayBody != null)
        {
            Text_OverlayBody.gameObject.SetActive(true);
            Text_OverlayBody.enabled = true;
            Text_OverlayBody.transform.SetAsLastSibling();
        }
    }

    /// <summary>
    /// 말풍선 표시를 닫고 현재 타이핑을 정지합니다.
    /// </summary>
    public void RequestHide()
    {
        RequestPrepareHidden();
    }

    public void RequestPrepareHidden()
    {
        ResolveReferences();

        if (Coroutine_Type != null)
        {
            StopCoroutine(Coroutine_Type);
            Coroutine_Type = null;
        }

        _isLooping = false;

        if (Text_Body != null)
            Text_Body.text = string.Empty;

        if (Text_ForcedBody != null)
            Text_ForcedBody.text = string.Empty;

        if (Text_OverlayBody != null)
            Text_OverlayBody.text = string.Empty;

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (Coroutine_Type != null)
        {
            StopCoroutine(Coroutine_Type);
            Coroutine_Type = null;
        }
    }

    private IEnumerator PlayTypingRoutine(string bodyText, float speedMultiplier)
    {
        string safeText = string.IsNullOrEmpty(bodyText) ? string.Empty : bodyText;
        float interval = _baseCharacterInterval / Mathf.Max(0.01f, speedMultiplier);

        do
        {
            SetVisibleSpeechText(string.Empty);

            for (int index = 0; index < safeText.Length; index++)
            {
                SetVisibleSpeechText(safeText.Substring(0, index + 1));
                yield return new WaitForSeconds(interval);
            }

            if (_isLooping)
                yield return new WaitForSeconds(1.2f);
        }
        while (_isLooping);

        Coroutine_Type = null;
    }

    private void SetVisibleSpeechText(string value)
    {
        if (Text_Body != null)
            Text_Body.text = value;

        if (Text_ForcedBody != null && Text_ForcedBody != Text_Body)
            Text_ForcedBody.text = value;

        if (Text_OverlayBody != null)
            Text_OverlayBody.text = value;
    }

    private void ResolveReferences()
    {
        if (Canvas_Bubble == null)
            Canvas_Bubble = GetComponentInChildren<Canvas>(true);

        if (Scroll_Body == null)
            Scroll_Body = GetComponentInChildren<ScrollRect>(true);

        if (Text_Body == null)
            Text_Body = GetComponentInChildren<TextMeshProUGUI>(true);

        if (Text_Body == null)
            CreateFallbackSpeechBubbleView();

        EnsureForcedOverlayText();
        EnsureScreenOverlayText();
        Text_Body = Text_ForcedBody != null ? Text_ForcedBody : Text_Body;
        OOTechTMPFontUtility.ApplyProjectFont(Text_Body);
    }

    private void EnsureForcedOverlayText()
    {
        if (Text_ForcedBody == null)
        {
            Transform existingTextTransform = transform.Find("Canvas_ForcedSpeechBubbleText/Text_ForcedSpeechBubbleBody");

            if (existingTextTransform != null)
                Text_ForcedBody = existingTextTransform.GetComponent<TextMeshProUGUI>();
        }

        if (Text_ForcedBody == null)
            CreateForcedOverlayText();

        PrepareForcedOverlayText();
    }

    private void CreateForcedOverlayText()
    {
        GameObject canvasObject = new GameObject("Canvas_ForcedSpeechBubbleText", typeof(RectTransform), typeof(Canvas));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localPosition = Vector3.zero;
        canvasObject.transform.localRotation = Quaternion.identity;
        canvasObject.transform.localScale = Vector3.one * 0.01f;

        Canvas forcedCanvas = canvasObject.GetComponent<Canvas>();
        forcedCanvas.renderMode = RenderMode.WorldSpace;
        forcedCanvas.overrideSorting = true;
        forcedCanvas.sortingOrder = 9000;

        RectTransform canvasRect = canvasObject.transform as RectTransform;
        canvasRect.sizeDelta = new Vector2(520f, 210f);
        canvasRect.anchoredPosition = Vector2.zero;

        GameObject textObject = new GameObject("Text_ForcedSpeechBubbleBody", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvasObject.transform, false);
        Text_ForcedBody = textObject.GetComponent<TextMeshProUGUI>();

        RectTransform textRect = textObject.transform as RectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(34f, 24f);
        textRect.offsetMax = new Vector2(-34f, -24f);
    }

    private void PrepareForcedOverlayText()
    {
        if (Text_ForcedBody == null)
            return;

        Canvas forcedCanvas = Text_ForcedBody.GetComponentInParent<Canvas>(true);

        if (forcedCanvas != null)
        {
            forcedCanvas.gameObject.SetActive(true);
            forcedCanvas.enabled = true;
            forcedCanvas.renderMode = RenderMode.WorldSpace;
            forcedCanvas.overrideSorting = true;
            forcedCanvas.sortingOrder = 9000;
            forcedCanvas.transform.SetAsLastSibling();
        }

        Text_ForcedBody.gameObject.SetActive(true);
        Text_ForcedBody.enabled = true;
        Text_ForcedBody.fontSize = 36f;
        Text_ForcedBody.color = Color.black;
        Text_ForcedBody.alignment = TextAlignmentOptions.Center;
        Text_ForcedBody.enableWordWrapping = true;
        Text_ForcedBody.overflowMode = TextOverflowModes.Overflow;
        Text_ForcedBody.raycastTarget = false;
        Text_ForcedBody.transform.SetAsLastSibling();
        OOTechTMPFontUtility.ApplyProjectFont(Text_ForcedBody);
    }

    private void EnsureScreenOverlayText()
    {
        if (Text_OverlayBody == null)
        {
            Transform existingTextTransform = transform.Find("Canvas_ScreenSpeechBubbleText/Text_ScreenSpeechBubbleBody");

            if (existingTextTransform != null)
                Text_OverlayBody = existingTextTransform.GetComponent<TextMeshProUGUI>();
        }

        if (Text_OverlayBody == null)
            CreateScreenOverlayText();

        PrepareScreenOverlayText();
        UpdateOverlayTextPosition();
    }

    private void CreateScreenOverlayText()
    {
        GameObject canvasObject = new GameObject("Canvas_ScreenSpeechBubbleText", typeof(RectTransform), typeof(Canvas));
        canvasObject.transform.SetParent(transform, false);
        Rect_OverlayCanvas = canvasObject.transform as RectTransform;

        Canvas overlayCanvas = canvasObject.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder = 9999;

        Rect_OverlayCanvas.sizeDelta = _overlayTextSize;
        Rect_OverlayCanvas.localScale = Vector3.one;

        GameObject textObject = new GameObject("Text_ScreenSpeechBubbleBody", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvasObject.transform, false);
        Text_OverlayBody = textObject.GetComponent<TextMeshProUGUI>();

        RectTransform textRect = textObject.transform as RectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private void PrepareScreenOverlayText()
    {
        if (Text_OverlayBody == null)
            return;

        Canvas overlayCanvas = Text_OverlayBody.GetComponentInParent<Canvas>(true);

        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(true);
            overlayCanvas.enabled = true;
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 9999;
            Rect_OverlayCanvas = overlayCanvas.transform as RectTransform;
        }

        if (Rect_OverlayCanvas != null)
        {
            Rect_OverlayCanvas.sizeDelta = _overlayTextSize;
            Rect_OverlayCanvas.localScale = Vector3.one;
        }

        Text_OverlayBody.gameObject.SetActive(true);
        Text_OverlayBody.enabled = true;
        Text_OverlayBody.fontSize = 30f;
        Text_OverlayBody.fontStyle = FontStyles.Bold;
        Text_OverlayBody.color = Color.black;
        Text_OverlayBody.alignment = TextAlignmentOptions.Center;
        Text_OverlayBody.enableWordWrapping = true;
        Text_OverlayBody.overflowMode = TextOverflowModes.Overflow;
        Text_OverlayBody.raycastTarget = false;
        Text_OverlayBody.transform.SetAsLastSibling();
        OOTechTMPFontUtility.ApplyProjectFont(Text_OverlayBody);
    }

    private void UpdateOverlayTextPosition()
    {
        if (Rect_OverlayCanvas == null || Text_OverlayBody == null || !gameObject.activeInHierarchy)
            return;

        Camera mainCamera = Camera.main;
        Vector3 screenPosition = mainCamera != null
            ? mainCamera.WorldToScreenPoint(transform.position)
            : transform.position;

        if (screenPosition.z < 0f)
            return;

        Rect_OverlayCanvas.position = new Vector3(
            screenPosition.x + _overlayScreenOffset.x,
            screenPosition.y + _overlayScreenOffset.y,
            0f);
    }

    private void CreateFallbackSpeechBubbleView()
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localPosition = Vector3.zero;
        canvasObject.transform.localRotation = Quaternion.identity;
        canvasObject.transform.localScale = Vector3.one * 0.01f;

        Canvas_Bubble = canvasObject.GetComponent<Canvas>();
        Canvas_Bubble.renderMode = RenderMode.WorldSpace;
        Canvas_Bubble.overrideSorting = true;
        Canvas_Bubble.sortingOrder = 6200;

        RectTransform canvasRect = canvasObject.transform as RectTransform;
        canvasRect.sizeDelta = new Vector2(360f, 150f);
        canvasRect.anchoredPosition = Vector2.zero;

        GameObject scrollObject = new GameObject("Scroll View", typeof(RectTransform), typeof(ScrollRect));
        scrollObject.transform.SetParent(canvasObject.transform, false);
        Scroll_Body = scrollObject.GetComponent<ScrollRect>();

        RectTransform scrollRect = scrollObject.transform as RectTransform;
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;

        GameObject contentObject = new GameObject("Content", typeof(RectTransform));
        contentObject.transform.SetParent(scrollObject.transform, false);
        RectTransform contentRect = contentObject.transform as RectTransform;
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(contentObject.transform, false);
        Text_Body = textObject.GetComponent<TextMeshProUGUI>();
        Text_Body.text = string.Empty;
        Text_Body.fontSize = 26f;
        Text_Body.color = Color.black;
        Text_Body.alignment = TextAlignmentOptions.Center;
        Text_Body.enableWordWrapping = true;
        Text_Body.raycastTarget = false;

        RectTransform textRect = textObject.transform as RectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 10f);
        textRect.offsetMax = new Vector2(-16f, -10f);

        Scroll_Body.content = contentRect;
        Scroll_Body.viewport = scrollRect;
        Scroll_Body.horizontal = false;
        Scroll_Body.vertical = false;
    }

    /// <summary>
    /// ScrollView의 배경은 투명하게 두고, 텍스트만 말풍선 안에 보이게 정리합니다.
    /// </summary>
    private void PrepareTransparentScrollView()
    {
        if (Canvas_Bubble != null)
        {
            Canvas_Bubble.renderMode = RenderMode.WorldSpace;
            Canvas_Bubble.overrideSorting = true;
            Canvas_Bubble.sortingOrder = 6200;
        }

        if (Scroll_Body == null)
            return;

        Image[] imageArray = Scroll_Body.GetComponentsInChildren<Image>(true);

        foreach (Image image in imageArray)
        {
            if (image == null)
                continue;

            Color color = image.color;
            color.a = 0f;
            image.color = color;
        }
    }
}
