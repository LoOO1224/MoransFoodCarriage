// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechJaeikController.cs
// - ??븷: 罹먮┃??諛곗슦???대룞, ?낅젰, ?좊땲硫붿씠???곹깭瑜??대떦?⑸땲??
// - 媛먮룆 愿?? 諛곗슦媛 臾대? ?꾩뿉???대뼸寃?嫄룰퀬 硫덉텛怨?諛섏쓳?섎뒗吏 ?뺥븯???곌린 吏?꾪몴?낅땲??
// - ?좎?蹂댁닔 ?ъ씤?? ?λ㈃ 吏꾪뻾 ?쒖꽌??Group Controller媛 留↔퀬, 罹먮┃???ㅽ겕由쏀듃???먭린 紐몄쓽 ?吏곸엫留?留↔쾶 ?⑸땲??
// =============================================================================
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Jaeik???대룞, ?먰봽 臾쇰━, S1_Object_Quest ?곹샇?묒슜???대떦?⑸땲??
/// Game View?먯꽌???뚮젅?댁뼱媛 WASD/留덉슦??Space/E濡??ъ씡??議곗쥌?섎뒗 諛곗슦 而⑦듃濡ㅻ윭?낅땲??
/// </summary>
public class OOTechJaeikController : MonoBehaviour
{
    // ?쎈뒗 ?쒖꽌:
    // 1. Update: ?ㅻ낫??留덉슦???낅젰??諛쏆븘 ?대룞, ?먰봽, ?곹샇?묒슜 ?붿껌???뺤씤?⑸땲??
    // 2. FixedUpdate: Rigidbody2D瑜??ㅼ젣濡??吏곸뿬 臾쇰━ ?꾩튂瑜?媛깆떊?⑸땲??
    // 3. UpdateJumpInput 怨꾩뿴: Space ?낅젰怨?isGrounded ?먯젙??泥섎━?⑸땲??
    // 4. UpdateInteractionInput 怨꾩뿴: E ?낅젰?쇰줈 ?뚯떇 ?ㅻ툕?앺듃? ?곹샇?묒슜?⑸땲??
    // 5. UpdateAnimationState 怨꾩뿴: ?꾩옱 ?吏곸엫??留욌뒗 ?좊땲硫붿씠???곹깭瑜?怨좊쫭?덈떎.
    // ?좎?蹂댁닔 二쇱쓽:
    // - Jaeik??紐??吏곸엫留?留↔퀬, Senario1?????蹂???쒖꽌??OOTechSenario1Controller媛 留≪뒿?덈떎.
    // - ?먰봽?μ씠???대룞媛??섏젙? Inspector???대룞/?먰봽 媛믩????뺤씤?⑸땲??
    // - UI ?꾨＼?꾪듃??媛?ν븯硫??섏씠?대씪???ㅻ툕?앺듃濡??먭퀬, 肄붾뱶 ?앹꽦? 理쒖냼?뷀빀?덈떎.

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
    /// Rigidbody, SpriteRenderer, Animator, Collider瑜?以鍮꾪븯怨?E ?곹샇?묒슜 ?쒖떆瑜??명똿?⑸땲??
    /// </summary>
    private void Awake()
    {
        CacheComponentReferences();
        CreateFallbackColliderIfNeeded();
        CreateDefaultInteractionPromptIfNeeded();
    }

    /// <summary>
    /// ?ъ씡??臾대????깆옣?????먰봽 媛?ν븳 臾쇰━ ?명똿怨??낅젰 ?좉툑 ?곹깭瑜??곸슜?⑸땲??
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
        // Update??諛곗슦??利됱꽍 ?낅젰??諛쏅뒗 ?쒓컙?낅땲??
        // 媛먮룆???낅젰???좉렐 ?λ㈃?먯꽌???대룞, ?먰봽, E ?곹샇?묒슜??紐⑤몢 諛쏆? ?딆뒿?덈떎.
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
        // FixedUpdate??臾대? ?μ튂 ?대떦?낅땲??
        // Rigidbody2D ?대룞怨?以묐젰? 臾쇰━ ?꾨젅?꾩뿉??泥섎━?댁빞 怨꾨떒/泥쒖옣 肄쒕━?붿? ?덉젙?곸쑝濡?留욌Ъ由쎈땲??
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
    /// ?쒗넗由ъ뼹/???以묒뿉???뚮젅?댁뼱 ?대룞???좉툒?덈떎.
    /// </summary>
    public void LockMovement()
    {
        SetMovementLocked(true);
    }

    /// <summary>
    /// ?쒗넗由ъ뼹/??붽? ?앸굹硫??뚮젅?댁뼱 ?대룞???ㅼ떆 ?덉슜?⑸땲??
    /// </summary>
    public void UnlockMovement()
    {
        SetMovementLocked(false);
    }

