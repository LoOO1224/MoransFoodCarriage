using UnityEngine;

/// <summary>
/// JaeikController
/// 
/// Senario1Group에서 재익의 다양한 상태(Walking, Eatting, Jumping)를 관리하는 컨트롤러입니다.
/// Idle, Walking은 루프, Eatting과 Jumping은 1회성 애니메이션으로 처리합니다.
/// </summary>
public class JaeikController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private bool _isMovementLockedOnStart = false;

    [Header("Component References")]
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;

    [Header("Animator State Names")]
    [SerializeField] private string _idleStateName = "Jaeik_Idle";
    [SerializeField] private string _walkStateName = "Jaeik_isWalking";
    [SerializeField] private string _eatingStateName = "Jaeik_isEatting";
    [SerializeField] private string _jumpingStateName = "Jaeik_isJumping";

    // ==================== 내부 상태 ====================
    private Vector2 _moveInput;
    private bool _isMovementLocked;
    private bool _isPlayingOneShotAnimation;
    private string _currentAnimationStateName;

    private void OnEnable()
    {
        ApplyDefaultPhysicsSetting();
        SetMovementLocked(_isMovementLockedOnStart);
    }

    private void Update()
    {
        UpdateMoveInput();
        UpdateSpriteFlip();
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void OnDisable()
    {
        SetAnimatorSpeed(1f);
        StopPlayer();
    }

    // ==================== 외부 제어 ====================
    public void LockMovement()
    {
        SetMovementLocked(true);
    }

    public void UnlockMovement()
    {
        SetMovementLocked(false);
    }

    public void SetMovementLocked(bool isLocked)
    {
        _isMovementLocked = isLocked;
        if (_isMovementLocked)
        {
            _moveInput = Vector2.zero;
            StopPlayer();
            PlayAnimationState(_idleStateName);
        }
    }

    /// <summary>
    /// E 상호작용 시 먹는 애니메이션을 1회 재생합니다.
    /// </summary>
    public void PlayEatingAnimationOnce()
    {
        PlayOneShotAnimation(_eatingStateName);
    }

    /// <summary>
    /// Space 입력 시 점프 애니메이션을 1회 재생합니다.
    /// </summary>
    public void PlayJumpingAnimationOnce()
    {
        PlayOneShotAnimation(_jumpingStateName);
    }

    private void PlayOneShotAnimation(string stateName)
    {
        _isPlayingOneShotAnimation = true;
        _moveInput = Vector2.zero;
        StopPlayer();
        PlayAnimationState(stateName);
    }

    public void StopOneShotAnimation()
    {
        _isPlayingOneShotAnimation = false;
        PlayAnimationState(_idleStateName);
    }

    // ==================== 물리 설정 ====================
    private void ApplyDefaultPhysicsSetting()
    {
        if (_rigidbody == null) return;
        _rigidbody.gravityScale = 0f;
        _rigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    // ==================== 이동 처리 ====================
    private void UpdateMoveInput()
    {
        if (_isMovementLocked)
        {
            _moveInput = Vector2.zero;
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector2(moveX, moveY).normalized;
    }

    private void MovePlayer()
    {
        if (_rigidbody == null) return;
        _rigidbody.linearVelocity = _moveInput * _moveSpeed;
    }

    private void StopPlayer()
    {
        if (_rigidbody != null)
            _rigidbody.linearVelocity = Vector2.zero;
    }

    // ==================== 방향 / 애니메이션 ====================
    private void UpdateSpriteFlip()
    {
        if (_spriteRenderer == null) return;
        if (Mathf.Abs(_moveInput.x) <= 0.01f) return;

        _spriteRenderer.flipX = _moveInput.x < 0f;
    }

    private void UpdateAnimationState()
    {
        if (_isPlayingOneShotAnimation) return;

        bool isMoving = !_isMovementLocked && _moveInput.sqrMagnitude > 0.01f;
        PlayAnimationState(isMoving ? _walkStateName : _idleStateName);
    }

    private void PlayAnimationState(string stateName)
    {
        if (_animator == null) return;
        if (string.IsNullOrEmpty(stateName)) return;
        if (_currentAnimationStateName == stateName) return;

        _animator.Play(stateName);
        _currentAnimationStateName = stateName;
    }

    private void SetAnimatorSpeed(float speed)
    {
        if (_animator != null)
            _animator.speed = Mathf.Max(0.01f, speed);
    }
}