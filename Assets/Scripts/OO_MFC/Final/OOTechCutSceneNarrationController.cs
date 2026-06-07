// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCutSceneNarrationController.cs
// - 역할: PreFinal_Narration과 EpilogueGroup의 한 컷짜리 나레이션을 진행합니다.
// - 영화 비유: 한 장면 배경을 꽉 잡고 내레이션 자막만 넘기는 컷신 조감독입니다.
// - 유지보수 포인트: 나레이션 ID와 다음 그룹은 OO_FinalCueSheet에서 우선 읽고, 데이터가 없으면 fallback으로 진행합니다.
// =============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class OOTechCutSceneNarrationController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string _cueSheetDataId = "Final_CueSheet_01";

    private OO_FinalCueSheet Data_CueSheet;
    private OOTechStage3DialogueCue Cue_Dialogue;
    private Coroutine Coroutine_Sequence;

    /// <summary>
    /// 컷신 그룹이 켜지면 배경을 카메라에 맞추고 나레이션을 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        ResolveComponents();
        ResolveCueSheetData();
        RequestHideHUD();
        FitCameraToLargestBackground();

        if (Coroutine_Sequence != null)
            StopCoroutine(Coroutine_Sequence);

        Coroutine_Sequence = StartCoroutine(PlayCutSceneRoutine());
    }

    private void OnDisable()
    {
        if (Coroutine_Sequence != null)
        {
            StopCoroutine(Coroutine_Sequence);
            Coroutine_Sequence = null;
        }
    }

    private IEnumerator PlayCutSceneRoutine()
    {
        string narrationId = ResolveNarrationId();
        string fallbackText = gameObject.name == ResolvePreFinalGroupName()
            ? "모든 여정의 마지막 문이 열립니다."
            : "모란의 요리마차 이야기가 막을 내립니다.";

        yield return Cue_Dialogue.RequestShowNarrationAndWait(narrationId, fallbackText);
        RequestSwitchGroup(gameObject.name, ResolveNextGroupName());
        Coroutine_Sequence = null;
    }

    private void ResolveComponents()
    {
        if (Cue_Dialogue == null)
            Cue_Dialogue = GetComponent<OOTechStage3DialogueCue>();

        if (Cue_Dialogue == null)
            Cue_Dialogue = gameObject.AddComponent<OOTechStage3DialogueCue>();
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
            hudController.SetHUDVisible(false);
    }

    private string ResolveNarrationId()
    {
        if (gameObject.name == ResolvePreFinalGroupName())
            return Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.PreFinalNarrationId) ? Data_CueSheet.PreFinalNarrationId : "narration_prologue_08";

        return Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.EpilogueNarrationId) ? Data_CueSheet.EpilogueNarrationId : "narration_Epilogue_01";
    }

    private string ResolveNextGroupName()
    {
        if (gameObject.name == ResolvePreFinalGroupName())
            return Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.FinalStageGroupName) ? Data_CueSheet.FinalStageGroupName : "FinalStageGroup";

        return Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.EndingCreditGroupName) ? Data_CueSheet.EndingCreditGroupName : "EndingCreditGroup";
    }

    private string ResolvePreFinalGroupName()
    {
        return Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.PreFinalGroupName) ? Data_CueSheet.PreFinalGroupName : "PreFinal_Narration";
    }

    private void FitCameraToLargestBackground()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        SpriteRenderer backgroundRenderer = ResolveLargestSpriteRenderer();

        if (backgroundRenderer == null)
            return;

        backgroundRenderer.gameObject.SetActive(true);
        backgroundRenderer.enabled = true;
        backgroundRenderer.sortingLayerName = "Background";
        backgroundRenderer.sortingOrder = -1000;

        CameraFollowController followController = mainCamera.GetComponent<CameraFollowController>();

        if (followController != null)
            followController.enabled = false;

        Bounds bounds = backgroundRenderer.bounds;
        float sizeByHeight = bounds.extents.y;
        float sizeByWidth = bounds.extents.x / Mathf.Max(0.01f, mainCamera.aspect);
        Vector3 cameraPosition = bounds.center;
        cameraPosition.z = mainCamera.transform.position.z;

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
        mainCamera.transform.position = cameraPosition;
    }

    private SpriteRenderer ResolveLargestSpriteRenderer()
    {
        SpriteRenderer[] rendererArray = GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer bestRenderer = null;
        float bestArea = 0f;

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
                continue;

            float area = Mathf.Abs(spriteRenderer.bounds.size.x * spriteRenderer.bounds.size.y);

            if (area <= bestArea)
                continue;

            bestRenderer = spriteRenderer;
            bestArea = area;
        }

        return bestRenderer;
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
