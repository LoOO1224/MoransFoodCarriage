// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechSenario1Controller.cs
// - 역할: OO_MFC 씬 진행을 보조하는 스크립트입니다.
// - 유지보수: 씬 참조, 데이터 ID, UIManager 전환 흐름을 확인하며 수정합니다.
// =============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Senario1Group ?꾩껜 ?먮쫫??吏?섑븯??肄쒖떆??而⑦듃濡ㅻ윭?낅땲??
/// ?쒖옉 ?쒗넗由ъ뼹, ?ъ씡 議곗옉, ?뚯떇 ?곹샇?묒슜, 蹂?? 移대찓???ъ빱?? ??? ?꾨즺 踰꾪듉 ?쒖꽌瑜?愿由ы빀?덈떎.
/// </summary>
public class OOTechSenario1Controller : MonoBehaviour
{
    // ?쎈뒗 ?쒖꽌:
    // 1. OnEnable/Start 怨꾩뿴: Senario1Group ?낆옣 吏곹썑 移대찓?? 諛곌꼍, Jaeik 議곗옉 ?좉툑??以鍮꾪빀?덈떎.
    // 2. TutorialGuide 愿??硫붿꽌?? ?쒖옉 ?덈궡 臾멸뎄瑜??곗씠???쒕━釉먯쑝濡??꾩슦怨?議곗옉???댁젣?⑸땲??
    // 3. Jaeik ?곹샇?묒슜 愿??硫붿꽌?? ?뚯떇 洹쇱쿂 E ?낅젰, 癒밴린 ?좊땲硫붿씠?? 蹂???곗텧??吏꾪뻾?⑸땲??
    // 4. DialogueSequence 愿??硫붿꽌?? ?섎젅?댁뀡, ?ъ씡援? 異섏뼇, 紐⑤? ??щ? ?쒖꽌?濡?吏꾪뻾?⑸땲??
    // 5. CameraFocus 愿??硫붿꽌?? ?꾩옱 留먰븯??諛곗슦?먭쾶 移대찓???ъ빱?ㅻ? ?섍퉩?덈떎.
    // ?좎?蹂댁닔 二쇱쓽:
    // - 罹먮┃???대룞? OOTechJaeikController 媛숈? 罹먮┃??而댄룷?뚰듃??留↔퉩?덈떎.
    // - 諛곌꼍/罹먮┃??李몄“??OOTechSceneContext? OOTechSceneObject ??븷?쒕? ?곗꽑 ?ъ슜?⑸땲??
    // - ??Controller媛 UI ?앹꽦源뚯? 留↔린 ?쒖옉?섎㈃ 踰꾧렇媛 而ㅼ?誘濡? UI??蹂꾨룄 View/Group?먯꽌 愿由ы빀?덈떎.

    private const string _roleJaeik = "Jaeik";
    private const string _roleQuestObject = "QuestObject";
    private const string _roleMrJaeik = "MrJaeik";
    private const string _roleChunyang = "Chunyang";
    private const string _roleMoran = "Moran";
    private const string _roleBackgroundBeforeTransformation = "BackgroundBeforeTransformation";
    private const string _roleBackgroundAfterTransformation = "BackgroundAfterTransformation";

    [Header("Scene Components")]
    [SerializeField] private OOTechSceneContext Context_Scene;

    [HideInInspector]
    [SerializeField] private OOTechJaeikController Character_Jaeik;
    [HideInInspector]
    [SerializeField] private Transform Transform_Jaeik;
    [HideInInspector]
    [SerializeField] private SpriteRenderer SpriteRenderer_Jaeik;
    [SerializeField] private Sprite Sprite_JaeikAfterInteraction;

    [HideInInspector]
    [SerializeField] private string _jaeikObjectName = "Jaeik";
    [HideInInspector]
    [SerializeField] private string _questObjectName = "S1_Object_Quest";
    [HideInInspector]
    [SerializeField] private string _mrJaeikObjectName = "Mr.Jaeik";
    [HideInInspector]
    [SerializeField] private string _chunyangObjectName = "Chunyang";
    [HideInInspector]
    [SerializeField] private string _chunyangFallbackObjectName = "ChunYang_Idle";
    [HideInInspector]
    [SerializeField] private string _moranObjectName = "Moran";
    [HideInInspector]
    [SerializeField] private string _moranFallbackObjectName = "Moran_Idle";
    [HideInInspector]
    [SerializeField] private string _backgroundObjectName = "Senario1Background";
    [HideInInspector]
    [SerializeField] private string _background2ObjectName = "Senario1Background2";

