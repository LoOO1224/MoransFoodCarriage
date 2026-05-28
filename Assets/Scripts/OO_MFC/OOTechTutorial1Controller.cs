using System.Collections;
using UnityEngine;

/// <summary>
/// Tutorial1Group의 시작 흐름을 관리합니다.
/// 그룹이 열리면 장영심 이동을 잠그고 TutorialGuideGroup을 UIManager에 등록한 뒤, 가이드 종료 후 이동을 해제합니다.
/// </summary>
public class OOTechTutorial1Controller : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private JangYoungSimController Character_JangYoungSim;

    [Header("Tutorial Guide UI")]
    [SerializeField] private GameObject Group_TutorialGuide;
    [SerializeField] private OOTechTutorialGuideUI UI_TutorialGuide;

    [Header("UI Group Names")]
    [SerializeField] private string _tutorialGuideGroupName = "TutorialGuideGroup";

    [Header("Narration Id")]
    [SerializeField] private string _tutorialNarrationId = "narration_tutorial_01";

    [Header("Start Rule")]
    [SerializeField] private bool _isShowGuideOnEnable = true;
    [SerializeField] private bool _isLockPlayerUntilGuideEnd = true;

    // ==================== 튜토리얼 상태 ====================
    private Coroutine _openGuideCoroutine;
    private bool _isGuideFinished;

    private void OnEnable()
    {
        StartTutorial();
    }

    private void OnDisable()
    {
        StopOpenGuideCoroutine();
        CloseTutorialGuide();
    }

    // ==================== 튜토리얼 시작 ====================

    /// <summary>
    /// Tutorial1Group이 열릴 때 가이드 표시와 플레이어 이동 잠금을 시작합니다.
    /// </summary>
    private void StartTutorial()
    {
        _isGuideFinished = false;

        if (_isLockPlayerUntilGuideEnd)
            LockPlayerMovement();

        if (!_isShowGuideOnEnable)
        {
            FinishTutorialGuide();
            return;
        }

        StopOpenGuideCoroutine();
        _openGuideCoroutine = StartCoroutine(OpenTutorialGuideRoutine());
    }

    private IEnumerator OpenTutorialGuideRoutine()
    {
        yield return null;

        while (OOTechUIManager.Inst == null || OOTechGameDataManager.Inst == null)
            yield return null;

        RegisterTutorialGuideGroup();
        OpenTutorialGuide();

        _openGuideCoroutine = null;
    }

    private void RegisterTutorialGuideGroup()
    {
        if (OOTechUIManager.Inst == null)
            return;

        if (Group_TutorialGuide == null)
        {
            Debug.LogError("[OOTechTutorial1Controller] TutorialGuideGroup 직접 참조가 연결되어 있지 않습니다.");
            return;
        }

        OOTechUIManager.Inst.RegisterUI(_tutorialGuideGroupName, Group_TutorialGuide);
    }

    private void OpenTutorialGuide()
    {
        if (OOTechUIManager.Inst == null)
            return;

        if (UI_TutorialGuide == null)
        {
            Debug.LogError("[OOTechTutorial1Controller] OOTechTutorialGuideUI 직접 참조가 연결되어 있지 않습니다.");
            FinishTutorialGuide();
            return;
        }

        OO_Narration narrationData = OOTechGameDataManager.Inst.GetNarrationData(_tutorialNarrationId);
        if (narrationData == null)
        {
            Debug.LogWarning($"[OOTechTutorial1Controller] 튜토리얼 가이드 데이터를 찾을 수 없습니다: {_tutorialNarrationId}");
            FinishTutorialGuide();
            return;
        }

        if (!OOTechUIManager.Inst.OpenUI(_tutorialGuideGroupName))
        {
            Debug.LogError($"[OOTechTutorial1Controller] TutorialGuideGroup을 열 수 없습니다: {_tutorialGuideGroupName}");
            FinishTutorialGuide();
            return;
        }

        UI_TutorialGuide.ShowGuide(narrationData, OnTutorialGuideEnd);
    }

    // ==================== 플레이어 이동 제어 ====================

    private void LockPlayerMovement()
    {
        if (Character_JangYoungSim != null)
            Character_JangYoungSim.LockMovement();
    }

    private void UnlockPlayerMovement()
    {
        if (Character_JangYoungSim != null)
            Character_JangYoungSim.UnlockMovement();
    }

    // ==================== 가이드 종료 ====================

    private void OnTutorialGuideEnd()
    {
        FinishTutorialGuide();
    }

    private void FinishTutorialGuide()
    {
        if (_isGuideFinished)
            return;

        _isGuideFinished = true;

        CloseTutorialGuide();

        if (_isLockPlayerUntilGuideEnd)
            UnlockPlayerMovement();
    }

    private void CloseTutorialGuide()
    {
        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.CloseUI(_tutorialGuideGroupName);
    }

    private void StopOpenGuideCoroutine()
    {
        if (_openGuideCoroutine == null)
            return;

        StopCoroutine(_openGuideCoroutine);
        _openGuideCoroutine = null;
    }
}