    /// <summary>
    /// ?낅젰 ?먯껜瑜?耳쒓퀬 ?뺣땲?? 爰쇱쭏 ?뚮뒗 ?꾩옱 ?대룞怨??대┃ ?대룞 紐⑺몴瑜??④퍡 ?뺣━?⑸땲??
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
    /// ?몃? 媛먮룆 ?ㅽ겕由쏀듃媛 ?대룞 ?좉툑 ?곹깭瑜?吏곸젒 吏?뺥븷 ???ъ슜?⑸땲??
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
    /// E???곹샇?묒슜 ??? 利??뚯떇 ?ㅻ툕?앺듃 ?꾩튂瑜??곌껐?⑸땲??
    /// </summary>
    public void SetInteractionTarget(Transform target)
    {
        Transform_InteractionTarget = target;
        UpdateInteractionPrompt();
    }

    /// <summary>
    /// E?ㅺ? 諛섏쓳?섎뒗 嫄곕━ 媛믪쓣 ?ㅼ젙?⑸땲??
    /// </summary>
    public void SetInteractionDistance(float distance)
    {
        _interactionDistance = Mathf.Max(0.1f, distance);
    }

    /// <summary>
    /// ?뚯떇 ?곹샇?묒슜???앸궗?붿? 湲곕줉??E ?꾨＼?꾪듃媛 ?ㅼ떆 ?⑥? ?딄쾶 ?⑸땲??
    /// </summary>
    public void SetInteractionCompleted(bool isCompleted)
    {
        _isInteractionCompleted = isCompleted;

        if (_isInteractionCompleted)
            HideInteractionPrompt();
    }

    /// <summary>
    /// ?뚯떇 E ?곹샇?묒슜???ㅼ뼱?붿쓣 ??Scenario1Controller媛 諛쏆쓣 肄쒕갚???깅줉?⑸땲??
    /// </summary>
    public void BindEatInteractionRequestEvent(Action callback)
    {
        _eatInteractionRequestEvent -= callback;
        _eatInteractionRequestEvent += callback;
    }

    /// <summary>
    /// ?뚯떇 E ?곹샇?묒슜 肄쒕갚???댁젣?⑸땲??
    /// </summary>
    public void UnbindEatInteractionRequestEvent(Action callback)
    {
        _eatInteractionRequestEvent -= callback;
    }

    /// <summary>
    /// 癒밴린 ?좊땲硫붿씠?섏쓣 湲곕낯 ?띾룄 0.3?쇰줈 ??踰??ъ깮?⑸땲??
    /// </summary>
    public void PlayEatAnimationOnce(Action onComplete = null)
    {
        PlayEatAnimationOnce(0.3f, onComplete);
    }

    /// <summary>
    /// 癒밴린 ?좊땲硫붿씠?섏쓣 吏???띾룄濡???踰??ъ깮?섍퀬 ?앸굹硫?肄쒕갚???몄텧?⑸땲??
    /// </summary>
    public void PlayEatAnimationOnce(float animationSpeed, Action onComplete = null)
    {
        StartOneShotAnimation(_eatStateName, _eatAnimationSeconds, animationSpeed, false, onComplete);
    }

    /// <summary>
    /// 蹂???좊땲硫붿씠?섏쓣 ??踰??ъ깮?섍퀬 留덉?留??꾨젅?꾩뿉 硫덉땅?덈떎.
    /// </summary>
    public void PlayTransformedAnimationOnce(float animationSpeed, Action onComplete = null)
    {
        StartOneShotAnimation(_transformedStateName, _transformedAnimationSeconds, animationSpeed, true, onComplete);
    }

    /// <summary>
    /// ?먰봽 ?좊땲硫붿씠?섏쓣 吏㏃? 1?뚯꽦 ?곗텧濡??ъ깮?⑸땲??
    /// </summary>
    public void PlayJumpingAnimationOnce(Action onComplete = null)
    {
        StartOneShotAnimation(_jumpStateName, _jumpAnimationMinimumSeconds, 1f, false, onComplete);
    }