    [HideInInspector]
    [SerializeField] private Transform Transform_InteractionCube;
    [HideInInspector]
    [SerializeField] private Transform Transform_QuestObject;
    [HideInInspector]
    [SerializeField] private Transform Transform_MrJaeik;
    [HideInInspector]
    [SerializeField] private Transform Transform_Chunyang;
    [HideInInspector]
    [SerializeField] private Transform Transform_Moran;
    [HideInInspector]
    [SerializeField] private GameObject Object_Senario1Background;
    [HideInInspector]
    [SerializeField] private GameObject Object_Senario1Background2;

    [Header("Senario1 Rule")]
    [SerializeField] private bool _isSnapMrJaeikToJaeikPosition = true;
    [SerializeField] private float _questInteractionDistance = 2.9f;

    [Header("Camera")]
    [HideInInspector]
    [SerializeField] private CameraFollowController Camera_Follow;
    [HideInInspector]
    [SerializeField] private Camera Camera_Main;
    [SerializeField] private float _cameraFocusWaitSeconds = 0.65f;
    [SerializeField] private float _scenarioCameraPadding = 1.02f;
    [SerializeField] private float _fallbackScenarioCameraSize = 8f;
    [SerializeField] private float _maximumValidScenarioCameraSize = 50f;

    [Header("UI & Tutorial")]
    [HideInInspector]
    [SerializeField] private GameObject Group_TutorialGuide;
    [HideInInspector]
    [SerializeField] private OOTechTutorialGuideUI UI_TutorialGuide;
    [HideInInspector]
    [SerializeField] private GameObject Group_Dialogue;
    [HideInInspector]
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
    [SerializeField] private int _chunyangVisibleSortingOrder = 70;

    [Header("Character Idle Clip")]
    [SerializeField] private AnimationClip Clip_MrJaeikIdle;
    [SerializeField] private AnimationClip Clip_ChunyangIdle;

    [HideInInspector]
    [SerializeField] private Senario1_BGMPlayer BGM_Player;

    [Header("Skip Button")]
    [SerializeField] private NextButtonController Prefab_CommonSkipButton;
    [SerializeField] private string _currentGroupName = "Senario1Group";
    [SerializeField] private string _nextGroupName = "1st_Road_to_Stage1";
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

    /// <summary>
    /// Scenario1 臾대?媛 耳쒖?硫??꾩껜 吏꾪뻾 猷⑦떞???쒖옉?⑸땲??
    /// </summary>
    private void OnEnable()
    {
        StopMainRoutine();
        _mainRoutine = StartCoroutine(StartSenario1GroupRoutine());
    }

    /// <summary>
    /// Scenario1 臾대?媛 爰쇱쭏 ??肄붾（?? UI, ?쒓컙 ?뺤?, ?댄럺?몃? 紐⑤몢 ?뺣━?⑸땲??
    /// </summary>
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
        // ??猷⑦떞? Senario1Group??肄쒖떆?몄엯?덈떎.
        // 臾대?媛 ?대━硫??명듃? 諛곗슦瑜?珥덇린 ?꾩튂濡??명똿?섍퀬, ?쒗넗由ъ뼹 ?덈궡媛 ?앸궃 ?ㅼ뿉留?議곗옉沅뚯쓣 ?섍퉩?덈떎.
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

