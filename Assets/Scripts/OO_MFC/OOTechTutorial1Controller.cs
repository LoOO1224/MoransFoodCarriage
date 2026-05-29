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
    [Header("Character")]
    [SerializeField] private JangYoungSimController Character_JangYoungSim;
    [SerializeField] private Transform Transform_JangYoungSim;
    [SerializeField] private Transform Transform_Moran;
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

    [Header("Interaction Rule")]
    [SerializeField] private float _interactionDistance = 2.2f;
    [SerializeField] private KeyCode _interactionKey = KeyCode.E;
    [SerializeField] private float _interactionAnimationWaitSeconds = 2.1f;
    [SerializeField] private float _interactionAnimationSpeed = 0.6f;

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

    // ==================== 런타임 생성 객체 ====================
    private GameObject Object_InteractionPrompt;
    private TextMeshPro Text_InteractionPrompt;
    private GameObject Object_InteractionFlash;
    private Image Image_InteractionFlash;
    private GameObject Object_CommonSkipButton;
    private NextButtonController Button_CommonSkip;

    private void OnEnable()
    {
        StartTutorial();
    }

    private void Update()
    {
        UpdateInteractionInput();
    }

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

    private void CacheRuntimeReference()
    {
        if (Transform_JangYoungSim == null && Character_JangYoungSim != null)
            Transform_JangYoungSim = Character_JangYoungSim.transform;
    }

    private IEnumerator OpenInitialTutorialGuideRoutine()
    {
        yield return null;

        while (OOTechUIManager.Inst == null || OOTechGameDataManager.Inst == null)
            yield return null;

        RegisterTutorialGuideGroup();
        OpenInitialTutorialGuide();

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

    private bool IsPlayerNearMoran()
    {
        if (Transform_JangYoungSim == null || Transform_Moran == null)
            return false;

        float distance = Vector2.Distance(Transform_JangYoungSim.position, Transform_Moran.position);
        return distance <= _interactionDistance;
    }

    /// <summary>
    /// 모란과 상호작용을 시작합니다.
    /// 두 캐릭터의 1회성 애니메이션을 재생한 뒤 캐릭터 대화로 넘어갑니다.
    /// </summary>
    private void StartMoranInteraction()
    {
        StopInteractionCoroutine();
        _interactionCoroutine = StartCoroutine(PlayMoranInteractionRoutine());
    }

    private IEnumerator PlayMoranInteractionRoutine()
    {
        _isInteractionRunning = true;
        HideInteractionPrompt();
        LockPlayerMovement();

        yield return PlayInteractionFlashRoutine();

        float animationSpeed = Mathf.Clamp(_interactionAnimationSpeed, 0.5f, 0.7f);

        if (Character_JangYoungSim != null)
            Character_JangYoungSim.PlaySurprisedAnimationOnce(animationSpeed);

        if (Animator_Moran != null && !string.IsNullOrEmpty(_moranWakeUpStateName))
        {
            Animator_Moran.enabled = true;
            Animator_Moran.speed = animationSpeed;
            Animator_Moran.Play(_moranWakeUpStateName, 0, 0f);
        }

        yield return new WaitForSeconds(_interactionAnimationWaitSeconds / animationSpeed);

        if (Character_JangYoungSim != null)
            Character_JangYoungSim.HoldSurprisedAnimationLastFrame();

        if (Animator_Moran != null)
            Animator_Moran.speed = 1f;

        OpenCharacterDialogueSequence();

        _interactionCoroutine = null;
    }

    // ==================== 캐릭터 대화 ====================

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

    private void EndCharacterDialogueSequence()
    {
        CloseDialogueGroup();
        ShowCompleteTutorialGuide();
    }

    // ==================== 임무 완료 가이드 ====================

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

    private void SetInteractionPromptActive(bool isActive)
    {
        if (Group_EButton != null)
        {
            if (Group_EButton.activeSelf != isActive)
                Group_EButton.SetActive(isActive);

            if (isActive)
                StartEButtonBlinkEffect();
            else
                StopEButtonBlinkEffect();

            return;
        }

        if (Object_InteractionPrompt != null)
            Object_InteractionPrompt.SetActive(isActive);
    }

    private void HideInteractionPrompt()
    {
        SetInteractionPromptActive(false);
    }

    private void PrepareEButtonPrompt()
    {
        if (Group_EButton == null)
            return;

        if (Rect_EButton == null)
            Rect_EButton = Group_EButton.transform as RectTransform;

        if (CanvasGroup_EButton == null)
            CanvasGroup_EButton = Group_EButton.GetComponent<CanvasGroup>();

        if (CanvasGroup_EButton == null)
            CanvasGroup_EButton = Group_EButton.AddComponent<CanvasGroup>();

        CreateEButtonTextIfNeeded();

        if (Text_EButton != null)
            Text_EButton.text = _eButtonText;

        CacheEButtonOriginIfNeeded();
        UpdateInteractionPromptPosition();
        Group_EButton.SetActive(false);
    }

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

    private void UpdateInteractionPromptPosition()
    {
        if (Group_EButton != null && Transform_Moran != null)
            Group_EButton.transform.position = Transform_Moran.position + _eButtonWorldOffset;
    }

    private void StartEButtonBlinkEffect()
    {
        if (_eButtonBlinkCoroutine != null)
            return;

        CacheEButtonOriginIfNeeded();
        _eButtonBlinkCoroutine = StartCoroutine(PlayEButtonBlinkEffectRoutine());
    }

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

    private void PrepareMoranAnimator()
    {
        if (!_isDisableMoranAnimatorUntilInteraction)
            return;

        if (Animator_Moran != null)
            Animator_Moran.enabled = false;
    }

    // ==================== 공용 넘어가기 버튼 ====================

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
