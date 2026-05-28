using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// DialogueUI
/// 대화창의 화자 이름, 본문, 다음 버튼을 관리합니다.
/// 일반 대화 데이터와 프롤로그 나레이션 데이터를 모두 표시할 수 있도록 구성했습니다.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("UI 참조")]
    [FormerlySerializedAs("_speakerNameText")]
    [SerializeField] private TextMeshProUGUI Text_SpeakerName;

    [FormerlySerializedAs("_dialogueText")]
    [SerializeField] private TextMeshProUGUI Text_Dialogue;

    [FormerlySerializedAs("_nextButton")]
    [SerializeField] private Button Button_Next;

    // ==================== 대화 상태 ====================
    private OO_Dialogue _currentDialogue;
    private readonly List<string> _narrationTextList = new List<string>();
    private int _currentNarrationTextIndex;
    private Action _onDialogueEnd;

    private void OnEnable()
    {
        BindButtonEvent();
    }

    private void OnDisable()
    {
        UnbindButtonEvent();
    }

    // ==================== 버튼 바인딩 ====================

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

    private void AddNarrationTextList(List<string> narrationTexts)
    {
        if (narrationTexts == null)
            return;

        foreach (string narrationText in narrationTexts)
        {
            if (string.IsNullOrWhiteSpace(narrationText))
                continue;

            _narrationTextList.Add(narrationText.Trim());
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
    /// 다음 버튼 클릭 시 나레이션 다음 문장 또는 다음 컷씬으로 진행합니다.
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
        {
            Debug.Log($"[DialogueUI] 다음 대화 ID가 지정되어 있습니다: {_currentDialogue.NextDialogueId}");
            FinishDialogue();
            return;
        }

        FinishDialogue();
    }

    // ==================== 대화창 닫기 ====================

    /// <summary>
    /// 외부에서 대화창을 강제로 닫을 때 사용합니다.
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
            Text_Dialogue.text = dialogueText ?? string.Empty;
    }

    private void SetNextButtonActive(bool isActive)
    {
        if (Button_Next != null)
            Button_Next.gameObject.SetActive(isActive);
    }
}
