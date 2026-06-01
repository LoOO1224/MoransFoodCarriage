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
    [Header("Text")]
    [FormerlySerializedAs("_speakerNameText")]
    [SerializeField] private TextMeshProUGUI Text_SpeakerName;

    [FormerlySerializedAs("_dialogueText")]
    [SerializeField] private TextMeshProUGUI Text_Dialogue;

    [Header("Button")]
    [FormerlySerializedAs("_nextButton")]
    [SerializeField] private Button Button_Next;

    [Header("Scroll")]
    [SerializeField] private ScrollRect Scroll_Dialogue;
    [SerializeField] private RectTransform Rect_DialogueContent;
    [SerializeField] private float _scrollContentPadding = 16f;

    // ==================== 대화 상태 ====================
    private OO_Dialogue _currentDialogue;
    private readonly List<string> _narrationTextList = new List<string>();
    private int _currentNarrationTextIndex;
    private Action _onDialogueEnd;
    private Coroutine _refreshScrollCoroutine;
    private OOTechDialogueLayout Layout_Dialogue;
    private OOTechDialogueSpeakerNameBackdrop Backdrop_SpeakerName;

    private void Awake()
    {
        ApplyProjectFont();
        PrepareSpeakerNameBackdrop();
        ApplyDialogueLayout();
    }

    private void OnEnable()
    {
        ApplyProjectFont();
        PrepareSpeakerNameBackdrop();
        ApplyDialogueLayout();
        BindButtonEvent();
    }

    private void OnDisable()
    {
        UnbindButtonEvent();
        StopRefreshScrollCoroutine();
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

    private void ApplyProjectFont()
    {
        OOTechTMPFontUtility.ApplyProjectFont(Text_SpeakerName);
        OOTechTMPFontUtility.ApplyProjectFont(Text_Dialogue);
    }

    private void PrepareSpeakerNameBackdrop()
    {
        if (Backdrop_SpeakerName == null)
            Backdrop_SpeakerName = GetComponent<OOTechDialogueSpeakerNameBackdrop>();

        if (Backdrop_SpeakerName == null)
            Backdrop_SpeakerName = gameObject.AddComponent<OOTechDialogueSpeakerNameBackdrop>();
    }

    private void BindButtonEvent()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(NextDialogue);
        Button_Next.onClick.AddListener(NextDialogue);
    }

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

        gameObject.SetActive(true);

        SetSpeakerName(dialogueData.SpeakerName);
        SetDialogueText(dialogueData.Text);
        SetNextButtonActive(dialogueData.SelectionNameList == null || dialogueData.SelectionNameList.Count == 0);
    }

    // ==================== 나레이션 표시 ====================

    /// <summary>
    /// OO_Narration 데이터를 기반으로 나레이션을 표시합니다.
    /// SpeakerNameText는 요구사항에 따라 항상 "나레이션"으로 고정합니다.
    /// </summary>
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

        AddNarrationTextList(narrationData.NarrationTexts);

        if (_narrationTextList.Count == 0)
        {
            Debug.LogWarning($"[DialogueUI] 나레이션 텍스트가 비어 있습니다: {narrationData.Id}");
            FinishDialogue();
            return;
        }

        gameObject.SetActive(true);

        SetSpeakerName("나레이션");
        SetNextButtonActive(true);
        ShowCurrentNarrationText();
    }

    /// <summary>
    /// 나레이션 텍스트를 표시 단위로 정리합니다.
    /// 기본은 JSON의 한 항목을 한 번에 보여주며, 필요하면 텍스트 안의 &lt;np&gt; 태그로 페이지를 나눌 수 있습니다.
    /// </summary>
    private void AddNarrationTextList(List<string> narrationTexts)
    {
        if (narrationTexts == null)
            return;

        foreach (string narrationText in narrationTexts)
            AddNarrationText(narrationText);
    }

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
        if (_narrationTextList.Count > 0)
        {
            MoveNextNarrationText();
            return;
        }

        MoveNextDialogue();
    }

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
        ClearDialogueState();
        gameObject.SetActive(false);
    }

    private void FinishDialogue()
    {
        Action onDialogueEnd = _onDialogueEnd;
        ClearDialogueState();
        onDialogueEnd?.Invoke();
    }

    private void ClearDialogueState()
    {
        _currentDialogue = null;
        _narrationTextList.Clear();
        _currentNarrationTextIndex = 0;
        _onDialogueEnd = null;
    }

    // ==================== UI 값 설정 ====================

    private void SetSpeakerName(string speakerName)
    {
        if (Text_SpeakerName != null)
            Text_SpeakerName.text = string.IsNullOrEmpty(speakerName) ? "나레이션" : speakerName;
    }

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

    private void SetNextButtonActive(bool isActive)
    {
        if (Button_Next != null)
            Button_Next.gameObject.SetActive(isActive);
    }

    // ==================== 스크롤 갱신 ====================

    /// <summary>
    /// TMP 텍스트의 실제 선호 높이를 Content에 반영합니다.
    /// 이 보정이 없으면 긴 나레이션이 마스크 밖으로 밀려 보이거나 Content 높이가 0으로 남을 수 있습니다.
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
