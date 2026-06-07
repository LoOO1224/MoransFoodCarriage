// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechUIManager.cs
// - ??븷: ?щ윭 ?λ㈃?먯꽌 ?④퍡 ?곕뒗 怨듯넻 Manager?낅땲??
// - 媛먮룆 愿?? 媛?遺?쒖뿉 怨듯넻 李쎄뎄瑜??댁뼱 二쇰뒗 ?쒖옉 蹂몃??낅땲??
// - ?좎?蹂댁닔 ?ъ씤?? ?뱀젙 ?λ㈃???몃? ?곗텧??吏곸젒 泥섎━?섏? 留먭퀬, 怨듯넻 議고쉶/?깅줉/?붿껌 API留??좎??⑸땲??
// =============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// OOTechUIManager
/// ?ъ뿉 諛곗튂??UI 洹몃９???깅줉?섍퀬 Open / Close ?곹깭瑜?以묒븰?먯꽌 愿由ы빀?덈떎.
/// PDF 洹쒖튃???곕씪 ?좉퇋 UI ?뺤옣 濡쒖쭅? UIManagerExtension?먯꽌 ?몄텧?섍퀬,
/// ???대옒?ㅻ뒗 ?쒖닔?섍쾶 UI 洹몃９???앹꽦 紐⑸줉怨??대┛ 紐⑸줉留?愿由ы빀?덈떎.
/// </summary>
public class OOTechUIManager : MonoBehaviour
{
    public static OOTechUIManager Inst { get; private set; }

    [Header("Scene UI Groups")]
    [SerializeField] private OOTechUIGroupReference[] UIGroup_InitialArray = Array.Empty<OOTechUIGroupReference>();

