// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechCookingGroupController.cs
// - ??븷: ?붾━ ?쒖뒪?쒖쓽 ?낅젰, 議곕━?꾧뎄, ?덉떆???먯젙???대떦?섎뒗 ?ㅽ겕由쏀듃?낅땲??
// - 媛먮룆 愿?? 遺???λ㈃?먯꽌 ?щ즺? 議곕━?꾧뎄 諛곗슦媛 ?대뼡 ?쒖꽌濡?留뚮굹?붿? 愿由ы빀?덈떎.
// - ?좎?蹂댁닔 ?ъ씤?? ?щ즺 洹쒖튃? ?곗씠?곗? DropTarget ??븷?쒕줈 鍮쇨퀬, UI 諛곗튂??CookingUIGroup?먯꽌 吏곸젒 ?섏젙?⑸땲??
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
/// CookingGroup???붾━ ?쒗넗由ъ뼹, 媛留덉넡 ?쒕옒洹??먯젙, ?꾩꽦 ??붾? 吏?섑빀?덈떎.
/// 遺??UI ?뚰뭹? CookingUIGroup??諛곗튂?섍퀬, ??而⑦듃濡ㅻ윭???щ즺媛 ?ㅼ뼱媛???쒖꽌留?愿由ы빀?덈떎.
/// </summary>
[DisallowMultipleComponent]
public class OOTechCookingGroupController : MonoBehaviour
{
    private static bool _isSharedToolGuideCompleted;
    private static bool _isSharedJulguGuideCompleted;

    // ?쎈뒗 ?쒖꽌:
    // 1. OnEnable: 遺??臾대?媛 ?대┫ ??移대찓?? 議곕━?꾧뎄, ?몃깽?좊━ UI瑜?以鍮꾪빀?덈떎.
    // 2. ShowToolGuideSequence: 媛留덉넡怨??꾨쭏 ?ъ슜踰뺤쓣 ?쒗넗由ъ뼹濡?蹂댁뿬以띾땲??
    // 3. TryAddIngredientToTool 怨꾩뿴: ?쒕옒洹명븳 ?щ즺媛 ?щ컮瑜?議곕━?꾧뎄???ㅼ뼱媛붾뒗吏 ?먯젙?⑸땲??
    // 4. TryCompleteCooking 怨꾩뿴: ? + 梨꾩냼 議고빀???꾩꽦 ?뚯떇?쇰줈 諛붾뚮뒗吏 ?뺤씤?⑸땲??
    // 5. OnDisable: 遺?뚯쓣 ?レ쓣 ??移대찓?쇱? HUD ?곹깭瑜??먮옒 濡쒕뱶 臾대?濡??섎룎由쎈땲??
    // ?좎?蹂댁닔 二쇱쓽:
    // - UI ?꾩튂? ?대?吏??CookingUIGroup?먯꽌 吏곸젒 ?섏젙?⑸땲??
    // - ???щ즺/?붾━??媛?ν븯硫?OO_Ingredient, OO_Recipe, OO_Cook ?곗씠?곕줈 異붽??⑸땲??
    // - ??Controller媛 ??而ㅼ?硫?議곕━ ?먯젙, ?쒗넗由ъ뼹, ?몃깽?좊━ ?쒖떆瑜?蹂꾨룄 而댄룷?뚰듃濡?遺꾨━?댁빞 ?⑸땲??

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
    private string _carrotIngredientId = "Ing_Carrot_01";
    private string _carrotStarchCookId = "OO_CarrotStarch_1";
    private string[] _defaultCauldronAcceptedIngredientIdArray = { "Ing_Rice_01", "Ing_Kimch_01", "OO_CarrotStarch_1" };
    private string[] _defaultCuttingboardAcceptedIngredientIdArray = { "Ing_Veggie_01", "Ing_Pumpkin_01", "Ing_ChiliPepper_01", "Ing_Carrot_01" };
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
    /// 遺??臾대?媛 ?대━硫?移대찓?쇰? Kitchen 諛곌꼍??留욎텛怨?媛留덉넡 媛?대뱶瑜??쒖옉?⑸땲??
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
        PrepareStage4ReturnButtonIfNeeded();
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
    /// HUD 踰꾪듉 ?몄텧 寃쎈줈媛 ?딄꺼??CookingGroup 吏꾩엯 ???몃깽?좊━? ?꾨Т ?⑤꼸???ㅼ떆 ?쎈땲??
    /// Game View?먯꽌??遺???낆옣 吏곹썑 ?щ즺 ?щ’??蹂댁씠怨??쒕옒洹??낅젰??諛쏆쓣 ???덇쾶 ?섎뒗 ?덉쟾?μ튂?낅땲??
    /// </summary>
    private void RequestOpenCookingSupportHUDIfNeeded()
    {
        OOTechRoadHUDController hudController = OOTechSceneQuery.RequestFirstComponent<OOTechRoadHUDController>(
            delegate (OOTechRoadHUDController targetHUD)
            {
                return targetHUD != null;
            },
            true);

        if (hudController == null)
            return;

        hudController.SetHUDVisible(true);
        hudController.RequestOpenCookingSupportHUD();
        hudController.RequestRefreshInventoryView();
    }

    /// <summary>
    /// CookingGroup??耳쒖쭊 諛붾줈 ?ㅼ쓬 ?꾨젅?꾩뿉??HUD瑜??ㅼ떆 ?쎈땲?? 臾대? ?꾪솚 吏곹썑 諛곗슦? ?뚰뭹 ?깅줉 ?쒖꽌媛 ?닿툔?섎룄 ?몃깽?좊━ ?쒕옒洹몃? 蹂듦뎄?섎뒗 蹂댄뿕?낅땲??
    /// </summary>
    private IEnumerator RequestOpenCookingSupportHUDNextFrameRoutine()
    {
        yield return null;
        RequestOpenCookingSupportHUDIfNeeded();
    }

