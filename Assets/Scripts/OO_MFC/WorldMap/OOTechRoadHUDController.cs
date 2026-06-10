// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechRoadHUDController.cs
// - 역할: 월드맵, 도로, 스테이지 진입, HUD 흐름을 연결합니다.
// - 유지보수: UIManager 전환과 그룹 활성/비활성 순서가 게임 진행을 결정하므로 호출 순서를 유지합니다.
// =============================================================================
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
/// RoadGroup怨?StageGroup??怨듭쑀?섎뒗 HUD瑜?吏?섑빀?덈떎.
/// HUDUIGroup?대씪???ㅼ젣 ???뚰뭹? ?ъ슜?먭? ?몄쭛?섍퀬, ??而⑦듃濡ㅻ윭??踰꾪듉/?⑤꼸/NEW 諛곗????먮쭔 泥섎━?⑸땲??
/// </summary>
[DisallowMultipleComponent]
public class OOTechRoadHUDController : MonoBehaviour
{
    // ?쎈뒗 ?쒖꽌:
    // 1. PrepareHUD: HUDUIGroup怨?View 李몄“瑜?以鍮꾪빀?덈떎.
    // 2. BindButtonEvents 怨꾩뿴: ?몃깽?좊━, ?꾧컧, ?꾨Т, ?붾━, ?붾뱶留?踰꾪듉???곌껐?⑸땲??
    // 3. RequestShowHUDGuide 怨꾩뿴: HUD ?쒗넗由ъ뼹 ?붿궡?쒖? ?덈궡 ?⑤꼸??吏꾪뻾?⑸땲??
    // 4. RefreshInventoryView 怨꾩뿴: ?뚮젅?댁뼱 Model???꾩씠?쒖쓣 ?붾㈃ ?щ’?쇰줈 蹂댁뿬以띾땲??
    // 5. RequestSetStageQuestMission 怨꾩뿴: ?꾨Т ?띿뒪?몄? NEW ?쒖떆瑜?媛깆떊?⑸땲??
    // ?좎?蹂댁닔 二쇱쓽:
    // - 踰꾪듉/?⑤꼸 諛곗튂??HUDUIGroup?먯꽌 吏곸젒 ?섏젙?⑸땲??
    // - ???ㅽ겕由쏀듃??HUD ?먮쫫留?留↔퀬, ?ㅼ젣 UI ?ㅻ툕?앺듃 李몄“??OOTechRoadHUDView??紐⑥쓭?덈떎.
    // - ?뱀젙 RoadGroup ?꾩슜 ?곗텧???섏뼱?섎㈃ Road Controller 履쎌쑝濡???퉩?덈떎.

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

    [Header("Inventory Scroll")]
    [SerializeField] private float _inventorySlotHeight = 76f;
    [SerializeField] private float _inventorySlotSpacing = 10f;
    [SerializeField] private float _inventoryContentBottomPadding = 18f;

    [Header("Overlay View")]
    [SerializeField] private int _overlaySpriteSortingOrder = 3000;

    [Header("New Badge")]
    [SerializeField] private float _newBadgeBlinkSpeed = 7f;
    [SerializeField] private float _newBadgeMinimumAlpha = 0.25f;

    [Header("Mission Effect")]
    [SerializeField] private float _missionCompleteHoldDuration = 0.85f;
    [SerializeField] private float _missionCompleteFadeDuration = 1.65f;

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
    private ScrollRect Scroll_InventorySlots;

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
    private TextMeshProUGUI Text_WorldMapNewBadge;
    private TextMeshProUGUI Text_CookingNewBadge;
    private TextMeshProUGUI Text_CookingLabel;
    private TextMeshProUGUI Text_InventoryQuantityGuide;
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
    private bool _isStage4PresentationHUDLockedOpen;
    private bool _isStage4CookingSupportLockedOpen;
    private bool _isStage4PersistentGuideLockedOpen;
    private string _stage4CookingGuideTitle;
    private string _stage4CookingGuideBody;
    private bool _isCookingQuestActive;
    private bool _isCookingQuestComplete;
    private bool _isCookingMissionRemoved;
    private bool _isEastRoadMissionRemoved;
    private string _roadMissionText = "\uB3D9\uCABD\uC758 \uB9C8\uC744\uB85C \uAC00\uC2DC\uC624";
    private string _stageQuestMissionText;
    private float _missionCompleteEffectAlpha = 1f;
    private Coroutine _inventoryNewBadgeCoroutine;
    private Coroutine _codexNewBadgeCoroutine;
    private Coroutine _missionNewBadgeCoroutine;
    private Coroutine _worldMapNewBadgeCoroutine;
    private Coroutine _cookingNewBadgeCoroutine;
    private Coroutine _missionCompleteEffectCoroutine;
    private Coroutine _autoOpenNewPanelCoroutine;
    private Coroutine _forceCloseInventoryPanelCoroutine;
    private readonly List<OOTechRoadHUDButtonKind> _pendingAutoOpenButtonKindList = new List<OOTechRoadHUDButtonKind>();

    public bool IsCookingUnlocked => _isCookingUnlocked;
    public bool IsStage4PresentationHUDLockedOpen => _isStage4PresentationHUDLockedOpen;
    public bool IsStage4CookingSupportLockedOpen => _isStage4CookingSupportLockedOpen;
    public bool IsStage4PersistentGuideLockedOpen => _isStage4PersistentGuideLockedOpen;
    public bool IsOverlayOpen => _isOverlayOpen;
    public string OwnerGroupName => string.IsNullOrEmpty(_ownerGroupName) ? gameObject.name : _ownerGroupName;

    /// <summary>
    /// ??HUD媛 ?대뒓 RoadGroup ?먮뒗 StageGroup???뚯냽?몄? 湲곕줉?⑸땲??
    /// ?붾뱶留?遺?뚯뿉???뚯븘媛???諛붾줈 ??臾대?濡?蹂듦??섍린 ?꾪븳 ?대쫫?쒖엯?덈떎.
    /// </summary>
    public void SetOwnerGroupName(string ownerGroupName)
    {
        _ownerGroupName = ownerGroupName;
    }

    public void RequestEnableStage4PresentationHUD(string ownerGroupName, string missionText, string guideTitle, string guideBody)
    {
        RequestEnableStage4PresentationHUD(ownerGroupName, missionText, guideTitle, guideBody, false, false);
    }

    public void RequestEnableStage4PresentationHUD(string ownerGroupName, string missionText, string guideTitle, string guideBody, bool isForceCookingSupportOpen, bool isPersistentGuide)
    {
        if (!string.IsNullOrEmpty(ownerGroupName))
            _ownerGroupName = ownerGroupName;

        bool wasLockedOpen = _isStage4PresentationHUDLockedOpen;
        _isStage4PresentationHUDLockedOpen = true;

        if (isForceCookingSupportOpen)
            _isStage4CookingSupportLockedOpen = true;
        else if (!_isCookingOverlayOpen)
            _isStage4CookingSupportLockedOpen = false;

        if (isPersistentGuide)
            _isStage4PersistentGuideLockedOpen = true;
        else if (!_isCookingOverlayOpen)
            _isStage4PersistentGuideLockedOpen = false;

        if (!wasLockedOpen)
            PrepareHUD();

        SetHUDVisible(true);
        SetBottomHUDActive(true);
        SetCookingUnlocked(true, true);
        RefreshInventoryView();

        if (!string.IsNullOrWhiteSpace(missionText))
        {
            _isEastRoadMissionRemoved = true;
            _stageQuestMissionText = missionText;
            RefreshMissionText();
            SetMissionNewBadgeActive(true, false);
        }

        if (isForceCookingSupportOpen)
        {
            SetInventoryPanelActive(true);
            SetMissionPanelActive(true);
        }

        bool hasGuideText = !string.IsNullOrWhiteSpace(guideTitle) || !string.IsNullOrWhiteSpace(guideBody);

        if (hasGuideText)
        {
            _stage4CookingGuideTitle = guideTitle;
            _stage4CookingGuideBody = guideBody;
        }

        if (hasGuideText)
        {
            if (isPersistentGuide)
                ShowPersistentRoadMessageTopCenter(guideTitle, guideBody);
            else if (_isCookingOverlayOpen || isForceCookingSupportOpen)
                ShowPersistentRoadMessageTopCenter(guideTitle, guideBody);
            else
                CloseHUDGuide();
        }
        else if (!isPersistentGuide)
        {
            if (!_isCookingOverlayOpen)
                CloseHUDGuide();
        }
    }

    /// <summary>
    /// HUDUIGroup??踰꾪듉怨??⑤꼸???곌껐?섍퀬 ?쒖옉 ?쒖떆 ?곹깭瑜??뺣━?⑸땲??
    /// Game View?먯꽌???섎떒 HUD媛 耳쒖?怨??몃깽?좊━/?꾨Т ?⑤꼸? ?ロ엺 ?곹깭媛 ?⑸땲??
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
        SetWorldMapNewBadgeActive(false);
        SetCookingNewBadgeActive(false);
        RefreshCookingButtonView();
        SetBottomHUDActive(true);