    /// <summary>
    /// ?ъ뿉 ?덈뒗 諛곗슦, 諛곌꼍, UI, BGM 李몄“瑜?紐⑤몢 ?섏쭛?⑸땲??
    /// ??븷?쒓? ?덉쑝硫?癒쇱? ?곌퀬, ?놁쑝硫??대쫫 寃?됱쑝濡?蹂댁“?⑸땲??
    /// </summary>
    private void CacheAllReferences()
    {
        CacheSceneContextReference();

        if (Transform_Jaeik == null)
            Transform_Jaeik = RequestChildTransform(_jaeikObjectName);

        if (Transform_QuestObject == null)
            Transform_QuestObject = RequestChildTransform(_questObjectName);

        if (Transform_InteractionCube == null)
            Transform_InteractionCube = Transform_QuestObject;

        if (Transform_MrJaeik == null)
            Transform_MrJaeik = RequestChildTransform(_mrJaeikObjectName);

        if (Transform_Chunyang == null)
            Transform_Chunyang = RequestChildTransform(_chunyangObjectName);

        if (Transform_Chunyang == null)
            Transform_Chunyang = RequestChildTransform(_chunyangFallbackObjectName);

        if (Transform_Moran == null)
            Transform_Moran = RequestChildTransform(_moranObjectName);

        if (Transform_Moran == null)
            Transform_Moran = RequestChildTransform(_moranFallbackObjectName);

        if (Object_Senario1Background == null)
            Object_Senario1Background = RequestChildGameObject(_backgroundObjectName);

        if (Object_Senario1Background2 == null)
            Object_Senario1Background2 = RequestChildGameObject(_background2ObjectName);

        if (SpriteRenderer_Jaeik == null && Transform_Jaeik != null)
            SpriteRenderer_Jaeik = Transform_Jaeik.GetComponent<SpriteRenderer>();

        CacheJaeikController();
        CacheCameraReference();
        CacheBGMReference();
        CacheUIReference();
        CacheBackgroundOriginSprite(Object_Senario1Background);
        CacheBackgroundOriginSprite(Object_Senario1Background2);
    }

    /// <summary>
    /// OOTechSceneObject ??븷?쒕? 癒쇱? ?쎌뒿?덈떎.
    /// ?덉쟾 ??蹂듭궗蹂몄쓣 ?꾪빐 ?대쫫 寃?됱? ?ㅼそ 蹂댁“留앹쑝濡쒕쭔 ?④꺼 ?〓땲??
    /// </summary>
    private void CacheSceneContextReference()
    {
        if (Context_Scene == null)
            Context_Scene = GetComponent<OOTechSceneContext>();

        if (Context_Scene == null)
            return;

        Context_Scene.CacheSceneObjects();

        Transform_Jaeik = ResolveRoleTransform(_roleJaeik, Transform_Jaeik);
        Transform_QuestObject = ResolveRoleTransform(_roleQuestObject, Transform_QuestObject);
        Transform_InteractionCube = ResolveRoleTransform(_roleQuestObject, Transform_InteractionCube);
        Transform_MrJaeik = ResolveRoleTransform(_roleMrJaeik, Transform_MrJaeik);
        Transform_Chunyang = ResolveRoleTransform(_roleChunyang, Transform_Chunyang);
        Transform_Moran = ResolveRoleTransform(_roleMoran, Transform_Moran);
        Object_Senario1Background = ResolveRoleObject(_roleBackgroundBeforeTransformation, Object_Senario1Background);
        Object_Senario1Background2 = ResolveRoleObject(_roleBackgroundAfterTransformation, Object_Senario1Background2);

        if (Transform_Jaeik != null)
        {
            Character_Jaeik = Transform_Jaeik.GetComponent<OOTechJaeikController>();
            SpriteRenderer_Jaeik = Transform_Jaeik.GetComponent<SpriteRenderer>();
        }
    }

    private Transform ResolveRoleTransform(string roleId, Transform fallback)
    {
        Transform roleTransform = Context_Scene.GetRoleTransform(roleId);
        return roleTransform != null ? roleTransform : fallback;
    }

    private GameObject ResolveRoleObject(string roleId, GameObject fallback)
    {
        GameObject roleObject = Context_Scene.GetRoleObject(roleId);
        return roleObject != null ? roleObject : fallback;
    }

    /// <summary>
    /// Jaeik ?ㅻ툕?앺듃???대룞/?먰봽/?곹샇?묒슜 而⑦듃濡ㅻ윭瑜??곌껐?⑸땲??
    /// </summary>
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

    /// <summary>
    /// 硫붿씤 移대찓?쇱? CameraFollowController瑜?李얠븘 移대찓???먯뿉 ?ъ슜??以鍮꾨? ?⑸땲??
    /// </summary>
    private void CacheCameraReference()
    {
        if (Camera_Main == null)
            Camera_Main = Camera.main;

        if (Camera_Follow == null && Camera_Main != null)
            Camera_Main.TryGetComponent(out Camera_Follow);

        if (Camera_Main != null && (_originCameraOrthographicSize <= 0f || IsCameraSizeTooWide(_originCameraOrthographicSize)))
            _originCameraOrthographicSize = ResolveScenarioCameraSize();
    }