    /// <summary>
    /// 留??꾨젅??UI ?쒕∼ ?곸뿭怨??붿궡?쒓? ?ㅼ젣 媛留덉넡 ?꾩튂瑜??곕씪媛?꾨줉 蹂댁젙?⑸땲??
    /// </summary>
    /// <summary>
    /// Stage3 ?곌뎔 遺?뚯뿉?쒕뒗 ?덇뎄瑜?吏곸젒 ?대┃??? 1媛쒕? ??1媛쒕줈 諛붽퓠?덈떎.
    /// ?쒕옒洹??먯젙???붾뱾?ㅻ룄 Game View 吏꾪뻾??留됲엳吏 ?딅룄濡? ?덇뎄 諛곗슦媛 ?먭린 ??븷??吏곸젒 ?섑뻾?섎뒗 寃쎈줈?낅땲??
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
    /// 遺??臾대?媛 ?ロ엳硫???? 諛곗? 源쒕묀?? 移대찓???곹깭瑜??먮옒 濡쒕뱶 ?λ㈃?쇰줈 蹂듦뎄?⑸땲??
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
    /// Stage1 珥뚯옣 蹂댁긽 援먰솚???꾨씫??梨?遺?뚯뿉 ?ㅼ뼱??寃쎌슦 ?몃깽?좊━瑜?蹂댁젙?⑸땲??
    /// ?곹솕濡?移섎㈃ 遺???λ㈃ ?쒖옉 ?꾩뿉 ?뚰뭹 ?대떦???몃컯二쎌쓣 ?뚯닔?섍퀬, ?ㅼ쓬 ?붾━???꾩슂??泥?뼇怨좎텛? 源移섎? ?щ젮?먮뒗 ?먯엯?덈떎.
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

    private bool IsOpenedFromStage4_2Group()
    {
        string previousGroupName = OOTechGroupNavigationHistory.GetPreviousGroup(gameObject.name, string.Empty);
        return previousGroupName == "Stage4_2Group";
    }

    /// <summary>
    /// EncounterGroup?먯꽌 ?대┛ 遺?뚯? ?뚯븘媛湲?踰꾪듉??諛섎뱶??EncounterGroup?쇰줈 怨좎젙?⑸땲??
    /// ?댁쟾 Stage2 蹂듦? 由ъ뒪?덇? ?⑥븘 ?덉뼱?????λ㈃?먯꽌???곌뎔 臾대?濡??뚯븘媛???⑸땲??
    /// </summary>
    private void PrepareEncounterReturnButtonIfNeeded()
    {
        if (!IsOpenedFromEncounterGroup())
            return;

        BackButtonController[] backButtonArray = GetComponentsInChildren<BackButtonController>(true);

        foreach (BackButtonController backButton in backButtonArray)
            backButton.SetPreviousGroup("EncounterGroup");

        Transform returnButtonTransform = RequestChildObjectByName(transform, "Button_RuntimeReturn");

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

        GameObject encounterGroupObject = RequestSceneObjectByName("EncounterGroup");

        gameObject.SetActive(false);

        if (encounterGroupObject != null)
        encounterGroupObject.SetActive(true);
    }

    /// <summary>
    /// Stage4_2?먯꽌 ?대┛ 遺?뚯? ?좊겮 ?섏뒪??以묎컙 ?꾩튂濡??뚯븘媛???⑸땲??
    /// ?뚯븘媛湲?踰꾪듉???댁쟾 Stage??Road濡??ν븯硫??먯떆?멸? ?딄린誘濡? ???λ㈃?먯꽌??Stage4_2Group?쇰줈 怨좎젙?⑸땲??
    /// </summary>
    private void PrepareStage4ReturnButtonIfNeeded()
    {
        if (!IsOpenedFromStage4_2Group())
            return;

        BackButtonController[] backButtonArray = GetComponentsInChildren<BackButtonController>(true);

        foreach (BackButtonController backButton in backButtonArray)
            backButton.SetPreviousGroup("Stage4_2Group");
    }

    /// <summary>
    /// ?몃? ?쒕옒洹??щ’???꾩옱 遺?뚯씠 ?곌뎔 Encounter ?꾩슜 遺?뚯씤吏 ?뺤씤?????ъ슜?⑸땲??
    /// ???λ㈃?먯꽌??????덇뎄??1媛쒖뵫留??ｊ쾶 ?섏뿬 ?섎웾 ?좏깮 ?ш퀬瑜?留됱뒿?덈떎.
    /// </summary>
    public bool IsStage3EncounterCooking()
    {
        return IsOpenedFromEncounterGroup();
    }

    /// <summary>
    /// Stage3 遺??吏꾩엯 ???댁쟾 ?쒗넗由ъ뼹 ?쒖옉 蹂댁긽???ㅼ떆 蹂댁젙?섏뼱 ?/梨꾩냼媛 12媛쒕줈 ?섎룎?꾧???寃쎌슦瑜?留됱뒿?덈떎.
    /// ?대? ?≪씠??轅?≪쓣 留뚮뱺 ?ㅼ뿉???뚮젅?댁뼱 吏꾪뻾 ?섎웾??嫄대뱶由ъ? ?딆뒿?덈떎.
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
        OOTechItemModel itemModel = RequestInventoryItemModel(itemList, itemDataId);

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
    /// OO_CookingCueSheet ?곗씠?곗뿉??遺???λ㈃??湲곕낯 ?レ옄? ID瑜??쎌뼱 ?곸슜?⑸땲??
    /// ?곹솕 鍮꾩쑀濡쒕뒗 怨듭뿰 ?쒖옉 ?꾩뿉 議곌컧?낆씠 ?먯떆?몃? 蹂닿퀬 議곕챸 諛앷린, 移대찓??嫄곕━, ?덈궡???띾룄瑜?留욎텛???④퀎?낅땲??
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
    /// ?뚮젅?댁뼱媛 ?щ즺 ?щ’???쒕옒洹명븷 ???먯뿉 ??寃껋쿂??蹂댁씠???꾩떆 ?붿긽 UI瑜?留뚮벊?덈떎.
    /// </summary>
    public RectTransform CreateDragGhost(string itemDataId, Vector2 screenPosition)
    {
        return CreateDragGhost(itemDataId, screenPosition, 1);
    }

