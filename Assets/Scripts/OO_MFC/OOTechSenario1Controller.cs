using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Directs the full Senario1Group flow: opening guide, Jaeik control,
/// quest interaction, transformation, camera focus, dialogue, and completion.
/// </summary>
public class OOTechSenario1Controller : MonoBehaviour
{
    [Header("Character Reference")]
    [SerializeField] private OOTechJaeikController Character_Jaeik;
    [SerializeField] private Transform Transform_Jaeik;
    [SerializeField] private SpriteRenderer SpriteRenderer_Jaeik;
    [SerializeField] private Sprite Sprite_JaeikAfterInteraction;

    [Header("Object Names (Fallback)")]
    [SerializeField] private string _jaeikObjectName = "Jaeik";
    [SerializeField] private string _questObjectName = "S1_Object_Quest";
    [SerializeField] private string _mrJaeikObjectName = "Mr.Jaeik";
    [SerializeField] private string _chunyangObjectName = "Chunyang";
    [SerializeField] private string _chunyangFallbackObjectName = "ChunYang_Idle";
    [SerializeField] private string _moranObjectName = "Moran";
    [SerializeField] private string _moranFallbackObjectName = "Moran_Idle";
    [SerializeField] private string _backgroundObjectName = "Senario1Background";
    [SerializeField] private string _background2ObjectName = "Senario1Background2";

    [Header("Senario1 Objects")]
    [SerializeField] private Transform Transform_InteractionCube;
    [SerializeField] private Transform Transform_QuestObject;
    [SerializeField] private Transform Transform_MrJaeik;
    [SerializeField] private Transform Transform_Chunyang;
    [SerializeField] private Transform Transform_Moran;
    [SerializeField] private GameObject Object_Senario1Background;
    [SerializeField] private GameObject Object_Senario1Background2;
    [SerializeField] private bool _isSnapMrJaeikToJaeikPosition = true;
    [SerializeField] private float _questInteractionDistance = 2.9f;

    [Header("Camera")]
    [SerializeField] private CameraFollowController Camera_Follow;
    [SerializeField] private Camera Camera_Main;
    [SerializeField] private float _cameraFocusWaitSeconds = 0.65f;

    [Header("UI & Tutorial")]
    [SerializeField] private GameObject Group_TutorialGuide;
    [SerializeField] private OOTechTutorialGuideUI UI_TutorialGuide;
    [SerializeField] private GameObject Group_Dialogue;
    [SerializeField] private DialogueUI UI_Dialogue;
    [SerializeField] private string _tutorialGuideGroupName = "TutorialGuideGroup";
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private string _startTutorialId = "narration_tutorial_03";
    [SerializeField] private string _completeTutorialId = "narration_tutorial_02";
    [SerializeField] private bool _isFreezeTimeDuringStartGuide = true;

    [Header("Data Id")]
    [SerializeField] private string _narrationAfterTransformId = "narration_prologue_07";
    [SerializeField] private string _narratorSpeakerId = "character_narrator_01";
    [SerializeField] private string _mrJaeikSpeakerId = "character_Mr.Jaeik_04";
    [SerializeField] private string _chunyangSpeakerId = "character_Chunyang_05";
    [SerializeField] private string _moranSpeakerId = "character_Moran_02";
    [SerializeField] private string _dialogueMrJaeikFirstId = "character_Mr.Jaeik_01";
    [SerializeField] private string _dialogueChunyangFirstId = "character_Chunyang_01";
    [SerializeField] private string _dialogueMrJaeikSecondId = "character_Mr.Jaeik_02";
    [SerializeField] private string _dialogueMoranId = "character_Moran_02";
    [SerializeField] private string _dialogueChunyangSecondId = "character_Chunyang_02";
    [SerializeField] private string _mrJaeikIdleStateName = "Mr.Jaeik_Idle";
    [SerializeField] private string _chunyangIdleStateName = "Chunyang_Idle";
    [SerializeField] private string _moranIdleStateName = "Moran_Idle";

    [Header("Character Idle Clip")]
    [SerializeField] private AnimationClip Clip_MrJaeikIdle;
    [SerializeField] private AnimationClip Clip_ChunyangIdle;

    [Header("BGM")]
    [SerializeField] private Senario1_BGMPlayer BGM_Player;

    [Header("Skip Button")]
    [SerializeField] private NextButtonController Prefab_CommonSkipButton;
    [SerializeField] private string _currentGroupName = "Senario1Group";
    [SerializeField] private string _nextGroupName = "WorldMapGroup";
    [SerializeField] private string _skipButtonText = "\uB118\uC5B4\uAC00\uAE30";
    [SerializeField] private Vector2 _skipButtonAnchoredPosition = new Vector2(-90f, -70f);