    /// <summary>
    /// Scenario1Group???쒖옉?????ъ슜??湲곕낯 移대찓???ш린瑜?諛곌꼍 湲곗??쇰줈 怨꾩궛?⑸땲??
    /// ?댁쟾 Road/Stage???볦? 珥ъ쁺 媛믪씠 ?⑥븘 ?덉쑝硫?罹먮┃??臾대?媛 ?덈Т 硫由?蹂댁씠誘濡? ??洹몃９??諛곌꼍 ?ш린瑜?湲곗? ?뚯쫰濡??곷땲??
    /// </summary>
    private float ResolveScenarioCameraSize()
    {
        SpriteRenderer backgroundRenderer = ResolveScenarioBackgroundRenderer();

        if (backgroundRenderer == null || Camera_Main == null)
            return Mathf.Max(1f, _fallbackScenarioCameraSize);

        Bounds bounds = backgroundRenderer.bounds;
        float aspect = Mathf.Max(0.01f, Camera_Main.aspect);
        float sizeByHeight = bounds.extents.y;
        float sizeByWidth = bounds.extents.x / aspect;
        return Mathf.Max(1f, Mathf.Max(sizeByHeight, sizeByWidth) * Mathf.Max(1f, _scenarioCameraPadding));
    }

    private SpriteRenderer ResolveScenarioBackgroundRenderer()
    {
        GameObject backgroundObject = Object_Senario1Background != null ? Object_Senario1Background : RequestChildGameObject(_backgroundObjectName);

        if (backgroundObject == null)
            backgroundObject = Object_Senario1Background2 != null ? Object_Senario1Background2 : RequestChildGameObject(_background2ObjectName);

        return backgroundObject != null ? backgroundObject.GetComponentInChildren<SpriteRenderer>(true) : null;
    }

    private bool IsCameraSizeTooWide(float cameraSize)
    {
        return cameraSize > Mathf.Max(1f, _maximumValidScenarioCameraSize);
    }

    private void ApplyScenarioBaseCameraSize()
    {
        if (Camera_Main == null || !Camera_Main.orthographic)
            return;

        Camera_Main.orthographicSize = GetOriginCameraSize();
    }

    private void CacheBGMReference()
    {
        if (BGM_Player == null)
            BGM_Player = GetComponent<Senario1_BGMPlayer>();

        if (BGM_Player == null)
            BGM_Player = GetComponentInChildren<Senario1_BGMPlayer>(true);
    }

    /// <summary>
    /// DialogueGroup怨?TutorialGuideGroup??李얠븘 ?곗씠???쒖떆??UI濡??곌껐?⑸땲??
    /// </summary>
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

        uiObject = RequestChildGameObject(groupName);

        if (OOTechUIManager.Inst != null)
            uiObject = uiObject != null ? uiObject : OOTechUIManager.Inst.GetCreatedUI(groupName);

        if (uiObject == null)
            uiObject = RequestSceneGameObject(groupName);

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

    /// <summary>
    /// Scenario1 ?곗텧 ?쒓컙 媛믪쓣 ?붽뎄?ы빆 湲곗??쇰줈 怨좎젙?⑸땲??
    /// </summary>
    private void ApplyRequiredScenarioTiming()
    {
        _eatAnimatorSpeed = 0.3f;
        _transformedAnimatorSpeed = 0.2f;
        _postEatDelaySeconds = 4f;
        _transformationEffectSeconds = 4f;
        _blinkCount = 4;
    }

