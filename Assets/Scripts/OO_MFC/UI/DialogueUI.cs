// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: DialogueUI.cs
// - 역할: UI 표시와 입력 연결을 담당하는 UI 컴포넌트입니다.
// - 감독 관점: 관객에게 보이는 패널과 버튼의 무대 동선을 담당합니다.
// - 유지보수 포인트: 사용자가 직접 편집할 UI는 하이어라키/프리팹에 두고, 코드에서 즉석 생성하지 않습니다.
// =============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 대화창의 화자 이름, 본문, 다음 버튼, 스크롤 영역을 관리합니다.
/// 프롤로그 나레이션처럼 긴 문장이 들어오는 경우에는 ScrollRect의 Content를 갱신하고
/// 새 문장이 표시될 때마다 스크롤 위치를 맨 위로 되돌립니다.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    // 읽는 순서:
    // 1. OpenDialogue/StartDialogue 계열: 외부 Controller가 대사 ID를 넘기면 데이터에서 대사를 읽습니다.
    // 2. SetSpeaker 계열: OO_Character 데이터를 이용해 화자 이름을 표시합니다.
    // 3. SetDialogueText 계열: OO_Dialogue 또는 OO_Narration의 실제 문장을 패널에 넣습니다.
    // 4. NextDialogue: 이어가기 버튼 또는 패널 밖 클릭으로 다음 대사를 진행합니다.
    // 5. CloseDialogue 계열: 대사가 끝나면 DialogueGroup을 닫거나 다음 큐로 넘깁니다.
    // 유지보수 주의:
    // - 대사 내용은 코드에 쓰지 말고 OO_Dialogue/OO_Narration/OO_DialogueGroup에서 드리븐합니다.
    // - DialoguePanel 위치와 버튼 이미지는 하이어라키에서 직접 수정합니다.
    // - 화자 이름 배경은 OOTechDialogueSpeakerNameBackdrop이 보조합니다.

    [Header("Text")]
    [FormerlySerializedAs("_speakerNameText")]
    [SerializeField] private TextMeshProUGUI Text_SpeakerName;

    [FormerlySerializedAs("_dialogueText")]
    [SerializeField] private TextMeshProUGUI Text_Dialogue;

    [Header("Button")]
    [FormerlySerializedAs("_nextButton")]
    [SerializeField] private Button Button_Next;

    [Header("Choice")]
    [SerializeField] private GameObject Root_ChoicePanel;
    [SerializeField] private Button Button_ChoiceYes;
    [SerializeField] private Button Button_ChoiceNo;
    [SerializeField] private TextMeshProUGUI Text_ChoiceYes;
    [SerializeField] private TextMeshProUGUI Text_ChoiceNo;

    [Header("Scroll")]
    [SerializeField] private ScrollRect Scroll_Dialogue;
    [SerializeField] private RectTransform Rect_DialogueContent;
    [SerializeField] private float _scrollContentPadding = 16f;

    [Header("Outside Click")]
    [SerializeField] private bool _isAdvanceByOutsideClick = true;
    [SerializeField] private float _outsideClickDelay = 0.08f;
    [SerializeField] private int _roadViewSortingOrder = 1400;

    // ==================== 대화 상태 ====================
    private OO_Dialogue _currentDialogue;
    private readonly List<string> _narrationTextList = new List<string>();
    private int _currentNarrationTextIndex;
    private Action _onDialogueEnd;
    private OO_Choice _currentChoice;
    private Action<int> _onChoiceSelected;
    private bool _isChoiceActive;
    private Coroutine _refreshScrollCoroutine;
    private OOTechDialogueLayout Layout_Dialogue;
    private OOTechDialogueSpeakerNameBackdrop Backdrop_SpeakerName;
    private GameObject Root_OutsideClickBlocker;
    private Button Button_OutsideClickBlocker;
    private float _outsideClickReadyTime;

    /// <summary>
    /// 대화 UI가 처음 준비될 때 폰트, 화자 이름 배경, 기본 레이아웃을 준비합니다.
    /// </summary>
    private void Awake()
    {
        ApplyProjectFont();
        PrepareSpeakerNameBackdrop();
        ApplyDialogueLayout();
        PrepareChoicePanel();
    }

    /// <summary>
    /// DialogueGroup이 열릴 때 버튼 이벤트와 표시 스타일을 다시 연결합니다.
    /// </summary>
    private void OnEnable()
    {
        ApplyProjectFont();
        PrepareSpeakerNameBackdrop();
        ApplyDialogueLayout();
        PrepareChoicePanel();
        BindButtonEvent();
    }

    /// <summary>
    /// DialogueGroup이 닫힐 때 버튼 이벤트, 스크롤 갱신, 바깥 클릭 블로커를 정리합니다.
    /// </summary>
    private void OnDisable()
    {
        UnbindButtonEvent();
        StopRefreshScrollCoroutine();
        ClearChoiceState();
        SetOutsideClickBlockerActive(false);
    }

    /// <summary>
    /// 버튼이 아니라 대화창 밖을 클릭해도 이어가기처럼 진행되도록 감시합니다.
    /// </summary>
    private void Update()
    {
        if (_isChoiceActive)
        {
            UpdateChoiceKeyboardInput();
            return;
        }

        if (!CanAdvanceByOutsideClick())
            return;

        NextDialogue();
    }

    // ==================== 버튼 바인딩 ====================

    /// <summary>
    /// Dialogue_NextButton은 현재 표시 중인 문장 묶음만 진행합니다.
    /// 프롤로그 컷씬 전환은 DialogueUI가 끝났음을 PrologueController에 알려서 처리합니다.
    /// </summary>
    private void ApplyDialogueLayout()
    {
        if (Layout_Dialogue == null)
            Layout_Dialogue = GetComponent<OOTechDialogueLayout>();

        if (Layout_Dialogue != null)
            Layout_Dialogue.ApplyLayout();
    }

    /// <summary>
    /// Road View에서는 HUD가 하단에 있으므로 대화창을 중앙 무대로 올리고 HUD보다 위에 렌더링합니다.
    /// </summary>
    public void RequestRoadViewLayout()
    {
        if (Layout_Dialogue == null)
            Layout_Dialogue = GetComponent<OOTechDialogueLayout>();

        if (Layout_Dialogue == null)
            Layout_Dialogue = gameObject.AddComponent<OOTechDialogueLayout>();

        Layout_Dialogue.ApplyRoadViewLayout();
        RaiseCanvasForRoadViewDialogue();
    }

    /// <summary>
    /// 프로젝트 공통 한글 폰트를 화자 이름과 본문에 적용합니다.
    /// </summary>
    private void ApplyProjectFont()
    {
        OOTechTMPFontUtility.ApplyProjectFont(Text_SpeakerName);
        OOTechTMPFontUtility.ApplyProjectFont(Text_Dialogue);
        OOTechTMPFontUtility.ApplyProjectFont(Text_ChoiceYes);
        OOTechTMPFontUtility.ApplyProjectFont(Text_ChoiceNo);
    }

    /// <summary>
    /// SpeakerNameText 뒤의 반투명 이름표 컴포넌트를 준비합니다.
    /// </summary>
    private void PrepareSpeakerNameBackdrop()
    {
        if (Backdrop_SpeakerName == null)
            Backdrop_SpeakerName = GetComponent<OOTechDialogueSpeakerNameBackdrop>();

        if (Backdrop_SpeakerName == null)
            Backdrop_SpeakerName = gameObject.AddComponent<OOTechDialogueSpeakerNameBackdrop>();
    }

    /// <summary>
    /// Next 버튼을 현재 대화 진행 함수에 연결합니다.
    /// </summary>
    private void BindButtonEvent()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(NextDialogue);
        Button_Next.onClick.AddListener(NextDialogue);
    }

    /// <summary>
    /// Next 버튼 이벤트를 해제해 중복 호출을 막습니다.
    /// </summary>
    private void UnbindButtonEvent()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(NextDialogue);
    }

    // ==================== 일반 대화 표시 ====================

    /// <summary>
    /// OO_Dialogue 데이터를 기반으로 일반 대화를 표시합니다.
    /// </summary>
    public void ShowDialogue(OO_Dialogue dialogueData, Action onDialogueEnd = null)
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("[DialogueUI] 표시할 대화 데이터가 없습니다.");
            return;
        }

        _currentDialogue = dialogueData;
        _onDialogueEnd = onDialogueEnd;
        _narrationTextList.Clear();
        _currentNarrationTextIndex = 0;
        ClearChoiceState();

        gameObject.SetActive(true);
        BlockOutsideClickBriefly();

        SetSpeakerName(dialogueData.SpeakerName);
        SetDialogueText(dialogueData.Text);
        SetNextButtonActive(dialogueData.SelectionNameList == null || dialogueData.SelectionNameList.Count == 0);
    }

    // ==================== 나레이션 표시 ====================

    /// <summary>
    /// OO_Narration 데이터를 기반으로 나레이션을 표시합니다.
    /// SpeakerNameText는 요구사항에 따라 항상 "나레이션"으로 고정합니다.
    /// </summary>
    /// <summary>
    /// OO_Choice 데이터를 기존 DialoguePanel 위에 선택지 모드로 표시합니다.
    /// 영화 비유로는 같은 무대 세트에 "관객 선택 큐"만 추가로 내려놓는 방식입니다.
    /// </summary>
    public void ShowChoice(OO_Choice choiceData, Action<int> onChoiceSelected)
    {
        if (choiceData == null)
        {
            Debug.LogWarning("[DialogueUI] 표시할 선택지 데이터가 없습니다.");
            return;
        }

        _currentDialogue = null;
        _onDialogueEnd = null;
        _narrationTextList.Clear();
        _currentNarrationTextIndex = 0;
        _currentChoice = choiceData;
        _onChoiceSelected = onChoiceSelected;
        _isChoiceActive = true;

        gameObject.SetActive(true);
        PrepareChoicePanel();
        BlockOutsideClickBriefly();

        SetSpeakerName(choiceData.SpeakerName);
        SetDialogueText(choiceData.PromptText);
        SetNextButtonActive(false);
        ApplyChoiceButtonText(choiceData);
        SetChoicePanelActive(true);
    }

    public void ShowNarration(OO_Narration narrationData, Action onDialogueEnd = null)
    {
        if (narrationData == null)
        {
            Debug.LogWarning("[DialogueUI] 표시할 나레이션 데이터가 없습니다.");
            return;
        }

        _currentDialogue = null;
        _onDialogueEnd = onDialogueEnd;
        _narrationTextList.Clear();
        _currentNarrationTextIndex = 0;
        ClearChoiceState();

        AddNarrationTextList(narrationData.NarrationTexts);

        if (_narrationTextList.Count == 0)
        {
            Debug.LogWarning($"[DialogueUI] 나레이션 텍스트가 비어 있습니다: {narrationData.Id}");
            FinishDialogue();
            return;
        }

        gameObject.SetActive(true);

        SetSpeakerName("나레이션");
        BlockOutsideClickBriefly();
        SetNextButtonActive(true);
        ShowCurrentNarrationText();
    }

    /// <summary>
    /// 나레이션 텍스트를 표시 단위로 정리합니다.
    /// 기본은 JSON의 한 항목을 한 번에 보여주며, 필요하면 텍스트 안의 &lt;np&gt; 태그로 페이지를 나눌 수 있습니다.
    /// </summary>
    /// <summary>
    /// 나레이션 문장 묶음을 현재 페이지 단위로 추가합니다.
    /// </summary>
    private void AddNarrationTextList(List<string> narrationTexts)
    {
        if (narrationTexts == null)
            return;

        foreach (string narrationText in narrationTexts)
            AddNarrationText(narrationText);
    }

    /// <summary>
    /// 긴 나레이션 안의 &lt;np&gt; 태그를 기준으로 표시 페이지를 나눕니다.
    /// </summary>
    private void AddNarrationText(string narrationText)
    {
        if (string.IsNullOrWhiteSpace(narrationText))
            return;

        string normalizedText = narrationText.Trim();
        string[] splitTextArray = normalizedText.Split(new[] { "<np>" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string splitText in splitTextArray)
        {
            if (string.IsNullOrWhiteSpace(splitText))
                continue;

            _narrationTextList.Add(splitText.Trim());
        }
    }

    /// <summary>
    /// 현재 나레이션 페이지를 대화창 본문에 보여줍니다.
    /// </summary>
    private void ShowCurrentNarrationText()
    {
        if (_currentNarrationTextIndex < 0 || _currentNarrationTextIndex >= _narrationTextList.Count)
        {
            FinishDialogue();
            return;
        }

        SetSpeakerName("나레이션");
        SetDialogueText(_narrationTextList[_currentNarrationTextIndex]);
    }

    // ==================== 다음 대화 진행 ====================

    /// <summary>
    /// 다음 버튼 클릭 시 현재 나레이션의 다음 표시 단위로 진행합니다.
    /// 현재 파트의 나레이션이 모두 끝나면 종료 콜백을 호출하여 PrologueController가 다음 컷씬을 열게 합니다.
    /// </summary>
    public void NextDialogue()
    {
        if (_isChoiceActive)
            return;

        if (_narrationTextList.Count > 0)
        {
            MoveNextNarrationText();
            return;
        }

        MoveNextDialogue();
    }

    /// <summary>
    /// 나레이션이면 다음 페이지로, 일반 대화면 현재 대화를 종료합니다.
    /// </summary>
    private void MoveNextNarrationText()
    {
        _currentNarrationTextIndex++;

        if (_currentNarrationTextIndex < _narrationTextList.Count)
        {
            ShowCurrentNarrationText();
            return;
        }

        FinishDialogue();
    }

    /// <summary>
    /// 일반 대화는 한 번 읽으면 종료 콜백으로 다음 콜시트에 넘깁니다.
    /// </summary>
    private void MoveNextDialogue()
    {
        if (_currentDialogue == null)
        {
            FinishDialogue();
            return;
        }

        if (!string.IsNullOrEmpty(_currentDialogue.NextDialogueId))
            Debug.Log($"[DialogueUI] 다음 대화 ID가 지정되어 있습니다: {_currentDialogue.NextDialogueId}");

        FinishDialogue();
    }

    // ==================== 대화창 닫기 ====================

    /// <summary>
    /// 외부에서 대화창을 강제로 닫을 때 사용합니다.
    /// DialogueGroup 자체가 아니라 DialoguePanel만 끄고 싶을 때도 이 메서드를 호출합니다.
    /// </summary>
    public void CloseDialogue()
    {
        SetOutsideClickBlockerActive(false);
        ClearDialogueState();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 대화가 끝났음을 외부 컨트롤러에 알리고 현재 상태를 비웁니다.
    /// </summary>
    private void FinishDialogue()
    {
        Action onDialogueEnd = _onDialogueEnd;
        SetOutsideClickBlockerActive(false);
        ClearDialogueState();
        onDialogueEnd?.Invoke();
    }

    private void ClearDialogueState()
    {
        _currentDialogue = null;
        _narrationTextList.Clear();
        _currentNarrationTextIndex = 0;
        _onDialogueEnd = null;
        ClearChoiceState();
    }

    // ==================== UI 값 설정 ====================

    /// <summary>
    /// 화자 이름 칸에 표시할 이름을 적용합니다. 비어 있으면 나레이션으로 표시합니다.
    /// </summary>
    private void SetSpeakerName(string speakerName)
    {
        if (Text_SpeakerName != null)
            Text_SpeakerName.text = string.IsNullOrEmpty(speakerName) ? "나레이션" : speakerName;
    }

    /// <summary>
    /// 본문 텍스트를 적용하고 스크롤/높이를 즉시 갱신합니다.
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
    /// 이어가기 버튼 표시와 바깥 클릭 진행 가능 상태를 함께 맞춥니다.
    /// </summary>
    private void SetNextButtonActive(bool isActive)
    {
        if (Button_Next != null)
            Button_Next.gameObject.SetActive(isActive);

        SetOutsideClickBlockerActive(isActive && HasActiveDialogueText());
    }

    // ==================== Outside click advance ====================

    /// <summary>
    /// 대화창 바깥 클릭이 이어가기 입력으로 처리될 수 있는지 확인합니다.
    /// </summary>
    private bool CanAdvanceByOutsideClick()
    {
        if (!_isAdvanceByOutsideClick)
            return false;

        if (_isChoiceActive)
            return false;

        if (Root_OutsideClickBlocker != null && Root_OutsideClickBlocker.activeInHierarchy)
            return false;

        if (!gameObject.activeInHierarchy)
            return false;

        if (!HasActiveDialogueText())
            return false;

        if (Time.unscaledTime < _outsideClickReadyTime)
            return false;

        if (Button_Next != null && !Button_Next.gameObject.activeInHierarchy)
            return false;

        if (!Input.GetMouseButtonDown(0))
            return false;

        return !IsPointerInsideDialoguePanel();
    }

    /// <summary>
    /// 대화창 뒤에 투명 버튼을 깔아 바깥 클릭을 받을 준비를 합니다.
    /// </summary>
    private void PrepareOutsideClickBlocker()
    {
        if (Root_OutsideClickBlocker != null)
            return;

        Transform parentTransform = transform.parent;

        if (parentTransform == null)
            return;

        Root_OutsideClickBlocker = new GameObject("Button_DialogueOutsideClickBlocker", typeof(RectTransform));
        Root_OutsideClickBlocker.transform.SetParent(parentTransform, false);

        RectTransform blockerRect = Root_OutsideClickBlocker.transform as RectTransform;

        if (blockerRect != null)
        {
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.pivot = new Vector2(0.5f, 0.5f);
            blockerRect.offsetMin = Vector2.zero;
            blockerRect.offsetMax = Vector2.zero;
            blockerRect.localScale = Vector3.one;
        }

        Image blockerImage = Root_OutsideClickBlocker.AddComponent<Image>();
        blockerImage.color = new Color(0f, 0f, 0f, 0f);
        blockerImage.raycastTarget = true;

        Button_OutsideClickBlocker = Root_OutsideClickBlocker.AddComponent<Button>();
        Button_OutsideClickBlocker.transition = Selectable.Transition.None;
        Button_OutsideClickBlocker.onClick.AddListener(HandleOutsideClickBlockerClicked);
        Root_OutsideClickBlocker.SetActive(false);
    }

    /// <summary>
    /// 바깥 클릭 블로커를 켜고 끕니다.
    /// </summary>
    private void SetOutsideClickBlockerActive(bool isActive)
    {
        if (!_isAdvanceByOutsideClick)
            isActive = false;

        if (isActive)
            PrepareOutsideClickBlocker();

        if (Root_OutsideClickBlocker == null)
            return;

        Root_OutsideClickBlocker.SetActive(isActive);

        if (!isActive)
            return;

        Root_OutsideClickBlocker.transform.SetSiblingIndex(transform.GetSiblingIndex());
        transform.SetAsLastSibling();
    }

    /// <summary>
    /// 투명 블로커가 클릭되면 NextDialogue와 같은 동작을 실행합니다.
    /// </summary>
    private void HandleOutsideClickBlockerClicked()
    {
        if (Time.unscaledTime < _outsideClickReadyTime)
            return;

        NextDialogue();
    }

    private bool HasActiveDialogueText()
    {
        return _currentDialogue != null || _narrationTextList.Count > 0 || _currentChoice != null;
    }

    /// <summary>
    /// ChoicePanel을 준비합니다. 씬에 배치되어 있으면 재사용하고, 없으면 DialoguePanel 안에 최소 버튼만 만듭니다.
    /// </summary>
    private void PrepareChoicePanel()
    {
        if (Root_ChoicePanel == null)
            Root_ChoicePanel = FindChildByName(transform, "ChoicePanel");

        if (Root_ChoicePanel == null)
            Root_ChoicePanel = CreateChoicePanelObject();

        if (Button_ChoiceYes == null)
            Button_ChoiceYes = FindChoiceButton("Button_ChoiceY", Root_ChoicePanel);

        if (Button_ChoiceNo == null)
            Button_ChoiceNo = FindChoiceButton("Button_ChoiceN", Root_ChoicePanel);

        if (Button_ChoiceYes == null)
            Button_ChoiceYes = CreateChoiceButton(Root_ChoicePanel.transform, "Button_ChoiceY", "예");

        if (Button_ChoiceNo == null)
            Button_ChoiceNo = CreateChoiceButton(Root_ChoicePanel.transform, "Button_ChoiceN", "아니오");

        Text_ChoiceYes = Text_ChoiceYes != null ? Text_ChoiceYes : Button_ChoiceYes.GetComponentInChildren<TextMeshProUGUI>(true);
        Text_ChoiceNo = Text_ChoiceNo != null ? Text_ChoiceNo : Button_ChoiceNo.GetComponentInChildren<TextMeshProUGUI>(true);

        BindChoiceButtonEvent();
        SetChoicePanelActive(false);
    }

    private GameObject CreateChoicePanelObject()
    {
        GameObject panelObject = new GameObject("ChoicePanel", typeof(RectTransform));
        panelObject.transform.SetParent(transform, false);

        RectTransform panelRect = panelObject.transform as RectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 28f);
        panelRect.sizeDelta = new Vector2(520f, 84f);

        HorizontalLayoutGroup layoutGroup = panelObject.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = 24f;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;

        return panelObject;
    }

    private Button CreateChoiceButton(Transform parentTransform, string buttonName, string labelText)
    {
        GameObject buttonObject = new GameObject(buttonName, typeof(RectTransform));
        buttonObject.transform.SetParent(parentTransform, false);

        RectTransform buttonRect = buttonObject.transform as RectTransform;
        buttonRect.sizeDelta = new Vector2(220f, 64f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.08f, 0.08f, 0.08f, 0.88f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        GameObject textObject = new GameObject("Text_Label", typeof(RectTransform));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.transform as RectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = labelText;
        label.fontSize = 30f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        OOTechTMPFontUtility.ApplyProjectFont(label);

        return button;
    }

    private Button FindChoiceButton(string buttonName, GameObject rootObject)
    {
        if (rootObject == null)
            return null;

        GameObject buttonObject = FindChildByName(rootObject.transform, buttonName);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    private void BindChoiceButtonEvent()
    {
        if (Button_ChoiceYes != null)
        {
            Button_ChoiceYes.onClick.RemoveListener(SelectChoiceYes);
            Button_ChoiceYes.onClick.AddListener(SelectChoiceYes);
        }

        if (Button_ChoiceNo != null)
        {
            Button_ChoiceNo.onClick.RemoveListener(SelectChoiceNo);
            Button_ChoiceNo.onClick.AddListener(SelectChoiceNo);
        }
    }

    private void ApplyChoiceButtonText(OO_Choice choiceData)
    {
        string yesText = GetChoiceButtonText(choiceData, 0, "예");
        string noText = GetChoiceButtonText(choiceData, 1, "아니오");

        if (Text_ChoiceYes != null)
            Text_ChoiceYes.text = yesText;

        if (Text_ChoiceNo != null)
            Text_ChoiceNo.text = noText;
    }

    private string GetChoiceButtonText(OO_Choice choiceData, int index, string fallbackText)
    {
        if (choiceData == null || choiceData.OptionTextList == null || index < 0 || index >= choiceData.OptionTextList.Count)
            return fallbackText;

        string optionText = choiceData.OptionTextList[index];
        return string.IsNullOrWhiteSpace(optionText) ? fallbackText : optionText;
    }

    private void UpdateChoiceKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Return))
            SelectChoice(0);
        else if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.Escape))
            SelectChoice(1);
    }

    private void SelectChoiceYes()
    {
        SelectChoice(0);
    }

    private void SelectChoiceNo()
    {
        SelectChoice(1);
    }

    private void SelectChoice(int choiceIndex)
    {
        if (!_isChoiceActive)
            return;

        Action<int> onChoiceSelected = _onChoiceSelected;
        ClearChoiceState();
        onChoiceSelected?.Invoke(choiceIndex);
    }

    private void SetChoicePanelActive(bool isActive)
    {
        if (Root_ChoicePanel != null)
            Root_ChoicePanel.SetActive(isActive);
    }

    private void ClearChoiceState()
    {
        _currentChoice = null;
        _onChoiceSelected = null;
        _isChoiceActive = false;
        SetChoicePanelActive(false);
    }

    private GameObject FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(objectName))
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

    private bool IsPointerInsideDialoguePanel()
    {
        RectTransform panelRect = transform as RectTransform;

        if (panelRect == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(panelRect, Input.mousePosition, GetCanvasCamera());
    }

    private Camera GetCanvasCamera()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return canvas.worldCamera;
    }

    private void BlockOutsideClickBriefly()
    {
        _outsideClickReadyTime = Time.unscaledTime + _outsideClickDelay;
    }

    /// <summary>
    /// Road View 대화창이 HUD보다 앞에 보이도록 Canvas 정렬 순서를 올립니다.
    /// </summary>
    private void RaiseCanvasForRoadViewDialogue()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            return;

        canvas.overrideSorting = true;
        canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, _roadViewSortingOrder);
    }

    // ==================== 스크롤 갱신 ====================

    /// <summary>
    /// TMP 텍스트의 실제 선호 높이를 Content에 반영합니다.
    /// 이 보정이 없으면 긴 나레이션이 마스크 밖으로 밀려 보이거나 Content 높이가 0으로 남을 수 있습니다.
    /// </summary>
    /// <summary>
    /// 본문 내용 길이에 맞춰 TMP 텍스트와 스크롤 Content 높이를 갱신합니다.
    /// </summary>
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
        dialogueTextRect.localScale = Vector3.one;

        dialogueTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
        Text_Dialogue.ForceMeshUpdate();

        float preferredHeight = Text_Dialogue.GetPreferredValues(Text_Dialogue.text, contentWidth, 0f).y;
        float finalHeight = Mathf.Max(preferredHeight + _scrollContentPadding, 80f);

        dialogueTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight);

        if (Rect_DialogueContent != null)
        {
            Rect_DialogueContent.anchorMin = new Vector2(0f, 1f);
            Rect_DialogueContent.anchorMax = new Vector2(1f, 1f);
            Rect_DialogueContent.pivot = new Vector2(0.5f, 1f);
            Rect_DialogueContent.anchoredPosition = Vector2.zero;
            Rect_DialogueContent.localScale = Vector3.one;
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

    /// <summary>
    /// Vertical Layout Group과 Content Size Fitter가 계산한 높이를 즉시 반영합니다.
    /// Rect_DialogueContent를 연결하지 않은 경우에도 기존 대화 출력은 계속 동작합니다.
    /// </summary>
    private void RebuildScrollContent()
    {
        Canvas.ForceUpdateCanvases();

        if (Rect_DialogueContent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(Rect_DialogueContent);

        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// 새 문장이 표시될 때 스크롤을 맨 위로 올립니다.
    /// </summary>
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

        if (Rect_DialogueContent != null)
            Rect_DialogueContent.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// DialoguePanel이 켜진 직후에는 RectTransform 폭과 높이가 다음 프레임에 확정될 수 있습니다.
    /// 그래서 한 프레임 뒤에 같은 보정을 한 번 더 수행해 줄거리가 빈칸처럼 보이는 상황을 막습니다.
    /// </summary>
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