        if (IsOwnerRoadGroup() && OOTechGameManager.Inst != null && OOTechGameManager.Inst.ConsumePendingWorldMapNewBadge())
            SetWorldMapNewBadgeActive(true);
    }

    /// <summary>
    /// HUD ?꾩껜瑜?蹂댁씠嫄곕굹 ?④퉩?덈떎.
    /// ?붾뱶留듭뿉?쒕뒗 HUD瑜??꾧퀬, 遺?뚯뿉?쒕뒗 ?꾩슂???⑤꼸留??ㅼ떆 ?대┰?덈떎.
    /// </summary>
    public void SetHUDVisible(bool isVisible)
    {
        if (_isStage4PresentationHUDLockedOpen)
            isVisible = true;

        if (isVisible)
            RequestActivateParentChain(Root_HUD);

        if (Root_HUD != null)
            Root_HUD.SetActive(isVisible);
    }

    public void RequestForceHideForCutScene()
    {
        _isStage4PresentationHUDLockedOpen = false;
        _isStage4CookingSupportLockedOpen = false;
        _isStage4PersistentGuideLockedOpen = false;
        _stage4CookingGuideTitle = null;
        _stage4CookingGuideBody = null;
        _isCookingOverlayOpen = false;
        Controller_CookingOverlay = null;
        CloseHUDGuide();
        SetInventoryPanelActive(false);
        SetMissionPanelActive(false);

        if (Root_HUD != null)
            Root_HUD.SetActive(false);
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
    /// ?붾━?섍린 踰꾪듉???좉툑 ?곹깭瑜?媛깆떊?⑸땲??
    /// 1st_Road_to_Stage1??泥?留듭뿉?쒕뒗 ?좉꺼 ?덈떎媛 RoadMap1 ?댄썑 ?대┰?덈떎.
    /// </summary>
    public void SetCookingUnlocked(bool isUnlocked, bool isShowNewBadge = true)
    {
        bool wasUnlocked = _isCookingUnlocked;
        _isCookingUnlocked = isUnlocked;
        RefreshCookingButtonView();

        if (_isCookingUnlocked && !wasUnlocked && isShowNewBadge)
            SetCookingNewBadgeActive(true);
        else if (!_isCookingUnlocked)
            SetCookingNewBadgeActive(false);
    }

    /// <summary>
    /// ?몃깽?좊━????臾쇨굔???ㅼ뼱?붾떎??NEW 諛곗?瑜?耳쒓굅???뺣땲??
    /// </summary>
    public void SetInventoryNewBadgeActive(bool isActive)
    {
        SetBadgeActive(Text_InventoryNewBadge, isActive, ref _inventoryNewBadgeCoroutine);

        if (isActive)
            RequestAutoOpenNewPanel(OOTechRoadHUDButtonKind.Inventory);
    }

    /// <summary>
    /// ?꾧컧 媛깆떊???뚮━??NEW 諛곗?瑜?耳쒓굅???뺣땲??
    /// </summary>
    public void SetCodexNewBadgeActive(bool isActive)
    {
        SetBadgeActive(Text_CodexNewBadge, isActive, ref _codexNewBadgeCoroutine);
    }

    /// <summary>
    /// ?꾨Т 媛깆떊???뚮━??NEW 諛곗?瑜?耳쒓굅???뺣땲??
    /// </summary>
    public void SetMissionNewBadgeActive(bool isActive, bool isAutoOpenPanel = true)
    {
        SetBadgeActive(Text_MissionNewBadge, isActive, ref _missionNewBadgeCoroutine);

        if (isActive && isAutoOpenPanel)
            RequestAutoOpenNewPanel(OOTechRoadHUDButtonKind.Mission);
    }

    /// <summary>
    /// 월드맵 진행도 갱신을 알려 주는 NEW 뱃지를 켜거나 끕니다.
    /// 영화 비유로는 다음 촬영지 지도가 바뀌었을 때 관객에게 한 번 지도를 펼쳐 보여 주는 신호입니다.
    /// </summary>
    public void SetWorldMapNewBadgeActive(bool isActive)
    {
        SetBadgeActive(Text_WorldMapNewBadge, isActive, ref _worldMapNewBadgeCoroutine);

        if (isActive && IsOwnerRoadGroup())
            RequestAutoOpenNewPanel(OOTechRoadHUDButtonKind.WorldMap);
    }

    /// <summary>
    /// 월드맵 NEW 자동 오픈은 Stage 클리어 연출 중이 아니라 RoadGroup으로 복귀했을 때만 허용합니다.
    /// Game View에서는 클리어 패널을 가리지 않고, 다음 이동 화면에서만 새 지도를 한 번 보여 줍니다.
    /// </summary>
    private bool IsOwnerRoadGroup()
    {
        string ownerName = string.IsNullOrEmpty(_ownerGroupName) ? gameObject.name : _ownerGroupName;
        return !string.IsNullOrEmpty(ownerName) && ownerName.Contains("Road_to_Stage");
    }

    /// <summary>
    /// ?붾━?섍린媛 ?덈줈 ?대졇?ㅻ뒗 NEW 諛곗?瑜?耳쒓굅???뺣땲??
    /// ?곹솕濡?鍮꾩쑀?섎㈃ ?ロ? ?덈뜕 遺???명듃 臾몄씠 ?대졇????愿媛앹뿉寃??묒? ?덈궡?깆쓣 耳?二쇰뒗 ??븷?낅땲??
    /// </summary>
    public void SetCookingNewBadgeActive(bool isActive)
    {
        ResolveCookingNewBadge();
        SetBadgeActive(Text_CookingNewBadge, isActive, ref _cookingNewBadgeCoroutine);
    }

    /// <summary>
    /// ?붾━ ?꾨Т媛 諛쒓툒?섎㈃ ?꾨Т?먯뿉 ?댁빞 ???쇱쓣 ?щ━怨?NEW ?쒖떆瑜?耳?땲??
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
    /// ?쇱콈二??쒖옉???앸굹硫?泥?踰덉㎏ ?꾨Т 以꾩뿉 痍⑥냼?좉낵 ?섏씠?쒖븘???곗텧???ъ깮?⑸땲??
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
    /// Stage1Group???꾩갑?덉쓣 ??Road ?꾨Т瑜??꾨즺 泥섎━?섍퀬 ??StageQuest 臾멸뎄瑜??꾨Т?먯뿉 ?쒖떆?⑸땲??
    /// </summary>
    public void RequestSetStageQuestMission(string stageQuestText)
    {
        PrepareHUD();
        _isEastRoadMissionRemoved = true;
        _stageQuestMissionText = stageQuestText;
        SetMissionNewBadgeActive(true);
        RefreshMissionText();
    }

    /// <summary>
    /// RoadGroup留덈떎 湲곕낯 湲??덈궡 ?꾨Т瑜?援먯껜?⑸땲??
    /// ?곹솕濡?移섎㈃ ?ㅼ쓬 珥ъ쁺 ?μ냼瑜??곸? 肄쒖떆?몃? HUD 諛곗슦?먭쾶 ?덈줈 遺숈씠???λ㈃?낅땲??
    /// </summary>
    public void RequestSetRoadMissionText(string roadMissionText, bool isShowNewBadge)
    {
        RequestSetRoadMissionText(roadMissionText, isShowNewBadge, true);
    }

    public void RequestSetRoadMissionText(string roadMissionText, bool isShowNewBadge, bool isAutoOpenMissionPanel)
    {
        if (Text_MissionContent == null)
            PrepareHUD();

        _roadMissionText = roadMissionText;
        _isEastRoadMissionRemoved = string.IsNullOrWhiteSpace(_roadMissionText);
        RefreshMissionText();

        if (isShowNewBadge)
            SetMissionNewBadgeActive(true, isAutoOpenMissionPanel);
    }

    /// <summary>
    /// ?꾩옱 ?뚮젅?댁뼱 ?몃깽?좊━ 紐⑤뜽???ㅼ떆 ?쎌뼱 HUD ?щ’??媛깆떊?⑸땲??
    /// </summary>
    public void RequestRefreshInventoryView()
    {
        RefreshInventoryView();
    }

    /// <summary>
    /// CookingGroup??吏곸젒 ?대졇嫄곕굹 HUD ?몄텧 寃쎈줈媛 ?딄릿 寃쎌슦?먮룄 遺?뚯슜 ?몃깽?좊━/?꾨Т ?⑤꼸??媛뺤젣濡??쎈땲??
    /// Game View?먯꽌???붾━ ?λ㈃???ㅼ뼱?ㅼ옄留덉옄 ?щ즺 ?щ’??蹂닿퀬 ?쒕옒洹명븷 ???덇쾶 留뚮뱶???덉쟾 ?먯엯?덈떎.
    /// </summary>
    public void RequestOpenCookingSupportHUD()
    {
        CreateHUDCanvasIfNeeded();
        ApplyBottomHUDLayout();
        BindHUDViewReferences();
        ResolveCookingOverlayControllerIfNeeded();
        OpenCookingSupportHUD();
    }

    /// <summary>
    /// 선택지/대화가 시작된 뒤 인벤토리 패널이 대사를 가리는 상황을 막기 위해 잠시 후 닫습니다.
    /// 부엌에 다시 들어오면 RequestOpenCookingSupportHUD가 다시 열어 주므로 조리 흐름은 막히지 않습니다.
    /// </summary>
    public void RequestCloseInventoryPanelAfterDelay(float delaySeconds)
    {
        if (_isStage4PresentationHUDLockedOpen)
            return;

        if (!isActiveAndEnabled)
            return;

        StartCoroutine(CloseInventoryPanelAfterDelayRoutine(delaySeconds));
    }

    private IEnumerator CloseInventoryPanelAfterDelayRoutine(float delaySeconds)
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, delaySeconds));
        SetInventoryPanelActive(false);
    }

    public void RequestForceCloseInventoryPanelForEncounterReturn()
    {
        if (_isStage4PresentationHUDLockedOpen)
            return;

        CancelPendingInventoryAutoOpen();
        SetInventoryPanelActive(false);

        if (!isActiveAndEnabled)
            return;

        if (_forceCloseInventoryPanelCoroutine != null)
            StopCoroutine(_forceCloseInventoryPanelCoroutine);

        _forceCloseInventoryPanelCoroutine = StartCoroutine(ForceCloseInventoryPanelRoutine());
    }

    private IEnumerator ForceCloseInventoryPanelRoutine()
    {
        float[] delayArray = { 0f, 0.2f, 1f, 2f };

        for (int index = 0; index < delayArray.Length; index++)
        {
            float delaySeconds = delayArray[index];

            if (delaySeconds > 0f)
                yield return new WaitForSecondsRealtime(delaySeconds);
            else
                yield return null;

            CancelPendingInventoryAutoOpen();
            SetInventoryPanelActive(false);
        }

        _forceCloseInventoryPanelCoroutine = null;
    }

    private void CancelPendingInventoryAutoOpen()
    {
        _pendingAutoOpenButtonKindList.RemoveAll(delegate (OOTechRoadHUDButtonKind buttonKind)
        {
            return buttonKind == OOTechRoadHUDButtonKind.Inventory;
        });
    }

    public void RequestCloseInventoryAndMissionPanelsAfterDelay(float delaySeconds)
    {
        if (_isStage4PresentationHUDLockedOpen)
            return;

        if (!isActiveAndEnabled)
            return;

        StartCoroutine(CloseInventoryAndMissionPanelsAfterDelayRoutine(delaySeconds));
    }

    private IEnumerator CloseInventoryAndMissionPanelsAfterDelayRoutine(float delaySeconds)
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, delaySeconds));
        SetInventoryPanelActive(false);
        SetMissionPanelActive(false);
    }

    /// <summary>
    /// ?꾧컧泥섎읆 ?ㅻⅨ ?ㅻ줈媛湲?濡쒖쭅?쇰줈 ?뚯븘??寃쎌슦?먮룄 HUD? ?대룞 ?좉툑??蹂듦뎄?⑸땲??
    /// Game View?먯꽌???⑥뿀??HUD媛 ?ㅼ떆 耳쒖?怨?MFC ?대룞 ?낅젰???ㅼ떆 ?댁븘?⑸땲??
    /// </summary>
    public void RequestRestoreFromOverlayReturn()
    {
        _isOverlayOpen = false;

        if (_isCookingOverlayOpen)
        {
            _isStage4CookingSupportLockedOpen = false;
            _isStage4PersistentGuideLockedOpen = false;
            CloseCookingSupportHUD();
        }
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
        RequestRestoreOwnerRoadMovement();
    }

    /// <summary>
    /// ?몃? ?ㅻ줈媛湲?踰꾪듉???ㅻ쾭?덉씠瑜??レ븯?붿? 媛먯???HUD ?좉툑???먮룞?쇰줈 ?됰땲??
    /// </summary>
    private void Update()
    {
        UpdateHUDGuideClickInput();
        RecoverExternalOverlayCloseIfNeeded();
    }

    /// <summary>
    /// ?뱀젙 HUD 踰꾪듉???붿궡?쒕줈 媛由ы궎???쒗넗由ъ뼹 媛?대뱶瑜??쒖떆?⑸땲??
    /// Game View?먯꽌??踰꾪듉 ?꾩뿉 ?묒? ?ㅻ챸 ?⑤꼸???밸땲??
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
    /// ?뱀젙 踰꾪듉???꾨땲??濡쒕뱶 ?붾㈃ 以묒븰??吏㏃? ?덈궡 硫붿떆吏瑜??꾩썎?덈떎.
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

    public void ShowRoadMessageTopCenter(string title, string description, UnityAction onNext)
    {
        CreateGuideOverlayIfNeeded();

        if (Root_GuideOverlay == null)
        {
            onNext?.Invoke();
            return;
        }

        Root_GuideOverlay.SetActive(true);
        ApplyGuideOverlayLayout(new Vector2(0f, 620f), title, description, onNext);
    }

    public void ShowPersistentRoadMessageTopCenter(string title, string description)
    {
        CreateGuideOverlayIfNeeded();

        if (Root_GuideOverlay == null)
            return;

        _isStage4PersistentGuideLockedOpen = true;
        Root_GuideOverlay.SetActive(true);
        ApplyGuideOverlayLayout(new Vector2(0f, 620f), title, description, null, true);
    }

    /// <summary>
    /// HUD ?ъ빱??媛?대뱶瑜??レ뒿?덈떎.
    /// </summary>
    public void CloseHUDGuide()
    {
        if (_isStage4PersistentGuideLockedOpen)
            return;

        _guideClickAction = null;

        if (Root_GuideOverlay != null)
            Root_GuideOverlay.SetActive(false);
    }

    /// <summary>
    /// ?꾩슂?섎떎硫?湲곕낯 ?꾩씠?쒖쓣 ?몃깽?좊━????踰덈쭔 吏湲됲빀?덈떎.
    /// ?쒗넗由ъ뼹 蹂댁긽 諛⑹떇怨?異⑸룎?섏? ?딅룄濡??몄뒪?숉꽣 ?뚮옒洹멸? 耳쒖쭊 寃쎌슦?먮쭔 ?숈옉?⑸땲??
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
    /// ?ъ뿉 諛곗튂??HUDUIGroup??李얘퀬 Canvas ?ㅼ젙??1920x1080 湲곗??쇰줈 ?뺣━?⑸땲??
    /// ??硫붿꽌?쒕뒗 ??HUD瑜?留뚮뱾吏 ?딄퀬, 以鍮꾨맂 ?뚰뭹???놁쑝硫??먮윭瑜??④퉩?덈떎.
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
            Root_HUD = RequestChildObjectByName(transform, "HUDUIGroup");

        if (Root_HUD == null)
            Root_HUD = RequestChildObjectByName(transform, "RoadHUDCanvas");

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
    /// OOTechRoadHUDView媛 ?ㅺ퀬 ?덈뒗 ?ㅼ젣 踰꾪듉/?⑤꼸 李몄“瑜?而⑦듃濡ㅻ윭 ?꾨뱶???곌껐?⑸땲??
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
        Text_WorldMapNewBadge = View_HUD.WorldMapNewBadgeText;
        Text_CookingLabel = View_HUD.CookingLabelText;
        Text_InventoryQuantityGuide = View_HUD.InventoryQuantityGuideText;
        PrepareInventoryQuantityGuide();
        Image_CookingButton = View_HUD.CookingButtonImage;
        ResolveCookingNewBadge();
        Scroll_InventorySlots = View_HUD.InventoryScrollRect;
        Rect_InventoryContent = View_HUD.InventoryContentRect;
        Slot_InventoryItemTemplate = View_HUD.InventorySlotTemplate;
        ConfigureInventoryScrollView(false);

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

    private void ResolveCookingNewBadge()
    {
        if (Text_CookingNewBadge != null || Button_Cooking == null)
            return;

        GameObject badgeObjectInButton = OOTechSceneQuery.RequestChildObjectByName(Button_Cooking.transform, "NewBadge_Cooking");
        Transform badgeTransform = badgeObjectInButton != null ? badgeObjectInButton.transform : null;

        if (badgeTransform != null)
            Text_CookingNewBadge = badgeTransform.GetComponent<TextMeshProUGUI>();

        if (Text_CookingNewBadge != null)
            return;

        GameObject badgeObject = new GameObject("NewBadge_Cooking", typeof(RectTransform));
        badgeObject.transform.SetParent(Button_Cooking.transform, false);

        RectTransform badgeRect = badgeObject.transform as RectTransform;
        badgeRect.anchorMin = new Vector2(1f, 1f);
        badgeRect.anchorMax = new Vector2(1f, 1f);
        badgeRect.pivot = new Vector2(0.5f, 0.5f);
        badgeRect.anchoredPosition = new Vector2(-16f, -12f);
        badgeRect.sizeDelta = new Vector2(86f, 36f);

        Text_CookingNewBadge = badgeObject.AddComponent<TextMeshProUGUI>();
        Text_CookingNewBadge.text = "NEW";
        Text_CookingNewBadge.fontSize = 26f;
        Text_CookingNewBadge.alignment = TextAlignmentOptions.Center;
        Text_CookingNewBadge.color = new Color(1f, 0.86f, 0.1f, 1f);
        Text_CookingNewBadge.raycastTarget = false;
        Text_CookingNewBadge.fontStyle = FontStyles.Bold;
        OOTechTMPFontUtility.ApplyProjectFont(Text_CookingNewBadge);
        badgeObject.SetActive(false);
    }

    /// <summary>
    /// HUD 踰꾪듉 ?대┃ ?대깽?몃? 媛곴컖??湲곕뒫 ?먯뿉 ?곌껐?⑸땲??
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
    /// 怨듭쑀 HUD媛 ?덉쑝硫?濡쒖뺄 以묐났 HUD媛 ?대┃??媛濡쒕쭑吏 ?딅룄濡??꾩옱 而⑦듃濡ㅻ윭??HUD瑜?怨듭쑀 HUD濡??곌껐?⑸땲??
    /// </summary>
    private OOTechRoadHUDView ResolveSharedHUDView()
    {
        GameObject sharedSystemObject = RequestSceneObjectByName(_sharedHUDSystemGroupName);
        GameObject sharedHUDObject = sharedSystemObject != null ? RequestChildObjectByName(sharedSystemObject.transform, _sharedHUDGroupName) : null;

        if (sharedHUDObject == null)
            sharedHUDObject = RequestSceneObjectByName("GlobalHUDUIGroup");

        if (sharedHUDObject == null)
            return null;

        return sharedHUDObject.GetComponent<OOTechRoadHUDView>();
    }

    /// <summary>
    /// 怨듭슜 HUD瑜??곕뒗 ?숈븞 洹몃９ ?먯떇?쇰줈 ?⑥븘 ?덈뒗 ?덉쟾 HUD媛 踰꾪듉 ?대┃??癒뱀? ?딅룄濡?鍮꾪솢?깊솕?⑸땲??
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
    /// 踰꾪듉??蹂댁씠?붾뜲 ?대┃?????섎뒗 ?곹솴??留됯린 ?꾪빐 Raycast? interactable ?곹깭瑜?蹂듦뎄?⑸땲??
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
    /// HUD瑜?1920x1080 寃뚯엫 ?붾㈃ ?꾩쓽 Overlay濡?怨좎젙?⑸땲??
    /// 濡쒕뱶留??대?吏??臾대? ?명듃?닿퀬, HUD???뚮젅?댁뼱媛 ?꾨Ⅴ??媛앹꽍 履?議곗옉?먯엯?덈떎.
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
        {
            RectTransform rootRect = Root_HUD.transform as RectTransform;

            if (rootRect != null)
            {
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.pivot = new Vector2(0.5f, 0.5f);
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
                rootRect.localScale = Vector3.one;
            }

            Root_HUD.transform.SetAsLastSibling();
        }

        if (Root_BottomBar == null && View_HUD != null)
        {
            View_HUD.ResolveReferences();
            Root_BottomBar = View_HUD.BottomBar;
        }

        RectTransform bottomBarRect = Root_BottomBar != null ? Root_BottomBar.transform as RectTransform : null;

        if (bottomBarRect != null)
        {
            bottomBarRect.anchorMin = new Vector2(0f, 0f);
            bottomBarRect.anchorMax = new Vector2(1f, 0f);
            bottomBarRect.pivot = new Vector2(0.5f, 0f);
            bottomBarRect.anchoredPosition = Vector2.zero;
            bottomBarRect.sizeDelta = new Vector2(0f, bottomBarRect.sizeDelta.y);
            bottomBarRect.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// ?뚮젅?댁뼱 ?몃깽?좊━ 紐⑤뜽???쎌뼱 ?щ’ 紐⑸줉???ㅼ떆 洹몃┰?덈떎.
    /// ?뺤쟻 ?쒗뵆由??щ’留?蹂듭젣?섎ŉ, HUD 堉덈? ?먯껜?????ㅻ툕?앺듃瑜??좎??⑸땲??
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
        bool hasKoreanCakeSlot = false;
        bool hasHoneyKoreanCakeSlot = false;

        foreach (OOTechItemModel item in itemList)
        {
            if (item == null || item.ItemStackCount <= 0)
                continue;

            hasVisibleItem = true;
            if (item.ItemDataId == "OO_KoreanCake_1")
                hasKoreanCakeSlot = true;

            if (item.ItemDataId == "OO_HoneyKoreanCake_1")
                hasHoneyKoreanCakeSlot = true;

            CreateInventoryItemSlot(item);
        }

        if (!hasKoreanCakeSlot && OOTechGameManager.Inst != null && OOTechGameManager.Inst.GetItemCount("OO_KoreanCake_1") > 0)
        {
            hasVisibleItem = true;
            OOTechItemModel koreanCakeModel = new OOTechItemModel
            {
                ItemUniqueId = System.DateTime.UtcNow.Ticks,
                ItemDataId = "OO_KoreanCake_1",
                ItemStackCount = OOTechGameManager.Inst.GetItemCount("OO_KoreanCake_1")
            };

            CreateInventoryItemSlot(koreanCakeModel);
            Debug.LogWarning("[OOTechRoadHUDController] KoreanCake slot was repaired during inventory refresh.");
        }

        if (!hasHoneyKoreanCakeSlot && OOTechGameManager.Inst != null && OOTechGameManager.Inst.GetItemCount("OO_HoneyKoreanCake_1") > 0)
        {
            hasVisibleItem = true;
            OOTechItemModel honeyKoreanCakeModel = new OOTechItemModel
            {
                ItemUniqueId = System.DateTime.UtcNow.Ticks,
                ItemDataId = "OO_HoneyKoreanCake_1",
                ItemStackCount = OOTechGameManager.Inst.GetItemCount("OO_HoneyKoreanCake_1")
            };

            CreateInventoryItemSlot(honeyKoreanCakeModel);
            Debug.LogWarning("[OOTechRoadHUDController] HoneyKoreanCake slot was repaired during inventory refresh.");
        }

        if (!hasVisibleItem)
            CreateInventoryTextRow("비어 있음");
    }

    /// <summary>
    /// ?몃깽?좊━媛 鍮꾩뼱 ?덉쓣 ???덈궡 臾멸뎄 ?щ’???쒖떆?⑸땲??
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

        RebuildInventoryScrollView(true);
    }

    /// <summary>
    /// ?ㅼ젣 ?꾩씠??紐⑤뜽 ?섎굹瑜??몃깽?좊━ ?щ’?쇰줈 ?쒖떆?⑸땲??
    /// 遺?뚯씠 ?대젮 ?덉쑝硫????щ’???쒕옒洹?湲곕뒫???곌껐?⑸땲??
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
            slotView.RequestSetupItem(item.ItemDataId, $"{itemName}x{item.ItemStackCount}", itemIconSprite);

        Debug.Log($"[OOTechRoadHUDController] Inventory slot created: Slot_{item.ItemDataId} x{item.ItemStackCount}");
        RebuildInventoryScrollView(true);

        if ((item.ItemDataId == "OO_KoreanCake_1" || item.ItemDataId == "OO_HoneyKoreanCake_1") && Scroll_InventorySlots != null)
            Scroll_InventorySlots.verticalNormalizedPosition = 1f;

        if (Controller_CookingOverlay == null || !Controller_CookingOverlay.gameObject.activeInHierarchy)
            return;

        OOTechCookingIngredientDragItem dragItem = slotObject.GetComponent<OOTechCookingIngredientDragItem>();

        if (dragItem == null)
            dragItem = slotObject.AddComponent<OOTechCookingIngredientDragItem>();

        dragItem.Setup(Controller_CookingOverlay, item.ItemDataId, itemName, item.ItemStackCount);
    }

    /// <summary>
    /// ?쒗뵆由우쓣 ?쒖쇅??湲곗〈 ?몃깽?좊━ ?щ’ ?됱쓣 ?쒓굅?⑸땲??
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

            if (Application.isPlaying)
                Destroy(childTransform.gameObject);
            else
                DestroyImmediate(childTransform.gameObject);
        }
    }

    /// <summary>
    /// Slot_InventoryItemTemplate??蹂듭젣???쒖떆???щ’ ??移몄쓣 留뚮벊?덈떎.
    /// 諛섎났 紐⑸줉?대씪???ш린留??고???蹂듭젣瑜??덉슜?⑸땲??
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
        ApplyInventorySlotRowLayout(slotObject);
        return slotObject;
    }

    /// <summary>
    /// ?몃깽?좊━ ScrollRect瑜??꾩꽦 ?녿뒗 ?ㅽ겕濡ㅻ줈 怨좎젙?⑸땲??
    /// ?곹솕濡?移섎㈃ ?뚰뭹 ?좊컲??愿媛??먯쓣 ?볥뒗 ?쒓컙 ?먯쐞移섎줈 ?吏 ?딄퀬, 媛먮룆???대젮???꾩튂??硫덉떠 ?덇쾶 留뚮뱶???μ튂?낅땲??
    /// </summary>
    private void ConfigureInventoryScrollView(bool isResetToTop)
    {
        if (Scroll_InventorySlots == null && Rect_InventoryContent != null)
            Scroll_InventorySlots = Rect_InventoryContent.GetComponentInParent<ScrollRect>(true);

        if (Scroll_InventorySlots == null && Root_InventoryPanel != null)
        {
            GameObject scrollObject = RequestChildObjectByName(Root_InventoryPanel.transform, "Scroll_InventorySlots");

            if (scrollObject != null)
            {
                Scroll_InventorySlots = scrollObject.GetComponent<ScrollRect>();

                if (Scroll_InventorySlots == null)
                    Scroll_InventorySlots = scrollObject.AddComponent<ScrollRect>();
            }
        }

        if (Scroll_InventorySlots == null)
            return;

        Scroll_InventorySlots.horizontal = false;
        Scroll_InventorySlots.vertical = true;
        Scroll_InventorySlots.movementType = ScrollRect.MovementType.Clamped;
        Scroll_InventorySlots.elasticity = 0f;
        Scroll_InventorySlots.scrollSensitivity = 38f;
        Scroll_InventorySlots.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        Scroll_InventorySlots.verticalScrollbarSpacing = 8f;

        if (Rect_InventoryContent != null)
        {
            Scroll_InventorySlots.content = Rect_InventoryContent;

            if (Scroll_InventorySlots.viewport == null && Rect_InventoryContent.parent is RectTransform viewportRect)
                Scroll_InventorySlots.viewport = viewportRect;
        }

        BindInventoryScrollbarIfNeeded();

        if (isResetToTop)
            Scroll_InventorySlots.verticalNormalizedPosition = 1f;
    }

    private void BindInventoryScrollbarIfNeeded()
    {
        if (Scroll_InventorySlots == null || Root_InventoryPanel == null)
            return;

        if (Scroll_InventorySlots.verticalScrollbar != null)
        {
            Scroll_InventorySlots.verticalScrollbar.gameObject.SetActive(true);
            return;
        }

        GameObject scrollbarObject = RequestChildObjectByName(Root_InventoryPanel.transform, "Scrollbar_InventoryLeft");

        if (scrollbarObject == null)
            return;

        Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();

        if (scrollbar == null)
            return;

        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.gameObject.SetActive(true);
        Scroll_InventorySlots.verticalScrollbar = scrollbar;
    }

    /// <summary>
    /// ?щ’ ??以꾩쓽 ?믪씠瑜??듭씪?⑸땲??
    /// 諛곗슦?ㅼ씠 以?留욎떠 ?쒖빞 ?꾨옒 以꾧퉴吏 ?ㅽ겕濡?臾대??먯꽌 ?뺥솗??蹂댁씠湲??뚮Ц?? ?쒗뵆由?蹂듭젣 吏곹썑 ?ш린瑜?蹂댁젙?⑸땲??
    /// </summary>
    private void ApplyInventorySlotRowLayout(GameObject slotObject)
    {
        if (slotObject == null || !(slotObject.transform is RectTransform slotRect))
            return;

        float slotHeight = GetInventorySlotHeight();
        slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotHeight);

        LayoutElement layoutElement = slotObject.GetComponent<LayoutElement>();

        if (layoutElement == null)
            layoutElement = slotObject.AddComponent<LayoutElement>();

        layoutElement.minHeight = slotHeight;
        layoutElement.preferredHeight = slotHeight;
        layoutElement.flexibleHeight = 0f;
    }

    /// <summary>
    /// ?꾩씠??媛쒖닔??留욎떠 Content ?믪씠瑜??ㅼ떆 怨꾩궛?⑸땲??
    /// ??媛믪씠 遺議깊븯硫??꾨옒 諛곗슦媛 臾대? 諛뽰뿉 ???덈뒗 寃껋쿂??留덉?留??꾩씠?쒖씠 留덉뒪???ㅼ뿉 媛?ㅼ쭛?덈떎.
    /// </summary>
    private void RebuildInventoryScrollView(bool isResetToTop)
    {
        if (Rect_InventoryContent == null)
            return;

        ConfigureInventoryScrollView(false);
        ApplyInventoryContentLayout();
        LayoutRebuilder.ForceRebuildLayoutImmediate(Rect_InventoryContent);
        Canvas.ForceUpdateCanvases();

        if (Scroll_InventorySlots != null)
        {
            Scroll_InventorySlots.StopMovement();

            if (isResetToTop)
                Scroll_InventorySlots.verticalNormalizedPosition = 1f;
        }
    }

    private void ApplyInventoryContentLayout()
    {
        Rect_InventoryContent.anchorMin = new Vector2(0f, 1f);
        Rect_InventoryContent.anchorMax = new Vector2(1f, 1f);
        Rect_InventoryContent.pivot = new Vector2(0.5f, 1f);

        VerticalLayoutGroup layoutGroup = Rect_InventoryContent.GetComponent<VerticalLayoutGroup>();

        if (layoutGroup != null)
        {
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = _inventorySlotSpacing;
        }

        float contentHeight = CalculateInventoryContentHeight();
        Rect_InventoryContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
        Rect_InventoryContent.anchoredPosition = new Vector2(Rect_InventoryContent.anchoredPosition.x, 0f);
    }

    private float CalculateInventoryContentHeight()
    {
        int visibleSlotCount = 0;

        for (int index = 0; index < Rect_InventoryContent.childCount; index++)
        {
            Transform childTransform = Rect_InventoryContent.GetChild(index);

            if (childTransform == null || !childTransform.gameObject.activeSelf)
                continue;

            if (Slot_InventoryItemTemplate != null && childTransform.gameObject == Slot_InventoryItemTemplate)
                continue;

            visibleSlotCount++;
        }

        float slotHeight = GetInventorySlotHeight();
        float contentHeight = visibleSlotCount * slotHeight;

        if (visibleSlotCount > 1)
            contentHeight += (visibleSlotCount - 1) * _inventorySlotSpacing;

        contentHeight += _inventoryContentBottomPadding;

        RectTransform viewportRect = Scroll_InventorySlots != null && Scroll_InventorySlots.viewport != null ? Scroll_InventorySlots.viewport : null;
        float viewportHeight = viewportRect != null ? viewportRect.rect.height : 0f;
        return Mathf.Max(contentHeight, viewportHeight);
    }

    private float GetInventorySlotHeight()
    {
        if (_inventorySlotHeight > 1f)
            return _inventorySlotHeight;

        if (Slot_InventoryItemTemplate != null && Slot_InventoryItemTemplate.transform is RectTransform templateRect)
            return Mathf.Max(1f, templateRect.rect.height, templateRect.sizeDelta.y);

        return 76f;
    }

    /// <summary>
    /// ?붾━?섍린 踰꾪듉???쒖떆 ?곹깭瑜??꾩옱 ?좉툑 ?곹깭??留욎땅?덈떎.
    /// </summary>
    private void RefreshCookingButtonView()
    {
        if (Button_Cooking != null)
            Button_Cooking.interactable = true;

        if (Image_CookingButton != null)
            Image_CookingButton.color = _isCookingUnlocked ? new Color(0.94f, 0.72f, 0.42f, 0.94f) : new Color(0.62f, 0.43f, 0.26f, 0.72f);

        if (Text_CookingLabel != null)
            Text_CookingLabel.text = "\uC694\uB9AC\uD558\uAE30";
    }

    /// <summary>
    /// ?몃깽?좊━ 踰꾪듉???꾨Ⅴ硫??몃깽?좊━ ?⑤꼸???좉??섍퀬 NEW 諛곗?瑜??뺤씤 泥섎━?⑸땲??
    /// </summary>
    private void OnInventoryButtonClicked()
    {
        RefreshInventoryView();
        SetInventoryPanelActive(Root_InventoryPanel != null && !Root_InventoryPanel.activeSelf);
        SetMissionPanelActive(false);
        SetInventoryNewBadgeActive(false);
    }

    /// <summary>
    /// ?꾧컧 踰꾪듉???꾨Ⅴ硫?CodexGroup?쇰줈 ?대룞?⑸땲??
    /// </summary>
    private void OnCodexButtonClicked()
    {
        SetCodexNewBadgeActive(false);
        RequestOpenSceneGroup(_codexGroupName);
    }

    /// <summary>
    /// ?꾨Т 踰꾪듉???꾨Ⅴ硫??꾨Т ?⑤꼸???좉??섍퀬 NEW 諛곗?瑜??뺤씤 泥섎━?⑸땲??
    /// </summary>
    private void OnMissionButtonClicked()
    {
        RefreshMissionText();
        SetMissionPanelActive(Root_MissionPanel != null && !Root_MissionPanel.activeSelf);
        SetInventoryPanelActive(false);
        SetMissionNewBadgeActive(false);
    }

    /// <summary>
    /// ?붾━?섍린 踰꾪듉???꾨Ⅴ硫?CookingGroup?쇰줈 ?대룞?⑸땲??
    /// ?좉꺼 ?덉쑝硫??꾩쭅 ?대━吏 ?딆븯?ㅻ뒗 ?덈궡留?蹂댁뿬以띾땲??
    /// </summary>
    private void OnCookingButtonClicked()
    {
        if (!_isCookingUnlocked)
        {
            ShowRoadMessage("\uC694\uB9AC\uD558\uAE30", "[요리하기] 임무를 완수하기 위한 요리를 만드는 곳입니다. 다음 로드맵에서 활성화됩니다.", null);
            return;
        }

        SetCookingNewBadgeActive(false);
        RequestOpenSceneGroup(_cookingGroupName);
    }

    /// <summary>
    /// ?붾뱶留?踰꾪듉???꾨Ⅴ硫?WorldMapGroup???쎈땲??
    /// </summary>
    private void OnWorldMapButtonClicked()
    {
        SetWorldMapNewBadgeActive(false);
        RequestOpenSceneGroup(_worldMapGroupName);
    }

    private void RequestAutoOpenNewPanel(OOTechRoadHUDButtonKind buttonKind)
    {
        if (!isActiveAndEnabled)
            return;

        if (IsAutoOpenTargetAlreadyVisible(buttonKind))
            return;

        if (!_pendingAutoOpenButtonKindList.Contains(buttonKind))
            _pendingAutoOpenButtonKindList.Add(buttonKind);

        SortAutoOpenQueue();

        if (_autoOpenNewPanelCoroutine == null)
            _autoOpenNewPanelCoroutine = StartCoroutine(PlayAutoOpenNewPanelRoutine());
    }

    /// <summary>
    /// NEW 뱃지가 붙은 UI를 우선순위대로 한 번씩 열어 줍니다.
    /// 인벤토리, 임무, 월드맵이 동시에 갱신될 때 마지막 요청이 앞 요청을 끊지 않도록 큐로 처리합니다.
    /// </summary>
    private IEnumerator PlayAutoOpenNewPanelRoutine()
    {
        while (_pendingAutoOpenButtonKindList.Count > 0)
        {
            yield return null;

            if (!gameObject.activeInHierarchy)
                break;

            SortAutoOpenQueue();
            OOTechRoadHUDButtonKind buttonKind = _pendingAutoOpenButtonKindList[0];
            _pendingAutoOpenButtonKindList.RemoveAt(0);

            if (buttonKind == OOTechRoadHUDButtonKind.Inventory)
            {
                RefreshInventoryView();
                SetInventoryPanelActive(true);
                SetMissionPanelActive(false);
                yield return new WaitForSecondsRealtime(0.15f);
                continue;
            }

            if (buttonKind == OOTechRoadHUDButtonKind.Mission)
            {
                RefreshMissionText();
                SetMissionPanelActive(true);
                SetInventoryPanelActive(false);
                yield return new WaitForSecondsRealtime(0.15f);
                continue;
            }

            if (buttonKind == OOTechRoadHUDButtonKind.WorldMap)
            {
                OnWorldMapButtonClicked();
                yield return new WaitForSecondsRealtime(0.15f);
            }
        }

        _autoOpenNewPanelCoroutine = null;
    }

    private void SortAutoOpenQueue()
    {
        _pendingAutoOpenButtonKindList.Sort(delegate (OOTechRoadHUDButtonKind firstKind, OOTechRoadHUDButtonKind secondKind)
        {
            return GetAutoOpenPriority(firstKind).CompareTo(GetAutoOpenPriority(secondKind));
        });
    }

    private int GetAutoOpenPriority(OOTechRoadHUDButtonKind buttonKind)
    {
        if (buttonKind == OOTechRoadHUDButtonKind.Inventory)
            return 0;

        if (buttonKind == OOTechRoadHUDButtonKind.Mission)
            return 1;

        if (buttonKind == OOTechRoadHUDButtonKind.WorldMap)
            return 2;

        return 99;
    }

    private bool IsAutoOpenTargetAlreadyVisible(OOTechRoadHUDButtonKind buttonKind)
    {
        if (buttonKind == OOTechRoadHUDButtonKind.Inventory)
            return Root_InventoryPanel != null && Root_InventoryPanel.activeInHierarchy;

        if (buttonKind == OOTechRoadHUDButtonKind.Mission)
            return Root_MissionPanel != null && Root_MissionPanel.activeInHierarchy;

        return false;
    }

    /// <summary>
    /// 硫붿씤 硫붾돱 踰꾪듉???꾨Ⅴ硫??뺤씤 ?앹뾽???꾩썎?덈떎.
    /// </summary>
    private void OnMainMenuButtonClicked()
    {
        ShowMainMenuConfirmPopup();
    }

    private void SetInventoryPanelActive(bool isActive)
    {
        if (_isStage4CookingSupportLockedOpen)
            isActive = true;

        if (Root_InventoryPanel != null)
            Root_InventoryPanel.SetActive(isActive);

        SetInventoryQuantityGuideActive(isActive);
    }

    /// <summary>
    /// ?몃깽?좊━ ?덉뿉 ?섎웾 議곗젅 ?덈궡 臾멸뎄瑜?以鍮꾪빀?덈떎.
    /// ?곹솕濡?移섎㈃ ?붾━ ?λ㈃ ?꾩뿉 ?뚰뭹??紐?媛?吏묐뒗吏 ?뚮젮二쇰뒗 ?묒? ?먯뭅?쒖엯?덈떎.
    /// </summary>
    private void PrepareInventoryQuantityGuide()
    {
        if (Text_InventoryQuantityGuide == null)
            return;

        OOTechTMPFontUtility.ApplyProjectFont(Text_InventoryQuantityGuide);
        Text_InventoryQuantityGuide.text = "Ctrl + \uB9C8\uC6B0\uC2A4 \uD720: \uC9D1\uC744 \uC218\uB7C9 \uC870\uC808\n\uC544\uC774\uCF58\uC744 \uC870\uB9AC\uB3C4\uAD6C\uB85C \uB4DC\uB798\uADF8";
        Text_InventoryQuantityGuide.gameObject.SetActive(false);
    }

    private void SetInventoryQuantityGuideActive(bool isActive)
    {
        if (Text_InventoryQuantityGuide == null)
            return;

        Text_InventoryQuantityGuide.gameObject.SetActive(isActive);
    }

    private void SetMissionPanelActive(bool isActive)
    {
        if (_isStage4CookingSupportLockedOpen)
            isActive = true;

        ApplyMissionPanelBottomRightLayoutIfNeeded();
        RefreshMissionText();

        if (Root_MissionPanel != null)
            Root_MissionPanel.SetActive(isActive);
    }

    /// <summary>
    /// ?덉쟾 以묒븰 湲곕낯 諛곗튂濡??⑥븘 ?덈뒗 ?꾨Т?먮쭔 ?곗륫 ?섎떒?쇰줈 ??퉩?덈떎.
    /// 媛먮룆??吏곸젒 ?≪? 而ㅼ뒪? 諛곗튂??嫄대뱶由ъ? ?딆뒿?덈떎.
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
    /// ?꾨Т???띿뒪?몃? ?꾩옱 吏꾪뻾 ?곹깭??留욎떠 ?ㅼ떆 ?곷땲??
    /// </summary>
    private void RefreshMissionText()
    {
        if (Text_MissionContent == null)
            return;

        Text_MissionContent.richText = true;
        string cookingMissionText = CreateCookingMissionText();

        string eastRoadMissionText = _isEastRoadMissionRemoved || string.IsNullOrWhiteSpace(_roadMissionText) ? string.Empty : "○ " + _roadMissionText + "\n";
        string stageQuestText = string.IsNullOrWhiteSpace(_stageQuestMissionText) ? string.Empty : "○ " + _stageQuestMissionText + "\n";
        Text_MissionContent.text = "\uD604\uC7AC \uC784\uBB34\n" + cookingMissionText + eastRoadMissionText + stageQuestText;
    }

    /// <summary>
    /// ?붾━ ?꾨Т 以꾩쓽 ?꾩옱 ?쒖떆 ?곹깭瑜?留뚮벊?덈떎.
    /// ?꾨즺 ?곗텧 以묒뿉??痍⑥냼?좉낵 ?뚰뙆媛믪쑝濡?泥?踰덉㎏ 以꾨쭔 ?먯젏 ?щ씪吏寃??⑸땲??
    /// </summary>
    private string CreateCookingMissionText()
    {
        if (!_isCookingQuestActive || _isCookingMissionRemoved)
            return string.Empty;

        string missionText = "○ 배고픈 모란과 동료들을 위해 요리하세요.";

        if (!_isCookingQuestComplete)
            return missionText + "\n";

        string alphaHex = Mathf.Clamp(Mathf.RoundToInt(_missionCompleteEffectAlpha * 255f), 0, 255).ToString("X2");
        return $"<color=#202020{alphaHex}><s>{missionText}</s></color>\n";
    }

    /// <summary>
    /// ?꾨즺???꾨Т 以꾩쓣 ?좉퉸 蹂댁뿬以 ??泥쒖쿇??吏?곌퀬, ?⑥? ?꾨Т媛 ?꾨줈 ?щ씪?ㅺ쾶 ?⑸땲??
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
    /// NEW 諛곗?瑜?耳쒓퀬 ?꾨ŉ, 耳쒖쭏 ?뚮뒗 源쒕묀?대뒗 肄붾（?댁쓣 ?쒖옉?⑸땲??
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

        if (isActive && isActiveAndEnabled)
            badgeCoroutine = StartCoroutine(BlinkNewBadgeRoutine(badgeText));
    }

    /// <summary>
    /// NEW 湲?먯쓽 ?뚰뙆媛믪쓣 ?붾뱾???뚮젅?댁뼱 ?덉뿉 ?꾧쾶 留뚮벊?덈떎.
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
    /// ?곗씠??留ㅻ땲??먯꽌 ?щ즺/?붾━ ?대쫫??李얘퀬, ?놁쓣 ?뚮뒗 ?쒗넗由ъ뼹??湲곕낯 ?대쫫??諛섑솚?⑸땲??
    /// </summary>
    private string GetIngredientDisplayName(string ingredientDataId)
    {
        if (OOTechItemCatalogManager.Inst != null)
            return OOTechItemCatalogManager.Inst.GetItemDisplayName(ingredientDataId);

        if (!string.IsNullOrEmpty(ingredientDataId) && OOTechGameDataManager.Inst != null)
        {
            OOTechGameDataManager.Inst.TryGetIngredientData(ingredientDataId, out OO_Ingredient ingredientData);

            if (ingredientData != null && !string.IsNullOrEmpty(ingredientData.Name))
                return ingredientData.Name;

            OOTechGameDataManager.Inst.TryGetCookData(ingredientDataId, out OO_Cook cookData);

            if (cookData != null && !string.IsNullOrEmpty(cookData.Name))
                return cookData.Name;
        }

        if (ingredientDataId == _defaultInventoryItemId)
            return "쌀";

        if (ingredientDataId == "Ing_Veggie_01")
            return "채소";

        if (ingredientDataId == "Ing_Pumpkin_01")
            return "호박";

        if (ingredientDataId == "Ing_ChiliPepper_01")
            return "청양고추";

        if (ingredientDataId == "Ing_Kimch_01")
            return "김치";

        if (ingredientDataId == "Ing_Honey_01")
            return "꿀";

        if (ingredientDataId == "Ing_Carrot_01")
            return "당근";

        if (ingredientDataId == "OO_VegetableSoup_1")
            return "야채죽";

        if (ingredientDataId == "OO_KoreanCake_1")
            return "떡";

        if (ingredientDataId == "OO_HoneyKoreanCake_1")
            return "꿀떡";

        return string.IsNullOrEmpty(ingredientDataId) ? "알 수 없는 아이템" : ingredientDataId;
    }

    /// <summary>
    /// ?꾩씠??移댄깉濡쒓렇?먯꽌 ?몃깽?좊━ ?щ’???쒖떆??Sprite ?꾩씠肄섏쓣 媛?몄샃?덈떎.
    /// </summary>
    private Sprite GetItemIconSprite(string itemDataId)
    {
        return OOTechItemCatalogManager.RequestItemIconSprite(itemDataId);
    }

    /// <summary>
    /// ?붾뱶留? 遺?? ?꾧컧 媛숈? ?ㅻⅨ 洹몃９???닿퀬 ?댁쟾 洹몃９ ?대쫫??湲곕줉?⑸땲??
    /// </summary>
    private bool RequestOpenSceneGroup(string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
            return false;

        GameObject groupObject = RequestSceneObjectByName(groupName);
        string previousGroupName = GetOwnerGroupName();
        OOTechGroupNavigationHistory.SetPreviousGroup(groupName, previousGroupName);

        PrepareOverlayController(groupName, groupObject);

        if (groupName == _cookingGroupName)
            return RequestOpenCookingGroupDirect(groupObject, previousGroupName);

        if (OOTechUIManager.Inst != null && groupObject != null)
        {
            OOTechUIManager.Inst.RegisterUI(groupName, groupObject);

            if (OOTechUIManager.Inst.OpenUI(groupName))
            {
                PrepareOverlayReturnButton(groupObject, groupName, previousGroupName);
                PrepareOverlayVisualPriority(groupObject, groupName);
                HandleOverlayOpened(groupName);
                DeactivatePreviousGroupForCooking(groupName, previousGroupName);
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
            DeactivatePreviousGroupForCooking(groupName, previousGroupName);
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
        DeactivatePreviousGroupForCooking(groupName, previousGroupName);
        return true;
    }

    private bool RequestOpenCookingGroupDirect(GameObject cookingGroupObject, string previousGroupName)
    {
        if (cookingGroupObject == null)
        {
            Debug.LogWarning($"[OOTechRoadHUDController] Scene group not found: {_cookingGroupName}");
            return false;
        }

        if (OOTechUIManager.Inst != null)
            OOTechUIManager.Inst.RegisterUI(_cookingGroupName, cookingGroupObject);

        cookingGroupObject.SetActive(true);
        PrepareOverlayController(_cookingGroupName, cookingGroupObject);
        PrepareOverlayReturnButton(cookingGroupObject, _cookingGroupName, previousGroupName);
        PrepareOverlayVisualPriority(cookingGroupObject, _cookingGroupName);
        HandleOverlayOpened(_cookingGroupName);
        DeactivatePreviousGroupForCooking(_cookingGroupName, previousGroupName);
        ForceDeactivateStage4GroupsForCooking(previousGroupName);

        cookingGroupObject.SetActive(true);
        if (Controller_CookingOverlay != null)
            Controller_CookingOverlay.RequestForceKitchenPresentationOpen();

        ForceDeactivateStage4GroupsForCooking(previousGroupName);
        return true;
    }

    private void DeactivatePreviousGroupForCooking(string openedGroupName, string previousGroupName)
    {
        if (openedGroupName != _cookingGroupName || string.IsNullOrEmpty(previousGroupName))
            return;

        GameObject previousGroupObject = RequestSceneObjectByName(previousGroupName);

        if (previousGroupObject == null || previousGroupObject == gameObject)
            return;

        previousGroupObject.SetActive(false);
    }

    private void ForceDeactivateStage4GroupsForCooking(string previousGroupName)
    {
        ForceDeactivateSceneGroup("Stage4_2Group");
        ForceDeactivateSceneGroup("Stage4_1Group");
        ForceDeactivateSceneGroup("4th_Road_to_Stage4");
    }

    private void ForceDeactivateSceneGroup(string groupName)
    {
        GameObject groupObject = RequestSceneObjectByName(groupName);

        if (groupObject == null)
            return;

        groupObject.SetActive(false);
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
    /// ?붾뱶留?遺??臾대?媛 ?대┫ ???댁쟾 濡쒕뱶 諛곌꼍留됰낫???욎뿉 蹂댁씠?꾨줉 SpriteRenderer ?뺣젹媛믪쓣 蹂댁젙?⑸땲??
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
    /// ?ㅻ쾭?덉씠 洹몃９???대졇????HUD ?쒖떆 洹쒖튃???곸슜?⑸땲??
    /// ?붾뱶留듭? HUD瑜??꾧퀬, 遺?뚯? ?몃깽?좊━/?꾨Т ?⑤꼸???대┰?덈떎.
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
    /// ?ㅻ쾭?덉씠???뚯븘媛湲?踰꾪듉???꾨Ⅴ硫?吏곸쟾 洹몃９?쇰줈 蹂듦??⑸땲??
    /// </summary>
    private void HandleOverlayReturnClicked(string currentGroupName, string previousGroupName)
    {
        if (currentGroupName == _cookingGroupName)
        {
            RequestReturnFromCookingGroupDirect(currentGroupName, previousGroupName);
            return;
        }

        if (OOTechUIManager.Inst != null)
        {
            OOTechUIManager.Inst.CloseUI(currentGroupName);
            OOTechUIManager.Inst.OpenUI(previousGroupName);
        }
        else
        {
            GameObject currentGroup = RequestSceneObjectByName(currentGroupName);
            GameObject previousGroup = RequestSceneObjectByName(previousGroupName);

            if (currentGroup != null)
                currentGroup.SetActive(false);

            if (previousGroup != null)
                previousGroup.SetActive(true);
        }

        if (previousGroupName == GetOwnerGroupName())
            RequestRestoreFromOverlayReturn();
        else
        {
            _isOverlayOpen = false;
            CloseCookingSupportHUD();
        }
    }

    private void RequestReturnFromCookingGroupDirect(string currentGroupName, string previousGroupName)
    {
        GameObject currentGroup = RequestSceneObjectByName(currentGroupName);
        GameObject previousGroup = RequestSceneObjectByName(previousGroupName);

        if (currentGroup != null)
            currentGroup.SetActive(false);

        if (previousGroup != null)
        {
            if (OOTechUIManager.Inst != null)
                OOTechUIManager.Inst.RegisterUI(previousGroupName, previousGroup);

            previousGroup.SetActive(true);

            OOTechStage4GroupController stage4Controller = previousGroup.GetComponent<OOTechStage4GroupController>();

            if (stage4Controller != null)
            {
                stage4Controller.RequestResolveStage4_2CookingReturn(true);
                stage4Controller.RequestPlayRabbitCarrotCakeReadyIfAvailable();
            }

            OOTechRoadToStage1Controller roadController = previousGroup.GetComponent<OOTechRoadToStage1Controller>();

            if (roadController == null)
                roadController = previousGroup.GetComponentInChildren<OOTechRoadToStage1Controller>(true);

            if (roadController != null)
                roadController.RequestRestoreRoadMovementAfterOverlayReturn();
        }

        _isOverlayOpen = false;
        _isCookingOverlayOpen = false;
        _isStage4PersistentGuideLockedOpen = false;
        Controller_CookingOverlay = null;
        CloseHUDGuide();

        if (Canvas_HUD != null)
            Canvas_HUD.sortingOrder = _sortingOrder;

        SetHUDVisible(true);
        SetBottomHUDActive(true);
        RefreshInventoryView();
        RefreshMissionText();
        RequestRestoreOwnerRoadMovement();
    }

    private void RequestRestoreOwnerRoadMovement()
    {
        OOTechRoadToStage1Controller roadController = GetComponent<OOTechRoadToStage1Controller>();

        if (roadController == null)
            roadController = GetComponentInChildren<OOTechRoadToStage1Controller>(true);

        if (roadController != null)
            roadController.RequestRestoreRoadMovementAfterOverlayReturn();
    }

    /// <summary>
    /// 遺?뚯뿉?쒕뒗 ?섎떒 踰꾪듉? ?④린怨??몃깽?좊━/?꾨Т ?⑤꼸留??붾━ 蹂댁“ HUD濡??좎??⑸땲??
    /// </summary>
    private void OpenCookingSupportHUD()
    {
        _isCookingOverlayOpen = true;
        ResolveCookingOverlayControllerIfNeeded();
        SetHUDVisible(true);
        CloseHUDGuide();
        HideMainMenuConfirmPopup();
        ApplyCookingSupportPanelLayout();
        ShowStage4CookingGuideInKitchenIfNeeded();

        if (Canvas_HUD != null)
            Canvas_HUD.sortingOrder = _sortingOrder + 600;

        SetBottomHUDActive(false);

        SetInventoryPanelActive(true);
        SetMissionPanelActive(true);
        RefreshInventoryView();
        RefreshMissionText();
        SetInventoryPanelActive(true);
        ConfigureInventoryScrollView(false);
    }

    /// <summary>
    /// 遺??吏꾩엯 寃쎈줈媛 HUD 踰꾪듉?대뱺 ?ㅻⅨ ?먯떆?몃뱺, ?꾩옱 耳쒖쭊 CookingGroup Controller瑜??ㅼ떆 李얠뒿?덈떎.
    /// ???곌껐???덉뼱???몃깽?좊━ ?щ’??議곕━?꾧뎄濡??쒕옒洹몃맆 ???덉뒿?덈떎.
    /// </summary>
    private void ResolveCookingOverlayControllerIfNeeded()
    {
        if (Controller_CookingOverlay != null && Controller_CookingOverlay.gameObject.activeInHierarchy)
            return;

        Controller_CookingOverlay = null;

        GameObject cookingGroupObject = RequestSceneObjectByName(_cookingGroupName);

        if (cookingGroupObject != null)
            Controller_CookingOverlay = cookingGroupObject.GetComponent<OOTechCookingGroupController>();

        if (Controller_CookingOverlay != null && Controller_CookingOverlay.gameObject.activeInHierarchy)
            return;

        List<OOTechCookingGroupController> cookingControllerArray = OOTechSceneQuery.RequestCollectComponents<OOTechCookingGroupController>(true);

        foreach (OOTechCookingGroupController cookingController in cookingControllerArray)
        {
            if (cookingController == null || !cookingController.gameObject.activeInHierarchy)
                continue;

            Controller_CookingOverlay = cookingController;
            return;
        }
    }

    /// <summary>
    /// 遺?뚯쓣 ?レ쑝硫?HUD瑜??먮옒 濡쒕뱶 ?붾㈃ ?곹깭濡??섎룎由쎈땲??
    /// </summary>
    private void CloseCookingSupportHUD()
    {
        if (_isStage4CookingSupportLockedOpen)
        {
            OpenCookingSupportHUD();
            return;
        }

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

    private void ShowStage4CookingGuideInKitchenIfNeeded()
    {
        if (!IsStage4CookingGuideOwner())
            return;

        string title = string.IsNullOrWhiteSpace(_stage4CookingGuideTitle) ? "당근전 조리 가이드" : _stage4CookingGuideTitle;
        string body = string.IsNullOrWhiteSpace(_stage4CookingGuideBody)
            ? "떡과 당근을 레시피 조합에서 당근전분으로 만들고, 당근전분을 가마솥에 넣어 당근전을 완성하세요."
            : _stage4CookingGuideBody;

        ShowPersistentRoadMessageTopCenter(title, body);
    }

    private bool IsStage4CookingGuideOwner()
    {
        string ownerName = OwnerGroupName;
        return !string.IsNullOrWhiteSpace(ownerName) &&
               (ownerName.Contains("Stage4") || ownerName.Contains("4th_Road"));
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
            inventoryRect.sizeDelta = new Vector2(560f, 520f);
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
    /// 遺??吏??紐⑤뱶?먯꽌 ?섎떒 HUD 踰꾪듉怨?NEW 諛곗?瑜??④린嫄곕굹 蹂듦뎄?⑸땲??
    /// </summary>
    private void SetBottomHUDActive(bool isActive)
    {
        if (_isStage4PresentationHUDLockedOpen)
            isActive = true;

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
            inventoryRect.sizeDelta = new Vector2(560f, 520f);
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
    /// ?붾뱶留?遺???꾧컧??諛곗튂??Button_RuntimeReturn???꾩옱 蹂듦? 紐⑹쟻吏???곌껐?⑸땲??
    /// </summary>
    private void PrepareOverlayReturnButton(GameObject groupObject, string currentGroupName, string previousGroupName)
    {
        if (groupObject == null)
            return;

        BackButtonController[] backButtonArray = groupObject.GetComponentsInChildren<BackButtonController>(true);

        foreach (BackButtonController backButton in backButtonArray)
            backButton.SetPreviousGroup(previousGroupName);

        GameObject existingReturnButtonObject = RequestChildObjectByName(groupObject.transform, "Button_RuntimeReturn");

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
    /// 硫붿씤 硫붾돱濡??뚯븘媛덉? ?뺤씤?섎뒗 ?앹뾽???쎈땲??
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
    /// ?뺤씤 ?앹뾽?먯꽌 ?덈? ?꾨Ⅴ硫??꾩옱 洹몃９???リ퀬 MainMenuGroup???쎈땲??
    /// </summary>
    private void ConfirmReturnToMainMenu()
    {
        HideMainMenuConfirmPopup();

        GameObject ownerGroup = RequestSceneObjectByName(GetOwnerGroupName());
        GameObject mainMenuGroup = RequestSceneObjectByName(_mainMenuGroupName);

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
    /// ?ъ뿉 諛곗튂???뺤씤 ?앹뾽 踰꾪듉?????꾨땲???숈옉???곌껐?⑸땲??
    /// </summary>
    private void CreateConfirmPopupIfNeeded()
    {
        if (Root_ConfirmPopup == null)
        {
            Debug.LogError("[OOTechRoadHUDController] Panel_MainMenuConfirm is missing from HUDUIGroup.");
            return;
        }

        if (Text_MainMenuConfirmMessage != null)
            Text_MainMenuConfirmMessage.text = "\uC815\uB9D0 \uBA54\uC778 \uBA54\uB274\uB85C \uB3CC\uC544\uAC00\uC2DC\uACA0\uC2B5\uB2C8\uAE4C?";

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
    /// HUD ?쒗넗由ъ뼹 ?붿궡?쒖? ?ㅻ챸 ?⑤꼸???좏깮??踰꾪듉 洹쇱쿂??諛곗튂?⑸땲??
    /// </summary>
    private void ApplyGuideOverlayLayout(Vector2 targetLocalPosition, string title, string description, UnityAction onNext)
    {
        ApplyGuideOverlayLayout(targetLocalPosition, title, description, onNext, false);
    }

    private void ApplyGuideOverlayLayout(Vector2 targetLocalPosition, string title, string description, UnityAction onNext, bool isPersistent)
    {
        if (Rect_FocusArrow != null)
            Rect_FocusArrow.anchoredPosition = new Vector2(targetLocalPosition.x, Mathf.Clamp(targetLocalPosition.y + 100f, 170f, 930f));

        if (Rect_GuideTextPanel != null)
            Rect_GuideTextPanel.anchoredPosition = new Vector2(Mathf.Clamp(targetLocalPosition.x, -710f, 710f), Mathf.Clamp(targetLocalPosition.y + 170f, 240f, 830f));

        if (Text_GuideTitle != null)
            Text_GuideTitle.text = string.IsNullOrEmpty(title) ? "\uD29C\uD1A0\uB9AC\uC5BC" : title;

        if (Text_GuideBody != null)
            Text_GuideBody.text = string.IsNullOrEmpty(description) ? "\uC774 \uAE30\uB2A5\uC740 \uB098\uC911\uC5D0 \uB370\uC774\uD130\uB85C \uAD50\uCCB4\uB429\uB2C8\uB2E4." : description;

        if (isPersistent)
        {
            _guideClickAction = null;
            DisableLegacyGuideNextButtonObject();
            DisableGuideClickArea(ResolveGuideTextPanelButton());
            DisableGuideClickArea(ResolveGuideOverlayButton());
            return;
        }

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
        if (_isStage4PersistentGuideLockedOpen)
            return;

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
        guideClickButton.interactable = true;

        if (guideClickButton.targetGraphic != null)
            guideClickButton.targetGraphic.raycastTarget = true;
    }

    private void DisableGuideClickArea(Button guideClickButton)
    {
        if (guideClickButton == null)
            return;

        guideClickButton.onClick.RemoveAllListeners();
        guideClickButton.interactable = false;

        if (guideClickButton.targetGraphic != null)
            guideClickButton.targetGraphic.raycastTarget = false;
    }

    /// <summary>
    /// ?덉쟾 Button_GuideNext ?뚰뭹? ?④린怨? HUD 媛?대뱶???⑤꼸?대굹 諛곌꼍 ?대┃?쇰줈留??ㅼ쓬 ?먮줈 ?섍퉩?덈떎.
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

    private GameObject RequestSceneObjectByName(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return null;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            GameObject foundObject = RequestChildObjectByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private bool IsSceneGroupActive(string groupName)
    {
        GameObject groupObject = RequestSceneObjectByName(groupName);
        return groupObject != null && groupObject.activeSelf;
    }

    private GameObject RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        if (rootTransform == null)
            return null;

        if (rootTransform.name == objectName)
            return rootTransform.gameObject;

        for (int index = 0; index < rootTransform.childCount; index++)
        {
            GameObject foundObject = RequestChildObjectByName(rootTransform.GetChild(index), objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }
}
