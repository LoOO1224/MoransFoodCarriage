// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCookingGroupController.cs
// - 역할: 요리 시스템의 입력, 조리도구, 레시피 판정을 담당하는 스크립트입니다.
// - 감독 관점: 부엌 장면에서 재료와 조리도구 배우가 어떤 순서로 만나는지 관리합니다.
// - 유지보수 포인트: 재료 규칙은 데이터와 DropTarget 역할표로 빼고, UI 배치는 CookingUIGroup에서 직접 수정합니다.
// =============================================================================
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// CookingGroup의 요리 튜토리얼, 가마솥 드래그 판정, 완성 대화를 지휘합니다.
/// 부엌 UI 소품은 CookingUIGroup에 배치하고, 이 컨트롤러는 재료가 들어가는 순서만 관리합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechCookingGroupController : MonoBehaviour
{
    private static bool _isSharedToolGuideCompleted;
    private static bool _isSharedJulguGuideCompleted;

    // 읽는 순서:
    // 1. OnEnable: 부엌 무대가 열릴 때 카메라, 조리도구, 인벤토리 UI를 준비합니다.
    // 2. ShowToolGuideSequence: 가마솥과 도마 사용법을 튜토리얼로 보여줍니다.
    // 3. TryAddIngredientToTool 계열: 드래그한 재료가 올바른 조리도구에 들어갔는지 판정합니다.
    // 4. TryCompleteCooking 계열: 쌀 + 채소 조합이 완성 음식으로 바뀌는지 확인합니다.
    // 5. OnDisable: 부엌을 닫을 때 카메라와 HUD 상태를 원래 로드 무대로 되돌립니다.
    // 유지보수 주의:
    // - UI 위치와 이미지는 CookingUIGroup에서 직접 수정합니다.
    // - 새 재료/요리는 가능하면 OO_Ingredient, OO_Recipe, OO_Cook 데이터로 추가합니다.
    // - 이 Controller가 더 커지면 조리 판정, 튜토리얼, 인벤토리 표시를 별도 컴포넌트로 분리해야 합니다.

    [Header("Data Id")]
    [SerializeField] private string _cookingCueSheetId = "Cooking_CueSheet_01";
    private string _cauldronTutorialId = "narration_tutorial_11";
    private string _cuttingboardTutorialId = "narration_tutorial_12";
    private string _julguTutorialId = "narration_tutorial_14";

    [Header("Scene Role")]
    private string _cauldronObjectName = "Cauldron";
    private string _cuttingboardObjectName = "Cuttingboard";
    private string _julguObjectName = "Julgu";
    private float _cauldronDropAreaPadding = 1.18f;
    private float _cuttingboardDropAreaPadding = 1.18f;
    private float _julguDropAreaPadding = 1.18f;

    [Header("Ingredient Rule")]
    private string _riceIngredientId = "Ing_Rice_01";
    private string _vegetableIngredientId = "Ing_Veggie_01";
    private string _pumpkinIngredientId = "Ing_Pumpkin_01";
    private string _pumpkinSoupCookId = "OO_PumpkinSoup_1";
    private string _koreanCakeCookId = "OO_KoreanCake_1";
    private string _kimchIngredientId = "Ing_Kimch_01";
    private string _chiliPepperIngredientId = "Ing_ChiliPepper_01";
    private string[] _defaultCauldronAcceptedIngredientIdArray = { "Ing_Rice_01", "Ing_Kimch_01" };
    private string[] _defaultCuttingboardAcceptedIngredientIdArray = { "Ing_Veggie_01", "Ing_Pumpkin_01", "Ing_ChiliPepper_01" };
    private string[] _defaultJulguAcceptedIngredientIdArray = { "Ing_Rice_01" };
    private int _stage1PumpkinSoupExchangeCount = 10;
    private int _stage1ChiefRewardCount = 10;

    [Header("Canvas")]
    private int _sortingOrder = 1260;
    private Vector2 _referenceResolution = new Vector2(1920f, 1080f);

    [Header("Render Priority")]
    private string _cookingSpriteSortingLayerName = "Characters";
    private int _cookingCanvasSortingOrder = 32000;
    private int _kitchenRendererSortingOrder = 32000;
    private int _toolRendererSortingOrder = 32010;

    [Header("Camera")]
    [SerializeField] private Camera Camera_Main;
    private float _cameraPadding = 1.04f;
    private bool _isCoverKitchenScreen = true;
    private float _kitchenCoverPadding = 0.98f;

    [Header("New Badge")]
    private float _newBadgeBlinkSpeed = 7f;
    private float _newBadgeMinimumAlpha = 0.25f;

    [Header("Guide Arrow")]
    private float _guideArrowBlinkSpeed = 6f;
    private float _guideArrowMinimumAlpha = 0.25f;

    [Header("Drag Ghost")]
    private Vector2 _dragGhostIconSize = new Vector2(88f, 88f);

    [Header("Honey Cake Combine")]
    private string _honeyIngredientId = "Ing_Honey_01";
    private string _honeyKoreanCakeCookId = "OO_HoneyKoreanCake_1";

    private readonly OOTechCookingCueSheetService _cueSheetService = new OOTechCookingCueSheetService();
    private readonly OOTechCookingToolResolver _toolResolver = new OOTechCookingToolResolver();
    private readonly OOTechCookingDropFlow _dropFlow = new OOTechCookingDropFlow();
    private readonly OOTechCookingRecipeService _recipeService = new OOTechCookingRecipeService();
    private readonly OOTechCookingDragGhostPresenter _dragGhostPresenter = new OOTechCookingDragGhostPresenter();
    private readonly OOTechCookingIngredientSelectionModel _selectionModel = new OOTechCookingIngredientSelectionModel();
    private string _defaultStatusText = "재료를 집어 알맞은 조리도구 위에 올려주세요.";

    private CameraFollowController Camera_Follow;
    private bool _hasSavedCameraState;
    private Vector3 _savedCameraPosition;
    private float _savedOrthographicSize;
    private bool _savedOrthographic;
    private bool _savedFollowEnabled;

    private GameObject Root_Canvas;
    private RectTransform Rect_Root;
    private RectTransform Rect_InventoryContent;
    private RectTransform Rect_Cauldron;
    private RectTransform Rect_Cuttingboard;
    private Transform Transform_Cauldron;
    private Transform Transform_Cuttingboard;
    private Transform Transform_Julgu;
    private SpriteRenderer Renderer_Cauldron;
    private SpriteRenderer Renderer_Cuttingboard;
    private SpriteRenderer Renderer_Julgu;
    private Collider2D Collider_Cauldron;
    private Collider2D Collider_Cuttingboard;
    private Collider2D Collider_Julgu;
    private OOTechCookingToolDropTarget Tool_Cauldron;
    private OOTechCookingToolDropTarget Tool_Cuttingboard;
    private OOTechCookingToolDropTarget Tool_Julgu;
    private GameObject Root_GuideBubble;
    private GameObject Root_CuttingboardGuideBubble;
    private GameObject Root_InventoryGuideArrow;
    private GameObject Root_CauldronGuideArrow;
    private GameObject Root_CuttingboardGuideArrow;
    private TextMeshProUGUI Text_Status;
    private TextMeshProUGUI Text_Pot;
    private TextMeshProUGUI Text_GuideTitle;
    private TextMeshProUGUI Text_GuideBody;
    private TextMeshProUGUI Text_CuttingboardGuideTitle;
    private TextMeshProUGUI Text_CuttingboardGuideBody;
    private TextMeshProUGUI Text_InventoryNewBadge;
    private Button Button_GuideConfirm;
    private Button Button_CuttingboardGuideConfirm;
    private RectTransform Rect_DragGhostTemplate;
    private GameObject Root_HoneyCakeCombinePanel;
    private Button Button_HoneyCakeCombine;
    private Image Image_HoneyCakeSlotA;
    private Image Image_HoneyCakeSlotB;
    private TextMeshProUGUI Text_HoneyCakeGuide;
    private string _honeyCakeSlotAItemId;
    private string _honeyCakeSlotBItemId;
    private OOTechCookingGroupView View_Cooking;
    private OOTechCookingGuideCue Cue_Guide;
    private OOTechCookingInventoryBridge Bridge_Inventory;
    private OOTechCookingResultPresenter Presenter_Result;
    private Coroutine _toolGuideCoroutine;
    private Coroutine _cauldronArrowBlinkCoroutine;
    private Coroutine _cuttingboardArrowBlinkCoroutine;
    private UnityAction _guideConfirmAction;
    private bool _isToolGuideComplete;
    private bool _isJulguGuideActive;
    private Coroutine Coroutine_KoreanCakeInventoryRepair;

    /// <summary>
    /// 부엌 무대가 열리면 카메라를 Kitchen 배경에 맞추고 가마솥 가이드를 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        NormalizeCookingGroupTransformIfNeeded();
        EnsureCookingManager();
        ApplyCookingCueSheetData();
        ResolveCauldronReference();
        ResolveCuttingboardReference();
        ResolveJulguReference();
        ApplyKitchenCameraView();
        ApplyCookingRenderPriority();
        PrepareCookingView();
        UpdateHoneyCakeCombineUnlockState();
        PrepareEncounterReturnButtonIfNeeded();
        ResolveRoleComponents();
        RequestOpenCookingSupportHUDIfNeeded();
        StartCoroutine(RequestOpenCookingSupportHUDNextFrameRoutine());
        RepairStage1RewardInventoryIfNeeded();
        NormalizeStage3CookingInventoryIfNeeded();
        RefreshInventorySlots();
        RefreshPotView();

        if (IsOpenedFromEncounterGroup() && !_isSharedJulguGuideCompleted && Transform_Julgu != null)
        {
            ShowJulguGuideSequence();
        }
        else if (_isSharedToolGuideCompleted)
        {
            _isToolGuideComplete = true;
            HideGuideBubble();
            SetStatus(_defaultStatusText);
        }
        else
        {
            ShowToolGuideSequence();
        }
    }

    /// <summary>
    /// HUD 버튼 호출 경로가 끊겨도 CookingGroup 진입 시 인벤토리와 임무 패널을 다시 엽니다.
    /// Game View에서는 부엌 입장 직후 재료 슬롯이 보이고 드래그 입력을 받을 수 있게 하는 안전장치입니다.
    /// </summary>
    private void RequestOpenCookingSupportHUDIfNeeded()
    {
        OOTechRoadHUDController hudController = FindAnyObjectByType<OOTechRoadHUDController>(FindObjectsInactive.Include);

        if (hudController == null)
            return;

        hudController.RequestOpenCookingSupportHUD();
    }

    /// <summary>
    /// CookingGroup이 켜진 바로 다음 프레임에도 HUD를 다시 엽니다. 무대 전환 직후 배우와 소품 등록 순서가 어긋나도 인벤토리 드래그를 복구하는 보험입니다.
    /// </summary>
    private IEnumerator RequestOpenCookingSupportHUDNextFrameRoutine()
    {
        yield return null;
        RequestOpenCookingSupportHUDIfNeeded();
    }

    /// <summary>
    /// 매 프레임 UI 드롭 영역과 화살표가 실제 가마솥 위치를 따라가도록 보정합니다.
    /// </summary>
    /// <summary>
    /// Stage3 산군 부엌에서는 절구를 직접 클릭해 쌀 1개를 떡 1개로 바꿉니다.
    /// 드래그 판정이 흔들려도 Game View 진행이 막히지 않도록, 절구 배우가 자기 역할을 직접 수행하는 경로입니다.
    /// </summary>
    private void Update()
    {
        if (!IsOpenedFromEncounterGroup() || !_isToolGuideComplete || Transform_Julgu == null)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (!IsPointerInsideJulguInteractionArea(Input.mousePosition))
            return;

        TryConvertRiceToKoreanCakeOnJulgu(_riceIngredientId, _julguObjectName);
    }

    private bool IsPointerInsideJulguInteractionArea(Vector2 screenPosition)
    {
        if (IsPointerInsideJulgu(screenPosition))
            return true;

        ResolveCameraReference();

        if (Camera_Main == null || Transform_Julgu == null)
            return false;

        Vector3 julguScreenPoint = Camera_Main.WorldToScreenPoint(Transform_Julgu.position);
        float safeRadius = 220f;
        return Vector2.Distance(screenPosition, new Vector2(julguScreenPoint.x, julguScreenPoint.y)) <= safeRadius;
    }

    private void LateUpdate()
    {
        if (Root_Canvas == null)
            return;

        UpdateCauldronDropArea();
        UpdateCuttingboardDropArea();
        UpdateGuideArrowLayout();
    }

    /// <summary>
    /// 부엌 무대가 닫히면 대화, 배지 깜빡임, 카메라 상태를 원래 로드 장면으로 복구합니다.
    /// </summary>
    private void OnDisable()
    {
        _guideConfirmAction = null;
        _isToolGuideComplete = false;
        StopToolGuideRoutine();
        StopGuideArrowBlink(Root_CauldronGuideArrow, ref _cauldronArrowBlinkCoroutine);
        StopGuideArrowBlink(Root_CuttingboardGuideArrow, ref _cuttingboardArrowBlinkCoroutine);
        StopInventoryNewBadgeBlink();
        if (Presenter_Result != null)
            Presenter_Result.RequestCloseDialogueGroup();

        RestoreCameraView();
    }

    /// <summary>
    /// Stage1 촌장 보상 교환이 누락된 채 부엌에 들어온 경우 인벤토리를 보정합니다.
    /// 영화로 치면 부엌 장면 시작 전에 소품 담당이 호박죽을 회수하고, 다음 요리에 필요한 청양고추와 김치를 올려두는 큐입니다.
    /// </summary>
    private void RepairStage1RewardInventoryIfNeeded()
    {
        if (!IsOpenedFromSecondRoadToStage2())
            return;

        if (OOTechGameManager.Inst == null)
            return;

        if (OOTechGameManager.Inst.GetItemCount(_pumpkinSoupCookId) < _stage1PumpkinSoupExchangeCount)
            return;

        if (!OOTechGameManager.Inst.RemoveItem(_pumpkinSoupCookId, _stage1PumpkinSoupExchangeCount))
            return;

        AddInventoryItemToTargetCount(_chiliPepperIngredientId, _stage1ChiefRewardCount);
        AddInventoryItemToTargetCount(_kimchIngredientId, _stage1ChiefRewardCount);
        NotifyRoadHUDInventoryRefresh();
        NotifyRoadHUDInventoryNewBadge();
        Debug.Log("[OOTechCookingGroupController] Stage1 missing reward was repaired before cooking.");
    }

    private bool IsOpenedFromSecondRoadToStage2()
    {
        string previousGroupName = OOTechGroupNavigationHistory.GetPreviousGroup(gameObject.name, string.Empty);
        return previousGroupName == "2nd_Road_to_Stage2";
    }

    private bool IsOpenedFromEncounterGroup()
    {
        string previousGroupName = OOTechGroupNavigationHistory.GetPreviousGroup(gameObject.name, string.Empty);
        return previousGroupName == "EncounterGroup";
    }

    /// <summary>
    /// EncounterGroup에서 열린 부엌은 돌아가기 버튼을 반드시 EncounterGroup으로 고정합니다.
    /// 이전 Stage2 복귀 리스너가 남아 있어도 이 장면에서는 산군 무대로 돌아가야 합니다.
    /// </summary>
    private void PrepareEncounterReturnButtonIfNeeded()
    {
        if (!IsOpenedFromEncounterGroup())
            return;

        BackButtonController[] backButtonArray = GetComponentsInChildren<BackButtonController>(true);

        foreach (BackButtonController backButton in backButtonArray)
            backButton.SetPreviousGroup("EncounterGroup");

        Transform returnButtonTransform = FindChildByName(transform, "Button_RuntimeReturn");

        if (returnButtonTransform == null)
            return;

        Button returnButton = returnButtonTransform.GetComponent<Button>();

        if (returnButton == null)
            return;

        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(ReturnToEncounterGroup);
    }

    private void ReturnToEncounterGroup()
    {
        OOTechGroupNavigationHistory.SetPreviousGroup(gameObject.name, "EncounterGroup");

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.CloseUI(gameObject.name);
            OOTechUIManager.Inst.OpenUI("EncounterGroup");
            return;
        }

        GameObject encounterGroupObject = FindSceneObjectByName("EncounterGroup");

        gameObject.SetActive(false);

        if (encounterGroupObject != null)
            encounterGroupObject.SetActive(true);
    }

    /// <summary>
    /// 외부 드래그 슬롯이 현재 부엌이 산군 Encounter 전용 부엌인지 확인할 때 사용합니다.
    /// 이 장면에서는 쌀을 절구에 1개씩만 넣게 하여 수량 선택 사고를 막습니다.
    /// </summary>
    public bool IsStage3EncounterCooking()
    {
        return IsOpenedFromEncounterGroup();
    }

    /// <summary>
    /// Stage3 부엌 진입 시 이전 튜토리얼 시작 보상이 다시 보정되어 쌀/채소가 12개로 되돌아가는 경우를 막습니다.
    /// 이미 떡이나 꿀떡을 만든 뒤에는 플레이어 진행 수량을 건드리지 않습니다.
    /// </summary>
    private void NormalizeStage3CookingInventoryIfNeeded()
    {
        if (!IsOpenedFromEncounterGroup() || OOTechGameManager.Inst == null)
            return;

        if (OOTechGameManager.Inst.GetItemCount(_koreanCakeCookId) > 0 ||
            OOTechGameManager.Inst.GetItemCount(_honeyKoreanCakeCookId) > 0)
            return;

        List<OOTechItemModel> itemList = OOTechGameManager.Inst.GetPlayerItemList();
        ClampInventoryItemCount(itemList, _riceIngredientId, 11);
        ClampInventoryItemCount(itemList, _vegetableIngredientId, 11);
        RequestLogInventorySnapshot("Normalize Stage3 cooking inventory");
    }

    private void ClampInventoryItemCount(List<OOTechItemModel> itemList, string itemDataId, int maxCount)
    {
        OOTechItemModel itemModel = FindInventoryItemModel(itemList, itemDataId);

        if (itemModel == null)
            return;

        itemModel.ItemStackCount = Mathf.Min(itemModel.ItemStackCount, Mathf.Max(0, maxCount));
    }

    private void AddInventoryItemToTargetCount(string itemDataId, int targetCount)
    {
        if (OOTechGameManager.Inst == null || string.IsNullOrEmpty(itemDataId))
            return;

        int safeTargetCount = Mathf.Max(1, targetCount);
        int currentCount = OOTechGameManager.Inst.GetItemCount(itemDataId);
        int addCount = safeTargetCount - currentCount;

        if (addCount <= 0)
            return;

        OOTechGameManager.Inst.AddItem(itemDataId, addCount);
    }

    /// <summary>
    /// OO_CookingCueSheet 데이터에서 부엌 장면의 기본 숫자와 ID를 읽어 적용합니다.
    /// 영화 비유로는 공연 시작 전에 조감독이 큐시트를 보고 조명 밝기, 카메라 거리, 안내판 속도를 맞추는 단계입니다.
    /// </summary>
    private void ApplyCookingCueSheetData()
    {
        OO_CookingCueSheet cueSheetData = _cueSheetService.RequestGetCueSheetData(_cookingCueSheetId);

        if (cueSheetData == null)
            return;

        if (!string.IsNullOrEmpty(cueSheetData.CauldronTutorialId))
            _cauldronTutorialId = cueSheetData.CauldronTutorialId;

        if (!string.IsNullOrEmpty(cueSheetData.CuttingboardTutorialId))
            _cuttingboardTutorialId = cueSheetData.CuttingboardTutorialId;

        if (!string.IsNullOrEmpty(cueSheetData.JulguTutorialId))
            _julguTutorialId = cueSheetData.JulguTutorialId;

        if (!string.IsNullOrEmpty(cueSheetData.CauldronToolId))
            _cauldronObjectName = cueSheetData.CauldronToolId;

        if (!string.IsNullOrEmpty(cueSheetData.CuttingboardToolId))
            _cuttingboardObjectName = cueSheetData.CuttingboardToolId;

        if (!string.IsNullOrEmpty(cueSheetData.JulguToolId))
            _julguObjectName = cueSheetData.JulguToolId;

        _sortingOrder = cueSheetData.SortingOrder > 0 ? cueSheetData.SortingOrder : _sortingOrder;
        _referenceResolution = _cueSheetService.RequestGetReferenceResolution(cueSheetData, _referenceResolution);
        _cameraPadding = cueSheetData.CameraPadding > 0f ? cueSheetData.CameraPadding : _cameraPadding;
        _guideArrowBlinkSpeed = cueSheetData.GuideArrowBlinkSpeed > 0f ? cueSheetData.GuideArrowBlinkSpeed : _guideArrowBlinkSpeed;
        _guideArrowMinimumAlpha = cueSheetData.GuideArrowMinimumAlpha > 0f ? cueSheetData.GuideArrowMinimumAlpha : _guideArrowMinimumAlpha;
        _newBadgeBlinkSpeed = cueSheetData.NewBadgeBlinkSpeed > 0f ? cueSheetData.NewBadgeBlinkSpeed : _newBadgeBlinkSpeed;
        _newBadgeMinimumAlpha = cueSheetData.NewBadgeMinimumAlpha > 0f ? cueSheetData.NewBadgeMinimumAlpha : _newBadgeMinimumAlpha;
        _dragGhostIconSize = _cueSheetService.RequestGetDragGhostSize(cueSheetData, _dragGhostIconSize);

        if (!string.IsNullOrEmpty(cueSheetData.DefaultStatusText))
            _defaultStatusText = cueSheetData.DefaultStatusText;
    }

    /// <summary>
    /// 플레이어가 재료 슬롯을 드래그할 때 손에 든 것처럼 보이는 임시 잔상 UI를 만듭니다.
    /// </summary>
    public RectTransform CreateDragGhost(string itemDataId, Vector2 screenPosition)
    {
        return CreateDragGhost(itemDataId, screenPosition, 1);
    }

    /// <summary>
    /// 선택한 수량을 표시한 드래그 잔상 UI를 만듭니다.
    /// Game View에서는 음식 아이콘만 손에 들고, 여러 개를 집었을 때만 작게 xN 표시가 붙습니다.
    /// </summary>
    public RectTransform CreateDragGhost(string itemDataId, Vector2 screenPosition, int itemQuantity)
    {
        if (Rect_Root == null)
            return null;

        if (Rect_DragGhostTemplate == null)
        {
            Debug.LogWarning("[OOTechCookingGroupController] Slot_DragGhostTemplate is missing from CookingUIGroup.");
            return null;
        }

        return _dragGhostPresenter.RequestCreateDragGhost(Rect_Root, Rect_DragGhostTemplate, itemDataId, screenPosition, itemQuantity, _dragGhostIconSize);
    }

    /// <summary>
    /// CookingGroup 원점이 멀리 밀려 있으면 Kitchen/Cauldron 세트만 가까운 로컬 좌표로 다시 정리합니다.
    /// Game View의 월드 위치는 유지하고, 감독이 Scene View에서 편집하기 쉬운 무대 좌표로 되돌립니다.
    /// </summary>
    private void NormalizeCookingGroupTransformIfNeeded()
    {
        Vector3 groupOffset = transform.localPosition;

        if (groupOffset.sqrMagnitude <= 0.01f)
            return;

        for (int index = 0; index < transform.childCount; index++)
        {
            Transform childTransform = transform.GetChild(index);

            if (childTransform is RectTransform)
                continue;

            childTransform.localPosition += groupOffset;
        }

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 마우스 포인터가 실제 가마솥 또는 UI 드롭 영역 안에 있는지 확인합니다.
    /// </summary>
    public bool IsPointerInsideCauldron(Vector2 screenPosition)
    {
        ResolveCauldronReference();

        if (IsPointerInsideSceneTool(screenPosition, Tool_Cauldron, Transform_Cauldron, Renderer_Cauldron, Collider_Cauldron, _cauldronDropAreaPadding))
            return true;

        UpdateCauldronDropArea();

        if (Rect_Cauldron == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(Rect_Cauldron, screenPosition, null);
    }

    /// <summary>
    /// 마우스 포인터가 도마 위에 있는지 확인합니다.
    /// </summary>
    public bool IsPointerInsideCuttingboard(Vector2 screenPosition)
    {
        ResolveCuttingboardReference();

        if (IsPointerInsideSceneTool(screenPosition, Tool_Cuttingboard, Transform_Cuttingboard, Renderer_Cuttingboard, Collider_Cuttingboard, _cuttingboardDropAreaPadding))
            return true;

        UpdateCuttingboardDropArea();

        if (Rect_Cuttingboard == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(Rect_Cuttingboard, screenPosition, null);
    }

    /// <summary>
    /// 마우스 포인터가 절구 위에 있는지 확인합니다.
    /// Stage3에서는 쌀을 절구에 올려 떡을 만들기 때문에 월드 오브젝트 기준으로 판정합니다.
    /// </summary>
    public bool IsPointerInsideJulgu(Vector2 screenPosition)
    {
        ResolveJulguReference();
        return IsPointerInsideSceneTool(screenPosition, Tool_Julgu, Transform_Julgu, Renderer_Julgu, Collider_Julgu, _julguDropAreaPadding);
    }

    /// <summary>
    /// 재료를 놓은 화면 좌표가 어느 조리도구 위인지 판정하고, 맞는 역할표에만 투입합니다.
    /// </summary>
    public void RequestDropIngredientAtPosition(string itemDataId, Vector2 screenPosition)
    {
        RequestDropIngredientAtPosition(itemDataId, screenPosition, 1);
    }

    /// <summary>
    /// 선택한 수량만큼 재료를 놓은 위치의 조리도구에 올립니다.
    /// 기본은 1개이며, 플레이어가 Ctrl+휠로 올린 수량만큼만 소비합니다.
    /// </summary>
    public void RequestDropIngredientAtPosition(string itemDataId, Vector2 screenPosition, int itemQuantity)
    {
        if (!_isToolGuideComplete)
        {
            SetStatus("조리도구 안내를 확인한 뒤 재료를 넣어주세요.");
            return;
        }

        if (IsOpenedFromEncounterGroup() && itemDataId == _riceIngredientId && IsPointerInsideJulguInteractionArea(screenPosition))
        {
            TryConvertRiceToKoreanCakeOnJulgu(_riceIngredientId, _julguObjectName);
            Debug.LogWarning("[OOTechCookingGroupController] Julgu independent system accepted rice before normal drop flow.");
            return;
        }

        if (TryDropIngredientToHoneyCakeCombine(itemDataId, screenPosition))
            return;

        OOTechCookingDropToolType dropToolType = _dropFlow.RequestResolveDropTool(IsPointerInsideCauldron(screenPosition), IsPointerInsideCuttingboard(screenPosition), IsPointerInsideJulgu(screenPosition));

        if (dropToolType == OOTechCookingDropToolType.Cauldron)
        {
            RequestDropIngredientToTool(itemDataId, itemQuantity, Tool_Cauldron, _cauldronObjectName, "가마솥");
            return;
        }

        if (dropToolType == OOTechCookingDropToolType.Cuttingboard)
        {
            RequestDropIngredientToTool(itemDataId, itemQuantity, Tool_Cuttingboard, _cuttingboardObjectName, "도마");
            return;
        }

        if (dropToolType == OOTechCookingDropToolType.Julgu)
        {
            RequestDropIngredientToTool(itemDataId, itemQuantity, Tool_Julgu, _julguObjectName, "절구");
            return;
        }

        if (IsOpenedFromEncounterGroup() && itemDataId == _riceIngredientId && Tool_Julgu != null)
        {
            RequestDropIngredientToTool(itemDataId, itemQuantity, Tool_Julgu, _julguObjectName, "절구");
            Debug.LogWarning("[OOTechCookingGroupController] Julgu pointer fallback accepted rice for Stage3 cooking.");
            return;
        }

        SetStatus("재료를 조리도구 위에 올려놓으세요.");
    }

    /// <summary>
    /// 가마솥에 재료 하나를 넣어 달라는 요청을 처리합니다.
    /// 성공하면 인벤토리에서 1개를 빼고 냄비 상태를 갱신합니다.
    /// </summary>
    public void RequestDropIngredient(string itemDataId)
    {
        if (string.IsNullOrEmpty(itemDataId))
            return;

        if (OOTechGameManager.Inst == null || OOTechGameManager.Inst.GetItemCount(itemDataId) <= 0)
        {
            SetStatus("인벤토리에 재료가 없습니다.");
            return;
        }

        if (!CanAcceptIngredient(itemDataId))
        {
            SetStatus("지금 가마솥에 넣을 수 있는 재료가 아닙니다.");
            return;
        }

        if (!OOTechGameManager.Inst.RemoveItem(itemDataId, 1))
        {
            SetStatus("재료를 꺼낼 수 없습니다.");
            return;
        }

        RegisterSelectedIngredient(itemDataId, 1, _cauldronObjectName);
        RefreshInventorySlots();
        RefreshPotView();
        TryCompleteCooking();
    }

    /// <summary>
    /// 실제 조리도구 역할표를 확인한 뒤 재료를 소비하고 레시피 판정을 요청합니다.
    /// </summary>
    private void RequestDropIngredientToTool(string itemDataId, int itemQuantity, OOTechCookingToolDropTarget toolTarget, string fallbackToolId, string fallbackToolName)
    {
        if (string.IsNullOrEmpty(itemDataId))
            return;

        int dropQuantity = Mathf.Max(1, itemQuantity);

        if (OOTechGameManager.Inst == null || OOTechGameManager.Inst.GetItemCount(itemDataId) <= 0)
        {
            SetStatus("인벤토리에 재료가 없습니다.");
            return;
        }

        int itemCount = OOTechGameManager.Inst.GetItemCount(itemDataId);
        dropQuantity = Mathf.Clamp(dropQuantity, 1, itemCount);

        if (fallbackToolId == _julguObjectName && itemDataId == _riceIngredientId)
        {
            if (TryConvertRiceToKoreanCakeOnJulgu(itemDataId, fallbackToolId))
                return;

            dropQuantity = 1;
        }

        if (!CanToolAcceptIngredient(itemDataId, toolTarget, fallbackToolId))
        {
            SetStatus("올바르지 않은 재료입니다!");
            return;
        }

        if (!CanAcceptIngredient(itemDataId))
        {
            SetStatus("이미 들어갔거나 레시피에 맞지 않는 재료입니다.");
            return;
        }

        if (!OOTechGameManager.Inst.RemoveItem(itemDataId, dropQuantity))
        {
            SetStatus("재료를 꺼낼 수 없습니다.");
            return;
        }

        RegisterSelectedIngredient(itemDataId, dropQuantity, RequestResolveToolId(toolTarget, fallbackToolId));

        if (TryCompleteJulguKoreanCakeImmediately(itemDataId, dropQuantity, fallbackToolId))
            return;

        RefreshInventorySlots();
        RefreshPotView();
        SetStatus($"{GetItemDisplayName(itemDataId)} {dropQuantity}개를 {GetToolDisplayName(toolTarget, fallbackToolName)}에 올렸습니다.");
        TryCompleteCooking();
    }

    /// <summary>
    /// Stage3 절구는 쌀을 넣는 순간 떡으로 바로 바뀌는 특수 조리도구입니다.
    /// 레시피 매니저 연결이 늦어져도 Game View 진행이 끊기지 않도록, 절구+쌀 조합은 여기서 즉시 완성 처리합니다.
    /// </summary>
    private bool TryCompleteJulguKoreanCakeImmediately(string itemDataId, int dropQuantity, string fallbackToolId)
    {
        if (fallbackToolId != _julguObjectName || itemDataId != _riceIngredientId)
            return false;

        int resultCount = 1;
        RequestForceAddInventoryItem(_koreanCakeCookId, resultCount);
        RequestMoveInventoryItemToTop(_koreanCakeCookId);
        RequestLogInventorySnapshot("After Julgu KoreanCake");
        _selectionModel.RequestClear();
        RefreshInventorySlots();
        RefreshPotView();
        SetInventoryNewBadgeActive(true);
        NotifyRoadHUDInventoryRefresh();
        NotifyRoadHUDInventoryNewBadge();
        HideGuideBubble();
        SetStatus($"떡 {resultCount}개 완성! 인벤토리에 추가되었습니다.");
        Debug.Log($"[OOTechCookingGroupController] Julgu made KoreanCake. rice={dropQuantity}, result={resultCount}");
        RequestStartKoreanCakeInventoryRepair();
        return true;
    }

    /// <summary>
    /// Stage3 절구 전용 직접 변환입니다.
    /// 쌀 차감과 떡 추가를 같은 인벤토리 리스트 안에서 즉시 처리해, 중간 큐나 UI 갱신 순서 때문에 떡이 사라지는 일을 막습니다.
    /// </summary>
    private bool TryConvertRiceToKoreanCakeOnJulgu(string itemDataId, string fallbackToolId)
    {
        if (fallbackToolId != _julguObjectName || itemDataId != _riceIngredientId)
            return false;

        if (RequestForceJulguRiceToKoreanCake())
            return true;

        if (OOTechGameManager.Inst == null)
        {
            SetStatus("인벤토리 매니저가 없어 떡을 만들 수 없습니다.");
            return true;
        }

        List<OOTechItemModel> itemList = OOTechGameManager.Inst.GetPlayerItemList();
        OOTechItemModel riceItem = FindInventoryItemModel(itemList, _riceIngredientId);

        if (riceItem == null || riceItem.ItemStackCount <= 0)
        {
            SetStatus("쌀이 부족합니다.");
            RequestLogInventorySnapshot("Julgu failed - rice missing");
            return true;
        }

        riceItem.ItemStackCount -= 1;

        if (riceItem.ItemStackCount <= 0)
            itemList.Remove(riceItem);

        OOTechItemModel cakeItem = FindInventoryItemModel(itemList, _koreanCakeCookId);

        if (cakeItem == null)
        {
            cakeItem = new OOTechItemModel
            {
                ItemUniqueId = System.DateTime.UtcNow.Ticks,
                ItemDataId = _koreanCakeCookId,
                ItemStackCount = 0
            };

            itemList.Insert(0, cakeItem);
        }

        cakeItem.ItemStackCount += 1;
        RequestMoveInventoryItemToTop(_koreanCakeCookId);
        RequestLogInventorySnapshot("Direct Julgu transaction rice-1 cake+1");
        _selectionModel.RequestClear();
        RefreshInventorySlots();
        RefreshPotView();
        SetInventoryNewBadgeActive(true);
        NotifyRoadHUDInventoryRefresh();
        NotifyRoadHUDInventoryNewBadge();
        HideGuideBubble();
        SetStatus("떡 1개 완성! 인벤토리에 추가되었습니다.");
        RequestStartKoreanCakeInventoryRepair();
        Debug.LogWarning("[OOTechCookingGroupController] Direct Julgu transaction completed. Rice -1, KoreanCake +1.");
        return true;
    }

    /// <summary>
    /// 절구 전용 최종 안전 처리입니다.
    /// 가마솥/도마 레시피 흐름, 선택 모델, Bridge 연결을 전부 우회하고 현재 HUD가 읽는 GameManager 인벤토리를 직접 수정합니다.
    /// </summary>
    private bool RequestForceJulguRiceToKoreanCake()
    {
        if (OOTechGameManager.Inst == null)
        {
            SetStatus("인벤토리 매니저가 없어 떡을 만들 수 없습니다.");
            Debug.LogError("[OOTechCookingGroupController] Julgu force convert failed. OOTechGameManager.Inst is missing.");
            return true;
        }

        bool hasConverted = RequestForceJulguRiceToKoreanCake(OOTechGameManager.Inst);

        if (!hasConverted)
        {
            SetStatus("쌀이 부족합니다.");
            Debug.LogWarning("[OOTechCookingGroupController] Julgu force convert failed. Rice is missing in OOTechGameManager.Inst.");
            return true;
        }

        _selectionModel.RequestClear();
        RefreshInventorySlots();
        RefreshPotView();
        SetInventoryNewBadgeActive(true);
        NotifyRoadHUDInventoryRefresh();
        NotifyRoadHUDInventoryNewBadge();
        RequestOpenCookingSupportHUDIfNeeded();
        HideGuideBubble();
        SetStatus("떡 1개 완성! 인벤토리에 추가되었습니다.");
        RequestStartKoreanCakeInventoryRepair();
        Debug.LogWarning("[OOTechCookingGroupController] Julgu independent system completed. Rice -1, KoreanCake +1.");
        return true;
    }

    private bool RequestForceJulguRiceToKoreanCake(OOTechGameManager gameManager)
    {
        List<OOTechItemModel> itemList = gameManager.GetPlayerItemList();
        OOTechItemModel riceItem = FindInventoryItemModel(itemList, _riceIngredientId);

        if (riceItem == null || riceItem.ItemStackCount <= 0)
            return false;

        riceItem.ItemStackCount -= 1;

        if (riceItem.ItemStackCount <= 0)
            itemList.Remove(riceItem);

        OOTechItemModel cakeItem = FindInventoryItemModel(itemList, _koreanCakeCookId);

        if (cakeItem == null)
        {
            cakeItem = new OOTechItemModel
            {
                ItemUniqueId = System.DateTime.UtcNow.Ticks,
                ItemDataId = _koreanCakeCookId,
                ItemStackCount = 0
            };

            itemList.Insert(0, cakeItem);
        }

        cakeItem.ItemStackCount += 1;
        MoveInventoryItemToTop(itemList, _koreanCakeCookId);
        Debug.LogWarning($"[OOTechCookingGroupController] Julgu converted inventory on manager={gameManager.name}: {_riceIngredientId}-1, {_koreanCakeCookId}+1");
        return true;
    }

    private void MoveInventoryItemToTop(List<OOTechItemModel> itemList, string itemDataId)
    {
        if (itemList == null || string.IsNullOrEmpty(itemDataId))
            return;

        for (int index = 0; index < itemList.Count; index++)
        {
            OOTechItemModel itemModel = itemList[index];

            if (itemModel == null || itemModel.ItemDataId != itemDataId)
                continue;

            if (index <= 0)
                return;

            itemList.RemoveAt(index);
            itemList.Insert(0, itemModel);
            return;
        }
    }

    private OOTechItemModel FindInventoryItemModel(List<OOTechItemModel> itemList, string itemDataId)
    {
        if (itemList == null || string.IsNullOrEmpty(itemDataId))
            return null;

        foreach (OOTechItemModel itemModel in itemList)
        {
            if (itemModel != null && itemModel.ItemDataId == itemDataId)
                return itemModel;
        }

        return null;
    }

    /// <summary>
    /// 절구 완성 직후 HUD가 같은 프레임에서 이전 목록을 붙잡는 경우가 있어, 짧게 재확인 루틴을 돌립니다.
    /// 무대 비유로는 소품 담당자가 떡을 창고에 넣은 뒤 객석 진열대까지 올라왔는지 세 번 확인하는 절차입니다.
    /// </summary>
    private void RequestStartKoreanCakeInventoryRepair()
    {
        if (Coroutine_KoreanCakeInventoryRepair != null)
            StopCoroutine(Coroutine_KoreanCakeInventoryRepair);

        Coroutine_KoreanCakeInventoryRepair = StartCoroutine(RepairKoreanCakeInventoryRoutine());
    }

    private IEnumerator RepairKoreanCakeInventoryRoutine()
    {
        for (int index = 0; index < 3; index++)
        {
            yield return null;

            if (OOTechGameManager.Inst == null)
                continue;

            if (OOTechGameManager.Inst.GetItemCount(_koreanCakeCookId) <= 0)
                RequestForceAddInventoryItem(_koreanCakeCookId, 1);

            RequestMoveInventoryItemToTop(_koreanCakeCookId);
            RefreshInventorySlots();
            NotifyRoadHUDInventoryRefresh();
            RequestOpenCookingSupportHUDIfNeeded();
            RequestLogInventorySnapshot($"KoreanCake repair frame {index + 1}");
        }

        Coroutine_KoreanCakeInventoryRepair = null;
    }

    /// <summary>
    /// GameManager.AddItem 호출 뒤에도 수량이 늘지 않으면 인벤토리 모델에 직접 삽입합니다.
    /// Game View 진행을 막는 핵심 보상은 이중 안전장치로 보장합니다.
    /// </summary>
    private void RequestForceAddInventoryItem(string itemDataId, int count)
    {
        if (OOTechGameManager.Inst == null || string.IsNullOrEmpty(itemDataId) || count <= 0)
            return;

        int beforeCount = OOTechGameManager.Inst.GetItemCount(itemDataId);
        int addCount = Mathf.Max(1, count);
        OOTechGameManager.Inst.AddItem(itemDataId, addCount);
        int afterCount = OOTechGameManager.Inst.GetItemCount(itemDataId);

        if (afterCount >= beforeCount + addCount)
        {
            RequestMoveInventoryItemToTop(itemDataId);
            Debug.LogWarning($"[OOTechCookingGroupController] Force add verified: {itemDataId} x{afterCount}");
            return;
        }

        List<OOTechItemModel> itemList = OOTechGameManager.Inst.GetPlayerItemList();

        foreach (OOTechItemModel itemModel in itemList)
        {
            if (itemModel == null || itemModel.ItemDataId != itemDataId)
                continue;

            itemModel.ItemStackCount = beforeCount + addCount;
            RequestMoveInventoryItemToTop(itemDataId);
            Debug.LogWarning($"[OOTechCookingGroupController] Force repaired inventory count: {itemDataId} x{itemModel.ItemStackCount}");
            return;
        }

        OOTechItemModel newItemModel = new OOTechItemModel
        {
            ItemUniqueId = System.DateTime.UtcNow.Ticks,
            ItemDataId = itemDataId,
            ItemStackCount = beforeCount + addCount
        };

        itemList.Insert(0, newItemModel);
        Debug.LogWarning($"[OOTechCookingGroupController] Force inserted inventory item: {itemDataId} x{newItemModel.ItemStackCount}");
    }

    /// <summary>
    /// CookingGroup 상단에 레시피 조합 버튼과 두 칸짜리 조합판을 준비합니다.
    /// Game View에서는 ChoicePanel 대신 플레이어가 직접 떡과 꿀을 올려 꿀떡을 만드는 작은 조합대입니다.
    /// </summary>
    private void EnsureHoneyCakeCombineUI()
    {
        if (Rect_Root == null || Button_HoneyCakeCombine != null)
            return;

        GameObject buttonObject = CreateCookingUIObject(Rect_Root, "Button_HoneyCakeCombine");
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetCookingUIRect(buttonRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-190f, -128f), new Vector2(260f, 64f));
        Image buttonImage = RequestGetOrAddImage(buttonObject, new Color(1f, 0.92f, 0.45f, 0.96f));
        buttonImage.raycastTarget = true;
        Button_HoneyCakeCombine = RequestGetOrAddButton(buttonObject);
        Button_HoneyCakeCombine.onClick.RemoveAllListeners();
        Button_HoneyCakeCombine.onClick.AddListener(ToggleHoneyCakeCombinePanel);
        TextMeshProUGUI buttonText = CreateCookingUIText(buttonObject.transform, "Text_Label", "레시피 조합", 30f, Color.black);
        SetCookingUIRect(buttonText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Root_HoneyCakeCombinePanel = CreateCookingUIObject(Rect_Root, "Panel_HoneyCakeCombine");
        RectTransform panelRect = Root_HoneyCakeCombinePanel.GetComponent<RectTransform>();
        SetCookingUIRect(panelRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-320f, -224f), new Vector2(560f, 116f));
        Image panelImage = RequestGetOrAddImage(Root_HoneyCakeCombinePanel, new Color(0f, 0f, 0f, 0.58f));
        panelImage.raycastTarget = true;

        Image_HoneyCakeSlotA = CreateCombineSlot(Root_HoneyCakeCombinePanel.transform, "Image_CombineSlotA", new Vector2(-126f, -6f));
        TextMeshProUGUI plusText = CreateCookingUIText(Root_HoneyCakeCombinePanel.transform, "Text_Plus", "+", 42f, Color.white);
        SetCookingUIRect(plusText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -6f), new Vector2(50f, 72f));
        Image_HoneyCakeSlotB = CreateCombineSlot(Root_HoneyCakeCombinePanel.transform, "Image_CombineSlotB", new Vector2(126f, -6f));
        Text_HoneyCakeGuide = CreateCookingUIText(Root_HoneyCakeCombinePanel.transform, "Text_Guide", "떡과 꿀을 빈 칸에 끌어다 놓으면 꿀떡이 완성됩니다.", 22f, Color.white);
        SetCookingUIRect(Text_HoneyCakeGuide.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(520f, 32f));

        Root_HoneyCakeCombinePanel.SetActive(false);
        UpdateHoneyCakeCombineUnlockState();
    }

    /// <summary>
    /// 꿀떡 조합 버튼은 EncounterGroup에서 부엌으로 들어온 장면에서만 열어 둡니다.
    /// 무대 비유로는 산군 장면에서만 쓰는 특수 소품이라, 다른 공연에서는 소품함을 잠가 두는 처리입니다.
    /// </summary>
    private void UpdateHoneyCakeCombineUnlockState()
    {
        bool isUnlocked = IsOpenedFromEncounterGroup();

        if (Button_HoneyCakeCombine != null)
            Button_HoneyCakeCombine.gameObject.SetActive(isUnlocked);

        if (isUnlocked)
            return;

        _honeyCakeSlotAItemId = string.Empty;
        _honeyCakeSlotBItemId = string.Empty;

        ClearHoneyCakeSlot(Image_HoneyCakeSlotA);
        ClearHoneyCakeSlot(Image_HoneyCakeSlotB);

        if (Root_HoneyCakeCombinePanel != null)
            Root_HoneyCakeCombinePanel.SetActive(false);
    }

    private Image CreateCombineSlot(Transform parentTransform, string objectName, Vector2 anchoredPosition)
    {
        GameObject slotObject = CreateCookingUIObject(parentTransform, objectName);
        RectTransform slotRect = slotObject.GetComponent<RectTransform>();
        SetCookingUIRect(slotRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(82f, 82f));
        Image slotImage = RequestGetOrAddImage(slotObject, new Color(1f, 1f, 1f, 0.88f));
        slotImage.raycastTarget = true;
        slotImage.preserveAspect = true;
        return slotImage;
    }

    private void ToggleHoneyCakeCombinePanel()
    {
        if (Root_HoneyCakeCombinePanel == null)
            return;

        if (!IsOpenedFromEncounterGroup())
        {
            Root_HoneyCakeCombinePanel.SetActive(false);
            return;
        }

        bool isActive = !Root_HoneyCakeCombinePanel.activeSelf;
        Root_HoneyCakeCombinePanel.SetActive(isActive);

        if (isActive)
            SetStatus("떡과 꿀을 조합 칸에 끌어다 놓으세요.");
    }

    private bool TryDropIngredientToHoneyCakeCombine(string itemDataId, Vector2 screenPosition)
    {
        if (!IsOpenedFromEncounterGroup() || Root_HoneyCakeCombinePanel == null || !Root_HoneyCakeCombinePanel.activeSelf)
            return false;

        if (itemDataId != _koreanCakeCookId && itemDataId != _honeyIngredientId)
        {
            SetStatus("꿀떡 조합에는 떡과 꿀만 넣을 수 있습니다.");
            return true;
        }

        Image targetSlot = null;

        if (Image_HoneyCakeSlotA != null && RectTransformUtility.RectangleContainsScreenPoint(Image_HoneyCakeSlotA.rectTransform, screenPosition, null))
            targetSlot = Image_HoneyCakeSlotA;
        else if (Image_HoneyCakeSlotB != null && RectTransformUtility.RectangleContainsScreenPoint(Image_HoneyCakeSlotB.rectTransform, screenPosition, null))
            targetSlot = Image_HoneyCakeSlotB;

        if (targetSlot == null)
            return false;

        string currentSlotItemId = targetSlot == Image_HoneyCakeSlotA ? _honeyCakeSlotAItemId : _honeyCakeSlotBItemId;

        if (!string.IsNullOrEmpty(currentSlotItemId))
        {
            SetStatus("이미 재료가 들어간 칸입니다.");
            return true;
        }

        if (OOTechGameManager.Inst == null || !OOTechGameManager.Inst.RemoveItem(itemDataId, 1))
        {
            SetStatus("인벤토리에 재료가 부족합니다.");
            return true;
        }

        if (targetSlot == Image_HoneyCakeSlotA)
            _honeyCakeSlotAItemId = itemDataId;
        else
            _honeyCakeSlotBItemId = itemDataId;

        targetSlot.sprite = OOTechItemCatalogManager.RequestItemIconSprite(itemDataId);
        targetSlot.color = Color.white;
        RefreshInventorySlots();
        RequestTryCompleteHoneyCakeCombine();
        return true;
    }

    private void RequestTryCompleteHoneyCakeCombine()
    {
        bool hasKoreanCake = _honeyCakeSlotAItemId == _koreanCakeCookId || _honeyCakeSlotBItemId == _koreanCakeCookId;
        bool hasHoney = _honeyCakeSlotAItemId == _honeyIngredientId || _honeyCakeSlotBItemId == _honeyIngredientId;

        if (!hasKoreanCake || !hasHoney || OOTechGameManager.Inst == null)
            return;

        RequestForceAddInventoryItem(_honeyKoreanCakeCookId, 1);
        RequestMoveInventoryItemToTop(_honeyKoreanCakeCookId);
        _honeyCakeSlotAItemId = string.Empty;
        _honeyCakeSlotBItemId = string.Empty;
        ClearHoneyCakeSlot(Image_HoneyCakeSlotA);
        ClearHoneyCakeSlot(Image_HoneyCakeSlotB);
        Root_HoneyCakeCombinePanel.SetActive(false);
        RefreshInventorySlots();
        SetInventoryNewBadgeActive(true);
        NotifyRoadHUDInventoryNewBadge();
        RequestOpenCookingSupportHUDIfNeeded();
        SetStatus("꿀떡 1개 완성! 인벤토리에 추가되었습니다.");
        RequestLogInventorySnapshot("After HoneyKoreanCake combine");
    }

    private void ClearHoneyCakeSlot(Image slotImage)
    {
        if (slotImage == null)
            return;

        slotImage.sprite = null;
        slotImage.color = new Color(1f, 1f, 1f, 0.88f);
    }

    private GameObject CreateCookingUIObject(Transform parentTransform, string objectName)
    {
        GameObject targetObject = new GameObject(objectName, typeof(RectTransform));
        targetObject.transform.SetParent(parentTransform, false);
        return targetObject;
    }

    private TextMeshProUGUI CreateCookingUIText(Transform parentTransform, string objectName, string text, float fontSize, Color color)
    {
        GameObject textObject = CreateCookingUIObject(parentTransform, objectName);
        TextMeshProUGUI labelText = textObject.AddComponent<TextMeshProUGUI>();
        OOTechTMPFontUtility.ApplyProjectFont(labelText);
        labelText.text = text;
        labelText.fontSize = fontSize;
        labelText.color = color;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.raycastTarget = false;
        return labelText;
    }

    private Image RequestGetOrAddImage(GameObject targetObject, Color color)
    {
        Image image = targetObject.GetComponent<Image>();

        if (image == null)
            image = targetObject.AddComponent<Image>();

        image.color = color;
        return image;
    }

    private Button RequestGetOrAddButton(GameObject targetObject)
    {
        Button button = targetObject.GetComponent<Button>();

        if (button == null)
            button = targetObject.AddComponent<Button>();

        return button;
    }

    private void SetCookingUIRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
        rectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// 방금 만든 아이템을 인벤토리 맨 위로 올립니다.
    /// Game View에서는 떡 슬롯이 스크롤 아래에 숨어 보이지 않는 상황을 막는 장치입니다.
    /// </summary>
    private void RequestMoveInventoryItemToTop(string itemDataId)
    {
        if (OOTechGameManager.Inst == null || string.IsNullOrEmpty(itemDataId))
            return;

        List<OOTechItemModel> itemList = OOTechGameManager.Inst.GetPlayerItemList();

        for (int index = 0; index < itemList.Count; index++)
        {
            OOTechItemModel itemModel = itemList[index];

            if (itemModel == null || itemModel.ItemDataId != itemDataId)
                continue;

            if (index <= 0)
                return;

            itemList.RemoveAt(index);
            itemList.Insert(0, itemModel);
            return;
        }
    }

    /// <summary>
    /// 조리 직후 인벤토리 모델 전체를 로그로 남깁니다.
    /// 떡이 모델에 들어갔는지, UI 슬롯 생성에서 빠지는지 분리해서 확인합니다.
    /// </summary>
    private void RequestLogInventorySnapshot(string reason)
    {
        if (OOTechGameManager.Inst == null)
            return;

        List<OOTechItemModel> itemList = OOTechGameManager.Inst.GetPlayerItemList();
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        stringBuilder.Append("[OOTechCookingGroupController] Inventory Snapshot - ");
        stringBuilder.Append(reason);
        stringBuilder.Append(": ");

        for (int index = 0; index < itemList.Count; index++)
        {
            OOTechItemModel itemModel = itemList[index];

            if (itemModel == null)
                continue;

            if (index > 0)
                stringBuilder.Append(" / ");

            stringBuilder.Append(itemModel.ItemDataId);
            stringBuilder.Append(" x");
            stringBuilder.Append(itemModel.ItemStackCount);
        }

        Debug.Log(stringBuilder.ToString());
    }

    /// <summary>
    /// 현재까지 들어간 재료 조합이 완성 레시피인지 확인하고, 성공 시 완성 음식을 인벤토리에 넣습니다.
    /// </summary>
    private void TryCompleteCooking()
    {
        CookingResult cookingResult = OOTechCookingManager.Inst != null ? OOTechCookingManager.Inst.TryCook(_selectionModel.SelectedIngredientIdList) : null;

        if (cookingResult == null || !cookingResult.IsSuccess)
        {
            SetStatus("재료가 가마솥에 들어갔습니다. 다음 재료를 넣어주세요.");
            return;
        }

        int resultCount = CalculateCookingResultCount(cookingResult.ResultItemId);

        if (resultCount <= 0)
        {
            SetStatus(GetCookingQuantityGuide(cookingResult.ResultItemId));
            return;
        }

        if (!IsRecipeToolMatched(cookingResult.ResultItemId))
        {
            SetStatus("올바른 조리도구가 아닙니다!");
            return;
        }

        RefundUnusedSelectedIngredients(cookingResult.ResultItemId, resultCount);
        OOTechGameManager.Inst.AddItem(cookingResult.ResultItemId, resultCount);
        string cookName = GetItemDisplayName(cookingResult.ResultItemId);
        _selectionModel.RequestClear();
        RefreshInventorySlots();
        RefreshPotView();
        SetInventoryNewBadgeActive(true);
        NotifyRoadHUDInventoryNewBadge();
        HideGuideBubble();
        RequestPlayCookingCompleteDialogue(cookingResult.ResultItemId);
        SetStatus($"{cookName} 완성! 인벤토리에 새 음식이 들어갔습니다.");
    }

    /// <summary>
    /// 지금 선택한 재료가 레시피의 중간 단계로 허용되는지 확인합니다.
    /// </summary>
    /// <summary>
    /// 재료가 해당 조리도구의 역할표와 맞는지 확인합니다.
    /// </summary>
    /// <summary>
    /// 호박죽 튜토리얼은 쌀 10개와 호박 10개를 한 번에 조리하는 장면입니다.
    /// 첫 재료 1개씩은 이미 제거되었으므로, 남은 재료를 확인해 추가 9회분까지 함께 차감합니다.
    /// </summary>
    private int CalculateCookingResultCount(string resultItemId)
    {
        return _recipeService.RequestCalculateResultCount(resultItemId, _selectionModel);
    }

    /// <summary>
    /// 조리도구에 올라간 재료 이름표와 실제 개수를 기록합니다.
    /// 같은 재료를 여러 번 넣어도 레시피 판정용 이름표는 하나만 두고, 수량만 누적합니다.
    /// </summary>
    private void RegisterSelectedIngredient(string itemDataId, int itemQuantity)
    {
        _selectionModel.RequestRegisterIngredient(itemDataId, itemQuantity);
    }

    private void RegisterSelectedIngredient(string itemDataId, int itemQuantity, string toolId)
    {
        _selectionModel.RequestRegisterIngredient(itemDataId, itemQuantity, toolId);
    }

    /// <summary>
    /// 조리도구에 올라간 특정 재료의 개수를 반환합니다.
    /// </summary>
    private int GetSelectedIngredientAmount(string itemDataId)
    {
        return _selectionModel.RequestGetIngredientAmount(itemDataId);
    }

    private string RequestResolveToolId(OOTechCookingToolDropTarget toolTarget, string fallbackToolId)
    {
        if (toolTarget != null && !string.IsNullOrEmpty(toolTarget.ToolId))
            return toolTarget.ToolId;

        return fallbackToolId;
    }

    /// <summary>
    /// 완성 후보 레시피가 요구한 조리도구와 실제 드롭 위치가 일치하는지 확인합니다.
    /// 쌀 하나만 보고 떡을 만들던 버그를 막고, 쌀이 Julgu 위에 있을 때만 떡 레시피를 통과시킵니다.
    /// </summary>
    private bool IsRecipeToolMatched(string resultItemId)
    {
        OO_Recipe recipeData = _recipeService.RequestFindRecipeByResultItemId(resultItemId);

        if (recipeData == null || recipeData.RequiredToolIds == null || recipeData.RequiredToolIds.Count == 0)
            return true;

        List<string> ingredientIdList = recipeData.RequiredIngredientIds != null && recipeData.RequiredIngredientIds.Count > 0
            ? recipeData.RequiredIngredientIds
            : recipeData.RequiredIngredients;

        if (ingredientIdList == null || ingredientIdList.Count == 0)
            return true;

        for (int index = 0; index < ingredientIdList.Count; index++)
        {
            if (index >= recipeData.RequiredToolIds.Count)
                continue;

            string ingredientId = ingredientIdList[index];
            string requiredToolId = recipeData.RequiredToolIds[index];

            if (string.IsNullOrEmpty(ingredientId) || string.IsNullOrEmpty(requiredToolId))
                continue;

            if (requiredToolId == "Choice")
                continue;

            string actualToolId = _selectionModel.RequestGetIngredientToolId(ingredientId);

            if (actualToolId != requiredToolId)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 선택 수량이 완성에 필요한 양보다 많으면 남은 재료를 인벤토리로 돌려줍니다.
    /// 감독 비유로는 요리에 쓰지 않은 소품을 소품 창고로 되돌려 낭비를 막는 장면입니다.
    /// </summary>
    private void RefundUnusedSelectedIngredients(string resultItemId, int resultCount)
    {
        if (OOTechGameManager.Inst == null || _selectionModel.SelectedIngredientAmountDic.Count == 0)
            return;

        Dictionary<string, int> usedAmountDic = CreateUsedIngredientAmountDic(resultItemId, resultCount);

        foreach (KeyValuePair<string, int> pair in _selectionModel.SelectedIngredientAmountDic)
        {
            usedAmountDic.TryGetValue(pair.Key, out int usedAmount);
            int remainAmount = pair.Value - usedAmount;

            if (remainAmount > 0)
                OOTechGameManager.Inst.AddItem(pair.Key, remainAmount);
        }
    }

    /// <summary>
    /// 완성 음식 하나에 실제로 소비되는 재료 수량표를 만듭니다.
    /// </summary>
    private Dictionary<string, int> CreateUsedIngredientAmountDic(string resultItemId, int resultCount)
    {
        return _recipeService.RequestCreateUsedIngredientAmountDic(resultItemId, resultCount, _selectionModel);
    }

    /// <summary>
    /// 아직 수량이 부족할 때 플레이어에게 필요한 조작을 안내합니다.
    /// </summary>
    private string GetCookingQuantityGuide(string resultItemId)
    {
        return _recipeService.RequestGetQuantityGuide(resultItemId);
    }

    private bool CanToolAcceptIngredient(string itemDataId, OOTechCookingToolDropTarget toolTarget, string fallbackToolId)
    {
        if (toolTarget != null && toolTarget.HasAcceptedIngredientRule)
            return toolTarget.CanAcceptIngredient(itemDataId);

        if (fallbackToolId == _cauldronObjectName)
            return IsIngredientInArray(itemDataId, _defaultCauldronAcceptedIngredientIdArray);

        if (fallbackToolId == _cuttingboardObjectName)
            return IsIngredientInArray(itemDataId, _defaultCuttingboardAcceptedIngredientIdArray);

        if (fallbackToolId == _julguObjectName)
            return IsIngredientInArray(itemDataId, _defaultJulguAcceptedIngredientIdArray);

        return false;
    }

    private bool IsIngredientInArray(string itemDataId, string[] acceptedIngredientIdArray)
    {
        if (string.IsNullOrEmpty(itemDataId) || acceptedIngredientIdArray == null)
            return false;

        for (int index = 0; index < acceptedIngredientIdArray.Length; index++)
        {
            if (acceptedIngredientIdArray[index] == itemDataId)
                return true;
        }

        return false;
    }

    private string GetToolDisplayName(OOTechCookingToolDropTarget toolTarget, string fallbackToolName)
    {
        if (toolTarget != null && !string.IsNullOrEmpty(toolTarget.DisplayName))
            return toolTarget.DisplayName;

        return fallbackToolName;
    }

    private bool CanAcceptIngredient(string itemDataId)
    {
        if (_recipeService.RequestCanAcceptIngredient(itemDataId, _selectionModel))
            return true;

        return IsFallbackIngredientAccepted(itemDataId);
    }

    /// <summary>
    /// 데이터가 아직 완성되지 않았을 때도 쌀+채소/호박 튜토리얼이 동작하도록 하는 안전망입니다.
    /// </summary>
    private bool IsFallbackIngredientAccepted(string itemDataId)
    {
        if (itemDataId != _riceIngredientId &&
            itemDataId != _vegetableIngredientId &&
            itemDataId != _pumpkinIngredientId &&
            itemDataId != _kimchIngredientId &&
            itemDataId != _chiliPepperIngredientId)
            return false;

        if (itemDataId == _vegetableIngredientId ||
            itemDataId == _pumpkinIngredientId ||
            itemDataId == _chiliPepperIngredientId)
        {
            return !_selectionModel.SelectedIngredientIdList.Contains(_vegetableIngredientId) &&
                   !_selectionModel.SelectedIngredientIdList.Contains(_pumpkinIngredientId) &&
                   !_selectionModel.SelectedIngredientIdList.Contains(_chiliPepperIngredientId);
        }

        if (itemDataId == _riceIngredientId || itemDataId == _kimchIngredientId)
            return !_selectionModel.SelectedIngredientIdList.Contains(_riceIngredientId) && !_selectionModel.SelectedIngredientIdList.Contains(_kimchIngredientId);

        return !_selectionModel.SelectedIngredientIdList.Contains(itemDataId);
    }

    /// <summary>
    /// 요리 매니저가 씬에 없으면 최소 동작을 위해 매니저 오브젝트를 준비합니다.
    /// </summary>
    private void EnsureCookingManager()
    {
        if (OOTechCookingManager.Inst != null)
            return;

        Debug.LogError("[OOTechCookingGroupController] OOTechCookingManager is missing. 씬에 배치된 CookingManager 오브젝트를 확인하세요.");
    }

    /// <summary>
    /// CookingUIGroup의 View 컴포넌트에서 말풍선, 드롭 영역, 텍스트 소품을 연결합니다.
    /// </summary>
    private void PrepareCookingView()
    {
        if (Root_Canvas != null)
            return;

        View_Cooking = GetComponentInChildren<OOTechCookingGroupView>(true);

        if (View_Cooking == null)
        {
            Debug.LogError("[OOTechCookingGroupController] CookingUIGroup with OOTechCookingGroupView is missing.");
            return;
        }

        View_Cooking.ResolveReferences();
        Root_Canvas = View_Cooking.gameObject;
        Rect_Root = View_Cooking.RootRect;
        NormalizeCookingCanvasRoot();
        Rect_Cauldron = View_Cooking.CauldronDropAreaRect;
        Rect_Cuttingboard = View_Cooking.CuttingboardDropAreaRect;
        Text_Pot = View_Cooking.PotContentText;
        Root_GuideBubble = View_Cooking.GuideBubble;
        Text_GuideTitle = View_Cooking.GuideTitleText;
        Text_GuideBody = View_Cooking.GuideBodyText;
        Button_GuideConfirm = View_Cooking.GuideConfirmButton;
        Root_CuttingboardGuideBubble = View_Cooking.CuttingboardGuideBubble;
        Text_CuttingboardGuideTitle = View_Cooking.CuttingboardGuideTitleText;
        Text_CuttingboardGuideBody = View_Cooking.CuttingboardGuideBodyText;
        Button_CuttingboardGuideConfirm = View_Cooking.CuttingboardGuideConfirmButton;
        Root_InventoryGuideArrow = View_Cooking.InventoryGuideArrow;
        Root_CauldronGuideArrow = View_Cooking.CauldronGuideArrow;
        Root_CuttingboardGuideArrow = View_Cooking.CuttingboardGuideArrow;
        Text_Status = View_Cooking.StatusText;
        Text_InventoryNewBadge = View_Cooking.InventoryNewBadgeText;
        Rect_DragGhostTemplate = View_Cooking.DragGhostTemplateRect;
        EnsureHoneyCakeCombineUI();

        Canvas canvas = Root_Canvas.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = Mathf.Max(_sortingOrder, _cookingCanvasSortingOrder);
        }

        Root_Canvas.transform.SetAsLastSibling();

        CanvasScaler canvasScaler = Root_Canvas.GetComponent<CanvasScaler>();

        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = _referenceResolution;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        if (Rect_DragGhostTemplate != null)
            Rect_DragGhostTemplate.gameObject.SetActive(false);

        if (Root_GuideBubble != null)
            Root_GuideBubble.SetActive(false);

        if (Root_CuttingboardGuideBubble != null)
            Root_CuttingboardGuideBubble.SetActive(false);

        if (Root_InventoryGuideArrow != null)
            Root_InventoryGuideArrow.SetActive(false);

        SetGuidePointerActive(false);
        SetCuttingboardGuidePointerActive(false);
    }

    /// <summary>
    /// 부엌 무대의 보조 담당 컴포넌트를 찾고 필요한 UI 참조를 넘깁니다.
    /// 감독이 모든 안내판을 직접 들지 않고, 인벤토리 연락 담당과 결과 연출 담당에게 역할표를 붙이는 단계입니다.
    /// </summary>
    private void ResolveRoleComponents()
    {
        Cue_Guide = Cue_Guide != null ? Cue_Guide : GetComponent<OOTechCookingGuideCue>();
        Bridge_Inventory = Bridge_Inventory != null ? Bridge_Inventory : GetComponent<OOTechCookingInventoryBridge>();
        Presenter_Result = Presenter_Result != null ? Presenter_Result : GetComponent<OOTechCookingResultPresenter>();

        if (Bridge_Inventory != null)
            Bridge_Inventory.RequestSetup(Text_InventoryNewBadge, _newBadgeBlinkSpeed, _newBadgeMinimumAlpha);
    }

    /// <summary>
    /// CookingUIGroup 루트 Canvas가 0 스케일이나 잘못된 Rect로 저장되었을 때 화면 전체 UI 기준으로 복구합니다.
    /// </summary>
    private void NormalizeCookingCanvasRoot()
    {
        if (Rect_Root == null)
            return;

        Rect_Root.localScale = Vector3.one;
        Rect_Root.anchorMin = Vector2.zero;
        Rect_Root.anchorMax = Vector2.one;
        Rect_Root.pivot = new Vector2(0.5f, 0.5f);
        Rect_Root.offsetMin = Vector2.zero;
        Rect_Root.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// CookingGroup 자식인 Cauldron 오브젝트의 Transform, Renderer, Collider를 찾습니다.
    /// </summary>
    private void ResolveCauldronReference()
    {
        if (Transform_Cauldron != null && Transform_Cauldron.gameObject.scene.IsValid())
            return;

        Transform_Cauldron = FindChildByName(transform, _cauldronObjectName);
        Renderer_Cauldron = null;
        Collider_Cauldron = null;

        if (Transform_Cauldron == null)
            return;

        Renderer_Cauldron = Transform_Cauldron.GetComponent<SpriteRenderer>();

        if (Renderer_Cauldron == null)
            Renderer_Cauldron = Transform_Cauldron.GetComponentInChildren<SpriteRenderer>(true);

        Collider_Cauldron = Transform_Cauldron.GetComponent<Collider2D>();

        if (Collider_Cauldron == null)
            Collider_Cauldron = Transform_Cauldron.GetComponentInChildren<Collider2D>(true);

        Tool_Cauldron = Transform_Cauldron.GetComponent<OOTechCookingToolDropTarget>();

        if (Tool_Cauldron == null)
            Tool_Cauldron = Transform_Cauldron.gameObject.AddComponent<OOTechCookingToolDropTarget>();

        OO_CookingTool toolData = _toolResolver.RequestSetupTool(Tool_Cauldron, _cauldronObjectName, "가마솥", _defaultCauldronAcceptedIngredientIdArray, _cauldronDropAreaPadding);

        if (toolData != null)
        {
            _cauldronDropAreaPadding = toolData.DropAreaPadding > 0f ? toolData.DropAreaPadding : _cauldronDropAreaPadding;

            if (!string.IsNullOrEmpty(toolData.GuideTutorialId))
                _cauldronTutorialId = toolData.GuideTutorialId;
        }
    }

    /// <summary>
    /// CookingGroup 자식 Cuttingboard 오브젝트의 Transform, Renderer, Collider, 역할표를 찾습니다.
    /// </summary>
    private void ResolveCuttingboardReference()
    {
        if (Transform_Cuttingboard != null && Transform_Cuttingboard.gameObject.scene.IsValid())
            return;

        Transform_Cuttingboard = FindChildByName(transform, _cuttingboardObjectName);
        Renderer_Cuttingboard = null;
        Collider_Cuttingboard = null;
        Tool_Cuttingboard = null;

        if (Transform_Cuttingboard == null)
            return;

        Renderer_Cuttingboard = Transform_Cuttingboard.GetComponent<SpriteRenderer>();

        if (Renderer_Cuttingboard == null)
            Renderer_Cuttingboard = Transform_Cuttingboard.GetComponentInChildren<SpriteRenderer>(true);

        Collider_Cuttingboard = Transform_Cuttingboard.GetComponent<Collider2D>();

        if (Collider_Cuttingboard == null)
            Collider_Cuttingboard = Transform_Cuttingboard.GetComponentInChildren<Collider2D>(true);

        Tool_Cuttingboard = Transform_Cuttingboard.GetComponent<OOTechCookingToolDropTarget>();

        if (Tool_Cuttingboard == null)
            Tool_Cuttingboard = Transform_Cuttingboard.gameObject.AddComponent<OOTechCookingToolDropTarget>();

        OO_CookingTool toolData = _toolResolver.RequestSetupTool(Tool_Cuttingboard, _cuttingboardObjectName, "도마", _defaultCuttingboardAcceptedIngredientIdArray, _cuttingboardDropAreaPadding);

        if (toolData != null)
        {
            _cuttingboardDropAreaPadding = toolData.DropAreaPadding > 0f ? toolData.DropAreaPadding : _cuttingboardDropAreaPadding;

            if (!string.IsNullOrEmpty(toolData.GuideTutorialId))
                _cuttingboardTutorialId = toolData.GuideTutorialId;
        }
    }

    /// <summary>
    /// CookingGroup 자식 Julgu 오브젝트의 Transform, Renderer, Collider, 역할표를 찾습니다.
    /// 절구는 Stage3부터 추가되는 조리도구이므로 없으면 경고 없이 기존 요리 흐름을 유지합니다.
    /// </summary>
    private void ResolveJulguReference()
    {
        if (Transform_Julgu != null && Transform_Julgu.gameObject.scene.IsValid())
            return;

        Transform_Julgu = FindChildByName(transform, _julguObjectName);
        Renderer_Julgu = null;
        Collider_Julgu = null;
        Tool_Julgu = null;

        if (Transform_Julgu == null)
            return;

        Renderer_Julgu = Transform_Julgu.GetComponent<SpriteRenderer>();

        if (Renderer_Julgu == null)
            Renderer_Julgu = Transform_Julgu.GetComponentInChildren<SpriteRenderer>(true);

        Collider_Julgu = Transform_Julgu.GetComponent<Collider2D>();

        if (Collider_Julgu == null)
            Collider_Julgu = Transform_Julgu.GetComponentInChildren<Collider2D>(true);

        Tool_Julgu = Transform_Julgu.GetComponent<OOTechCookingToolDropTarget>();

        if (Tool_Julgu == null)
            Tool_Julgu = Transform_Julgu.gameObject.AddComponent<OOTechCookingToolDropTarget>();

        OO_CookingTool toolData = _toolResolver.RequestSetupTool(Tool_Julgu, _julguObjectName, "절구", _defaultJulguAcceptedIngredientIdArray, _julguDropAreaPadding);

        if (toolData != null)
        {
            _julguDropAreaPadding = toolData.DropAreaPadding > 0f ? toolData.DropAreaPadding : _julguDropAreaPadding;

            if (!string.IsNullOrEmpty(toolData.GuideTutorialId))
                _julguTutorialId = toolData.GuideTutorialId;
        }
    }

    /// <summary>
    /// 화면 좌표를 실제 월드 좌표로 바꿔 가마솥 Collider 또는 Sprite Bounds 안인지 확인합니다.
    /// </summary>
    private bool IsPointerInsideSceneTool(Vector2 screenPosition, OOTechCookingToolDropTarget toolTarget, Transform toolTransform, SpriteRenderer toolRenderer, Collider2D toolCollider, float dropAreaPadding)
    {
        ResolveCameraReference();

        if (Camera_Main == null || toolTransform == null)
            return false;

        if (toolTarget != null && toolTarget.IsPointerInside(screenPosition, Camera_Main))
            return true;

        Vector3 worldPoint = GetWorldPointOnToolPlane(screenPosition, toolTransform);

        if (toolCollider != null && toolCollider.OverlapPoint(worldPoint))
            return true;

        if (toolRenderer == null)
            return false;

        Bounds bounds = toolRenderer.bounds;
        bounds.Expand(bounds.size * Mathf.Max(0f, dropAreaPadding - 1f));
        worldPoint.z = bounds.center.z;
        return bounds.Contains(worldPoint);
    }

    private Vector3 GetWorldPointOnToolPlane(Vector2 screenPosition, Transform toolTransform)
    {
        float planeDistance = Mathf.Abs(Camera_Main.transform.position.z - toolTransform.position.z);
        Vector3 worldPoint = Camera_Main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, planeDistance));
        worldPoint.z = toolTransform.position.z;
        return worldPoint;
    }

    private bool IsPointerInsideSceneCauldron(Vector2 screenPosition)
    {
        if (Transform_Cauldron == null)
            return false;

        ResolveCameraReference();

        if (Camera_Main == null)
            return false;

        Vector3 worldPoint = GetWorldPointOnCauldronPlane(screenPosition);

        if (Collider_Cauldron != null && Collider_Cauldron.OverlapPoint(worldPoint))
            return true;

        if (Renderer_Cauldron == null)
            return false;

        Bounds bounds = Renderer_Cauldron.bounds;
        bounds.Expand(bounds.size * Mathf.Max(0f, _cauldronDropAreaPadding - 1f));
        worldPoint.z = bounds.center.z;
        return bounds.Contains(worldPoint);
    }

    private Vector3 GetWorldPointOnCauldronPlane(Vector2 screenPosition)
    {
        float planeDistance = Mathf.Abs(Camera_Main.transform.position.z - Transform_Cauldron.position.z);
        Vector3 worldPoint = Camera_Main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, planeDistance));
        worldPoint.z = Transform_Cauldron.position.z;
        return worldPoint;
    }

    /// <summary>
    /// 실제 가마솥 크기를 화면 UI 좌표로 변환해 드롭 영역을 맞춥니다.
    /// </summary>
    private void UpdateCauldronDropArea()
    {
        if (Rect_Cauldron == null || Transform_Cauldron == null)
            return;

        ResolveCameraReference();

        if (Camera_Main == null || Rect_Root == null)
            return;

        Bounds bounds = GetCauldronBounds();
        Vector2 minLocalPoint;
        Vector2 maxLocalPoint;
        Vector3 minScreenPoint = Camera_Main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.center.z));
        Vector3 maxScreenPoint = Camera_Main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.center.z));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect_Root, minScreenPoint, null, out minLocalPoint);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect_Root, maxScreenPoint, null, out maxLocalPoint);

        Vector2 centerPoint = (minLocalPoint + maxLocalPoint) * 0.5f;
        Vector2 sizeDelta = new Vector2(Mathf.Abs(maxLocalPoint.x - minLocalPoint.x), Mathf.Abs(maxLocalPoint.y - minLocalPoint.y));
        sizeDelta *= Mathf.Max(1f, _cauldronDropAreaPadding);

        Rect_Cauldron.anchoredPosition = centerPoint;
        Rect_Cauldron.sizeDelta = new Vector2(Mathf.Max(180f, sizeDelta.x), Mathf.Max(120f, sizeDelta.y));
    }

    private void UpdateCuttingboardDropArea()
    {
        if (Rect_Cuttingboard == null || Transform_Cuttingboard == null)
            return;

        ResolveCameraReference();

        if (Camera_Main == null || Rect_Root == null)
            return;

        Bounds bounds = GetCuttingboardBounds();
        Vector2 minLocalPoint;
        Vector2 maxLocalPoint;
        Vector3 minScreenPoint = Camera_Main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.center.z));
        Vector3 maxScreenPoint = Camera_Main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.center.z));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect_Root, minScreenPoint, null, out minLocalPoint);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect_Root, maxScreenPoint, null, out maxLocalPoint);

        Vector2 centerPoint = (minLocalPoint + maxLocalPoint) * 0.5f;
        Vector2 sizeDelta = new Vector2(Mathf.Abs(maxLocalPoint.x - minLocalPoint.x), Mathf.Abs(maxLocalPoint.y - minLocalPoint.y));
        sizeDelta *= Mathf.Max(1f, _cuttingboardDropAreaPadding);

        Rect_Cuttingboard.anchoredPosition = centerPoint;
        Rect_Cuttingboard.sizeDelta = new Vector2(Mathf.Max(180f, sizeDelta.x), Mathf.Max(120f, sizeDelta.y));
    }

    private Bounds GetCauldronBounds()
    {
        if (Collider_Cauldron != null)
            return Collider_Cauldron.bounds;

        if (Renderer_Cauldron != null)
            return Renderer_Cauldron.bounds;

        return new Bounds(Transform_Cauldron.position, Vector3.one);
    }

    private Bounds GetCuttingboardBounds()
    {
        if (Tool_Cuttingboard != null)
            return Tool_Cuttingboard.GetWorldBounds();

        if (Collider_Cuttingboard != null)
            return Collider_Cuttingboard.bounds;

        if (Renderer_Cuttingboard != null)
            return Renderer_Cuttingboard.bounds;

        return new Bounds(Transform_Cuttingboard.position, Vector3.one);
    }

    private Bounds GetJulguBounds()
    {
        if (Tool_Julgu != null)
            return Tool_Julgu.GetWorldBounds();

        if (Collider_Julgu != null)
            return Collider_Julgu.bounds;

        if (Renderer_Julgu != null)
            return Renderer_Julgu.bounds;

        return new Bounds(Transform_Julgu != null ? Transform_Julgu.position : transform.position, Vector3.one);
    }

    /// <summary>
    /// 로드 HUD의 인벤토리 패널을 다시 그리도록 요청합니다.
    /// </summary>
    private void RefreshInventorySlots()
    {
        NotifyRoadHUDInventoryRefresh();
    }

    /// <summary>
    /// 가마솥 안에 들어간 재료 목록을 텍스트로 보여줍니다.
    /// </summary>
    private void RefreshPotView()
    {
        if (Text_Pot == null)
            return;

        if (_selectionModel.SelectedIngredientIdList.Count == 0)
        {
            Text_Pot.text = string.Empty;
            Text_Pot.gameObject.SetActive(false);
            return;
        }

        Text_Pot.gameObject.SetActive(true);
        string text = string.Empty;

        foreach (string ingredientId in _selectionModel.SelectedIngredientIdList)
        {
            int amount = Mathf.Max(1, GetSelectedIngredientAmount(ingredientId));
            text += $"{GetItemDisplayName(ingredientId)} x{amount}\n";
        }

        Text_Pot.text = text.TrimEnd();
    }

    /// <summary>
    /// 처음 부엌에 들어왔을 때 가마솥 사용법 말풍선과 화살표를 보여줍니다.
    /// </summary>
    private void ShowToolGuideSequence()
    {
        StopToolGuideRoutine();
        _isToolGuideComplete = false;
        _isJulguGuideActive = false;
        _toolGuideCoroutine = StartCoroutine(PlayToolGuideSequenceRoutine());
    }

    private void ShowJulguGuideSequence()
    {
        StopToolGuideRoutine();
        _isToolGuideComplete = false;
        _isJulguGuideActive = true;
        _toolGuideCoroutine = StartCoroutine(PlayJulguGuideRoutine());
    }

    private IEnumerator PlayToolGuideSequenceRoutine()
    {
        bool isCauldronGuideDone = false;
        ShowCauldronGuide();
        _guideConfirmAction = delegate
        {
            isCauldronGuideDone = true;
        };

        yield return new WaitUntil(() => isCauldronGuideDone);

        bool isCuttingboardGuideDone = false;
        ShowCuttingboardGuide(delegate
        {
            isCuttingboardGuideDone = true;
        });

        yield return new WaitUntil(() => isCuttingboardGuideDone);

        _isToolGuideComplete = true;
        _isSharedToolGuideCompleted = true;
        _toolGuideCoroutine = null;
        SetStatus(_defaultStatusText);
    }

    /// <summary>
    /// Stage3에서 새로 열린 절구 사용법만 단독으로 안내합니다.
    /// Game View에서는 기존 안내 말풍선을 재사용하고 화살표만 Julgu 위로 옮깁니다.
    /// </summary>
    private IEnumerator PlayJulguGuideRoutine()
    {
        bool isJulguGuideDone = false;
        ShowJulguGuide(delegate
        {
            isJulguGuideDone = true;
        });

        yield return new WaitUntil(() => isJulguGuideDone);

        _isToolGuideComplete = true;
        _isSharedJulguGuideCompleted = true;
        _isJulguGuideActive = false;
        _toolGuideCoroutine = null;
        SetGuidePointerActive(false);
        SetStatus("절구를 클릭하거나 더블 클릭하면 쌀 1개가 떡 1개로 바뀝니다.");
    }

    private void StopToolGuideRoutine()
    {
        if (_toolGuideCoroutine == null)
            return;

        StopCoroutine(_toolGuideCoroutine);
        _toolGuideCoroutine = null;
    }

    private void ShowCauldronGuide()
    {
        _guideConfirmAction = null;
        GetCauldronGuideData(out string title, out string description);

        if (Root_GuideBubble == null)
        {
            Debug.LogWarning("[OOTechCookingGroupController] Panel_CauldronGuide is missing from CookingUIGroup.");
            return;
        }

        ApplyGuideText(title, description);
        Root_GuideBubble.SetActive(true);

        if (Button_GuideConfirm != null)
        {
            Button_GuideConfirm.onClick.RemoveAllListeners();
            Button_GuideConfirm.onClick.AddListener(delegate
            {
                Root_GuideBubble.SetActive(false);
                SetGuidePointerActive(false);
                UnityAction guideConfirmAction = _guideConfirmAction;
                _guideConfirmAction = null;
                guideConfirmAction?.Invoke();
            });
        }

        CreateGuidePointersIfNeeded();
    }

    /// <summary>
    /// Tutorial 데이터에서 가마솥 설명을 읽고, 없으면 기본 안내 문구를 사용합니다.
    /// </summary>
    private void ShowCuttingboardGuide(UnityAction onConfirm)
    {
        GetCuttingboardGuideData(out string title, out string description);

        if (Root_CuttingboardGuideBubble == null)
        {
            Debug.LogWarning("[OOTechCookingGroupController] Panel_CuttingboardGuide is missing from CookingUIGroup.");
            onConfirm?.Invoke();
            return;
        }

        ApplyCuttingboardGuideText(title, description);
        Root_CuttingboardGuideBubble.SetActive(true);
        SetCuttingboardGuidePointerActive(true);
        UpdateGuideArrowLayout();

        if (Button_CuttingboardGuideConfirm != null)
        {
            Button_CuttingboardGuideConfirm.onClick.RemoveAllListeners();
            Button_CuttingboardGuideConfirm.onClick.AddListener(delegate
            {
                HideCuttingboardGuideBubble();
                onConfirm?.Invoke();
            });
        }
    }

    private void ShowJulguGuide(UnityAction onConfirm)
    {
        GetJulguGuideData(out string title, out string description);

        if (Root_GuideBubble == null)
        {
            Debug.LogWarning("[OOTechCookingGroupController] Panel_CauldronGuide is missing, Julgu guide is skipped.");
            onConfirm?.Invoke();
            return;
        }

        ApplyGuideText(title, description);
        Root_GuideBubble.SetActive(true);
        SetGuidePointerActive(true);
        UpdateGuideArrowLayout();

        if (Button_GuideConfirm != null)
        {
            Button_GuideConfirm.onClick.RemoveAllListeners();
            Button_GuideConfirm.onClick.AddListener(delegate
            {
                HideGuideBubble();
                onConfirm?.Invoke();
            });
        }
    }

    private void GetCauldronGuideData(out string title, out string description)
    {
        string fallbackDescription = "왼쪽 인벤토리의 쌀과 채소를 가운데 가마솥과 도마로 끌어다 놓으세요.\n기본은 하나씩 집습니다. 여러 개를 집으려면 슬롯 위에서 Ctrl+마우스 휠로 수량을 조절하세요.\n쌀 + 채소가 준비되면 야채죽이 완성됩니다.";

        if (Cue_Guide != null)
        {
            Cue_Guide.RequestGetGuideData(_cauldronTutorialId, "가마솥", fallbackDescription, out title, out description);
            return;
        }

        title = "가마솥";
        description = fallbackDescription;
    }

    /// <summary>
    /// 말풍선 제목과 본문 텍스트를 적용합니다.
    /// </summary>
    private void GetCuttingboardGuideData(out string title, out string description)
    {
        string fallbackDescription = "채소는 도마에 올려놓으세요.\n가마솥에 쌀, 도마에 채소가 준비되면 야채죽이 완성됩니다.";

        if (Cue_Guide != null)
        {
            Cue_Guide.RequestGetGuideData(_cuttingboardTutorialId, "도마", fallbackDescription, out title, out description);
            return;
        }

        title = "도마";
        description = fallbackDescription;
    }

    private void GetJulguGuideData(out string title, out string description)
    {
        string fallbackDescription = "절구는 쌀을 떡으로 만드는 독립 조리도구입니다. 절구를 클릭하거나 더블 클릭하면 쌀 1개가 떡 1개로 바뀝니다.";

        if (Cue_Guide != null)
        {
            Cue_Guide.RequestGetGuideData(_julguTutorialId, "절구", fallbackDescription, out title, out description);
            return;
        }

        title = "절구";
        description = fallbackDescription;
    }

    private void ApplyGuideText(string title, string description)
    {
        if (Root_GuideBubble != null)
            Root_GuideBubble.SetActive(true);

        if (Text_GuideTitle != null)
            Text_GuideTitle.text = title;

        if (Text_GuideBody != null)
            Text_GuideBody.text = description;
    }

    private void ApplyCuttingboardGuideText(string title, string description)
    {
        if (Root_CuttingboardGuideBubble != null)
            Root_CuttingboardGuideBubble.SetActive(true);

        if (Text_CuttingboardGuideTitle != null)
            Text_CuttingboardGuideTitle.text = title;

        if (Text_CuttingboardGuideBody != null)
            Text_CuttingboardGuideBody.text = description;
    }

    /// <summary>
    /// 요리 성공처럼 다음 컷으로 바로 넘어가야 할 때 현재 안내 말풍선과 화살표를 정리합니다.
    /// </summary>
    private void HideGuideBubble()
    {
        _guideConfirmAction = null;

        if (Root_GuideBubble != null)
            Root_GuideBubble.SetActive(false);

        if (Root_CuttingboardGuideBubble != null)
            Root_CuttingboardGuideBubble.SetActive(false);

        _isJulguGuideActive = false;
        SetGuidePointerActive(false);
        SetCuttingboardGuidePointerActive(false);
    }

    private void HideCuttingboardGuideBubble()
    {
        if (Root_CuttingboardGuideBubble != null)
            Root_CuttingboardGuideBubble.SetActive(false);

        SetCuttingboardGuidePointerActive(false);
    }

    /// <summary>
    /// 씬에 배치된 가마솥 화살표만 켜고 위치를 갱신합니다.
    /// </summary>
    private void CreateGuidePointersIfNeeded()
    {
        if (Rect_Root == null)
            return;

        if (Root_CauldronGuideArrow == null)
        {
            Debug.LogWarning("[OOTechCookingGroupController] Text_CauldronGuideArrow is missing from CookingUIGroup.");
            return;
        }

        SetGuidePointerActive(true);
        UpdateGuideArrowLayout();
    }

    private void SetGuidePointerActive(bool isActive)
    {
        if (Root_InventoryGuideArrow != null)
            Root_InventoryGuideArrow.SetActive(false);

        if (Root_CauldronGuideArrow != null)
        {
            Root_CauldronGuideArrow.SetActive(isActive);

            if (isActive)
                StartGuideArrowBlink(Root_CauldronGuideArrow, ref _cauldronArrowBlinkCoroutine);
            else
                StopGuideArrowBlink(Root_CauldronGuideArrow, ref _cauldronArrowBlinkCoroutine);
        }
    }

    private void SetCuttingboardGuidePointerActive(bool isActive)
    {
        if (Root_CuttingboardGuideArrow == null)
            return;

        Root_CuttingboardGuideArrow.SetActive(isActive);

        if (isActive)
            StartGuideArrowBlink(Root_CuttingboardGuideArrow, ref _cuttingboardArrowBlinkCoroutine);
        else
            StopGuideArrowBlink(Root_CuttingboardGuideArrow, ref _cuttingboardArrowBlinkCoroutine);
    }

    private void UpdateGuideArrowLayout()
    {
        if (Root_CauldronGuideArrow != null && _isJulguGuideActive && TryGetJulguTopLocalPoint(out Vector2 julguTopLocalPoint))
        {
            RectTransform julguArrowRect = Root_CauldronGuideArrow.transform as RectTransform;

            if (julguArrowRect != null)
            {
                julguArrowRect.anchorMin = new Vector2(0.5f, 0.5f);
                julguArrowRect.anchorMax = new Vector2(0.5f, 0.5f);
                julguArrowRect.pivot = new Vector2(0.5f, 0.5f);
                julguArrowRect.anchoredPosition = julguTopLocalPoint + new Vector2(0f, 62f);
            }
        }
        else if (Root_CauldronGuideArrow != null && TryGetCauldronTopLocalPoint(out Vector2 cauldronTopLocalPoint))
        {
            RectTransform cauldronArrowRect = Root_CauldronGuideArrow.transform as RectTransform;

            if (cauldronArrowRect != null)
            {
                cauldronArrowRect.anchorMin = new Vector2(0.5f, 0.5f);
                cauldronArrowRect.anchorMax = new Vector2(0.5f, 0.5f);
                cauldronArrowRect.pivot = new Vector2(0.5f, 0.5f);
                cauldronArrowRect.anchoredPosition = cauldronTopLocalPoint + new Vector2(0f, 62f);
            }
        }
        else if (Root_CauldronGuideArrow != null && Rect_Cauldron != null)
        {
            RectTransform cauldronArrowRect = Root_CauldronGuideArrow.transform as RectTransform;

            if (cauldronArrowRect != null)
                cauldronArrowRect.anchoredPosition = Rect_Cauldron.anchoredPosition + new Vector2(0f, Rect_Cauldron.sizeDelta.y * 0.5f + 72f);
        }

        if (Root_CuttingboardGuideArrow != null && TryGetCuttingboardTopLocalPoint(out Vector2 cuttingboardTopLocalPoint))
        {
            RectTransform cuttingboardArrowRect = Root_CuttingboardGuideArrow.transform as RectTransform;

            if (cuttingboardArrowRect != null)
            {
                cuttingboardArrowRect.anchorMin = new Vector2(0.5f, 0.5f);
                cuttingboardArrowRect.anchorMax = new Vector2(0.5f, 0.5f);
                cuttingboardArrowRect.pivot = new Vector2(0.5f, 0.5f);
                cuttingboardArrowRect.anchoredPosition = cuttingboardTopLocalPoint + new Vector2(0f, 62f);
            }
        }
        else if (Root_CuttingboardGuideArrow != null && Rect_Cuttingboard != null)
        {
            RectTransform cuttingboardArrowRect = Root_CuttingboardGuideArrow.transform as RectTransform;

            if (cuttingboardArrowRect != null)
                cuttingboardArrowRect.anchoredPosition = Rect_Cuttingboard.anchoredPosition + new Vector2(0f, Rect_Cuttingboard.sizeDelta.y * 0.5f + 72f);
        }
    }

    /// <summary>
    /// 실제 Cauldron 오브젝트의 윗부분을 UI 로컬 좌표로 바꿉니다.
    /// 화살표가 드롭 영역이 아니라 가마솥 소품 바로 위를 가리키게 하는 기준점입니다.
    /// </summary>
    private bool TryGetCauldronTopLocalPoint(out Vector2 localPoint)
    {
        localPoint = Vector2.zero;

        if (Transform_Cauldron == null || Rect_Root == null)
            return false;

        ResolveCameraReference();

        if (Camera_Main == null)
            return false;

        Bounds bounds = GetCauldronBounds();
        Vector3 worldPoint = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        Vector3 screenPoint = Camera_Main.WorldToScreenPoint(worldPoint);
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect_Root, screenPoint, null, out localPoint);
    }

    private bool TryGetJulguTopLocalPoint(out Vector2 localPoint)
    {
        localPoint = Vector2.zero;

        if (Transform_Julgu == null || Rect_Root == null)
            return false;

        ResolveCameraReference();

        if (Camera_Main == null)
            return false;

        Bounds bounds = GetJulguBounds();
        Vector3 worldPoint = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        Vector3 screenPoint = Camera_Main.WorldToScreenPoint(worldPoint);
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect_Root, screenPoint, null, out localPoint);
    }

    private bool TryGetCuttingboardTopLocalPoint(out Vector2 localPoint)
    {
        localPoint = Vector2.zero;

        if (Transform_Cuttingboard == null || Rect_Root == null)
            return false;

        ResolveCameraReference();

        if (Camera_Main == null)
            return false;

        Bounds bounds = GetCuttingboardBounds();
        Vector3 worldPoint = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        Vector3 screenPoint = Camera_Main.WorldToScreenPoint(worldPoint);
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect_Root, screenPoint, null, out localPoint);
    }

    private void StartGuideArrowBlink(GameObject arrowObject, ref Coroutine blinkCoroutine)
    {
        if (arrowObject == null || blinkCoroutine != null || !gameObject.activeInHierarchy)
            return;

        TextMeshProUGUI arrowText = arrowObject.GetComponent<TextMeshProUGUI>();

        if (arrowText == null)
            return;

        blinkCoroutine = StartCoroutine(BlinkGuideArrowRoutine(arrowText));
    }

    private IEnumerator BlinkGuideArrowRoutine(TextMeshProUGUI arrowText)
    {
        while (arrowText != null && arrowText.gameObject.activeSelf)
        {
            Color arrowColor = arrowText.color;
            float wave = (Mathf.Sin(Time.unscaledTime * _guideArrowBlinkSpeed) + 1f) * 0.5f;
            arrowColor.a = Mathf.Lerp(_guideArrowMinimumAlpha, 1f, wave);
            arrowText.color = arrowColor;
            yield return null;
        }
    }

    private void StopGuideArrowBlink(GameObject arrowObject, ref Coroutine blinkCoroutine)
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (arrowObject == null)
            return;

        TextMeshProUGUI arrowText = arrowObject.GetComponent<TextMeshProUGUI>();

        if (arrowText == null)
            return;

        Color arrowColor = arrowText.color;
        arrowColor.a = 1f;
        arrowText.color = arrowColor;
    }

    /// <summary>
    /// 부엌 안의 NEW 배지를 켜거나 끄고, 켜질 때는 깜빡이게 합니다.
    /// </summary>
    private void SetInventoryNewBadgeActive(bool isActive)
    {
        if (Bridge_Inventory != null)
            Bridge_Inventory.RequestSetInventoryNewBadgeActive(isActive);
    }

    private void StopInventoryNewBadgeBlink()
    {
        if (Bridge_Inventory != null)
            Bridge_Inventory.RequestStopInventoryNewBadgeBlink();
    }

    /// <summary>
    /// 열려 있는 Road HUD들에게 인벤토리 NEW 배지를 켜 달라고 알립니다.
    /// </summary>
    private void NotifyRoadHUDInventoryNewBadge()
    {
        if (Bridge_Inventory != null)
        {
            Bridge_Inventory.RequestNotifyRoadHUDInventoryNewBadge();
            return;
        }

        foreach (OOTechRoadHUDController hudController in FindObjectsByType<OOTechRoadHUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (hudController == null || !hudController.gameObject.activeInHierarchy)
                continue;

            hudController.RequestRefreshInventoryView();
            hudController.SetInventoryNewBadgeActive(true);
        }
    }

    /// <summary>
    /// 열려 있는 Road HUD들에게 인벤토리 목록을 다시 그려 달라고 알립니다.
    /// </summary>
    private void NotifyRoadHUDInventoryRefresh()
    {
        if (Bridge_Inventory != null)
        {
            Bridge_Inventory.RequestNotifyRoadHUDInventoryRefresh();
            return;
        }

        foreach (OOTechRoadHUDController hudController in FindObjectsByType<OOTechRoadHUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (hudController == null || !hudController.gameObject.activeInHierarchy)
                continue;

            hudController.RequestRefreshInventoryView();
        }
    }

    /// <summary>
    /// 야채죽 제작이 끝났다고 Road HUD에 알려 임무판 체크 표시를 갱신합니다.
    /// </summary>
    private void NotifyRoadHUDCookingQuestComplete()
    {
        if (Bridge_Inventory != null)
            Bridge_Inventory.RequestNotifyRoadHUDCookingQuestComplete();
    }

    /// <summary>
    /// 합창 대사가 끝난 뒤 플레이어에게 임무가 완료되었음을 짧게 안내합니다.
    /// </summary>
    private void ShowCookingMissionCompleteGuide()
    {
        ApplyGuideText("임무 완수", "야채죽을 완성했습니다.\n임무 UI에 완료 표시가 추가되었습니다.");
        SetGuidePointerActive(false);

        if (Button_GuideConfirm == null)
            return;

        Button_GuideConfirm.onClick.RemoveAllListeners();
        Button_GuideConfirm.onClick.AddListener(delegate
        {
            HideGuideBubble();
        });
    }

    /// <summary>
    /// 요리 완성 후 세 인물이 함께 말하는 DialogueGroup 대사를 시작합니다.
    /// </summary>
    private void RequestPlayCookingCompleteDialogue(string resultItemId)
    {
        if (Presenter_Result != null)
            Presenter_Result.RequestPlayCookingCompleteDialogue(resultItemId, ConsumeCookingCompleteResult, CompleteCookingTutorialMission);
    }

    /// <summary>
    /// 첫 요리 튜토리얼의 완성 음식은 대사로 먹은 뒤 인벤토리에서 1개 제거합니다.
    /// Game View에서는 "잘 먹었습니다" 대사가 끝난 다음 야채죽 슬롯이 사라집니다.
    /// </summary>
    private void ConsumeCookingCompleteResult(string resultItemId)
    {
        if (OOTechGameManager.Inst == null)
            return;

        if (!OOTechGameManager.Inst.RemoveItem(resultItemId, 1))
            return;

        RefreshInventorySlots();
        NotifyRoadHUDInventoryRefresh();
        Debug.Log($"[OOTechCookingGroupController] Consumed cooked result after dialogue: {resultItemId}");
    }

    private void CompleteCookingTutorialMission()
    {
        NotifyRoadHUDCookingQuestComplete();
        ShowCookingMissionCompleteGuide();
    }

    /// <summary>
    /// 부엌 카메라가 Kitchen 배경 전체를 담도록 Orthographic Size와 위치를 조정합니다.
    /// </summary>
    private void ApplyKitchenCameraView()
    {
        ResolveCameraReference();

        if (Camera_Main == null)
            return;

        SpriteRenderer kitchenRenderer = FindChildRenderer("Kitchen");

        if (kitchenRenderer == null)
            return;

        SaveCameraStateIfNeeded();

        if (Camera_Follow != null)
            Camera_Follow.enabled = false;

        Bounds bounds = kitchenRenderer.bounds;
        float aspect = Camera_Main.aspect > 0f ? Camera_Main.aspect : 16f / 9f;
        float containSize = Mathf.Max(bounds.extents.y, bounds.extents.x / aspect) * _cameraPadding;
        float coverSize = Mathf.Min(bounds.extents.y, bounds.extents.x / aspect) * _kitchenCoverPadding;
        float targetSize = _isCoverKitchenScreen ? coverSize : containSize;
        Vector3 cameraPosition = bounds.center;
        cameraPosition.z = Camera_Main.transform.position.z;

        Camera_Main.orthographic = true;
        Camera_Main.orthographicSize = Mathf.Max(0.1f, targetSize);
        Camera_Main.transform.position = cameraPosition;
    }

    /// <summary>
    /// CookingGroup이 열릴 때 부엌 무대와 조리도구 SpriteRenderer를 가장 앞쪽으로 올립니다.
    /// Game View에서는 이전 Road/Stage 배경이 뒤에 남아 있어도 Kitchen 화면이 덮이지 않게 하는 보험 장치입니다.
    /// </summary>
    private void ApplyCookingRenderPriority()
    {
        SpriteRenderer[] rendererArray = GetComponentsInChildren<SpriteRenderer>(true);

        for (int index = 0; index < rendererArray.Length; index++)
        {
            SpriteRenderer spriteRenderer = rendererArray[index];

            if (spriteRenderer == null || !spriteRenderer.gameObject.activeInHierarchy)
                continue;

            ApplyCookingSortingLayer(spriteRenderer);

            if (spriteRenderer.name == "Kitchen")
                spriteRenderer.sortingOrder = Mathf.Max(spriteRenderer.sortingOrder, _kitchenRendererSortingOrder);
            else
                spriteRenderer.sortingOrder = Mathf.Max(spriteRenderer.sortingOrder, _toolRendererSortingOrder);
        }
    }

    private void ApplyCookingSortingLayer(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null || string.IsNullOrEmpty(_cookingSpriteSortingLayerName))
            return;

        int sortingLayerId = SortingLayer.NameToID(_cookingSpriteSortingLayerName);

        if (sortingLayerId == 0 && _cookingSpriteSortingLayerName != "Default")
            return;

        spriteRenderer.sortingLayerName = _cookingSpriteSortingLayerName;
    }

    /// <summary>
    /// 부엌을 닫을 때 이전 로드 장면의 카메라 위치와 Follow 상태로 복구합니다.
    /// </summary>
    private void RestoreCameraView()
    {
        if (!_hasSavedCameraState || Camera_Main == null)
            return;

        Camera_Main.transform.position = _savedCameraPosition;
        Camera_Main.orthographicSize = _savedOrthographicSize;
        Camera_Main.orthographic = _savedOrthographic;

        if (Camera_Follow != null)
            Camera_Follow.enabled = _savedFollowEnabled;

        _hasSavedCameraState = false;
    }

    /// <summary>
    /// 카메라를 부엌용으로 바꾸기 전에 원래 상태를 한 번 저장합니다.
    /// </summary>
    private void SaveCameraStateIfNeeded()
    {
        if (_hasSavedCameraState || Camera_Main == null)
            return;

        _savedCameraPosition = Camera_Main.transform.position;
        _savedOrthographicSize = Camera_Main.orthographicSize;
        _savedOrthographic = Camera_Main.orthographic;
        _savedFollowEnabled = Camera_Follow != null && Camera_Follow.enabled;
        _hasSavedCameraState = true;
    }

    private void ResolveCameraReference()
    {
        if (Camera_Main == null)
            Camera_Main = Camera.main;

        if (Camera_Follow == null && Camera_Main != null)
            Camera_Main.TryGetComponent(out Camera_Follow);
    }

    private SpriteRenderer FindChildRenderer(string childName)
    {
        Transform childTransform = FindChildByName(transform, childName);

        if (childTransform != null && childTransform.TryGetComponent(out SpriteRenderer childRenderer))
            return childRenderer;

        return GetComponentInChildren<SpriteRenderer>(true);
    }

    private Transform FindChildByName(Transform rootTransform, string childName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == childName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = FindChildByName(rootTransform.GetChild(index), childName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
    }

    private GameObject FindSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return null;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            Transform foundTransform = FindChildByName(rootObject.transform, objectName);

            if (foundTransform != null)
                return foundTransform.gameObject;
        }

        return null;
    }

    private string GetItemDisplayName(string itemDataId)
    {
        if (OOTechItemCatalogManager.Inst != null)
            return OOTechItemCatalogManager.Inst.GetItemDisplayName(itemDataId);

        if (OOTechGameDataManager.Inst != null)
        {
            OOTechGameDataManager.Inst.TryGetIngredientData(itemDataId, out OO_Ingredient ingredientData);

            if (ingredientData != null && !string.IsNullOrEmpty(ingredientData.Name))
                return ingredientData.Name;

            OOTechGameDataManager.Inst.TryGetCookData(itemDataId, out OO_Cook cookData);

            if (cookData != null && !string.IsNullOrEmpty(cookData.Name))
                return cookData.Name;
        }

        if (itemDataId == _riceIngredientId)
            return "쌀";

        if (itemDataId == _vegetableIngredientId)
            return "채소";

        if (itemDataId == _pumpkinIngredientId)
            return "호박";

        if (itemDataId == "Ing_ChiliPepper_01")
            return "청양고추";

        if (itemDataId == "Ing_Fish_01")
            return "조기";

        if (itemDataId == "Ing_Kimch_01")
            return "김치";

        if (itemDataId == "OO_PumpkinSoup_1")
            return "호박죽";

        if (itemDataId == "OO_VegetableSoup_1")
            return "야채죽";

        if (itemDataId == "OO_GrilledFishMeal_1")
            return "조기밥상";

        if (itemDataId == "OO_KimchiStew_1")
            return "김치찌개";

        return string.IsNullOrEmpty(itemDataId) ? "알 수 없는 아이템" : itemDataId;
    }

    /// <summary>
    /// 부엌 하단 상태 문구를 갱신합니다.
    /// </summary>
    public void RequestShowCookingStatus(string statusText)
    {
        SetStatus(statusText);
    }

    private void SetStatus(string statusText)
    {
        if (Text_Status != null)
            Text_Status.text = statusText;
    }
}