    /// <summary>
    /// ?좏깮???섎웾???쒖떆???쒕옒洹??붿긽 UI瑜?留뚮벊?덈떎.
    /// Game View?먯꽌???뚯떇 ?꾩씠肄섎쭔 ?먯뿉 ?ㅺ퀬, ?щ윭 媛쒕? 吏묒뿀???뚮쭔 ?묎쾶 xN ?쒖떆媛 遺숈뒿?덈떎.
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
    /// CookingGroup ?먯젏??硫由?諛???덉쑝硫?Kitchen/Cauldron ?명듃留?媛源뚯슫 濡쒖뺄 醫뚰몴濡??ㅼ떆 ?뺣━?⑸땲??
    /// Game View???붾뱶 ?꾩튂???좎??섍퀬, 媛먮룆??Scene View?먯꽌 ?몄쭛?섍린 ?ъ슫 臾대? 醫뚰몴濡??섎룎由쎈땲??
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
    /// 留덉슦???ъ씤?곌? ?ㅼ젣 媛留덉넡 ?먮뒗 UI ?쒕∼ ?곸뿭 ?덉뿉 ?덈뒗吏 ?뺤씤?⑸땲??
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
    /// 留덉슦???ъ씤?곌? ?꾨쭏 ?꾩뿉 ?덈뒗吏 ?뺤씤?⑸땲??
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
    /// 留덉슦???ъ씤?곌? ?덇뎄 ?꾩뿉 ?덈뒗吏 ?뺤씤?⑸땲??
    /// Stage3?먯꽌??????덇뎄???щ젮 ?≪쓣 留뚮뱾湲??뚮Ц???붾뱶 ?ㅻ툕?앺듃 湲곗??쇰줈 ?먯젙?⑸땲??
    /// </summary>
    public bool IsPointerInsideJulgu(Vector2 screenPosition)
    {
        ResolveJulguReference();
        return IsPointerInsideSceneTool(screenPosition, Tool_Julgu, Transform_Julgu, Renderer_Julgu, Collider_Julgu, _julguDropAreaPadding);
    }

    /// <summary>
    /// ?щ즺瑜??볦? ?붾㈃ 醫뚰몴媛 ?대뒓 議곕━?꾧뎄 ?꾩씤吏 ?먯젙?섍퀬, 留욌뒗 ??븷?쒖뿉留??ъ엯?⑸땲??
    /// </summary>
    public void RequestDropIngredientAtPosition(string itemDataId, Vector2 screenPosition)
    {
        RequestDropIngredientAtPosition(itemDataId, screenPosition, 1);
    }

    /// <summary>
    /// ?좏깮???섎웾留뚰겮 ?щ즺瑜??볦? ?꾩튂??議곕━?꾧뎄???щ┰?덈떎.
    /// 湲곕낯? 1媛쒖씠硫? ?뚮젅?댁뼱媛 Ctrl+?좊줈 ?щ┛ ?섎웾留뚰겮留??뚮퉬?⑸땲??
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
    /// 媛留덉넡???щ즺 ?섎굹瑜??ｌ뼱 ?щ씪???붿껌??泥섎━?⑸땲??
    /// ?깃났?섎㈃ ?몃깽?좊━?먯꽌 1媛쒕? 鍮쇨퀬 ?꾨퉬 ?곹깭瑜?媛깆떊?⑸땲??
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
    /// ?ㅼ젣 議곕━?꾧뎄 ??븷?쒕? ?뺤씤?????щ즺瑜??뚮퉬?섍퀬 ?덉떆???먯젙???붿껌?⑸땲??
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
    /// Stage3 ?덇뎄??????ｋ뒗 ?쒓컙 ?≪쑝濡?諛붾줈 諛붾뚮뒗 ?뱀닔 議곕━?꾧뎄?낅땲??
    /// ?덉떆??留ㅻ땲? ?곌껐????뼱?몃룄 Game View 吏꾪뻾???딄린吏 ?딅룄濡? ?덇뎄+? 議고빀? ?ш린??利됱떆 ?꾩꽦 泥섎━?⑸땲??
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
    /// Stage3 ?덇뎄 ?꾩슜 吏곸젒 蹂?섏엯?덈떎.
    /// ? 李④컧怨???異붽?瑜?媛숈? ?몃깽?좊━ 由ъ뒪???덉뿉??利됱떆 泥섎━?? 以묎컙 ?먮굹 UI 媛깆떊 ?쒖꽌 ?뚮Ц???≪씠 ?щ씪吏???쇱쓣 留됱뒿?덈떎.
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
        OOTechItemModel riceItem = RequestInventoryItemModel(itemList, _riceIngredientId);

        if (riceItem == null || riceItem.ItemStackCount <= 0)
        {
            SetStatus("쌀이 부족합니다.");
            RequestLogInventorySnapshot("Julgu failed - rice missing");
            return true;
        }

        riceItem.ItemStackCount -= 1;

        if (riceItem.ItemStackCount <= 0)
            itemList.Remove(riceItem);

        OOTechItemModel cakeItem = RequestInventoryItemModel(itemList, _koreanCakeCookId);

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
    /// ?덇뎄 ?꾩슜 理쒖쥌 ?덉쟾 泥섎━?낅땲??
    /// 媛留덉넡/?꾨쭏 ?덉떆???먮쫫, ?좏깮 紐⑤뜽, Bridge ?곌껐???꾨? ?고쉶?섍퀬 ?꾩옱 HUD媛 ?쎈뒗 GameManager ?몃깽?좊━瑜?吏곸젒 ?섏젙?⑸땲??
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
        OOTechItemModel riceItem = RequestInventoryItemModel(itemList, _riceIngredientId);

