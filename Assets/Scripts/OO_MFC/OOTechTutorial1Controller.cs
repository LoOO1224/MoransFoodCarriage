// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechTutorial1Controller.cs
// - 역할: 해당 그룹의 튜토리얼/시나리오 진행 순서를 담당하는 장면 Controller입니다.
// - 감독 관점: 배우 등장, 대사, 카메라 포커스, 다음 장면 이동을 큐시트 순서대로 지휘합니다.
// - 유지보수 포인트: 캐릭터 이동/아이템/버튼 생성 같은 세부 책임은 별도 컴포넌트로 분리해야 합니다.
// =============================================================================
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tutorial1Group의 전체 진행을 관리합니다.
/// 시작 가이드, 플레이어 이동 잠금, 모란 상호작용, 캐릭터 대화, 임무 완료 가이드, 다음 그룹 버튼 표시를 순서대로 처리합니다.
/// </summary>
public class OOTechTutorial1Controller : MonoBehaviour
{
    // 읽는 순서:
    // 1. OnEnable/Start 계열: Tutorial1Group 진입 시 Moran, JangYoungSim, 튜토리얼 UI를 준비합니다.
    // 2. Guide 관련 메서드: 안내 문구를 데이터 드리븐으로 띄우고 캐릭터 조작 잠금을 풉니다.
    // 3. Interaction 관련 메서드: Moran_WakeUp 근처에서 E 버튼 표시와 상호작용을 처리합니다.
    // 4. Animation 관련 메서드: Surprised 애니메이션 완료 후 마지막 프레임 정지 같은 상태를 처리합니다.
    // 5. Skip 관련 메서드: 배경 안에 있는 CommonSkipButton으로 다음 그룹 이동을 처리합니다.
    // 유지보수 주의:
    // - Tutorial1Group 전용 Moran_WakeUp은 다른 그룹에 섞이면 안 됩니다.
    // - E 버튼은 화면 전체가 아니라 대상 근처/고정 UI 위치에서 작게 보여야 합니다.
    // - 튜토리얼 UI 배치는 하이어라키에서 직접 수정하고, 코드 생성은 줄입니다.

    [Header("Character")]
    [SerializeField] private JangYoungSimController Character_JangYoungSim;
    [SerializeField] private Transform Transform_JangYoungSim;
    [SerializeField] private Transform Transform_Moran;
    [SerializeField] private SpriteRenderer Renderer_Moran;
    [SerializeField] private Animator Animator_Moran;

    [Header("Tutorial Guide UI")]
    [SerializeField] private GameObject Group_TutorialGuide;
    [SerializeField] private OOTechTutorialGuideUI UI_TutorialGuide;

    [Header("Dialogue UI")]
    [SerializeField] private DialogueUI UI_Dialogue;

    [Header("Common Skip Button")]
    [SerializeField] private GameObject Prefab_CommonSkipButton;

    [Header("UI Group Names")]
    [SerializeField] private string _tutorialGroupName = "Tutorial1Group";
    [SerializeField] private string _tutorialGuideGroupName = "TutorialGuideGroup";
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private string _nextGroupName = "Prologue2Group";

    [Header("Data Id")]
    [SerializeField] private string _tutorialNarrationId = "narration_tutorial_01";
    [SerializeField] private string _completeTutorialId = "narration_tutorial_02";
    [SerializeField] private string _moranDialogueId = "character_Moran_01";
    [SerializeField] private string _jangYoungSimDialogueId = "character_JangYoungSim_01";

    [Header("Start Rule")]
    [SerializeField] private bool _isShowGuideOnEnable = true;
    [SerializeField] private bool _isLockPlayerUntilGuideEnd = true;

    [Header("Camera View")]
    [SerializeField] private Camera Camera_Main;
    [SerializeField] private CameraFollowController Camera_Follow;
    [SerializeField] private string _tutorialBackgroundName = "Tutorial1Background";
    [SerializeField] private float _tutorialCameraPadding = 1.02f;
    [SerializeField] private float _fallbackTutorialCameraSize = 8f;

    [Header("Interaction Rule")]
    [SerializeField] private float _interactionDistance = 2.2f;
    [SerializeField] private float _minimumInteractionDistance = 3f;
    [SerializeField] private KeyCode _interactionKey = KeyCode.E;
    [SerializeField] private float _interactionAnimationWaitSeconds = 2.1f;
    [SerializeField] private float _interactionAnimationSpeed = 0.7f;

