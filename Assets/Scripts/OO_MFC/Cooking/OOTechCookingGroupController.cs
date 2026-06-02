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
    [Header("Data Id")]
    [SerializeField] private string _cauldronTutorialId = "narration_tutorial_11";
    [SerializeField] private string _cookingCompleteDialogueGroupId = "dialogue_group_cooking_vegetable_porridge_complete_01";
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";

    [Header("Scene Role")]
    [SerializeField] private string _cauldronObjectName = "Cauldron";
    [SerializeField] private float _cauldronDropAreaPadding = 1.18f;

    [Header("Canvas")]
    [SerializeField] private int _sortingOrder = 1260;
    [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);

    [Header("Camera")]
    [SerializeField] private Camera Camera_Main;
    [SerializeField] private float _cameraPadding = 1.04f;

    [Header("New Badge")]
    [SerializeField] private float _newBadgeBlinkSpeed = 7f;
    [SerializeField] private float _newBadgeMinimumAlpha = 0.25f;

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
    private Transform Transform_Cauldron;
    private SpriteRenderer Renderer_Cauldron;
    private Collider2D Collider_Cauldron;
    private GameObject Root_GuideBubble;
    private GameObject Root_InventoryGuideArrow;
    private GameObject Root_CauldronGuideArrow;
    private TextMeshProUGUI Text_Status;
    private TextMeshProUGUI Text_Pot;
    private TextMeshProUGUI Text_GuideTitle;
    private TextMeshProUGUI Text_GuideBody;
    private TextMeshProUGUI Text_InventoryNewBadge;
    private Button Button_GuideConfirm;
    private RectTransform Rect_DragGhostTemplate;
    private OOTechCookingGroupView View_Cooking;
    private GameObject Group_Dialogue;
    private DialogueUI UI_Dialogue;
    private Coroutine _inventoryNewBadgeCoroutine;
    private UnityAction _guideConfirmAction;
    private bool _isCookingCompleteDialoguePlaying;

    /// <summary>
    /// 부엌 무대가 열리면 카메라를 Kitchen 배경에 맞추고 가마솥 가이드를 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        NormalizeCookingGroupTransformIfNeeded();
        EnsureCookingManager();
        ResolveCauldronReference();
        ApplyKitchenCameraView();
        PrepareCookingView();
        RefreshInventorySlots();
        RefreshPotView();
        ShowCauldronGuide();
    }

    /// <summary>
    /// 매 프레임 UI 드롭 영역과 화살표가 실제 가마솥 위치를 따라가도록 보정합니다.
    /// </summary>
    private void LateUpdate()
    {
        if (Root_Canvas == null)
            return;

        UpdateCauldronDropArea();
        UpdateGuideArrowLayout();
    }

    /// <summary>
    /// 부엌 무대가 닫히면 대화, 배지 깜빡임, 카메라 상태를 원래 로드 장면으로 복구합니다.
    /// </summary>
    private void OnDisable()
    {
        _guideConfirmAction = null;
        _isCookingCompleteDialoguePlaying = false;
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

        TextMeshProUGUI ghostText = ghostObject.GetComponentInChildren<TextMeshProUGUI>(true);

        if (ghostText != null)
            ghostText.text = GetItemDisplayName(itemDataId);

        Transform iconTransform = FindChildByName(ghostObject.transform, "Image_ItemIcon");
        Image iconImage = iconTransform != null ? iconTransform.GetComponent<Image>() : null;

        if (iconImage != null)
        {
            Sprite iconSprite = OOTechItemCatalogManager.Inst != null ? OOTechItemCatalogManager.Inst.GetItemIconSprite(itemDataId) : null;
            iconImage.sprite = iconSprite;
            iconImage.enabled = iconSprite != null;
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

        if (IsPointerInsideSceneCauldron(screenPosition))
            return true;

        UpdateCauldronDropArea();

        if (Rect_Cauldron == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(Rect_Cauldron, screenPosition, null);
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

        OOTechGameManager.Inst.AddItem(cookingResult.ResultItemId, 1);
        string cookName = GetItemDisplayName(cookingResult.ResultItemId);
        _selectedIngredientIdList.Clear();
        RefreshInventorySlots();
        RefreshPotView();
        SetInventoryNewBadgeActive(true);
        NotifyRoadHUDInventoryNewBadge();
        HideGuideBubble();
        RequestPlayCookingCompleteDialogue();
        SetStatus($"{cookName} 완성! 인벤토리에 새 음식이 들어갔습니다.");
    }

    /// <summary>
    /// 지금 선택한 재료가 레시피의 중간 단계로 허용되는지 확인합니다.
    /// </summary>
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
        if (itemDataId != "Ing_Rice_01" && itemDataId != "Ing_Veggie_01" && itemDataId != "Ing_Pumpkin_01")
            return false;

        if (itemDataId == "Ing_Veggie_01" || itemDataId == "Ing_Pumpkin_01")
            return !_selectedIngredientIdList.Contains("Ing_Veggie_01") && !_selectedIngredientIdList.Contains("Ing_Pumpkin_01");

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
        Text_Pot = View_Cooking.PotContentText;
        Root_GuideBubble = View_Cooking.GuideBubble;
        Text_GuideTitle = View_Cooking.GuideTitleText;
        Text_GuideBody = View_Cooking.GuideBodyText;
        Button_GuideConfirm = View_Cooking.GuideConfirmButton;
        Root_InventoryGuideArrow = View_Cooking.InventoryGuideArrow;
        Root_CauldronGuideArrow = View_Cooking.CauldronGuideArrow;
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

        SetGuidePointerActive(false);
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
    }

    /// <summary>
    /// 화면 좌표를 실제 월드 좌표로 바꿔 가마솥 Collider 또는 Sprite Bounds 안인지 확인합니다.
    /// </summary>
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

    private Bounds GetCauldronBounds()
    {
        if (Collider_Cauldron != null)
            return Collider_Cauldron.bounds;

        if (Renderer_Cauldron != null)
            return Renderer_Cauldron.bounds;

        return new Bounds(Transform_Cauldron.position, Vector3.one);
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
            Text_Pot.text = "비어 있음";
            return;
        }

        string text = string.Empty;

        foreach (string ingredientId in _selectedIngredientIdList)
            text += GetItemDisplayName(ingredientId) + "\n";

        Text_Pot.text = text.TrimEnd();
    }

    /// <summary>
    /// 처음 부엌에 들어왔을 때 가마솥 사용법 말풍선과 화살표를 보여줍니다.
    /// </summary>
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
    private void ApplyGuideText(string title, string description)
    {
        if (Root_GuideBubble != null)
            Root_GuideBubble.SetActive(true);

        if (Text_GuideTitle != null)
            Text_GuideTitle.text = title;

        if (Text_GuideBody != null)
            Text_GuideBody.text = description;
    }

    /// <summary>
    /// 요리 성공처럼 다음 컷으로 바로 넘어가야 할 때 현재 안내 말풍선과 화살표를 정리합니다.
    /// </summary>
    private void HideGuideBubble()
    {
        _guideConfirmAction = null;

        if (Root_GuideBubble != null)
            Root_GuideBubble.SetActive(false);

        SetGuidePointerActive(false);
    }

    /// <summary>
    /// 씬에 배치된 인벤토리/가마솥 화살표를 켜고 위치를 갱신합니다.
    /// </summary>
    private void CreateGuidePointersIfNeeded()
    {
        if (Rect_Root == null)
            return;

        if (Root_InventoryGuideArrow == null || Root_CauldronGuideArrow == null)
        {
            Debug.LogWarning("[OOTechCookingGroupController] Cooking guide arrow objects are missing from CookingUIGroup.");
            return;
        }

        SetGuidePointerActive(true);
        UpdateGuideArrowLayout();
    }

    private void SetGuidePointerActive(bool isActive)
    {
        if (Root_InventoryGuideArrow != null)
            Root_InventoryGuideArrow.SetActive(isActive);

        if (Root_CauldronGuideArrow != null)
            Root_CauldronGuideArrow.SetActive(isActive);
    }

    private void UpdateGuideArrowLayout()
    {
        if (Root_CauldronGuideArrow == null || Rect_Cauldron == null)
            return;

        RectTransform arrowRect = Root_CauldronGuideArrow.transform as RectTransform;

        if (arrowRect == null)
            return;

        arrowRect.anchoredPosition = Rect_Cauldron.anchoredPosition + new Vector2(0f, Rect_Cauldron.sizeDelta.y * 0.5f + 72f);
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
    private void RequestPlayCookingCompleteDialogue()
    {
        if (!gameObject.activeInHierarchy || _isCookingCompleteDialoguePlaying)
            return;

        StartCoroutine(PlayCookingCompleteDialogueRoutine());
    }

    /// <summary>
    /// DialogueGroup을 열고 OO_DialogueGroup 데이터를 하나의 대화로 변환해 보여줍니다.
    /// </summary>
    private IEnumerator PlayCookingCompleteDialogueRoutine()
    {
        _isCookingCompleteDialoguePlaying = true;

        if (!TryOpenDialogueGroup())
        {
            NotifyRoadHUDCookingQuestComplete();
            ShowCookingMissionCompleteGuide();
            _isCookingCompleteDialoguePlaying = false;
            yield break;
        }

        OO_Dialogue dialogueData = CreateCookingCompleteDialogueData();

        if (dialogueData == null || UI_Dialogue == null)
        {
            CloseDialogueGroup();
            NotifyRoadHUDCookingQuestComplete();
            ShowCookingMissionCompleteGuide();
            _isCookingCompleteDialoguePlaying = false;
            yield break;
        }

        bool isDone = false;
        UI_Dialogue.ShowDialogue(dialogueData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
        CloseDialogueGroup();
        NotifyRoadHUDCookingQuestComplete();
        ShowCookingMissionCompleteGuide();
        _isCookingCompleteDialoguePlaying = false;
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
            OO_Ingredient ingredientData = OOTechGameDataManager.Inst.GetIngredientData(itemDataId);

            if (ingredientData != null && !string.IsNullOrEmpty(ingredientData.Name))
                return ingredientData.Name;

            OO_Cook cookData = OOTechGameDataManager.Inst.GetCookData(itemDataId);

            if (cookData != null && !string.IsNullOrEmpty(cookData.Name))
                return cookData.Name;
        }

        if (itemDataId == "Ing_Rice_01")
            return "쌀";

        if (itemDataId == "Ing_Veggie_01")
            return "채소";

        if (itemDataId == "Ing_Pumpkin_01")
            return "호박";

        if (itemDataId == "OO_Cook_1")
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