    [Header("Transformation")]
    [SerializeField] private float _eatAnimatorSpeed = 0.3f;
    [SerializeField] private float _transformedAnimatorSpeed = 0.2f;
    [SerializeField] private float _postEatDelaySeconds = 4f;
    [SerializeField] private float _transformZoomMultiplier = 0.68f;
    [SerializeField] private float _transformZoomInSeconds = 0.45f;
    [SerializeField] private float _transformZoomOutSeconds = 0.55f;
    [SerializeField] private bool _isUseTransformationEffect = true;
    [SerializeField] private float _transformationEffectSeconds = 4f;
    [SerializeField] private int _blinkCount = 4;
    [SerializeField] private float _blinkFadeSeconds = 0.08f;
    [SerializeField] private float _blackFadeSeconds = 0.5f;
    [SerializeField] private float _blackFadeHoldSeconds = 0.2f;
    [SerializeField] private Color _effectBaseColor = new Color(0.08f, 0.02f, 0.12f, 0f);
    [SerializeField] private Color _effectGoldColor = new Color(1f, 0.68f, 0.18f, 0.75f);
    [SerializeField] private Color _effectWhiteColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private int _effectSortingOrder = 190;
    [SerializeField] private int _effectRayCount = 14;

    private readonly Dictionary<GameObject, Sprite> _backgroundOriginSpriteDic = new Dictionary<GameObject, Sprite>();

    private NextButtonController _buttonCommonSkip;
    private GameObject Object_CommonSkipButton;
    private GameObject Object_EffectCanvas;
    private Image Image_EffectOverlay;
    private Coroutine _mainRoutine;
    private Coroutine _questRoutine;
    private bool _isInitialGuideFinished;
    private bool _isQuestSequenceStarted;
    private float _originCameraOrthographicSize;