    [Header("Animation State Names")]
    [SerializeField] private string _moranWakeUpStateName = "Moran_WakeUp";
    [SerializeField] private bool _isDisableMoranAnimatorUntilInteraction = true;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject Group_EButton;
    [SerializeField] private RectTransform Rect_EButton;
    [SerializeField] private RectTransform Rect_EButtonTextParent;
    [SerializeField] private CanvasGroup CanvasGroup_EButton;
    [SerializeField] private TextMeshProUGUI Text_EButton;
    [SerializeField] private string _eButtonText = "E";
    [SerializeField] private Vector3 _eButtonWorldOffset = new Vector3(0f, 2.1f, 0f);
    [SerializeField] private float _eButtonBlinkSpeed = 6f;
    [SerializeField] private float _eButtonMinAlpha = 0.35f;
    [SerializeField] private float _eButtonMaxAlpha = 1f;
    [SerializeField] private float _eButtonScalePower = 0.12f;
    [SerializeField] private float _eButtonMinimumWorldScale = 0.018f;
    [SerializeField] private int _eButtonSortingOrder = 9000;
    [SerializeField] private Vector2 _eButtonReferenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private Vector2 _eButtonScreenAnchor = new Vector2(0.78f, 0.28f);
    [SerializeField] private float _eButtonFixedPixelSize = 120f;
    [SerializeField] private float _eButtonMoranSizeRatio = 0.5f;
    [SerializeField] private float _eButtonMinimumPixelSize = 36f;
    [SerializeField] private float _eButtonMaximumPixelSize = 180f;
    [SerializeField] private string _interactionPromptText = "[E] 깨우기";
    [SerializeField] private Vector3 _interactionPromptOffset = new Vector3(0f, 1.4f, 0f);
    [SerializeField] private float _interactionPromptScale = 0.25f;
    [SerializeField] private float _interactionPromptFontSize = 5f;
    [SerializeField] private Color _interactionPromptColor = new Color(1f, 0.86f, 0.16f, 1f);
    [SerializeField] private int _interactionPromptSortingOrder = 40;

    [Header("Interaction Flash Effect")]
    [SerializeField] private bool _isUseInteractionFlash = true;
    [SerializeField] private Color _interactionFlashColor = new Color(1f, 1f, 1f, 0.78f);
    [SerializeField] private int _interactionFlashCount = 2;
    [SerializeField] private float _interactionFlashFadeSeconds = 0.08f;
    [SerializeField] private int _interactionFlashSortingOrder = 190;

    [Header("Skip Button View")]
    [SerializeField] private string _skipButtonText = "넘어가기";

    // ==================== 튜토리얼 상태 ====================
    private Coroutine _openGuideCoroutine;
    private Coroutine _interactionCoroutine;
    private Coroutine _eButtonBlinkCoroutine;
    private bool _isInitialGuideFinished;
    private bool _isInteractionRunning;
    private bool _isInteractionCompleted;
    private Vector3 _originEButtonScale = Vector3.one;
    private bool _isCachedEButtonOrigin;
    private Canvas Canvas_EButton;

    // ==================== 런타임 생성 객체 ====================
    private GameObject Object_InteractionPrompt;
    private TextMeshPro Text_InteractionPrompt;
    private GameObject Object_InteractionFlash;
    private Image Image_InteractionFlash;
    private GameObject Object_CommonSkipButton;
    private NextButtonController Button_CommonSkip;

    /// <summary>
    /// Tutorial1Group이 켜지면 시작 가이드와 이동 잠금부터 진행합니다.
    /// </summary>
    private void OnEnable()
    {
        StartTutorial();
    }

    /// <summary>
    /// 매 프레임 모란 근처 E 상호작용 입력을 확인합니다.
    /// </summary>
    private void Update()
    {
        UpdateInteractionInput();
    }

    /// <summary>
    /// Tutorial1Group이 꺼지면 진행 중인 가이드, 대화, E 버튼, 이펙트를 정리합니다.
    /// </summary>
    private void OnDisable()
    {
        StopOpenGuideCoroutine();
        StopInteractionCoroutine();
        CloseTutorialGuide();
        CloseDialogueGroup();
        HideInteractionPrompt();
        HideInteractionFlash();
        HideCommonSkipButton();
    }

    // ==================== 튜토리얼 시작 ====================

    /// <summary>
    /// Tutorial1Group이 열릴 때 첫 안내를 보여주고 플레이어 이동을 잠급니다.
    /// </summary>
    private void StartTutorial()
    {
        CacheRuntimeReference();
        PrepareTutorialCameraView();

        _isInitialGuideFinished = false;
        _isInteractionRunning = false;
        _isInteractionCompleted = false;

        CreateInteractionPromptIfNeeded();
        HideInteractionPrompt();
        HideCommonSkipButton();
        PrepareMoranAnimator();

        if (_isLockPlayerUntilGuideEnd)
            LockPlayerMovement();

        if (!_isShowGuideOnEnable)
        {
            FinishInitialGuide();
            return;
        }

        StopOpenGuideCoroutine();
        _openGuideCoroutine = StartCoroutine(OpenInitialTutorialGuideRoutine());
    }

    /// <summary>
    /// 장영심과 모란 참조를 보정합니다. 인스펙터 참조가 있으면 그 값을 우선 사용합니다.
    /// </summary>
    private void CacheRuntimeReference()
    {
        if (Transform_JangYoungSim == null && Character_JangYoungSim != null)
            Transform_JangYoungSim = Character_JangYoungSim.transform;

        if (Renderer_Moran == null && Transform_Moran != null)
            Renderer_Moran = Transform_Moran.GetComponentInChildren<SpriteRenderer>(true);
    }

