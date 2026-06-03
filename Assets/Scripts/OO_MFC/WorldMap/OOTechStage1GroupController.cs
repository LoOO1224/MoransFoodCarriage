using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Stage1Group의 첫 플레이 흐름을 담당합니다.
/// Moran 배우의 좌우 이동, Stage1-1/Stage1-2 배경 전환, 스테이지 이름 연출, 임무 갱신을 지휘합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechStage1GroupController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string _stageDataId = "OO_Stage_1";
    [SerializeField] private string _stageQuestDataId = "Stage1_Quest_01";
    [SerializeField] private string _currentGroupName = "Stage1Group";

    [Header("Scene Object Names")]
    [SerializeField] private string _stageMap1Name = "Stage1-1";
    [SerializeField] private string _stageMap2Name = "Stage1-2";
    [SerializeField] private string _moranObjectName = "Moran";
    [SerializeField] private string _stageNameObjectName = "StageName";
    [SerializeField] private string _placeholderCanvasName = "Canvas_StagePlaceholder";

    [Header("Moran Movement")]
    [SerializeField] private KeyCode _moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode _moveRightKey = KeyCode.D;
    [SerializeField] private KeyCode _runKey = KeyCode.LeftShift;
    [SerializeField] private float _walkSpeed = 2.2f;
    [SerializeField] private float _runSpeed = 4.8f;
    [SerializeField] private float _leftStartMargin = 0.65f;
    [SerializeField] private float _edgeExitMargin = 0.1f;
    [SerializeField] private float _moranLaneNormalizedHeight = 0.2f;

    [Header("Animation State")]
    [SerializeField] private string _idleStateName = "Moran_idle";
    [SerializeField] private string _walkingStateName = "Moran_isWalking";
    [SerializeField] private string _runningStateName = "Moran_isRunning";
    [SerializeField] private float _walkingAnimationSpeed = 0.75f;
    [SerializeField] private float _runningAnimationSpeed = 1.5f;

    [Header("Transition")]
    [SerializeField] private float _fadeOutSeconds = 0.35f;
    [SerializeField] private float _blackHoldSeconds = 0.08f;
    [SerializeField] private float _fadeInSeconds = 0.4f;

    [Header("Stage Title")]
    [SerializeField] private float _stageTitleFadeSeconds = 0.6f;
    [SerializeField] private float _stageTitleHoldSeconds = 1.1f;

    private GameObject Object_StageMap1;
    private GameObject Object_StageMap2;
    private GameObject Object_Moran;
    private SpriteRenderer Renderer_Moran;
    private Animator Animator_Moran;
    private TextMeshProUGUI Text_StageName;
    private OOTechRoadHUDController HUD_Road;
    private Camera Camera_Main;
    private Canvas Canvas_Fade;
    private Image Image_Fade;
    private int _currentMapIndex;
    private bool _isChangingMap;
    private string _currentAnimationStateName;

    /// <summary>
    /// Stage1Group이 켜질 때 배우와 배경을 첫 장면 기준으로 준비합니다.
    /// </summary>
    private void OnEnable()
    {
        ResolveReferences();
        DisablePlaceholderCanvas();
        PrepareHUDMission();
        PrepareStage();
        StartCoroutine(PlayStageNameRoutine());
    }

    /// <summary>
    /// Stage1Group이 꺼질 때 애니메이션과 페이드 상태를 정리합니다.
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
        _isChangingMap = false;
        SetFadeAlpha(0f);
        PlayMoranState(_idleStateName, 1f);
    }

    /// <summary>
    /// 플레이어 입력에 따라 Moran을 좌우로 이동시키고 맵 경계 전환을 판정합니다.
    /// </summary>
    private void Update()
    {
        if (_isChangingMap || Object_Moran == null)
        {
            PlayMoranState(_idleStateName, 1f);
            return;
        }

        int direction = 0;

        if (Input.GetKey(_moveLeftKey))
            direction -= 1;

        if (Input.GetKey(_moveRightKey))
            direction += 1;

        if (direction == 0)
        {
            PlayMoranState(_idleStateName, 1f);
            return;
        }

        bool isRunning = Input.GetKey(_runKey);
        MoveMoran(direction, isRunning);
    }

    private void ResolveReferences()
    {
        Object_StageMap1 = Object_StageMap1 != null ? Object_StageMap1 : FindChildByName(transform, _stageMap1Name);
        Object_StageMap2 = Object_StageMap2 != null ? Object_StageMap2 : FindChildByName(transform, _stageMap2Name);
        Object_Moran = Object_Moran != null ? Object_Moran : FindChildByName(transform, _moranObjectName);

        if (Object_Moran != null)
        {
            Renderer_Moran = Renderer_Moran != null ? Renderer_Moran : Object_Moran.GetComponentInChildren<SpriteRenderer>(true);
            Animator_Moran = Animator_Moran != null ? Animator_Moran : Object_Moran.GetComponentInChildren<Animator>(true);
        }

        HUD_Road = HUD_Road != null ? HUD_Road : GetComponent<OOTechRoadHUDController>();
        Camera_Main = Camera_Main != null ? Camera_Main : Camera.main;
        ResolveStageNameText();
        CreateFadeCanvasIfNeeded();
    }

    private void ResolveStageNameText()
    {
        if (Text_StageName != null)
            return;

        GameObject stageNameObject = FindChildByName(transform, _stageNameObjectName);

        if (stageNameObject == null)
            stageNameObject = FindSceneObjectByName(_stageNameObjectName);

        Text_StageName = stageNameObject != null ? stageNameObject.GetComponentInChildren<TextMeshProUGUI>(true) : null;

        if (Text_StageName != null)
            Text_StageName.gameObject.SetActive(false);
    }

    private void DisablePlaceholderCanvas()
    {
        GameObject placeholderCanvas = FindChildByName(transform, _placeholderCanvasName);

        if (placeholderCanvas != null)
            placeholderCanvas.SetActive(false);
    }

    private void PrepareHUDMission()
    {
        if (HUD_Road == null)
            return;

        HUD_Road.SetOwnerGroupName(_currentGroupName);
        HUD_Road.PrepareHUD();
        HUD_Road.SetCookingUnlocked(true);
        HUD_Road.SetHUDVisible(true);

        OO_StageQuest questData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStageQuestData(_stageQuestDataId) : null;
        string questText = questData != null && !string.IsNullOrEmpty(questData.Description) ? questData.Description : "이 마을의 촌장을 만나세요";
        HUD_Road.RequestSetStageQuestMission(questText);
    }

    private void PrepareStage()
    {
        _currentMapIndex = 0;
        SetCurrentMapActive();
        SetMoranActive(true);
        PlaceMoranAtMapEntry(false);
        PlayMoranState(_idleStateName, 1f);
        SetFadeAlpha(0f);
    }

    private void MoveMoran(int direction, bool isRunning)
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (mapRenderer == null)
            return;

        float speed = isRunning ? _runSpeed : _walkSpeed;
        Vector3 position = Object_Moran.transform.position;
        position.x += direction * speed * Time.deltaTime;
        position.y = CalculateLaneY(mapRenderer);

        float leftX = mapRenderer.bounds.min.x + _leftStartMargin;
        float rightX = mapRenderer.bounds.max.x - _edgeExitMargin;

        if (_currentMapIndex == 0)
            position.x = Mathf.Max(position.x, leftX);

        Object_Moran.transform.position = position;

        if (Renderer_Moran != null)
            Renderer_Moran.flipX = direction < 0;

        PlayMoranState(isRunning ? _runningStateName : _walkingStateName, isRunning ? _runningAnimationSpeed : _walkingAnimationSpeed);

        if (_currentMapIndex == 0 && position.x >= rightX)
            StartCoroutine(ChangeMapRoutine(1));
        else if (_currentMapIndex == 1 && position.x <= mapRenderer.bounds.min.x + _edgeExitMargin)
            StartCoroutine(ChangeMapRoutine(0));
    }

    private IEnumerator ChangeMapRoutine(int nextMapIndex)
    {
        if (_isChangingMap)
            yield break;

        _isChangingMap = true;
        PlayMoranState(_idleStateName, 1f);

        yield return FadeRoutine(0f, 1f, _fadeOutSeconds);

        if (_blackHoldSeconds > 0f)
            yield return new WaitForSeconds(_blackHoldSeconds);

        _currentMapIndex = nextMapIndex;
        SetCurrentMapActive();
        PlaceMoranAtMapEntry(nextMapIndex == 0);

        yield return FadeRoutine(1f, 0f, _fadeInSeconds);
        _isChangingMap = false;
    }

    private void SetCurrentMapActive()
    {
        if (Object_StageMap1 != null)
            Object_StageMap1.SetActive(_currentMapIndex == 0);

        if (Object_StageMap2 != null)
            Object_StageMap2.SetActive(_currentMapIndex == 1);
    }

    private void PlaceMoranAtMapEntry(bool isFromLeftEdge)
    {
        SpriteRenderer mapRenderer = GetCurrentMapRenderer();

        if (Object_Moran == null || mapRenderer == null)
            return;

        Vector3 position = Object_Moran.transform.position;
        position.x = isFromLeftEdge ? mapRenderer.bounds.max.x - _leftStartMargin : mapRenderer.bounds.min.x + _leftStartMargin;
        position.y = CalculateLaneY(mapRenderer);
        Object_Moran.transform.position = position;
    }

    private float CalculateLaneY(SpriteRenderer mapRenderer)
    {
        return Mathf.Lerp(mapRenderer.bounds.min.y, mapRenderer.bounds.max.y, Mathf.Clamp01(_moranLaneNormalizedHeight));
    }

    private SpriteRenderer GetCurrentMapRenderer()
    {
        GameObject mapObject = _currentMapIndex == 0 ? Object_StageMap1 : Object_StageMap2;
        return mapObject != null ? mapObject.GetComponentInChildren<SpriteRenderer>(true) : null;
    }

    private void SetMoranActive(bool isActive)
    {
        if (Object_Moran != null)
            Object_Moran.SetActive(isActive);
    }

    private void PlayMoranState(string stateName, float speed)
    {
        if (Animator_Moran == null || string.IsNullOrEmpty(stateName))
            return;

        Animator_Moran.speed = speed;

        if (_currentAnimationStateName == stateName)
            return;

        _currentAnimationStateName = stateName;
        Animator_Moran.Play(stateName, 0, 0f);
    }

    private IEnumerator PlayStageNameRoutine()
    {
        if (Text_StageName == null)
            yield break;

        OO_Stage stageData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetStageData(_stageDataId) : null;
        Text_StageName.text = stageData != null && !string.IsNullOrEmpty(stageData.Name) ? stageData.Name : "동쪽 마을";
        Text_StageName.gameObject.SetActive(true);

        yield return FadeTextRoutine(0f, 1f, _stageTitleFadeSeconds);
        yield return new WaitForSeconds(_stageTitleHoldSeconds);
        yield return FadeTextRoutine(1f, 0f, _stageTitleFadeSeconds);

        Text_StageName.gameObject.SetActive(false);
    }

    private IEnumerator FadeTextRoutine(float fromAlpha, float toAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color color = Text_StageName.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(fromAlpha, toAlpha, elapsedTime / Mathf.Max(0.01f, duration));
            Text_StageName.color = color;
            yield return null;
        }

        color.a = toAlpha;
        Text_StageName.color = color;
    }

    private void CreateFadeCanvasIfNeeded()
    {
        if (Canvas_Fade != null)
            return;

        GameObject canvasObject = new GameObject("Canvas_Stage1Fade");
        canvasObject.transform.SetParent(transform, false);
        Canvas_Fade = canvasObject.AddComponent<Canvas>();
        Canvas_Fade.renderMode = RenderMode.ScreenSpaceOverlay;
        Canvas_Fade.overrideSorting = true;
        Canvas_Fade.sortingOrder = 5000;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("Image_Fade");
        imageObject.transform.SetParent(canvasObject.transform, false);
        Image_Fade = imageObject.AddComponent<Image>();
        Image_Fade.color = Color.black;
        RectTransform imageRect = imageObject.transform as RectTransform;
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;
        SetFadeAlpha(0f);
    }

    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            SetFadeAlpha(Mathf.Lerp(fromAlpha, toAlpha, elapsedTime / Mathf.Max(0.01f, duration)));
            yield return null;
        }

        SetFadeAlpha(toAlpha);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (Image_Fade == null)
            return;

        Color color = Image_Fade.color;
        color.a = alpha;
        Image_Fade.color = color;
        Image_Fade.raycastTarget = alpha > 0.01f;
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
        if (rootTransform == null)
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
