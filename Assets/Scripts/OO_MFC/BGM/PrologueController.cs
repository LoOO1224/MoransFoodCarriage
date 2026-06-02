using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Prologue1Group, Prologue2Group처럼 프롤로그 그룹에 붙어서 컷씬과 나레이션 진행을 관리합니다.
/// 컷씬 오브젝트와 DialogueUI는 인스펙터 직접 참조만 사용하며, UI 열기와 닫기는 OOTechUIManager를 통해 처리합니다.
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
    [SerializeField] private Button Button_OpenDialogueArea;

    [Header("UI Group Names")]
    [SerializeField] private string _prologueGroupName = "Prologue1Group";
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private string _tutorialGroupName = "Tutorial1Group";

    [Header("Dialogue Open Rule")]
    [FormerlySerializedAs("_isRequireFirstClickToShowDialogue")]
    [SerializeField] private bool _isRequireClickToShowDialogue = true;
    [SerializeField] private bool _isAllowScreenClickToShowDialogue = true;
    [SerializeField] private bool _isIgnorePointerOverUI = true;
    [SerializeField] private bool _isCloseDialogueGroupOnDisable = true;

    [Header("Narration Id")]
    [SerializeField] private string[] _narrationIdArray =
    {
        "narration_prologue_01",
        "narration_prologue_02",
        "narration_prologue_03"
    };

    // ==================== 프롤로그 상태 ====================
    private int _currentCutSceneIndex;
    private int _dialogueOpenBlockFrame;
    private bool _isWaitingDialogueOpen;

    /// <summary>
    /// 프롤로그 무대가 켜지면 버튼을 연결하고 첫 컷씬을 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        BindButtonEvent();
        StartPrologue();
    }

    /// <summary>
    /// 클릭으로 대화를 열어야 하는 컷씬인지 매 프레임 확인합니다.
    /// </summary>
    private void Update()
    {
        UpdateScreenClickToOpenDialogue();
    }

    /// <summary>
    /// 프롤로그 무대가 닫히면 버튼 이벤트와 DialogueGroup을 정리합니다.
    /// </summary>
    private void OnDisable()
    {
        UnbindButtonEvent();
        CloseDialogueGroupOnDisabled();
    }

    // ==================== 버튼 바인딩 ====================

    /// <summary>
    /// 스킵 버튼과 컷씬 클릭 영역 버튼을 현재 프롤로그 이벤트에 연결합니다.
    /// </summary>
    private void BindButtonEvent()
    {
        if (Button_Skip != null)
        {
            Button_Skip.onClick.RemoveListener(OnSkipButtonClicked);
            Button_Skip.onClick.AddListener(OnSkipButtonClicked);
        }

        if (Button_OpenDialogueArea != null)
        {
            Button_OpenDialogueArea.onClick.RemoveListener(OnOpenDialogueAreaClicked);
            Button_OpenDialogueArea.onClick.AddListener(OnOpenDialogueAreaClicked);
        }
    }

    /// <summary>
    /// 버튼 이벤트를 해제해 다시 열릴 때 중복 호출되지 않게 합니다.
    /// </summary>
    private void UnbindButtonEvent()
    {
        if (Button_Skip != null)
            Button_Skip.onClick.RemoveListener(OnSkipButtonClicked);

        if (Button_OpenDialogueArea != null)
            Button_OpenDialogueArea.onClick.RemoveListener(OnOpenDialogueAreaClicked);
    }

    // ==================== 프롤로그 시작 ====================

    /// <summary>
    /// 프롤로그가 켜질 때 첫 컷씬을 먼저 보여주고, 클릭 후 DialogueGroup과 나레이션을 표시합니다.
    /// </summary>
    private void StartPrologue()
    {
        _currentCutSceneIndex = 0;

        Debug.Log("[PrologueController] 프롤로그 시작");

        ShowCutSceneThenWaitDialogueOpen();
    }

    /// <summary>
    /// 현재 컷씬 오브젝트만 켜고 나머지는 끕니다.
    /// </summary>
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

    // ==================== 컷씬 후 대화 열기 ====================

    /// <summary>
    /// 현재 컷씬을 먼저 전체 화면으로 보여준 뒤 DialogueGroup을 닫고 클릭 입력을 기다립니다.
    /// Prologue1CutScene2, Prologue1CutScene3도 같은 규칙으로 배경을 먼저 보여줍니다.
    /// </summary>
    private void ShowCutSceneThenWaitDialogueOpen()
    {
        ShowCurrentCutScene();

        if (!_isRequireClickToShowDialogue)
        {
            OpenDialogueForCurrentCutScene();
            return;
        }

        CloseDialogueForCutSceneView();

        _isWaitingDialogueOpen = true;
        _dialogueOpenBlockFrame = Time.frameCount;
    }

    /// <summary>
    /// 컷씬 전체 화면을 먼저 보여주기 위해 DialogueGroup과 DialoguePanel을 함께 닫습니다.
    /// </summary>
    private void CloseDialogueForCutSceneView()
    {
        if (UI_Dialogue != null)
            UI_Dialogue.CloseDialogue();

        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.CloseUI(_dialogueGroupName);
    }

    /// <summary>
    /// 화면 클릭이 들어오면 현재 컷씬의 DialogueGroup을 엽니다.
    /// </summary>
    private void UpdateScreenClickToOpenDialogue()
    {
        if (!_isWaitingDialogueOpen)
            return;

        if (!_isAllowScreenClickToShowDialogue)
            return;

        if (Time.frameCount <= _dialogueOpenBlockFrame)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (IsPointerOverUI())
            return;

        OpenDialogueForCurrentCutScene();
    }

    private bool IsPointerOverUI()
    {
        return _isIgnorePointerOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// 컷씬 위에 투명 버튼을 둘 경우 해당 버튼의 OnClick에 연결해서 현재 컷씬의 나레이션을 엽니다.
    /// </summary>
    public void OnOpenDialogueAreaClicked()
    {
        if (!_isWaitingDialogueOpen)
            return;

        OpenDialogueForCurrentCutScene();
    }

    private void OpenDialogueForCurrentCutScene()
    {
        _isWaitingDialogueOpen = false;
        ShowCurrentNarration();
    }

    // ==================== 나레이션 진행 ====================

    /// <summary>
    /// 현재 컷씬 번호에 맞는 Narration ID를 가져와 DialogueUI에 표시합니다.
    /// </summary>
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

        if (UI_Dialogue == null)
        {
            Debug.LogError("[PrologueController] DialogueUI 직접 참조가 연결되어 있지 않습니다.");
            return;
        }

        if (OOTechUIManager.Inst == null)
        {
            Debug.LogError("[PrologueController] OOTechUIManager를 찾을 수 없어 DialogueGroup을 열 수 없습니다.");
            return;
        }

        if (!OOTechUIManager.Inst.OpenUI(_dialogueGroupName))
        {
            Debug.LogError($"[PrologueController] DialogueGroup을 열 수 없습니다: {_dialogueGroupName}");
            return;
        }

        UI_Dialogue.ShowNarration(narrationData, OnNarrationEnd);
    }

    private string GetCurrentNarrationId()
    {
        if (_narrationIdArray == null)
            return string.Empty;

        if (_currentCutSceneIndex < 0 || _currentCutSceneIndex >= _narrationIdArray.Length)
            return string.Empty;

        return _narrationIdArray[_currentCutSceneIndex];
    }

    private void OnNarrationEnd()
    {
        MoveNextCutSceneOrComplete();
    }

    /// <summary>
    /// 나레이션이 끝나면 다음 컷씬 또는 다음 그룹으로 진행합니다.
    /// </summary>
    private void MoveNextCutSceneOrComplete()
    {
        _currentCutSceneIndex++;

        if (_currentCutSceneIndex < GetCutSceneCount())
        {
            ShowCutSceneThenWaitDialogueOpen();
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
        Debug.Log($"[PrologueController] 프롤로그 완료 → {_tutorialGroupName}으로 이동");

        if (OOTechUIManager.Inst == null)
        {
            Debug.LogError("[PrologueController] OOTechUIManager를 찾을 수 없습니다.");
            return;
        }

        OOTechUIManager.Inst.CloseUI(_dialogueGroupName);
        OOTechUIManager.Inst.CloseUI(_prologueGroupName);

        if (!OOTechUIManager.Inst.OpenUI(_tutorialGroupName))
            OOTechUIManager.Inst.OpenUI("TutorialGroup");
    }

    private void CloseDialogueGroupOnDisabled()
    {
        if (!_isCloseDialogueGroupOnDisable)
            return;

        if (OOTechUIManager.Inst == null)
            return;

        OOTechUIManager.Inst.CloseUI(_dialogueGroupName);
    }

    // ==================== 버튼 이벤트 ====================

    /// <summary>
    /// Dialogue NextButton을 외부에서 직접 연결한 경우에도 동작하도록 제공하는 이벤트 함수입니다.
    /// 기본 흐름에서는 DialogueUI.Button_Next가 DialogueUI.NextDialogue를 직접 호출합니다.
    /// </summary>
    public void OnNextDialogueClicked()
    {
        if (UI_Dialogue != null)
            UI_Dialogue.NextDialogue();
    }

    /// <summary>
    /// SkipButton 클릭 시 현재 컷씬을 건너뛰고 다음 컷씬 또는 튜토리얼로 이동합니다.
    /// CommonSkipButton은 NextButtonController를 사용하지만, 기존 직접 연결 버튼과도 호환되도록 유지합니다.
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

    /// <summary>
    /// 현재 컷씬을 스킵하고 다음 컷씬 또는 튜토리얼로 진행합니다.
    /// </summary>
    private void SkipCurrentCutScene()
    {
        Debug.Log($"[PrologueController] 컷씬 스킵: {_currentCutSceneIndex + 1}");

        if (UI_Dialogue != null)
            UI_Dialogue.CloseDialogue();

        MoveNextCutSceneOrComplete();
    }
}
