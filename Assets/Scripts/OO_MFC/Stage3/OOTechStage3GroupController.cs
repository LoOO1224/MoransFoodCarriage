// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechStage3GroupController.cs
// - 역할: Stage3Group의 첫 입장 큐만 지휘하는 얇은 Controller입니다.
// - 영화 비유: 무대감독은 "마차 입장, 산군 등장, 첫 대사, Encounter 전환" 큐만 부릅니다.
// - 유지보수 포인트: 배우 찾기는 역할표(OOTechSceneObject), 대사/수치는 OO_Stage3CueSheet 데이터가 담당합니다.
// =============================================================================
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Stage3Group의 시작 연출을 담당합니다.
/// Game View에서는 MFC가 EntryPoint_A까지 이동한 뒤, 산군이 커지며 위협/공격 애니메이션을 보여주고 EncounterGroup으로 넘어갑니다.
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

    private OO_Stage3CueSheet Data_CueSheet;
    private Coroutine Coroutine_Sequence;
    private Vector3 _sangunOriginalScale;
    private bool _hasSangunOriginalScale;

    /// <summary>
    /// 그룹이 켜질 때 Stage3 첫 큐를 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        RequestStartStage3OpeningCue();
    }

    /// <summary>
    /// 그룹이 꺼질 때 진행 중인 큐를 정리합니다.
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
    /// 리허설 중 Stage3 첫 큐를 다시 실행할 수 있게 하는 공개 메서드입니다.
    /// </summary>
    public void RequestStartStage3OpeningCue()
    {
        ResolveComponents();
        ResolveCueSheetData();

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

        if (Cue_Dialogue != null)
            yield return Cue_Dialogue.RequestShowDialogueAndWait(Data_CueSheet.SangunFirstDialogueId);

        yield return new WaitForSeconds(0.25f);
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
    /// Stage3 배경 전체가 Game View에 들어오도록 카메라를 맞춥니다.
    /// 촬영감독이 배우 등장 전에 무대 전체 샷을 먼저 잡는 단계입니다.
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
    /// 산군이 작게 나타난 뒤 원래 크기로 커지며 위협 애니메이션을 켭니다.
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
        GameObject encounterGroup = FindSceneObjectByName(encounterGroupName);

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

    private GameObject FindSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = FindChildByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private GameObject FindChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrEmpty(objectName))
            return null;

        if (rootTransform.name == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = FindChildByName(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }
}
