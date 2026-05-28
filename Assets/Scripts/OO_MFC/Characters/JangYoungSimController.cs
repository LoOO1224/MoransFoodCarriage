using UnityEngine;

/// <summary>
/// Tutorial1Group에서 장영심의 WASD 이동과 기본 애니메이션 상태를 관리합니다.
/// 튜토리얼 가이드가 열려 있는 동안에는 이동을 잠그고, 가이드 종료 후 외부 컨트롤러가 이동을 해제합니다.
/// </summary>
public class JangYoungSimController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private bool _isMovementLockedOnStart = true;

    [Header("Component References")]
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;

    [Header("Animator State Names")]
    [SerializeField] private string _idleStateName = "JangYoungSim_Idle";
    [SerializeField] private string _walkStateName = "JangYoungSim_Walk";
    [SerializeField] private string _surprisedStateName = "JangYoungSim_Surprised";

    // ==================== 이동 상태 ====================
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

    /// <summary>
    /// 튜토리얼 가이드처럼 플레이어 조작을 막아야 할 때 호출합니다.
    /// </summary>
    public void LockMovement()
    {
        SetMovementLocked(true);
    }

    /// <summary>
    /// 튜토리얼 가이드가 끝난 뒤 플레이어 조작을 다시 허용할 때 호출합니다.
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
            PlayAnimationState(_idleStateName);
        }
    }

    /// <summary>
    /// 장영심의 놀람 애니메이션을 1회성 연출로 재생합니다.
    /// 이동 애니메이션 갱신이 해당 연출을 덮어쓰지 않도록 별도 상태로 잠급니다.
    /// </summary>
    public void PlaySurprisedAnimationOnce()
    {
        PlaySurprisedAnimationOnce(1f);
    }

    /// <summary>
    /// 장영심의 놀람 애니메이션을 지정한 속도로 1회성 재생합니다.
    /// 모란 기상 연출과 속도를 맞춰야 할 때 Tutorial1Controller에서 호출합니다.
    /// </summary>
    public void PlaySurprisedAnimationOnce(float animationSpeed)
    {
        _isPlayingOneShotAnimation = true;
        _moveInput = Vector2.zero;
        StopPlayer();
        SetAnimatorSpeed(animationSpeed);
        PlayAnimationState(_surprisedStateName);
    }

    /// <summary>
    /// 1회성 연출을 종료하고 Idle 상태로 되돌립니다.
    /// </summary>
    public void StopOneShotAnimation()
    {
        _isPlayingOneShotAnimation = false;
        SetAnimatorSpeed(1f);
        PlayAnimationState(_idleStateName);
    }

    // ==================== 물리 설정 ====================

    /// <summary>
    /// 2D 필드 이동에 필요한 기본 물리 설정을 보정합니다.
    /// Gravity는 0으로 고정하고, 회전은 Freeze Rotation Z 상태를 유지합니다.
    /// </summary>
    private void ApplyDefaultPhysicsSetting()
    {
        if (_rigidbody == null)
            return;

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
        if (_rigidbody == null)
            return;

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
        if (_spriteRenderer == null)
            return;

        if (Mathf.Abs(_moveInput.x) <= 0.01f)
            return;

        _spriteRenderer.flipX = _moveInput.x < 0f;
    }

    private void UpdateAnimationState()
    {
        if (_isPlayingOneShotAnimation)
            return;

        bool isMoving = !_isMovementLocked && _moveInput.sqrMagnitude > 0.01f;
        PlayAnimationState(isMoving ? _walkStateName : _idleStateName);
    }

    private void PlayAnimationState(string stateName)
    {
        if (_animator == null)
            return;

        if (string.IsNullOrEmpty(stateName))
            return;

        if (_currentAnimationStateName == stateName)
            return;

        _animator.Play(stateName);
        _currentAnimationStateName = stateName;
    }

    private void SetAnimatorSpeed(float animationSpeed)
    {
        if (_animator == null)
            return;

        _animator.speed = Mathf.Max(0.01f, animationSpeed);
    }
}