    private void OnEnable()
    {
        StopMainRoutine();
        _mainRoutine = StartCoroutine(StartSenario1GroupRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _mainRoutine = null;
        _questRoutine = null;
        RestoreTimeScale();
        CloseTutorialGuide();
        CloseDialogueGroup();
        HideCommonSkipButton();
        HideEffectOverlay();
    }

    private IEnumerator StartSenario1GroupRoutine()
    {
        // 이 루틴은 Senario1Group의 콜시트입니다.
        // 무대가 열리면 세트와 배우를 초기 위치로 세팅하고, 튜토리얼 안내가 끝난 뒤에만 조작권을 넘깁니다.
        _isInitialGuideFinished = false;
        _isQuestSequenceStarted = false;

        CacheAllReferences();
        ApplyRequiredScenarioTiming();
        PrepareInitialState();
        LockJaeikControl();

        yield return WaitForManagersReady();

        CacheAllReferences();
        RegisterRuntimeUIGroup();

        if (_isFreezeTimeDuringStartGuide)
            FreezeTimeForTutorial();

        yield return OpenTutorialGuideAndWait(_startTutorialId, false);

        FinishInitialGuide();
        _mainRoutine = null;
    }

    private void CacheAllReferences()
    {
        if (Transform_Jaeik == null)
            Transform_Jaeik = FindChildTransform(_jaeikObjectName);

        if (Transform_QuestObject == null)
            Transform_QuestObject = FindChildTransform(_questObjectName);

        if (Transform_InteractionCube == null)
            Transform_InteractionCube = Transform_QuestObject;

        if (Transform_MrJaeik == null)
            Transform_MrJaeik = FindChildTransform(_mrJaeikObjectName);

        if (Transform_Chunyang == null)
            Transform_Chunyang = FindChildTransform(_chunyangObjectName);

        if (Transform_Chunyang == null)
            Transform_Chunyang = FindChildTransform(_chunyangFallbackObjectName);

        if (Transform_Moran == null)
            Transform_Moran = FindChildTransform(_moranObjectName);

        if (Transform_Moran == null)
            Transform_Moran = FindChildTransform(_moranFallbackObjectName);

        if (Object_Senario1Background == null)
            Object_Senario1Background = FindChildGameObject(_backgroundObjectName);

        if (Object_Senario1Background2 == null)
            Object_Senario1Background2 = FindChildGameObject(_background2ObjectName);

        if (SpriteRenderer_Jaeik == null && Transform_Jaeik != null)
            SpriteRenderer_Jaeik = Transform_Jaeik.GetComponent<SpriteRenderer>();

        CacheJaeikController();
        CacheCameraReference();
        CacheBGMReference();
        CacheUIReference();
        CacheBackgroundOriginSprite(Object_Senario1Background);
        CacheBackgroundOriginSprite(Object_Senario1Background2);
    }

    private void CacheJaeikController()
    {
        if (Transform_Jaeik == null)
            return;

        if (Character_Jaeik == null)
            Character_Jaeik = Transform_Jaeik.GetComponent<OOTechJaeikController>();

        if (Character_Jaeik == null)
            Character_Jaeik = Transform_Jaeik.gameObject.AddComponent<OOTechJaeikController>();

        Rigidbody2D rigidbody = Transform_Jaeik.GetComponent<Rigidbody2D>();
        SpriteRenderer spriteRenderer = Transform_Jaeik.GetComponent<SpriteRenderer>();
        Animator animator = Transform_Jaeik.GetComponent<Animator>();
        Collider2D collider = GetBestJaeikCollider(Transform_Jaeik);
        Character_Jaeik.SetComponentReference(rigidbody, spriteRenderer, animator, collider);
    }

    private Collider2D GetBestJaeikCollider(Transform target)
    {
        if (target == null)
            return null;

        Collider2D[] colliderArray = target.GetComponents<Collider2D>();

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

    private void CacheCameraReference()
    {
        if (Camera_Main == null)
            Camera_Main = Camera.main;

        if (Camera_Follow == null && Camera_Main != null)
            Camera_Main.TryGetComponent(out Camera_Follow);

        if (Camera_Main != null && _originCameraOrthographicSize <= 0f)
            _originCameraOrthographicSize = Camera_Main.orthographicSize;
    }

    private void CacheBGMReference()
    {
        if (BGM_Player == null)
            BGM_Player = GetComponent<Senario1_BGMPlayer>();

        if (BGM_Player == null)
            BGM_Player = GetComponentInChildren<Senario1_BGMPlayer>(true);
    }

    private void CacheUIReference()
    {
        if (Group_TutorialGuide == null)
            Group_TutorialGuide = ResolveUIGroup(_tutorialGuideGroupName);

        if (UI_TutorialGuide == null && Group_TutorialGuide != null)
            UI_TutorialGuide = Group_TutorialGuide.GetComponentInChildren<OOTechTutorialGuideUI>(true);

        if (Group_Dialogue == null)
            Group_Dialogue = ResolveUIGroup(_dialogueGroupName);

        if (UI_Dialogue == null && Group_Dialogue != null)
            UI_Dialogue = Group_Dialogue.GetComponentInChildren<DialogueUI>(true);
    }

    private GameObject ResolveUIGroup(string groupName)
    {
        GameObject uiObject = null;

        uiObject = FindChildGameObject(groupName);

        if (OOTechUIManager.Inst != null)
            uiObject = uiObject != null ? uiObject : OOTechUIManager.Inst.GetCreatedUI(groupName);

        if (uiObject == null)
            uiObject = FindSceneGameObject(groupName);

        return uiObject;
    }

    private void CacheBackgroundOriginSprite(GameObject backgroundObject)
    {
        if (backgroundObject == null || _backgroundOriginSpriteDic.ContainsKey(backgroundObject))
            return;

        SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
            _backgroundOriginSpriteDic[backgroundObject] = spriteRenderer.sprite;
    }

    private void ApplyRequiredScenarioTiming()
    {
        _eatAnimatorSpeed = 0.3f;
        _transformedAnimatorSpeed = 0.2f;
        _postEatDelaySeconds = 4f;
        _transformationEffectSeconds = 4f;
        _blinkCount = 4;
    }

    private void PrepareInitialState()
    {
        // 첫 장면 세팅:
        // 배경1은 켜고 배경2, Mr.Jaeik, Chunyang, Moran은 뒤에 등장할 배우라서 아직 무대 뒤에 둡니다.
        HideCommonSkipButton();
        HideEffectOverlay();
        HideOpeningUIGroup();
        PrepareBackgroundObject(Object_Senario1Background, true);
        PrepareBackgroundObject(Object_Senario1Background2, false);
        SetObjectActive(Transform_Jaeik, true);
        SetObjectActive(Transform_MrJaeik, false);
        SetObjectActive(Transform_Chunyang, false);
        SetObjectActive(Transform_Moran, false);
        PrepareCharacterIdleAnimation(Transform_Chunyang, _chunyangIdleStateName, Clip_ChunyangIdle);
        PrepareCharacterIdleAnimation(Transform_Moran, _moranIdleStateName);

        if (Character_Jaeik != null)
            Character_Jaeik.SetInteractionCompleted(false);

        FocusCameraOnJaeikImmediately();

        if (BGM_Player != null)
            BGM_Player.PlayJaeikFocusedBGM();
    }

    private void HideOpeningUIGroup()
    {
        if (Group_TutorialGuide != null)
            Group_TutorialGuide.SetActive(false);

        if (Group_Dialogue != null)
            Group_Dialogue.SetActive(false);
    }

    private void PrepareBackgroundObject(GameObject backgroundObject, bool isActive)
    {
        if (backgroundObject == null)
            return;

        backgroundObject.SetActive(isActive);

        Animator animator = backgroundObject.GetComponent<Animator>();
        if (animator != null)
            animator.enabled = false;

        SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            return;

        spriteRenderer.enabled = true;

        if (_backgroundOriginSpriteDic.TryGetValue(backgroundObject, out Sprite originSprite) && originSprite != null)
            spriteRenderer.sprite = originSprite;

        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;
    }

    private void PrepareCharacterIdleAnimation(Transform target, string idleStateName, AnimationClip idleClip = null)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            return;

        if (idleClip != null)
        {
            OOTechIdleAnimationPlayer idleAnimationPlayer = target.GetComponent<OOTechIdleAnimationPlayer>();

            if (idleAnimationPlayer == null)
                idleAnimationPlayer = target.gameObject.AddComponent<OOTechIdleAnimationPlayer>();

            idleAnimationPlayer.PlayIdle(idleClip);
            return;
        }

        Animator animator = target.GetComponent<Animator>();
        if (animator == null || string.IsNullOrEmpty(idleStateName))
            return;

        animator.enabled = true;
        animator.speed = 1f;

        PlayAnimatorState(animator, idleStateName);
    }

    private void RegisterRuntimeUIGroup()
    {
        if (OOTechUIManager.Inst == null)
            return;

        if (Group_TutorialGuide != null)
            OOTechUIManager.Inst.RegisterUI(_tutorialGuideGroupName, Group_TutorialGuide);

        if (Group_Dialogue != null)
            OOTechUIManager.Inst.RegisterUI(_dialogueGroupName, Group_Dialogue);
    }

    private void PrepareForPlayerControl()
    {
        if (Character_Jaeik == null)
            return;

        Transform interactionTarget = Transform_QuestObject != null ? Transform_QuestObject : Transform_InteractionCube;
        Character_Jaeik.SetInteractionTarget(interactionTarget);
        Character_Jaeik.SetInteractionDistance(_questInteractionDistance);
        Character_Jaeik.SetInteractionCompleted(false);
        Character_Jaeik.BindEatInteractionRequestEvent(OnEatInteractionRequested);
    }

    private void LockJaeikControl()
    {
        if (Character_Jaeik == null)
            return;

        Character_Jaeik.LockMovement();
        Character_Jaeik.SetInputEnabled(false);
        Character_Jaeik.HideInteractionPrompt();
    }

    private void UnlockJaeikControl()
    {
        if (Character_Jaeik == null)
            return;

        Character_Jaeik.UnlockMovement();
        Character_Jaeik.SetInputEnabled(true);
    }

    private void FinishInitialGuide()
    {
        if (_isInitialGuideFinished)
            return;

        _isInitialGuideFinished = true;
        RestoreTimeScale();
        CloseTutorialGuide();
        PrepareForPlayerControl();
        UnlockJaeikControl();
    }

    private void OnEatInteractionRequested()
    {
        if (_isQuestSequenceStarted)
            return;

        _questRoutine = StartCoroutine(PlayQuestSequenceRoutine());
    }

    private IEnumerator PlayQuestSequenceRoutine()
    {
        // 음식과 상호작용한 순간부터는 플레이어 입력을 잠급니다.
        // 배우가 애드리브로 움직이지 않게 막고, 카메라/효과/애니메이션 큐를 순서대로 실행하는 구간입니다.
        _isQuestSequenceStarted = true;
        LockJaeikControl();

        yield return PlayEatAnimationAndWait();
        yield return new WaitForSeconds(_postEatDelaySeconds);
        ChangeJaeikAppearance();

        Coroutine blinkCoroutine = null;
        if (_isUseTransformationEffect)
            blinkCoroutine = StartCoroutine(PlayTransformationBlinkRoutine());

        yield return PlayTransformedZoomAnimationAndWait();

        if (blinkCoroutine != null)
            yield return blinkCoroutine;

        yield return PlayBlackFadeRoutine();

        SwitchToSecondBackground();
        ActivateMrJaeik();

        yield return PlayAfterTransformDataSequenceRoutine();

        _questRoutine = null;
    }

    private IEnumerator PlayEatAnimationAndWait()
    {
        if (Character_Jaeik == null)
            yield break;

        bool done = false;
        Character_Jaeik.PlayEatAnimationOnce(_eatAnimatorSpeed, () => done = true);

        while (!done)
            yield return null;
    }

    private IEnumerator PlayTransformedZoomAnimationAndWait()
    {
        if (Character_Jaeik == null)
            yield break;

        yield return ZoomCameraRoutine(GetCurrentCameraSize(), GetZoomedCameraSize(), _transformZoomInSeconds);

        bool done = false;
        Character_Jaeik.PlayTransformedAnimationOnce(_transformedAnimatorSpeed, () => done = true);

        while (!done)
            yield return null;

        yield return ZoomCameraRoutine(GetCurrentCameraSize(), GetOriginCameraSize(), _transformZoomOutSeconds);
    }

    private void ChangeJaeikAppearance()
    {
        if (SpriteRenderer_Jaeik != null && Sprite_JaeikAfterInteraction != null)
            SpriteRenderer_Jaeik.sprite = Sprite_JaeikAfterInteraction;
    }

    private IEnumerator PlayTransformationBlinkRoutine()
    {
        CreateEffectOverlayIfNeeded();

        if (Image_EffectOverlay == null)
            yield break;

        SetEffectOverlayActive(true);

        float delay = Mathf.Max(0f, _transformationEffectSeconds - (_blinkCount * _blinkFadeSeconds * 2f));
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        for (int i = 0; i < _blinkCount; i++)
        {
            Color blinkColor = i % 2 == 0 ? _effectWhiteColor : _effectGoldColor;
            yield return FadeEffectOverlay(blinkColor, 0f, blinkColor.a, _blinkFadeSeconds);
            yield return FadeEffectOverlay(blinkColor, blinkColor.a, 0f, _blinkFadeSeconds);
        }

        HideEffectOverlay();
    }

    private IEnumerator PlayBlackFadeRoutine()
    {
        CreateEffectOverlayIfNeeded();

        if (Image_EffectOverlay == null)
            yield break;

        Color black = Color.black;
        SetEffectOverlayActive(true);

        yield return FadeEffectOverlay(black, 0f, 1f, _blackFadeSeconds);
        yield return new WaitForSeconds(_blackFadeHoldSeconds);
        yield return FadeEffectOverlay(black, 1f, 0f, _blackFadeSeconds);

        HideEffectOverlay();
    }

    private IEnumerator FadeEffectOverlay(Color color, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float lerpValue = Mathf.Clamp01(elapsedTime / duration);
            SetEffectOverlayColor(color, Mathf.Lerp(startAlpha, endAlpha, lerpValue));
            yield return null;
        }

        SetEffectOverlayColor(color, endAlpha);
    }