        if (riceItem == null || riceItem.ItemStackCount <= 0)
            return false;

        riceItem.ItemStackCount -= 1;

        if (riceItem.ItemStackCount <= 0)
            itemList.Remove(riceItem);

        OOTechItemModel cakeItem = RequestInventoryItemModel(itemList, _koreanCakeCookId);

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

    private OOTechItemModel RequestInventoryItemModel(List<OOTechItemModel> itemList, string itemDataId)
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
    /// ?덇뎄 ?꾩꽦 吏곹썑 HUD媛 媛숈? ?꾨젅?꾩뿉???댁쟾 紐⑸줉??遺숈옟??寃쎌슦媛 ?덉뼱, 吏㏐쾶 ?ы솗??猷⑦떞???뚮┰?덈떎.
    /// 臾대? 鍮꾩쑀濡쒕뒗 ?뚰뭹 ?대떦?먭? ?≪쓣 李쎄퀬???ｌ? ??媛앹꽍 吏꾩뿴?源뚯? ?щ씪?붾뒗吏 ??踰??뺤씤?섎뒗 ?덉감?낅땲??
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
    /// GameManager.AddItem ?몄텧 ?ㅼ뿉???섎웾???섏? ?딆쑝硫??몃깽?좊━ 紐⑤뜽??吏곸젒 ?쎌엯?⑸땲??
    /// Game View 吏꾪뻾??留됰뒗 ?듭떖 蹂댁긽? ?댁쨷 ?덉쟾?μ튂濡?蹂댁옣?⑸땲??
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
    /// CookingGroup ?곷떒???덉떆??議고빀 踰꾪듉怨???移몄쭨由?議고빀?먯쓣 以鍮꾪빀?덈떎.
    /// Game View?먯꽌??ChoicePanel ????뚮젅?댁뼱媛 吏곸젒 ?↔낵 轅???щ젮 轅?≪쓣 留뚮뱶???묒? 議고빀??낅땲??
    /// </summary>
    private void EnsureHoneyCakeCombineUI()
    {
        if (Rect_Root == null || Button_HoneyCakeCombine != null)
            return;

        GameObject buttonObject = CreateCookingUIObject(Rect_Root, "Button_HoneyCakeCombine");
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetCookingUIRect(buttonRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-190f, -150f), new Vector2(260f, 64f));
        Image buttonImage = RequestGetOrAddImage(buttonObject, new Color(1f, 0.92f, 0.45f, 0.96f));
        buttonImage.raycastTarget = true;
        Button_HoneyCakeCombine = RequestGetOrAddButton(buttonObject);
        Button_HoneyCakeCombine.onClick.RemoveAllListeners();
        Button_HoneyCakeCombine.onClick.AddListener(ToggleHoneyCakeCombinePanel);
        TextMeshProUGUI buttonText = CreateCookingUIText(buttonObject.transform, "Text_Label", "레시피 조합", 30f, Color.black);
        SetCookingUIRect(buttonText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Root_HoneyCakeCombinePanel = CreateCookingUIObject(Rect_Root, "Panel_HoneyCakeCombine");
        RectTransform panelRect = Root_HoneyCakeCombinePanel.GetComponent<RectTransform>();
        SetCookingUIRect(panelRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-320f, -250f), new Vector2(560f, 130f));
        Image panelImage = RequestGetOrAddImage(Root_HoneyCakeCombinePanel, new Color(0f, 0f, 0f, 0.58f));
        panelImage.raycastTarget = true;

