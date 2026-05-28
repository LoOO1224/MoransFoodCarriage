using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// PrologueController
/// Prologue1Group에 붙어서 프롤로그 컷씬 3개와 나레이션 진행을 관리합니다.
/// UI 오픈 / 클로즈는 OOTechUIManager를 통해 처리하고,
/// 나레이션 데이터는 OOTechGameDataManager에서 가져옵니다.
/// </summary>
public class PrologueController : MonoBehaviour
{
    [Header("Prologue CutScenes")]
    [FormerlySerializedAs("_cutScene1")]
    [SerializeField] private GameObject CutScene_01;

    [FormerlySerializedAs("_cutScene2")]
    [SerializeField] private GameObject CutScene_02;

    [FormerlySerializedAs("_cutScene3")]
    [SerializeField] private GameObject CutScene_03;

    [Header("Dialogue UI")]
    [SerializeField] private DialogueUI UI_Dialogue;

    [Header("Button")]
    [SerializeField] private Button Button_Skip;

    [Header("UI Group Names")]
    [SerializeField] private string _prologueGroupName = "Prologue1Group";
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private string _tutorialGroupName = "TutorialGroup";

    [Header("Narration Id")]
    [SerializeField] private string[] _narrationIdArray =
    {
        "narration_prologue_01",
        "narration_prologue_02",
        "narration_prologue_03"
    };

    // ==================== 프롤로그 상태 ====================
    private int _currentCutSceneIndex;

    private void OnEnable()
    {
        BindButtonEvent();
        StartPrologue();
    }

    private void OnDisable()
    {
        UnbindButtonEvent();
    }

    // ==================== 버튼 바인딩 ====================

    private void BindButtonEvent()
    {
        if (Button_Skip == null)
            return;

        Button_Skip.onClick.RemoveListener(OnSkipButtonClicked);
        Button_Skip.onClick.AddListener(OnSkipButtonClicked);
    }

    private void UnbindButtonEvent()
    {
        if (Button_Skip == null)
            return;

        Button_Skip.onClick.RemoveListener(OnSkipButtonClicked);
    }

    // ==================== 프롤로그 시작 ====================

    private void StartPrologue()
    {
        _currentCutSceneIndex = 0;

        Debug.Log("[PrologueController] 프롤로그 시작");

        ShowCurrentCutScene();
        ShowCurrentNarration();
    }

    private void ShowCurrentCutScene()
    {
        SetCutSceneActive(CutScene_01, _currentCutSceneIndex == 0);
        SetCutSceneActive(CutScene_02, _currentCutSceneIndex == 1);
        SetCutSceneActive(CutScene_03, _currentCutSceneIndex == 2);
    }

    private void SetCutSceneActive(GameObject cutScene, bool isActive)
    {
        if (cutScene != null)
            cutScene.SetActive(isActive);
    }

    // ==================== 나레이션 진행 ====================

    private void ShowCurrentNarration()
    {
        string narrationId = GetCurrentNarrationId();

        if (string.IsNullOrEmpty(narrationId))
        {
            Debug.LogWarning($"[PrologueController] 현재 컷씬에 연결된 나레이션 ID가 없습니다. Index: {_currentCutSceneIndex}");
            CompletePrologue();
            return;
        }

        if (OOTechGameDataManager.Inst == null)
        {
            Debug.LogError("[PrologueController] OOTechGameDataManager를 찾을 수 없습니다.");
            return;
        }

        OO_Narration narrationData = OOTechGameDataManager.Inst.GetNarrationData(narrationId);
        if (narrationData == null)
            return;

        DialogueUI dialogueUI = GetDialogueUI();
        if (dialogueUI == null)
        {
            Debug.LogError("[PrologueController] DialogueUI 참조를 찾을 수 없습니다.");
            return;
        }

        OOTechUIManager.Inst?.OpenUI(_dialogueGroupName);
        dialogueUI.ShowNarration(narrationData, OnNarrationEnd);
    }

    private string GetCurrentNarrationId()
    {
        if (_narrationIdArray == null)
            return string.Empty;

        if (_currentCutSceneIndex < 0 || _currentCutSceneIndex >= _narrationIdArray.Length)
            return string.Empty;

        return _narrationIdArray[_currentCutSceneIndex];
    }

    private DialogueUI GetDialogueUI()
    {
        if (UI_Dialogue != null)
            return UI_Dialogue;

        if (OOTechUIManager.Inst == null)
            return null;

        GameObject dialogueGroup = OOTechUIManager.Inst.GetCreatedUI(_dialogueGroupName);
        if (dialogueGroup == null)
            return null;

        UI_Dialogue = dialogueGroup.GetComponentInChildren<DialogueUI>(true);
        return UI_Dialogue;
    }

    private void OnNarrationEnd()
    {
        MoveNextCutSceneOrComplete();
    }

    private void MoveNextCutSceneOrComplete()
    {
        _currentCutSceneIndex++;

        if (_currentCutSceneIndex < GetCutSceneCount())
        {
            ShowCurrentCutScene();
            ShowCurrentNarration();
            return;
        }

        CompletePrologue();
    }

    private int GetCutSceneCount()
    {
        if (_narrationIdArray == null)
            return 0;

        return _narrationIdArray.Length;
    }

    // ==================== 프롤로그 종료 ====================

    private void CompletePrologue()
    {
        Debug.Log("[PrologueController] 프롤로그 완료 → TutorialGroup으로 이동");

        if (OOTechUIManager.Inst == null)
        {
            Debug.LogError("[PrologueController] OOTechUIManager를 찾을 수 없습니다.");
            return;
        }

        OOTechUIManager.Inst.CloseUI(_dialogueGroupName);
        OOTechUIManager.Inst.CloseUI(_prologueGroupName);

        if (!OOTechUIManager.Inst.OpenUI(_tutorialGroupName))
            OOTechUIManager.Inst.OpenUI("Tutorial1Group");
    }

    // ==================== 버튼 이벤트 ====================

    /// <summary>
    /// Dialogue NextButton을 외부에서 직접 연결한 경우에도 동작하도록 제공하는 이벤트 함수입니다.
    /// 기본 흐름에서는 DialogueUI.Button_Next가 DialogueUI.NextDialogue를 직접 호출합니다.
    /// </summary>
    public void OnNextDialogueClicked()
    {
        DialogueUI dialogueUI = GetDialogueUI();
        if (dialogueUI != null)
            dialogueUI.NextDialogue();
    }

    /// <summary>
    /// SkipButton 클릭 시 현재 컷씬을 건너뛰고 다음 컷씬 또는 튜토리얼로 이동합니다.
    /// </summary>
    public void OnSkipButtonClicked()
    {
        SkipCurrentCutScene();
    }

    /// <summary>
    /// 기존 인스펙터 이벤트 이름과의 호환을 위한 메서드입니다.
    /// </summary>
    public void OnSkipClicked()
    {
        SkipCurrentCutScene();
    }

    private void SkipCurrentCutScene()
    {
        Debug.Log($"[PrologueController] 컷씬 스킵: {_currentCutSceneIndex + 1}");

        DialogueUI dialogueUI = GetDialogueUI();
        if (dialogueUI != null)
            dialogueUI.CloseDialogue();

        MoveNextCutSceneOrComplete();
    }
}