    private void SwitchToSecondBackground()
    {
        // 세트 전환:
        // 배경1 무대막을 내리고 배경2 무대막을 올립니다.
        PrepareBackgroundObject(Object_Senario1Background, false);
        PrepareBackgroundObject(Object_Senario1Background2, true);
    }

    private void ActivateMrJaeik()
    {
        if (Transform_MrJaeik == null)
            return;

        if (_isSnapMrJaeikToJaeikPosition && Transform_Jaeik != null)
        {
            Vector3 position = Transform_Jaeik.position;
            position.z = Transform_MrJaeik.position.z;
            Transform_MrJaeik.position = position;
        }

        SetObjectActive(Transform_MrJaeik, true);
        SetObjectActive(Transform_Jaeik, false);
        PrepareCharacterIdleAnimation(Transform_MrJaeik, _mrJaeikIdleStateName, Clip_MrJaeikIdle);
        FocusCamera(Transform_MrJaeik, true);

        if (BGM_Player != null)
            BGM_Player.PlayJaeikFocusedBGM();
    }

    private IEnumerator PlayAfterTransformDataSequenceRoutine()
    {
        // 감독 노트:
        // 변신이 끝난 뒤 첫 쇼트는 배우 대사가 아니라 나레이션입니다.
        // 무대 위에 Mr.Jaeik 배우를 세워둔 상태에서 관객에게 상황 설명을 한 컷만 먼저 보여줍니다.
        yield return OpenNarrationAsDialogueAndWait(_narrationAfterTransformId, _narratorSpeakerId);

        // 나레이션 컷이 끝나면 같은 DialogueGroup을 재사용해서,
        // 화자만 Mr.Jaeik으로 바꾸고 재익군의 첫 대사를 데이터에서 꺼내 재생합니다.
        yield return OpenDialogueAndWait(_dialogueMrJaeikFirstId, _mrJaeikSpeakerId, Transform_MrJaeik, true);

        CloseDialogueGroup();
        yield return new WaitForSeconds(0.25f);

        // 다음 장면은 춘양 등장이므로 패널을 잠깐 닫고,
        // 카메라 조명을 춘양 배우에게 넘긴 뒤 다시 대사 패널을 엽니다.
        SetObjectActive(Transform_Chunyang, true);
        PrepareCharacterIdleAnimation(Transform_Chunyang, _chunyangIdleStateName, Clip_ChunyangIdle);
        FocusCamera(Transform_Chunyang, false);

        if (BGM_Player != null)
            BGM_Player.PlayChunyangFocusedBGM();

        yield return new WaitForSeconds(_cameraFocusWaitSeconds);
        yield return OpenDialogueAndWait(_dialogueChunyangFirstId, _chunyangSpeakerId, Transform_Chunyang, false);

        // 여기부터는 대사 패널을 닫지 않고 유지합니다.
        // 배우가 말할 때마다 카메라 포커스와 화자 이름만 교체해서 한 씬처럼 이어 보이게 합니다.
        yield return OpenDialogueAndWait(_dialogueMrJaeikSecondId, _mrJaeikSpeakerId, Transform_MrJaeik, false);

        SetObjectActive(Transform_Moran, true);
        PrepareCharacterIdleAnimation(Transform_Moran, _moranIdleStateName);
        yield return OpenDialogueAndWait(_dialogueMoranId, _moranSpeakerId, Transform_Moran, false);
        yield return OpenDialogueAndWait(_dialogueChunyangSecondId, _chunyangSpeakerId, Transform_Chunyang, false);

        CloseDialogueGroup();
        yield return OpenTutorialGuideAndWait(_completeTutorialId, true);
        CloseTutorialGuide();
        ShowCommonSkipButton();
    }

