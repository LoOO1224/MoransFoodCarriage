using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Senario1Group에서 재익의 이동, 점프, 상호작용 요청을 담당하는 캐릭터 컨트롤러입니다.
/// 실제 화면 번쩍임, 페이드, 외형 변경 같은 그룹 연출은 OOTechSenario1Controller가 처리합니다.
/// </summary>
public class OOTechJaeikController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private bool _isMovementLockedOnStart;

    [Header("Component References")]
    [SerializeField] private Rigidbody2D Rigidbody_Jaeik;
    [SerializeField] private SpriteRenderer SpriteRenderer_Jaeik;
    [SerializeField] private Animator Animator_Jaeik;

    [Header("Animator Parameter Names")]
    [SerializeField] private string _isMovingParameterName = "isMoving";
    [SerializeField] private string _jumpTriggerName = "Jump";
    [SerializeField] private string _eatTriggerName = "Eat";

    [Header("Animator State Names")]
    [SerializeField] private string _idleStateName = "Jaeik_Idle";
    [SerializeField] private string _walkStateName = "Jaeik_isWalking";
    [SerializeField] private string _jumpStateName = "Jaeik_isJumping";
    [SerializeField] private string _eatStateName = "Jaeik_isEatting";

    [Header("One Shot Animation")]
    [SerializeField] private float _jumpAnimationSeconds = 1f;
    [SerializeField] private float _eatAnimationSeconds = 2.1f;
    [SerializeField] private bool _isUseDirectStatePlay = true;

    [Header("Interaction")]
    [SerializeField] private Transform Transform_InteractionTarget;
    [SerializeField] private float _interactionDistance = 2.2f;
    [SerializeField] private KeyCode _interactionKey = KeyCode.E;
    [SerializeField] private bool _isInteractionCompleted;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject Group_InteractionPrompt;
    [SerializeField] private TextMeshPro Text_InteractionPrompt;
    [SerializeField] private string _interactionPromptText = "[E] 먹기";
    [SerializeField] private Vector3 _interactionPromptOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private float _interactionPromptScale = 0.18f;
    [SerializeField] private float _interactionPromptFontSize = 6f;
    [SerializeField] private Color _interactionPromptColor = new Color(1f, 0.86f, 0.18f, 1f);
    [SerializeField] private int _interactionPromptSortingOrder = 60;
    [SerializeField] private bool _isCreateDefaultPrompt = true;

    // ==================== 입력 / 이동 상태 ====================
    private Vector2 _moveInput;
    private bool _isMovementLocked;
    private bool _isPlayingOneShotAnimation;
    private string _currentAnimationStateName;
    private Coroutine _oneShotAnimationCoroutine;

    // ==================== 이벤트 ====================
    private event Action _onEatInteractionRequested;

    private void Awake()
    {
        CreateDefaultPromptIfNeeded();
    }

    private void OnEnable()
    {
        ApplyDefaultPhysicsSetting();
        SetMovementLocked(_isMovementLockedOnStart);
        HideInteractionPrompt();
    }

    private void Update()
    {
        UpdateMoveInput();
        UpdateJumpInput();
        UpdateEatInteractionInput();
        UpdateSpriteFlip();
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void OnDisable()
    {
        StopOneShotAnimationCoroutine();
        SetAnimatorSpeed(1f);
        SetAnimatorMoving(false);
        _currentAnimationStateName = string.Empty;
        HideInteractionPrompt();
        StopPlayer();
    }

    // ==================== 외부 제어 ====================

    /// <summary>
    /// 씬 직렬화 참조가 비어 있을 때 Senario1Controller가 직접 알고 있는 Jaeik 오브젝트의 컴포넌트를 주입합니다.
    /// 씬 전체 검색을 사용하지 않고, 이미 연결된 Transform 내부 컴포넌트만 전달받습니다.
    /// </summary>
    public void SetComponentReference(Rigidbody2D rigidbodyJaeik, SpriteRenderer spriteRendererJaeik, Animator animatorJaeik)
    {
        Rigidbody_Jaeik = rigidbodyJaeik;
        SpriteRenderer_Jaeik = spriteRendererJaeik;
        Animator_Jaeik = animatorJaeik;

        ApplyDefaultPhysicsSetting();
    }

    /// <summary>
    /// 튜토리얼 안내나 연출 중 플레이어 입력을 잠글 때 호출합니다.
    /// </summary>
    public void LockMovement()
    {
        SetMovementLocked(true);
    }

    /// <summary>
    /// 잠긴 플레이어 입력을 다시 허용할 때 호출합니다.
    /// </summary>
    public void UnlockMovement()
    {
        SetMovementLocked(false);
    }

    /// <summary>
    /// 이동 잠금 상태를 직접 지정합니다.
    /// </summary>
    public void SetMovementLocked(bool isLocked)
    {
        _isMovementLocked = isLocked;

        if (_isMovementLocked)
        {
            _moveInput = Vector2.zero;
            StopPlayer();
            SetAnimatorMoving(false);
            PlayAnimationState(_idleStateName);
        }
    }

    /// <summary>
    /// 상호작용 대상 Transform을 설정합니다.
    /// Senario1Controller가 직접 참조로 연결한 Cube를 전달합니다.
    /// </summary>
    public void SetInteractionTarget(Transform interactionTarget)
    {
        Transform_InteractionTarget = interactionTarget;
        CreateDefaultPromptIfNeeded();
    }

    /// <summary>
    /// 상호작용 완료 여부를 설정합니다.
    /// 완료 후에는 E 프롬프트와 입력 요청을 막습니다.
    /// </summary>
    public void SetInteractionCompleted(bool isCompleted)
    {
        _isInteractionCompleted = isCompleted;

        if (_isInteractionCompleted)
            HideInteractionPrompt();
    }

    /// <summary>
    /// E 상호작용 요청 이벤트를 등록합니다.
    /// 화면 번쩍임과 외형 변경은 Senario1Controller에서 처리합니다.
    /// </summary>
    public void BindEatInteractionRequestEvent(Action onEatInteractionRequested)
    {
        _onEatInteractionRequested -= onEatInteractionRequested;
        _onEatInteractionRequested += onEatInteractionRequested;
    }

    /// <summary>
    /// E 상호작용 요청 이벤트 등록을 해제합니다.
    /// </summary>
    public void UnbindEatInteractionRequestEvent(Action onEatInteractionRequested)
    {
        _onEatInteractionRequested -= onEatInteractionRequested;
    }

    /// <summary>
    /// 재익의 먹기 애니메이션을 1회 재생합니다.
    /// 상호작용 연출이 끝난 뒤 Senario1Controller가 호출합니다.
    /// </summary>
    public void PlayEatAnimationOnce(Action onAnimationEnd = null)
    {
        PlayOneShotAnimation(_eatStateName, _eatTriggerName, _eatAnimationSeconds, onAnimationEnd);
    }

    /// <summary>
    /// 재익의 점프 애니메이션을 1회 재생합니다.
    /// Space 입력 또는 외부 연출에서 사용할 수 있습니다.
    /// </summary>
    public void PlayJumpAnimationOnce(Action onAnimationEnd = null)
    {
        PlayOneShotAnimation(_jumpStateName, _jumpTriggerName, _jumpAnimationSeconds, onAnimationEnd);
    }

    /// <summary>
    /// 외부 연출에서 프롬프트를 명시적으로 숨겨야 할 때 호출합니다.
    /// </summary>
    public void HideInteractionPrompt()
    {
        if (Group_InteractionPrompt != null)
            Group_InteractionPrompt.SetActive(false);
    }

    // ==================== 입력 처리 ====================

    private void UpdateMoveInput()
    {
        if (_isMovementLocked || _isPlayingOneShotAnimation)
        {
            _moveInput = Vector2.zero;
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector2(moveX, moveY).normalized;
    }

    private void UpdateJumpInput()
    {
        if (_isMovementLocked || _isPlayingOneShotAnimation)
            return;

        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        PlayJumpAnimationOnce();
    }

    private void UpdateEatInteractionInput()
    {
        if (_isMovementLocked || _isPlayingOneShotAnimation || _isInteractionCompleted)
        {
            HideInteractionPrompt();
            return;
        }

        bool isNearInteractionTarget = IsNearInteractionTarget();
        SetInteractionPromptActive(isNearInteractionTarget);

        if (!isNearInteractionTarget)
            return;

        if (!Input.GetKeyDown(_interactionKey))
            return;

        RequestEatInteraction();
    }

    private void RequestEatInteraction()
    {
        HideInteractionPrompt();

        if (_onEatInteractionRequested != null)
        {
            _onEatInteractionRequested.Invoke();
            return;
        }

        PlayEatAnimationOnce();
    }

    // ==================== 이동 처리 ====================

    private void MovePlayer()
    {
        if (Rigidbody_Jaeik == null)
            return;

        Rigidbody_Jaeik.linearVelocity = _moveInput * _moveSpeed;
    }

    private void StopPlayer()
    {
        if (Rigidbody_Jaeik != null)
            Rigidbody_Jaeik.linearVelocity = Vector2.zero;
    }

    private void ApplyDefaultPhysicsSetting()
    {
        if (Rigidbody_Jaeik == null)
            return;

        Rigidbody_Jaeik.gravityScale = 0f;
        Rigidbody_Jaeik.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    // ==================== 애니메이션 처리 ====================

    private void UpdateSpriteFlip()
    {
        if (SpriteRenderer_Jaeik == null)
            return;

        if (Mathf.Abs(_moveInput.x) <= 0.01f)
            return;

        SpriteRenderer_Jaeik.flipX = _moveInput.x < 0f;
    }

    private void UpdateAnimationState()
    {
        if (_isPlayingOneShotAnimation)
            return;

        bool isMoving = !_isMovementLocked && _moveInput.sqrMagnitude > 0.01f;
        SetAnimatorMoving(isMoving);

        if (_isUseDirectStatePlay)
            PlayAnimationState(isMoving ? _walkStateName : _idleStateName);
    }

    private void PlayOneShotAnimation(string stateName, string triggerName, float animationSeconds, Action onAnimationEnd)
    {
        StopOneShotAnimationCoroutine();
        _oneShotAnimationCoroutine = StartCoroutine(PlayOneShotAnimationRoutine(stateName, triggerName, animationSeconds, onAnimationEnd));
    }

    private IEnumerator PlayOneShotAnimationRoutine(string stateName, string triggerName, float animationSeconds, Action onAnimationEnd)
    {
        _isPlayingOneShotAnimation = true;
        _moveInput = Vector2.zero;
        StopPlayer();
        SetAnimatorMoving(false);

        if (Animator_Jaeik != null)
        {
            if (!string.IsNullOrEmpty(triggerName))
                Animator_Jaeik.SetTrigger(triggerName);

            if (_isUseDirectStatePlay)
                PlayAnimationState(stateName);
        }

        yield return new WaitForSeconds(Mathf.Max(0.01f, animationSeconds));

        _isPlayingOneShotAnimation = false;
        PlayAnimationState(_idleStateName);
        onAnimationEnd?.Invoke();

        _oneShotAnimationCoroutine = null;
    }

    private void StopOneShotAnimationCoroutine()
    {
        if (_oneShotAnimationCoroutine == null)
            return;

        StopCoroutine(_oneShotAnimationCoroutine);
        _oneShotAnimationCoroutine = null;
        _isPlayingOneShotAnimation = false;
    }

    private void PlayAnimationState(string stateName)
    {
        if (Animator_Jaeik == null)
            return;

        if (string.IsNullOrEmpty(stateName))
            return;

        if (_currentAnimationStateName == stateName)
            return;

        Animator_Jaeik.Play(stateName, 0, 0f);
        _currentAnimationStateName = stateName;
    }

    private void SetAnimatorMoving(bool isMoving)
    {
        if (Animator_Jaeik == null || string.IsNullOrEmpty(_isMovingParameterName))
            return;

        Animator_Jaeik.SetBool(_isMovingParameterName, isMoving);
    }

    private void SetAnimatorSpeed(float speed)
    {
        if (Animator_Jaeik != null)
            Animator_Jaeik.speed = Mathf.Max(0.01f, speed);
    }

    // ==================== 상호작용 프롬프트 ====================

    private bool IsNearInteractionTarget()
    {
        if (Transform_InteractionTarget == null)
            return false;

        float distance = Vector2.Distance(transform.position, Transform_InteractionTarget.position);
        return distance <= _interactionDistance;
    }

    private void SetInteractionPromptActive(bool isActive)
    {
        if (Group_InteractionPrompt == null)
            return;

        if (isActive)
            UpdateInteractionPromptPosition();

        Group_InteractionPrompt.SetActive(isActive);
    }

    private void UpdateInteractionPromptPosition()
    {
        if (Group_InteractionPrompt == null || Transform_InteractionTarget == null)
            return;

        Group_InteractionPrompt.transform.position = Transform_InteractionTarget.position + _interactionPromptOffset;
    }

    private void CreateDefaultPromptIfNeeded()
    {
        if (!_isCreateDefaultPrompt)
            return;

        if (Group_InteractionPrompt != null || Transform_InteractionTarget == null)
            return;

        Group_InteractionPrompt = new GameObject("Text_JaeikInteractionPrompt");
        Group_InteractionPrompt.transform.SetParent(Transform_InteractionTarget, false);
        Group_InteractionPrompt.transform.localPosition = _interactionPromptOffset;
        Group_InteractionPrompt.transform.localRotation = Quaternion.identity;
        Group_InteractionPrompt.transform.localScale = Vector3.one * _interactionPromptScale;

        Text_InteractionPrompt = Group_InteractionPrompt.AddComponent<TextMeshPro>();
        Text_InteractionPrompt.text = _interactionPromptText;
        Text_InteractionPrompt.fontSize = _interactionPromptFontSize;
        Text_InteractionPrompt.alignment = TextAlignmentOptions.Center;
        Text_InteractionPrompt.color = _interactionPromptColor;
        Text_InteractionPrompt.raycastTarget = false;
        Text_InteractionPrompt.textWrappingMode = TextWrappingModes.NoWrap;
        Text_InteractionPrompt.sortingOrder = _interactionPromptSortingOrder;

        HideInteractionPrompt();
    }
}