    /// <summary>
    /// Tutorial1Group에 들어올 때 카메라를 Tutorial1Background 기준 크기로 다시 맞춥니다.
    /// Stage/Road에서 쓰던 광각 카메라 값이 남아 있으면 배경이 점처럼 작아지므로, 이 그룹이 자기 촬영 렌즈를 직접 세팅합니다.
    /// </summary>
    private void PrepareTutorialCameraView()
    {
        if (Camera_Main == null)
            Camera_Main = Camera.main;

        if (Camera_Main == null)
            return;

        if (Camera_Follow == null)
            Camera_Main.TryGetComponent(out Camera_Follow);

        SpriteRenderer backgroundRenderer = ResolveTutorialBackgroundRenderer();

        if (backgroundRenderer != null)
            FocusCameraOnRenderer(backgroundRenderer, _tutorialCameraPadding);
        else
            Camera_Main.orthographicSize = Mathf.Max(1f, _fallbackTutorialCameraSize);

        if (Camera_Follow != null && Transform_JangYoungSim != null)
        {
            Camera_Follow.enabled = true;
            Camera_Follow.SetTarget(Transform_JangYoungSim);
        }
    }

    private SpriteRenderer ResolveTutorialBackgroundRenderer()
    {
        Transform[] childTransformArray = GetComponentsInChildren<Transform>(true);

        foreach (Transform childTransform in childTransformArray)
        {
            if (childTransform == null || childTransform.name != _tutorialBackgroundName)
                continue;

            SpriteRenderer spriteRenderer = childTransform.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
                return spriteRenderer;

            return childTransform.GetComponentInChildren<SpriteRenderer>(true);
        }

        return null;
    }

    private void FocusCameraOnRenderer(SpriteRenderer targetRenderer, float padding)
    {
        if (Camera_Main == null || targetRenderer == null)
            return;

        Bounds bounds = targetRenderer.bounds;
        float aspect = Mathf.Max(0.01f, Camera_Main.aspect);
        float sizeByHeight = bounds.extents.y;
        float sizeByWidth = bounds.extents.x / aspect;
        float targetSize = Mathf.Max(sizeByHeight, sizeByWidth) * Mathf.Max(1f, padding);
        Vector3 cameraPosition = bounds.center;
        cameraPosition.z = Camera_Main.transform.position.z;

        Camera_Main.orthographic = true;
        Camera_Main.orthographicSize = Mathf.Max(1f, targetSize);
        Camera_Main.transform.position = cameraPosition;
    }

    /// <summary>
    /// 매니저 준비를 기다린 뒤 시작 튜토리얼 가이드를 엽니다.
    /// </summary>
    private IEnumerator OpenInitialTutorialGuideRoutine()
    {
        yield return null;

        while (OOTechUIManager.Inst == null || OOTechGameDataManager.Inst == null)
            yield return null;

        RegisterTutorialGuideGroup();
        OpenInitialTutorialGuide();

        _openGuideCoroutine = null;
    }

    /// <summary>
    /// TutorialGuideGroup을 UIManager에 등록해 OpenUI로 열 수 있게 합니다.
    /// </summary>
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

    /// <summary>
    /// Tutorial 데이터 또는 Narration 데이터를 찾아 첫 안내 패널을 보여줍니다.
    /// </summary>
    private void OpenInitialTutorialGuide()
    {
        if (!TryOpenTutorialGuide())
        {
            FinishInitialGuide();
            return;
        }

        OO_Tutorial tutorialData = OOTechGameDataManager.Inst.GetTutorialData(_tutorialNarrationId);

        if (tutorialData != null)
        {
            UI_TutorialGuide.SetTitleEmphasisActive(false);
            UI_TutorialGuide.ShowGuide(tutorialData, OnInitialTutorialGuideEnd);
            return;
        }

        OO_Narration narrationData = OOTechGameDataManager.Inst.GetNarrationData(_tutorialNarrationId);
        if (narrationData != null)
        {
            UI_TutorialGuide.SetTitleEmphasisActive(false);
            UI_TutorialGuide.ShowGuide(narrationData, OnInitialTutorialGuideEnd);
            return;
        }

        Debug.LogWarning($"[OOTechTutorial1Controller] 시작 튜토리얼 데이터를 찾을 수 없습니다: {_tutorialNarrationId}");
        FinishInitialGuide();
    }

