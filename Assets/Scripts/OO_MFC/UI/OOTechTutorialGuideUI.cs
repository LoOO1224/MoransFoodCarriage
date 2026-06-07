// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechTutorialGuideUI.cs
// - 역할: UI 표시와 입력 연결을 담당하는 UI 컴포넌트입니다.
// - 감독 관점: 관객에게 보이는 패널과 버튼의 무대 동선을 담당합니다.
// - 유지보수 포인트: 사용자가 직접 편집할 UI는 하이어라키/프리팹에 두고, 코드에서 즉석 생성하지 않습니다.
// =============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TutorialGuideGroup 전용 UI입니다.
/// DialogueGroup과 비슷하게 나레이션 데이터를 스크롤 가능한 안내문으로 보여주지만, 화면 중상단에 표시되는 튜토리얼 가이드 역할만 담당합니다.
/// </summary>
public class OOTechTutorialGuideUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private RectTransform Rect_Root;
    [SerializeField] private Image Image_DimBackground;
    [SerializeField] private Image Image_Panel;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI Text_Title;
    [SerializeField] private TextMeshProUGUI Text_Dialogue;

    [Header("Button")]
    [SerializeField] private Button Button_Next;
    [SerializeField] private TextMeshProUGUI Text_NextButton;

    [Header("Scroll")]
    [SerializeField] private ScrollRect Scroll_Dialogue;
    [SerializeField] private RectTransform Rect_DialogueContent;
    [SerializeField] private float _scrollContentPadding = 16f;

    [Header("Default View Asset")]
    [SerializeField] private TMP_FontAsset Font_Default;
    [SerializeField] private Sprite Sprite_PanelBackground;
    [SerializeField] private Sprite Sprite_NextButton;
    [SerializeField] private Sprite Sprite_NextButtonHovered;
    [SerializeField] private Sprite Sprite_NextButtonPressed;

    [Header("Default Text")]
    [SerializeField] private string _guideTitle = "튜토리얼 가이드";
    [SerializeField] private string _nextButtonText = "이어가기";
    [SerializeField] private bool _isCreateDefaultViewOnAwake = true;

    [Header("Complete Emphasis Effect")]
    [SerializeField] private Color _titleEmphasisColorA = Color.white;
    [SerializeField] private Color _titleEmphasisColorB = new Color(1f, 0.82f, 0.05f, 1f);
    [SerializeField] private float _titleEmphasisSpeed = 7f;
    [SerializeField] private float _titleEmphasisScalePower = 0.08f;

    // ==================== 가이드 상태 ====================
    private readonly List<string> _guideTextList = new List<string>();
    private int _currentGuideTextIndex;
    private Action _onGuideEnd;
    private Coroutine _refreshScrollCoroutine;
    private Coroutine _titleEmphasisCoroutine;
    private Color _originTitleColor = Color.white;
    private Vector3 _originTitleScale = Vector3.one;
    private bool _isCachedTitleOrigin;
    private int _lastGuideAdvanceFrame = -1;

    /// <summary>
    /// 가이드 UI가 처음 준비될 때 씬 소품이 없으면 최소 기본 뷰를 준비합니다.
    /// </summary>
    private void Awake()
    {
        CreateDefaultViewIfNeeded();
    }

    /// <summary>
    /// TutorialGuideGroup이 열릴 때 이어가기 버튼 이벤트를 연결합니다.
    /// </summary>
    private void OnEnable()
    {
        BindButtonEvent();
    }

    /// <summary>
    /// TutorialGuideGroup이 닫힐 때 버튼 이벤트와 강조/스크롤 코루틴을 정리합니다.
    /// </summary>
    private void OnDisable()
    {
        UnbindButtonEvent();
        StopRefreshScrollCoroutine();
        StopTitleEmphasisEffect();
    }

    /// <summary>
    /// 안내 패널이 떠 있는 동안에는 화면 어디를 클릭해도 이어가기 버튼과 같은 큐로 넘깁니다.
    /// 버튼 클릭과 배경 클릭이 같은 프레임에 겹쳐도 한 번만 처리되도록 막습니다.
    /// </summary>
    private void Update()
    {
        if (_guideTextList.Count == 0)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        NextGuide();
    }

    // ==================== 버튼 바인딩 ====================

    /// <summary>
    /// 이어가기 버튼을 현재 가이드 진행 함수에 연결합니다.
    /// </summary>
    private void BindButtonEvent()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(NextGuide);
        Button_Next.onClick.AddListener(NextGuide);
    }

    /// <summary>
    /// 이어가기 버튼 이벤트를 해제해 중복 호출을 막습니다.
    /// </summary>
    private void UnbindButtonEvent()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(NextGuide);
    }

    // ==================== 가이드 표시 ====================

    /// <summary>
    /// OO_Narration 데이터를 기반으로 튜토리얼 가이드를 표시합니다.
    /// </summary>
    public void ShowGuide(OO_Narration narrationData, Action onGuideEnd = null)
    {
        if (narrationData == null)
        {
            Debug.LogWarning("[OOTechTutorialGuideUI] 표시할 튜토리얼 가이드 데이터가 없습니다.");
            return;
        }

        gameObject.SetActive(true);

        _onGuideEnd = onGuideEnd;
        _currentGuideTextIndex = 0;
        _lastGuideAdvanceFrame = Time.frameCount;
        _guideTextList.Clear();

        AddGuideTextList(narrationData.NarrationTexts);

        if (_guideTextList.Count == 0)
        {
            Debug.LogWarning($"[OOTechTutorialGuideUI] 튜토리얼 가이드 텍스트가 비어 있습니다: {narrationData.Id}");
            FinishGuide();
            return;
        }

        SetTitle(string.IsNullOrEmpty(narrationData.Title) ? _guideTitle : narrationData.Title);
        SetNextButtonText(_nextButtonText);
        ShowCurrentGuideText();
    }

    /// <summary>
    /// OO_Tutorial 데이터를 기반으로 튜토리얼 가이드 팝업을 표시합니다.
    /// Tutorial1Group의 시작 안내와 임무 완수 안내처럼 상단 가이드 전용 데이터에 사용합니다.
    /// </summary>
    public void ShowGuide(OO_Tutorial tutorialData, Action onGuideEnd = null)
    {
        if (tutorialData == null)
        {
            Debug.LogWarning("[OOTechTutorialGuideUI] 표시할 튜토리얼 데이터가 없습니다.");
            return;
        }

        gameObject.SetActive(true);

        _onGuideEnd = onGuideEnd;
        _currentGuideTextIndex = 0;
        _lastGuideAdvanceFrame = Time.frameCount;
        _guideTextList.Clear();

        AddGuideText(tutorialData.Description);

        if (_guideTextList.Count == 0)
        {
            Debug.LogWarning($"[OOTechTutorialGuideUI] 튜토리얼 안내 문장이 비어 있습니다: {tutorialData.Id}");
            FinishGuide();
            return;
        }

        SetTitle(string.IsNullOrEmpty(tutorialData.Title) ? _guideTitle : tutorialData.Title);
        SetNextButtonText(_nextButtonText);
        ShowCurrentGuideText();
    }

    /// <summary>
    /// 가이드 패널을 닫고 현재 표시 상태를 비웁니다.
    /// </summary>
    public void CloseGuide()
    {
        StopTitleEmphasisEffect();
        ClearGuideState();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 임무 완수처럼 성취감을 강조해야 하는 안내에서 제목을 반짝이게 합니다.
    /// 일반 튜토리얼 안내와 대화 UI에는 영향을 주지 않습니다.
    /// </summary>
    public void SetTitleEmphasisActive(bool isActive)
    {
        if (isActive)
        {
            StartTitleEmphasisEffect();
            return;
        }

        StopTitleEmphasisEffect();
    }

    /// <summary>
    /// 데이터 문장 목록을 표시 페이지 목록으로 옮깁니다.
    /// </summary>
    private void AddGuideTextList(List<string> narrationTexts)
    {
        if (narrationTexts == null)
            return;

        foreach (string narrationText in narrationTexts)
            AddGuideText(narrationText);
    }

    /// <summary>
    /// &lt;np&gt; 태그를 기준으로 튜토리얼 문장을 여러 페이지로 나눕니다.
    /// </summary>
    private void AddGuideText(string guideText)
    {
        if (string.IsNullOrWhiteSpace(guideText))
            return;

        string[] splitTextArray = guideText.Trim().Split(new[] { "<np>" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string splitText in splitTextArray)
        {
            if (string.IsNullOrWhiteSpace(splitText))
                continue;

            _guideTextList.Add(splitText.Trim());
        }
    }

    /// <summary>
    /// 현재 페이지의 튜토리얼 문장을 본문에 표시합니다.
    /// </summary>
    private void ShowCurrentGuideText()
    {
        if (_currentGuideTextIndex < 0 || _currentGuideTextIndex >= _guideTextList.Count)
        {
            FinishGuide();
            return;
        }

        SetDialogueText(_guideTextList[_currentGuideTextIndex]);
    }

    // ==================== 다음 가이드 진행 ====================

    /// <summary>
    /// 이어가기 버튼을 눌렀을 때 다음 안내문으로 이동합니다.
    /// 모든 안내문을 읽으면 종료 콜백을 호출합니다.
    /// </summary>
    public void NextGuide()
    {
        if (_lastGuideAdvanceFrame == Time.frameCount)
            return;

        _lastGuideAdvanceFrame = Time.frameCount;
        _currentGuideTextIndex++;

        if (_currentGuideTextIndex < _guideTextList.Count)
        {
            ShowCurrentGuideText();
            return;
        }

        FinishGuide();
    }

    /// <summary>
    /// 가이드가 끝났음을 외부 컨트롤러에 알립니다.
    /// </summary>
    private void FinishGuide()
    {
        Action onGuideEnd = _onGuideEnd;
        ClearGuideState();

        if (gameObject.activeSelf)
            gameObject.SetActive(false);

        onGuideEnd?.Invoke();
    }

    /// <summary>
    /// 현재 가이드 데이터와 콜백을 초기화합니다.
    /// </summary>
    private void ClearGuideState()
    {
        _guideTextList.Clear();
        _currentGuideTextIndex = 0;
        _onGuideEnd = null;
    }

    // ==================== 완료 강조 효과 ====================

    /// <summary>
    /// 임무 완료 같은 장면에서 제목을 반짝이게 하는 강조 연출을 시작합니다.
    /// </summary>
    private void StartTitleEmphasisEffect()
    {
        StopTitleEmphasisEffect();

        if (Text_Title == null)
            return;

        CacheTitleOriginIfNeeded();
        _titleEmphasisCoroutine = StartCoroutine(PlayTitleEmphasisEffectRoutine());
    }

    /// <summary>
    /// 제목 색상과 크기를 반복해서 흔들어 완료감을 강조합니다.
    /// </summary>
    private IEnumerator PlayTitleEmphasisEffectRoutine()
    {
        while (true)
        {
            float lerpValue = (Mathf.Sin(Time.unscaledTime * _titleEmphasisSpeed) + 1f) * 0.5f;

            if (Text_Title != null)
            {
                Text_Title.color = Color.Lerp(_titleEmphasisColorA, _titleEmphasisColorB, lerpValue);
                Text_Title.rectTransform.localScale = _originTitleScale * (1f + (_titleEmphasisScalePower * lerpValue));
            }

            yield return null;
        }
    }

    /// <summary>
    /// 제목 강조 연출을 멈추고 원래 스타일로 되돌립니다.
    /// </summary>
    private void StopTitleEmphasisEffect()
    {
        if (_titleEmphasisCoroutine != null)
        {
            StopCoroutine(_titleEmphasisCoroutine);
            _titleEmphasisCoroutine = null;
        }

        if (!_isCachedTitleOrigin || Text_Title == null)
            return;

        Text_Title.color = _originTitleColor;
        Text_Title.rectTransform.localScale = _originTitleScale;
    }

    /// <summary>
    /// 제목 강조 전 원래 색과 크기를 저장합니다.
    /// </summary>
    private void CacheTitleOriginIfNeeded()
    {
        if (_isCachedTitleOrigin || Text_Title == null)
            return;

        _originTitleColor = Text_Title.color;
        _originTitleScale = Text_Title.rectTransform.localScale;
        _isCachedTitleOrigin = true;
    }

    // ==================== UI 값 설정 ====================

    /// <summary>
    /// 튜토리얼 제목 텍스트를 적용합니다.
    /// </summary>
    private void SetTitle(string title)
    {
        if (Text_Title != null)
            Text_Title.text = title;
    }

    /// <summary>
    /// 튜토리얼 본문을 적용하고 스크롤 크기를 갱신합니다.
    /// </summary>
    private void SetDialogueText(string dialogueText)
    {
        if (Text_Dialogue != null)
        {
            Text_Dialogue.text = dialogueText ?? string.Empty;
            Text_Dialogue.gameObject.SetActive(true);
            Text_Dialogue.enabled = true;
            Text_Dialogue.color = new Color(Text_Dialogue.color.r, Text_Dialogue.color.g, Text_Dialogue.color.b, 1f);
            Text_Dialogue.canvasRenderer.SetAlpha(1f);
            Text_Dialogue.textWrappingMode = TextWrappingModes.Normal;
            Text_Dialogue.overflowMode = TextOverflowModes.Overflow;
            Text_Dialogue.maskable = true;
        }

        ResizeDialogueTextToPreferredHeight();
        RebuildScrollContent();
        ResetScrollPosition();
        RequestRefreshScrollOnNextFrame();
    }

    /// <summary>
    /// 이어가기 버튼 문구를 적용합니다.
    /// </summary>
    private void SetNextButtonText(string buttonText)
    {
        if (Text_NextButton != null)
            Text_NextButton.text = buttonText;
    }

    // ==================== 기본 뷰 생성 ====================

    /// <summary>
    /// 씬에 세부 UI 자식이 연결되지 않은 경우에도 바로 동작하도록 기본 가이드 뷰를 생성합니다.
    /// 생성된 오브젝트는 TutorialGuideGroup 내부에만 만들어지며, 검색 함수는 사용하지 않습니다.
    /// </summary>
    private void CreateDefaultViewIfNeeded()
    {
        if (!_isCreateDefaultViewOnAwake)
            return;

        if (Rect_Root == null)
            Rect_Root = transform as RectTransform;

        if (Rect_Root == null)
        {
            Debug.LogError("[OOTechTutorialGuideUI] TutorialGuideGroup에는 RectTransform이 필요합니다.");
            return;
        }

        StretchFullScreen(Rect_Root);

        if (Text_Dialogue != null && Button_Next != null)
            return;

        CreateDimBackground();
        CreateGuidePanel();
    }

    private void CreateDimBackground()
    {
        GameObject dimObject = CreateRectObject("Image_DimBackground", Rect_Root);
        RectTransform dimRect = dimObject.transform as RectTransform;
        StretchFullScreen(dimRect);

        Image_DimBackground = dimObject.AddComponent<Image>();
        Image_DimBackground.color = new Color(0f, 0f, 0f, 0.45f);
        Image_DimBackground.raycastTarget = true;
    }

    private void CreateGuidePanel()
    {
        GameObject panelObject = CreateRectObject("TutorialGuidePanel", Rect_Root);
        RectTransform panelRect = panelObject.transform as RectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -80f);
        panelRect.sizeDelta = new Vector2(1400f, 330f);

        Image_Panel = panelObject.AddComponent<Image>();
        Image_Panel.sprite = Sprite_PanelBackground;
        Image_Panel.type = Image.Type.Sliced;
        Image_Panel.color = new Color(1f, 0.93f, 0.76f, 0.94f);
        Image_Panel.raycastTarget = true;

        CreateTitleText(panelRect);
        CreateScrollView(panelRect);
        CreateNextButton(panelRect);
    }

    private void CreateTitleText(RectTransform panelRect)
    {
        GameObject titleObject = CreateRectObject("Text_Title", panelRect);
        RectTransform titleRect = titleObject.transform as RectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -34f);
        titleRect.sizeDelta = new Vector2(-120f, 55f);

        Text_Title = titleObject.AddComponent<TextMeshProUGUI>();
        ApplyTextStyle(Text_Title, 44f, new Color(0.12f, 0.08f, 0.05f, 1f), TextAlignmentOptions.Center);
        Text_Title.text = _guideTitle;
    }

    private void CreateScrollView(RectTransform panelRect)
    {
        GameObject scrollObject = CreateRectObject("Scroll_TutorialGuide", panelRect);
        RectTransform scrollRect = scrollObject.transform as RectTransform;
        scrollRect.anchorMin = new Vector2(0f, 0f);
        scrollRect.anchorMax = new Vector2(1f, 1f);
        scrollRect.pivot = new Vector2(0.5f, 0.5f);
        scrollRect.anchoredPosition = new Vector2(0f, -24f);
        scrollRect.sizeDelta = new Vector2(-140f, -160f);

        Scroll_Dialogue = scrollObject.AddComponent<ScrollRect>();
        Scroll_Dialogue.horizontal = false;
        Scroll_Dialogue.vertical = true;
        Scroll_Dialogue.movementType = ScrollRect.MovementType.Clamped;

        GameObject viewportObject = CreateRectObject("Viewport", scrollRect);
        RectTransform viewportRect = viewportObject.transform as RectTransform;
        StretchFullScreen(viewportRect);

        Image viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0f);
        viewportImage.raycastTarget = false;
        viewportObject.AddComponent<RectMask2D>();

        GameObject contentObject = CreateRectObject("Content", viewportRect);
        Rect_DialogueContent = contentObject.transform as RectTransform;
        Rect_DialogueContent.anchorMin = new Vector2(0f, 1f);
        Rect_DialogueContent.anchorMax = new Vector2(1f, 1f);
        Rect_DialogueContent.pivot = new Vector2(0.5f, 1f);
        Rect_DialogueContent.anchoredPosition = Vector2.zero;
        Rect_DialogueContent.sizeDelta = Vector2.zero;

        GameObject textObject = CreateRectObject("Text_Dialogue", Rect_DialogueContent);
        RectTransform textRect = textObject.transform as RectTransform;
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        Text_Dialogue = textObject.AddComponent<TextMeshProUGUI>();
        ApplyTextStyle(Text_Dialogue, 36f, new Color(0.12f, 0.08f, 0.05f, 1f), TextAlignmentOptions.TopLeft);

        Scroll_Dialogue.viewport = viewportRect;
        Scroll_Dialogue.content = Rect_DialogueContent;
    }

    private void CreateNextButton(RectTransform panelRect)
    {
        GameObject buttonObject = CreateRectObject("Button_Next", panelRect);
        RectTransform buttonRect = buttonObject.transform as RectTransform;
        buttonRect.anchorMin = new Vector2(1f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0f);
        buttonRect.pivot = new Vector2(1f, 0f);
        buttonRect.anchoredPosition = new Vector2(-110f, 32f);
        buttonRect.sizeDelta = new Vector2(210f, 68f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.sprite = Sprite_NextButton;
        buttonImage.type = Image.Type.Sliced;
        buttonImage.color = new Color(0.86f, 0.54f, 0.30f, 0.96f);
        buttonImage.raycastTarget = true;

        Button_Next = buttonObject.AddComponent<Button>();
        Button_Next.targetGraphic = buttonImage;

        SpriteState spriteState = Button_Next.spriteState;
        spriteState.highlightedSprite = Sprite_NextButtonHovered;
        spriteState.pressedSprite = Sprite_NextButtonPressed;
        Button_Next.spriteState = spriteState;

        GameObject textObject = CreateRectObject("Text_NextButton", buttonRect);
        RectTransform textRect = textObject.transform as RectTransform;
        StretchFullScreen(textRect);

        Text_NextButton = textObject.AddComponent<TextMeshProUGUI>();
        ApplyTextStyle(Text_NextButton, 30f, new Color(0.12f, 0.08f, 0.05f, 1f), TextAlignmentOptions.Center);
        Text_NextButton.text = _nextButtonText;
    }

    private GameObject CreateRectObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.layer = gameObject.layer;
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private void StretchFullScreen(RectTransform rectTransform)
    {
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    private void ApplyTextStyle(TextMeshProUGUI text, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        if (text == null)
            return;

        if (Font_Default != null)
            text.font = Font_Default;

        OOTechTMPFontUtility.ApplyProjectFont(text);

        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
    }

    // ==================== 스크롤 갱신 ====================

    private void ResizeDialogueTextToPreferredHeight()
    {
        if (Text_Dialogue == null)
            return;

        RectTransform dialogueTextRect = Text_Dialogue.rectTransform;
        float contentWidth = GetDialogueContentWidth(dialogueTextRect);

        dialogueTextRect.anchorMin = new Vector2(0f, 1f);
        dialogueTextRect.anchorMax = new Vector2(1f, 1f);
        dialogueTextRect.pivot = new Vector2(0.5f, 1f);
        dialogueTextRect.anchoredPosition = Vector2.zero;
        dialogueTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);

        Text_Dialogue.ForceMeshUpdate();

        float preferredHeight = Text_Dialogue.GetPreferredValues(Text_Dialogue.text, contentWidth, 0f).y;
        float finalHeight = Mathf.Max(preferredHeight + _scrollContentPadding, 100f);

        dialogueTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight);

        if (Rect_DialogueContent != null)
        {
            Rect_DialogueContent.anchoredPosition = Vector2.zero;
            Rect_DialogueContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight);
        }

        Text_Dialogue.SetVerticesDirty();
        Text_Dialogue.SetLayoutDirty();
        Text_Dialogue.ForceMeshUpdate();
    }

    private float GetDialogueContentWidth(RectTransform dialogueTextRect)
    {
        if (Rect_DialogueContent != null && Rect_DialogueContent.rect.width > 1f)
            return Rect_DialogueContent.rect.width;

        if (dialogueTextRect != null && dialogueTextRect.rect.width > 1f)
            return dialogueTextRect.rect.width;

        return 1200f;
    }

    private void RebuildScrollContent()
    {
        Canvas.ForceUpdateCanvases();

        if (Rect_DialogueContent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(Rect_DialogueContent);

        Canvas.ForceUpdateCanvases();
    }

    private void ResetScrollPosition()
    {
        if (Rect_DialogueContent != null)
            Rect_DialogueContent.anchoredPosition = Vector2.zero;

        if (Scroll_Dialogue == null)
            return;

        if (Rect_DialogueContent != null)
            Scroll_Dialogue.content = Rect_DialogueContent;

        Scroll_Dialogue.StopMovement();
        Scroll_Dialogue.verticalNormalizedPosition = 1f;
    }

    private void RequestRefreshScrollOnNextFrame()
    {
        StopRefreshScrollCoroutine();

        if (gameObject.activeInHierarchy)
            _refreshScrollCoroutine = StartCoroutine(RefreshScrollOnNextFrame());
    }

    private IEnumerator RefreshScrollOnNextFrame()
    {
        yield return null;

        ResizeDialogueTextToPreferredHeight();
        RebuildScrollContent();
        ResetScrollPosition();

        _refreshScrollCoroutine = null;
    }

    private void StopRefreshScrollCoroutine()
    {
        if (_refreshScrollCoroutine == null)
            return;

        StopCoroutine(_refreshScrollCoroutine);
        _refreshScrollCoroutine = null;
    }
}
