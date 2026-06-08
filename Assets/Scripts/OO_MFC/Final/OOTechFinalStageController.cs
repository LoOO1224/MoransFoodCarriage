// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechFinalStageController.cs
// - ??븷: FinalStageGroup?먯꽌 Moran ?먮룞 ?대룞, YeonSanJa ?앹궗/?됰났 ?좊땲硫붿씠?? 留덉?留???щ? 吏꾪뻾?⑸땲??
// - ?곹솕 鍮꾩쑀: 留덉?留?臾대????숈꽑 媛먮룆?낅땲?? Moran? 怨꾨떒???ㅻⅤ??諛곗슦, YeonSanJa???대줈利덉뾽??諛쏅뒗 諛곗슦?낅땲??
// - ?좎?蹂댁닔 ?ъ씤?? YeonSanJa/YeonSanZa/YanSanZa ?쇱슜? 肄붾뱶?먯꽌 fallback 泥섎━?섎릺, ?κ린?곸쑝濡?JSON/?ㅻ툕?앺듃 ?대쫫? YeonSanJa濡??듭씪?⑸땲??
// =============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class OOTechFinalStageController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string _cueSheetDataId = "Final_CueSheet_01";

    [Header("BGM")]
    [SerializeField] private AudioClip Clip_FinalStageBGM;

    private OO_FinalCueSheet Data_CueSheet;
    private OOTechSceneContext Context_Scene;
    private OOTechStage3DialogueCue Cue_Dialogue;
    private Transform Transform_Moran;
    private Transform Transform_YeonSanJa;
    private Transform Transform_EndPoint;
    private SpriteRenderer Renderer_Background;
    private Coroutine Coroutine_Sequence;
    private bool _hasFullStageCameraView;
    private Vector3 _fullStageCameraPosition;
    private float _fullStageCameraSize;

    /// <summary>
    /// FinalStageGroup??耳쒖?硫?BGM, 移대찓?? 諛곗슦 ?꾩튂瑜?以鍮꾪븯怨?留덉?留??먮? ?쒖옉?⑸땲??
    /// </summary>
    private void OnEnable()
    {
        ResolveComponents();
        ResolveCueSheetData();
        RequestHideHUD();
        RequestPlayFinalBGM();
        FitCameraToBackground();

        if (Coroutine_Sequence != null)
            StopCoroutine(Coroutine_Sequence);

        Coroutine_Sequence = StartCoroutine(PlayFinalStageRoutine());
    }

    private void OnDisable()
    {
        if (Coroutine_Sequence != null)
        {
            StopCoroutine(Coroutine_Sequence);
            Coroutine_Sequence = null;
        }
    }

    private IEnumerator PlayFinalStageRoutine()
    {
        yield return Cue_Dialogue.RequestShowDialogueAndWait(ResolveExistingDialogueId(CreateOpeningDialogueCandidateList()));
        yield return MoveMoranToEndPointRoutine();
        yield return PlayYeonSanJaEatingRoutine();

        foreach (string dialogueId in CreateHappyDialogueCandidateList())
        {
            string resolvedDialogueId = ResolveExistingDialogueId(new List<string> { dialogueId });

            if (!string.IsNullOrEmpty(resolvedDialogueId))
                yield return Cue_Dialogue.RequestShowDialogueAndWait(resolvedDialogueId);
        }

        RequestSwitchGroup(gameObject.name, ResolveEpilogueGroupName());
        Coroutine_Sequence = null;
    }

    private IEnumerator MoveMoranToEndPointRoutine()
    {
        if (Transform_Moran == null || Transform_EndPoint == null)
            yield break;

        Vector3 startPosition = Transform_Moran.position;
        Vector3 startScale = Transform_Moran.localScale;
        float targetScale = Data_CueSheet != null && Data_CueSheet.MoranEndScale > 0f ? Data_CueSheet.MoranEndScale : 0.6f;
        float baseSpeed = Data_CueSheet != null && Data_CueSheet.MoranMoveSpeed > 0f ? Data_CueSheet.MoranMoveSpeed : 180f;
        float speed = baseSpeed * 0.5f;
        float stuckSeconds = (Data_CueSheet != null && Data_CueSheet.MoranStuckFallbackSeconds > 0f ? Data_CueSheet.MoranStuckFallbackSeconds : 1.5f) * 1.5f;
        float lastProgressDistance = Vector3.Distance(Transform_Moran.position, Transform_EndPoint.position);
        float stuckTimer = 0f;

        RequestPlayActorState(Transform_Moran, "Moran_isWalking", 1f, true);

        while (Vector3.Distance(Transform_Moran.position, Transform_EndPoint.position) > 4f)
        {
            float distance = Vector3.Distance(Transform_Moran.position, Transform_EndPoint.position);

            if (distance >= lastProgressDistance - 0.1f)
                stuckTimer += Time.deltaTime;
            else
                stuckTimer = 0f;

            lastProgressDistance = distance;

            if (stuckTimer >= stuckSeconds)
            {
                Debug.LogWarning("[OOTechFinalStageController] Moran movement was stuck. Direct interpolation fallback engaged.");
                break;
            }

            Transform_Moran.position = Vector3.MoveTowards(Transform_Moran.position, Transform_EndPoint.position, speed * Time.deltaTime);
            ApplyMoranPerspectiveScale(startPosition, startScale, targetScale);
            yield return null;
        }

        float fallbackElapsed = 0f;
        Vector3 fallbackStart = Transform_Moran.position;

        float fallbackDuration = 2.4f;

        while (Vector3.Distance(Transform_Moran.position, Transform_EndPoint.position) > 1f && fallbackElapsed < fallbackDuration)
        {
            fallbackElapsed += Time.deltaTime;
            Transform_Moran.position = Vector3.Lerp(fallbackStart, Transform_EndPoint.position, fallbackElapsed / fallbackDuration);
            ApplyMoranPerspectiveScale(startPosition, startScale, targetScale);
            yield return null;
        }

        Transform_Moran.position = Transform_EndPoint.position;
        Transform_Moran.localScale = startScale * targetScale;
        RequestPlayActorState(Transform_Moran, "Moran_idle", 1f, false);
        RequestStopActorAnimation(Transform_Moran);
    }

    private void ApplyMoranPerspectiveScale(Vector3 startPosition, Vector3 startScale, float targetScale)
    {
        if (Transform_Moran == null || Transform_EndPoint == null)
            return;

        float totalDistance = Vector3.Distance(startPosition, Transform_EndPoint.position);
        float currentDistance = Vector3.Distance(startPosition, Transform_Moran.position);
        float normalized = totalDistance > 0.01f ? Mathf.Clamp01(currentDistance / totalDistance) : 1f;
        float scale = Mathf.Lerp(1f, targetScale, normalized);
        Transform_Moran.localScale = startScale * scale;
    }

    private IEnumerator ZoomCameraToYeonSanJaRoutine()
    {
        yield return ZoomCameraToYeonSanJaRoutine(1.6f);
    }

    private IEnumerator ZoomCameraToYeonSanJaRoutine(float duration)
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null || Transform_YeonSanJa == null)
            yield break;

        Vector3 startPosition = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;
        Vector3 targetPosition = Transform_YeonSanJa.position;
        targetPosition.z = startPosition.z;
        float targetSize = Data_CueSheet != null && Data_CueSheet.CameraZoomSize > 0f ? Data_CueSheet.CameraZoomSize : startSize * 0.55f;
        float elapsedTime = 0f;
        float safeDuration = Mathf.Max(0.1f, duration);

        while (elapsedTime < safeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsedTime / safeDuration));
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }
    }

    private IEnumerator RestoreFullStageCameraRoutine(float duration)
    {
        if (!_hasFullStageCameraView)
            yield break;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            yield break;

        Vector3 startPosition = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;
        float elapsedTime = 0f;
        float safeDuration = Mathf.Max(0.1f, duration);

        while (elapsedTime < safeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsedTime / safeDuration));
            mainCamera.transform.position = Vector3.Lerp(startPosition, _fullStageCameraPosition, t);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, _fullStageCameraSize, t);
            yield return null;
        }

        mainCamera.transform.position = _fullStageCameraPosition;
        mainCamera.orthographicSize = _fullStageCameraSize;
    }

    private IEnumerator PlayYeonSanJaEatingRoutine()
    {
        float speed = Data_CueSheet != null && Data_CueSheet.YeonSanJaEatingSpeed > 0f ? Data_CueSheet.YeonSanJaEatingSpeed : 0.5f;
        yield return ZoomCameraToYeonSanJaRoutine(1.8f);
        RequestPlayActorStateAny(Transform_YeonSanJa, speed, true, "YeonSanJa_isEatting", "YeonSanZa_isEatting", "YanSanZa_isEatting");
        yield return new WaitForSeconds(3.2f / Mathf.Max(0.01f, speed));
        RequestPlayActorStateAny(Transform_YeonSanJa, 1f, true, "YeonSanJa_isHappy", "YeonSanZa_isHappy", "YanSanZa_isHappy");
        yield return RestoreFullStageCameraRoutine(1.4f);
    }

    private void ResolveComponents()
    {
        if (Context_Scene == null)
            Context_Scene = GetComponent<OOTechSceneContext>();

        if (Context_Scene == null)
            Context_Scene = gameObject.AddComponent<OOTechSceneContext>();

        Context_Scene.CacheSceneObjects();

        if (Cue_Dialogue == null)
            Cue_Dialogue = GetComponent<OOTechStage3DialogueCue>();

        if (Cue_Dialogue == null)
            Cue_Dialogue = gameObject.AddComponent<OOTechStage3DialogueCue>();

        Cue_Dialogue.SetCutSceneBottomLayoutEnabled(true);

        Transform_Moran = ResolveRoleTransform(ResolveMoranRoleId(), "Moran");
        Transform_YeonSanJa = ResolveRoleTransform(ResolveYeonSanJaRoleId(), "YeonSanJa", "YeonSanZa", "YanSanZa");
        Transform_EndPoint = ResolveRoleTransform(ResolveEndPointRoleId(), "End_Point");
        Renderer_Background = ResolveBackgroundRenderer();
    }

    private void ResolveCueSheetData()
    {
        Data_CueSheet = OOTechGameDataManager.Inst != null
            ? OOTechGameDataManager.Inst.GetFinalCueSheetData(_cueSheetDataId)
            : null;
    }

    private void RequestHideHUD()
    {
        List<OOTechRoadHUDController> hudArray = OOTechSceneQuery.RequestCollectComponents<OOTechRoadHUDController>(true);

        foreach (OOTechRoadHUDController hudController in hudArray)
        {
            if (hudController != null)
                hudController.RequestForceHideForCutScene();
        }
    }

    private void RequestPlayFinalBGM()
    {
        AudioClip clip = Clip_FinalStageBGM;

        if (clip == null)
            clip = Resources.Load<AudioClip>("Audio/BGM/FinalStage_BGM");

        if (clip != null && OOTechSoundManager.Inst != null)
            OOTechSoundManager.Inst.PlayBGM(clip, true);
    }

    private void FitCameraToBackground()
    {
        if (Renderer_Background == null)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        CameraFollowController followController = mainCamera.GetComponent<CameraFollowController>();

        if (followController != null)
            followController.enabled = false;

        Bounds bounds = Renderer_Background.bounds;
        Vector3 cameraPosition = bounds.center;
        cameraPosition.z = mainCamera.transform.position.z;

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = Mathf.Max(bounds.extents.y, bounds.extents.x / Mathf.Max(0.01f, mainCamera.aspect));
        mainCamera.transform.position = cameraPosition;

        _fullStageCameraPosition = mainCamera.transform.position;
        _fullStageCameraSize = mainCamera.orthographicSize;
        _hasFullStageCameraView = true;
    }

    private void RequestPlayActorState(Transform actorTransform, string stateName, float speed, bool isForce)
    {
        if (actorTransform == null || string.IsNullOrEmpty(stateName))
            return;

        Animator animator = actorTransform.GetComponentInChildren<Animator>(true);

        if (animator == null)
            return;

        animator.enabled = true;
        animator.speed = Mathf.Max(0.01f, speed);
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        int stateHash = Animator.StringToHash(stateName);

        for (int layerIndex = 0; layerIndex < animator.layerCount; layerIndex++)
        {
            if (!animator.HasState(layerIndex, stateHash))
                continue;

            animator.Play(stateHash, layerIndex, isForce ? 0f : animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime);
            return;
        }

        Debug.LogWarning($"[OOTechFinalStageController] Animator state missing: {actorTransform.name}/{stateName}");
    }

    private void RequestStopActorAnimation(Transform actorTransform)
    {
        if (actorTransform == null)
            return;

        Animator animator = actorTransform.GetComponentInChildren<Animator>(true);

        if (animator != null)
            animator.speed = 0f;
    }

    private void RequestPlayActorStateAny(Transform actorTransform, float speed, bool isForce, params string[] stateNameArray)
    {
        if (actorTransform == null || stateNameArray == null)
            return;

        Animator animator = actorTransform.GetComponentInChildren<Animator>(true);

        if (animator == null)
            return;

        animator.enabled = true;
        animator.speed = Mathf.Max(0.01f, speed);
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        foreach (string stateName in stateNameArray)
        {
            if (string.IsNullOrEmpty(stateName))
                continue;

            int stateHash = Animator.StringToHash(stateName);

            for (int layerIndex = 0; layerIndex < animator.layerCount; layerIndex++)
            {
                if (!animator.HasState(layerIndex, stateHash))
                    continue;

                animator.Play(stateHash, layerIndex, isForce ? 0f : animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime);
                return;
            }
        }

        if (stateNameArray.Length > 0)
            Debug.LogWarning($"[OOTechFinalStageController] Animator state missing: {actorTransform.name}/{stateNameArray[0]}");
    }

    private Transform ResolveRoleTransform(string roleId, params string[] fallbackNameArray)
    {
        if (Context_Scene != null)
        {
            Transform roleTransform = Context_Scene.GetRoleTransform(roleId);

            if (roleTransform != null)
                return roleTransform;
        }

        foreach (string fallbackName in fallbackNameArray)
        {
            Transform foundTransform = RequestChildObjectByName(transform, fallbackName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
    }

    private SpriteRenderer ResolveBackgroundRenderer()
    {
        SpriteRenderer[] rendererArray = GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer bestRenderer = null;
        float bestArea = 0f;

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
                continue;

            if (spriteRenderer.gameObject.name == "FinalStageBackground")
                return spriteRenderer;

            float area = Mathf.Abs(spriteRenderer.bounds.size.x * spriteRenderer.bounds.size.y);

            if (area <= bestArea)
                continue;

            bestRenderer = spriteRenderer;
            bestArea = area;
        }

        return bestRenderer;
    }

    private string ResolveExistingDialogueId(List<string> candidateList)
    {
        if (OOTechGameDataManager.Inst == null)
            return candidateList != null && candidateList.Count > 0 ? candidateList[0] : string.Empty;

        foreach (string candidate in candidateList)
        {
            if (!string.IsNullOrEmpty(candidate) && OOTechGameDataManager.Inst.GetDialogueData(candidate) != null)
                return candidate;
        }

        return candidateList != null && candidateList.Count > 0 ? candidateList[0] : string.Empty;
    }

    private List<string> CreateOpeningDialogueCandidateList()
    {
        return new List<string>
        {
            Data_CueSheet != null ? Data_CueSheet.FinalOpeningDialogueId : string.Empty,
            "character_YeonSanJa_01",
            "character_YeonSanZa_01",
            "character_YanSanZa_01"
        };
    }

    private List<string> CreateHappyDialogueCandidateList()
    {
        if (Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.FinalHappyDialogueIdList))
            return SplitTextList(Data_CueSheet.FinalHappyDialogueIdList);

        if (IsDialogueExists("character_YanSanJa_02") || IsDialogueExists("character_YanSanJa_03"))
            return new List<string> { "character_YanSanJa_02", "character_YanSanJa_03" };

        if (IsDialogueExists("character_YeonSanJa_02") || IsDialogueExists("character_YeonSanJa_03"))
            return new List<string> { "character_YeonSanJa_02", "character_YeonSanJa_03" };

        if (IsDialogueExists("character_YeonSanZa_02") || IsDialogueExists("character_YeonSanZa_03"))
            return new List<string> { "character_YeonSanZa_02", "character_YeonSanZa_03" };

        return new List<string>
        {
            "character_YeonSanJa_02",
            "character_YeonSanJa_03"
        };
    }

    private bool IsDialogueExists(string dialogueId)
    {
        return OOTechGameDataManager.Inst != null &&
               !string.IsNullOrEmpty(dialogueId) &&
               OOTechGameDataManager.Inst.GetDialogueData(dialogueId) != null;
    }

    private List<string> SplitTextList(string rawText)
    {
        List<string> resultList = new List<string>();

        if (string.IsNullOrWhiteSpace(rawText))
            return resultList;

        string[] splitArray = rawText.Split('|');

        foreach (string value in splitArray)
        {
            if (!string.IsNullOrWhiteSpace(value))
                resultList.Add(value.Trim());
        }

        return resultList;
    }

    private string ResolveMoranRoleId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.MoranRoleId) ? Data_CueSheet.MoranRoleId : "Moran";
    private string ResolveYeonSanJaRoleId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.YeonSanJaRoleId) ? Data_CueSheet.YeonSanJaRoleId : "YeonSanJa";
    private string ResolveEndPointRoleId() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.EndPointRoleId) ? Data_CueSheet.EndPointRoleId : "End_Point";
    private string ResolveEpilogueGroupName() => Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.EpilogueGroupName) ? Data_CueSheet.EpilogueGroupName : "EpilogueGroup";

    private Transform RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(objectName))
            return null;

        if (rootTransform.name == objectName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = RequestChildObjectByName(rootTransform.GetChild(index), objectName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
    }

    private void RequestSwitchGroup(string closingGroupName, string openingGroupName)
    {
        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.CloseUI(closingGroupName);
            OOTechUIManager.Inst.OpenUI(openingGroupName);
            return;
        }

        gameObject.SetActive(false);
    }
}

