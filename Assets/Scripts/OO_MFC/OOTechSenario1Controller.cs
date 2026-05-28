using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Senario1Group의 시작 흐름, 카메라 타겟 변경, 공용 스킵 버튼, 재익 상호작용 연출을 관리합니다.
/// 캐릭터 이동과 애니메이션 입력은 OOTechJaeikController에 위임합니다.
/// </summary>
public class OOTechSenario1Controller : MonoBehaviour
{
    [Header("Character Reference")]
    [SerializeField] private OOTechJaeikController Character_Jaeik;
    [SerializeField] private Transform Transform_Jaeik;
    [SerializeField] private SpriteRenderer SpriteRenderer_Jaeik;
    [SerializeField] private Sprite Sprite_JaeikAfterInteraction;

    [Header("Camera Reference")]
    [SerializeField] private CameraFollowController Camera_Follow;

    [Header("Interaction Reference")]
    [SerializeField] private Transform Transform_InteractionCube;

    [Header("Common Skip Button")]
    [SerializeField] private NextButtonController Prefab_CommonSkipButton;
    [SerializeField] private string _currentGroupName = "Senario1Group";
    [SerializeField] private string _nextGroupName = "WorldMapGroup";
    [SerializeField] private string _skipButtonText = "넘어가기";

    [Header("Interaction Effect")]
    [SerializeField] private bool _isUseInteractionFlash = true;
    [SerializeField] private Color _flashColor = new Color(1f, 1f, 1f, 0.78f);
    [SerializeField] private int _flashCount = 2;
    [SerializeField] private float _flashFadeSeconds = 0.08f;
    [SerializeField] private float _fadeHoldSeconds = 0.15f;
    [SerializeField] private int _effectSortingOrder = 190;

    // ==================== 런타임 생성 객체 ====================
    private NextButtonController Button_CommonSkip;
    private GameObject Object_EffectCanvas;
    private Image Image_EffectFade;
    private Coroutine _interactionEffectCoroutine;

    private void OnEnable()
    {
        StartSenario1Group();
    }

    private void OnDisable()
    {
        StopInteractionEffectCoroutine();
        UnbindCharacterEvent();
        HideCommonSkipButton();
        HideEffectCanvas();
    }

    // ==================== 그룹 시작 ====================

    /// <summary>
    /// Senario1Group이 열릴 때 카메라를 재익에게 맞추고, 상호작용과 공용 스킵 버튼을 준비합니다.
    /// </summary>
    private void StartSenario1Group()
    {
        CacheCharacterReferenceIfNeeded();
        SetCameraTargetToJaeik();
        PrepareCharacterInteraction();
        ShowCommonSkipButton();
    }

    /// <summary>
    /// 씬 저장 과정에서 캐릭터 컨트롤러 PPtr이 끊겨도 재익 Transform 기준으로 필요한 컴포넌트를 다시 묶습니다.
    /// Transform_Jaeik은 인스펙터에 직접 연결된 참조이므로 씬 전체 검색을 사용하지 않습니다.
    /// </summary>
    private void CacheCharacterReferenceIfNeeded()
    {
        if (Transform_Jaeik == null)
        {
            Debug.LogWarning("[OOTechSenario1Controller] Transform_Jaeik 참조가 비어 있습니다.");
            return;
        }

        if (Character_Jaeik == null && !Transform_Jaeik.TryGetComponent(out Character_Jaeik))
            Character_Jaeik = Transform_Jaeik.gameObject.AddComponent<OOTechJaeikController>();

        Rigidbody2D rigidbodyJaeik = null;
        Animator animatorJaeik = null;

        Transform_Jaeik.TryGetComponent(out rigidbodyJaeik);
        Transform_Jaeik.TryGetComponent(out animatorJaeik);

        if (SpriteRenderer_Jaeik == null)
            Transform_Jaeik.TryGetComponent(out SpriteRenderer_Jaeik);

        if (Character_Jaeik != null)
            Character_Jaeik.SetComponentReference(rigidbodyJaeik, SpriteRenderer_Jaeik, animatorJaeik);
    }

    private void SetCameraTargetToJaeik()
    {
        if (Camera_Follow == null || Transform_Jaeik == null)
        {
            Debug.LogWarning("[OOTechSenario1Controller] Camera_Follow 또는 Transform_Jaeik 참조가 비어 있습니다.");
            return;
        }

        Camera_Follow.SetTarget(Transform_Jaeik);
    }

    private void PrepareCharacterInteraction()
    {
        if (Character_Jaeik == null)
        {
            Debug.LogWarning("[OOTechSenario1Controller] Character_Jaeik 참조가 비어 있습니다.");
            return;
        }

        Character_Jaeik.SetInteractionTarget(Transform_InteractionCube);
        Character_Jaeik.SetInteractionCompleted(false);
        Character_Jaeik.BindEatInteractionRequestEvent(OnJaeikEatInteractionRequested);
    }

    private void UnbindCharacterEvent()
    {
        if (Character_Jaeik != null)
            Character_Jaeik.UnbindEatInteractionRequestEvent(OnJaeikEatInteractionRequested);
    }

