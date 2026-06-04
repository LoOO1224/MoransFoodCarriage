// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: JangYoungSimController.cs
// - 역할: 캐릭터 배우의 이동, 입력, 애니메이션 상태를 담당합니다.
// - 감독 관점: 배우가 무대 위에서 어떻게 걷고 멈추고 반응하는지 정하는 연기 지도표입니다.
// - 유지보수 포인트: 장면 진행 순서는 Group Controller가 맡고, 캐릭터 스크립트는 자기 몸의 움직임만 맡게 합니다.
// =============================================================================
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

    /// <summary>
    /// 장영심이 등장할 때 기본 물리와 시작 이동 잠금 상태를 적용합니다.
    /// </summary>
    private void OnEnable()
    {
        ApplyDefaultPhysicsSetting();
        SetMovementLocked(_isMovementLockedOnStart);
    }

    /// <summary>
    /// WASD 입력, 방향 전환, Idle/Walk 애니메이션 상태를 갱신합니다.
    /// </summary>
    private void Update()
    {
        UpdateMoveInput();
        UpdateSpriteFlip();
        UpdateAnimationState();
    }

    /// <summary>
    /// Rigidbody2D 속도로 실제 장영심 위치를 이동시킵니다.
    /// </summary>
    private void FixedUpdate()
    {
        MovePlayer();
    }

    /// <summary>
    /// 튜토리얼 무대가 꺼질 때 연출 상태와 속도를 초기화합니다.
    /// </summary>
    private void OnDisable()
    {
        _isPlayingOneShotAnimation = false;
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

            if (!_isPlayingOneShotAnimation)
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

    /// <summary>
    /// 놀람 연출을 마지막 프레임에 고정하고 기본 애니메이션 갱신을 막습니다.
    /// </summary>
    public void HoldSurprisedAnimationLastFrame()
    {
        _isPlayingOneShotAnimation = true;
        _moveInput = Vector2.zero;
        StopPlayer();

        if (_animator == null || string.IsNullOrEmpty(_surprisedStateName))
            return;

        _animator.Play(_surprisedStateName, 0, 1f);
        _animator.Update(0f);
        _animator.speed = 0f;
        _currentAnimationStateName = _surprisedStateName;
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

    /// <summary>
    /// 이동 잠금이 풀려 있을 때만 WASD 입력을 읽습니다.
    /// </summary>
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

    /// <summary>
    /// 입력 방향에 맞춰 Rigidbody2D 속도를 적용합니다.
    /// </summary>
    private void MovePlayer()
    {
        if (_rigidbody == null)
            return;

        _rigidbody.linearVelocity = _moveInput * _moveSpeed;
    }

    /// <summary>
    /// 이동을 즉시 멈춥니다.
    /// </summary>
    private void StopPlayer()
    {
        if (_rigidbody != null)
            _rigidbody.linearVelocity = Vector2.zero;
    }

    // ==================== 방향 / 애니메이션 ====================

    /// <summary>
    /// 좌우 이동 방향에 맞춰 스프라이트를 뒤집습니다.
    /// </summary>
    private void UpdateSpriteFlip()
    {
        if (_spriteRenderer == null)
            return;

        if (Mathf.Abs(_moveInput.x) <= 0.01f)
            return;

        _spriteRenderer.flipX = _moveInput.x < 0f;
    }

    /// <summary>
    /// 이동 중이면 Walk, 멈춰 있으면 Idle 애니메이션으로 전환합니다.
    /// </summary>
    private void UpdateAnimationState()
    {
        if (_isPlayingOneShotAnimation)
            return;

        bool isMoving = !_isMovementLocked && _moveInput.sqrMagnitude > 0.01f;
        PlayAnimationState(isMoving ? _walkStateName : _idleStateName);
    }

    /// <summary>
    /// 같은 애니메이션을 반복 재생하지 않도록 상태 이름을 비교해 재생합니다.
    /// </summary>
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

    /// <summary>
    /// Animator 속도를 지정합니다.
    /// </summary>
    private void SetAnimatorSpeed(float animationSpeed)
    {
        if (_animator == null)
            return;

        _animator.speed = Mathf.Max(0.01f, animationSpeed);
    }
}
