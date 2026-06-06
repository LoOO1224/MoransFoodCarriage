// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechStagePlaceholderController.cs
// - 역할: 로드맵, 월드맵, 스테이지 전환 흐름을 담당하는 장면 Controller입니다.
// - 감독 관점: 길 위의 장면 전환 큐시트를 들고 있는 무대감독입니다.
// - 유지보수 포인트: 배경/버튼/캐릭터 배치는 오브젝트와 View가 맡고, 이 스크립트는 순서 지휘만 맡아야 합니다.
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 각 StageGroup의 세부 퀘스트가 들어오기 전까지 쓰는 임시 무대 컨트롤러입니다.
/// 흰 배경 세트, 공용 HUD, 다음 RoadGroup으로 넘어가는 버튼 큐를 관리합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechStagePlaceholderController : MonoBehaviour
{
    [Header("Group Transition")]
    [SerializeField] private string _currentGroupName;
    [SerializeField] private string _nextGroupName;
    [SerializeField] private string _buttonText = "넘어가기";

    [Header("View")]
    [SerializeField] private int _sortingOrder = 900;
    [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);

    private GameObject Root_Canvas;
    private Button Button_Next;
    private TextMeshProUGUI Text_Button;
    private OOTechRoadHUDController HUD_Shared;

    /// <summary>
    /// 에디터 보수 스크립트가 Stage 번호에 맞춰 현재/다음 그룹 이름을 세팅할 때 사용합니다.
    /// </summary>
    public void Configure(string currentGroupName, string nextGroupName, string buttonText)
    {
        _currentGroupName = currentGroupName;
        _nextGroupName = nextGroupName;

        if (!string.IsNullOrEmpty(buttonText))
            _buttonText = buttonText;

        if (Text_Button != null)
            Text_Button.text = _buttonText;
    }

    /// <summary>
    /// StageGroup이 켜지면 임시 무대 UI와 HUD를 준비합니다.
    /// </summary>
    private void OnEnable()
    {
        NormalizeButtonTextIfNeeded();
        PrepareStageBackground();
        PrepareView();
        PrepareSharedHUD();
        BindButton();
    }

    /// <summary>
    /// StageGroup이 꺼질 때 버튼 이벤트와 HUD 표시를 정리합니다.
    /// </summary>
    private void OnDisable()
    {
        UnbindButton();

        if (HUD_Shared != null)
            HUD_Shared.SetHUDVisible(false);
    }

    /// <summary>
    /// 씬에 배치된 Canvas_StagePlaceholder에서 버튼 소품을 찾아 연결합니다.
    /// </summary>
    private void PrepareView()
    {
        if (Root_Canvas == null)
            Root_Canvas = FindChildByName(transform, "Canvas_StagePlaceholder");

        if (Root_Canvas == null)
        {
            Debug.LogWarning($"[OOTechStagePlaceholderController] {gameObject.name} needs Canvas_StagePlaceholder as a child object.");
            return;
        }

        Canvas canvas = Root_Canvas.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = _sortingOrder;
        }

        GameObject buttonObject = FindChildByName(Root_Canvas.transform, "Button_NextStage");
        Button_Next = buttonObject != null ? buttonObject.GetComponent<Button>() : null;
        Text_Button = buttonObject != null ? buttonObject.GetComponentInChildren<TextMeshProUGUI>(true) : null;

        if (Text_Button != null)
            Text_Button.text = _buttonText;
    }

    /// <summary>
    /// StageGroup이 켜질 때 실제 배경막을 먼저 올리고, 흰색 임시 배경막은 내립니다.
    /// 영화 무대로 보면 Stage2Background는 실제 세트이고 Image_WhiteBackground는 임시 리허설 천막입니다.
    /// </summary>
    private void PrepareStageBackground()
    {
        GameObject stageBackgroundObject = FindStageBackgroundObject();

        if (stageBackgroundObject == null)
            return;

        stageBackgroundObject.SetActive(true);
        NormalizeStageBackgroundView(stageBackgroundObject);
        DisablePlaceholderWhiteBackground();

        Debug.Log($"[OOTechStagePlaceholderController] Stage background enabled: {gameObject.name} -> {stageBackgroundObject.name}");
    }

    /// <summary>
    /// 현재 StageGroup 이름에 맞는 배경 오브젝트를 찾습니다.
    /// 예: Stage2Group 무대에서는 Stage2Background 배우를 찾습니다.
    /// </summary>
    private GameObject FindStageBackgroundObject()
    {
        if (HasRenderableStageBackground(gameObject))
            return gameObject;

        string groupName = string.IsNullOrEmpty(_currentGroupName) ? gameObject.name : _currentGroupName;
        string expectedBackgroundName = groupName.Replace("Group", "Background");
        GameObject stageBackgroundObject = FindChildByName(transform, expectedBackgroundName);

        if (stageBackgroundObject != null)
            return stageBackgroundObject;

        string typoSafeBackgroundName = groupName.Replace("Group", "Backound");
        stageBackgroundObject = FindChildByName(transform, typoSafeBackgroundName);

        if (stageBackgroundObject != null)
            return stageBackgroundObject;

        return FindFirstRealBackgroundChild(transform);
    }

    /// <summary>
    /// Stage3Group처럼 그룹 루트 자체에 배경 Image를 붙인 경우도 실제 배경 배우로 인정합니다.
    /// 감독이 무대 벽 자체에 그림을 붙여둔 상황이므로, 별도 자식 소품이 없어도 배경으로 사용합니다.
    /// </summary>
    private bool HasRenderableStageBackground(GameObject targetObject)
    {
        if (targetObject == null)
            return false;

        Image backgroundImage = targetObject.GetComponent<Image>();

        if (backgroundImage != null && backgroundImage.sprite != null)
            return true;

        SpriteRenderer backgroundRenderer = targetObject.GetComponent<SpriteRenderer>();
        return backgroundRenderer != null && backgroundRenderer.sprite != null;
    }

    /// <summary>
    /// 이름이 조금 달라도 Background/Backound가 붙은 실제 배경 자식을 찾습니다.
    /// Image_WhiteBackground는 임시막이라 실제 배경으로 취급하지 않습니다.
    /// </summary>
    private GameObject FindFirstRealBackgroundChild(Transform rootTransform)
    {
        if (rootTransform == null)
            return null;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform childTransform = rootTransform.GetChild(index);

            if (childTransform == null)
                continue;

            string childName = childTransform.name;
            bool isBackgroundName = childName.Contains("Background") || childName.Contains("Backound");
            bool isPlaceholderWhiteBackground = childName == "Image_WhiteBackground";

            if (isBackgroundName && !isPlaceholderWhiteBackground)
                return childTransform.gameObject;

            GameObject foundObject = FindFirstRealBackgroundChild(childTransform);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    /// <summary>
    /// Stage 배경 UI가 Game View 전체에 맞도록 Canvas와 RectTransform을 정리합니다.
    /// 감독이 화면비를 바꿔도 배경막이 1920x1080 기준으로 무대 뒤를 꽉 채우게 하는 안전장치입니다.
    /// </summary>
    private void NormalizeStageBackgroundView(GameObject stageBackgroundObject)
    {
        SpriteRenderer backgroundRenderer = stageBackgroundObject.GetComponent<SpriteRenderer>();

        if (backgroundRenderer != null)
        {
            NormalizeStageSpriteBackgroundView(stageBackgroundObject, backgroundRenderer);
            return;
        }

        RectTransform rectTransform = stageBackgroundObject.GetComponent<RectTransform>();
        Image backgroundImage = stageBackgroundObject.GetComponent<Image>();

        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        Canvas canvas = stageBackgroundObject.GetComponent<Canvas>();

        if (canvas == null && backgroundImage != null)
            canvas = stageBackgroundObject.AddComponent<Canvas>();

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = _sortingOrder - 100;
        }

        CanvasScaler canvasScaler = stageBackgroundObject.GetComponent<CanvasScaler>();

        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = _referenceResolution;
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        if (backgroundImage != null)
            backgroundImage.raycastTarget = false;
    }

    /// <summary>
    /// SpriteRenderer로 만든 Stage 배경을 1920x1080 월드 무대에 맞춥니다.
    /// 영화로 치면 UI 천막이 아니라 실제 배경 세트라서, 무대 중앙에 놓고 화면 크기만큼 키워야 합니다.
    /// </summary>
    private void NormalizeStageSpriteBackgroundView(GameObject stageBackgroundObject, SpriteRenderer backgroundRenderer)
    {
        Canvas canvas = stageBackgroundObject.GetComponent<Canvas>();

        if (canvas != null)
            canvas.enabled = false;

        CanvasScaler canvasScaler = stageBackgroundObject.GetComponent<CanvasScaler>();

        if (canvasScaler != null)
            canvasScaler.enabled = false;

        Image backgroundImage = stageBackgroundObject.GetComponent<Image>();

        if (backgroundImage != null)
            backgroundImage.enabled = false;

        Transform backgroundTransform = stageBackgroundObject.transform;
        const float targetWidth = 1920f;
        const float targetHeight = 1080f;

        backgroundTransform.localPosition = new Vector3(targetWidth * 0.5f, targetHeight * 0.5f, 0f);
        backgroundTransform.localRotation = Quaternion.identity;

        if (backgroundRenderer.sprite != null)
        {
            Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;
            float scaleX = targetWidth / Mathf.Max(0.01f, spriteSize.x);
            float scaleY = targetHeight / Mathf.Max(0.01f, spriteSize.y);

            backgroundTransform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        backgroundRenderer.enabled = true;
        backgroundRenderer.sortingLayerName = "Background";
        backgroundRenderer.sortingOrder = -1;

        Color color = backgroundRenderer.color;

        if (color.a <= 0.01f)
        {
            color.a = 1f;
            backgroundRenderer.color = color;
        }
    }

    /// <summary>
    /// 실제 Stage 배경이 있을 때는 흰색 임시 배경을 꺼서 배경 이미지를 가리지 않게 합니다.
    /// </summary>
    private void DisablePlaceholderWhiteBackground()
    {
        if (Root_Canvas == null)
            Root_Canvas = FindChildByName(transform, "Canvas_StagePlaceholder");

        if (Root_Canvas == null)
            return;

        GameObject whiteBackgroundObject = FindChildByName(Root_Canvas.transform, "Image_WhiteBackground");

        if (whiteBackgroundObject != null)
            whiteBackgroundObject.SetActive(false);
    }

    /// <summary>
    /// 버튼 텍스트가 깨진 상태라면 기본 한글 텍스트로 복구합니다.
    /// </summary>
    private void NormalizeButtonTextIfNeeded()
    {
        if (string.IsNullOrWhiteSpace(_buttonText) || _buttonText.Contains("?"))
            _buttonText = "넘어가기";
    }

    /// <summary>
    /// 넘어가기 버튼을 다음 그룹 이동 큐에 연결합니다.
    /// </summary>
    private void BindButton()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(OnNextButtonClicked);
        Button_Next.onClick.AddListener(OnNextButtonClicked);
    }

    /// <summary>
    /// StageGroup에서도 인벤토리/임무 확인이 가능하도록 공용 HUD를 켭니다.
    /// </summary>
    private void PrepareSharedHUD()
    {
        if (HUD_Shared == null)
            HUD_Shared = GetComponent<OOTechRoadHUDController>();

        if (HUD_Shared == null)
        {
            Debug.LogWarning($"[OOTechStagePlaceholderController] {gameObject.name} needs OOTechRoadHUDController attached in the scene.");
            return;
        }

        HUD_Shared.SetOwnerGroupName(_currentGroupName);
        HUD_Shared.PrepareHUD();
        HUD_Shared.SetCookingUnlocked(true);
        HUD_Shared.SetHUDVisible(true);
    }

    private void UnbindButton()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(OnNextButtonClicked);
    }

    /// <summary>
    /// 버튼 클릭 시 현재 StageGroup을 닫고 다음 RoadGroup 또는 EpilogueGroup을 엽니다.
    /// </summary>
    private void OnNextButtonClicked()
    {
        RequestSwitchSceneGroup(_currentGroupName, _nextGroupName);
    }

    /// <summary>
    /// UIManager 등록 상태를 우선 사용하고, 실패하면 씬 오브젝트 활성화로 그룹을 전환합니다.
    /// </summary>
    private bool RequestSwitchSceneGroup(string closingGroupName, string openingGroupName)
    {
        GameObject closingGroupObject = FindSceneObjectByName(closingGroupName);
        GameObject openingGroupObject = FindSceneObjectByName(openingGroupName);

        if (OOTechUIManager.Inst != null)
        {
            if (closingGroupObject != null)
                OOTechUIManager.Inst.RegisterUI(closingGroupName, closingGroupObject);

            if (openingGroupObject != null)
                OOTechUIManager.Inst.RegisterUI(openingGroupName, openingGroupObject);

            OOTechUIManager.Inst.CloseUI(closingGroupName);

            if (OOTechUIManager.Inst.OpenUI(openingGroupName))
                return true;
        }

        if (closingGroupObject != null)
            closingGroupObject.SetActive(false);

        if (openingGroupObject == null)
        {
            Debug.LogWarning($"[OOTechStagePlaceholderController] Next group not found: {openingGroupName}");
            return false;
        }

        openingGroupObject.SetActive(true);
        return true;
    }

    private GameObject FindSceneObjectByName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return null;

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