    // ==================== 재익 상호작용 연출 ====================

    private void OnJaeikEatInteractionRequested()
    {
        if (_interactionEffectCoroutine != null)
            return;

        _interactionEffectCoroutine = StartCoroutine(PlayJaeikInteractionEffectRoutine());
    }

    private IEnumerator PlayJaeikInteractionEffectRoutine()
    {
        if (Character_Jaeik != null)
        {
            Character_Jaeik.LockMovement();
            Character_Jaeik.HideInteractionPrompt();
        }

        if (_isUseInteractionFlash)
            yield return PlayFlashAndFadeRoutine();

        ChangeJaeikAppearanceIfReady();

        if (Character_Jaeik != null)
            Character_Jaeik.PlayEatAnimationOnce(OnJaeikEatAnimationEnd);

        _interactionEffectCoroutine = null;
    }

    private void OnJaeikEatAnimationEnd()
    {
        if (Character_Jaeik != null)
        {
            Character_Jaeik.SetInteractionCompleted(true);
            Character_Jaeik.UnlockMovement();
        }
    }

    private void ChangeJaeikAppearanceIfReady()
    {
        if (SpriteRenderer_Jaeik == null || Sprite_JaeikAfterInteraction == null)
            return;

        SpriteRenderer_Jaeik.sprite = Sprite_JaeikAfterInteraction;
    }

    // ==================== 화면 번쩍임 / 페이드 ====================

    private IEnumerator PlayFlashAndFadeRoutine()
    {
        CreateEffectCanvasIfNeeded();

        if (Object_EffectCanvas == null || Image_EffectFade == null)
            yield break;

        Object_EffectCanvas.SetActive(true);

        for (int i = 0; i < _flashCount; i++)
        {
            yield return FadeEffect(0f, _flashColor.a);
            yield return FadeEffect(_flashColor.a, 0f);
        }

        yield return FadeEffect(0f, 1f);
        yield return new WaitForSeconds(_fadeHoldSeconds);
        yield return FadeEffect(1f, 0f);

        HideEffectCanvas();
    }

    private IEnumerator FadeEffect(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < _flashFadeSeconds)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float lerpValue = Mathf.Clamp01(elapsedTime / _flashFadeSeconds);
            SetEffectAlpha(Mathf.Lerp(startAlpha, endAlpha, lerpValue));
            yield return null;
        }

        SetEffectAlpha(endAlpha);
    }

    private void CreateEffectCanvasIfNeeded()
    {
        if (Object_EffectCanvas != null)
            return;

        Object_EffectCanvas = new GameObject("Image_Senario1InteractionFade", typeof(RectTransform));
        Object_EffectCanvas.layer = gameObject.layer;
        Object_EffectCanvas.transform.SetParent(transform, false);

        RectTransform effectRect = Object_EffectCanvas.transform as RectTransform;
        StretchFullScreen(effectRect);

        Canvas canvas = Object_EffectCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = _effectSortingOrder;

        CanvasScaler canvasScaler = Object_EffectCanvas.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        Image_EffectFade = Object_EffectCanvas.AddComponent<Image>();
        Image_EffectFade.raycastTarget = false;
        SetEffectAlpha(0f);
        Object_EffectCanvas.SetActive(false);
    }

    private void HideEffectCanvas()
    {
        if (Image_EffectFade != null)
            SetEffectAlpha(0f);

        if (Object_EffectCanvas != null)
            Object_EffectCanvas.SetActive(false);
    }

    private void SetEffectAlpha(float alpha)
    {
        if (Image_EffectFade == null)
            return;

        Color effectColor = _flashColor;
        effectColor.a = alpha;
        Image_EffectFade.color = effectColor;
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

    private void StopInteractionEffectCoroutine()
    {
        if (_interactionEffectCoroutine == null)
            return;

        StopCoroutine(_interactionEffectCoroutine);
        _interactionEffectCoroutine = null;
    }

    // ==================== 공용 스킵 버튼 ====================

    private void ShowCommonSkipButton()
    {
        CreateCommonSkipButtonIfNeeded();

        if (Button_CommonSkip == null)
            return;

        Button_CommonSkip.gameObject.SetActive(true);
        Button_CommonSkip.SetGroupName(_currentGroupName, _nextGroupName);
        Button_CommonSkip.SetButtonText(_skipButtonText);
    }

    private void HideCommonSkipButton()
    {
        if (Button_CommonSkip != null)
            Button_CommonSkip.gameObject.SetActive(false);
    }

    private void CreateCommonSkipButtonIfNeeded()
    {
        if (Button_CommonSkip != null)
            return;

        if (Prefab_CommonSkipButton == null)
        {
            Debug.LogWarning("[OOTechSenario1Controller] CommonSkipButton 프리팹 참조가 비어 있습니다.");
            return;
        }

        Button_CommonSkip = Instantiate(Prefab_CommonSkipButton, transform, false);
        Button_CommonSkip.name = "CommonSkipButton_Senario1";
        Button_CommonSkip.gameObject.SetActive(false);
    }
}
