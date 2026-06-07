// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: MainMenuController.cs
// - 역할: MainMenuGroup의 버튼 입력을 받아 다음 무대를 여는 시작 화면 Controller입니다.
// - 영화 비유: 첫 상영관 로비에서 관객이 본편, 도감, 종료, 개발 리허설 장면을 고르는 안내 데스크입니다.
// - 유지보수 포인트: 실제 화면 전환은 UIManager에 요청하고, 이 클래스는 버튼 큐만 전달합니다.
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// MainMenuGroup의 버튼 이벤트를 담당합니다.
/// Game View에서는 플레이어가 처음 만나는 메뉴 버튼과 개발자용 빠른 진입 버튼을 제공합니다.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Codex BGM")]
    [SerializeField] private AudioClip _codexBGM;

    [Header("Developer Skip")]
    [SerializeField] private bool _isShowDeveloperSkipButtons = false;
    [SerializeField] private string _developerRoad1GroupName = "1st_Road_to_Stage1";
    [SerializeField] private string _developerStage1GroupName = "Stage1Group";
    [SerializeField] private string _developerRoad2GroupName = "2nd_Road_to_Stage2";
    [SerializeField] private Vector2 _developerButtonStartPosition = new Vector2(32f, -32f);
    [SerializeField] private Vector2 _developerButtonSize = new Vector2(280f, 54f);
    [SerializeField] private float _developerButtonSpacing = 12f;

    private RectTransform Root_DeveloperSkipPanel;

    private readonly string[] _exclusiveGroupNameArray =
    {
        "MainMenuGroup",
        "CodexGroup",
        "Prologue1Group",
        "Prologue2Group",
        "Tutorial1Group",
        "Senario1Group",
        "WorldMapGroup",
        "1st_Road_to_Stage1",
        "2nd_Road_to_Stage2",
        "3rd_Road_to_Stage3",
        "4th_Road_to_Stage4",
        "Final_Road_to_FinalStage",
        "CookingGroup",
        "Stage1Group",
        "Stage2Group",
        "Stage3Group",
        "Stage4_1Group",
        "Stage4Group",
        "FinalStageGroup",
        "EpilogueGroup",
        "DialogueGroup",
        "TutorialGuideGroup"
    };

    /// <summary>
    /// 메뉴가 준비될 때 개발자용 리허설 버튼도 같이 준비합니다.
    /// 감독 비유로는 본 공연 버튼 옆에 특정 장면으로 바로 가는 리허설 큐 버튼을 붙이는 단계입니다.
    /// </summary>
    private void Awake()
    {
        PrepareDeveloperSkipButtons();
    }

    /// <summary>
    /// MainMenuGroup이 다시 켜질 때 개발자 버튼 표시 상태를 유지합니다.
    /// </summary>
    private void OnEnable()
    {
        if (Root_DeveloperSkipPanel != null)
            Root_DeveloperSkipPanel.gameObject.SetActive(_isShowDeveloperSkipButtons);
    }

    // ==================== 기본 메뉴 버튼 ====================

    /// <summary>
    /// 시작 버튼을 누르면 Prologue1Group으로 진행합니다.
    /// </summary>
    public void OnStartButtonClicked()
    {
        UIManagerExtension.OnStartButtonClicked();
    }

    /// <summary>
    /// 도감 버튼을 누르면 CodexGroup을 열고 도감 BGM이 있으면 재생합니다.
    /// </summary>
    public void OnCodexButtonClicked()
    {
        UIManagerExtension.OnCodexButtonClicked();

        if (OOTechSoundManager.Inst != null && _codexBGM != null)
            OOTechSoundManager.Inst.PlayBGM(_codexBGM);
    }

    /// <summary>
    /// 종료 버튼을 누르면 게임 종료 요청을 전달합니다.
    /// </summary>
    public void OnExitButtonClicked()
    {
        UIManagerExtension.OnExitButtonClicked();
    }

    // ==================== 개발자용 바로가기 버튼 ====================

    /// <summary>
    /// 개발 테스트용 바로가기 버튼 3개를 MainMenuGroup 안에 준비합니다.
    /// Game View에서는 빨강, 주황, 노랑 버튼으로 로드1, 스테이지1, 로드2에 바로 들어갑니다.
    /// </summary>
    private void PrepareDeveloperSkipButtons()
    {
        if (!_isShowDeveloperSkipButtons)
            return;

        Canvas canvas = GetComponentInChildren<Canvas>(true);

        if (canvas == null)
            canvas = CreateDeveloperSkipCanvas();

        Root_DeveloperSkipPanel = FindOrCreateDeveloperSkipRoot(canvas.transform);
        ClearDeveloperButtonListenerArray();

        CreateDeveloperSkipButton("Button_DevSkip_Road1", "DEV Road 1", new Color(0.85f, 0.06f, 0.06f, 0.92f), 0, delegate
        {
            RequestDeveloperSkipToGroup(_developerRoad1GroupName);
        });

        CreateDeveloperSkipButton("Button_DevSkip_Stage1", "DEV Stage 1", new Color(1f, 0.45f, 0.02f, 0.92f), 1, delegate
        {
            RequestDeveloperSkipToGroup(_developerStage1GroupName);
        });

        CreateDeveloperSkipButton("Button_DevSkip_Road2", "DEV Road 2", new Color(1f, 0.9f, 0.05f, 0.92f), 2, delegate
        {
            RequestDeveloperSkipToGroup(_developerRoad2GroupName);
        });

        Root_DeveloperSkipPanel.gameObject.SetActive(true);
    }

    private Canvas CreateDeveloperSkipCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas_MainMenuDeveloperSkip");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 20000;

        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private RectTransform FindOrCreateDeveloperSkipRoot(Transform canvasTransform)
    {
        Transform rootTransform = canvasTransform.Find("Panel_DeveloperSkipButtons");

        if (rootTransform != null)
            return rootTransform as RectTransform;

        GameObject rootObject = new GameObject("Panel_DeveloperSkipButtons");
        rootObject.transform.SetParent(canvasTransform, false);

        RectTransform rootRect = rootObject.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.anchoredPosition = _developerButtonStartPosition;
        rootRect.sizeDelta = new Vector2(_developerButtonSize.x, (_developerButtonSize.y + _developerButtonSpacing) * 3f);
        return rootRect;
    }

    private void ClearDeveloperButtonListenerArray()
    {
        if (Root_DeveloperSkipPanel == null)
            return;

        Button[] buttonArray = Root_DeveloperSkipPanel.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttonArray)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }
    }

    private void CreateDeveloperSkipButton(string objectName, string labelText, Color color, int index, UnityAction clickAction)
    {
        Transform buttonTransform = Root_DeveloperSkipPanel.Find(objectName);
        GameObject buttonObject = buttonTransform != null ? buttonTransform.gameObject : new GameObject(objectName);
        buttonObject.transform.SetParent(Root_DeveloperSkipPanel, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();

        if (buttonRect == null)
            buttonRect = buttonObject.AddComponent<RectTransform>();

        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(0f, 1f);
        buttonRect.pivot = new Vector2(0f, 1f);
        buttonRect.anchoredPosition = new Vector2(0f, -index * (_developerButtonSize.y + _developerButtonSpacing));
        buttonRect.sizeDelta = _developerButtonSize;

        Image buttonImage = buttonObject.GetComponent<Image>();

        if (buttonImage == null)
            buttonImage = buttonObject.AddComponent<Image>();

        buttonImage.color = color;

        Button button = buttonObject.GetComponent<Button>();

        if (button == null)
            button = buttonObject.AddComponent<Button>();

        button.targetGraphic = buttonImage;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(clickAction);

        TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);

        if (label == null)
            label = CreateDeveloperButtonLabel(buttonObject.transform);

        label.text = labelText;
    }

    private TextMeshProUGUI CreateDeveloperButtonLabel(Transform parentTransform)
    {
        GameObject labelObject = new GameObject("Text_Label");
        labelObject.transform.SetParent(parentTransform, false);

        RectTransform labelRect = labelObject.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 26f;
        label.color = Color.black;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    /// <summary>
    /// 개발 리허설 버튼이 눌리면 현재 그룹들을 닫고 목표 그룹만 켭니다.
    /// MainMenuGroup에서 Prologue를 건너뛰고 바로 특정 무대로 진입할 때 사용합니다.
    /// </summary>
    private void RequestDeveloperSkipToGroup(string targetGroupName)
    {
        if (OOTechUIManager.Inst == null)
        {
            Debug.LogError("[MainMenuController] Developer skip failed. OOTechUIManager is missing.");
            return;
        }

        foreach (string groupName in _exclusiveGroupNameArray)
        {
            if (groupName == targetGroupName)
                continue;

            OOTechUIManager.Inst.CloseUI(groupName);
        }

        OOTechUIManager.Inst.OpenUI(targetGroupName);
        Debug.Log($"[MainMenuController] Developer skip opened: {targetGroupName}");
    }
}
