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
using UnityEngine.UI;

/// <summary>
/// CookingGroup의 요리 튜토리얼, 가마솥 드래그 판정, 완성 대화를 지휘합니다.
/// 부엌 UI 소품은 CookingUIGroup에 배치하고, 이 컨트롤러는 재료가 들어가는 순서만 관리합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechCookingGroupController : MonoBehaviour
{
    private static bool _isSharedToolGuideCompleted;

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
    [SerializeField] private string _cauldronTutorialId = "narration_tutorial_11";
    [SerializeField] private string _cuttingboardTutorialId = "narration_tutorial_12";
    [SerializeField] private string _moranCookingCompleteDialogueId = "character_Moran_05";
    [SerializeField] private string _cookingCompleteDialogueGroupId = "dialogue_group_cooking_vegetable_porridge_complete_01";
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";

    [Header("Scene Role")]
    [SerializeField] private string _cauldronObjectName = "Cauldron";
    [SerializeField] private string _cuttingboardObjectName = "Cuttingboard";
    [SerializeField] private float _cauldronDropAreaPadding = 1.18f;
    [SerializeField] private float _cuttingboardDropAreaPadding = 1.18f;

    [Header("Ingredient Rule")]
    [SerializeField] private string _riceIngredientId = "Ing_Rice_01";
    [SerializeField] private string _vegetableIngredientId = "Ing_Veggie_01";
    [SerializeField] private string _pumpkinIngredientId = "Ing_Pumpkin_01";
    [SerializeField] private string[] _defaultCauldronAcceptedIngredientIdArray = { "Ing_Rice_01" };
    [SerializeField] private string[] _defaultCuttingboardAcceptedIngredientIdArray = { "Ing_Veggie_01", "Ing_Pumpkin_01" };

    [Header("Canvas")]
    [SerializeField] private int _sortingOrder = 1260;
    [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);

    [Header("Camera")]
    [SerializeField] private Camera Camera_Main;
    [SerializeField] private float _cameraPadding = 1.04f;

    [Header("New Badge")]
    [SerializeField] private float _newBadgeBlinkSpeed = 7f;
    [SerializeField] private float _newBadgeMinimumAlpha = 0.25f;

    [Header("Guide Arrow")]
    [SerializeField] private float _guideArrowBlinkSpeed = 6f;
    [SerializeField] private float _guideArrowMinimumAlpha = 0.25f;

    [Header("Drag Ghost")]
    [SerializeField] private Vector2 _dragGhostIconSize = new Vector2(88f, 88f);

    private readonly List<string> _selectedIngredientIdList = new List<string>();

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
    private SpriteRenderer Renderer_Cauldron;
    private SpriteRenderer Renderer_Cuttingboard;
    private Collider2D Collider_Cauldron;
    private Collider2D Collider_Cuttingboard;
    private OOTechCookingToolDropTarget Tool_Cauldron;
    private OOTechCookingToolDropTarget Tool_Cuttingboard;
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
    private OOTechCookingGroupView View_Cooking;
    private GameObject Group_Dialogue;
    private DialogueUI UI_Dialogue;
    private Coroutine _inventoryNewBadgeCoroutine;
    private Coroutine _toolGuideCoroutine;
    private Coroutine _cauldronArrowBlinkCoroutine;
    private Coroutine _cuttingboardArrowBlinkCoroutine;
    private UnityAction _guideConfirmAction;
    private bool _isToolGuideComplete;
    private bool _isCookingCompleteDialoguePlaying;
    private string _pendingCookingCompleteResultItemId;

    /// <summary>
    /// 부엌 무대가 열리면 카메라를 Kitchen 배경에 맞추고 가마솥 가이드를 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        NormalizeCookingGroupTransformIfNeeded();
        EnsureCookingManager();
        ResolveCauldronReference();
        ResolveCuttingboardReference();
        ApplyKitchenCameraView();
        PrepareCookingView();
        RefreshInventorySlots();
        RefreshPotView();

        if (_isSharedToolGuideCompleted)
        {
            _isToolGuideComplete = true;
            HideGuideBubble();
            SetStatus("재료를 알맞은 조리도구에 올려 요리하세요.");
        }
        else
        {
            ShowToolGuideSequence();
        }
    }

    /// <summary>
    /// 매 프레임 UI 드롭 영역과 화살표가 실제 가마솥 위치를 따라가도록 보정합니다.
    /// </summary>
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
        _isCookingCompleteDialoguePlaying = false;
        _pendingCookingCompleteResultItemId = string.Empty;
        _isToolGuideComplete = false;
        StopToolGuideRoutine();
        StopGuideArrowBlink(Root_CauldronGuideArrow, ref _cauldronArrowBlinkCoroutine);
        StopGuideArrowBlink(Root_CuttingboardGuideArrow, ref _cuttingboardArrowBlinkCoroutine);
        StopInventoryNewBadgeBlink();
        CloseDialogueGroup();
        RestoreCameraView();
    }

    /// <summary>
    /// 플레이어가 재료 슬롯을 드래그할 때 손에 든 것처럼 보이는 임시 잔상 UI를 만듭니다.
    /// </summary>
    public RectTransform CreateDragGhost(string itemDataId, Vector2 screenPosition)
    {
        if (Rect_Root == null)
            return null;

        if (Rect_DragGhostTemplate == null)
        {
            Debug.LogWarning("[OOTechCookingGroupController] Slot_DragGhostTemplate is missing from CookingUIGroup.");
            return null;
        }

        GameObject ghostObject = Instantiate(Rect_DragGhostTemplate.gameObject, Rect_Root, false);
        ghostObject.name = "Image_DragGhost";
        ghostObject.SetActive(true);
        RectTransform ghostRect = ghostObject.transform as RectTransform;
        ghostRect.position = screenPosition;

        Transform iconTransform = FindChildByName(ghostObject.transform, "Image_ItemIcon");
        Image iconImage = iconTransform != null ? iconTransform.GetComponent<Image>() : null;

        // 드래그 고스트는 슬롯 전체가 아니라, 배우가 손에 든 음식 소품처럼 아이콘만 보이게 합니다.
        foreach (TextMeshProUGUI ghostText in ghostObject.GetComponentsInChildren<TextMeshProUGUI>(true))
            ghostText.gameObject.SetActive(false);

        foreach (Image ghostImage in ghostObject.GetComponentsInChildren<Image>(true))
        {
            if (ghostImage == iconImage)
                continue;

            ghostImage.enabled = false;
            ghostImage.raycastTarget = false;
        }

        if (iconImage != null)
        {
            Sprite iconSprite = OOTechItemCatalogManager.RequestItemIconSprite(itemDataId);
            iconImage.gameObject.SetActive(true);
            iconImage.sprite = iconSprite;
            iconImage.enabled = iconSprite != null;
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            if (iconImage.transform is RectTransform iconRect)
            {
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
            }
        }

        if (ghostRect != null)
        {
            ghostRect.sizeDelta = _dragGhostIconSize;
            ghostRect.position = screenPosition;
            ghostRect.SetAsLastSibling();
        }

        CanvasGroup canvasGroup = ghostObject.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

        return ghostRect;
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
    /// 재료를 놓은 화면 좌표가 어느 조리도구 위인지 판정하고, 맞는 역할표에만 투입합니다.
    /// </summary>
    public void RequestDropIngredientAtPosition(string itemDataId, Vector2 screenPosition)
    {
        if (!_isToolGuideComplete)
        {
            SetStatus("조리도구 안내를 확인한 뒤 재료를 넣어주세요.");
            return;
        }

        if (IsPointerInsideCauldron(screenPosition))
        {
            RequestDropIngredientToTool(itemDataId, Tool_Cauldron, _cauldronObjectName, "가마솥");
            return;
        }

        if (IsPointerInsideCuttingboard(screenPosition))
        {
            RequestDropIngredientToTool(itemDataId, Tool_Cuttingboard, _cuttingboardObjectName, "도마");
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

        _selectedIngredientIdList.Add(itemDataId);
        RefreshInventorySlots();
        RefreshPotView();
        TryCompleteCooking();
    }

    /// <summary>
    /// 실제 조리도구 역할표를 확인한 뒤 재료를 소비하고 레시피 판정을 요청합니다.
    /// </summary>
    private void RequestDropIngredientToTool(string itemDataId, OOTechCookingToolDropTarget toolTarget, string fallbackToolId, string fallbackToolName)
    {
        if (string.IsNullOrEmpty(itemDataId))
            return;

        if (OOTechGameManager.Inst == null || OOTechGameManager.Inst.GetItemCount(itemDataId) <= 0)
        {
            SetStatus("인벤토리에 재료가 없습니다.");
            return;
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

        if (!OOTechGameManager.Inst.RemoveItem(itemDataId, 1))
        {
            SetStatus("재료를 꺼낼 수 없습니다.");
            return;
        }

        _selectedIngredientIdList.Add(itemDataId);
        RefreshInventorySlots();
        RefreshPotView();
        SetStatus($"{GetItemDisplayName(itemDataId)}을(를) {GetToolDisplayName(toolTarget, fallbackToolName)}에 올렸습니다.");
        TryCompleteCooking();
    }

    /// <summary>
    /// 현재까지 들어간 재료 조합이 완성 레시피인지 확인하고, 성공 시 완성 음식을 인벤토리에 넣습니다.
    /// </summary>
    private void TryCompleteCooking()
    {
        CookingResult cookingResult = OOTechCookingManager.Inst != null ? OOTechCookingManager.Inst.TryCook(_selectedIngredientIdList) : null;

        if (cookingResult == null || !cookingResult.IsSuccess)
        {
            SetStatus("재료가 가마솥에 들어갔습니다. 다음 재료를 넣어주세요.");
            return;
        }

        int resultCount = CalculateCookingResultCount(cookingResult.ResultItemId);
        OOTechGameManager.Inst.AddItem(cookingResult.ResultItemId, resultCount);
        string cookName = GetItemDisplayName(cookingResult.ResultItemId);
        _selectedIngredientIdList.Clear();
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
        if (resultItemId != "OO_PumpkinSoup_1" || OOTechGameManager.Inst == null)
            return 1;

        int remainingRiceCount = OOTechGameManager.Inst.GetItemCount(_riceIngredientId);
        int remainingPumpkinCount = OOTechGameManager.Inst.GetItemCount(_pumpkinIngredientId);
        int additionalCount = Mathf.Min(9, Mathf.Min(remainingRiceCount, remainingPumpkinCount));

        if (additionalCount <= 0)
            return 1;

        OOTechGameManager.Inst.RemoveItem(_riceIngredientId, additionalCount);
        OOTechGameManager.Inst.RemoveItem(_pumpkinIngredientId, additionalCount);
        return 1 + additionalCount;
    }

    private bool CanToolAcceptIngredient(string itemDataId, OOTechCookingToolDropTarget toolTarget, string fallbackToolId)
    {
        if (fallbackToolId == _cauldronObjectName)
            return IsIngredientInArray(itemDataId, _defaultCauldronAcceptedIngredientIdArray);

        if (fallbackToolId == _cuttingboardObjectName)
            return IsIngredientInArray(itemDataId, _defaultCuttingboardAcceptedIngredientIdArray);

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
        List<OO_Recipe> recipeList = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetRecipeDataList() : new List<OO_Recipe>();

        if (recipeList.Count == 0)
            return IsFallbackIngredientAccepted(itemDataId);

        foreach (OO_Recipe recipeData in recipeList)
        {
            if (recipeData == null || recipeData.RequiredIngredients == null)
                continue;

            List<string> candidateList = new List<string>(_selectedIngredientIdList) { itemDataId };

            if (IsPartialIngredientMatch(recipeData.RequiredIngredients, candidateList))
                return true;
        }

        return IsFallbackIngredientAccepted(itemDataId);
    }

    /// <summary>
    /// 레시피 재료 목록과 현재 후보 목록을 개수 기준으로 비교합니다.
    /// </summary>
    private bool IsPartialIngredientMatch(List<string> requiredIngredientList, List<string> candidateIngredientList)
    {
        Dictionary<string, int> requiredCountDic = CreateCountDic(requiredIngredientList);
        Dictionary<string, int> candidateCountDic = CreateCountDic(candidateIngredientList);

        foreach (KeyValuePair<string, int> pair in candidateCountDic)
        {
            if (!requiredCountDic.TryGetValue(pair.Key, out int requiredCount))
                return false;

            if (pair.Value > requiredCount)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 재료 ID별 개수를 세어 조합 비교용 Dictionary를 만듭니다.
    /// </summary>
    private Dictionary<string, int> CreateCountDic(List<string> ingredientIdList)
    {
        Dictionary<string, int> countDic = new Dictionary<string, int>();

        foreach (string ingredientId in ingredientIdList)
        {
            if (string.IsNullOrEmpty(ingredientId))
                continue;

            if (!countDic.ContainsKey(ingredientId))
                countDic[ingredientId] = 0;

            countDic[ingredientId]++;
        }

        return countDic;
    }

    /// <summary>
    /// 데이터가 아직 완성되지 않았을 때도 쌀+채소/호박 튜토리얼이 동작하도록 하는 안전망입니다.
    /// </summary>
    private bool IsFallbackIngredientAccepted(string itemDataId)
    {
        if (itemDataId != _riceIngredientId && itemDataId != _vegetableIngredientId && itemDataId != _pumpkinIngredientId)
            return false;

        if (itemDataId == _vegetableIngredientId || itemDataId == _pumpkinIngredientId)
            return !_selectedIngredientIdList.Contains(_vegetableIngredientId) && !_selectedIngredientIdList.Contains(_pumpkinIngredientId);

        return !_selectedIngredientIdList.Contains(itemDataId);
    }

    /// <summary>
    /// 요리 매니저가 씬에 없으면 최소 동작을 위해 매니저 오브젝트를 준비합니다.
    /// </summary>
    private void EnsureCookingManager()
    {
        if (OOTechCookingManager.Inst != null)
            return;

        GameObject managerObject = new GameObject("OOTechCookingManager");
        managerObject.AddComponent<OOTechCookingManager>();
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
        Rect_DragGhostTemplate = View_Cooking.DragGhostTemplateRect;

        Canvas canvas = Root_Canvas.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = _sortingOrder;
        }

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

        Tool_Cauldron.RequestSetupTool(_cauldronObjectName, "가마솥", _defaultCauldronAcceptedIngredientIdArray, _cauldronDropAreaPadding);
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

        Tool_Cuttingboard.RequestSetupTool(_cuttingboardObjectName, "도마", _defaultCuttingboardAcceptedIngredientIdArray, _cuttingboardDropAreaPadding);
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

        if (_selectedIngredientIdList.Count == 0)
        {
            Text_Pot.text = string.Empty;
            Text_Pot.gameObject.SetActive(false);
            return;
        }

        Text_Pot.gameObject.SetActive(true);
        string text = string.Empty;

        foreach (string ingredientId in _selectedIngredientIdList)
            text += GetItemDisplayName(ingredientId) + "\n";

        Text_Pot.text = text.TrimEnd();
    }

    /// <summary>
    /// 처음 부엌에 들어왔을 때 가마솥 사용법 말풍선과 화살표를 보여줍니다.
    /// </summary>
    private void ShowToolGuideSequence()
    {
        StopToolGuideRoutine();
        _isToolGuideComplete = false;
        _toolGuideCoroutine = StartCoroutine(PlayToolGuideSequenceRoutine());
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
        SetStatus("쌀은 가마솥에, 채소는 도마에 올려 요리를 완성하세요.");
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

    private void GetCauldronGuideData(out string title, out string description)
    {
        OO_Tutorial tutorialData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetTutorialData(_cauldronTutorialId) : null;
        title = tutorialData != null && !string.IsNullOrEmpty(tutorialData.Title) ? tutorialData.Title : string.Empty;

        if (string.IsNullOrEmpty(title) && tutorialData != null)
            title = tutorialData.Name;

        if (string.IsNullOrEmpty(title))
            title = "가마솥";

        description = tutorialData != null && !string.IsNullOrEmpty(tutorialData.Description)
            ? tutorialData.Description
            : "왼쪽 인벤토리의 쌀과 채소를 가운데 가마솥으로 끌어다 놓으세요.\n재료는 하나씩 넣는 것이 기본입니다.\n쌀 + 채소가 들어가면 채소죽이 완성됩니다.";
    }

    /// <summary>
    /// 말풍선 제목과 본문 텍스트를 적용합니다.
    /// </summary>
    private void GetCuttingboardGuideData(out string title, out string description)
    {
        OO_Tutorial tutorialData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetTutorialData(_cuttingboardTutorialId) : null;
        title = tutorialData != null && !string.IsNullOrEmpty(tutorialData.Title) ? tutorialData.Title : string.Empty;

        if (string.IsNullOrEmpty(title) && tutorialData != null)
            title = tutorialData.Name;

        if (string.IsNullOrEmpty(title))
            title = "도마";

        description = tutorialData != null && !string.IsNullOrEmpty(tutorialData.Description)
            ? tutorialData.Description
            : "채소는 도마에 올려놓으세요.\n가마솥에 쌀, 도마에 채소가 준비되면 채소죽이 완성됩니다.";
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
        if (Root_CauldronGuideArrow != null && TryGetCauldronTopLocalPoint(out Vector2 cauldronTopLocalPoint))
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
        if (Text_InventoryNewBadge == null)
            return;

        StopInventoryNewBadgeBlink();
        Text_InventoryNewBadge.gameObject.SetActive(isActive);

        Color badgeColor = Text_InventoryNewBadge.color;
        badgeColor.a = 1f;
        Text_InventoryNewBadge.color = badgeColor;

        if (isActive && gameObject.activeInHierarchy)
            _inventoryNewBadgeCoroutine = StartCoroutine(BlinkInventoryNewBadgeRoutine());
    }

    /// <summary>
    /// 부엌 NEW 배지의 알파를 흔들어 새 음식 획득을 강조합니다.
    /// </summary>
    private IEnumerator BlinkInventoryNewBadgeRoutine()
    {
        while (Text_InventoryNewBadge != null && Text_InventoryNewBadge.gameObject.activeSelf)
        {
            Color badgeColor = Text_InventoryNewBadge.color;
            float wave = (Mathf.Sin(Time.unscaledTime * _newBadgeBlinkSpeed) + 1f) * 0.5f;
            badgeColor.a = Mathf.Lerp(_newBadgeMinimumAlpha, 1f, wave);
            Text_InventoryNewBadge.color = badgeColor;
            yield return null;
        }
    }

    private void StopInventoryNewBadgeBlink()
    {
        if (_inventoryNewBadgeCoroutine == null)
            return;

        StopCoroutine(_inventoryNewBadgeCoroutine);
        _inventoryNewBadgeCoroutine = null;
    }

    /// <summary>
    /// 열려 있는 Road HUD들에게 인벤토리 NEW 배지를 켜 달라고 알립니다.
    /// </summary>
    private void NotifyRoadHUDInventoryNewBadge()
    {
        OOTechRoadHUDController[] hudControllerArray = FindObjectsByType<OOTechRoadHUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (OOTechRoadHUDController hudController in hudControllerArray)
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
        OOTechRoadHUDController[] hudControllerArray = FindObjectsByType<OOTechRoadHUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (OOTechRoadHUDController hudController in hudControllerArray)
        {
            if (hudController == null || !hudController.gameObject.activeInHierarchy)
                continue;

            hudController.RequestRefreshInventoryView();
        }
    }

    /// <summary>
    /// 채소죽 제작이 끝났다고 Road HUD에 알려 임무판 체크 표시를 갱신합니다.
    /// </summary>
    private void NotifyRoadHUDCookingQuestComplete()
    {
        OOTechRoadHUDController[] hudControllerArray = FindObjectsByType<OOTechRoadHUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (OOTechRoadHUDController hudController in hudControllerArray)
        {
            if (hudController == null || !hudController.gameObject.activeInHierarchy)
                continue;

            hudController.RequestCompleteCookingQuest();
        }
    }

    /// <summary>
    /// 합창 대사가 끝난 뒤 플레이어에게 임무가 완료되었음을 짧게 안내합니다.
    /// </summary>
    private void ShowCookingMissionCompleteGuide()
    {
        ApplyGuideText("임무 완수", "채소죽을 완성했습니다.\n임무 UI에 완료 표시가 추가되었습니다.");
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
        if (!IsRoadMapCookingTutorialResult(resultItemId))
            return;

        if (!gameObject.activeInHierarchy || _isCookingCompleteDialoguePlaying)
            return;

        _pendingCookingCompleteResultItemId = resultItemId;
        StartCoroutine(PlayCookingCompleteDialogueRoutine());
    }

    private bool IsRoadMapCookingTutorialResult(string resultItemId)
    {
        return resultItemId == "OO_VegetableSoup_1";
    }

    /// <summary>
    /// DialogueGroup을 열고 OO_DialogueGroup 데이터를 하나의 대화로 변환해 보여줍니다.
    /// </summary>
    private IEnumerator PlayCookingCompleteDialogueRoutine()
    {
        _isCookingCompleteDialoguePlaying = true;

        if (!TryOpenDialogueGroup())
        {
            ConsumePendingCookingCompleteResult();
            NotifyRoadHUDCookingQuestComplete();
            ShowCookingMissionCompleteGuide();
            _isCookingCompleteDialoguePlaying = false;
            yield break;
        }

        OO_Dialogue dialogueData = CreateCookingCompleteDialogueData();

        if (dialogueData == null || UI_Dialogue == null)
        {
            CloseDialogueGroup();
            ConsumePendingCookingCompleteResult();
            NotifyRoadHUDCookingQuestComplete();
            ShowCookingMissionCompleteGuide();
            _isCookingCompleteDialoguePlaying = false;
            yield break;
        }

        OO_Dialogue moranDialogueData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetDialogueData(_moranCookingCompleteDialogueId) : null;

        if (moranDialogueData != null)
            yield return ShowDialogueDataAndWait(moranDialogueData);

        yield return ShowDialogueDataAndWait(dialogueData);
        CloseDialogueGroup();
        ConsumePendingCookingCompleteResult();
        NotifyRoadHUDCookingQuestComplete();
        ShowCookingMissionCompleteGuide();
        _isCookingCompleteDialoguePlaying = false;
    }

    /// <summary>
    /// 첫 요리 튜토리얼의 완성 음식은 대사로 먹은 뒤 인벤토리에서 1개 제거합니다.
    /// Game View에서는 "잘 먹었습니다" 대사가 끝난 다음 채소죽 슬롯이 사라집니다.
    /// </summary>
    private void ConsumePendingCookingCompleteResult()
    {
        if (string.IsNullOrEmpty(_pendingCookingCompleteResultItemId))
            return;

        string resultItemId = _pendingCookingCompleteResultItemId;
        _pendingCookingCompleteResultItemId = string.Empty;

        if (OOTechGameManager.Inst == null)
            return;

        if (!OOTechGameManager.Inst.RemoveItem(resultItemId, 1))
            return;

        RefreshInventorySlots();
        NotifyRoadHUDInventoryRefresh();
        Debug.Log($"[OOTechCookingGroupController] Consumed cooked result after dialogue: {resultItemId}");
    }

    private IEnumerator ShowDialogueDataAndWait(OO_Dialogue dialogueData)
    {
        if (dialogueData == null || UI_Dialogue == null)
            yield break;

        bool isDone = false;
        UI_Dialogue.ShowDialogue(dialogueData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
    }

    /// <summary>
    /// OO_DialogueGroup 데이터를 DialogueUI가 표시할 수 있는 OO_Dialogue 형태로 변환합니다.
    /// </summary>
    private OO_Dialogue CreateCookingCompleteDialogueData()
    {
        OO_DialogueGroup dialogueGroupData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.GetDialogueGroupData(_cookingCompleteDialogueGroupId) : null;

        if (dialogueGroupData == null)
            return CreateDialogueData(_cookingCompleteDialogueGroupId, "춘양 + 재익군 + 모란", "잘 먹었습니다!");

        string speakerName = CreateDialogueGroupSpeakerName(dialogueGroupData);
        string text = CreateDialogueGroupText(dialogueGroupData);

        if (string.IsNullOrEmpty(text))
            text = "잘 먹었습니다!";

        if (string.IsNullOrEmpty(speakerName))
            speakerName = "춘양 + 재익군 + 모란";

        return CreateDialogueData(dialogueGroupData.Id, speakerName, text);
    }

    /// <summary>
    /// 여러 화자 ID를 실제 캐릭터 이름으로 바꿔 "춘양 + 재익군 + 모란" 형태로 만듭니다.
    /// </summary>
    private string CreateDialogueGroupSpeakerName(OO_DialogueGroup dialogueGroupData)
    {
        List<string> speakerNameList = new List<string>();

        if (dialogueGroupData.SpeakerCharacterIdList != null)
        {
            foreach (string speakerCharacterId in dialogueGroupData.SpeakerCharacterIdList)
            {
                string speakerName = GetCharacterName(speakerCharacterId);

                if (!string.IsNullOrEmpty(speakerName) && !speakerNameList.Contains(speakerName))
                    speakerNameList.Add(speakerName);
            }
        }

        if (speakerNameList.Count == 0 && dialogueGroupData.SpeakerNameList != null)
        {
            foreach (string speakerName in dialogueGroupData.SpeakerNameList)
            {
                if (!string.IsNullOrEmpty(speakerName) && !speakerNameList.Contains(speakerName))
                    speakerNameList.Add(speakerName);
            }
        }

        if (speakerNameList.Count == 0 && dialogueGroupData.DialogueIdList != null && OOTechGameDataManager.Inst != null)
        {
            foreach (string dialogueId in dialogueGroupData.DialogueIdList)
            {
                OO_Dialogue dialogueData = OOTechGameDataManager.Inst.GetDialogueData(dialogueId);

                if (dialogueData != null && !string.IsNullOrEmpty(dialogueData.SpeakerName) && !speakerNameList.Contains(dialogueData.SpeakerName))
                    speakerNameList.Add(dialogueData.SpeakerName);
            }
        }

        return string.Join(" + ", speakerNameList);
    }

    /// <summary>
    /// DialogueGroup의 직접 텍스트가 없으면 연결된 Dialogue 데이터에서 대표 문장을 가져옵니다.
    /// </summary>
    private string CreateDialogueGroupText(OO_DialogueGroup dialogueGroupData)
    {
        if (!string.IsNullOrEmpty(dialogueGroupData.Text))
            return dialogueGroupData.Text;

        if (dialogueGroupData.DialogueIdList == null || OOTechGameDataManager.Inst == null)
            return string.Empty;

        foreach (string dialogueId in dialogueGroupData.DialogueIdList)
        {
            OO_Dialogue dialogueData = OOTechGameDataManager.Inst.GetDialogueData(dialogueId);

            if (dialogueData != null && !string.IsNullOrEmpty(dialogueData.Text))
                return dialogueData.Text;
        }

        return string.Empty;
    }

    private string GetCharacterName(string characterId)
    {
        if (OOTechGameDataManager.Inst == null || string.IsNullOrEmpty(characterId))
            return string.Empty;

        OO_Character characterData = OOTechGameDataManager.Inst.GetCharacterData(characterId);
        return characterData != null ? characterData.Name : string.Empty;
    }

    private OO_Dialogue CreateDialogueData(string dialogueId, string speakerName, string text)
    {
        return new OO_Dialogue
        {
            Id = dialogueId,
            SpeakerName = speakerName,
            Text = text
        };
    }

    private bool TryOpenDialogueGroup()
    {
        if (Group_Dialogue == null)
            Group_Dialogue = FindSceneObjectByName(_dialogueGroupName);

        if (Group_Dialogue == null)
            return false;

        if (UI_Dialogue == null)
            UI_Dialogue = Group_Dialogue.GetComponentInChildren<DialogueUI>(true);

        if (UI_Dialogue == null)
            return false;

        UI_Dialogue.RequestRoadViewLayout();

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.RegisterUI(_dialogueGroupName, Group_Dialogue);

            if (OOTechUIManager.Inst.OpenUI(_dialogueGroupName))
            {
                UI_Dialogue.RequestRoadViewLayout();
                return true;
            }
        }

        Group_Dialogue.SetActive(true);
        UI_Dialogue.RequestRoadViewLayout();
        return true;
    }

    private void CloseDialogueGroup()
    {
        if (UI_Dialogue != null)
            UI_Dialogue.CloseDialogue();

        if (OOTechUIManager.Inst != null && OOTechUIManager.Inst.CloseUI(_dialogueGroupName))
            return;

        if (Group_Dialogue != null)
            Group_Dialogue.SetActive(false);
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
        float targetSize = Mathf.Max(bounds.extents.y, bounds.extents.x / aspect) * _cameraPadding;
        Vector3 cameraPosition = bounds.center;
        cameraPosition.z = Camera_Main.transform.position.z;

        Camera_Main.orthographic = true;
        Camera_Main.orthographicSize = Mathf.Max(0.1f, targetSize);
        Camera_Main.transform.position = cameraPosition;
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

    private GameObject FindSceneObjectByName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

        GameObject[] objectArray = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject sceneObject in objectArray)
        {
            if (sceneObject == null || sceneObject.name != objectName)
                continue;

            if (!sceneObject.scene.IsValid())
                continue;

            return sceneObject;
        }

        return null;
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

        if (itemDataId == "OO_PumpkinSoup_1")
            return "호박죽";

        if (itemDataId == "OO_VegetableSoup_1")
            return "채소죽";

        return string.IsNullOrEmpty(itemDataId) ? "알 수 없는 아이템" : itemDataId;
    }

    /// <summary>
    /// 부엌 하단 상태 문구를 갱신합니다.
    /// </summary>
    private void SetStatus(string statusText)
    {
        if (Text_Status != null)
            Text_Status.text = statusText;
    }
}