    private IEnumerator OpenNarrationAsDialogueAndWait(string narrationId, string speakerCharacterId)
    {
        if (!TryOpenDialogueGroup())
            yield break;

        OO_Narration narrationData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetNarrationData(narrationId) : null;

        if (narrationData == null)
            yield break;

        OO_Dialogue dialogueData = CreateDialogueFromNarration(narrationData, speakerCharacterId);
        yield return ShowDialogueDataAndWait(dialogueData);
    }

    private IEnumerator OpenDialogueAndWait(string dialogueId, string speakerCharacterId, Transform focusTarget, bool isSnapCamera)
    {
        FocusCamera(focusTarget, isSnapCamera);

        if (!isSnapCamera)
            yield return new WaitForSeconds(_cameraFocusWaitSeconds);

        if (!TryOpenDialogueGroup())
            yield break;

        OO_Dialogue dialogueData = GetDialogueDataWithSpeaker(dialogueId, speakerCharacterId);
        yield return ShowDialogueDataAndWait(dialogueData);
    }

    private IEnumerator ShowDialogueDataAndWait(OO_Dialogue dialogueData)
    {
        if (UI_Dialogue == null || dialogueData == null)
            yield break;

        bool isDone = false;
        UI_Dialogue.ShowDialogue(dialogueData, () => isDone = true);

        while (!isDone)
            yield return null;
    }