    private static readonly string[] _autoRegisterGroupNameArray =
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
        "EncounterGroup",
        "Stage4_1Group",
        "Stage4_2Group",
        "Stage4Group",
        "PreFinal_Narration",
        "FinalStageGroup",
        "EpilogueGroup",
        "EndingCreditGroup",
        "DialogueGroup",
        "TutorialGuideGroup"
    };

    // ==================== UI Dictionary ====================
    private readonly Dictionary<string, GameObject> _createdUIDic = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, GameObject> _openedUIDic = new Dictionary<string, GameObject>();

    /// <summary>
    /// ???꾪솚 ?꾩뿉???좎??섎뒗 ?⑥씪 UI 留ㅻ땲?濡??깅줉?섍퀬 珥덇린 UI 洹몃９???깅줉?⑸땲??
    /// </summary>
    private void Awake()
    {
        if (Inst != null && Inst != this)
        {
            Destroy(gameObject);
            return;
        }

        Inst = this;
        DontDestroyOnLoad(gameObject);

        RegisterInitialUIGroupArray();
        RegisterKnownSceneGroupArray();
        EnsureEventSystem();

        Debug.Log("[OOTechUIManager] 초기화 완료");
    }

    /// <summary>
    /// 留ㅻ땲?媛 ?뚭눼?????꾩뿭 李몄“瑜?鍮꾩썎?덈떎.
    /// </summary>
    private void OnDestroy()
    {
        if (Inst == this)
            Inst = null;
    }

    // ==================== UI ?깅줉 ====================

    /// <summary>
    /// ?몄뒪?숉꽣?먯꽌 ?깅줉????UI 洹몃９???쒖옉 ?쒖젏??紐⑤몢 ?깅줉?⑸땲??
    /// 鍮꾪솢???ㅻ툕?앺듃???ш린???깅줉?섎?濡?OpenUI ?몄텧濡??덉젙?곸쑝濡??ㅼ떆 耳????덉뒿?덈떎.
    /// </summary>
    private void RegisterInitialUIGroupArray()
    {
        foreach (OOTechUIGroupReference uiGroupReference in UIGroup_InitialArray)
        {
            if (uiGroupReference == null)
                continue;

            RegisterUI(uiGroupReference.Name, uiGroupReference.Group);
        }
    }

    /// <summary>
    /// ?몄뒪?숉꽣 ?깅줉?쒓? 鍮좎쭊 洹몃９?????대쫫?쒕? 湲곗??쇰줈 ?먮룞 ?깅줉?⑸땲??
    /// 媛먮룆??洹몃９ ?ㅻ툕?앺듃瑜?蹂듭궗?대룄 UIManager媛 ?ㅼ떆 臾대?瑜?李얜뒗 ?덉쟾留앹엯?덈떎.
    /// </summary>
    private void RegisterKnownSceneGroupArray()
    {
        foreach (string groupName in _autoRegisterGroupNameArray)
        {
            if (string.IsNullOrEmpty(groupName) || _createdUIDic.ContainsKey(groupName))
                continue;

            GameObject groupObject = RequestSceneObjectByName(groupName);

            if (groupObject != null)
                RegisterUI(groupName, groupObject);
        }
    }

    /// <summary>
    /// EventSystem???놁쑝硫?UI 踰꾪듉 ?대┃??癒뱀? ?딆쑝誘濡?理쒖냼 ?낅젰 ?뚰뭹??以鍮꾪빀?덈떎.
    /// </summary>
    private void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        if (eventSystem.GetComponent<BaseInputModule>() == null)
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
    }

    /// <summary>
    /// UI 洹몃９???앹꽦 紐⑸줉???깅줉?⑸땲??
    /// ?대? ?깅줉???대쫫? 理쒖떊 李몄“濡?媛깆떊?⑸땲??
    /// </summary>
    public void RegisterUI(string uiName, GameObject uiObject)
    {
        if (string.IsNullOrEmpty(uiName))
        {
            Debug.LogWarning("[OOTechUIManager] UI 이름이 비어 있어 등록할 수 없습니다.");
            return;
        }

        if (uiObject == null)
        {
            Debug.LogWarning($"[OOTechUIManager] UI 오브젝트가 없어 등록할 수 없습니다: {uiName}");
            return;
        }

        _createdUIDic[uiName] = uiObject;

        if (uiObject.activeSelf)
            _openedUIDic[uiName] = uiObject;
        else
            _openedUIDic.Remove(uiName);

        Debug.Log($"[OOTechUIManager] UI 등록 완료: {uiName}");
    }

    // ==================== UI ?닿린 / ?リ린 ====================

    /// <summary>
    /// ?깅줉??UI 洹몃９???쒖꽦?뷀빀?덈떎.
    /// </summary>
    public bool OpenUI(string uiName)
    {
        if (!TryGetCreatedUI(uiName, out GameObject uiObject))
            return false;

        uiObject.SetActive(true);
        _openedUIDic[uiName] = uiObject;

        Debug.Log($"[OOTechUIManager] OpenUI 성공: {uiName}");
        return true;
    }

    /// <summary>
    /// ?깅줉??UI 洹몃９??鍮꾪솢?깊솕?⑸땲??
    /// </summary>
    public bool CloseUI(string uiName)
    {
        if (!TryGetCreatedUI(uiName, out GameObject uiObject))
            return false;

        uiObject.SetActive(false);
        _openedUIDic.Remove(uiName);

        Debug.Log($"[OOTechUIManager] CloseUI 성공: {uiName}");
        return true;
    }

    /// <summary>
    /// ?レ쓣 UI? ??UI瑜???踰덉뿉 ?꾪솚?⑸땲??
    /// ????곸씠 ?깅줉?섏뼱 ?덉? ?딆쑝硫??꾩옱 UI瑜??レ? ?딆뒿?덈떎.
    /// </summary>
    public bool SwitchUI(string closingUIName, string openingUIName)
    {
        if (!ContainsCreatedUI(openingUIName) && !TryAutoRegisterSceneGroup(openingUIName))
        {
            Debug.LogWarning($"[OOTechUIManager] 전환 대상 UI가 등록되지 않았습니다: {openingUIName}");
            return false;
        }

        CloseUI(closingUIName);
        return OpenUI(openingUIName);
    }

    // ==================== UI 議고쉶 ====================

    /// <summary>
    /// ?깅줉??UI 洹몃９ ?ㅻ툕?앺듃瑜?諛섑솚?⑸땲??
    /// ?ㅻⅨ 而⑦듃濡ㅻ윭媛 UI 李몄“瑜?吏곸젒 李얠? ?딄퀬 UIManager瑜??듯빐 媛?몄삱 ???ъ슜?⑸땲??
    /// </summary>
    public GameObject GetCreatedUI(string uiName)
    {
        return TryGetCreatedUI(uiName, out GameObject uiObject) ? uiObject : null;
    }

    /// <summary>
    /// UI媛 ?꾩옱 ?대젮?덈뒗吏 ?뺤씤?⑸땲??
    /// </summary>
    public bool IsOpenedUI(string uiName)
    {
        return !string.IsNullOrEmpty(uiName) && _openedUIDic.ContainsKey(uiName);
    }

    private bool ContainsCreatedUI(string uiName)
    {
        return !string.IsNullOrEmpty(uiName) && _createdUIDic.ContainsKey(uiName);
    }

    private bool TryGetCreatedUI(string uiName, out GameObject uiObject)
    {
        uiObject = null;

        if (string.IsNullOrEmpty(uiName))
        {
            Debug.LogWarning("[OOTechUIManager] UI 이름이 비어 있습니다.");
            return false;
        }

        if (!_createdUIDic.TryGetValue(uiName, out uiObject) || uiObject == null)
            TryAutoRegisterSceneGroup(uiName);

        if (!_createdUIDic.TryGetValue(uiName, out uiObject) || uiObject == null)
        {
            Debug.LogWarning($"[OOTechUIManager] 등록되지 않은 UI입니다: {uiName}");
            return false;
        }

        return true;
    }

    private bool TryAutoRegisterSceneGroup(string uiName)
    {
        GameObject groupObject = RequestSceneObjectByName(uiName);

        if (groupObject == null)
            return false;

        RegisterUI(uiName, groupObject);
        return true;
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

    // ==================== ?붾쾭洹?====================

    /// <summary>
    /// ?꾩옱 ?깅줉??UI 紐⑸줉??肄섏넄??異쒕젰?⑸땲??
    /// </summary>
    public void PrintRegisteredUI()
    {
        Debug.Log("========== 등록된 UI 목록 ==========");

        foreach (KeyValuePair<string, GameObject> pair in _createdUIDic)
            Debug.Log($"[OOTechUIManager] {pair.Key} / ActiveSelf: {pair.Value.activeSelf}");
    }

    // ==================== ?몄쓽 硫붿꽌??====================

    /// <summary>
    /// MainMenuGroup???쎈땲?? ?ㅻⅨ 而⑦듃濡ㅻ윭??媛꾨떒 ?몄텧???몄쓽 硫붿꽌?쒖엯?덈떎.
    /// </summary>
    public void ShowMainMenu()
    {
        OpenUI("MainMenuGroup");
    }

    /// <summary>
    /// MainMenuGroup???レ뒿?덈떎. ?ㅻⅨ 而⑦듃濡ㅻ윭??媛꾨떒 ?몄텧???몄쓽 硫붿꽌?쒖엯?덈떎.
    /// </summary>
    public void HideMainMenu()
    {
        CloseUI("MainMenuGroup");
    }
}

/// <summary>
/// ?몄뒪?숉꽣?먯꽌 UI ?대쫫怨????ㅻ툕?앺듃瑜??④퍡 臾띠뼱 ?깅줉?섍린 ?꾪븳 ?곗씠?곗엯?덈떎.
/// </summary>
[Serializable]
public class OOTechUIGroupReference
{
    public string Name;
    public GameObject Group;
}

