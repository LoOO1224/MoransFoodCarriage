using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Jaeik의 이동, 점프 물리, S1_Object_Quest 상호작용을 담당합니다.
/// Game View에서는 플레이어가 WASD/마우스/Space/E로 재익을 조종하는 배우 컨트롤러입니다.
/// </summary>
public class OOTechJaeikController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _jumpVelocity = 19.5f;
    [SerializeField] private float _gravityScale = 4.8f;
    [SerializeField] private float _apexGravityScale = 2.2f;
    [SerializeField] private float _fallGravityScale = 8.8f;
    [SerializeField] private float _maxFallSpeed = 24f;
    [SerializeField] private float _coyoteTimeSeconds = 0.12f;
    [SerializeField] private float _minimumJumpInterval = 0.18f;
    [SerializeField] private bool _isMovementLockedOnStart = false;
    [SerializeField] private bool _isUseClickMove = true;
    [SerializeField] private float _clickMoveStopDistance = 0.16f;
    [SerializeField] private float _clickMoveMinWorldDistance = 0.08f;

    [Header("Ground Check")]
    [SerializeField] private Collider2D Collider_Jaeik;
    [SerializeField] private LayerMask _groundLayerMask = ~0;
    [SerializeField] private float _groundCheckDistance = 0.1f;
    [SerializeField] private float _groundNormalYThreshold = 0.35f;
    [SerializeField] private float _groundCheckIgnoreAfterJumpSeconds = 0.08f;
    [SerializeField] private bool _isCreateColliderIfMissing = true;
    [SerializeField] private Vector2 _fallbackColliderSize = new Vector2(1f, 1.8f);
    [SerializeField] private Vector2 _fallbackColliderOffset = new Vector2(0f, 0.85f);

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
    [SerializeField] private string _transformedStateName = "Jaeik_isTransformed";
    [SerializeField] private float _jumpAnimationMinimumSeconds = 0.25f;
    [SerializeField] private float _eatAnimationSeconds = 2.15f;
    [SerializeField] private float _transformedAnimationSeconds = 2.1f;
    [SerializeField] private bool _isUseDirectStatePlay = true;

    [Header("Interaction Settings")]
    [SerializeField] private Transform Transform_InteractionTarget;
    [SerializeField] private float _interactionDistance = 2.8f;
    [SerializeField] private float _minimumInteractionDistance = 3.3f;
    [SerializeField] private KeyCode _interactionKey = KeyCode.E;
    [SerializeField] private bool _isInteractionCompleted = false;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject Group_InteractionPrompt;
    [SerializeField] private TextMeshPro Text_InteractionPrompt;
    [SerializeField] private string _interactionPromptText = "[E] Eat";
    [SerializeField] private Vector3 _interactionPromptOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private float _interactionPromptScale = 0.18f;
    [SerializeField] private float _interactionPromptFontSize = 6f;
    [SerializeField] private Color _interactionPromptColor = new Color(1f, 0.86f, 0.18f, 1f);
    [SerializeField] private int _interactionPromptSortingOrder = 60;
    [SerializeField] private bool _isCreateDefaultPrompt = true;

    public bool IsGrounded { get; private set; }
    public bool IsInputEnabled { get; private set; }

    private readonly RaycastHit2D[] _groundHitArray = new RaycastHit2D[8];
    private readonly ContactPoint2D[] _contactPointArray = new ContactPoint2D[8];

    private Action _eatInteractionRequestEvent;
    private Coroutine _oneShotAnimationCoroutine;
    private string _currentAnimationStateName;
    private float _moveInputX;
    private float _clickMoveTargetX;
    private float _lastGroundedTime;
    private float _lastJumpTime = -999f;
    private bool _isMovementLocked;
    private bool _isClickMoveActive;
    private bool _isPlayingOneShotAnimation;

    /// <summary>
    /// Rigidbody, SpriteRenderer, Animator, Collider를 준비하고 E 상호작용 표시를 세팅합니다.
    /// </summary>
    private void Awake()
    {
        CacheComponentReferences();
        CreateFallbackColliderIfNeeded();
        CreateDefaultInteractionPromptIfNeeded();
    }

    /// <summary>
    /// 재익이 무대에 등장할 때 점프 가능한 물리 세팅과 입력 잠금 상태를 적용합니다.
    /// </summary>
    private void OnEnable()
    {
        ApplyRequiredScenarioPhysics();
        ApplyDefaultPhysicsSetting();
        SetMovementLocked(_isMovementLockedOnStart);
        SetInputEnabled(!_isMovementLockedOnStart);
        HideInteractionPrompt();
    }

    private void Update()
    {
        // Update는 배우의 즉석 입력을 받는 시간입니다.
        // 감독이 입력을 잠근 장면에서는 이동, 점프, E 상호작용을 모두 받지 않습니다.
        RefreshGroundedState();

        if (!IsInputEnabled || _isMovementLocked)
        {
            _moveInputX = 0f;
            HideInteractionPrompt();
            return;
        }

        UpdateMoveInput();
        UpdateJumpInput();
        UpdateInteractionInput();
        UpdateInteractionPrompt();
        UpdateSpriteFlip();
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        // FixedUpdate는 무대 장치 담당입니다.
        // Rigidbody2D 이동과 중력은 물리 프레임에서 처리해야 계단/천장 콜리더와 안정적으로 맞물립니다.
        RefreshGroundedState();
        ApplyHorizontalMovement();
        ApplyGravityScale();
        ClampFallSpeed();
    }

    private void OnDisable()
    {
        StopOneShotAnimation();
        SetAnimatorSpeed(1f);
        StopPlayer();
        HideInteractionPrompt();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ResolveCollisionContact(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ResolveCollisionContact(collision);
    }

    public void SetComponentReference(Rigidbody2D rigidbody, SpriteRenderer spriteRenderer, Animator animator, Collider2D collider)
    {
        if (rigidbody != null)
            Rigidbody_Jaeik = rigidbody;

        if (spriteRenderer != null)
            SpriteRenderer_Jaeik = spriteRenderer;

        if (animator != null)
            Animator_Jaeik = animator;

        if (collider != null)
            Collider_Jaeik = collider;
    }

    /// <summary>
    /// 튜토리얼/대화 중에는 플레이어 이동을 잠급니다.
    /// </summary>
    public void LockMovement()
    {
        SetMovementLocked(true);
    }

    /// <summary>
    /// 튜토리얼/대화가 끝나면 플레이어 이동을 다시 허용합니다.
    /// </summary>
    public void UnlockMovement()
    {
        SetMovementLocked(false);
    }

    /// <summary>
    /// 입력 자체를 켜고 끕니다. 꺼질 때는 현재 이동과 클릭 이동 목표를 함께 정리합니다.
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        IsInputEnabled = enabled;

        if (!IsInputEnabled)
        {
            _moveInputX = 0f;
            _isClickMoveActive = false;
            StopPlayer();
        }
    }

    /// <summary>
    /// 외부 감독 스크립트가 이동 잠금 상태를 직접 지정할 때 사용합니다.
    /// </summary>
    public void SetMovementLocked(bool isLocked)
    {
        _isMovementLocked = isLocked;

        if (!_isMovementLocked)
            return;

        _moveInputX = 0f;
        _isClickMoveActive = false;
        StopPlayer();

        if (!_isPlayingOneShotAnimation)
            PlayAnimationState(_idleStateName);
    }

    /// <summary>
    /// E키 상호작용 대상, 즉 음식 오브젝트 위치를 연결합니다.
    /// </summary>
    public void SetInteractionTarget(Transform target)
    {
        Transform_InteractionTarget = target;
        UpdateInteractionPrompt();
    }

    /// <summary>
    /// E키가 반응하는 거리 값을 설정합니다.
    /// </summary>
    public void SetInteractionDistance(float distance)
    {
        _interactionDistance = Mathf.Max(0.1f, distance);
    }

    /// <summary>
    /// 음식 상호작용이 끝났는지 기록해 E 프롬프트가 다시 뜨지 않게 합니다.
    /// </summary>
    public void SetInteractionCompleted(bool isCompleted)
    {
        _isInteractionCompleted = isCompleted;

        if (_isInteractionCompleted)
            HideInteractionPrompt();
    }

    /// <summary>
    /// 음식 E 상호작용이 들어왔을 때 Scenario1Controller가 받을 콜백을 등록합니다.
    /// </summary>
    public void BindEatInteractionRequestEvent(Action callback)
    {
        _eatInteractionRequestEvent -= callback;
        _eatInteractionRequestEvent += callback;
    }

    /// <summary>
    /// 음식 E 상호작용 콜백을 해제합니다.
    /// </summary>
    public void UnbindEatInteractionRequestEvent(Action callback)
    {
        _eatInteractionRequestEvent -= callback;
    }

    /// <summary>
    /// 먹기 애니메이션을 기본 속도 0.3으로 한 번 재생합니다.
    /// </summary>
    public void PlayEatAnimationOnce(Action onComplete = null)
    {
        PlayEatAnimationOnce(0.3f, onComplete);
    }

    /// <summary>
    /// 먹기 애니메이션을 지정 속도로 한 번 재생하고 끝나면 콜백을 호출합니다.
    /// </summary>
    public void PlayEatAnimationOnce(float animationSpeed, Action onComplete = null)
    {
        StartOneShotAnimation(_eatStateName, _eatAnimationSeconds, animationSpeed, false, onComplete);
    }

    /// <summary>
    /// 변신 애니메이션을 한 번 재생하고 마지막 프레임에 멈춥니다.
    /// </summary>
    public void PlayTransformedAnimationOnce(float animationSpeed, Action onComplete = null)
    {
        StartOneShotAnimation(_transformedStateName, _transformedAnimationSeconds, animationSpeed, true, onComplete);
    }

    /// <summary>
    /// 점프 애니메이션을 짧은 1회성 연출로 재생합니다.
    /// </summary>
    public void PlayJumpingAnimationOnce(Action onComplete = null)
    {
        StartOneShotAnimation(_jumpStateName, _jumpAnimationMinimumSeconds, 1f, false, onComplete);
    }

    /// <summary>
    /// 먹기/변신 같은 1회성 애니메이션 코루틴을 정리합니다.
    /// </summary>
    public void StopOneShotAnimation()
    {
        if (_oneShotAnimationCoroutine != null)
        {
            StopCoroutine(_oneShotAnimationCoroutine);
            _oneShotAnimationCoroutine = null;
        }

        _isPlayingOneShotAnimation = false;
        SetAnimatorSpeed(1f);
    }

    /// <summary>
    /// E 상호작용 안내 문구를 숨깁니다.
    /// </summary>
    public void HideInteractionPrompt()
    {
        if (Group_InteractionPrompt != null)
            Group_InteractionPrompt.SetActive(false);
    }

    private void CacheComponentReferences()
    {
        if (Rigidbody_Jaeik == null)
            Rigidbody_Jaeik = GetComponent<Rigidbody2D>();

        if (SpriteRenderer_Jaeik == null)
            SpriteRenderer_Jaeik = GetComponent<SpriteRenderer>();

        if (Animator_Jaeik == null)
            Animator_Jaeik = GetComponent<Animator>();

        if (Collider_Jaeik == null)
            Collider_Jaeik = FindBestBodyCollider();
    }

    private Collider2D FindBestBodyCollider()
    {
        Collider2D[] colliderArray = GetComponents<Collider2D>();

        foreach (Collider2D collider in colliderArray)
        {
            if (collider != null && collider.enabled && !collider.isTrigger && collider is BoxCollider2D)
                return collider;
        }

        foreach (Collider2D collider in colliderArray)
        {
            if (collider != null && collider.enabled && !collider.isTrigger)
                return collider;
        }

        return null;
    }

    private void CreateFallbackColliderIfNeeded()
    {
        if (!_isCreateColliderIfMissing || Collider_Jaeik != null)
            return;

        BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.size = _fallbackColliderSize;
        boxCollider.offset = _fallbackColliderOffset;
        Collider_Jaeik = boxCollider;
    }

    private void ApplyRequiredScenarioPhysics()
    {
        _jumpVelocity = Mathf.Max(_jumpVelocity, 19.5f);
        _gravityScale = Mathf.Max(_gravityScale, 4.8f);
        _apexGravityScale = Mathf.Clamp(_apexGravityScale, 1.2f, _gravityScale);
        _fallGravityScale = Mathf.Max(_fallGravityScale, 8.8f);
        _maxFallSpeed = Mathf.Max(_maxFallSpeed, 24f);
    }

    private void ApplyDefaultPhysicsSetting()
    {
        if (Rigidbody_Jaeik == null)
            return;

        Rigidbody_Jaeik.bodyType = RigidbodyType2D.Dynamic;
        Rigidbody_Jaeik.gravityScale = _gravityScale;
        Rigidbody_Jaeik.constraints |= RigidbodyConstraints2D.FreezeRotation;
        Rigidbody_Jaeik.interpolation = RigidbodyInterpolation2D.Interpolate;
        Rigidbody_Jaeik.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    /// <summary>
    /// 키보드 입력이 없을 때 마우스 클릭 이동 목표를 확인합니다.
    /// </summary>
    private void UpdateMoveInput()
    {
        float keyboardX = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(keyboardX) > 0.01f)
        {
            _isClickMoveActive = false;
            _moveInputX = Mathf.Sign(keyboardX);
            return;
        }

        TrySetClickMoveTarget();
        _moveInputX = GetClickMoveInputX();
    }

    /// <summary>
    /// 화면을 클릭한 지점을 월드 좌표로 바꿔 재익이 그 방향으로 걷게 합니다.
    /// </summary>
    private void TrySetClickMoveTarget()
    {
        if (!_isUseClickMove || !Input.GetMouseButtonDown(0))
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Camera camera = Camera.main;
        if (camera == null)
            return;

        Vector3 clickWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);

        if (Mathf.Abs(clickWorldPosition.x - transform.position.x) < _clickMoveMinWorldDistance)
            return;

        _clickMoveTargetX = clickWorldPosition.x;
        _isClickMoveActive = true;
    }

    private float GetClickMoveInputX()
    {
        if (!_isClickMoveActive)
            return 0f;

        float distanceX = _clickMoveTargetX - transform.position.x;

        if (Mathf.Abs(distanceX) <= _clickMoveStopDistance)
        {
            _isClickMoveActive = false;
            return 0f;
        }

        return Mathf.Sign(distanceX);
    }

    /// <summary>
    /// Space 입력을 받으면 점프 큐를 요청합니다.
    /// </summary>
    private void UpdateJumpInput()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        RequestJump();
    }

    private void RequestJump()
    {
        // 점프는 배우를 와이어로 들어 올리는 큐와 비슷합니다.
        // Space를 누르면 위쪽 속도를 한 번 크게 주고, 이후 낙하는 Physics2D 중력에게 맡깁니다.
        if (!CanJump())
            return;

        _lastJumpTime = Time.time;
        IsGrounded = false;

        if (Rigidbody_Jaeik != null)
            Rigidbody_Jaeik.linearVelocity = new Vector2(Rigidbody_Jaeik.linearVelocity.x, _jumpVelocity);

        TriggerAnimator(_jumpTriggerName);
        PlayAnimationState(_jumpStateName, true);
    }

    /// <summary>
    /// 점프 가능 조건을 확인합니다. 바닥에 있거나 아주 짧은 코요테 타임 안에서만 허용합니다.
    /// </summary>
    private bool CanJump()
    {
        if (_isPlayingOneShotAnimation)
            return false;

        if (Time.time - _lastJumpTime < _minimumJumpInterval)
            return false;

        return IsGrounded || Time.time - _lastGroundedTime <= _coyoteTimeSeconds;
    }

    /// <summary>
    /// 음식 근처에서 E키를 누르면 상호작용 이벤트를 한 번만 발생시킵니다.
    /// </summary>
    private void UpdateInteractionInput()
    {
        if (_isInteractionCompleted || Transform_InteractionTarget == null)
            return;

        if (!IsNearInteractionTarget())
            return;

        if (!Input.GetKeyDown(_interactionKey))
            return;

        _isInteractionCompleted = true;
        HideInteractionPrompt();
        _eatInteractionRequestEvent?.Invoke();
    }

    /// <summary>
    /// 재익과 음식 오브젝트 사이의 거리가 상호작용 범위 안인지 확인합니다.
    /// </summary>
    private bool IsNearInteractionTarget()
    {
        if (Transform_InteractionTarget == null)
            return false;

        float distance = Vector2.Distance(transform.position, Transform_InteractionTarget.position);
        return distance <= Mathf.Max(_interactionDistance, _minimumInteractionDistance);
    }

    /// <summary>
    /// 음식 근처에 있을 때만 E 안내 문구를 음식 위에 표시합니다.
    /// </summary>
    private void UpdateInteractionPrompt()
    {
        if (_isInteractionCompleted || Transform_InteractionTarget == null)
        {
            HideInteractionPrompt();
            return;
        }

        bool isActive = IsInputEnabled && !_isMovementLocked && IsNearInteractionTarget();
        SetInteractionPromptActive(isActive);

        if (!isActive || Group_InteractionPrompt == null)
            return;

        Group_InteractionPrompt.transform.position = Transform_InteractionTarget.position + _interactionPromptOffset;
    }

    /// <summary>
    /// 씬에 E 프롬프트가 없을 경우 재익 자식으로 최소 텍스트 프롬프트를 준비합니다.
    /// </summary>
    private void CreateDefaultInteractionPromptIfNeeded()
    {
        if (!_isCreateDefaultPrompt || Group_InteractionPrompt != null)
            return;

        Group_InteractionPrompt = new GameObject("Text_JaeikInteractionPrompt");
        Group_InteractionPrompt.transform.SetParent(transform, false);
        Group_InteractionPrompt.transform.localPosition = _interactionPromptOffset;
        Group_InteractionPrompt.transform.localScale = Vector3.one * Mathf.Max(_interactionPromptScale, 0.28f);

        Text_InteractionPrompt = Group_InteractionPrompt.AddComponent<TextMeshPro>();
        Text_InteractionPrompt.text = _interactionPromptText;
        Text_InteractionPrompt.fontSize = Mathf.Max(_interactionPromptFontSize, 7f);
        Text_InteractionPrompt.alignment = TextAlignmentOptions.Center;
        Text_InteractionPrompt.color = _interactionPromptColor;
        Text_InteractionPrompt.raycastTarget = false;
        Text_InteractionPrompt.textWrappingMode = TextWrappingModes.NoWrap;
        Text_InteractionPrompt.sortingOrder = Mathf.Max(_interactionPromptSortingOrder, 160);
    }

    private void SetInteractionPromptActive(bool isActive)
    {
        if (Group_InteractionPrompt == null)
            return;

        if (Group_InteractionPrompt.activeSelf != isActive)
            Group_InteractionPrompt.SetActive(isActive);
    }

    /// <summary>
    /// Rigidbody2D에 가로 이동 속도를 적용합니다.
    /// </summary>
    private void ApplyHorizontalMovement()
    {
        if (Rigidbody_Jaeik == null)
            return;

        if (!IsInputEnabled || _isMovementLocked || _isPlayingOneShotAnimation)
        {
            Rigidbody_Jaeik.linearVelocity = new Vector2(0f, Rigidbody_Jaeik.linearVelocity.y);
            return;
        }

        Rigidbody_Jaeik.linearVelocity = new Vector2(_moveInputX * _moveSpeed, Rigidbody_Jaeik.linearVelocity.y);
    }

    private void StopPlayer()
    {
        if (Rigidbody_Jaeik != null)
            Rigidbody_Jaeik.linearVelocity = Vector2.zero;
    }

    private void RefreshGroundedState()
    {
        // isGrounded는 배우 발이 무대 바닥에 닿았는지 확인하는 장치입니다.
        // 이 값이 true일 때만 다음 점프를 허용해서 공중 2단 점프를 막습니다.
        bool isGrounded = CheckGroundByCast();

        if (!isGrounded && Rigidbody_Jaeik != null && Mathf.Abs(Rigidbody_Jaeik.linearVelocity.y) < 0.01f)
            isGrounded = CheckGroundByOverlapFallback();

        IsGrounded = isGrounded;

        if (IsGrounded)
            _lastGroundedTime = Time.time;
    }

    private bool CheckGroundByCast()
    {
        if (Collider_Jaeik == null)
            return false;

        if (Time.time - _lastJumpTime < _groundCheckIgnoreAfterJumpSeconds)
            return false;

        ContactFilter2D contactFilter = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = true,
            layerMask = _groundLayerMask
        };

        int hitCount = Collider_Jaeik.Cast(Vector2.down, contactFilter, _groundHitArray, _groundCheckDistance);

        for (int i = 0; i < hitCount; i++)
        {
            if (_groundHitArray[i].collider == null)
                continue;

            if (_groundHitArray[i].normal.y >= _groundNormalYThreshold)
                return true;
        }

        return false;
    }

    private bool CheckGroundByOverlapFallback()
    {
        if (Collider_Jaeik == null)
            return false;

        ContactFilter2D contactFilter = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = true,
            layerMask = _groundLayerMask
        };

        int contactCount = Collider_Jaeik.GetContacts(contactFilter, _contactPointArray);

        for (int i = 0; i < contactCount; i++)
        {
            if (_contactPointArray[i].normal.y >= _groundNormalYThreshold)
                return true;
        }

        return false;
    }

    private void ApplyGravityScale()
    {
        // 상승 끝부분에서는 살짝 느리게, 떨어질 때는 더 빠르게 중력을 바꿉니다.
        // 화면에서는 크게 튀어 오른 뒤 너무 둥둥 떠 보이지 않고 콜리더 위에 빨리 안착합니다.
        if (Rigidbody_Jaeik == null)
            return;

        float velocityY = Rigidbody_Jaeik.linearVelocity.y;

        if (IsGrounded && velocityY <= 0.01f)
        {
            Rigidbody_Jaeik.gravityScale = _gravityScale;
            return;
        }

        if (Mathf.Abs(velocityY) < 1.2f)
        {
            Rigidbody_Jaeik.gravityScale = _apexGravityScale;
            return;
        }

        Rigidbody_Jaeik.gravityScale = velocityY < 0f ? _fallGravityScale : _gravityScale;
    }

    private void ClampFallSpeed()
    {
        if (Rigidbody_Jaeik == null)
            return;

        if (Rigidbody_Jaeik.linearVelocity.y >= -_maxFallSpeed)
            return;

        Rigidbody_Jaeik.linearVelocity = new Vector2(Rigidbody_Jaeik.linearVelocity.x, -_maxFallSpeed);
    }

    private void ResolveCollisionContact(Collision2D collision)
    {
        int contactCount = collision.GetContacts(_contactPointArray);

        for (int i = 0; i < contactCount; i++)
        {
            Vector2 normal = _contactPointArray[i].normal;

            if (normal.y >= _groundNormalYThreshold)
            {
                IsGrounded = true;
                _lastGroundedTime = Time.time;
            }

            if (normal.y <= -_groundNormalYThreshold)
                StopUpwardVelocity();
        }
    }

    /// <summary>
    /// 천장에 부딪히면 위쪽 속도를 즉시 0으로 만들어 다시 떨어지게 합니다.
    /// </summary>
    private void StopUpwardVelocity()
    {
        if (Rigidbody_Jaeik == null || Rigidbody_Jaeik.linearVelocity.y <= 0f)
            return;

        Rigidbody_Jaeik.linearVelocity = new Vector2(Rigidbody_Jaeik.linearVelocity.x, 0f);
    }

    /// <summary>
    /// 이동 방향에 맞춰 스프라이트 좌우 반전을 적용합니다.
    /// </summary>
    private void UpdateSpriteFlip()
    {
        if (SpriteRenderer_Jaeik == null || Mathf.Abs(_moveInputX) <= 0.01f)
            return;

        SpriteRenderer_Jaeik.flipX = _moveInputX < 0f;
    }

    /// <summary>
    /// 이동/점프/Idle 상태를 현재 입력과 바닥 판정에 맞춰 갱신합니다.
    /// </summary>
    private void UpdateAnimationState()
    {
        if (_isPlayingOneShotAnimation || Animator_Jaeik == null)
            return;

        bool isMoving = Mathf.Abs(_moveInputX) > 0.01f && IsGrounded;
        SetAnimatorBool(_isMovingParameterName, isMoving);

        if (!IsGrounded)
        {
            PlayAnimationState(_jumpStateName);
            return;
        }

        PlayAnimationState(isMoving ? _walkStateName : _idleStateName);
    }

    /// <summary>
    /// 먹기/변신처럼 플레이어 입력을 잠그는 1회성 애니메이션을 시작합니다.
    /// </summary>
    private void StartOneShotAnimation(string stateName, float animationSeconds, float animationSpeed, bool isHoldLastFrame, Action onComplete)
    {
        StopOneShotAnimation();

        _oneShotAnimationCoroutine = StartCoroutine(PlayOneShotAnimationRoutine(stateName, animationSeconds, animationSpeed, isHoldLastFrame, onComplete));
    }

    /// <summary>
    /// 1회성 애니메이션 동안 이동을 멈추고, 끝나면 Idle 복귀 또는 마지막 프레임 고정을 처리합니다.
    /// </summary>
    private IEnumerator PlayOneShotAnimationRoutine(string stateName, float animationSeconds, float animationSpeed, bool isHoldLastFrame, Action onComplete)
    {
        _isPlayingOneShotAnimation = true;
        _moveInputX = 0f;
        _isClickMoveActive = false;
        StopPlayer();
        HideInteractionPrompt();
        TriggerAnimator(stateName == _eatStateName ? _eatTriggerName : string.Empty);
        SetAnimatorSpeed(animationSpeed);
        PlayAnimationState(stateName, true);

        float waitSeconds = Mathf.Max(0.05f, animationSeconds / Mathf.Max(0.01f, animationSpeed));
        yield return new WaitForSeconds(waitSeconds);

        if (isHoldLastFrame)
        {
            SetAnimatorSpeed(0f);
        }
        else
        {
            SetAnimatorSpeed(1f);
            _isPlayingOneShotAnimation = false;
            PlayAnimationState(_idleStateName, true);
        }

        _oneShotAnimationCoroutine = null;
        onComplete?.Invoke();
    }

    private void PlayAnimationState(string stateName, bool isForce = false)
    {
        if (!_isUseDirectStatePlay || Animator_Jaeik == null || string.IsNullOrEmpty(stateName))
            return;

        if (!isForce && _currentAnimationStateName == stateName)
            return;

        PlayAnimatorState(Animator_Jaeik, stateName);
        _currentAnimationStateName = stateName;
    }

    private void TriggerAnimator(string triggerName)
    {
        if (Animator_Jaeik == null || string.IsNullOrEmpty(triggerName))
            return;

        if (!HasAnimatorParameter(Animator_Jaeik, triggerName, AnimatorControllerParameterType.Trigger))
            return;

        Animator_Jaeik.SetTrigger(triggerName);
    }

    private void SetAnimatorBool(string parameterName, bool value)
    {
        if (Animator_Jaeik == null || string.IsNullOrEmpty(parameterName))
            return;

        if (HasAnimatorParameter(Animator_Jaeik, parameterName, AnimatorControllerParameterType.Bool))
            Animator_Jaeik.SetBool(parameterName, value);
    }

    private void SetAnimatorSpeed(float speed)
    {
        if (Animator_Jaeik != null)
            Animator_Jaeik.speed = Mathf.Max(0f, speed);
    }

    private bool HasAnimatorParameter(Animator animator, string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (animator == null || string.IsNullOrEmpty(parameterName))
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == parameterType && parameter.name == parameterName)
                return true;
        }

        return false;
    }

    private void PlayAnimatorState(Animator animator, string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            return;

        int shortHash = Animator.StringToHash(stateName);
        int fullPathHash = Animator.StringToHash("Base Layer." + stateName);

        if (animator.HasState(0, shortHash))
        {
            animator.Play(shortHash, 0, 0f);
            return;
        }

        if (animator.HasState(0, fullPathHash))
        {
            animator.Play(fullPathHash, 0, 0f);
            return;
        }

        animator.Play(stateName, 0, 0f);
    }
}
