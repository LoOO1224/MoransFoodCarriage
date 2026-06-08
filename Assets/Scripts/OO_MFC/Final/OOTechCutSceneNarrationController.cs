// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCutSceneNarrationController.cs
// - 역할: PreFinal_Narration과 EpilogueGroup의 한 컷짜리 나레이션을 진행합니다.
// - 영화 비유: 한 장면 배경을 꽉 잡고 내레이션 자막만 넘기는 컷신 조감독입니다.
// - 유지보수 포인트: 나레이션 ID와 다음 그룹은 OO_FinalCueSheet에서 우선 읽고, 데이터가 없으면 fallback으로 진행합니다.
// =============================================================================
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

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
        DisableLegacyPrologueControllerIfNeeded();
        ResolveComponents();
        ResolveCueSheetData();
        RequestPlayCutSceneBGM();
        RequestHideHUD();
        FitCameraToLargestBackground();

        if (Coroutine_Sequence != null)
            StopCoroutine(Coroutine_Sequence);

        Coroutine_Sequence = StartCoroutine(PlayCutSceneRoutine());
    }

    private void DisableLegacyPrologueControllerIfNeeded()
    {
        PrologueController legacyController = GetComponent<PrologueController>();

        if (legacyController != null && legacyController.enabled)
            legacyController.enabled = false;
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

        Cue_Dialogue.SetCutSceneBottomLayoutEnabled(true);
    }

    private void ResolveCueSheetData()
    {
        Data_CueSheet = OOTechGameDataManager.Inst != null
            ? OOTechGameDataManager.Inst.GetFinalCueSheetData(_cueSheetDataId)
            : null;
    }

    private void RequestPlayCutSceneBGM()
    {
        if (OOTechSoundManager.Inst == null)
            return;

        AudioClip bgmClip = null;

        if (gameObject.name == ResolvePreFinalGroupName())
            bgmClip = ResolveFinalStageBGMClip();
        else
            bgmClip = ResolveMainMenuBGMClip();

        if (bgmClip != null)
            OOTechSoundManager.Inst.PlayBGM(bgmClip, true);
    }

    private AudioClip ResolveFinalStageBGMClip()
    {
        string resourcePath = Data_CueSheet != null && !string.IsNullOrEmpty(Data_CueSheet.FinalStageBGMPath)
            ? Data_CueSheet.FinalStageBGMPath
            : "Audio/BGM/FinalStage_BGM";
        AudioClip clip = Resources.Load<AudioClip>(resourcePath);

        if (clip != null)
            return clip;

#if UNITY_EDITOR
        clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/BGM/FinalStage_BGM.mp3");

        if (clip != null)
            return clip;
#endif

        return null;
    }

    private AudioClip ResolveMainMenuBGMClip()
    {
        AudioClip clip = ResolveMainMenuBGMClipFromScene();

        if (clip != null)
            return clip;

        clip = Resources.Load<AudioClip>("Audio/BGM/MainMenu_BGM");

        if (clip != null)
            return clip;

#if UNITY_EDITOR
        clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/BGM/MainMenu_BGM.mp3");

        if (clip != null)
            return clip;
#endif

        return null;
    }

    private AudioClip ResolveMainMenuBGMClipFromScene()
    {
        MainMenuBGMPlayer[] playerArray = Resources.FindObjectsOfTypeAll<MainMenuBGMPlayer>();

        foreach (MainMenuBGMPlayer player in playerArray)
        {
            if (player == null)
                continue;

            AudioClip clip = player.ResolveMainMenuBGMClip();

            if (clip != null)
                return clip;
        }

        return null;
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

        bool isUICutScenePrepared = PrepareUICutSceneBackground();

        SpriteRenderer backgroundRenderer = ResolveLargestSpriteRenderer();

        if (backgroundRenderer == null)
        {
            if (isUICutScenePrepared)
                DisableCameraFollow(mainCamera);

            return;
        }

        backgroundRenderer.gameObject.SetActive(true);
        backgroundRenderer.enabled = true;
        backgroundRenderer.sortingLayerName = "Background";
        backgroundRenderer.sortingOrder = -1000;

        DisableCameraFollow(mainCamera);

        Bounds bounds = backgroundRenderer.bounds;
        float sizeByHeight = bounds.extents.y;
        float sizeByWidth = bounds.extents.x / Mathf.Max(0.01f, mainCamera.aspect);
        Vector3 cameraPosition = bounds.center;
        cameraPosition.z = mainCamera.transform.position.z;

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
        mainCamera.transform.position = cameraPosition;
    }

    private bool PrepareUICutSceneBackground()
    {
        Image backgroundImage = ResolveUICutSceneImage();

        if (backgroundImage == null)
            return false;

        backgroundImage.gameObject.SetActive(true);
        backgroundImage.enabled = true;
        backgroundImage.raycastTarget = false;

        Color color = backgroundImage.color;
        color.a = 1f;
        backgroundImage.color = color;

        RectTransform rectTransform = backgroundImage.rectTransform;
        rectTransform.localScale = Vector3.one;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;

        Canvas canvas = backgroundImage.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = -10;
        }

        return true;
    }

    private Image ResolveUICutSceneImage()
    {
        Image[] imageArray = GetComponentsInChildren<Image>(true);

        foreach (Image image in imageArray)
        {
            if (image == null || image.sprite == null)
                continue;

            string objectName = image.gameObject.name;

            if (objectName.Contains("CutScene") || objectName.Contains("Background") || objectName.Contains("Backound"))
                return image;
        }

        return null;
    }

    private void DisableCameraFollow(Camera mainCamera)
    {
        if (mainCamera == null)
            return;

        CameraFollowController followController = mainCamera.GetComponent<CameraFollowController>();

        if (followController != null)
            followController.enabled = false;
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
        GameObject closingObject = RequestSceneObjectByName(closingGroupName);
        GameObject openingObject = RequestSceneObjectByName(openingGroupName);

        if (OOTechUIManager.Inst != null)
        {
            if (closingObject != null)
                OOTechUIManager.Inst.RegisterUI(closingGroupName, closingObject);

            if (openingObject != null)
                OOTechUIManager.Inst.RegisterUI(openingGroupName, openingObject);

            OOTechUIManager.Inst.CloseUI(closingGroupName);

            if (OOTechUIManager.Inst.OpenUI(openingGroupName))
                return;
        }

        if (openingObject != null)
            openingObject.SetActive(true);

        if (closingObject != null)
            closingObject.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    private GameObject RequestSceneObjectByName(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            return null;

        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return null;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            Transform foundTransform = RequestChildObjectByName(rootObject.transform, objectName);

            if (foundTransform != null)
                return foundTransform.gameObject;
        }

        return null;
    }

    private Transform RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null || string.IsNullOrWhiteSpace(objectName))
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
}
