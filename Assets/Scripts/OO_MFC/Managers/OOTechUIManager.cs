// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechUIManager.cs
// - 역할: 여러 장면에서 함께 쓰는 공통 Manager입니다.
// - 감독 관점: 각 부서에 공통 창구를 열어 주는 제작 본부입니다.
// - 유지보수 포인트: 특정 장면의 세부 연출을 직접 처리하지 말고, 공통 조회/등록/요청 API만 유지합니다.
// =============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// OOTechUIManager
/// 씬에 배치된 UI 그룹을 등록하고 Open / Close 상태를 중앙에서 관리합니다.
/// PDF 규칙에 따라 신규 UI 확장 로직은 UIManagerExtension에서 호출하고,
/// 이 클래스는 순수하게 UI 그룹의 생성 목록과 열린 목록만 관리합니다.
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
        "Stage4Group",
        "FinalStageGroup",
        "EpilogueGroup",
        "DialogueGroup",
        "TutorialGuideGroup"
    };

    // ==================== UI Dictionary ====================
    private readonly Dictionary<string, GameObject> _createdUIDic = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, GameObject> _openedUIDic = new Dictionary<string, GameObject>();

    /// <summary>
    /// 씬 전환 후에도 유지되는 단일 UI 매니저로 등록하고 초기 UI 그룹을 등록합니다.
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
    /// 매니저가 파괴될 때 전역 참조를 비웁니다.
    /// </summary>
    private void OnDestroy()
    {
        if (Inst == this)
            Inst = null;
    }

    // ==================== UI 등록 ====================

    /// <summary>
    /// 인스펙터에서 등록한 씬 UI 그룹을 시작 시점에 모두 등록합니다.
    /// 비활성 오브젝트도 여기서 등록되므로 OpenUI 호출로 안정적으로 다시 켤 수 있습니다.
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
    /// 인스펙터 등록표가 빠진 그룹도 씬 이름표를 기준으로 자동 등록합니다.
    /// 감독이 그룹 오브젝트를 복사해도 UIManager가 다시 무대를 찾는 안전망입니다.
    /// </summary>
    private void RegisterKnownSceneGroupArray()
    {
        foreach (string groupName in _autoRegisterGroupNameArray)
        {
            if (string.IsNullOrEmpty(groupName) || _createdUIDic.ContainsKey(groupName))
                continue;

            GameObject groupObject = FindSceneObjectByName(groupName);

            if (groupObject != null)
                RegisterUI(groupName, groupObject);
        }
    }

    /// <summary>
    /// EventSystem이 없으면 UI 버튼 클릭이 먹지 않으므로 최소 입력 소품을 준비합니다.
    /// </summary>
    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();

        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        if (eventSystem.GetComponent<BaseInputModule>() == null)
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
    }

    /// <summary>
    /// UI 그룹을 생성 목록에 등록합니다.
    /// 이미 등록된 이름은 최신 참조로 갱신합니다.
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

    // ==================== UI 열기 / 닫기 ====================

    /// <summary>
    /// 등록된 UI 그룹을 활성화합니다.
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
    /// 등록된 UI 그룹을 비활성화합니다.
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
    /// 닫을 UI와 열 UI를 한 번에 전환합니다.
    /// 열 대상이 등록되어 있지 않으면 현재 UI를 닫지 않습니다.
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

    // ==================== UI 조회 ====================

    /// <summary>
    /// 등록된 UI 그룹 오브젝트를 반환합니다.
    /// 다른 컨트롤러가 UI 참조를 직접 찾지 않고 UIManager를 통해 가져올 때 사용합니다.
    /// </summary>
    public GameObject GetCreatedUI(string uiName)
    {
        return TryGetCreatedUI(uiName, out GameObject uiObject) ? uiObject : null;
    }

    /// <summary>
    /// UI가 현재 열려있는지 확인합니다.
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
        GameObject groupObject = FindSceneObjectByName(uiName);

        if (groupObject == null)
            return false;

        RegisterUI(uiName, groupObject);
        return true;
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

    // ==================== 디버그 ====================

    /// <summary>
    /// 현재 등록된 UI 목록을 콘솔에 출력합니다.
    /// </summary>
    public void PrintRegisteredUI()
    {
        Debug.Log("========== 등록된 UI 목록 ==========");

        foreach (KeyValuePair<string, GameObject> pair in _createdUIDic)
            Debug.Log($"[OOTechUIManager] {pair.Key} / ActiveSelf: {pair.Value.activeSelf}");
    }

    // ==================== 편의 메서드 ====================

    /// <summary>
    /// MainMenuGroup을 엽니다. 다른 컨트롤러의 간단 호출용 편의 메서드입니다.
    /// </summary>
    public void ShowMainMenu()
    {
        OpenUI("MainMenuGroup");
    }

    /// <summary>
    /// MainMenuGroup을 닫습니다. 다른 컨트롤러의 간단 호출용 편의 메서드입니다.
    /// </summary>
    public void HideMainMenu()
    {
        CloseUI("MainMenuGroup");
    }
}

/// <summary>
/// 인스펙터에서 UI 이름과 씬 오브젝트를 함께 묶어 등록하기 위한 데이터입니다.
/// </summary>
[Serializable]
public class OOTechUIGroupReference
{
    public string Name;
    public GameObject Group;
}