    private OO_Dialogue CreateDialogueFromNarration(OO_Narration narrationData, string speakerCharacterId)
    {
        // OO_Narration 하나를 DialogueGroup이 읽을 수 있는 임시 대본 카드로 바꿉니다.
        // 여기서는 narration_prologue_07의 첫 파트만 사용해서 한 번만 넘기게 합니다.
        string text = GetFirstNarrationPartText(narrationData);

        return new OO_Dialogue
        {
            Id = narrationData.Id,
            SpeakerName = GetCharacterName(speakerCharacterId, "Narration"),
            Text = text,
            NextDialogueId = string.Empty,
            SelectionNameList = new List<string>(),
            SelectionDialogueIdList = new List<string>(),
            TexturePath = string.Empty,
            VoicePath = string.Empty
        };
    }

    private string GetFirstNarrationPartText(OO_Narration narrationData)
    {
        if (narrationData == null || narrationData.NarrationTexts == null)
            return string.Empty;

        foreach (string narrationText in narrationData.NarrationTexts)
        {
            if (!string.IsNullOrWhiteSpace(narrationText))
                return narrationText.Trim();
        }

        return string.Empty;
    }

    private OO_Dialogue GetDialogueDataWithSpeaker(string dialogueId, string speakerCharacterId)
    {
        OO_Dialogue sourceData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetDialogueData(dialogueId) : null;

        if (sourceData == null)
            return null;

        return new OO_Dialogue
        {
            Id = sourceData.Id,
            SpeakerName = GetCharacterName(speakerCharacterId, sourceData.SpeakerName),
            Text = sourceData.Text,
            NextDialogueId = string.Empty,
            SelectionNameList = sourceData.SelectionNameList,
            SelectionDialogueIdList = sourceData.SelectionDialogueIdList,
            TexturePath = sourceData.TexturePath,
            VoicePath = sourceData.VoicePath
        };
    }