    /// <summary>
    /// ?ъ씡???吏곸씠湲???泥?臾대? ?곹깭瑜?留뚮벊?덈떎.
    /// 諛곌꼍1怨?Jaeik? 蹂댁씠怨? 蹂?????깆옣??諛곗슦?ㅼ? 臾대? ?ㅼ뿉 ?〓땲??
    /// </summary>
    private void PrepareInitialState()
    {
        // 泥??λ㈃ ?명똿:
        // 諛곌꼍1? 耳쒓퀬 諛곌꼍2, Mr.Jaeik, Chunyang, Moran? ?ㅼ뿉 ?깆옣??諛곗슦?쇱꽌 ?꾩쭅 臾대? ?ㅼ뿉 ?〓땲??
        HideCommonSkipButton();
        HideEffectOverlay();
        HideOpeningUIGroup();
        HideScenarioColliderRenderer();
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

    /// <summary>
    /// Coliider_ 怨꾩뿴 ?ㅻ툕?앺듃媛 ??踰쎌쿂??蹂댁씠吏 ?딅룄濡?Renderer瑜??뺣땲??
    /// Collider ?먯껜???댁븘 ?덉뼱???щ챸 踰쎌쑝濡?異⑸룎?⑸땲??
    /// </summary>
    private void HideScenarioColliderRenderer()
    {
        Transform[] childTransformArray = GetComponentsInChildren<Transform>(true);

        foreach (Transform childTransform in childTransformArray)
        {
            if (childTransform == null || !childTransform.name.StartsWith("Coliider_"))
                continue;

            SpriteRenderer spriteRenderer = childTransform.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            Renderer renderer = childTransform.GetComponent<Renderer>();

            if (renderer != null)
                renderer.enabled = false;
        }
    }

    /// <summary>
    /// 諛곌꼍 ?ㅻ툕?앺듃???쒖꽦?? Animator, SpriteRenderer ?곹깭瑜?珥덇린?뷀빀?덈떎.
    /// </summary>
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

    /// <summary>
    /// ????λ㈃???깆옣??罹먮┃?곗쓽 Idle ?좊땲硫붿씠?섏쓣 誘몃━ 以鍮꾪빀?덈떎.
    /// </summary>
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

    /// <summary>
    /// UIManager媛 TutorialGuideGroup怨?DialogueGroup???????덈룄濡??깅줉?⑸땲??
    /// </summary>
    private void RegisterRuntimeUIGroup()
    {
        if (OOTechUIManager.Inst == null)
            return;

        if (Group_TutorialGuide != null)
            OOTechUIManager.Inst.RegisterUI(_tutorialGuideGroupName, Group_TutorialGuide);

        if (Group_Dialogue != null)
            OOTechUIManager.Inst.RegisterUI(_dialogueGroupName, Group_Dialogue);
    }

    /// <summary>
    /// Jaeik?먭쾶 ?뚯떇 ?곹샇?묒슜 ??곴낵 E??肄쒕갚???곌껐?⑸땲??
    /// </summary>
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

    /// <summary>
    /// ????곗텧 以?Jaeik 議곗옉沅뚯쓣 ?좉툒?덈떎.
    /// </summary>
    private void LockJaeikControl()
    {
        if (Character_Jaeik == null)
            return;

        Character_Jaeik.LockMovement();
        Character_Jaeik.SetInputEnabled(false);
        Character_Jaeik.HideInteractionPrompt();
    }

    /// <summary>
    /// ?쒖옉 媛?대뱶媛 ?앸궃 ??Jaeik 議곗옉沅뚯쓣 ?뚮젅?댁뼱?먭쾶 ?섍퉩?덈떎.
    /// </summary>
    private void UnlockJaeikControl()
    {
        if (Character_Jaeik == null)
            return;

        Character_Jaeik.UnlockMovement();
        Character_Jaeik.SetInputEnabled(true);
    }

    /// <summary>
    /// ?쒖옉 ?쒗넗由ъ뼹??留덉튂怨??쒓컙 ?뺤?瑜?????ㅼ젣 ?뚮젅?대? ?쒖옉?⑸땲??
    /// </summary>
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

    /// <summary>
    /// ?뚯떇 洹쇱쿂?먯꽌 E ?곹샇?묒슜???ㅼ뼱?ㅻ㈃ 蹂???섏뒪??猷⑦떞????踰??쒖옉?⑸땲??
    /// </summary>
    private void OnEatInteractionRequested()
    {
        if (_isQuestSequenceStarted)
            return;

        _questRoutine = StartCoroutine(PlayQuestSequenceRoutine());
    }

    /// <summary>
    /// 癒밴린 ?좊땲硫붿씠?? 蹂???댄럺?? 諛곌꼍 援먯껜, ?꾩냽 ?곗씠????붾? ?쒖꽌?濡??ㅽ뻾?⑸땲??
    /// </summary>
    private IEnumerator PlayQuestSequenceRoutine()
    {
        // ?뚯떇怨??곹샇?묒슜???쒓컙遺?곕뒗 ?뚮젅?댁뼱 ?낅젰???좉툒?덈떎.
        // 諛곗슦媛 ?좊뱶由щ툕濡??吏곸씠吏 ?딄쾶 留됯퀬, 移대찓???④낵/?좊땲硫붿씠???먮? ?쒖꽌?濡??ㅽ뻾?섎뒗 援ш컙?낅땲??
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

    /// <summary>
    /// Jaeik_isEatting ?좊땲硫붿씠?섏쓣 ?ъ깮?섍퀬 ?앸궇 ?뚭퉴吏 湲곕떎由쎈땲??
    /// </summary>
    private IEnumerator PlayEatAnimationAndWait()
    {
        if (Character_Jaeik == null)
            yield break;

        bool done = false;
        Character_Jaeik.PlayEatAnimationOnce(_eatAnimatorSpeed, () => done = true);

        while (!done)
            yield return null;
    }

    /// <summary>
    /// 蹂???ㅽ봽?쇱씠??1?ъ씠???숈븞 移대찓?쇰? 以뚯씤?섍퀬, ?앸굹硫??먮옒 ?ш린濡?以뚯븘?껎빀?덈떎.
    /// </summary>
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

    /// <summary>
    /// ?뚯떇 ?곹샇?묒슜 ?댄썑 ?꾩슂?섎㈃ Jaeik ?ㅽ봽?쇱씠?몃? 援먯껜?⑸땲??
    /// </summary>
    private void ChangeJaeikAppearance()
    {
        if (SpriteRenderer_Jaeik != null && Sprite_JaeikAfterInteraction != null)
            SpriteRenderer_Jaeik.sprite = Sprite_JaeikAfterInteraction;
    }

    /// <summary>
    /// ?꾩껜 ?붾㈃ 源쒕묀?꾩쑝濡?蹂???먮꼫吏媛 ?곗????곗텧??留뚮벊?덈떎.
    /// </summary>
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

    /// <summary>
    /// 諛곌꼍 援먯껜 ?꾪썑濡?寃? ?섏씠?????꾩썐???ъ깮?⑸땲??
    /// </summary>
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

    /// <summary>
    /// ?붾㈃ ?꾩껜 ?ㅻ쾭?덉씠 ?됯낵 ?뚰뙆瑜?蹂닿컙???뚮옒???섏씠???④낵瑜?留뚮벊?덈떎.
    /// </summary>
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

    /// <summary>
    /// Senario1Background瑜??꾧퀬 Senario1Background2瑜?耳?땲??
    /// </summary>
    private void SwitchToSecondBackground()
    {
        // ?명듃 ?꾪솚:
        // 諛곌꼍1 臾대?留됱쓣 ?대━怨?諛곌꼍2 臾대?留됱쓣 ?щ┰?덈떎.
        PrepareBackgroundObject(Object_Senario1Background, false);
        PrepareBackgroundObject(Object_Senario1Background2, true);
    }

    /// <summary>
    /// 蹂?????먮옒 Jaeik???꾧퀬 Mr.Jaeik??耳???移대찓???ъ빱?ㅻ? ??퉩?덈떎.
    /// </summary>
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

    /// <summary>
    /// 蹂?????섎젅?댁뀡, ?ъ씡援?異섏뼇/紐⑤? ??? ?꾨즺 媛?대뱶, ?섏뼱媛湲?踰꾪듉???쒖꽌?濡??ㅽ뻾?⑸땲??
    /// </summary>
    private IEnumerator PlayAfterTransformDataSequenceRoutine()
    {
        // 媛먮룆 ?명듃:
        // 蹂?좎씠 ?앸궃 ??泥??쇳듃??諛곗슦 ??ш? ?꾨땲???섎젅?댁뀡?낅땲??
        // 臾대? ?꾩뿉 Mr.Jaeik 諛곗슦瑜??몄썙???곹깭?먯꽌 愿媛앹뿉寃??곹솴 ?ㅻ챸????而룸쭔 癒쇱? 蹂댁뿬以띾땲??
        yield return OpenNarrationAsDialogueAndWait(_narrationAfterTransformId, _narratorSpeakerId);

        // ?섎젅?댁뀡 而룹씠 ?앸굹硫?媛숈? DialogueGroup???ъ궗?⑺빐??
        // ?붿옄留?Mr.Jaeik?쇰줈 諛붽씀怨??ъ씡援곗쓽 泥???щ? ?곗씠?곗뿉??爰쇰궡 ?ъ깮?⑸땲??
        yield return OpenDialogueAndWait(_dialogueMrJaeikFirstId, _mrJaeikSpeakerId, Transform_MrJaeik, true);

        CloseDialogueGroup();
        yield return new WaitForSeconds(0.25f);

        // ?ㅼ쓬 ?λ㈃? 異섏뼇 ?깆옣?대?濡??⑤꼸???좉퉸 ?リ퀬,
        // 移대찓??議곕챸??異섏뼇 諛곗슦?먭쾶 ?섍릿 ???ㅼ떆 ????⑤꼸???쎈땲??
        SetObjectActive(Transform_Chunyang, true);
        RequestRestoreVisibleCharacterView(Transform_Chunyang, _chunyangVisibleSortingOrder);
        PrepareCharacterIdleAnimation(Transform_Chunyang, _chunyangIdleStateName, Clip_ChunyangIdle);
        FocusCamera(Transform_Chunyang, false);

        if (BGM_Player != null)
            BGM_Player.PlayChunyangFocusedBGM();

        yield return new WaitForSeconds(_cameraFocusWaitSeconds);
        yield return OpenDialogueAndWait(_dialogueChunyangFirstId, _chunyangSpeakerId, Transform_Chunyang, false);

        // ?ш린遺?곕뒗 ????⑤꼸???レ? ?딄퀬 ?좎??⑸땲??
        // 諛곗슦媛 留먰븷 ?뚮쭏??移대찓???ъ빱?ㅼ? ?붿옄 ?대쫫留?援먯껜?댁꽌 ???ъ쿂???댁뼱 蹂댁씠寃??⑸땲??
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

    /// <summary>
    /// Narration ?곗씠?곕? DialogueUI媛 ?쒖떆?????덈뒗 ????뺥깭濡?諛붽퓭 ???뚰듃 蹂댁뿬以띾땲??
    /// </summary>
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

    /// <summary>
    /// ?뱀젙 ?붿옄??Dialogue ?곗씠?곕? ?꾩슦怨? 移대찓?쇰? ?대떦 諛곗슦?먭쾶 ?대룞?쒗궓 ??湲곕떎由쎈땲??
    /// </summary>
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

    /// <summary>
    /// DialogueUI???곗씠?곕? ?쒖떆?섍퀬 ?뚮젅?댁뼱媛 ?댁뼱媛湲????뚭퉴吏 湲곕떎由쎈땲??
    /// </summary>
    private IEnumerator ShowDialogueDataAndWait(OO_Dialogue dialogueData)
    {
        if (UI_Dialogue == null || dialogueData == null)
            yield break;

        bool isDone = false;
        UI_Dialogue.ShowDialogue(dialogueData, () => isDone = true);

        while (!isDone)
            yield return null;
    }

    /// <summary>
    /// Narration??泥??띿뒪?몃? Dialogue ?뺥깭濡?媛먯떥 ?붿옄 ?대쫫??遺숈엯?덈떎.
    /// </summary>
    private OO_Dialogue CreateDialogueFromNarration(OO_Narration narrationData, string speakerCharacterId)
    {
        // OO_Narration ?섎굹瑜?DialogueGroup???쎌쓣 ???덈뒗 ?꾩떆 ?蹂?移대뱶濡?諛붽퓠?덈떎.
        // ?ш린?쒕뒗 narration_prologue_07??泥??뚰듃留??ъ슜?댁꽌 ??踰덈쭔 ?섍린寃??⑸땲??
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

    /// <summary>
    /// Dialogue ?곗씠?곗뿉 ?곗씠???쒕━釉??붿옄 ?대쫫??蹂댁젙??諛섑솚?⑸땲??
    /// </summary>
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

    /// <summary>
    /// TutorialGuideGroup???닿퀬 ?쒗넗由ъ뼹 ?곗씠????嫄댁쓣 ?쒖떆?????꾨즺源뚯? 湲곕떎由쎈땲??
    /// </summary>
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

    /// <summary>
    /// 移대찓?쇰? 利됱떆 Jaeik?먭쾶 留욎떠 Scenario1 ?쒖옉 珥덉젏???닿툔?섏? ?딄쾶 ?⑸땲??
    /// </summary>
    private void FocusCameraOnJaeikImmediately()
    {
        FocusCamera(Transform_Jaeik, true);
    }

    /// <summary>
    /// CameraFollowController???寃잛쓣 ?대떦 諛곗슦濡?諛붽씀怨? ?꾩슂?섎㈃ 移대찓?쇰? 利됱떆 ?ㅻ깄?⑸땲??
    /// </summary>
    private void FocusCamera(Transform target, bool isSnapImmediately)
    {
        if (target == null)
            return;

        CacheCameraReference();
        ApplyScenarioBaseCameraSize();

        if (Camera_Follow != null)
        {
            Camera_Follow.enabled = true;
            Camera_Follow.SetTarget(target);
        }

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

    /// <summary>
    /// 移대찓??orthographicSize瑜?蹂닿컙??蹂??以뚯씤/以뚯븘?껋쓣 留뚮벊?덈떎.
    /// </summary>
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

    /// <summary>
    /// ?꾩껜 ?붾㈃ ?댄럺?몄슜 Canvas? Image ?ㅻ쾭?덉씠瑜?以鍮꾪빀?덈떎.
    /// </summary>
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

    /// <summary>
    /// ?꾨Т ?꾨즺 ??怨듭슜 ?섏뼱媛湲?踰꾪듉??蹂댁뿬以띾땲??
    /// </summary>
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

    /// <summary>
    /// CommonSkipButton ?꾨━?뱀쓣 Scenario1Group ?먯떇?쇰줈 ??踰덈쭔 ?앹꽦?⑸땲??
    /// </summary>
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

    /// <summary>
    /// GameDataManager? UIManager媛 以鍮꾨맆 ?뚭퉴吏 湲곕떎由쎈땲??
    /// </summary>
    private IEnumerator WaitForManagersReady()
    {
        while (OOTechGameDataManager.Inst == null || OOTechUIManager.Inst == null)
            yield return null;
    }

    /// <summary>
    /// ?쒗넗由ъ뼹 ?덈궡 以?寃뚯엫 吏꾪뻾??硫덉텛湲??꾪빐 TimeScale??0?쇰줈 ?〓땲??
    /// </summary>
    private void FreezeTimeForTutorial()
    {
        Time.timeScale = 0f;
    }

    /// <summary>
    /// ?쒗넗由ъ뼹 醫낅즺 ??TimeScale??1濡?蹂듦뎄?⑸땲??
    /// </summary>
    private void RestoreTimeScale()
    {
        Time.timeScale = 1f;
    }

    private void SetObjectActive(Transform target, bool isActive)
    {
        if (target != null)
            target.gameObject.SetActive(isActive);
    }

    /// <summary>
    /// ?깆옣 諛곗슦??SpriteRenderer瑜??ㅼ떆 蹂댁씠???곹깭濡??뺣━?⑸땲??
    /// ?곹솕濡?移섎㈃ 臾대? ?꾩뿉 ?щ씪??諛곗슦媛 諛곌꼍留??ㅼ뿉 臾삵엳吏 ?딅룄濡?議곕챸怨??욌뮘 ?쒖꽌瑜??ㅼ떆 留욎텛???먯엯?덈떎.
    /// </summary>
    private void RequestRestoreVisibleCharacterView(Transform target, int minimumSortingOrder)
    {
        if (target == null)
            return;

        SpriteRenderer[] rendererArray = target.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null)
                continue;

            spriteRenderer.enabled = true;
            spriteRenderer.sortingLayerName = "Characters";
            spriteRenderer.sortingOrder = Mathf.Max(spriteRenderer.sortingOrder, minimumSortingOrder);

            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }

    private Transform RequestChildTransform(string objectName)
    {
        GameObject childObject = RequestChildGameObject(objectName);
        return childObject != null ? childObject.transform : null;
    }

    private GameObject RequestChildGameObject(string objectName)
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

    private GameObject RequestSceneGameObject(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

        List<GameObject> sceneObjectArray = new List<GameObject>();
        Scene activeScene = SceneManager.GetActiveScene();

        if (!activeScene.IsValid())
            return null;

        sceneObjectArray.AddRange(activeScene.GetRootGameObjects());

        foreach (GameObject sceneObject in sceneObjectArray)
        {
            GameObject foundObject = OOTechSceneQuery.RequestChildObjectByName(sceneObject != null ? sceneObject.transform : null, objectName);

            if (foundObject != null)
                return foundObject;
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


