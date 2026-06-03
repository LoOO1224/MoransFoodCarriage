using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum OOTechRoadHUDButtonKind
{
    Inventory,
    Codex,
    Mission,
    Cooking,
    WorldMap
}

/// <summary>
/// RoadGroup과 StageGroup이 공유하는 HUD를 지휘합니다.
/// HUDUIGroup이라는 실제 씬 소품은 사용자가 편집하고, 이 컨트롤러는 버튼/패널/NEW 배지의 큐만 처리합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechRoadHUDController : MonoBehaviour
{
    [Header("Group Names")]
    [SerializeField] private string _ownerGroupName;
    [SerializeField] private string _worldMapGroupName = "WorldMapGroup";
    [SerializeField] private string _cookingGroupName = "CookingGroup";
    [SerializeField] private string _codexGroupName = "CodexGroup";
    [SerializeField] private string _mainMenuGroupName = "MainMenuGroup";

    [Header("Inventory")]
    [SerializeField] private bool _isPrepareDefaultInventoryItem = false;
    [SerializeField] private string _defaultInventoryItemId = "Ing_Rice_01";
    [SerializeField] private int _defaultInventoryItemCount = 1;

    [Header("Canvas")]
    [SerializeField] private int _sortingOrder = 1200;
    [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private bool _isApplyRuntimeSupportPanelLayout = false;
    [SerializeField] private bool _isUseSharedHUD = true;
    [SerializeField] private string _sharedHUDSystemGroupName = "HUDSystemGroup";
    [SerializeField] private string _sharedHUDGroupName = "HUDUIGroup";
    [SerializeField] private bool _isMoveLegacyMissionPanelToBottomRight = true;
    [SerializeField] private Vector2 _missionPanelBottomRightPosition = new Vector2(-36f, 132f);
    [SerializeField] private Vector2 _missionPanelSize = new Vector2(560f, 220f);

    [Header("Overlay View")]
    [SerializeField] private int _overlaySpriteSortingOrder = 3000;

    [Header("New Badge")]
    [SerializeField] private float _newBadgeBlinkSpeed = 7f;
    [SerializeField] private float _newBadgeMinimumAlpha = 0.25f;

    [Header("Mission Effect")]
    [SerializeField] private float _missionCompleteHoldDuration = 0.25f;
    [SerializeField] private float _missionCompleteFadeDuration = 0.85f;

    private GameObject Root_HUD;
    private RectTransform Rect_HUD;
    private Canvas Canvas_HUD;
    private OOTechRoadHUDView View_HUD;
    private GameObject Root_BottomBar;
    private GameObject Root_InventoryPanel;
    private GameObject Root_MissionPanel;
    private GameObject Root_GuideOverlay;
    private GameObject Root_ConfirmPopup;
    private RectTransform Rect_InventoryContent;

    private Button Button_MainMenu;
    private Button Button_Inventory;
    private Button Button_Codex;
    private Button Button_Mission;
    private Button Button_Cooking;
    private Button Button_WorldMap;

    private TextMeshProUGUI Text_InventoryContent;
    private TextMeshProUGUI Text_MissionContent;
    private TextMeshProUGUI Text_InventoryNewBadge;
    private TextMeshProUGUI Text_CodexNewBadge;
    private TextMeshProUGUI Text_MissionNewBadge;
    private TextMeshProUGUI Text_CookingLabel;
    private TextMeshProUGUI Text_GuideTitle;
    private TextMeshProUGUI Text_GuideBody;
    private TextMeshProUGUI Text_MainMenuConfirmMessage;
    private Image Image_CookingButton;
    private GameObject Slot_InventoryItemTemplate;
    private RectTransform Rect_FocusArrow;
    private RectTransform Rect_GuideTextPanel;
    private Button Button_GuideNext;
    private Button Button_MainMenuConfirmYes;
    private Button Button_MainMenuConfirmNo;
    private OOTechCookingGroupController Controller_CookingOverlay;
    private UnityAction _guideClickAction;

    private bool _isDefaultInventoryPrepared;
    private bool _isCookingUnlocked;
    private bool _isOverlayOpen;
    private bool _isCookingOverlayOpen;
    private bool _isCookingQuestActive;
    private bool _isCookingQuestComplete;
    private bool _isCookingMissionRemoved;
    private bool _isEastRoadMissionComplete;
    private bool _isEastRoadMissionRemoved;
    private string _stageQuestMissionText;
    private float _missionCompleteEffectAlpha = 1f;
    private Coroutine _inventoryNewBadgeCoroutine;
    private Coroutine _codexNewBadgeCoroutine;
    private Coroutine _missionNewBadgeCoroutine;
    private Coroutine _missionCompleteEffectCoroutine;

    public bool IsCookingUnlocked => _isCookingUnlocked;
    public bool IsOverlayOpen => _isOverlayOpen;

    /// <summary>
    /// 이 HUD가 어느 RoadGroup 또는 StageGroup의 소속인지 기록합니다.
    /// 월드맵/부엌에서 돌아갈 때 바로 전 무대로 복귀하기 위한 이름표입니다.
    /// </summary>
    public void SetOwnerGroupName(string ownerGroupName)
    {
        _ownerGroupName = ownerGroupName;
    }

    /// <summary>
    /// HUDUIGroup의 버튼과 패널을 연결하고 시작 표시 상태를 정리합니다.
    /// Game View에서는 하단 HUD가 켜지고 인벤토리/임무 패널은 닫힌 상태가 됩니다.
    /// </summary>
    public void PrepareHUD()
    {
        if (string.IsNullOrEmpty(_ownerGroupName))
            _ownerGroupName = gameObject.name;

        PrepareDefaultInventoryItem();
        CreateHUDCanvasIfNeeded();
        SetHUDVisible(true);
        ApplyBottomHUDLayout();
        BindHUDViewReferences();
        ApplyMissionPanelBottomRightLayoutIfNeeded();
        BindHUDButtonEvents();
        RefreshInventoryView();
        RefreshMissionText();
        SetInventoryPanelActive(false);
        SetMissionPanelActive(false);
        SetInventoryNewBadgeActive(false);
        SetCodexNewBadgeActive(false);
        SetMissionNewBadgeActive(false);
        RefreshCookingButtonView();
        SetBottomHUDActive(true);
    }

    /// <summary>
    /// HUD 전체를 보이거나 숨깁니다.
    /// 월드맵에서는 HUD를 끄고, 부엌에서는 필요한 패널만 다시 살립니다.
    /// </summary>
    public void SetHUDVisible(bool isVisible)
    {
        if (isVisible)
            RequestActivateParentChain(Root_HUD);

        if (Root_HUD != null)
            Root_HUD.SetActive(isVisible);
    }

    private void RequestActivateParentChain(GameObject targetObject)
    {
        if (targetObject == null)
            return;

        Transform parentTransform = targetObject.transform.parent;

        while (parentTransform != null)
        {
            if (!parentTransform.gameObject.activeSelf)
                parentTransform.gameObject.SetActive(true);

            parentTransform = parentTransform.parent;
        }
    }

    /// <summary>
    /// 요리하기 버튼의 잠금 상태를 갱신합니다.
    /// 1st_Road_to_Stage1의 첫 맵에서는 잠겨 있다가 RoadMap1 이후 열립니다.
    /// </summary>
    public void SetCookingUnlocked(bool isUnlocked)
    {
        _isCookingUnlocked = isUnlocked;
        RefreshCookingButtonView();
    }

    /// <summary>
    /// 인벤토리에 새 물건이 들어왔다는 NEW 배지를 켜거나 끕니다.
    /// </summary>
    public void SetInventoryNewBadgeActive(bool isActive)
    {
        SetBadgeActive(Text_InventoryNewBadge, isActive, ref _inventoryNewBadgeCoroutine);
    }

    /// <summary>
    /// 도감 갱신을 알리는 NEW 배지를 켜거나 끕니다.
    /// </summary>
    public void SetCodexNewBadgeActive(bool isActive)
    {
        SetBadgeActive(Text_CodexNewBadge, isActive, ref _codexNewBadgeCoroutine);
    }

    /// <summary>
    /// 임무 갱신을 알리는 NEW 배지를 켜거나 끕니다.
    /// </summary>
    public void SetMissionNewBadgeActive(bool isActive)
    {
        SetBadgeActive(Text_MissionNewBadge, isActive, ref _missionNewBadgeCoroutine);
    }

    /// <summary>
    /// 요리 임무가 발급되면 임무판에 해야 할 일을 올리고 NEW 표시를 켭니다.
    /// </summary>
    public void RequestSetCookingQuestActive()
    {
        _isCookingQuestActive = true;
        _isCookingQuestComplete = false;
        _isCookingMissionRemoved = false;
        _missionCompleteEffectAlpha = 1f;
        StopMissionCompleteEffect();
        RefreshMissionText();
        SetMissionNewBadgeActive(true);
    }

    /// <summary>
    /// 채소죽 제작이 끝나면 첫 번째 임무 줄에 취소선과 페이드아웃 연출을 재생합니다.
    /// </summary>
    public void RequestCompleteCookingQuest()
    {
        _isCookingQuestActive = true;
        _isCookingQuestComplete = true;

        if (isActiveAndEnabled)
        {
            StopMissionCompleteEffect();
            _missionCompleteEffectCoroutine = StartCoroutine(PlayMissionCompleteEffectRoutine());
        }
        else
        {
            _isCookingMissionRemoved = true;
            RefreshMissionText();
        }

        SetMissionNewBadgeActive(true);
    }

    /// <summary>
    /// Stage1Group에 도착했을 때 Road 임무를 완료 처리하고 새 StageQuest 문구를 임무판에 표시합니다.
    /// </summary>
    public void RequestSetStageQuestMission(string stageQuestText)
    {
        PrepareHUD();
        _isEastRoadMissionComplete = true;
        _isEastRoadMissionRemoved = true;
        _stageQuestMissionText = stageQuestText;
        SetMissionNewBadgeActive(true);
        RefreshMissionText();
    }

    /// <summary>
    /// 현재 플레이어 인벤토리 모델을 다시 읽어 HUD 슬롯을 갱신합니다.
    /// </summary>
    public void RequestRefreshInventoryView()
    {
        RefreshInventoryView();
    }

    /// <summary>
    /// 도감처럼 다른 뒤로가기 로직으로 돌아온 경우에도 HUD와 이동 잠금을 복구합니다.
    /// Game View에서는 숨었던 HUD가 다시 켜지고 MFC 이동 입력이 다시 살아납니다.
    /// </summary>
    public void RequestRestoreFromOverlayReturn()
    {
        _isOverlayOpen = false;

        if (_isCookingOverlayOpen)
            CloseCookingSupportHUD();
        else
            Controller_CookingOverlay = null;

        CreateHUDCanvasIfNeeded();
        ApplyBottomHUDLayout();
        BindHUDViewReferences();
        ApplyMissionPanelBottomRightLayoutIfNeeded();
        BindHUDButtonEvents();
        RefreshInventoryView();
        RefreshMissionText();
        RefreshCookingButtonView();
        SetBottomHUDActive(true);
        CloseHUDGuide();
        HideMainMenuConfirmPopup();
        SetInventoryPanelActive(false);
        SetMissionPanelActive(false);
        SetHUDVisible(true);
    }

    /// <summary>
    /// 외부 뒤로가기 버튼이 오버레이를 닫았는지 감지해 HUD 잠금을 자동으로 풉니다.
    /// </summary>
    private void Update()
    {
        UpdateHUDGuideClickInput();
        RecoverExternalOverlayCloseIfNeeded();
    }

    /// <summary>
    /// 특정 HUD 버튼을 화살표로 가리키는 튜토리얼 가이드를 표시합니다.
    /// Game View에서는 버튼 위에 작은 설명 패널이 뜹니다.
    /// </summary>
    public void ShowHUDGuideStep(OOTechRoadHUDButtonKind buttonKind, string title, string description, UnityAction onNext)
    {
        CreateGuideOverlayIfNeeded();

        if (Root_GuideOverlay == null)
        {
            onNext?.Invoke();
            return;
        }

        RectTransform targetRect = GetButtonRect(buttonKind);
        Vector2 targetLocalPosition = GetTargetLocalPosition(targetRect);

        Root_GuideOverlay.SetActive(true);
        ApplyGuideOverlayLayout(targetLocalPosition, title, description, onNext);
    }

    /// <summary>
    /// 특정 버튼이 아니라 로드 화면 중앙에 짧은 안내 메시지를 띄웁니다.
    /// </summary>
    public void ShowRoadMessage(string title, string description, UnityAction onNext)
    {
        CreateGuideOverlayIfNeeded();

        if (Root_GuideOverlay == null)
        {
            onNext?.Invoke();
            return;
        }

        Root_GuideOverlay.SetActive(true);
        ApplyGuideOverlayLayout(new Vector2(0f, 260f), title, description, onNext);
    }

    /// <summary>
    /// HUD 포커스 가이드를 닫습니다.
    /// </summary>
    public void CloseHUDGuide()
    {
        _guideClickAction = null;

        if (Root_GuideOverlay != null)
            Root_GuideOverlay.SetActive(false);
    }

    /// <summary>
    /// 필요하다면 기본 아이템을 인벤토리에 한 번만 지급합니다.
    /// 튜토리얼 보상 방식과 충돌하지 않도록 인스펙터 플래그가 켜진 경우에만 동작합니다.
    /// </summary>
    private void PrepareDefaultInventoryItem()
    {
        if (!_isPrepareDefaultInventoryItem || _isDefaultInventoryPrepared)
            return;

        if (OOTechGameManager.Inst == null)
            return;

        List<OOTechItemModel> itemList = OOTechGameManager.Inst.GetPlayerItemList();

        foreach (OOTechItemModel item in itemList)
        {
            if (item != null && item.ItemDataId == _defaultInventoryItemId)
            {
                _isDefaultInventoryPrepared = true;
                return;
            }
        }

        OOTechGameManager.Inst.AddItem(_defaultInventoryItemId, Mathf.Max(1, _defaultInventoryItemCount));
        _isDefaultInventoryPrepared = true;
    }

    /// <summary>
    /// 씬에 배치된 HUDUIGroup을 찾고 Canvas 설정을 1920x1080 기준으로 정리합니다.
    /// 이 메서드는 새 HUD를 만들지 않고, 준비된 소품이 없으면 에러를 남깁니다.
    /// </summary>
    private void CreateHUDCanvasIfNeeded()
    {
        if (Root_HUD != null)
            return;

        View_HUD = _isUseSharedHUD ? ResolveSharedHUDView() : null;

        if (View_HUD == null)
            View_HUD = GetComponentInChildren<OOTechRoadHUDView>(true);

        if (View_HUD != null)
            Root_HUD = View_HUD.gameObject;

        if (Root_HUD == null)
            Root_HUD = FindChildByName(transform, "HUDUIGroup");

        if (Root_HUD == null)
            Root_HUD = FindChildByName(transform, "RoadHUDCanvas");

        if (Root_HUD == null)
        {
            Debug.LogError($"[OOTechRoadHUDController] HUDUIGroup is missing on {gameObject.name}. Create it from Tools/OO MFC/Create Editable HUDUIGroup before play.");
            return;
        }

        Rect_HUD = Root_HUD.transform as RectTransform;

        if (Rect_HUD == null)
        {
            Debug.LogError($"[OOTechRoadHUDController] HUDUIGroup needs RectTransform: {Root_HUD.name}");
            return;
        }

        Canvas_HUD = Root_HUD.GetComponent<Canvas>();

        if (Canvas_HUD == null)
        {
            Debug.LogError($"[OOTechRoadHUDController] HUDUIGroup needs Canvas: {Root_HUD.name}");
            return;
        }

        Canvas_HUD.renderMode = RenderMode.ScreenSpaceOverlay;
        Canvas_HUD.overrideSorting = true;
        Canvas_HUD.sortingOrder = _sortingOrder;

        GraphicRaycaster graphicRaycaster = Root_HUD.GetComponent<GraphicRaycaster>();

        if (graphicRaycaster == null)
            graphicRaycaster = Root_HUD.AddComponent<GraphicRaycaster>();

        graphicRaycaster.enabled = true;

        CanvasGroup canvasGroup = Root_HUD.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = Root_HUD.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        CanvasScaler canvasScaler = Root_HUD.GetComponent<CanvasScaler>();

        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = _referenceResolution;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        if (View_HUD == null)
            View_HUD = Root_HUD.GetComponent<OOTechRoadHUDView>();

        if (View_HUD == null)
        {
            Debug.LogError($"[OOTechRoadHUDController] HUDUIGroup needs OOTechRoadHUDView: {Root_HUD.name}");
            return;
        }

        View_HUD.ResolveReferences();
        BindHUDViewReferences();
        DeactivateLocalHUDViewArrayExceptRoot();
    }

    /// <summary>
    /// OOTechRoadHUDView가 들고 있는 실제 버튼/패널 참조를 컨트롤러 필드에 연결합니다.
    /// </summary>
    private void BindHUDViewReferences()
    {
        if (View_HUD == null)
            return;

        View_HUD.ResolveReferences();
        Root_BottomBar = View_HUD.BottomBar;
        Root_InventoryPanel = View_HUD.InventoryPanel;
        Root_MissionPanel = View_HUD.MissionPanel;

        Button_MainMenu = View_HUD.MainMenuButton;
        Button_Inventory = View_HUD.InventoryButton;
        Button_Codex = View_HUD.CodexButton;
        Button_Mission = View_HUD.MissionButton;
        Button_Cooking = View_HUD.CookingButton;
        Button_WorldMap = View_HUD.WorldMapButton;

        Text_MissionContent = View_HUD.MissionContentText;
        Text_InventoryNewBadge = View_HUD.InventoryNewBadgeText;
        Text_CodexNewBadge = View_HUD.CodexNewBadgeText;
        Text_MissionNewBadge = View_HUD.MissionNewBadgeText;
        Text_CookingLabel = View_HUD.CookingLabelText;
        Image_CookingButton = View_HUD.CookingButtonImage;
        Rect_InventoryContent = View_HUD.InventoryContentRect;
        Slot_InventoryItemTemplate = View_HUD.InventorySlotTemplate;

        Root_GuideOverlay = View_HUD.GuideOverlay;
        Rect_FocusArrow = View_HUD.FocusArrowRect;
        Rect_GuideTextPanel = View_HUD.GuideTextPanelRect;
        Text_GuideTitle = View_HUD.GuideTitleText;
        Text_GuideBody = View_HUD.GuideBodyText;
        Button_GuideNext = View_HUD.GuideNextButton;
        DisableLegacyGuideNextButtonObject();

        Root_ConfirmPopup = View_HUD.MainMenuConfirmPopup;
        Text_MainMenuConfirmMessage = View_HUD.MainMenuConfirmMessageText;
        Button_MainMenuConfirmYes = View_HUD.MainMenuConfirmYesButton;
        Button_MainMenuConfirmNo = View_HUD.MainMenuConfirmNoButton;
    }

    /// <summary>
    /// HUD 버튼 클릭 이벤트를 각각의 기능 큐에 연결합니다.
    /// </summary>
    private void BindHUDButtonEvents()
    {
        BindButtonEvent(Button_MainMenu, OnMainMenuButtonClicked);
        BindButtonEvent(Button_Inventory, OnInventoryButtonClicked);
        BindButtonEvent(Button_Codex, OnCodexButtonClicked);
        BindButtonEvent(Button_Mission, OnMissionButtonClicked);
        BindButtonEvent(Button_Cooking, OnCookingButtonClicked);
        BindButtonEvent(Button_WorldMap, OnWorldMapButtonClicked);
    }

    private void BindButtonEvent(Button button, UnityAction clickAction)
    {
        if (button == null || clickAction == null)
            return;

        PrepareButtonForClick(button);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(clickAction);
    }

    private void RecoverExternalOverlayCloseIfNeeded()
    {
        if (!_isOverlayOpen)
            return;

        if (IsSceneGroupActive(_worldMapGroupName) || IsSceneGroupActive(_cookingGroupName) || IsSceneGroupActive(_codexGroupName))
            return;

        RequestRestoreFromOverlayReturn();
    }

    /// <summary>
    /// 공유 HUD가 있으면 로컬 중복 HUD가 클릭을 가로막지 않도록 현재 컨트롤러의 HUD를 공유 HUD로 연결합니다.
    /// </summary>
    private OOTechRoadHUDView ResolveSharedHUDView()
    {
        GameObject sharedSystemObject = FindSceneObjectByName(_sharedHUDSystemGroupName);
        GameObject sharedHUDObject = sharedSystemObject != null ? FindChildByName(sharedSystemObject.transform, _sharedHUDGroupName) : null;

        if (sharedHUDObject == null)
            sharedHUDObject = FindSceneObjectByName("GlobalHUDUIGroup");

        if (sharedHUDObject == null)
            return null;

        return sharedHUDObject.GetComponent<OOTechRoadHUDView>();
    }

    /// <summary>
    /// 공용 HUD를 쓰는 동안 그룹 자식으로 남아 있는 예전 HUD가 버튼 클릭을 먹지 않도록 비활성화합니다.
    /// </summary>
    private void DeactivateLocalHUDViewArrayExceptRoot()
    {
        OOTechRoadHUDView[] localViewArray = GetComponentsInChildren<OOTechRoadHUDView>(true);

        foreach (OOTechRoadHUDView localView in localViewArray)
        {
            if (localView == null || localView.gameObject == Root_HUD)
                continue;

            localView.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 버튼이 보이는데 클릭이 안 되는 상황을 막기 위해 Raycast와 interactable 상태를 복구합니다.
    /// </summary>
    private void PrepareButtonForClick(Button button)
    {
        button.interactable = true;

        Graphic targetGraphic = button.targetGraphic;

        if (targetGraphic == null)
        {
            targetGraphic = button.GetComponent<Graphic>();
            button.targetGraphic = targetGraphic;
        }

        if (targetGraphic != null)
        {
            targetGraphic.raycastTarget = true;
            targetGraphic.enabled = true;
        }

        TextMeshProUGUI[] childTextArray = button.GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI childText in childTextArray)
            childText.raycastTarget = false;
    }

    /// <summary>
    /// HUD를 1920x1080 게임 화면 위의 Overlay로 고정합니다.
    /// 로드맵 이미지는 무대 세트이고, HUD는 플레이어가 누르는 객석 쪽 조작판입니다.
    /// </summary>
    private void ApplyBottomHUDLayout()
    {
        if (Canvas_HUD != null)
        {
            Canvas_HUD.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas_HUD.overrideSorting = true;
            Canvas_HUD.sortingOrder = _sortingOrder;
        }

        if (Root_HUD != null)
            Root_HUD.transform.SetAsLastSibling();
    }

    /// <summary>
    /// 플레이어 인벤토리 모델을 읽어 슬롯 목록을 다시 그립니다.
    /// 정적 템플릿 슬롯만 복제하며, HUD 뼈대 자체는 씬 오브젝트를 유지합니다.
    /// </summary>
    private void RefreshInventoryView()
    {
        if (Rect_InventoryContent == null)
            return;

        ClearInventorySlotRows();

        Text_InventoryContent = null;
        List<OOTechItemModel> itemList = OOTechGameManager.Inst != null ? OOTechGameManager.Inst.GetPlayerItemList() : new List<OOTechItemModel>();

        if (itemList.Count == 0)
        {
            CreateInventoryTextRow("비어 있음");
            return;
        }

        bool hasVisibleItem = false;

        foreach (OOTechItemModel item in itemList)
        {
            if (item == null || item.ItemStackCount <= 0)
                continue;

            hasVisibleItem = true;
            CreateInventoryItemSlot(item);
        }

        if (!hasVisibleItem)
            CreateInventoryTextRow("비어 있음");
    }

    /// <summary>
    /// 인벤토리가 비어 있을 때 안내 문구 슬롯을 표시합니다.
    /// </summary>
    private void CreateInventoryTextRow(string text)
    {
        GameObject slotObject = CreateInventorySlotRow("Slot_InventoryEmpty");

        if (slotObject == null)
            return;

        Text_InventoryContent = slotObject.GetComponentInChildren<TextMeshProUGUI>(true);

        if (Text_InventoryContent != null)
        {
            Text_InventoryContent.text = text;
            Text_InventoryContent.alignment = TextAlignmentOptions.TopLeft;
        }
    }

    /// <summary>
    /// 실제 아이템 모델 하나를 인벤토리 슬롯으로 표시합니다.
    /// 부엌이 열려 있으면 이 슬롯에 드래그 기능도 연결됩니다.
    /// </summary>
    private void CreateInventoryItemSlot(OOTechItemModel item)
    {
        GameObject slotObject = CreateInventorySlotRow("Slot_" + item.ItemDataId);

        if (slotObject == null)
            return;

        string itemName = GetIngredientDisplayName(item.ItemDataId);
        Sprite itemIconSprite = GetItemIconSprite(item.ItemDataId);
        OOTechInventorySlotView slotView = slotObject.GetComponent<OOTechInventorySlotView>();

        if (slotView != null)
            slotView.RequestSetupItem(item.ItemDataId, $"{itemName} x{item.ItemStackCount}", itemIconSprite);

        if (Controller_CookingOverlay == null || !Controller_CookingOverlay.gameObject.activeInHierarchy)
            return;

        OOTechCookingIngredientDragItem dragItem = slotObject.GetComponent<OOTechCookingIngredientDragItem>();

        if (dragItem == null)
        {
            Debug.LogWarning("[OOTechRoadHUDController] Slot template needs OOTechCookingIngredientDragItem for cooking drag-and-drop.");
            return;
        }

        dragItem.Setup(Controller_CookingOverlay, item.ItemDataId, itemName, item.ItemStackCount);
    }

    /// <summary>
    /// 템플릿을 제외한 기존 인벤토리 슬롯 행을 제거합니다.
    /// </summary>
    private void ClearInventorySlotRows()
    {
        if (Rect_InventoryContent == null)
            return;

        for (int index = Rect_InventoryContent.childCount - 1; index >= 0; index--)
        {
            Transform childTransform = Rect_InventoryContent.GetChild(index);

            if (Slot_InventoryItemTemplate != null && childTransform.gameObject == Slot_InventoryItemTemplate)
                continue;

            Destroy(childTransform.gameObject);
        }
    }

    /// <summary>
    /// Slot_InventoryItemTemplate을 복제해 표시용 슬롯 한 칸을 만듭니다.
    /// 반복 목록이라서 여기만 런타임 복제를 허용합니다.
    /// </summary>
    private GameObject CreateInventorySlotRow(string objectName)
    {
        if (Slot_InventoryItemTemplate == null)
        {
            Debug.LogError("[OOTechRoadHUDController] Slot_InventoryItemTemplate is missing from HUDUIGroup.");
            return null;
        }

        GameObject slotObject = Instantiate(Slot_InventoryItemTemplate, Rect_InventoryContent, false);
        slotObject.name = objectName;
        slotObject.SetActive(true);
        return slotObject;
    }

    /// <summary>
    /// 요리하기 버튼의 표시 상태를 현재 잠금 상태에 맞춥니다.
    /// </summary>
    private void RefreshCookingButtonView()
    {
        if (Button_Cooking != null)
            Button_Cooking.interactable = true;

        if (Image_CookingButton != null)
            Image_CookingButton.color = _isCookingUnlocked ? new Color(0.22f, 0.22f, 0.22f, 0.96f) : new Color(0.14f, 0.14f, 0.14f, 0.88f);

        if (Text_CookingLabel != null)
            Text_CookingLabel.text = _isCookingUnlocked ? "요리하기" : "요리하기";
    }

    /// <summary>
    /// 인벤토리 버튼을 누르면 인벤토리 패널을 토글하고 NEW 배지를 확인 처리합니다.
    /// </summary>
    private void OnInventoryButtonClicked()
    {
        RefreshInventoryView();
        SetInventoryPanelActive(Root_InventoryPanel != null && !Root_InventoryPanel.activeSelf);
        SetMissionPanelActive(false);
        SetInventoryNewBadgeActive(false);
    }

    /// <summary>
    /// 도감 버튼을 누르면 CodexGroup으로 이동합니다.
    /// </summary>
    private void OnCodexButtonClicked()
    {
        SetCodexNewBadgeActive(false);
        RequestOpenSceneGroup(_codexGroupName);
    }

    /// <summary>
    /// 임무 버튼을 누르면 임무 패널을 토글하고 NEW 배지를 확인 처리합니다.
    /// </summary>
    private void OnMissionButtonClicked()
    {
        RefreshMissionText();
        SetMissionPanelActive(Root_MissionPanel != null && !Root_MissionPanel.activeSelf);
        SetInventoryPanelActive(false);
        SetMissionNewBadgeActive(false);
    }

    /// <summary>
    /// 요리하기 버튼을 누르면 CookingGroup으로 이동합니다.
    /// 잠겨 있으면 아직 열리지 않았다는 안내만 보여줍니다.
    /// </summary>
    private void OnCookingButtonClicked()
    {
        if (!_isCookingUnlocked)
        {
            ShowRoadMessage("요리하기", "첫 번째 길을 지나면 요리하기가 열립니다.", null);
            return;
        }

        RequestOpenSceneGroup(_cookingGroupName);
    }

    /// <summary>
    /// 월드맵 버튼을 누르면 WorldMapGroup을 엽니다.
    /// </summary>
    private void OnWorldMapButtonClicked()
    {
        RequestOpenSceneGroup(_worldMapGroupName);
    }

    /// <summary>
    /// 메인 메뉴 버튼을 누르면 확인 팝업을 띄웁니다.
    /// </summary>
    private void OnMainMenuButtonClicked()
    {
        ShowMainMenuConfirmPopup();
    }

    private void SetInventoryPanelActive(bool isActive)
    {
        if (Root_InventoryPanel != null)
            Root_InventoryPanel.SetActive(isActive);
    }

    private void SetMissionPanelActive(bool isActive)
    {
        ApplyMissionPanelBottomRightLayoutIfNeeded();
        RefreshMissionText();

        if (Root_MissionPanel != null)
            Root_MissionPanel.SetActive(isActive);
    }

    /// <summary>
    /// 예전 중앙 기본 배치로 남아 있는 임무판만 우측 하단으로 옮깁니다.
    /// 감독이 직접 잡은 커스텀 배치는 건드리지 않습니다.
    /// </summary>
    private void ApplyMissionPanelBottomRightLayoutIfNeeded()
    {
        if (!_isMoveLegacyMissionPanelToBottomRight || Root_MissionPanel == null)
            return;

        RectTransform missionRect = Root_MissionPanel.transform as RectTransform;

        if (missionRect == null || !IsLegacyCenteredMissionPanelLayout(missionRect))
            return;

        ApplyMissionPanelBottomRightLayout(missionRect);
    }

    private bool IsLegacyCenteredMissionPanelLayout(RectTransform missionRect)
    {
        if (missionRect == null)
            return false;

        bool isCenterBottomAnchor = Mathf.Abs(missionRect.anchorMin.x - 0.5f) <= 0.01f
            && Mathf.Abs(missionRect.anchorMax.x - 0.5f) <= 0.01f
            && Mathf.Abs(missionRect.anchorMin.y) <= 0.01f
            && Mathf.Abs(missionRect.anchorMax.y) <= 0.01f;
        bool isCenterPosition = Mathf.Abs(missionRect.anchoredPosition.x) <= 2f;
        return isCenterBottomAnchor && isCenterPosition;
    }

    private void ApplyMissionPanelBottomRightLayout(RectTransform missionRect)
    {
        if (missionRect == null)
            return;

        missionRect.anchorMin = new Vector2(1f, 0f);
        missionRect.anchorMax = new Vector2(1f, 0f);
        missionRect.pivot = new Vector2(1f, 0f);
        missionRect.anchoredPosition = _missionPanelBottomRightPosition;
        missionRect.sizeDelta = _missionPanelSize;
    }

    /// <summary>
    /// 임무판 텍스트를 현재 진행 상태에 맞춰 다시 씁니다.
    /// </summary>
    private void RefreshMissionText()
    {
        if (Text_MissionContent == null)
            return;

        Text_MissionContent.richText = true;
        string cookingMissionText = CreateCookingMissionText();

        string eastRoadMissionText = _isEastRoadMissionRemoved ? string.Empty : "○ 동쪽의 마을로 가시오\n";
        string stageQuestText = string.IsNullOrWhiteSpace(_stageQuestMissionText) ? string.Empty : "○ " + _stageQuestMissionText + "\n";
        Text_MissionContent.text = "현재 임무\n" + cookingMissionText + eastRoadMissionText + stageQuestText;
    }

    /// <summary>
    /// 요리 임무 줄의 현재 표시 상태를 만듭니다.
    /// 완료 연출 중에는 취소선과 알파값으로 첫 번째 줄만 점점 사라지게 합니다.
    /// </summary>
    private string CreateCookingMissionText()
    {
        if (!_isCookingQuestActive || _isCookingMissionRemoved)
            return string.Empty;

        string missionText = "○ 배고픈 모란과 동료들을 위해 요리하세요.";

        if (!_isCookingQuestComplete)
            return missionText + "\n";

        string alphaHex = Mathf.Clamp(Mathf.RoundToInt(_missionCompleteEffectAlpha * 255f), 0, 255).ToString("X2");
        return $"<color=#FFFFFF{alphaHex}><s>{missionText}</s></color>\n";
    }

    /// <summary>
    /// 완료된 임무 줄을 잠깐 보여준 뒤 천천히 지우고, 남은 임무가 위로 올라오게 합니다.
    /// </summary>
    private IEnumerator PlayMissionCompleteEffectRoutine()
    {
        _isCookingMissionRemoved = false;
        _missionCompleteEffectAlpha = 1f;
        RefreshMissionText();

        if (_missionCompleteHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(_missionCompleteHoldDuration);

        float elapsedTime = 0f;
        float duration = Mathf.Max(0.05f, _missionCompleteFadeDuration);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            _missionCompleteEffectAlpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            RefreshMissionText();
            yield return null;
        }

        _missionCompleteEffectAlpha = 1f;
        _isCookingMissionRemoved = true;
        _missionCompleteEffectCoroutine = null;
        RefreshMissionText();
    }

    private void StopMissionCompleteEffect()
    {
        if (_missionCompleteEffectCoroutine == null)
            return;

        StopCoroutine(_missionCompleteEffectCoroutine);
        _missionCompleteEffectCoroutine = null;
    }

    /// <summary>
    /// NEW 배지를 켜고 끄며, 켜질 때는 깜빡이는 코루틴을 시작합니다.
    /// </summary>
    private void SetBadgeActive(TextMeshProUGUI badgeText, bool isActive, ref Coroutine badgeCoroutine)
    {
        if (badgeText == null)
            return;

        if (badgeCoroutine != null)
        {
            StopCoroutine(badgeCoroutine);
            badgeCoroutine = null;
        }

        badgeText.gameObject.SetActive(isActive);

        Color badgeColor = badgeText.color;
        badgeColor.a = 1f;
        badgeText.color = badgeColor;

        if (isActive)
            badgeCoroutine = StartCoroutine(BlinkNewBadgeRoutine(badgeText));
    }

    /// <summary>
    /// NEW 글자의 알파값을 흔들어 플레이어 눈에 띄게 만듭니다.
    /// </summary>
    private IEnumerator BlinkNewBadgeRoutine(TextMeshProUGUI badgeText)
    {
        while (badgeText != null && badgeText.gameObject.activeSelf)
        {
            Color badgeColor = badgeText.color;
            float wave = (Mathf.Sin(Time.unscaledTime * _newBadgeBlinkSpeed) + 1f) * 0.5f;
            badgeColor.a = Mathf.Lerp(_newBadgeMinimumAlpha, 1f, wave);
            badgeText.color = badgeColor;
            yield return null;
        }
    }

    /// <summary>
    /// 데이터 매니저에서 재료/요리 이름을 찾고, 없을 때는 튜토리얼용 기본 이름을 반환합니다.
    /// </summary>
    private string GetIngredientDisplayName(string ingredientDataId)
    {
        if (OOTechItemCatalogManager.Inst != null)
            return OOTechItemCatalogManager.Inst.GetItemDisplayName(ingredientDataId);

        if (!string.IsNullOrEmpty(ingredientDataId) && OOTechGameDataManager.Inst != null)
        {
            OO_Ingredient ingredientData = OOTechGameDataManager.Inst.GetIngredientData(ingredientDataId);

            if (ingredientData != null && !string.IsNullOrEmpty(ingredientData.Name))
                return ingredientData.Name;

            OO_Cook cookData = OOTechGameDataManager.Inst.GetCookData(ingredientDataId);

            if (cookData != null && !string.IsNullOrEmpty(cookData.Name))
                return cookData.Name;
        }

        if (ingredientDataId == _defaultInventoryItemId)
            return "쌀";

        if (ingredientDataId == "Ing_Veggie_01")
            return "채소";

        if (ingredientDataId == "Ing_Pumpkin_01")
            return "호박";

        if (ingredientDataId == "OO_VegetableSoup_1")
            return "채소죽";

        return string.IsNullOrEmpty(ingredientDataId) ? "알 수 없는 아이템" : ingredientDataId;
    }

    /// <summary>
    /// 아이템 카탈로그에서 인벤토리 슬롯에 표시할 Sprite 아이콘을 가져옵니다.
    /// </summary>
    private Sprite GetItemIconSprite(string itemDataId)
    {
        if (OOTechItemCatalogManager.Inst == null)
            return null;

        return OOTechItemCatalogManager.Inst.GetItemIconSprite(itemDataId);
    }

    /// <summary>
    /// 월드맵, 부엌, 도감 같은 다른 그룹을 열고 이전 그룹 이름을 기록합니다.
    /// </summary>
    private bool RequestOpenSceneGroup(string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
            return false;

        GameObject groupObject = FindSceneObjectByName(groupName);
        string previousGroupName = GetOwnerGroupName();
        OOTechGroupNavigationHistory.SetPreviousGroup(groupName, previousGroupName);

        PrepareOverlayController(groupName, groupObject);

        if (OOTechUIManager.Inst != null && groupObject != null)
        {
            OOTechUIManager.Inst.RegisterUI(groupName, groupObject);

            if (OOTechUIManager.Inst.OpenUI(groupName))
            {
                PrepareOverlayReturnButton(groupObject, groupName, previousGroupName);
                PrepareOverlayVisualPriority(groupObject, groupName);
                HandleOverlayOpened(groupName);
                return true;
            }
        }

        if (OOTechUIManager.Inst != null && OOTechUIManager.Inst.OpenUI(groupName))
        {
            GameObject createdGroup = OOTechUIManager.Inst.GetCreatedUI(groupName);
            PrepareOverlayController(groupName, createdGroup);
            PrepareOverlayReturnButton(createdGroup, groupName, previousGroupName);
            PrepareOverlayVisualPriority(createdGroup, groupName);
            HandleOverlayOpened(groupName);
            return true;
        }

        if (groupObject == null)
        {
            Debug.LogWarning($"[OOTechRoadHUDController] Scene group not found: {groupName}");
            return false;
        }

        groupObject.SetActive(true);
        PrepareOverlayReturnButton(groupObject, groupName, previousGroupName);
        PrepareOverlayVisualPriority(groupObject, groupName);
        HandleOverlayOpened(groupName);
        return true;
    }

    private string GetOwnerGroupName()
    {
        if (!string.IsNullOrEmpty(_ownerGroupName))
            return _ownerGroupName;

        return gameObject.name;
    }

    private void PrepareOverlayController(string groupName, GameObject groupObject)
    {
        if (groupObject == null)
            return;

        if (groupName == _worldMapGroupName && groupObject.GetComponent<OOTechWorldMapOverlayController>() == null)
            Debug.LogWarning($"[OOTechRoadHUDController] {groupName} needs OOTechWorldMapOverlayController on the group object.");

        if (groupName == _cookingGroupName)
        {
            Controller_CookingOverlay = groupObject.GetComponent<OOTechCookingGroupController>();

            if (Controller_CookingOverlay == null)
                Debug.LogWarning($"[OOTechRoadHUDController] {groupName} needs OOTechCookingGroupController on the group object.");
        }
    }

    /// <summary>
    /// 월드맵/부엌 무대가 열릴 때 이전 로드 배경막보다 앞에 보이도록 SpriteRenderer 정렬값을 보정합니다.
    /// </summary>
    private void PrepareOverlayVisualPriority(GameObject groupObject, string groupName)
    {
        if (groupObject == null)
            return;

        if (groupName != _worldMapGroupName && groupName != _cookingGroupName)
            return;

        SpriteRenderer[] rendererArray = groupObject.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer spriteRenderer in rendererArray)
        {
            if (spriteRenderer == null)
                continue;

            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = Mathf.Max(spriteRenderer.sortingOrder, _overlaySpriteSortingOrder);

            Color color = spriteRenderer.color;

            if (color.a <= 0.01f)
            {
                color.a = 1f;
                spriteRenderer.color = color;
            }
        }
    }

    /// <summary>
    /// 오버레이 그룹이 열렸을 때 HUD 표시 규칙을 적용합니다.
    /// 월드맵은 HUD를 끄고, 부엌은 인벤토리/임무 패널을 살립니다.
    /// </summary>
    private void HandleOverlayOpened(string groupName)
    {
        _isOverlayOpen = true;

        if (groupName == _cookingGroupName)
        {
            OpenCookingSupportHUD();
            return;
        }

        CloseCookingSupportHUD();
        SetInventoryPanelActive(false);
        SetMissionPanelActive(false);
        SetHUDVisible(false);
    }

    /// <summary>
    /// 오버레이의 돌아가기 버튼을 누르면 직전 그룹으로 복귀합니다.
    /// </summary>
    private void HandleOverlayReturnClicked(string currentGroupName, string previousGroupName)
    {
        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.CloseUI(currentGroupName);
            OOTechUIManager.Inst.OpenUI(previousGroupName);
        }
        else
        {
            GameObject currentGroup = FindSceneObjectByName(currentGroupName);
            GameObject previousGroup = FindSceneObjectByName(previousGroupName);

            if (currentGroup != null)
                currentGroup.SetActive(false);

            if (previousGroup != null)
                previousGroup.SetActive(true);
        }

        _isOverlayOpen = false;
        CloseCookingSupportHUD();

        if (previousGroupName == GetOwnerGroupName())
            SetHUDVisible(true);
    }

    /// <summary>
    /// 부엌에서는 하단 버튼은 숨기고 인벤토리/임무 패널만 요리 보조 HUD로 유지합니다.
    /// </summary>
    private void OpenCookingSupportHUD()
    {
        _isCookingOverlayOpen = true;
        SetHUDVisible(true);
        CloseHUDGuide();
        HideMainMenuConfirmPopup();
        ApplyCookingSupportPanelLayout();

        if (Canvas_HUD != null)
            Canvas_HUD.sortingOrder = _sortingOrder + 600;

        SetBottomHUDActive(false);

        RefreshInventoryView();
        RefreshMissionText();
        SetInventoryPanelActive(true);
        SetMissionPanelActive(true);
    }

    /// <summary>
    /// 부엌을 닫으면 HUD를 원래 로드 화면 상태로 되돌립니다.
    /// </summary>
    private void CloseCookingSupportHUD()
    {
        if (!_isCookingOverlayOpen)
            return;

        _isCookingOverlayOpen = false;
        Controller_CookingOverlay = null;

        if (Canvas_HUD != null)
            Canvas_HUD.sortingOrder = _sortingOrder;

        ApplyDefaultSupportPanelLayout();

        SetBottomHUDActive(true);

        SetInventoryPanelActive(false);
        SetMissionPanelActive(false);
        RefreshInventoryView();
        RefreshMissionText();
    }

    private void ApplyCookingSupportPanelLayout()
    {
        if (!_isApplyRuntimeSupportPanelLayout)
            return;

        RectTransform inventoryRect = Root_InventoryPanel != null ? Root_InventoryPanel.transform as RectTransform : null;
        RectTransform missionRect = Root_MissionPanel != null ? Root_MissionPanel.transform as RectTransform : null;

        if (inventoryRect != null)
        {
            inventoryRect.anchorMin = new Vector2(0f, 0f);
            inventoryRect.anchorMax = new Vector2(0f, 0f);
            inventoryRect.pivot = new Vector2(0f, 0f);
            inventoryRect.anchoredPosition = new Vector2(28f, 170f);
            inventoryRect.sizeDelta = new Vector2(460f, 330f);
        }

        if (missionRect != null)
        {
            missionRect.anchorMin = new Vector2(1f, 0f);
            missionRect.anchorMax = new Vector2(1f, 0f);
            missionRect.pivot = new Vector2(1f, 0f);
            missionRect.anchoredPosition = _missionPanelBottomRightPosition;
            missionRect.sizeDelta = _missionPanelSize;
        }
    }

    /// <summary>
    /// 부엌 지원 모드에서 하단 HUD 버튼과 NEW 배지를 숨기거나 복구합니다.
    /// </summary>
    private void SetBottomHUDActive(bool isActive)
    {
        if (Root_BottomBar != null)
            Root_BottomBar.SetActive(isActive);

        SetButtonActive(Button_MainMenu, isActive);
        SetButtonActive(Button_Inventory, isActive);
        SetButtonActive(Button_Codex, isActive);
        SetButtonActive(Button_Mission, isActive);
        SetButtonActive(Button_Cooking, isActive);
        SetButtonActive(Button_WorldMap, isActive);

        if (Text_InventoryNewBadge != null)
            Text_InventoryNewBadge.gameObject.SetActive(isActive && _inventoryNewBadgeCoroutine != null);

        if (Text_CodexNewBadge != null)
            Text_CodexNewBadge.gameObject.SetActive(isActive && _codexNewBadgeCoroutine != null);

        if (Text_MissionNewBadge != null)
            Text_MissionNewBadge.gameObject.SetActive(isActive && _missionNewBadgeCoroutine != null);
    }

    private void SetButtonActive(Button button, bool isActive)
    {
        if (button != null)
            button.gameObject.SetActive(isActive);
    }

    private void ApplyDefaultSupportPanelLayout()
    {
        if (!_isApplyRuntimeSupportPanelLayout)
            return;

        RectTransform inventoryRect = Root_InventoryPanel != null ? Root_InventoryPanel.transform as RectTransform : null;
        RectTransform missionRect = Root_MissionPanel != null ? Root_MissionPanel.transform as RectTransform : null;

        if (inventoryRect != null)
        {
            inventoryRect.anchorMin = new Vector2(0f, 0f);
            inventoryRect.anchorMax = new Vector2(0f, 0f);
            inventoryRect.pivot = new Vector2(0f, 0f);
            inventoryRect.anchoredPosition = new Vector2(28f, 170f);
            inventoryRect.sizeDelta = new Vector2(460f, 330f);
        }

        if (missionRect != null)
        {
            missionRect.anchorMin = new Vector2(1f, 0f);
            missionRect.anchorMax = new Vector2(1f, 0f);
            missionRect.pivot = new Vector2(1f, 0f);
            missionRect.anchoredPosition = _missionPanelBottomRightPosition;
            missionRect.sizeDelta = _missionPanelSize;
        }
    }

    /// <summary>
    /// 월드맵/부엌/도감에 배치된 Button_RuntimeReturn을 현재 복귀 목적지에 연결합니다.
    /// </summary>
    private void PrepareOverlayReturnButton(GameObject groupObject, string currentGroupName, string previousGroupName)
    {
        if (groupObject == null)
            return;

        BackButtonController[] backButtonArray = groupObject.GetComponentsInChildren<BackButtonController>(true);

        foreach (BackButtonController backButton in backButtonArray)
            backButton.SetPreviousGroup(previousGroupName);

        GameObject existingReturnButtonObject = FindChildByName(groupObject.transform, "Button_RuntimeReturn");

        if (existingReturnButtonObject != null)
        {
            Button existingReturnButton = existingReturnButtonObject.GetComponent<Button>();
            ConfigureOverlayReturnButton(existingReturnButton, currentGroupName, previousGroupName);
            return;
        }

        Debug.LogWarning($"[OOTechRoadHUDController] Button_RuntimeReturn is missing under {groupObject.name}. Create the button as a scene object so the director can edit it.");
    }

    private void ConfigureOverlayReturnButton(Button returnButton, string currentGroupName, string previousGroupName)
    {
        if (returnButton == null)
            return;

        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(delegate
        {
            HandleOverlayReturnClicked(currentGroupName, previousGroupName);
        });
    }

    /// <summary>
    /// 메인 메뉴로 돌아갈지 확인하는 팝업을 엽니다.
    /// </summary>
    private void ShowMainMenuConfirmPopup()
    {
        CreateConfirmPopupIfNeeded();

        if (Root_ConfirmPopup != null)
            Root_ConfirmPopup.SetActive(true);
    }

    private void HideMainMenuConfirmPopup()
    {
        if (Root_ConfirmPopup != null)
            Root_ConfirmPopup.SetActive(false);
    }

    /// <summary>
    /// 확인 팝업에서 예를 누르면 현재 그룹을 닫고 MainMenuGroup을 엽니다.
    /// </summary>
    private void ConfirmReturnToMainMenu()
    {
        HideMainMenuConfirmPopup();

        GameObject ownerGroup = FindSceneObjectByName(GetOwnerGroupName());
        GameObject mainMenuGroup = FindSceneObjectByName(_mainMenuGroupName);

        if (OOTechUIManager.Inst != null)
        {
            if (ownerGroup != null)
                OOTechUIManager.Inst.RegisterUI(GetOwnerGroupName(), ownerGroup);

            if (mainMenuGroup != null)
                OOTechUIManager.Inst.RegisterUI(_mainMenuGroupName, mainMenuGroup);

            OOTechUIManager.Inst.CloseUI(GetOwnerGroupName());

            if (OOTechUIManager.Inst.OpenUI(_mainMenuGroupName))
                return;
        }

        if (ownerGroup != null)
            ownerGroup.SetActive(false);

        if (mainMenuGroup != null)
            mainMenuGroup.SetActive(true);
    }

    /// <summary>
    /// 씬에 배치된 확인 팝업 버튼을 예/아니오 동작에 연결합니다.
    /// </summary>
    private void CreateConfirmPopupIfNeeded()
    {
        if (Root_ConfirmPopup == null)
        {
            Debug.LogError("[OOTechRoadHUDController] Panel_MainMenuConfirm is missing from HUDUIGroup.");
            return;
        }

        if (Text_MainMenuConfirmMessage != null)
            Text_MainMenuConfirmMessage.text = "정말 메인 메뉴로 돌아가시겠습니까?";

        BindButtonEvent(Button_MainMenuConfirmYes, ConfirmReturnToMainMenu);
        BindButtonEvent(Button_MainMenuConfirmNo, HideMainMenuConfirmPopup);
        Root_ConfirmPopup.SetActive(false);
    }

    private void CreateGuideOverlayIfNeeded()
    {
        if (Root_GuideOverlay != null)
            return;

        Debug.LogError("[OOTechRoadHUDController] Panel_HUDFocusGuide is missing from HUDUIGroup.");
    }

    /// <summary>
    /// HUD 튜토리얼 화살표와 설명 패널을 선택한 버튼 근처에 배치합니다.
    /// </summary>
    private void ApplyGuideOverlayLayout(Vector2 targetLocalPosition, string title, string description, UnityAction onNext)
    {
        if (Rect_FocusArrow != null)
            Rect_FocusArrow.anchoredPosition = new Vector2(targetLocalPosition.x, Mathf.Clamp(targetLocalPosition.y + 100f, 170f, 930f));

        if (Rect_GuideTextPanel != null)
            Rect_GuideTextPanel.anchoredPosition = new Vector2(Mathf.Clamp(targetLocalPosition.x, -710f, 710f), Mathf.Clamp(targetLocalPosition.y + 170f, 240f, 830f));

        if (Text_GuideTitle != null)
            Text_GuideTitle.text = string.IsNullOrEmpty(title) ? "튜토리얼" : title;

        if (Text_GuideBody != null)
            Text_GuideBody.text = string.IsNullOrEmpty(description) ? "이 기능은 나중에 데이터로 교체됩니다." : description;

        UnityAction guideNextAction = delegate
        {
            _guideClickAction = null;
            CloseHUDGuide();
            onNext?.Invoke();
        };

        _guideClickAction = guideNextAction;
        DisableLegacyGuideNextButtonObject();
        BindGuideClickArea(ResolveGuideTextPanelButton(), guideNextAction);
        BindGuideClickArea(ResolveGuideOverlayButton(), guideNextAction);
    }

    private void UpdateHUDGuideClickInput()
    {
        if (_guideClickAction == null || Root_GuideOverlay == null || !Root_GuideOverlay.activeInHierarchy)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        UnityAction guideClickAction = _guideClickAction;
        _guideClickAction = null;
        guideClickAction.Invoke();
    }

    private Button ResolveGuideTextPanelButton()
    {
        if (Rect_GuideTextPanel == null)
            return null;

        Button guidePanelButton = Rect_GuideTextPanel.GetComponent<Button>();

        if (guidePanelButton == null)
            guidePanelButton = Rect_GuideTextPanel.gameObject.AddComponent<Button>();

        Graphic targetGraphic = Rect_GuideTextPanel.GetComponent<Graphic>();

        if (targetGraphic != null)
        {
            targetGraphic.raycastTarget = true;
            guidePanelButton.targetGraphic = targetGraphic;
        }

        return guidePanelButton;
    }

    private Button ResolveGuideOverlayButton()
    {
        if (Root_GuideOverlay == null)
            return null;

        Button guideOverlayButton = Root_GuideOverlay.GetComponent<Button>();

        if (guideOverlayButton == null)
            guideOverlayButton = Root_GuideOverlay.AddComponent<Button>();

        Graphic targetGraphic = Root_GuideOverlay.GetComponent<Graphic>();

        if (targetGraphic != null)
        {
            targetGraphic.raycastTarget = true;
            guideOverlayButton.targetGraphic = targetGraphic;
        }

        return guideOverlayButton;
    }

    private void BindGuideClickArea(Button guideClickButton, UnityAction guideNextAction)
    {
        if (guideClickButton == null || guideNextAction == null)
            return;

        guideClickButton.onClick.RemoveAllListeners();
        guideClickButton.onClick.AddListener(guideNextAction);
    }

    /// <summary>
    /// 예전 Button_GuideNext 소품은 숨기고, HUD 가이드는 패널이나 배경 클릭으로만 다음 큐로 넘깁니다.
    /// </summary>
    private void DisableLegacyGuideNextButtonObject()
    {
        if (Button_GuideNext == null)
            return;

        if (Rect_GuideTextPanel != null && Button_GuideNext.transform == Rect_GuideTextPanel)
            return;

        Button_GuideNext.onClick.RemoveAllListeners();
        Button_GuideNext.gameObject.SetActive(false);
    }

    private Vector2 GetTargetLocalPosition(RectTransform targetRect)
    {
        if (targetRect == null || Rect_HUD == null)
            return new Vector2(0f, 120f);

        Vector3[] cornerArray = new Vector3[4];
        targetRect.GetWorldCorners(cornerArray);
        Vector3 worldCenter = (cornerArray[0] + cornerArray[2]) * 0.5f;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect_HUD, worldCenter, null, out Vector2 localPoint);
        return localPoint;
    }

    private RectTransform GetButtonRect(OOTechRoadHUDButtonKind buttonKind)
    {
        Button targetButton = null;

        switch (buttonKind)
        {
            case OOTechRoadHUDButtonKind.Inventory:
                targetButton = Button_Inventory;
                break;
            case OOTechRoadHUDButtonKind.Codex:
                targetButton = Button_Codex;
                break;
            case OOTechRoadHUDButtonKind.Mission:
                targetButton = Button_Mission;
                break;
            case OOTechRoadHUDButtonKind.Cooking:
                targetButton = Button_Cooking;
                break;
            case OOTechRoadHUDButtonKind.WorldMap:
                targetButton = Button_WorldMap;
                break;
        }

        return targetButton != null ? targetButton.transform as RectTransform : null;
    }

    private GameObject FindSceneObjectByName(string objectName)
    {
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

    private bool IsSceneGroupActive(string groupName)
    {
        GameObject groupObject = FindSceneObjectByName(groupName);
        return groupObject != null && groupObject.activeSelf;
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
