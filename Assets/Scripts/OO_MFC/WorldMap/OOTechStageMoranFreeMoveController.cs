// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechStageMoranFreeMoveController.cs
// - 역할: Stage4_1/Stage4_2의 Moran에게 Stage1과 같은 좌우 이동 조작감을 제공합니다.
// - 영화 비유: 배우 Moran에게 "좌우로 걸어가고 달리는 역할표"만 붙이는 컴포넌트입니다.
// - 유지보수 포인트: NPC 대화, 퀘스트, 장면 전환은 맡지 않고 이동과 애니메이션만 담당합니다.
// =============================================================================
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class OOTechStageMoranFreeMoveController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode _moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode _moveRightKey = KeyCode.D;
    [SerializeField] private KeyCode _runKey = KeyCode.LeftShift;

    [Header("Move")]
    [SerializeField] private float _walkSpeed = 2.2f;
    [SerializeField] private float _runSpeed = 4.8f;
    [SerializeField, Range(0f, 1f)] private float _laneNormalizedHeight = 0.2f;
    [SerializeField] private float _edgeMargin = 0.65f;
    [SerializeField] private float _fallbackMoveHalfWidth = 9.2f;

    [Header("Animation")]
    [SerializeField] private string _idleStateName = "Moran_idle";
    [SerializeField] private string _walkingStateName = "Moran_isWalking";
    [SerializeField] private string _runningStateName = "Moran_isRunning";
    [SerializeField] private float _walkingAnimationSpeed = 0.75f;
    [SerializeField] private float _runningAnimationSpeed = 1.5f;

    [Header("Renderer")]
    [SerializeField] private int _sortingOrder = 2200;

    private SpriteRenderer Renderer_Moran;
    private Animator Animator_Moran;
    private SpriteRenderer Renderer_Background;
    private string _currentAnimationStateName;

    /// <summary>
    /// 배우가 켜질 때 필요한 부품을 찾아서 화면에 보이도록 정리합니다.
    /// </summary>
    private void OnEnable()
    {
        ResolveComponents();
        RequestRepairView();
        RequestPlaceOnStageLane();
        PlayAnimationState(_idleStateName, 1f);
    }

    /// <summary>
    /// 매 프레임 입력을 읽어 Moran을 좌우로 이동시킵니다.
    /// </summary>
    private void Update()
    {
        ResolveComponents();

        float horizontalInput = ReadHorizontalInput();
        bool isMoving = Mathf.Abs(horizontalInput) > 0.01f;
        bool isRunning = isMoving && IsRunPressed();

        MoveMoran(horizontalInput, isRunning);
        UpdateAnimation(isMoving, isRunning);
    }

    private void ResolveComponents()
    {
        if (Renderer_Moran == null)
            Renderer_Moran = GetComponentInChildren<SpriteRenderer>(true);

        if (Animator_Moran == null)
            Animator_Moran = GetComponentInChildren<Animator>(true);

        if (Renderer_Background == null)
            Renderer_Background = ResolveBackgroundRenderer();
    }

    private void RequestRepairView()
    {
        if (Renderer_Moran == null)
            return;

        Renderer_Moran.enabled = true;
        Renderer_Moran.sortingOrder = Mathf.Max(Renderer_Moran.sortingOrder, _sortingOrder);

        Color color = Renderer_Moran.color;

        if (color.a <= 0.01f)
        {
            color.a = 1f;
            Renderer_Moran.color = color;
        }
    }

    private void RequestPlaceOnStageLane()
    {
        if (Renderer_Background == null)
            return;

        Vector3 position = transform.position;
        Bounds bounds = Renderer_Background.bounds;

        position.y = Mathf.Lerp(bounds.min.y, bounds.max.y, _laneNormalizedHeight);
        position.x = Mathf.Clamp(position.x, bounds.min.x + _edgeMargin, bounds.max.x - _edgeMargin);
        transform.position = position;
    }

    private float ReadHorizontalInput()
    {
        float input = 0f;

        if (Input.GetKey(_moveLeftKey) || Input.GetKey(KeyCode.LeftArrow))
            input -= 1f;

        if (Input.GetKey(_moveRightKey) || Input.GetKey(KeyCode.RightArrow))
            input += 1f;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.isPressed)
                input -= 1f;

            if (keyboard.rightArrowKey.isPressed)
                input += 1f;
        }
#endif

        return Mathf.Clamp(input, -1f, 1f);
    }

    private bool IsRunPressed()
    {
        if (Input.GetKey(_runKey))
            return true;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.leftShiftKey.isPressed;
#else
        return false;
#endif
    }

    private void MoveMoran(float horizontalInput, bool isRunning)
    {
        if (Mathf.Abs(horizontalInput) <= 0.01f)
            return;

        float speed = isRunning ? _runSpeed : _walkSpeed;
        Vector3 position = transform.position;

        position.x += horizontalInput * speed * Time.deltaTime;

        if (Renderer_Background != null)
        {
            Bounds bounds = Renderer_Background.bounds;
            position.x = Mathf.Clamp(position.x, bounds.min.x + _edgeMargin, bounds.max.x - _edgeMargin);
            position.y = Mathf.Lerp(bounds.min.y, bounds.max.y, _laneNormalizedHeight);
        }
        else
        {
            position.x = Mathf.Clamp(position.x, -_fallbackMoveHalfWidth, _fallbackMoveHalfWidth);
        }

        transform.position = position;

        if (Renderer_Moran != null)
            Renderer_Moran.flipX = horizontalInput < 0f;
    }

    private void UpdateAnimation(bool isMoving, bool isRunning)
    {
        if (!isMoving)
        {
            PlayAnimationState(_idleStateName, 1f);
            return;
        }

        PlayAnimationState(isRunning ? _runningStateName : _walkingStateName, isRunning ? _runningAnimationSpeed : _walkingAnimationSpeed);
    }

    private void PlayAnimationState(string stateName, float speed)
    {
        if (Animator_Moran == null || string.IsNullOrWhiteSpace(stateName))
            return;

        Animator_Moran.enabled = true;
        Animator_Moran.speed = Mathf.Max(0.01f, speed);

        if (_currentAnimationStateName == stateName)
            return;

        Animator_Moran.Play(stateName, 0, 0f);
        _currentAnimationStateName = stateName;
    }

    private SpriteRenderer ResolveBackgroundRenderer()
    {
        Transform rootTransform = transform.parent;

        while (rootTransform != null && !rootTransform.name.Contains("Group"))
            rootTransform = rootTransform.parent;

        if (rootTransform == null)
            return null;

        SpriteRenderer[] rendererArray = rootTransform.GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer bestRenderer = null;
        float bestArea = 0f;
        SpriteRenderer largestRenderer = null;
        float largestArea = 0f;

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
                continue;

            string objectName = spriteRenderer.gameObject.name;

            float area = spriteRenderer.bounds.size.x * spriteRenderer.bounds.size.y;

            if (area > largestArea)
            {
                largestRenderer = spriteRenderer;
                largestArea = area;
            }

            if (!objectName.Contains("Background") && !objectName.Contains("Backound"))
                continue;

            if (area <= bestArea)
                continue;

            bestRenderer = spriteRenderer;
            bestArea = area;
        }

        return bestRenderer != null ? bestRenderer : largestRenderer;
    }
}
