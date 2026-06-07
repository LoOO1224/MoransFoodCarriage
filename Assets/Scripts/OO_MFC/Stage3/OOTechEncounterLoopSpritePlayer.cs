// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechEncounterLoopSpritePlayer.cs
// - 역할: EncounterGroup 전용으로 Animator를 쓰지 않고 SpriteRenderer 프레임을 직접 넘깁니다.
// - 영화 비유: 자동 연기 장치가 계속 조명 사고를 내는 장면에서, 스태프가 컷 사진을 직접 넘겨 배우 연기를 고정합니다.
// - 유지보수 포인트: 다른 그룹 애니메이션에는 개입하지 않고, 이 컴포넌트가 붙은 Encounter 배우에게만 적용됩니다.
// =============================================================================
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Animator Controller 전이/컬링/상태 재진입을 우회해 SpriteRenderer.sprite만 직접 교체합니다.
/// Game View에서는 Moran_isScared, Mr_Jaeik_isThreatening처럼 계속 반복되는 장면을 깜빡임 없이 보여주는 장치입니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechEncounterLoopSpritePlayer : MonoBehaviour
{
    private const string DedicatedRendererName = "Renderer_EncounterLoop";

    [Header("Loop State")]
    [SerializeField] private string _stateName;
    [SerializeField] private Sprite[] Sprite_FrameArray = new Sprite[0];
    [SerializeField] private float _frameRate = 12f;
    [SerializeField] private bool _isPlayOnEnable = true;

    [Header("Actor View")]
    [SerializeField] private SpriteRenderer Renderer_Actor;
    [SerializeField] private Animator Animator_Actor;
    [SerializeField] private bool _isUseDedicatedRenderer = true;

    private Animator[] Animator_ActorArray = new Animator[0];
    private SpriteRenderer[] Renderer_ActorArray = new SpriteRenderer[0];
    private SpriteRenderer Renderer_Source;
    private Coroutine Coroutine_Loop;
    private Vector3 _savedRendererLocalPosition;
    private bool _hasSavedRendererLocalPosition;
    private Sprite Sprite_CurrentFrame;

    /// <summary>
    /// EncounterGroup이 켜지면 Animator를 끄고 Sprite 루프를 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        ResolveReferences();

        if (_isPlayOnEnable)
            RequestPlayLoopState(_stateName, 1f);
    }

    private void OnDisable()
    {
        StopLoopRoutine();
    }

    /// <summary>
    /// 다른 배우 컴포넌트가 같은 프레임에 Animator를 다시 켜는 경우가 있어 Encounter 전용 장면에서는 계속 꺼 둡니다.
    /// 영화 비유로는 자동 조명 콘솔이 다시 켜져도, 이 컷에서는 수동 조명 담당자가 매 프레임 스위치를 고정하는 처리입니다.
    /// </summary>
    private void LateUpdate()
    {
        if (Coroutine_Loop == null)
            return;

        DisableActorAnimators();
        RequestKeepRendererVisible();
        RequestKeepOnlyPrimaryRendererVisible();

        if (Sprite_CurrentFrame != null && Renderer_Actor != null && Renderer_Actor.sprite != Sprite_CurrentFrame)
            Renderer_Actor.sprite = Sprite_CurrentFrame;
    }

    /// <summary>
    /// Stage3EncounterController가 특정 상태를 요청할 때, 이 컴포넌트가 담당하는 상태라면 직접 프레임 루프를 시작합니다.
    /// </summary>
    public bool RequestPlayLoopState(string stateName, float speed)
    {
        if (string.IsNullOrEmpty(stateName) || stateName != _stateName)
            return false;

        ResolveReferences();
        PrepareDedicatedRendererIfNeeded();

        if (Renderer_Actor == null || Sprite_FrameArray == null || Sprite_FrameArray.Length == 0)
        {
            Debug.LogWarning($"[OOTechEncounterLoopSpritePlayer] Frame setup missing: {gameObject.name}, State={stateName}", this);
            return false;
        }

        DisableActorAnimators();

        RequestKeepRendererVisible();
        RequestKeepOnlyPrimaryRendererVisible();
        _savedRendererLocalPosition = Renderer_Actor.transform.localPosition;
        _hasSavedRendererLocalPosition = true;

        StopLoopRoutine();
        Coroutine_Loop = StartCoroutine(PlayLoopRoutine(Mathf.Max(0.01f, speed)));
        return true;
    }

    private IEnumerator PlayLoopRoutine(float speed)
    {
        int frameIndex = 0;
        float secondsPerFrame = 1f / Mathf.Max(1f, _frameRate * speed);

        while (isActiveAndEnabled)
        {
            Sprite frameSprite = Sprite_FrameArray[frameIndex];

            if (frameSprite != null)
            {
                Sprite_CurrentFrame = frameSprite;
                Renderer_Actor.sprite = frameSprite;
                Renderer_Actor.transform.localPosition = GetFrameLocalPosition(frameSprite);
            }

            frameIndex = (frameIndex + 1) % Sprite_FrameArray.Length;
            yield return new WaitForSeconds(secondsPerFrame);
        }
    }

    /// <summary>
    /// 프레임마다 Sprite Rect와 Pivot이 달라도 발 위치가 흔들리지 않도록 위치를 보정합니다.
    /// 큰 스케일로 키운 배우일수록 작은 Pivot 차이가 크게 보이므로, 첫 프레임의 하단 중앙을 기준으로 맞춥니다.
    /// </summary>
    private Vector3 GetFrameLocalPosition(Sprite frameSprite)
    {
        if (frameSprite == null || Sprite_FrameArray == null || Sprite_FrameArray.Length == 0 || Sprite_FrameArray[0] == null)
            return _savedRendererLocalPosition;

        if (!_hasSavedRendererLocalPosition)
        {
            _savedRendererLocalPosition = Renderer_Actor.transform.localPosition;
            _hasSavedRendererLocalPosition = true;
        }

        Sprite baseSprite = Sprite_FrameArray[0];
        Vector3 offset = new Vector3(
            baseSprite.bounds.center.x - frameSprite.bounds.center.x,
            baseSprite.bounds.min.y - frameSprite.bounds.min.y,
            0f);

        return _savedRendererLocalPosition + offset;
    }

    private void StopLoopRoutine()
    {
        if (Coroutine_Loop == null)
            return;

        StopCoroutine(Coroutine_Loop);
        Coroutine_Loop = null;
    }

    private void ResolveReferences()
    {
        Renderer_ActorArray = GetComponentsInChildren<SpriteRenderer>(true);

        SpriteRenderer dedicatedRenderer = ResolveDedicatedSpriteRenderer();

        if (dedicatedRenderer != null)
            Renderer_Actor = dedicatedRenderer;

        if (Renderer_Actor == null)
            Renderer_Actor = ResolvePrimarySpriteRenderer();

        if (Renderer_Source == null)
            Renderer_Source = ResolvePrimarySpriteRendererExcept(Renderer_Actor);

        if (Renderer_Source == null)
            Renderer_Source = Renderer_Actor;

        if (Animator_Actor == null)
            Animator_Actor = GetComponent<Animator>();

        if (Animator_Actor == null)
            Animator_Actor = GetComponentInChildren<Animator>(true);

        Animator_ActorArray = GetComponentsInChildren<Animator>(true);
    }

    /// <summary>
    /// 원본 배우의 SpriteRenderer가 다른 컴포넌트와 충돌하면 깜빡임이 계속 납니다.
    /// Encounter 전용 표시 렌더러를 새로 만들고, 원본 렌더러들은 모두 끈 뒤 이 렌더러 하나만 프레임을 넘깁니다.
    /// </summary>
    private void PrepareDedicatedRendererIfNeeded()
    {
        if (!_isUseDedicatedRenderer || Renderer_Source == null)
            return;

        GameObject dedicatedObjectByName = OOTechSceneQuery.RequestChildObjectByName(transform, DedicatedRendererName);
        Transform dedicatedTransform = dedicatedObjectByName != null ? dedicatedObjectByName.transform : null;

        if (dedicatedTransform == null)
        {
            GameObject runtimeObjectByName = OOTechSceneQuery.RequestChildObjectByName(transform, "Runtime_EncounterLoopRenderer");
            dedicatedTransform = runtimeObjectByName != null ? runtimeObjectByName.transform : null;
        }

        GameObject dedicatedObject = dedicatedTransform != null ? dedicatedTransform.gameObject : null;

        if (dedicatedObject == null)
        {
            dedicatedObject = new GameObject(DedicatedRendererName);
            dedicatedObject.transform.SetParent(transform, false);
        }
        else
        {
            dedicatedObject.name = DedicatedRendererName;
        }

        SpriteRenderer dedicatedRenderer = dedicatedObject.GetComponent<SpriteRenderer>();

        if (dedicatedRenderer == null)
            dedicatedRenderer = dedicatedObject.AddComponent<SpriteRenderer>();

        dedicatedObject.transform.localPosition = Renderer_Source.transform.localPosition;
        dedicatedObject.transform.localRotation = Renderer_Source.transform.localRotation;
        dedicatedObject.transform.localScale = Renderer_Source.transform.localScale;
        dedicatedRenderer.sortingLayerID = Renderer_Source.sortingLayerID;
        dedicatedRenderer.sortingOrder = Renderer_Source.sortingOrder + 2;
        dedicatedRenderer.flipX = Renderer_Source.flipX;
        dedicatedRenderer.flipY = Renderer_Source.flipY;
        dedicatedRenderer.color = Color.white;
        dedicatedRenderer.sprite = Sprite_FrameArray != null && Sprite_FrameArray.Length > 0 ? Sprite_FrameArray[0] : Renderer_Source.sprite;
        Renderer_Actor = dedicatedRenderer;
        Renderer_ActorArray = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private SpriteRenderer ResolveDedicatedSpriteRenderer()
    {
        GameObject dedicatedObjectByName = OOTechSceneQuery.RequestChildObjectByName(transform, DedicatedRendererName);
        Transform dedicatedTransform = dedicatedObjectByName != null ? dedicatedObjectByName.transform : null;

        if (dedicatedTransform == null)
        {
            GameObject runtimeObjectByName = OOTechSceneQuery.RequestChildObjectByName(transform, "Runtime_EncounterLoopRenderer");
            dedicatedTransform = runtimeObjectByName != null ? runtimeObjectByName.transform : null;
        }

        return dedicatedTransform != null ? dedicatedTransform.GetComponent<SpriteRenderer>() : null;
    }

    private SpriteRenderer ResolvePrimarySpriteRenderer()
    {
        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();

        if (rootRenderer != null && rootRenderer.sprite != null)
            return rootRenderer;

        SpriteRenderer bestRenderer = null;
        float bestArea = -1f;

        foreach (SpriteRenderer spriteRenderer in Renderer_ActorArray)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
                continue;

            float area = spriteRenderer.sprite.bounds.size.x * spriteRenderer.sprite.bounds.size.y;

            if (area <= bestArea)
                continue;

            bestArea = area;
            bestRenderer = spriteRenderer;
        }

        return bestRenderer;
    }

    private SpriteRenderer ResolvePrimarySpriteRendererExcept(SpriteRenderer exceptRenderer)
    {
        SpriteRenderer bestRenderer = null;
        float bestArea = -1f;

        foreach (SpriteRenderer spriteRenderer in Renderer_ActorArray)
        {
            if (spriteRenderer == null || spriteRenderer == exceptRenderer || spriteRenderer.sprite == null)
                continue;

            float area = spriteRenderer.sprite.bounds.size.x * spriteRenderer.sprite.bounds.size.y;

            if (area <= bestArea)
                continue;

            bestArea = area;
            bestRenderer = spriteRenderer;
        }

        return bestRenderer;
    }

    private void DisableActorAnimators()
    {
        ResolveReferences();

        if (Animator_ActorArray == null || Animator_ActorArray.Length == 0)
            return;

        foreach (Animator animator in Animator_ActorArray)
        {
            if (animator == null)
                continue;

            animator.enabled = false;
            animator.speed = 0f;
        }
    }

    private void RequestKeepRendererVisible()
    {
        if (Renderer_Actor == null)
            return;

        Renderer_Actor.enabled = true;
        Renderer_Actor.forceRenderingOff = false;
        Color rendererColor = Renderer_Actor.color;
        rendererColor.a = 1f;
        Renderer_Actor.color = rendererColor;
    }

    private void RequestKeepOnlyPrimaryRendererVisible()
    {
        if (Renderer_ActorArray == null || Renderer_ActorArray.Length == 0 || Renderer_Actor == null)
            return;

        foreach (SpriteRenderer spriteRenderer in Renderer_ActorArray)
        {
            if (spriteRenderer == null || spriteRenderer == Renderer_Actor)
                continue;

            spriteRenderer.enabled = false;
            spriteRenderer.forceRenderingOff = true;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 배치모드/에디터 수리 단계에서 AnimationClip의 Sprite 프레임을 읽어 씬 컴포넌트에 저장합니다.
    /// 저장된 Sprite 배열은 빌드 런타임에서도 그대로 사용됩니다.
    /// </summary>
    public void RequestEditorSetupFromClip(string stateName, AnimationClip sourceClip, float speed)
    {
        _stateName = stateName;
        _frameRate = sourceClip != null ? Mathf.Max(1f, sourceClip.frameRate * Mathf.Max(0.01f, speed)) : _frameRate;
        Sprite_FrameArray = RequestExtractSpriteFrames(sourceClip);
        ResolveReferences();
        PrepareDedicatedRendererIfNeeded();
        DisableActorAnimators();
        RequestKeepOnlyPrimaryRendererVisible();
        RequestKeepRendererVisible();
        EditorUtility.SetDirty(this);
    }

    private Sprite[] RequestExtractSpriteFrames(AnimationClip sourceClip)
    {
        if (sourceClip == null)
            return new Sprite[0];

        EditorCurveBinding[] bindingArray = AnimationUtility.GetObjectReferenceCurveBindings(sourceClip);

        foreach (EditorCurveBinding binding in bindingArray)
        {
            if (binding.type != typeof(SpriteRenderer) || binding.propertyName != "m_Sprite")
                continue;

            ObjectReferenceKeyframe[] keyframeArray = AnimationUtility.GetObjectReferenceCurve(sourceClip, binding);
            Sprite[] spriteArray = new Sprite[keyframeArray.Length];

            for (int index = 0; index < keyframeArray.Length; index++)
                spriteArray[index] = keyframeArray[index].value as Sprite;

            return spriteArray;
        }

        return new Sprite[0];
    }
#endif
}