    /// <summary>
    /// 癒밴린/蹂??媛숈? 1?뚯꽦 ?좊땲硫붿씠??肄붾（?댁쓣 ?뺣━?⑸땲??
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
    /// E ?곹샇?묒슜 ?덈궡 臾멸뎄瑜??④퉩?덈떎.
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
            Collider_Jaeik = RequestBestBodyCollider();
    }

    private Collider2D RequestBestBodyCollider()
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
    /// ?ㅻ낫???낅젰???놁쓣 ??留덉슦???대┃ ?대룞 紐⑺몴瑜??뺤씤?⑸땲??
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
    /// ?붾㈃???대┃??吏?먯쓣 ?붾뱶 醫뚰몴濡?諛붽퓭 ?ъ씡??洹?諛⑺뼢?쇰줈 嫄룰쾶 ?⑸땲??
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
    /// Space ?낅젰??諛쏆쑝硫??먰봽 ?먮? ?붿껌?⑸땲??
    /// </summary>
    private void UpdateJumpInput()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        RequestJump();
    }

    private void RequestJump()
    {
        // ?먰봽??諛곗슦瑜???댁뼱濡??ㅼ뼱 ?щ━???먯? 鍮꾩듂?⑸땲??
        // Space瑜??꾨Ⅴ硫??꾩そ ?띾룄瑜???踰??ш쾶 二쇨퀬, ?댄썑 ?숉븯??Physics2D 以묐젰?먭쾶 留↔퉩?덈떎.
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
    /// ?먰봽 媛??議곌굔???뺤씤?⑸땲?? 諛붾떏???덇굅???꾩＜ 吏㏃? 肄붿슂??????덉뿉?쒕쭔 ?덉슜?⑸땲??
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
    /// ?뚯떇 洹쇱쿂?먯꽌 E?ㅻ? ?꾨Ⅴ硫??곹샇?묒슜 ?대깽?몃? ??踰덈쭔 諛쒖깮?쒗궢?덈떎.
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
    /// ?ъ씡怨??뚯떇 ?ㅻ툕?앺듃 ?ъ씠??嫄곕━媛 ?곹샇?묒슜 踰붿쐞 ?덉씤吏 ?뺤씤?⑸땲??
    /// </summary>
    private bool IsNearInteractionTarget()
    {
        if (Transform_InteractionTarget == null)
            return false;

        float distance = Vector2.Distance(transform.position, Transform_InteractionTarget.position);
        return distance <= Mathf.Max(_interactionDistance, _minimumInteractionDistance);
    }

    /// <summary>
    /// ?뚯떇 洹쇱쿂???덉쓣 ?뚮쭔 E ?덈궡 臾멸뎄瑜??뚯떇 ?꾩뿉 ?쒖떆?⑸땲??
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
    /// ?ъ뿉 E ?꾨＼?꾪듃媛 ?놁쓣 寃쎌슦 ?ъ씡 ?먯떇?쇰줈 理쒖냼 ?띿뒪???꾨＼?꾪듃瑜?以鍮꾪빀?덈떎.
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
    /// Rigidbody2D??媛濡??대룞 ?띾룄瑜??곸슜?⑸땲??
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
        // isGrounded??諛곗슦 諛쒖씠 臾대? 諛붾떏???우븯?붿? ?뺤씤?섎뒗 ?μ튂?낅땲??
        // ??媛믪씠 true???뚮쭔 ?ㅼ쓬 ?먰봽瑜??덉슜?댁꽌 怨듭쨷 2???먰봽瑜?留됱뒿?덈떎.
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
        // ?곸듅 ?앸?遺꾩뿉?쒕뒗 ?댁쭩 ?먮━寃? ?⑥뼱吏??뚮뒗 ??鍮좊Ⅴ寃?以묐젰??諛붽퓠?덈떎.
        // ?붾㈃?먯꽌???ш쾶 ????ㅻⅨ ???덈Т ?λ뫁 ??蹂댁씠吏 ?딄퀬 肄쒕━???꾩뿉 鍮⑤━ ?덉갑?⑸땲??
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
    /// 泥쒖옣??遺?ろ엳硫??꾩そ ?띾룄瑜?利됱떆 0?쇰줈 留뚮뱾???ㅼ떆 ?⑥뼱吏寃??⑸땲??
    /// </summary>
    private void StopUpwardVelocity()
    {
        if (Rigidbody_Jaeik == null || Rigidbody_Jaeik.linearVelocity.y <= 0f)
            return;

        Rigidbody_Jaeik.linearVelocity = new Vector2(Rigidbody_Jaeik.linearVelocity.x, 0f);
    }

    /// <summary>
    /// ?대룞 諛⑺뼢??留욎떠 ?ㅽ봽?쇱씠??醫뚯슦 諛섏쟾???곸슜?⑸땲??
    /// </summary>
    private void UpdateSpriteFlip()
    {
        if (SpriteRenderer_Jaeik == null || Mathf.Abs(_moveInputX) <= 0.01f)
            return;

        SpriteRenderer_Jaeik.flipX = _moveInputX < 0f;
    }

    /// <summary>
    /// ?대룞/?먰봽/Idle ?곹깭瑜??꾩옱 ?낅젰怨?諛붾떏 ?먯젙??留욎떠 媛깆떊?⑸땲??
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
    /// 癒밴린/蹂?좎쿂???뚮젅?댁뼱 ?낅젰???좉렇??1?뚯꽦 ?좊땲硫붿씠?섏쓣 ?쒖옉?⑸땲??
    /// </summary>
    private void StartOneShotAnimation(string stateName, float animationSeconds, float animationSpeed, bool isHoldLastFrame, Action onComplete)
    {
        StopOneShotAnimation();

        _oneShotAnimationCoroutine = StartCoroutine(PlayOneShotAnimationRoutine(stateName, animationSeconds, animationSpeed, isHoldLastFrame, onComplete));
    }

    /// <summary>
    /// 1?뚯꽦 ?좊땲硫붿씠???숈븞 ?대룞??硫덉텛怨? ?앸굹硫?Idle 蹂듦? ?먮뒗 留덉?留??꾨젅??怨좎젙??泥섎━?⑸땲??
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

