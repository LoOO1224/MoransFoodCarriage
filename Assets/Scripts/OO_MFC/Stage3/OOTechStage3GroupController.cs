// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechStage3GroupController.cs
// - ??븷: Stage3Group??泥??낆옣 ?먮쭔 吏?섑븯???뉗? Controller?낅땲??
// - ?곹솕 鍮꾩쑀: 臾대?媛먮룆? "留덉감 ?낆옣, ?곌뎔 ?깆옣, 泥???? Encounter ?꾪솚" ?먮쭔 遺由낅땲??
// - ?좎?蹂댁닔 ?ъ씤?? 諛곗슦 李얘린????븷??OOTechSceneObject), ????섏튂??OO_Stage3CueSheet ?곗씠?곌? ?대떦?⑸땲??
// =============================================================================
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Stage3Group???쒖옉 ?곗텧???대떦?⑸땲??
/// Game View?먯꽌??MFC媛 EntryPoint_A源뚯? ?대룞???? ?곌뎔??而ㅼ?硫??꾪삊/怨듦꺽 ?좊땲硫붿씠?섏쓣 蹂댁뿬二쇨퀬 EncounterGroup?쇰줈 ?섏뼱媛묐땲??
/// </summary>
[DisallowMultipleComponent]
public class OOTechStage3GroupController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string _cueSheetDataId = "Stage3_CueSheet_01";

    [Header("Role Components")]
    [SerializeField] private OOTechSceneContext Context_Scene;
    [SerializeField] private OOTechStage3DialogueCue Cue_Dialogue;
    [SerializeField] private OOTechStage2CameraCue Cue_Camera;

    [Header("BGM")]
    [SerializeField] private AudioClip _sangunBGM;
#if UNITY_EDITOR
    [SerializeField] private string _sangunBGMAssetPath = "Assets/Sounds/BGM/Sangun_BGM.mp3";
