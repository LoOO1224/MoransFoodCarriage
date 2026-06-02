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
        if (Root_Canvas != null)
            return;

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