    private string GetCharacterName(string characterId, string fallbackName)
    {
        OO_Character characterData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetCharacterData(characterId) : null;

        if (characterData != null && !string.IsNullOrEmpty(characterData.Name))
            return characterData.Name;

        return string.IsNullOrEmpty(fallbackName) ? characterId : fallbackName;
    }

    private IEnumerator OpenTutorialGuideAndWait(string tutorialId, bool isEmphasis)
    {
        if (!TryOpenTutorialGuide())
            yield break;

        if (UI_TutorialGuide == null)
            yield break;

        bool isDone = false;
        UI_TutorialGuide.SetTitleEmphasisActive(isEmphasis);

        OO_Tutorial tutorialData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetTutorialData(tutorialId) : null;
        if (tutorialData != null)
        {
            UI_TutorialGuide.ShowGuide(tutorialData, () => isDone = true);
        }
        else
        {
            OO_Narration narrationData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetNarrationData(tutorialId) : null;

            if (narrationData == null)
                yield break;

            UI_TutorialGuide.ShowGuide(narrationData, () => isDone = true);
        }

        while (!isDone)
            yield return null;
    }

    private bool TryOpenTutorialGuide()
    {
        CacheUIReference();
        RegisterRuntimeUIGroup();

        if (Group_TutorialGuide == null || UI_TutorialGuide == null)
        {
            Debug.LogError($"[OOTechSenario1Controller] Tutorial guide UI is missing: {_tutorialGuideGroupName}");
            return false;
        }

        if (OOTechUIManager.Inst != null && OOTechUIManager.Inst.OpenUI(_tutorialGuideGroupName))
            return true;

        Group_TutorialGuide.SetActive(true);
        return true;
    }

    private bool TryOpenDialogueGroup()
    {
        CacheUIReference();
        RegisterRuntimeUIGroup();

        if (Group_Dialogue == null || UI_Dialogue == null)
        {
            Debug.LogError($"[OOTechSenario1Controller] Dialogue UI is missing: {_dialogueGroupName}");
            return false;
        }

        if (OOTechUIManager.Inst != null && OOTechUIManager.Inst.OpenUI(_dialogueGroupName))
            return true;

        Group_Dialogue.SetActive(true);
        return true;
    }

    private void CloseTutorialGuide()
    {
        if (UI_TutorialGuide != null)
        {
            UI_TutorialGuide.SetTitleEmphasisActive(false);
            UI_TutorialGuide.CloseGuide();
        }

        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.CloseUI(_tutorialGuideGroupName);
        else if (Group_TutorialGuide != null)
            Group_TutorialGuide.SetActive(false);
    }

    private void CloseDialogueGroup()
    {
        if (UI_Dialogue != null)
            UI_Dialogue.CloseDialogue();

        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.CloseUI(_dialogueGroupName);
        else if (Group_Dialogue != null)
            Group_Dialogue.SetActive(false);
    }

    private void FocusCameraOnJaeikImmediately()
    {
        FocusCamera(Transform_Jaeik, true);
    }

    private void FocusCamera(Transform target, bool isSnapImmediately)
    {
        if (target == null)
            return;

        CacheCameraReference();

        if (Camera_Follow != null)
            Camera_Follow.SetTarget(target);

        if (isSnapImmediately)
            SnapCameraToTarget(target);
    }

    private void SnapCameraToTarget(Transform target)
    {
        if (Camera_Main == null || target == null)
            return;

        Transform cameraTransform = Camera_Main.transform;
        cameraTransform.position = new Vector3(target.position.x, target.position.y, cameraTransform.position.z);
    }

    private IEnumerator ZoomCameraRoutine(float startSize, float endSize, float duration)
    {
        if (Camera_Main == null || !Camera_Main.orthographic)
            yield break;

        duration = Mathf.Max(0.01f, duration);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float lerpValue = Mathf.Clamp01(elapsedTime / duration);
            Camera_Main.orthographicSize = Mathf.Lerp(startSize, endSize, lerpValue);
            yield return null;
        }

