// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechStageActorMotion.cs
// - 역할: Stage 배우 한 명의 이동, 점프, 애니메이션 상태를 담당하는 컴포넌트입니다.
// - 영화 비유: 무대감독이 배우 팔을 직접 잡아 움직이지 않고, 배우에게 "입장/대기/점프" 역할표를 붙여 스스로 연기하게 합니다.
// - 유지보수 포인트: Controller는 큐 순서만 부르고, 실제 움직임과 애니메이션 이름은 이 배우 컴포넌트에서 관리합니다.
// =============================================================================
using System.Collections;
using UnityEngine;

/// <summary>
/// Stage2 같은 연출 무대에서 캐릭터 배우에게 붙는 이동/애니메이션 역할 컴포넌트입니다.
/// Game View에서는 자동 입장, 플레이어 조작, 점프, Victory 같은 상태 전환을 이 컴포넌트가 담당합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechStageActorMotion : MonoBehaviour
{
    [Header("Animation State")]
    [SerializeField] private string _idleStateName = "Idle";
    [SerializeField] private string _walkingStateName = "Walk";
    [SerializeField] private string _runningStateName = "Run";
    [SerializeField] private string _jumpingStateName = "Jump";
    [SerializeField] private string _victoryStateName = "Victory";
    [SerializeField] private string _reactionStateName = "";

    [Header("Movement")]
    [SerializeField] private float _autoMoveSpeed = 130f;
    [SerializeField] private float _walkSpeed = 180f;
    [SerializeField] private float _runSpeed = 320f;
    [SerializeField] private float _jumpHeight = 145f;
    [SerializeField] private float _jumpDuration = 0.46f;
    [SerializeField] private bool _isFlipByMoveDirection = true;

    [Header("Physics Guard")]
    [SerializeField] private bool _isLockRigidbodyGravityOnStage = true;
    [SerializeField] private bool _isFreezeRigidbodyRotation = true;

    [Header("Input")]
    [SerializeField] private KeyCode _moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode _moveRightKey = KeyCode.D;
    [SerializeField] private KeyCode _runKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode _jumpKey = KeyCode.Space;

    private Animator Animator_Actor;
    private SpriteRenderer Renderer_Actor;
    private Rigidbody2D Rigidbody_Actor;
    private Coroutine Coroutine_Jump;
    private Bounds _movementBounds;
    private bool _hasMovementBounds;
    private bool _isPlayerInputEnabled;
    private bool _isJumping;
    private float _groundY;
    private string _currentStateName;
    private bool _hasSavedRigidbodyState;
    private float _savedGravityScale;
    private RigidbodyConstraints2D _savedConstraints;

    public bool IsPlayerInputEnabled => _isPlayerInputEnabled;

    /// <summary>
    /// 배우가 켜질 때 자기 몸에 붙은 Animator와 SpriteRenderer를 찾아 둡니다.
    /// 무대 위 배우가 자기 의상과 동작표를 확인하는 준비 단계입니다.
    /// </summary>
    private void Awake()
    {
        ResolveReferences();
        ApplyStagePhysicsGuard();
    }

    /// <summary>
    /// 배우가 다시 켜질 때 Rigidbody2D 중력이 자동 이동 큐를 무너뜨리지 않게 무대용 물리 상태를 준비합니다.
    /// Game View에서는 배우가 바닥 아래로 꺼지지 않고 정해진 EntryPoint까지 연기합니다.
    /// </summary>
    private void OnEnable()
    {
        ResolveReferences();
        ApplyStagePhysicsGuard();
    }

    /// <summary>
    /// 무대를 떠날 때 원래 Rigidbody2D 설정을 되돌립니다.
    /// 같은 배우 프리팹을 다른 장면에서 재사용할 때 물리 설정이 새지 않게 막는 정리 큐입니다.
    /// </summary>
    private void OnDisable()
    {
        RestoreStagePhysicsGuard();
    }

    /// <summary>
    /// 플레이어 조작이 허용된 동안 A/D/Shift/Space 입력을 읽어 배우를 움직입니다.
    /// </summary>
    private void Update()
    {
        if (!_isPlayerInputEnabled)
            return;

        UpdatePlayerMovement();
    }

    /// <summary>
    /// 외부 Controller가 배우의 기본 애니메이션 이름을 한 번에 지정할 때 사용합니다.
    /// 같은 컴포넌트를 Moran, Chunyang, GreedyDuck에게 재사용하기 위한 설정 입구입니다.
    /// </summary>
    public void RequestSetupStateNames(string idleStateName, string walkingStateName, string runningStateName, string jumpingStateName, string victoryStateName, string reactionStateName)
    {
        _idleStateName = idleStateName;
        _walkingStateName = walkingStateName;
        _runningStateName = runningStateName;
        _jumpingStateName = jumpingStateName;
        _victoryStateName = victoryStateName;
        _reactionStateName = reactionStateName;
    }

    /// <summary>
    /// 자동 입장과 플레이어 조작 속도를 지정합니다.
    /// 영화로 치면 배우별 보폭과 뛰는 속도를 무대 상황에 맞게 조절하는 작업입니다.
    /// </summary>
    public void RequestSetupMovement(float autoMoveSpeed, float walkSpeed, float runSpeed, float jumpHeight, float jumpDuration)
    {
        _autoMoveSpeed = Mathf.Max(1f, autoMoveSpeed);
        _walkSpeed = Mathf.Max(1f, walkSpeed);
        _runSpeed = Mathf.Max(_walkSpeed, runSpeed);
        _jumpHeight = Mathf.Max(10f, jumpHeight);
        _jumpDuration = Mathf.Max(0.12f, jumpDuration);
    }

    /// <summary>
    /// 플레이어가 움직일 수 있는 무대 경계를 지정합니다.
    /// Game View에서는 Moran이 배경 밖으로 빠져나가지 않게 막는 투명 무대 벽 역할입니다.
    /// </summary>
    public void RequestSetMovementBounds(Bounds movementBounds)
    {
        _movementBounds = movementBounds;
        _hasMovementBounds = true;
    }

    /// <summary>
    /// 플레이어 조작 가능 여부를 바꿉니다.
    /// 큐 진행 중에는 false, 임무가 시작되면 true로 바뀝니다.
    /// </summary>
    public void RequestSetPlayerInputEnabled(bool isEnabled)
    {
        _isPlayerInputEnabled = isEnabled;
        _groundY = transform.position.y;

        if (!isEnabled)
            RequestPlayIdle();
    }

    /// <summary>
    /// 배우를 지정된 목표 지점까지 자동으로 걷게 합니다.
    /// 세 배우가 화면 밖에서 각자 EntryPoint까지 들어오는 입장 연출에 사용합니다.
    /// </summary>
    public IEnumerator MoveToTargetRoutine(Transform targetTransform, float speed)
    {
        if (targetTransform == null)
            yield break;

        RequestPlayState(_walkingStateName, 1f, true);

        while (Vector2.Distance(transform.position, targetTransform.position) > 1f)
        {
            ApplyStagePhysicsGuard();
            Vector3 beforePosition = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, Mathf.Max(1f, speed) * Time.deltaTime);
            UpdateFlipByDelta(transform.position - beforePosition);
            yield return null;
        }

        transform.position = targetTransform.position;
        RequestPlayIdle();
    }

    /// <summary>
    /// 현재 위치에서 X 방향으로 지정 거리만큼 빠르게 도망가게 합니다.
    /// GreedyDuck이 불타는 상태로 무대 밖으로 퇴장할 때 사용합니다.
    /// </summary>
    public IEnumerator MoveToWorldPositionRoutine(Vector3 targetPosition, float speed, string moveStateName)
    {
        RequestPlayState(moveStateName, 1f, true);

        while (Vector2.Distance(transform.position, targetPosition) > 1f)
        {
            ApplyStagePhysicsGuard();
            Vector3 beforePosition = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Mathf.Max(1f, speed) * Time.deltaTime);
            UpdateFlipByDelta(transform.position - beforePosition);
            yield return null;
        }

        transform.position = targetPosition;
    }

    /// <summary>
    /// 배우를 바로 대기 상태로 돌립니다.
    /// </summary>
    public void RequestPlayIdle()
    {
        RequestPlayState(_idleStateName, 1f, false);
    }

    /// <summary>
    /// 배우의 리액션 상태를 재생합니다.
    /// Moran 춤, Chunyang 부끄러움, Mr.Jaeik 놀람처럼 대사 사이 연기 큐에 사용합니다.
    /// </summary>
    public bool RequestPlayReaction(float speed = 1f)
    {
        if (!string.IsNullOrEmpty(_reactionStateName) && RequestPlayState(_reactionStateName, speed, true))
            return true;

        return RequestPlayState(_victoryStateName, speed, true);
    }

    /// <summary>
    /// Victory 상태를 계속 반복시키기 위한 공개 API입니다.
    /// Stage Clear 후 Moran이 다음 Road로 넘어가기 전까지 반복 연기합니다.
    /// </summary>
    public bool RequestPlayVictory(float speed = 1f)
    {
        return RequestPlayState(_victoryStateName, speed, true);
    }

    /// <summary>
    /// 지정된 애니메이션 상태를 재생합니다.
    /// 상태가 없으면 false를 반환해 Controller가 fallback을 판단할 수 있게 합니다.
    /// </summary>
    public bool RequestPlayState(string stateName, float speed = 1f, bool isForceReplay = false)
    {
        ResolveReferences();

        if (Animator_Actor == null || string.IsNullOrEmpty(stateName))
            return false;

        if (!HasAnimatorState(stateName))
            return false;

        Animator_Actor.speed = Mathf.Max(0.01f, speed);

        if (!isForceReplay && _currentStateName == stateName)
            return true;

        _currentStateName = stateName;
        Animator_Actor.Play(stateName, 0, 0f);
        return true;
    }

    /// <summary>
    /// 현재 Animator 안에 상태가 있는지 확인합니다.
    /// 감독이 없는 동작을 시키려고 할 때 콘솔 경고 전에 안전하게 막는 확인표입니다.
    /// </summary>
    public bool HasAnimatorState(string stateName)
    {
        ResolveReferences();

        if (Animator_Actor == null || string.IsNullOrEmpty(stateName))
            return false;

        int stateHash = Animator.StringToHash(stateName);

        for (int layerIndex = 0; layerIndex < Animator_Actor.layerCount; layerIndex++)
        {
            if (Animator_Actor.HasState(layerIndex, stateHash))
                return true;
        }

        return false;
    }

    private void ResolveReferences()
    {
        if (Animator_Actor == null)
            Animator_Actor = GetComponentInChildren<Animator>(true);

        if (Renderer_Actor == null)
            Renderer_Actor = GetComponentInChildren<SpriteRenderer>(true);

        if (Rigidbody_Actor == null)
            Rigidbody_Actor = GetComponentInChildren<Rigidbody2D>(true);
    }

    /// <summary>
    /// Stage2의 자동이동은 Transform 큐를 쓰므로 Rigidbody2D 중력 속도를 0으로 잠급니다.
    /// 배우 몸에 Rigidbody2D는 남겨 두되, 무대 트랙에서 밑으로 추락하지 않게 붙잡는 안전장치입니다.
    /// </summary>
    private void ApplyStagePhysicsGuard()
    {
        if (!_isLockRigidbodyGravityOnStage || Rigidbody_Actor == null)
            return;

        if (!_hasSavedRigidbodyState)
        {
            _savedGravityScale = Rigidbody_Actor.gravityScale;
            _savedConstraints = Rigidbody_Actor.constraints;
            _hasSavedRigidbodyState = true;
        }

        Rigidbody_Actor.gravityScale = 0f;
        Rigidbody_Actor.linearVelocity = Vector2.zero;
        Rigidbody_Actor.angularVelocity = 0f;

        if (_isFreezeRigidbodyRotation)
            Rigidbody_Actor.constraints = _savedConstraints | RigidbodyConstraints2D.FreezeRotation;
    }

    private void RestoreStagePhysicsGuard()
    {
        if (!_hasSavedRigidbodyState || Rigidbody_Actor == null)
            return;

        Rigidbody_Actor.gravityScale = _savedGravityScale;
        Rigidbody_Actor.constraints = _savedConstraints;
        Rigidbody_Actor.linearVelocity = Vector2.zero;
        Rigidbody_Actor.angularVelocity = 0f;
        _hasSavedRigidbodyState = false;
    }

    private void UpdatePlayerMovement()
    {
        ApplyStagePhysicsGuard();
        int direction = 0;

        if (Input.GetKey(_moveLeftKey))
            direction -= 1;

        if (Input.GetKey(_moveRightKey))
            direction += 1;

        if (Input.GetKeyDown(_jumpKey))
            RequestJump();

        if (direction == 0)
        {
            if (!_isJumping)
                RequestPlayIdle();

            return;
        }

        bool isRunning = Input.GetKey(_runKey);
        float speed = isRunning ? _runSpeed : _walkSpeed;
        Vector3 position = transform.position;
        position.x += direction * speed * Time.deltaTime;

        if (_hasMovementBounds)
            position.x = Mathf.Clamp(position.x, _movementBounds.min.x, _movementBounds.max.x);

        transform.position = position;

        if (Renderer_Actor != null && _isFlipByMoveDirection)
            Renderer_Actor.flipX = direction < 0;

        if (!_isJumping)
            RequestPlayState(isRunning ? _runningStateName : _walkingStateName, isRunning ? 1.5f : 0.75f);
    }

    private void RequestJump()
    {
        if (_isJumping)
            return;

        if (Coroutine_Jump != null)
            StopCoroutine(Coroutine_Jump);

        Coroutine_Jump = StartCoroutine(PlayJumpRoutine());
    }

    private IEnumerator PlayJumpRoutine()
    {
        _isJumping = true;
        _groundY = transform.position.y;
        RequestPlayState(_jumpingStateName, 1f, true);

        float elapsedTime = 0f;

        while (elapsedTime < _jumpDuration)
        {
            ApplyStagePhysicsGuard();
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / _jumpDuration);
            float height = Mathf.Sin(normalizedTime * Mathf.PI) * _jumpHeight;
            Vector3 position = transform.position;
            position.y = _groundY + height;
            transform.position = position;
            yield return null;
        }

        Vector3 finalPosition = transform.position;
        finalPosition.y = _groundY;
        transform.position = finalPosition;
        _isJumping = false;
        Coroutine_Jump = null;
        RequestPlayIdle();
    }

    private void UpdateFlipByDelta(Vector3 deltaPosition)
    {
        if (Renderer_Actor == null || !_isFlipByMoveDirection || Mathf.Abs(deltaPosition.x) <= 0.001f)
            return;

        Renderer_Actor.flipX = deltaPosition.x < 0f;
    }
}