        Image_HoneyCakeSlotA = CreateCombineSlot(Root_HoneyCakeCombinePanel.transform, "Image_CombineSlotA", new Vector2(-126f, -6f));
        TextMeshProUGUI plusText = CreateCookingUIText(Root_HoneyCakeCombinePanel.transform, "Text_Plus", "+", 42f, Color.white);
        SetCookingUIRect(plusText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -6f), new Vector2(50f, 72f));
        Image_HoneyCakeSlotB = CreateCombineSlot(Root_HoneyCakeCombinePanel.transform, "Image_CombineSlotB", new Vector2(126f, -6f));
        Text_HoneyCakeGuide = CreateCookingUIText(Root_HoneyCakeCombinePanel.transform, "Text_Guide", "떡과 꿀, 또는 떡과 당근을 올려 조합하세요.", 22f, Color.white);
        SetCookingUIRect(Text_HoneyCakeGuide.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(520f, 32f));

        Root_HoneyCakeCombinePanel.SetActive(false);
        UpdateHoneyCakeCombineUnlockState();
    }

    /// <summary>
    /// 轅??議고빀 踰꾪듉? EncounterGroup?먯꽌 遺?뚯쑝濡??ㅼ뼱???λ㈃?먯꽌留??댁뼱 ?〓땲??
    /// 臾대? 鍮꾩쑀濡쒕뒗 ?곌뎔 ?λ㈃?먯꽌留??곕뒗 ?뱀닔 ?뚰뭹?대씪, ?ㅻⅨ 怨듭뿰?먯꽌???뚰뭹?⑥쓣 ?좉? ?먮뒗 泥섎━?낅땲??
    /// </summary>
    private void UpdateHoneyCakeCombineUnlockState()
    {
        bool isUnlocked = IsRecipeCombineUnlocked();

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

        if (!IsRecipeCombineUnlocked())
        {
            Root_HoneyCakeCombinePanel.SetActive(false);
            return;
        }

        bool isActive = !Root_HoneyCakeCombinePanel.activeSelf;
        Root_HoneyCakeCombinePanel.SetActive(isActive);

        if (isActive)
            SetStatus("떡과 꿀 또는 당근을 조합 칸에 올려주세요.");
    }

    private bool TryDropIngredientToHoneyCakeCombine(string itemDataId, Vector2 screenPosition)
    {
        if (!IsRecipeCombineUnlocked() || Root_HoneyCakeCombinePanel == null || !Root_HoneyCakeCombinePanel.activeSelf)
            return false;

        if (itemDataId != _koreanCakeCookId && itemDataId != _honeyIngredientId && itemDataId != _carrotIngredientId)
        {
            SetStatus("레시피 조합에는 떡, 꿀, 당근만 넣을 수 있습니다.");
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
        bool hasCarrot = _honeyCakeSlotAItemId == _carrotIngredientId || _honeyCakeSlotBItemId == _carrotIngredientId;

        if (!hasKoreanCake || OOTechGameManager.Inst == null)
            return;

        string resultItemId = string.Empty;
        string resultText = string.Empty;

        if (hasHoney)
        {
            resultItemId = _honeyKoreanCakeCookId;
            resultText = "꿀떡 1개 완성! 인벤토리에 추가되었습니다.";
        }
        else if (hasCarrot)
        {
            resultItemId = _carrotStarchCookId;
            resultText = "당근전분 1개 완성! 인벤토리에 추가되었습니다.";
        }

        if (string.IsNullOrEmpty(resultItemId))
            return;

        RequestForceAddInventoryItem(resultItemId, 1);
        RequestMoveInventoryItemToTop(resultItemId);
        _honeyCakeSlotAItemId = string.Empty;
        _honeyCakeSlotBItemId = string.Empty;
        ClearHoneyCakeSlot(Image_HoneyCakeSlotA);
        ClearHoneyCakeSlot(Image_HoneyCakeSlotB);
        Root_HoneyCakeCombinePanel.SetActive(false);
        RefreshInventorySlots();
        SetInventoryNewBadgeActive(true);
        NotifyRoadHUDInventoryNewBadge();
        RequestOpenCookingSupportHUDIfNeeded();
        SetStatus(resultText);
        RequestLogInventorySnapshot($"After RecipeCombine {resultItemId}");
    }

    /// <summary>
    /// ?덉떆??議고빀?먯? Stage3 ?곌뎔 議곗슦? Stage4 ?좊겮 ?섏뒪??遺?뚯뿉?쒕쭔 ?대┰?덈떎.
    /// ?곹솕濡?移섎㈃ ?뱀닔 ?뚰뭹???꾩슂???λ㈃?먯꽌留?臾대? ?꾩뿉 ?щ젮 ?ㅻⅨ ?λ㈃??議곗옉??諛⑺빐?섏? ?딄쾶 ?섎뒗 ?μ튂?낅땲??
    /// </summary>
    private bool IsRecipeCombineUnlocked()
    {
        return IsOpenedFromEncounterGroup() || IsOpenedFromStage4_2Group();
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
    /// 諛⑷툑 留뚮뱺 ?꾩씠?쒖쓣 ?몃깽?좊━ 留??꾨줈 ?щ┰?덈떎.
    /// Game View?먯꽌?????щ’???ㅽ겕濡??꾨옒???⑥뼱 蹂댁씠吏 ?딅뒗 ?곹솴??留됰뒗 ?μ튂?낅땲??
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
    /// 議곕━ 吏곹썑 ?몃깽?좊━ 紐⑤뜽 ?꾩껜瑜?濡쒓렇濡??④퉩?덈떎.
    /// ?≪씠 紐⑤뜽???ㅼ뼱媛붾뒗吏, UI ?щ’ ?앹꽦?먯꽌 鍮좎??붿? 遺꾨━?댁꽌 ?뺤씤?⑸땲??
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
    /// ?꾩옱源뚯? ?ㅼ뼱媛??щ즺 議고빀???꾩꽦 ?덉떆?쇱씤吏 ?뺤씤?섍퀬, ?깃났 ???꾩꽦 ?뚯떇???몃깽?좊━???ｌ뒿?덈떎.
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
            SetStatus("올바르지 않은 조리도구입니다!");
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
        SetStatus($"{cookName} 완성! 인벤토리에 음식이 들어갔습니다.");
    }

    /// <summary>
    /// 吏湲??좏깮???щ즺媛 ?덉떆?쇱쓽 以묎컙 ?④퀎濡??덉슜?섎뒗吏 ?뺤씤?⑸땲??
    /// </summary>
    /// <summary>
    /// ?щ즺媛 ?대떦 議곕━?꾧뎄????븷?쒖? 留욌뒗吏 ?뺤씤?⑸땲??
    /// </summary>
    /// <summary>
    /// ?몃컯二??쒗넗由ъ뼹? ? 10媛쒖? ?몃컯 10媛쒕? ??踰덉뿉 議곕━?섎뒗 ?λ㈃?낅땲??
    /// 泥??щ즺 1媛쒖뵫? ?대? ?쒓굅?섏뿀?쇰?濡? ?⑥? ?щ즺瑜??뺤씤??異붽? 9?뚮텇源뚯? ?④퍡 李④컧?⑸땲??
    /// </summary>
    private int CalculateCookingResultCount(string resultItemId)
    {
        return _recipeService.RequestCalculateResultCount(resultItemId, _selectionModel);
    }

    /// <summary>
    /// 議곕━?꾧뎄???щ씪媛??щ즺 ?대쫫?쒖? ?ㅼ젣 媛쒖닔瑜?湲곕줉?⑸땲??
    /// 媛숈? ?щ즺瑜??щ윭 踰??ｌ뼱???덉떆???먯젙???대쫫?쒕뒗 ?섎굹留??먭퀬, ?섎웾留??꾩쟻?⑸땲??
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
    /// 議곕━?꾧뎄???щ씪媛??뱀젙 ?щ즺??媛쒖닔瑜?諛섑솚?⑸땲??
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
    /// ?꾩꽦 ?꾨낫 ?덉떆?쇨? ?붽뎄??議곕━?꾧뎄? ?ㅼ젣 ?쒕∼ ?꾩튂媛 ?쇱튂?섎뒗吏 ?뺤씤?⑸땲??
    /// ? ?섎굹留?蹂닿퀬 ?≪쓣 留뚮뱾??踰꾧렇瑜?留됯퀬, ???Julgu ?꾩뿉 ?덉쓣 ?뚮쭔 ???덉떆?쇰? ?듦낵?쒗궢?덈떎.
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
    /// ?좏깮 ?섎웾???꾩꽦???꾩슂???묐낫??留롮쑝硫??⑥? ?щ즺瑜??몃깽?좊━濡??뚮젮以띾땲??
    /// 媛먮룆 鍮꾩쑀濡쒕뒗 ?붾━???곗? ?딆? ?뚰뭹???뚰뭹 李쎄퀬濡??섎룎????퉬瑜?留됰뒗 ?λ㈃?낅땲??
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
    /// ?꾩꽦 ?뚯떇 ?섎굹???ㅼ젣濡??뚮퉬?섎뒗 ?щ즺 ?섎웾?쒕? 留뚮벊?덈떎.
    /// </summary>
    private Dictionary<string, int> CreateUsedIngredientAmountDic(string resultItemId, int resultCount)
    {
        return _recipeService.RequestCreateUsedIngredientAmountDic(resultItemId, resultCount, _selectionModel);
    }

    /// <summary>
    /// ?꾩쭅 ?섎웾??遺議깊븷 ???뚮젅?댁뼱?먭쾶 ?꾩슂??議곗옉???덈궡?⑸땲??
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
    /// ?곗씠?곌? ?꾩쭅 ?꾩꽦?섏? ?딆븯???뚮룄 ?+梨꾩냼/?몃컯 ?쒗넗由ъ뼹???숈옉?섎룄濡??섎뒗 ?덉쟾留앹엯?덈떎.
    /// </summary>
    private bool IsFallbackIngredientAccepted(string itemDataId)
    {
        if (itemDataId != _riceIngredientId &&
            itemDataId != _vegetableIngredientId &&
            itemDataId != _pumpkinIngredientId &&
            itemDataId != _kimchIngredientId &&
            itemDataId != _chiliPepperIngredientId &&
            itemDataId != _carrotIngredientId &&
            itemDataId != _koreanCakeCookId &&
            itemDataId != _carrotStarchCookId)
            return false;

        if (itemDataId == _vegetableIngredientId ||
            itemDataId == _pumpkinIngredientId ||
            itemDataId == _chiliPepperIngredientId ||
            itemDataId == _carrotIngredientId)
        {
            return !_selectionModel.SelectedIngredientIdList.Contains(_vegetableIngredientId) &&
                   !_selectionModel.SelectedIngredientIdList.Contains(_pumpkinIngredientId) &&
                   !_selectionModel.SelectedIngredientIdList.Contains(_chiliPepperIngredientId) &&
                   !_selectionModel.SelectedIngredientIdList.Contains(_carrotIngredientId);
        }

        if (itemDataId == _riceIngredientId || itemDataId == _kimchIngredientId)
            return !_selectionModel.SelectedIngredientIdList.Contains(_riceIngredientId) && !_selectionModel.SelectedIngredientIdList.Contains(_kimchIngredientId);

        if (itemDataId == _koreanCakeCookId)
            return !_selectionModel.SelectedIngredientIdList.Contains(_koreanCakeCookId);

        if (itemDataId == _carrotStarchCookId)
            return !_selectionModel.SelectedIngredientIdList.Contains(_carrotStarchCookId);

        return !_selectionModel.SelectedIngredientIdList.Contains(itemDataId);
    }

    /// <summary>
    /// ?붾━ 留ㅻ땲?媛 ?ъ뿉 ?놁쑝硫?理쒖냼 ?숈옉???꾪빐 留ㅻ땲? ?ㅻ툕?앺듃瑜?以鍮꾪빀?덈떎.
    /// </summary>
    private void EnsureCookingManager()
    {
        if (OOTechCookingManager.Inst != null)
            return;

        Debug.LogError("[OOTechCookingGroupController] OOTechCookingManager is missing. 씬에 배치된 CookingManager 오브젝트를 확인하세요.");
    }

    /// <summary>
    /// CookingUIGroup??View 而댄룷?뚰듃?먯꽌 留먰뭾?? ?쒕∼ ?곸뿭, ?띿뒪???뚰뭹???곌껐?⑸땲??
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
    /// 遺??臾대???蹂댁“ ?대떦 而댄룷?뚰듃瑜?李얘퀬 ?꾩슂??UI 李몄“瑜??섍퉩?덈떎.
    /// 媛먮룆??紐⑤뱺 ?덈궡?먯쓣 吏곸젒 ?ㅼ? ?딄퀬, ?몃깽?좊━ ?곕씫 ?대떦怨?寃곌낵 ?곗텧 ?대떦?먭쾶 ??븷?쒕? 遺숈씠???④퀎?낅땲??
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
    /// CookingUIGroup 猷⑦듃 Canvas媛 0 ?ㅼ??쇱씠???섎せ??Rect濡???λ릺?덉쓣 ???붾㈃ ?꾩껜 UI 湲곗??쇰줈 蹂듦뎄?⑸땲??
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
    /// CookingGroup ?먯떇??Cauldron ?ㅻ툕?앺듃??Transform, Renderer, Collider瑜?李얠뒿?덈떎.
    /// </summary>
    private void ResolveCauldronReference()
    {
        if (Transform_Cauldron != null && Transform_Cauldron.gameObject.scene.IsValid())
            return;

        Transform_Cauldron = RequestChildObjectByName(transform, _cauldronObjectName);
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
    /// CookingGroup ?먯떇 Cuttingboard ?ㅻ툕?앺듃??Transform, Renderer, Collider, ??븷?쒕? 李얠뒿?덈떎.
    /// </summary>
    private void ResolveCuttingboardReference()
    {
        if (Transform_Cuttingboard != null && Transform_Cuttingboard.gameObject.scene.IsValid())
            return;

        Transform_Cuttingboard = RequestChildObjectByName(transform, _cuttingboardObjectName);
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
    /// CookingGroup ?먯떇 Julgu ?ㅻ툕?앺듃??Transform, Renderer, Collider, ??븷?쒕? 李얠뒿?덈떎.
    /// ?덇뎄??Stage3遺??異붽??섎뒗 議곕━?꾧뎄?대?濡??놁쑝硫?寃쎄퀬 ?놁씠 湲곗〈 ?붾━ ?먮쫫???좎??⑸땲??
    /// </summary>
    private void ResolveJulguReference()
    {
        if (Transform_Julgu != null && Transform_Julgu.gameObject.scene.IsValid())
            return;

        Transform_Julgu = RequestChildObjectByName(transform, _julguObjectName);
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
    /// ?붾㈃ 醫뚰몴瑜??ㅼ젣 ?붾뱶 醫뚰몴濡?諛붽퓭 媛留덉넡 Collider ?먮뒗 Sprite Bounds ?덉씤吏 ?뺤씤?⑸땲??
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
    /// ?ㅼ젣 媛留덉넡 ?ш린瑜??붾㈃ UI 醫뚰몴濡?蹂?섑빐 ?쒕∼ ?곸뿭??留욎땅?덈떎.
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
    /// 濡쒕뱶 HUD???몃깽?좊━ ?⑤꼸???ㅼ떆 洹몃━?꾨줉 ?붿껌?⑸땲??
    /// </summary>
    private void RefreshInventorySlots()
    {
        NotifyRoadHUDInventoryRefresh();
    }

    /// <summary>
    /// 媛留덉넡 ?덉뿉 ?ㅼ뼱媛??щ즺 紐⑸줉???띿뒪?몃줈 蹂댁뿬以띾땲??
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
    /// 泥섏쓬 遺?뚯뿉 ?ㅼ뼱?붿쓣 ??媛留덉넡 ?ъ슜踰?留먰뭾?좉낵 ?붿궡?쒕? 蹂댁뿬以띾땲??
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
    /// Stage3?먯꽌 ?덈줈 ?대┛ ?덇뎄 ?ъ슜踰뺣쭔 ?⑤룆?쇰줈 ?덈궡?⑸땲??
    /// Game View?먯꽌??湲곗〈 ?덈궡 留먰뭾?좎쓣 ?ъ궗?⑺븯怨??붿궡?쒕쭔 Julgu ?꾨줈 ??퉩?덈떎.
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
        SetStatus("절구 안내를 확인했습니다. 절구를 클릭하면 쌀 1개가 떡 1개로 바뀝니다.");
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
    /// Tutorial ?곗씠?곗뿉??媛留덉넡 ?ㅻ챸???쎄퀬, ?놁쑝硫?湲곕낯 ?덈궡 臾멸뎄瑜??ъ슜?⑸땲??
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
        string fallbackDescription = "가마솥은 쌀처럼 끓여야 하는 재료를 받습니다.\n기본은 하나씩 집습니다. 여러 개를 집으려면 인벤토리 위에서 Ctrl+마우스 휠로 수량을 조절하세요.\n쌀과 채소가 준비되면 야채죽이 완성됩니다.";

        if (Cue_Guide != null)
        {
            Cue_Guide.RequestGetGuideData(_cauldronTutorialId, "가마솥", fallbackDescription, out title, out description);
            return;
        }

        title = "가마솥";
        description = fallbackDescription;
    }

    /// <summary>
    /// 留먰뭾???쒕ぉ怨?蹂몃Ц ?띿뒪?몃? ?곸슜?⑸땲??
    /// </summary>
    private void GetCuttingboardGuideData(out string title, out string description)
    {
        string fallbackDescription = "도마는 채소처럼 손질해야 하는 재료를 받습니다.\n가마솥에 쌀, 도마에 채소가 준비되면 야채죽이 완성됩니다.";

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
        string fallbackDescription = "절구는 마우스 클릭으로 인벤토리의 쌀을 떡으로 만들 수 있습니다!";

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
    /// ?붾━ ?깃났泥섎읆 ?ㅼ쓬 而룹쑝濡?諛붾줈 ?섏뼱媛???????꾩옱 ?덈궡 留먰뭾?좉낵 ?붿궡?쒕? ?뺣━?⑸땲??
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
    /// ?ъ뿉 諛곗튂??媛留덉넡 ?붿궡?쒕쭔 耳쒓퀬 ?꾩튂瑜?媛깆떊?⑸땲??
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
    /// ?ㅼ젣 Cauldron ?ㅻ툕?앺듃???쀫?遺꾩쓣 UI 濡쒖뺄 醫뚰몴濡?諛붽퓠?덈떎.
    /// ?붿궡?쒓? ?쒕∼ ?곸뿭???꾨땲??媛留덉넡 ?뚰뭹 諛붾줈 ?꾨? 媛由ы궎寃??섎뒗 湲곗??먯엯?덈떎.
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
    /// 遺???덉쓽 NEW 諛곗?瑜?耳쒓굅???꾧퀬, 耳쒖쭏 ?뚮뒗 源쒕묀?닿쾶 ?⑸땲??
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
    /// ?대젮 ?덈뒗 Road HUD?ㅼ뿉寃??몃깽?좊━ NEW 諛곗?瑜?耳??щ씪怨??뚮┰?덈떎.
    /// </summary>
    private void NotifyRoadHUDInventoryNewBadge()
    {
        if (Bridge_Inventory != null)
        {
            Bridge_Inventory.RequestNotifyRoadHUDInventoryNewBadge();
            return;
        }

        List<OOTechRoadHUDController> hudControllerArray = OOTechSceneQuery.RequestCollectComponents<OOTechRoadHUDController>(true);

        foreach (OOTechRoadHUDController hudController in hudControllerArray)
        {
            if (hudController == null || !hudController.gameObject.activeInHierarchy)
                continue;

            hudController.RequestRefreshInventoryView();
            hudController.SetInventoryNewBadgeActive(true);
        }
    }

    /// <summary>
    /// ?대젮 ?덈뒗 Road HUD?ㅼ뿉寃??몃깽?좊━ 紐⑸줉???ㅼ떆 洹몃젮 ?щ씪怨??뚮┰?덈떎.
    /// </summary>
    private void NotifyRoadHUDInventoryRefresh()
    {
        if (Bridge_Inventory != null)
        {
            Bridge_Inventory.RequestNotifyRoadHUDInventoryRefresh();
            return;
        }

        List<OOTechRoadHUDController> hudControllerArray = OOTechSceneQuery.RequestCollectComponents<OOTechRoadHUDController>(true);

        foreach (OOTechRoadHUDController hudController in hudControllerArray)
        {
            if (hudController == null || !hudController.gameObject.activeInHierarchy)
                continue;

            hudController.RequestRefreshInventoryView();
        }
    }

    /// <summary>
    /// ?쇱콈二??쒖옉???앸궗?ㅺ퀬 Road HUD???뚮젮 ?꾨Т??泥댄겕 ?쒖떆瑜?媛깆떊?⑸땲??
    /// </summary>
    private void NotifyRoadHUDCookingQuestComplete()
    {
        if (Bridge_Inventory != null)
            Bridge_Inventory.RequestNotifyRoadHUDCookingQuestComplete();
    }

    /// <summary>
    /// ?⑹갹 ??ш? ?앸궃 ???뚮젅?댁뼱?먭쾶 ?꾨Т媛 ?꾨즺?섏뿀?뚯쓣 吏㏐쾶 ?덈궡?⑸땲??
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
    /// ?붾━ ?꾩꽦 ?????몃Ъ???④퍡 留먰븯??DialogueGroup ??щ? ?쒖옉?⑸땲??
    /// </summary>
    private void RequestPlayCookingCompleteDialogue(string resultItemId)
    {
        if (Presenter_Result != null)
            Presenter_Result.RequestPlayCookingCompleteDialogue(resultItemId, ConsumeCookingCompleteResult, CompleteCookingTutorialMission);
    }

    /// <summary>
    /// 泥??붾━ ?쒗넗由ъ뼹???꾩꽦 ?뚯떇? ??щ줈 癒뱀? ???몃깽?좊━?먯꽌 1媛??쒓굅?⑸땲??
    /// Game View?먯꽌??"??癒뱀뿀?듬땲?? ??ш? ?앸궃 ?ㅼ쓬 ?쇱콈二??щ’???щ씪吏묐땲??
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
    /// 遺??移대찓?쇨? Kitchen 諛곌꼍 ?꾩껜瑜??대룄濡?Orthographic Size? ?꾩튂瑜?議곗젙?⑸땲??
    /// </summary>
    private void ApplyKitchenCameraView()
    {
        ResolveCameraReference();

        if (Camera_Main == null)
            return;

        SpriteRenderer kitchenRenderer = RequestChildRenderer("Kitchen");

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
    /// CookingGroup???대┫ ??遺??臾대?? 議곕━?꾧뎄 SpriteRenderer瑜?媛???욎そ?쇰줈 ?щ┰?덈떎.
    /// Game View?먯꽌???댁쟾 Road/Stage 諛곌꼍???ㅼ뿉 ?⑥븘 ?덉뼱??Kitchen ?붾㈃????씠吏 ?딄쾶 ?섎뒗 蹂댄뿕 ?μ튂?낅땲??
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
    /// 遺?뚯쓣 ?レ쓣 ???댁쟾 濡쒕뱶 ?λ㈃??移대찓???꾩튂? Follow ?곹깭濡?蹂듦뎄?⑸땲??
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
    /// 移대찓?쇰? 遺?뚯슜?쇰줈 諛붽씀湲??꾩뿉 ?먮옒 ?곹깭瑜???踰???ν빀?덈떎.
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

    private SpriteRenderer RequestChildRenderer(string childName)
    {
        Transform childTransform = RequestChildObjectByName(transform, childName);

        if (childTransform != null && childTransform.TryGetComponent(out SpriteRenderer childRenderer))
            return childRenderer;

        return GetComponentInChildren<SpriteRenderer>(true);
    }

    private Transform RequestChildObjectByName(Transform rootTransform, string childName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == childName)
            return rootTransform;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            Transform foundTransform = RequestChildObjectByName(rootTransform.GetChild(index), childName);

            if (foundTransform != null)
                return foundTransform;
        }

        return null;
    }

    private GameObject RequestSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

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
            return "생선";

        if (itemDataId == "Ing_Kimch_01")
            return "김치";

        if (itemDataId == "OO_PumpkinSoup_1")
            return "호박죽";

        if (itemDataId == "OO_VegetableSoup_1")
            return "야채죽";

        if (itemDataId == "OO_GrilledFishMeal_1")
            return "생선구이 정식";

        if (itemDataId == "OO_KimchiStew_1")
            return "김치찌개";

        return string.IsNullOrEmpty(itemDataId) ? "알 수 없는 아이템" : itemDataId;
    }

    /// <summary>
    /// 遺???섎떒 ?곹깭 臾멸뎄瑜?媛깆떊?⑸땲??
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