        Camera_Main.orthographicSize = endSize;
    }

    private float GetOriginCameraSize()
    {
        if (_originCameraOrthographicSize <= 0f && Camera_Main != null)
            _originCameraOrthographicSize = Camera_Main.orthographicSize;

        return _originCameraOrthographicSize > 0f ? _originCameraOrthographicSize : 5f;
    }

    private float GetCurrentCameraSize()
    {
        return Camera_Main != null ? Camera_Main.orthographicSize : GetOriginCameraSize();
    }

    private float GetZoomedCameraSize()
    {
        return Mathf.Max(1f, GetOriginCameraSize() * _transformZoomMultiplier);
    }

    private void CreateEffectOverlayIfNeeded()
    {
        if (Object_EffectCanvas != null)
            return;

        Object_EffectCanvas = new GameObject("Senario1TransformationEffect", typeof(RectTransform));
        Object_EffectCanvas.layer = gameObject.layer;
        Object_EffectCanvas.transform.SetParent(transform, false);

        RectTransform rectTransform = Object_EffectCanvas.transform as RectTransform;
        StretchFullScreen(rectTransform);

        Canvas canvas = Object_EffectCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = _effectSortingOrder + Mathf.Max(0, _effectRayCount / 10);

        CanvasScaler canvasScaler = Object_EffectCanvas.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        Object_EffectCanvas.AddComponent<GraphicRaycaster>();
        Image_EffectOverlay = Object_EffectCanvas.AddComponent<Image>();
        Image_EffectOverlay.raycastTarget = false;
        SetEffectOverlayColor(_effectBaseColor, 0f);
        Object_EffectCanvas.SetActive(false);
    }

    private void SetEffectOverlayActive(bool isActive)
    {
        if (Object_EffectCanvas != null)
            Object_EffectCanvas.SetActive(isActive);
    }

    private void SetEffectOverlayColor(Color color, float alpha)
    {
        if (Image_EffectOverlay == null)
            return;

        color.a = alpha;
        Image_EffectOverlay.color = color;
    }

    private void HideEffectOverlay()
    {
        if (Image_EffectOverlay != null)
            SetEffectOverlayColor(_effectBaseColor, 0f);

        if (Object_EffectCanvas != null)
            Object_EffectCanvas.SetActive(false);
    }

    private void ShowCommonSkipButton()
    {
        CreateCommonSkipButtonIfNeeded();

        if (Object_CommonSkipButton == null)
            return;

        Object_CommonSkipButton.SetActive(true);

        if (_buttonCommonSkip != null)
        {
            _buttonCommonSkip.SetGroupName(_currentGroupName, _nextGroupName);
            _buttonCommonSkip.SetButtonText(_skipButtonText);
        }
    }

    private void HideCommonSkipButton()
    {
        if (Object_CommonSkipButton != null)
            Object_CommonSkipButton.SetActive(false);
    }

    private void CreateCommonSkipButtonIfNeeded()
    {
        if (Object_CommonSkipButton != null)
            return;

        if (Prefab_CommonSkipButton == null)
            return;

        _buttonCommonSkip = Instantiate(Prefab_CommonSkipButton, transform, false);
        Object_CommonSkipButton = _buttonCommonSkip.gameObject;
        Object_CommonSkipButton.name = "CommonSkipButton_Senario1";
        ApplySkipButtonPosition(Object_CommonSkipButton.transform as RectTransform);
        Object_CommonSkipButton.SetActive(false);
    }

    private void ApplySkipButtonPosition(RectTransform rectTransform)
    {
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = _skipButtonAnchoredPosition;
    }

    private IEnumerator WaitForManagersReady()
    {
        while (OOTechGameDataManager.Inst == null || OOTechUIManager.Inst == null)
            yield return null;
    }

    private void FreezeTimeForTutorial()
    {
        Time.timeScale = 0f;
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = 1f;
    }

    private void SetObjectActive(Transform target, bool isActive)
    {
        if (target != null)
            target.gameObject.SetActive(isActive);
    }

    private Transform FindChildTransform(string objectName)
    {
        GameObject childObject = FindChildGameObject(objectName);
        return childObject != null ? childObject.transform : null;
    }

    private GameObject FindChildGameObject(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

        Transform[] childArray = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in childArray)
        {
            if (child == null || child == transform)
                continue;

            if (child.name == objectName)
                return child.gameObject;
        }

        return null;
    }

    private GameObject FindSceneGameObject(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

        GameObject[] sceneObjectArray = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject sceneObject in sceneObjectArray)
        {
            if (sceneObject == null || sceneObject.name != objectName)
                continue;

            if (!sceneObject.scene.IsValid() || sceneObject.hideFlags != HideFlags.None)
                continue;

            return sceneObject;
        }

        return null;
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

    private void StopMainRoutine()
    {
        if (_mainRoutine == null)
            return;

        StopCoroutine(_mainRoutine);
        _mainRoutine = null;
    }
}