    /// <summary>
    /// TutorialGuideGroup을 열고 OOTechTutorialGuideUI 참조가 있는지 확인합니다.
    /// </summary>
    private bool TryOpenTutorialGuide()
    {
        if (OOTechUIManager.Inst == null)
            return false;

        if (UI_TutorialGuide == null)
        {
            Debug.LogError("[OOTechTutorial1Controller] OOTechTutorialGuideUI 직접 참조가 연결되어 있지 않습니다.");
            return false;
        }

        if (!OOTechUIManager.Inst.OpenUI(_tutorialGuideGroupName))
        {
            Debug.LogError($"[OOTechTutorial1Controller] TutorialGuideGroup을 열 수 없습니다: {_tutorialGuideGroupName}");
            return false;
        }

        return true;
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

    // ==================== 상호작용 입력 ====================

    /// <summary>
    /// 모란이 화면에 보이고 장영심이 가까이 있을 때만 E 버튼을 보여줍니다.
    /// E를 누르면 모란 기상 연출이 시작됩니다.
    /// </summary>
    private void UpdateInteractionInput()
    {
        if (!_isInitialGuideFinished)
            return;

        if (_isInteractionRunning || _isInteractionCompleted)
            return;

        bool isNearMoran = IsPlayerNearMoran();
        SetInteractionPromptActive(isNearMoran);

        if (!isNearMoran)
            return;

        UpdateInteractionPromptPosition();

        if (!Input.GetKeyDown(_interactionKey))
            return;

        StartMoranInteraction();
    }

    /// <summary>
    /// 장영심과 모란 사이의 거리가 상호작용 범위 안인지 확인합니다.
    /// </summary>
    private bool IsPlayerNearMoran()
    {
        if (Transform_JangYoungSim == null || Transform_Moran == null)
            return false;

        float distance = Vector2.Distance(Transform_JangYoungSim.position, Transform_Moran.position);
        return distance <= Mathf.Max(_interactionDistance, _minimumInteractionDistance);
    }

    /// <summary>
    /// 모란 오브젝트가 카메라 안에 들어와 있는지 확인합니다.
    /// </summary>
    private bool IsMoranVisibleToCamera()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null || Transform_Moran == null)
            return true;

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(Transform_Moran.position);
        return viewportPosition.z >= 0f &&
               viewportPosition.x >= 0f &&
               viewportPosition.x <= 1f &&
               viewportPosition.y >= 0f &&
               viewportPosition.y <= 1f;
    }

    /// <summary>
    /// 모란과 상호작용을 시작합니다.
    /// 두 캐릭터의 1회성 애니메이션을 재생한 뒤 캐릭터 대화로 넘어갑니다.
    /// </summary>
    /// <summary>
    /// 모란 기상 연출 코루틴을 시작합니다.
    /// </summary>
    private void StartMoranInteraction()
    {
        StopInteractionCoroutine();
        _interactionCoroutine = StartCoroutine(PlayMoranInteractionRoutine());
    }

    /// <summary>
    /// 화면 플래시, 장영심 놀람, 모란 WakeUp 애니메이션을 순서대로 재생합니다.
    /// </summary>
    private IEnumerator PlayMoranInteractionRoutine()
    {
        _isInteractionRunning = true;
        HideInteractionPrompt();
        LockPlayerMovement();

        yield return PlayInteractionFlashRoutine();

        float animationSpeed = Mathf.Clamp(_interactionAnimationSpeed, 0.5f, 0.7f);

        if (Character_JangYoungSim != null)
            Character_JangYoungSim.PlaySurprisedAnimationOnce(animationSpeed);

        PlayMoranWakeUpAnimation(animationSpeed);

        yield return new WaitForSeconds(_interactionAnimationWaitSeconds / animationSpeed);

        if (Character_JangYoungSim != null)
            Character_JangYoungSim.HoldSurprisedAnimationLastFrame();

        HoldMoranWakeUpAnimationLastFrame();

        OpenCharacterDialogueSequence();

        _interactionCoroutine = null;
    }

    /// <summary>
    /// Moran 배우가 잠에서 깨는 장면을 1회 재생합니다. 감독이 큐를 주면 배우의 Animator가 해당 컷을 처음부터 시작합니다.
    /// </summary>
    private void PlayMoranWakeUpAnimation(float animationSpeed)
    {
        if (Animator_Moran == null || string.IsNullOrEmpty(_moranWakeUpStateName))
        {
            Debug.LogWarning("[OOTechTutorial1Controller] Moran WakeUp cannot play. Animator or state name is missing.");
            return;
        }

        Animator_Moran.enabled = true;
        Animator_Moran.speed = animationSpeed;

        int stateHash = Animator.StringToHash(_moranWakeUpStateName);
        if (!Animator_Moran.HasState(0, stateHash))
        {
            Debug.LogWarning($"[OOTechTutorial1Controller] Moran WakeUp state not found: {_moranWakeUpStateName}. Check Animator Controller on Tutorial1Group/Moran.");
            return;
        }

        Animator_Moran.Play(stateHash, 0, 0f);
        Animator_Moran.Update(0f);
    }

    /// <summary>
    /// Moran WakeUp 컷이 끝나면 마지막 프레임에서 멈춥니다. 무대 위 배우가 다음 대사를 기다리는 상태입니다.
    /// </summary>
    private void HoldMoranWakeUpAnimationLastFrame()
    {
        if (Animator_Moran == null || string.IsNullOrEmpty(_moranWakeUpStateName))
            return;

        int stateHash = Animator.StringToHash(_moranWakeUpStateName);
        if (!Animator_Moran.HasState(0, stateHash))
            return;

        Animator_Moran.Play(stateHash, 0, 1f);
        Animator_Moran.Update(0f);
        Animator_Moran.speed = 0f;
    }

    // ==================== 캐릭터 대화 ====================

    /// <summary>
    /// 모란 대화와 장영심 대화를 DialogueGroup에 이어서 표시합니다.
    /// </summary>
    private void OpenCharacterDialogueSequence()
    {
        if (OOTechUIManager.Inst == null || OOTechGameDataManager.Inst == null)
        {
            Debug.LogError("[OOTechTutorial1Controller] UIManager 또는 GameDataManager가 없어 대화를 열 수 없습니다.");
            ShowCompleteTutorialGuide();
            return;
        }

        if (UI_Dialogue == null)
        {
            Debug.LogError("[OOTechTutorial1Controller] DialogueUI 직접 참조가 연결되어 있지 않습니다.");
            ShowCompleteTutorialGuide();
            return;
        }

        if (!OOTechUIManager.Inst.OpenUI(_dialogueGroupName))
        {
            Debug.LogError($"[OOTechTutorial1Controller] DialogueGroup을 열 수 없습니다: {_dialogueGroupName}");
            ShowCompleteTutorialGuide();
            return;
        }

        OO_Dialogue moranDialogueData = OOTechGameDataManager.Inst.GetDialogueData(_moranDialogueId);
        if (moranDialogueData == null)
        {
            EndCharacterDialogueSequence();
            return;
        }

        UI_Dialogue.ShowDialogue(moranDialogueData, OnMoranDialogueEnd);
    }

    private void OnMoranDialogueEnd()
    {
        OO_Dialogue jangYoungSimDialogueData = OOTechGameDataManager.Inst.GetDialogueData(_jangYoungSimDialogueId);

        if (jangYoungSimDialogueData == null)
        {
            EndCharacterDialogueSequence();
            return;
        }

        UI_Dialogue.ShowDialogue(jangYoungSimDialogueData, EndCharacterDialogueSequence);
    }

    /// <summary>
    /// 캐릭터 대화가 끝나면 임무 완료 튜토리얼 가이드를 엽니다.
    /// </summary>
    private void EndCharacterDialogueSequence()
    {
        CloseDialogueGroup();
        ShowCompleteTutorialGuide();
    }

    // ==================== 임무 완료 가이드 ====================

    /// <summary>
    /// 임무 완료 안내를 보여주고 완료 후 넘어가기 버튼을 띄웁니다.
    /// </summary>
    private void ShowCompleteTutorialGuide()
    {
        LockPlayerMovement();
        RegisterTutorialGuideGroup();

        if (!TryOpenTutorialGuide())
        {
            FinishCompleteGuide();
            return;
        }

        OO_Tutorial tutorialData = OOTechGameDataManager.Inst.GetTutorialData(_completeTutorialId);

        if (tutorialData == null)
        {
            Debug.LogWarning($"[OOTechTutorial1Controller] 임무 완료 튜토리얼 데이터를 찾을 수 없습니다: {_completeTutorialId}");
            FinishCompleteGuide();
            return;
        }

        UI_TutorialGuide.ShowGuide(tutorialData, OnCompleteTutorialGuideEnd);
        UI_TutorialGuide.SetTitleEmphasisActive(true);
    }

    // ==================== 가이드 종료 ====================

    private void OnInitialTutorialGuideEnd()
    {
        FinishInitialGuide();
    }

    /// <summary>
    /// 시작 가이드가 끝났을 때 장영심 조작을 풀어 실제 튜토리얼 플레이를 시작합니다.
    /// </summary>
    private void FinishInitialGuide()
    {
        if (_isInitialGuideFinished)
            return;

        _isInitialGuideFinished = true;

        CloseTutorialGuide();

        if (_isLockPlayerUntilGuideEnd)
            UnlockPlayerMovement();
    }

    private void OnCompleteTutorialGuideEnd()
    {
        FinishCompleteGuide();
    }

    /// <summary>
    /// 완료 가이드가 끝나면 다음 그룹으로 넘어갈 CommonSkipButton을 보여줍니다.
    /// </summary>
    private void FinishCompleteGuide()
    {
        _isInteractionCompleted = true;
        _isInteractionRunning = false;

        CloseTutorialGuide();
        UnlockPlayerMovement();
        ShowCommonSkipButton();
    }

    private void CloseTutorialGuide()
    {
        if (UI_TutorialGuide != null)
        {
            UI_TutorialGuide.SetTitleEmphasisActive(false);
            UI_TutorialGuide.CloseGuide();
        }

        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.CloseUI(_tutorialGuideGroupName);
    }

    private void CloseDialogueGroup()
    {
        if (UI_Dialogue != null)
            UI_Dialogue.CloseDialogue();

        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.CloseUI(_dialogueGroupName);
    }

    // ==================== 상호작용 표시 ====================

    /// <summary>
    /// 장영심 머리 위에 뜨는 월드 텍스트 E 프롬프트를 준비합니다.
    /// </summary>
    private void CreateInteractionPromptIfNeeded()
    {
        PrepareEButtonPrompt();

        if (Group_EButton != null)
            return;

        if (Object_InteractionPrompt != null || Transform_Moran == null)
            return;

        Object_InteractionPrompt = new GameObject("Text_InteractionPrompt");
        Object_InteractionPrompt.transform.SetParent(Transform_Moran, false);
        Object_InteractionPrompt.transform.localPosition = _interactionPromptOffset;
        Object_InteractionPrompt.transform.localRotation = Quaternion.identity;
        Object_InteractionPrompt.transform.localScale = Vector3.one * _interactionPromptScale;

        Text_InteractionPrompt = Object_InteractionPrompt.AddComponent<TextMeshPro>();
        Text_InteractionPrompt.text = _interactionPromptText;
        Text_InteractionPrompt.fontSize = _interactionPromptFontSize;
        Text_InteractionPrompt.alignment = TextAlignmentOptions.Center;
        Text_InteractionPrompt.color = _interactionPromptColor;
        Text_InteractionPrompt.raycastTarget = false;
        Text_InteractionPrompt.textWrappingMode = TextWrappingModes.NoWrap;
        Text_InteractionPrompt.sortingOrder = _interactionPromptSortingOrder;
    }

    /// <summary>
    /// E 프롬프트와 화면 Overlay E 버튼을 함께 켜거나 끕니다.
    /// </summary>
    private void SetInteractionPromptActive(bool isActive)
    {
        if (Group_EButton != null)
        {
            if (Group_EButton.activeSelf != isActive)
                Group_EButton.SetActive(isActive);

            if (isActive)
            {
                UpdateInteractionPromptPosition();
                StartEButtonBlinkEffect();
            }
            else
            {
                StopEButtonBlinkEffect();
            }

            return;
        }

        if (Object_InteractionPrompt != null)
            Object_InteractionPrompt.SetActive(isActive);
    }

    private void HideInteractionPrompt()
    {
        SetInteractionPromptActive(false);
    }

    /// <summary>
    /// 화면 우측 하단에 고정되는 E 버튼 UI를 연결하고 표시 설정을 적용합니다.
    /// </summary>
    private void PrepareEButtonPrompt()
    {
        if (Group_EButton == null)
            return;

        GameObject buttonObject = OOTechSceneQuery.RequestChildObjectByName(Group_EButton.transform, "Button");
        Transform buttonTransform = buttonObject != null ? buttonObject.transform : null;

        if (buttonTransform != null)
            Rect_EButton = buttonTransform as RectTransform;
        else if (Rect_EButton == null)
            Rect_EButton = Group_EButton.transform as RectTransform;

        Rect_EButtonTextParent = Rect_EButton;

        if (CanvasGroup_EButton == null)
            CanvasGroup_EButton = Group_EButton.GetComponent<CanvasGroup>();

        if (CanvasGroup_EButton == null)
            CanvasGroup_EButton = Group_EButton.AddComponent<CanvasGroup>();

        CanvasGroup_EButton.interactable = false;
        CanvasGroup_EButton.blocksRaycasts = false;
        PrepareEButtonCanvas();
        ApplyEButtonPresentation();
        CreateEButtonTextIfNeeded();

        if (Text_EButton != null)
            Text_EButton.text = _eButtonText;

        CacheEButtonOriginIfNeeded();
        UpdateInteractionPromptPosition();
        Group_EButton.SetActive(false);
    }

    /// <summary>
    /// E 버튼이 배경에 묻히지 않도록 크기, 색, 텍스트, Sorting Order를 보정합니다.
    /// </summary>
    private void ApplyEButtonPresentation()
    {
        if (Rect_EButton == null)
            return;

        if (IsEButtonOverlayCanvas())
        {
            ApplyFixedEButtonOverlayLayout();
            return;
        }

        float minimumScale = Mathf.Max(0.001f, _eButtonMinimumWorldScale);

        if (Rect_EButton.localScale.x < minimumScale)
            Rect_EButton.localScale = Vector3.one * minimumScale;
    }

    /// <summary>
    /// E 버튼 Canvas를 최상단 Overlay로 설정합니다.
    /// </summary>
    private void PrepareEButtonCanvas()
    {
        if (Group_EButton == null)
            return;

        Canvas_EButton = Group_EButton.GetComponent<Canvas>();

        if (Canvas_EButton == null)
            Canvas_EButton = Group_EButton.AddComponent<Canvas>();

        Canvas_EButton.renderMode = RenderMode.ScreenSpaceOverlay;
        Canvas_EButton.overrideSorting = true;
        Canvas_EButton.sortingOrder = _eButtonSortingOrder;

        RectTransform rootRect = Group_EButton.transform as RectTransform;

        if (rootRect != null)
            rootRect.localScale = Vector3.one;

        CanvasScaler canvasScaler = Group_EButton.GetComponent<CanvasScaler>();

        if (canvasScaler == null)
            canvasScaler = Group_EButton.AddComponent<CanvasScaler>();

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = _eButtonReferenceResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;

        if (Group_EButton.GetComponent<GraphicRaycaster>() == null)
            Group_EButton.AddComponent<GraphicRaycaster>();
    }

    /// <summary>
    /// 기존 E 버튼 텍스트가 없으면 Button 자식으로 TMP 텍스트를 보조 생성합니다.
    /// </summary>
    private void CreateEButtonTextIfNeeded()
    {
        if (Text_EButton != null)
            return;

        Transform textParent = Rect_EButtonTextParent != null ? Rect_EButtonTextParent : Rect_EButton;

        if (textParent == null)
            return;

        GameObject textObject = new GameObject("Text_E", typeof(RectTransform));
        textObject.layer = Group_EButton.layer;
        textObject.transform.SetParent(textParent, false);

        RectTransform textRect = textObject.transform as RectTransform;
        StretchFullScreen(textRect);

        Text_EButton = textObject.AddComponent<TextMeshProUGUI>();
        Text_EButton.text = _eButtonText;
        Text_EButton.fontSize = 72f;
        Text_EButton.fontStyle = FontStyles.Bold;
        Text_EButton.alignment = TextAlignmentOptions.Center;
        Text_EButton.color = Color.white;
        Text_EButton.raycastTarget = false;
    }

    /// <summary>
    /// 모란 위치나 화면 고정 규칙에 맞춰 E 버튼 위치를 갱신합니다.
    /// </summary>
    private void UpdateInteractionPromptPosition()
    {
        if (Group_EButton == null)
            return;

        if (IsEButtonOverlayCanvas())
        {
            ApplyFixedEButtonOverlayLayout();
            return;
        }

        if (Transform_Moran == null)
            return;

        Vector3 worldPosition = Transform_Moran.position + _eButtonWorldOffset;
        Group_EButton.transform.position = worldPosition;
    }

    private bool IsEButtonOverlayCanvas()
    {
        return Canvas_EButton != null && Canvas_EButton.renderMode == RenderMode.ScreenSpaceOverlay;
    }

    /// <summary>
    /// E 버튼을 화면 우측 하단 근처 고정 위치에 배치합니다.
    /// </summary>
    private void ApplyFixedEButtonOverlayLayout()
    {
        if (Rect_EButton == null)
            return;

        Vector2 clampedAnchor = new Vector2(Mathf.Clamp01(_eButtonScreenAnchor.x), Mathf.Clamp01(_eButtonScreenAnchor.y));
        float pixelSize = Mathf.Clamp(_eButtonFixedPixelSize, _eButtonMinimumPixelSize, _eButtonMaximumPixelSize);

        Rect_EButton.anchorMin = clampedAnchor;
        Rect_EButton.anchorMax = clampedAnchor;
        Rect_EButton.pivot = new Vector2(0.5f, 0.5f);
        Rect_EButton.anchoredPosition = Vector2.zero;
        Rect_EButton.localScale = Vector3.one;
        Rect_EButton.sizeDelta = new Vector2(pixelSize, pixelSize);

        Button button = Rect_EButton.GetComponent<Button>();

        if (button != null)
            button.interactable = false;

        Image buttonImage = Rect_EButton.GetComponent<Image>();

        if (buttonImage != null)
            buttonImage.raycastTarget = false;

        if (_isCachedEButtonOrigin)
            _originEButtonScale = Vector3.one;
    }

    /// <summary>
    /// 모란 크기의 절반 정도를 기준으로 E 버튼 픽셀 크기를 계산합니다.
    /// </summary>
    private float CalculateEButtonPixelSize(Camera mainCamera)
    {
        if (Renderer_Moran == null || mainCamera == null)
            return Mathf.Clamp(96f, _eButtonMinimumPixelSize, _eButtonMaximumPixelSize);

        Bounds bounds = Renderer_Moran.bounds;
        Vector3 minScreen = mainCamera.WorldToScreenPoint(bounds.min);
        Vector3 maxScreen = mainCamera.WorldToScreenPoint(bounds.max);
        float moranPixelHeight = Mathf.Abs(maxScreen.y - minScreen.y);
        float moranPixelWidth = Mathf.Abs(maxScreen.x - minScreen.x);
        float baseSize = Mathf.Min(moranPixelWidth, moranPixelHeight) * Mathf.Clamp01(_eButtonMoranSizeRatio);
        return Mathf.Clamp(baseSize, _eButtonMinimumPixelSize, _eButtonMaximumPixelSize);
    }

    private void StartEButtonBlinkEffect()
    {
        if (_eButtonBlinkCoroutine != null)
            return;

        CacheEButtonOriginIfNeeded();
        _eButtonBlinkCoroutine = StartCoroutine(PlayEButtonBlinkEffectRoutine());
    }

    /// <summary>
    /// E 버튼이 반짝이도록 알파와 스케일을 흔듭니다.
    /// </summary>
    private IEnumerator PlayEButtonBlinkEffectRoutine()
    {
        while (true)
        {
            float lerpValue = (Mathf.Sin(Time.unscaledTime * _eButtonBlinkSpeed) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(_eButtonMinAlpha, _eButtonMaxAlpha, lerpValue);

            if (CanvasGroup_EButton != null)
                CanvasGroup_EButton.alpha = alpha;

            if (Rect_EButton != null)
                Rect_EButton.localScale = _originEButtonScale * (1f + (_eButtonScalePower * lerpValue));

            yield return null;
        }
    }

    private void StopEButtonBlinkEffect()
    {
        if (_eButtonBlinkCoroutine != null)
        {
            StopCoroutine(_eButtonBlinkCoroutine);
            _eButtonBlinkCoroutine = null;
        }

        if (CanvasGroup_EButton != null)
            CanvasGroup_EButton.alpha = 1f;

        if (Rect_EButton != null && _isCachedEButtonOrigin)
            Rect_EButton.localScale = _originEButtonScale;
    }

    private void CacheEButtonOriginIfNeeded()
    {
        if (_isCachedEButtonOrigin || Rect_EButton == null)
            return;

        _originEButtonScale = Rect_EButton.localScale;
        _isCachedEButtonOrigin = true;
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

    // ==================== 상호작용 화면 번쩍임 ====================

    /// <summary>
    /// 모란 상호작용 시작 전에 짧은 전체 화면 플래시를 재생합니다.
    /// </summary>
    private IEnumerator PlayInteractionFlashRoutine()
    {
        if (!_isUseInteractionFlash)
            yield break;

        CreateInteractionFlashIfNeeded();

        if (Object_InteractionFlash == null || Image_InteractionFlash == null)
            yield break;

        Object_InteractionFlash.SetActive(true);

        for (int i = 0; i < _interactionFlashCount; i++)
        {
            yield return FadeInteractionFlash(0f, _interactionFlashColor.a);
            yield return FadeInteractionFlash(_interactionFlashColor.a, 0f);
        }

        HideInteractionFlash();
    }

    private IEnumerator FadeInteractionFlash(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < _interactionFlashFadeSeconds)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float lerpValue = Mathf.Clamp01(elapsedTime / _interactionFlashFadeSeconds);
            SetInteractionFlashAlpha(Mathf.Lerp(startAlpha, endAlpha, lerpValue));
            yield return null;
        }

        SetInteractionFlashAlpha(endAlpha);
    }

    private void CreateInteractionFlashIfNeeded()
    {
        if (Object_InteractionFlash != null)
            return;

        Object_InteractionFlash = new GameObject("Image_InteractionFlash", typeof(RectTransform));
        Object_InteractionFlash.layer = gameObject.layer;
        Object_InteractionFlash.transform.SetParent(transform, false);

        RectTransform flashRect = Object_InteractionFlash.transform as RectTransform;
        StretchFullScreen(flashRect);

        Canvas canvas = Object_InteractionFlash.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = _interactionFlashSortingOrder;

        CanvasScaler canvasScaler = Object_InteractionFlash.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        Image_InteractionFlash = Object_InteractionFlash.AddComponent<Image>();
        Image_InteractionFlash.raycastTarget = false;
        SetInteractionFlashAlpha(0f);
        Object_InteractionFlash.SetActive(false);
    }

    private void SetInteractionFlashAlpha(float alpha)
    {
        if (Image_InteractionFlash == null)
            return;

        Color flashColor = _interactionFlashColor;
        flashColor.a = alpha;
        Image_InteractionFlash.color = flashColor;
    }

    private void HideInteractionFlash()
    {
        if (Image_InteractionFlash != null)
            SetInteractionFlashAlpha(0f);

        if (Object_InteractionFlash != null)
            Object_InteractionFlash.SetActive(false);
    }

    // ==================== 모란 애니메이션 준비 ====================

    /// <summary>
    /// Moran_WakeUp 애니메이션이 상호작용 전에는 자동 재생되지 않도록 준비합니다.
    /// </summary>
    private void PrepareMoranAnimator()
    {
        if (!_isDisableMoranAnimatorUntilInteraction)
            return;

        if (Animator_Moran != null)
            Animator_Moran.enabled = false;
    }

    // ==================== 공용 넘어가기 버튼 ====================

    /// <summary>
    /// 튜토리얼 완료 후 다음 그룹으로 가는 넘어가기 버튼을 보여줍니다.
    /// </summary>
    private void ShowCommonSkipButton()
    {
        CreateCommonSkipButtonIfNeeded();

        if (Object_CommonSkipButton == null)
            return;

        Object_CommonSkipButton.SetActive(true);

        if (Button_CommonSkip != null)
        {
            Button_CommonSkip.SetGroupName(_tutorialGroupName, _nextGroupName);
            Button_CommonSkip.SetButtonText(_skipButtonText);
        }
    }

    private void HideCommonSkipButton()
    {
        if (Object_CommonSkipButton != null)
            Object_CommonSkipButton.SetActive(false);
    }

    /// <summary>
    /// CommonSkipButton 프리팹을 Tutorial1Group 자식으로 한 번만 생성합니다.
    /// </summary>
    private void CreateCommonSkipButtonIfNeeded()
    {
        if (Object_CommonSkipButton != null)
            return;

        if (Prefab_CommonSkipButton == null)
        {
            Debug.LogError("[OOTechTutorial1Controller] CommonSkipButton 프리팹이 연결되어 있지 않습니다.");
            return;
        }

        Object_CommonSkipButton = Instantiate(Prefab_CommonSkipButton, transform, false);
        Object_CommonSkipButton.name = "CommonSkipButton_Tutorial1";
        Button_CommonSkip = Object_CommonSkipButton.GetComponent<NextButtonController>();
        Object_CommonSkipButton.SetActive(false);
    }

    // ==================== 코루틴 정리 ====================

    private void StopOpenGuideCoroutine()
    {
        if (_openGuideCoroutine == null)
            return;

        StopCoroutine(_openGuideCoroutine);
        _openGuideCoroutine = null;
    }

    private void StopInteractionCoroutine()
    {
        if (_interactionCoroutine == null)
            return;

        StopCoroutine(_interactionCoroutine);
        _interactionCoroutine = null;

        if (Character_JangYoungSim != null)
            Character_JangYoungSim.StopOneShotAnimation();

        if (Animator_Moran != null)
            Animator_Moran.speed = 1f;
    }
}