#endif
    [SerializeField] private float _sangunBGMDelaySeconds = 3f;

    private OO_Stage3CueSheet Data_CueSheet;
    private Coroutine Coroutine_Sequence;
    private Vector3 _sangunOriginalScale;
    private bool _hasSangunOriginalScale;
    private bool _hasRequestedSangunBGM;

    /// <summary>
    /// 洹몃９??耳쒖쭏 ??Stage3 泥??먮? ?쒖옉?⑸땲??
    /// </summary>
    private void OnEnable()
    {
        RequestStartStage3OpeningCue();
    }

    /// <summary>
    /// 洹몃９??爰쇱쭏 ??吏꾪뻾 以묒씤 ?먮? ?뺣━?⑸땲??
    /// </summary>
    private void OnDisable()
    {
        if (Coroutine_Sequence != null)
        {
            StopCoroutine(Coroutine_Sequence);
            Coroutine_Sequence = null;
        }
    }

    /// <summary>
    /// 由ы뿀??以?Stage3 泥??먮? ?ㅼ떆 ?ㅽ뻾?????덇쾶 ?섎뒗 怨듦컻 硫붿꽌?쒖엯?덈떎.
    /// </summary>
    public void RequestStartStage3OpeningCue()
    {
        ResolveComponents();
        ResolveCueSheetData();
        RequestPlaySangunBGMAfterDelay();

        if (Coroutine_Sequence != null)
            StopCoroutine(Coroutine_Sequence);

        Coroutine_Sequence = StartCoroutine(PlayStage3OpeningRoutine());
    }

    private IEnumerator PlayStage3OpeningRoutine()
    {
        if (Data_CueSheet == null)
        {
            Debug.LogWarning("[OOTechStage3GroupController] Stage3 cue sheet missing. Opening cue skipped.");
            yield break;
        }

        Context_Scene.CacheSceneObjects();
        RequestRepairZeroScale(transform);
        Transform mfcTransform = Context_Scene.GetRoleTransform(Data_CueSheet.MFCRoleId);
        Transform entryPointTransform = Context_Scene.GetRoleTransform(Data_CueSheet.EntryPointAId);
        Transform sangunTransform = Context_Scene.GetRoleTransform(Data_CueSheet.SangunRoleId);
        OOTechStageActorMotion mfcMotion = mfcTransform != null ? mfcTransform.GetComponent<OOTechStageActorMotion>() : null;

        RequestFitCameraToBackground();
        PrepareSangunOpeningState(sangunTransform);

        if (mfcMotion != null && entryPointTransform != null)
            yield return mfcMotion.MoveToTargetRoutine(entryPointTransform, 180f);

        yield return PlaySangunAppearRoutine(sangunTransform);
        yield return PlaySangunAnimationRoutine(sangunTransform, "Sangun_isAttacking", Data_CueSheet.AttackingAnimationSpeed, 1.2f);
        RequestPlayAnimatorState(sangunTransform != null ? sangunTransform.GetComponentInChildren<Animator>(true) : null, "Sangun_Idle", 1f);

        if (Cue_Dialogue != null && !string.IsNullOrEmpty(Data_CueSheet.SangunFirstDialogueId))
            yield return Cue_Dialogue.RequestShowDialogueAndWait(Data_CueSheet.SangunFirstDialogueId);

        RequestOpenEncounterGroup();
        Coroutine_Sequence = null;
    }

    private void ResolveComponents()
    {
        if (Context_Scene == null)
            Context_Scene = GetComponent<OOTechSceneContext>();

        if (Context_Scene == null)
            Context_Scene = gameObject.AddComponent<OOTechSceneContext>();

        if (Cue_Dialogue == null)
            Cue_Dialogue = GetComponent<OOTechStage3DialogueCue>();

        if (Cue_Dialogue == null)
            Cue_Dialogue = gameObject.AddComponent<OOTechStage3DialogueCue>();

        if (Cue_Camera == null)
            Cue_Camera = GetComponent<OOTechStage2CameraCue>();

        if (Cue_Camera == null)
            Cue_Camera = gameObject.AddComponent<OOTechStage2CameraCue>();
    }

    private void ResolveCueSheetData()
    {
        Data_CueSheet = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStage3CueSheetData(_cueSheetDataId) : null;
    }

    /// <summary>
    /// Stage3 ?낆옣 ???곌뎔??湲곗슫???쒖꽌??源붾━?꾨줉 BGM ?먮? ?덉빟?⑸땲??
    /// 臾대? 鍮꾩쑀濡쒕뒗 諛곗슦媛 蹂댁씠湲?吏곸쟾???ㅼ??ㅽ듃?쇨? ??쾶 源붾━????대컢?낅땲??
    /// </summary>
    private void RequestPlaySangunBGMAfterDelay()
    {
        if (_hasRequestedSangunBGM)
            return;

        _hasRequestedSangunBGM = true;

        MonoBehaviour coroutineOwner = OOTechSoundManager.Inst != null ? OOTechSoundManager.Inst : this;
        coroutineOwner.StartCoroutine(PlaySangunBGMAfterDelayRoutine());
    }

    private IEnumerator PlaySangunBGMAfterDelayRoutine()
    {
        float delaySeconds = Mathf.Max(0f, _sangunBGMDelaySeconds);

        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        AudioClip bgmClip = ResolveSangunBGMClip();

        if (bgmClip == null)
        {
            Debug.LogWarning("[OOTechStage3GroupController] Sangun_BGM clip missing. Assign _sangunBGM in Inspector if this runs in build.");
            yield break;
        }

        if (OOTechSoundManager.Inst != null)
            OOTechSoundManager.Inst.PlayBGM(bgmClip, true);
    }

    private AudioClip ResolveSangunBGMClip()
    {
        if (_sangunBGM != null)
            return _sangunBGM;

        AudioClip resourcesClip = Resources.Load<AudioClip>("Audio/BGM/Sangun_BGM");

        if (resourcesClip != null)
            return resourcesClip;

#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(_sangunBGMAssetPath))
            return AssetDatabase.LoadAssetAtPath<AudioClip>(_sangunBGMAssetPath);
#endif

        return null;
    }

    /// <summary>
    /// Stage3 諛곌꼍 ?꾩껜媛 Game View???ㅼ뼱?ㅻ룄濡?移대찓?쇰? 留욎땅?덈떎.
    /// 珥ъ쁺媛먮룆??諛곗슦 ?깆옣 ?꾩뿉 臾대? ?꾩껜 ?룹쓣 癒쇱? ?〓뒗 ?④퀎?낅땲??
    /// </summary>
    private void RequestFitCameraToBackground()
    {
        SpriteRenderer backgroundRenderer = ResolveBestBackgroundRenderer();

        if (Cue_Camera == null || backgroundRenderer == null)
            return;

        Cue_Camera.RequestSaveAndDisableCameraFollow();
        Cue_Camera.RequestFocusCameraOnBounds(backgroundRenderer.bounds, 1f);
    }

    private SpriteRenderer ResolveBestBackgroundRenderer()
    {
        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();

        if (IsValidBackgroundRenderer(rootRenderer))
            return rootRenderer;

        SpriteRenderer[] rendererArray = GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer bestRenderer = null;
        float bestArea = 0f;

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (!IsValidBackgroundRenderer(spriteRenderer))
                continue;

            float area = spriteRenderer.bounds.size.x * spriteRenderer.bounds.size.y;

            if (area > bestArea)
            {
                bestRenderer = spriteRenderer;
                bestArea = area;
            }
        }

        return bestRenderer;
    }

    private bool IsValidBackgroundRenderer(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null || !spriteRenderer.enabled)
            return false;

        if (spriteRenderer.bounds.size.x <= 0.01f || spriteRenderer.bounds.size.y <= 0.01f)
            return false;

        string objectName = spriteRenderer.gameObject.name;
        return objectName.Contains("Background") || objectName.Contains("Backound") || spriteRenderer.transform == transform;
    }

    private void RequestRepairZeroScale(Transform targetTransform)
    {
        if (targetTransform == null)
            return;

        Vector3 scale = targetTransform.localScale;

        if (Mathf.Abs(scale.x) > 0.0001f && Mathf.Abs(scale.y) > 0.0001f && Mathf.Abs(scale.z) > 0.0001f)
            return;

        targetTransform.localScale = Vector3.one;
        Debug.LogWarning("[OOTechStage3GroupController] Stage3Group scale was zero. Repaired to Vector3.one.");
    }

    private void PrepareSangunOpeningState(Transform sangunTransform)
    {
        if (sangunTransform == null)
            return;

        sangunTransform.gameObject.SetActive(true);

        if (!_hasSangunOriginalScale)
        {
            _sangunOriginalScale = sangunTransform.localScale;
            _hasSangunOriginalScale = true;
        }

        float startRatio = Mathf.Clamp(Data_CueSheet.SangunStartScaleRatio, 0.05f, 1f);
        sangunTransform.localScale = _sangunOriginalScale * startRatio;
    }

    /// <summary>
    /// ?곌뎔???묎쾶 ?섑??????먮옒 ?ш린濡?而ㅼ?硫??꾪삊 ?좊땲硫붿씠?섏쓣 耳?땲??
    /// </summary>
    private IEnumerator PlaySangunAppearRoutine(Transform sangunTransform)
    {
        if (sangunTransform == null)
            yield break;

        Animator animator = sangunTransform.GetComponentInChildren<Animator>(true);
        float elapsedTime = 0f;
        bool isThreateningStarted = false;
        float threateningStartTime = 0f;
        float startRatio = Mathf.Clamp(Data_CueSheet.SangunStartScaleRatio, 0.05f, 1f);
        float triggerRatio = Mathf.Clamp(Data_CueSheet.SangunThreateningScaleRatio, startRatio, 1f);
        float duration = Mathf.Max(0.1f, Data_CueSheet.SangunAppearSeconds);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            float scaleRatio = Mathf.Lerp(startRatio, 1f, normalizedTime);
            sangunTransform.localScale = _sangunOriginalScale * scaleRatio;

            if (!isThreateningStarted && scaleRatio >= triggerRatio)
            {
                RequestPlayAnimatorState(animator, "Sangun_isThreatening", Data_CueSheet.ThreateningAnimationSpeed);
                isThreateningStarted = true;
                threateningStartTime = Time.time;
            }

            yield return null;
        }

        sangunTransform.localScale = _sangunOriginalScale;

        if (!isThreateningStarted)
            yield return PlaySangunAnimationRoutine(sangunTransform, "Sangun_isThreatening", Data_CueSheet.ThreateningAnimationSpeed, 1.2f);
        else
        {
            float threateningLength = GetAnimationClipLength(animator, "Sangun_isThreatening", 1.2f) / Mathf.Max(0.01f, Data_CueSheet.ThreateningAnimationSpeed);
            float waitedSeconds = Time.time - threateningStartTime;
            yield return new WaitForSeconds(Mathf.Max(0.1f, threateningLength - waitedSeconds));
        }
    }

    private IEnumerator PlaySangunAnimationRoutine(Transform sangunTransform, string stateName, float speed, float fallbackSeconds)
    {
        Animator animator = sangunTransform != null ? sangunTransform.GetComponentInChildren<Animator>(true) : null;

        if (animator == null || string.IsNullOrEmpty(stateName))
            yield break;

        animator.speed = Mathf.Max(0.01f, speed);
        RequestPlayAnimatorState(animator, stateName, speed);
        yield return new WaitForSeconds(GetAnimationClipLength(animator, stateName, fallbackSeconds) / Mathf.Max(0.01f, speed));
    }

    private void RequestPlayAnimatorState(Animator animator, string stateName, float speed)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            return;

        animator.speed = Mathf.Max(0.01f, speed);
        int stateHash = Animator.StringToHash(stateName);

        if (animator.HasState(0, stateHash))
            animator.Play(stateName, 0, 0f);
        else
            Debug.LogWarning($"[OOTechStage3GroupController] Missing Sangun animation state: {stateName}", animator);
    }

    private void RequestOpenEncounterGroup()
    {
        string encounterGroupName = Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.EncounterGroupId)
            ? Data_CueSheet.EncounterGroupId
            : "EncounterGroup";
        GameObject encounterGroup = RequestSceneObjectByName(encounterGroupName);

        if (encounterGroup != null && OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.RegisterUI(encounterGroupName, encounterGroup);

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.CloseUI(gameObject.name);
            OOTechUIManager.Inst.OpenUI(encounterGroupName);
            return;
        }

        gameObject.SetActive(false);

        if (encounterGroup != null)
            encounterGroup.SetActive(true);
    }

    private float GetAnimationClipLength(Animator animator, string clipName, float fallbackSeconds)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return fallbackSeconds;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == clipName)
                return Mathf.Max(0.1f, clip.length);
        }

        return fallbackSeconds;
    }

    private GameObject RequestSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = RequestChildObjectByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private GameObject RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(objectName))
            return null;

        if (rootTransform.name == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = RequestChildObjectByName(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }
}

