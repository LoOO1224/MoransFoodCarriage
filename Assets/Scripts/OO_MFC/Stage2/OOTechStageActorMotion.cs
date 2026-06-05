//// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechStageActorMotion.cs
// - ??븷: Stage 諛곗슦 ??紐낆쓽 ?대룞, ?먰봽, ?좊땲硫붿씠???곹깭瑜??대떦?섎뒗 而댄룷?뚰듃?낅땲??
// - ?곹솕 鍮꾩쑀: 臾대?媛먮룆??諛곗슦 ?붿쓣 吏곸젒 ?≪븘 ?吏곸씠吏 ?딄퀬, 諛곗슦?먭쾶 "?낆옣/?湲??먰봽" ??븷?쒕? 遺숈뿬 ?ㅼ뒪濡??곌린?섍쾶 ?⑸땲??
// - ?좎?蹂댁닔 ?ъ씤?? Controller?????쒖꽌留?遺瑜닿퀬, ?ㅼ젣 ?吏곸엫怨??좊땲硫붿씠???대쫫? ??諛곗슦 而댄룷?뚰듃?먯꽌 愿由ы빀?덈떎.
// =============================================================================
using System.Collections;
using UnityEngine;

/// <summary>
/// Stage2 媛숈? ?곗텧 臾대??먯꽌 罹먮┃??諛곗슦?먭쾶 遺숇뒗 ?대룞/?좊땲硫붿씠????븷 而댄룷?뚰듃?낅땲??
/// Game View?먯꽌???먮룞 ?낆옣, ?뚮젅?댁뼱 議곗옉, ?먰봽, Victory 媛숈? ?곹깭 ?꾪솚????而댄룷?뚰듃媛 ?대떦?⑸땲??
/// </summary>
/// 

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

    [Header("Player Physics")]
    [SerializeField] private bool _isUsePhysicsWhenPlayerInputEnabled = true;
    [SerializeField] private float _playerJumpVelocity = 330f;
    [SerializeField] private float _playerGravityScale = 38f;
    [SerializeField] private float _playerFallGravityScale = 48f;
    [SerializeField] private float _playerMaxFallSpeed = 780f;
    [SerializeField] private float _playerGroundCheckDistance = 10f;
    [SerializeField] private float _playerGroundSnapDistance = 220f;
    [SerializeField] private float _playerGroundSnapSkin = 0.03f;
    [SerializeField] private float _playerJumpSnapDelay = 0.22f;
    [SerializeField] private float _playerGroundSnapMaximumUpwardVelocity = 25f;
    [SerializeField] private float _playerGroundNormalYThreshold = 0.25f;
    [SerializeField] private LayerMask _playerGroundLayerMask = ~0;

    [Header("Input")]
    [SerializeField] private KeyCode _moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode _moveRightKey = KeyCode.D;
    [SerializeField] private KeyCode _runKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode _jumpKey = KeyCode.Space;

    private Animator Animator_Actor;
    private SpriteRenderer Renderer_Actor;
    private Rigidbody2D Rigidbody_Actor;
    private Collider2D Collider_Actor;
    private Coroutine Coroutine_Jump;
    private Bounds _movementBounds;
    private bool _hasMovementBounds;
    private bool _isPlayerInputEnabled;
    private bool _isJumping;
    private float _groundY;
    private float _lastJumpTime;
    private string _currentStateName;
    private bool _hasSavedRigidbodyState;
    private bool _isPauseStagePhysicsGuard;
    private float _savedGravityScale;
    private RigidbodyType2D _savedBodyType;
    private RigidbodyConstraints2D _savedConstraints;
    private readonly RaycastHit2D[] _groundHitArray = new RaycastHit2D[8];

    public bool IsPlayerInputEnabled => _isPlayerInputEnabled;

    /// <summary>
    /// 諛곗슦媛 耳쒖쭏 ???먭린 紐몄뿉 遺숈? Animator? SpriteRenderer瑜?李얠븘 ?〓땲??
    /// 臾대? ??諛곗슦媛 ?먭린 ?섏긽怨??숈옉?쒕? ?뺤씤?섎뒗 以鍮??④퀎?낅땲??
    /// </summary>
    private void Awake()
    {
        ResolveReferences();
        ApplyStagePhysicsGuard();
    }

    /// <summary>
    /// 諛곗슦媛 ?ㅼ떆 耳쒖쭏 ??Rigidbody2D 以묐젰???먮룞 ?대룞 ?먮? 臾대꼫?⑤━吏 ?딄쾶 臾대???臾쇰━ ?곹깭瑜?以鍮꾪빀?덈떎.
    /// Game View?먯꽌??諛곗슦媛 諛붾떏 ?꾨옒濡?爰쇱?吏 ?딄퀬 ?뺥빐吏?EntryPoint源뚯? ?곌린?⑸땲??
    /// </summary>
    private void OnEnable()
    {
        ResolveReferences();

        if (!_isPlayerInputEnabled)   // Player Input 以묒씠 ?꾨땺 ?뚮쭔
            ApplyStagePhysicsGuard();
    }

    /// <summary>
    /// 臾대?瑜??좊궇 ???먮옒 Rigidbody2D ?ㅼ젙???섎룎由쎈땲??
    /// 媛숈? 諛곗슦 ?꾨━?뱀쓣 ?ㅻⅨ ?λ㈃?먯꽌 ?ъ궗?⑺븷 ??臾쇰━ ?ㅼ젙???덉? ?딄쾶 留됰뒗 ?뺣━ ?먯엯?덈떎.
    /// </summary>
    private void OnDisable()
    {
        RestoreStagePhysicsGuard();
    }

    /// <summary>
    /// ?뚮젅?댁뼱 議곗옉???덉슜???숈븞 A/D/Shift/Space ?낅젰???쎌뼱 諛곗슦瑜??吏곸엯?덈떎.
    /// </summary>
    private void Update()
    {
        if (!_isPlayerInputEnabled)
            return;

        UpdatePlayerMovement();
    }

    /// <summary>
    /// ?몃? Controller媛 諛곗슦??湲곕낯 ?좊땲硫붿씠???대쫫????踰덉뿉 吏?뺥븷 ???ъ슜?⑸땲??
    /// 媛숈? 而댄룷?뚰듃瑜?Moran, Chunyang, GreedyDuck?먭쾶 ?ъ궗?⑺븯湲??꾪븳 ?ㅼ젙 ?낃뎄?낅땲??
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
    /// ?먮룞 ?낆옣怨??뚮젅?댁뼱 議곗옉 ?띾룄瑜?吏?뺥빀?덈떎.
    /// ?곹솕濡?移섎㈃ 諛곗슦蹂?蹂댄룺怨??곕뒗 ?띾룄瑜?臾대? ?곹솴??留욊쾶 議곗젅?섎뒗 ?묒뾽?낅땲??
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
    /// ?뚮젅?댁뼱媛 ?吏곸씪 ???덈뒗 臾대? 寃쎄퀎瑜?吏?뺥빀?덈떎.
    /// Game View?먯꽌??Moran??諛곌꼍 諛뽰쑝濡?鍮좎졇?섍?吏 ?딄쾶 留됰뒗 ?щ챸 臾대? 踰???븷?낅땲??
    /// </summary>
    public void RequestSetMovementBounds(Bounds movementBounds)
    {
        _movementBounds = movementBounds;
        _hasMovementBounds = true;
    }

    /// <summary>
    /// ?뚮젅?댁뼱 議곗옉 媛???щ?瑜?諛붽퓠?덈떎.
    /// ??吏꾪뻾 以묒뿉??false, ?꾨Т媛 ?쒖옉?섎㈃ true濡?諛붾앸땲??
    /// </summary>
    public void RequestSetPlayerInputEnabled(bool isEnabled)
    {
        _isPlayerInputEnabled = isEnabled;
        _groundY = transform.position.y;

        if (isEnabled)
        {
            PreparePlayerPhysicsIfNeeded();
            return;
        }

        if (Rigidbody_Actor != null)
            Rigidbody_Actor.linearVelocity = Vector2.zero;

        ApplyStagePhysicsGuard();

        if (!isEnabled)
            RequestPlayIdle();
    }

    /// <summary>
    /// ?먮룞 ?곗텧 以?Kinematic?쇰줈 ?좉릿 諛곗슦瑜?臾쇰━ ?대룞 媛?ν븳 Dynamic ?곹깭濡?諛붽퓠?덈떎.
    /// Stage2?먯꽌??GreedyDuck??源移섏컡媛쒕? 諛쏆? ??怨꾨떒/?꾩떆 諛쒗뙋??諛잕퀬 ?댁옣???뚮쭔 ?ъ슜?⑸땲??
    /// </summary>
    public void RequestSwitchToDynamicPhysics(float gravityScale = -1f)
    {
        ResolveReferences();

        if (Rigidbody_Actor == null)
            return;

        RestoreStagePhysicsGuard();
        _isPauseStagePhysicsGuard = true;
        Rigidbody_Actor.bodyType = RigidbodyType2D.Dynamic;
        Rigidbody_Actor.gravityScale = gravityScale >= 0f ? gravityScale : _playerGravityScale;
        Rigidbody_Actor.linearVelocity = Vector2.zero;
        Rigidbody_Actor.angularVelocity = 0f;
        Rigidbody_Actor.interpolation = RigidbodyInterpolation2D.Interpolate;
        Rigidbody_Actor.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (_isFreezeRigidbodyRotation)
            Rigidbody_Actor.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    /// <summary>
    /// 諛곗슦瑜?吏?뺣맂 紐⑺몴 吏?먭퉴吏 ?먮룞?쇰줈 嫄룰쾶 ?⑸땲??
    /// ??諛곗슦媛 ?붾㈃ 諛뽰뿉??媛곸옄 EntryPoint源뚯? ?ㅼ뼱?ㅻ뒗 ?낆옣 ?곗텧???ъ슜?⑸땲??
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
            MoveActorPosition(Vector3.MoveTowards(transform.position, targetTransform.position, Mathf.Max(1f, speed) * Time.deltaTime));
            UpdateFlipByDelta(transform.position - beforePosition);
            yield return null;
        }

        MoveActorPosition(targetTransform.position);
        RequestPlayIdle();
    }

    /// <summary>
    /// ?꾩옱 ?꾩튂?먯꽌 X 諛⑺뼢?쇰줈 吏??嫄곕━留뚰겮 鍮좊Ⅴ寃??꾨쭩媛寃??⑸땲??
    /// GreedyDuck??遺덊????곹깭濡?臾대? 諛뽰쑝濡??댁옣?????ъ슜?⑸땲??
    /// </summary>
    public IEnumerator MoveToWorldPositionRoutine(Vector3 targetPosition, float speed, string moveStateName, float animationSpeed = 1f)
    {
        RequestPlayState(moveStateName, Mathf.Max(0.01f, animationSpeed), true);

        while (Vector2.Distance(transform.position, targetPosition) > 1f)
        {
            ApplyStagePhysicsGuard();
            Vector3 beforePosition = transform.position;
            MoveActorPosition(Vector3.MoveTowards(transform.position, targetPosition, Mathf.Max(1f, speed) * Time.deltaTime));
            UpdateFlipByDelta(transform.position - beforePosition);
            yield return null;
        }

        MoveActorPosition(targetPosition);
    }

    /// <summary>
    /// 諛곗슦瑜?諛붾줈 ?湲??곹깭濡??뚮┰?덈떎.
    /// </summary>
    public void RequestPlayIdle()
    {
        RequestPlayState(_idleStateName, 1f, false);
    }

    /// <summary>
    /// 諛곗슦??由ъ븸???곹깭瑜??ъ깮?⑸땲??
    /// Moran 異? Chunyang 遺?꾨윭?, Mr.Jaeik ??뚯쿂??????ъ씠 ?곌린 ?먯뿉 ?ъ슜?⑸땲??
    /// </summary>
    public bool RequestPlayReaction(float speed = 1f)
    {
        if (!string.IsNullOrEmpty(_reactionStateName) && RequestPlayState(_reactionStateName, speed, true))
            return true;

        return RequestPlayState(_victoryStateName, speed, true);
    }

    /// <summary>
    /// Victory ?곹깭瑜?怨꾩냽 諛섎났?쒗궎湲??꾪븳 怨듦컻 API?낅땲??
    /// Stage Clear ??Moran???ㅼ쓬 Road濡??섏뼱媛湲??꾧퉴吏 諛섎났 ?곌린?⑸땲??
    /// </summary>
    public bool RequestPlayVictory(float speed = 1f)
    {
        return RequestPlayState(_victoryStateName, speed, true);
    }

    /// <summary>
    /// 吏?뺣맂 ?좊땲硫붿씠???곹깭瑜??ъ깮?⑸땲??
    /// ?곹깭媛 ?놁쑝硫?false瑜?諛섑솚??Controller媛 fallback???먮떒?????덇쾶 ?⑸땲??
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
    /// ?꾩옱 Animator ?덉뿉 ?곹깭媛 ?덈뒗吏 ?뺤씤?⑸땲??
    /// 媛먮룆???녿뒗 ?숈옉???쒗궎?ㅺ퀬 ????肄섏넄 寃쎄퀬 ?꾩뿉 ?덉쟾?섍쾶 留됰뒗 ?뺤씤?쒖엯?덈떎.
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

        if (Collider_Actor == null)
            Collider_Actor = ResolveBodyCollider();
    }

    private Collider2D ResolveBodyCollider()
    {
        Collider2D[] colliderArray = GetComponentsInChildren<Collider2D>(true);

        foreach (Collider2D collider in colliderArray)
        {
            if (collider != null && collider.enabled && !collider.isTrigger)
                return collider;
        }

        foreach (Collider2D collider in colliderArray)
        {
            if (collider != null && collider.enabled)
                return collider;
        }

        return null;
    }

    /// <summary>
    /// Stage2???먮룞?대룞? Transform ?먮? ?곕?濡?Rigidbody2D瑜?Kinematic 臾대? 諛곗슦 ?곹깭濡??좉툒?덈떎.
    /// Dynamic ?곹깭瑜?洹몃?濡??먮㈃ Collider_Bottom/Collider_Stairs? 寃뱀튌 ??臾쇰━ solver媛 諛곗슦瑜??꾨줈 諛???щ┰?덈떎.
    /// </summary>
    private void ApplyStagePhysicsGuard()
    {
        if (_isPlayerInputEnabled && _isUsePhysicsWhenPlayerInputEnabled)
            return;

        if (_isPauseStagePhysicsGuard)
            return;

        if (!_isLockRigidbodyGravityOnStage || Rigidbody_Actor == null)
            return;

        if (!_hasSavedRigidbodyState)
        {
            _savedGravityScale = Rigidbody_Actor.gravityScale;
            _savedBodyType = Rigidbody_Actor.bodyType;
            _savedConstraints = Rigidbody_Actor.constraints;
            _hasSavedRigidbodyState = true;
        }

        Rigidbody_Actor.bodyType = RigidbodyType2D.Kinematic;
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

        Rigidbody_Actor.linearVelocity = Vector2.zero;
        Rigidbody_Actor.angularVelocity = 0f;
        Rigidbody_Actor.bodyType = _savedBodyType;
        Rigidbody_Actor.gravityScale = _savedGravityScale;
        Rigidbody_Actor.constraints = _savedConstraints;
        _hasSavedRigidbodyState = false;
        _isPauseStagePhysicsGuard = false;
    }

    private void UpdatePlayerMovement()
    {
        if (_isPlayerInputEnabled && _isUsePhysicsWhenPlayerInputEnabled)
        {
            UpdatePlayerPhysicsMovement();
            return;
        }

        // Stage ?먮룞 ?대룞/?곗텧???뚮쭔 Guard ?곸슜
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

    private bool IsUsingPlayerPhysics()
    {
        return _isPlayerInputEnabled && _isUsePhysicsWhenPlayerInputEnabled && Rigidbody_Actor != null;
    }

    private void PreparePlayerPhysicsIfNeeded()
    {
        if (Rigidbody_Actor == null) return;

        // Guard 媛뺤젣 ?댁젣
        _hasSavedRigidbodyState = false;
        _isPauseStagePhysicsGuard = false;
        Rigidbody_Actor.bodyType = RigidbodyType2D.Dynamic;
        Rigidbody_Actor.gravityScale = _playerGravityScale;

        Rigidbody_Actor.linearVelocity = Vector2.zero;
        Rigidbody_Actor.angularVelocity = 0f;
        Rigidbody_Actor.interpolation = RigidbodyInterpolation2D.Interpolate;
        Rigidbody_Actor.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (_isFreezeRigidbodyRotation)
            Rigidbody_Actor.constraints |= RigidbodyConstraints2D.FreezeRotation;

        ApplyGroundSnapInsurance(false);
        _isJumping = false;
    }

    private void MoveActorPosition(Vector3 nextPosition)
    {
        if (Rigidbody_Actor != null && Rigidbody_Actor.bodyType == RigidbodyType2D.Dynamic)
        {
            Rigidbody_Actor.MovePosition(nextPosition);
            return;
        }

        transform.position = nextPosition;
    }

    private void UpdatePlayerPhysicsMovement()
    {
        if (Rigidbody_Actor == null)
        {
            UpdatePlayerMovementByTransformFallback();
            return;
        }

        int direction = 0;

        if (Input.GetKey(_moveLeftKey))
            direction -= 1;

        if (Input.GetKey(_moveRightKey))
            direction += 1;

        bool isGrounded = CheckPlayerGrounded();

        if (Input.GetKeyDown(_jumpKey) && isGrounded)
            RequestPhysicsJump();

        bool isRunning = Input.GetKey(_runKey);
        float speed = isRunning ? _runSpeed : _walkSpeed;
        Vector2 velocity = Rigidbody_Actor.linearVelocity;
        velocity.x = direction * speed;

        if (velocity.y < -_playerMaxFallSpeed)
            velocity.y = -_playerMaxFallSpeed;

        Rigidbody_Actor.gravityScale = velocity.y < -0.01f ? _playerFallGravityScale : _playerGravityScale;
        Rigidbody_Actor.linearVelocity = velocity;
        ApplyGroundSnapInsurance(isGrounded);
        ClampPhysicsPositionToBounds();
        UpdateFlipByDirection(direction);
        UpdatePhysicsAnimation(direction, isRunning, isGrounded);
    }

    private void UpdatePlayerMovementByTransformFallback()
    {
        _isUsePhysicsWhenPlayerInputEnabled = false;
        UpdatePlayerMovement();
    }

    private void RequestPhysicsJump()
    {
        if (Rigidbody_Actor == null)
            return;

        Vector2 velocity = Rigidbody_Actor.linearVelocity;
        velocity.y = _playerJumpVelocity;
        Rigidbody_Actor.linearVelocity = velocity;
        _isJumping = true;
        _lastJumpTime = Time.time;
        RequestPlayState(_jumpingStateName, 1f, true);
    }

    private bool CheckPlayerGrounded()
    {
        ResolveReferences();

        if (Collider_Actor == null)
            return false;

        ContactFilter2D contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = _playerGroundLayerMask,
            useTriggers = false
        };

        int hitCount = Collider_Actor.Cast(Vector2.down, contactFilter, _groundHitArray, _playerGroundCheckDistance);

        for (int index = 0; index < hitCount; index++)
        {
            RaycastHit2D hit = _groundHitArray[index];

            if (hit.collider == null || hit.collider.isTrigger)
                continue;

            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
                continue;

            if (hit.normal.y >= _playerGroundNormalYThreshold)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Dynamic ?꾪솚 吏곹썑 諛붾떏 ?먯젙???볦튂硫?諛곗슦媛 怨듭쨷??怨좎젙?섏뼱 蹂댁씪 ???덉뼱 ?꾨옒 諛붾떏?쇰줈 ?덉쟾 蹂댁젙?⑸땲??
    /// ?먰봽 吏곹썑?먮뒗 ?곸슜?섏? ?딆븘??Moran???먭??ㅻ━?먭쾶 ?щ씪媛???먰봽 沅ㅼ쟻? ?좎??⑸땲??
    /// </summary>
    private void ApplyGroundSnapInsurance(bool isGrounded)
    {
        if (isGrounded || Rigidbody_Actor == null || Collider_Actor == null)
            return;

        if (Time.time - _lastJumpTime < _playerJumpSnapDelay)
            return;

        if (Rigidbody_Actor.linearVelocity.y > _playerGroundSnapMaximumUpwardVelocity)
            return;

        ContactFilter2D contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = _playerGroundLayerMask,
            useTriggers = false
        };

        float snapDistance = Mathf.Max(_playerGroundCheckDistance, _playerGroundSnapDistance);
        int hitCount = Collider_Actor.Cast(Vector2.down, contactFilter, _groundHitArray, snapDistance);
        float bestDistance = float.MaxValue;

        for (int index = 0; index < hitCount; index++)
        {
            RaycastHit2D hit = _groundHitArray[index];

            if (!IsValidGroundHit(hit))
                continue;

            if (hit.distance < bestDistance)
                bestDistance = hit.distance;
        }

        if (bestDistance == float.MaxValue || bestDistance <= _playerGroundSnapSkin)
            return;

        Vector3 position = transform.position;
        position.y -= Mathf.Max(0f, bestDistance - _playerGroundSnapSkin);
        transform.position = position;
        Rigidbody_Actor.linearVelocity = new Vector2(Rigidbody_Actor.linearVelocity.x, 0f);
    }

    private bool IsValidGroundHit(RaycastHit2D hit)
    {
        if (hit.collider == null || hit.collider.isTrigger)
            return false;

        if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
            return false;

        return hit.normal.y >= _playerGroundNormalYThreshold;
    }

    private void ClampPhysicsPositionToBounds()
    {
        if (!_hasMovementBounds || Rigidbody_Actor == null)
            return;

        Vector3 position = transform.position;
        float clampedX = Mathf.Clamp(position.x, _movementBounds.min.x, _movementBounds.max.x);

        if (Mathf.Approximately(position.x, clampedX))
            return;

        transform.position = new Vector3(clampedX, position.y, position.z);
        Rigidbody_Actor.linearVelocity = new Vector2(0f, Rigidbody_Actor.linearVelocity.y);
    }

    private void UpdateFlipByDirection(int direction)
    {
        if (Renderer_Actor == null || !_isFlipByMoveDirection || direction == 0)
            return;

        Renderer_Actor.flipX = direction < 0;
    }

    private void UpdatePhysicsAnimation(int direction, bool isRunning, bool isGrounded)
    {
        if (!isGrounded)
        {
            RequestPlayState(_jumpingStateName, 1f);
            return;
        }

        _isJumping = false;

        if (direction == 0)
        {
            RequestPlayIdle();
            return;
        }

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
