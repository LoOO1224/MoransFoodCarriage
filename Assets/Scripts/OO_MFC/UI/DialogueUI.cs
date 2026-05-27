using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI _speakerNameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private Button _nextButton;

    private OO_Dialogue _currentDialogue;
    private System.Action _onDialogueEnd;

    private void Awake()
    {
        if (_nextButton != null)
            _nextButton.onClick.AddListener(NextDialogue);
    }

    // ==================== 대화 시작 ====================
    public void ShowDialogue(OO_Dialogue dialogueData, System.Action onEnd = null)
    {
        _currentDialogue = dialogueData;
        _onDialogueEnd = onEnd;

        gameObject.SetActive(true);

        if (_speakerNameText != null)
            _speakerNameText.text = dialogueData.SpeakerName;

        if (_dialogueText != null)
            _dialogueText.text = dialogueData.Text;

        // 선택지가 있으면 다음 버튼 숨기기 (나중에 확장)
        if (_nextButton != null)
            _nextButton.gameObject.SetActive(dialogueData.SelectionNameList.Count == 0);
    }

    // ==================== 다음 대화로 이동 ====================
    public void NextDialogue()
    {
        if (_currentDialogue == null) return;

        if (!string.IsNullOrEmpty(_currentDialogue.NextDialogueId))
        {
            // TODO: DialogueGroupData에서 다음 대화 찾아서 ShowDialogue 호출
            Debug.Log($"[DialogueUI] 다음 대화 ID: {_currentDialogue.NextDialogueId}");
        }
        else
        {
            // 대화 종료
            CloseDialogue();
        }
    }

    // ==================== 대화창 닫기 ====================
    public void CloseDialogue()
    {
        gameObject.SetActive(false);
        _currentDialogue = null;

        if (_onDialogueEnd != null)
        {
            _onDialogueEnd.Invoke();
            _onDialogueEnd = null;
        }
    }
}