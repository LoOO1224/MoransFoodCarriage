using System;
using System.Collections.Generic;
using UnityEngine;

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

    // ==================== UI Dictionary ====================
    private readonly Dictionary<string, GameObject> _createdUIDic = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, GameObject> _openedUIDic = new Dictionary<string, GameObject>();

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

        Debug.Log("[OOTechUIManager] 초기화 완료");
    }

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
        if (!ContainsCreatedUI(openingUIName))
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
        {
            Debug.LogWarning($"[OOTechUIManager] 등록되지 않은 UI입니다: {uiName}");
            return false;
        }

        return true;
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

    public void ShowMainMenu()
    {
        OpenUI("MainMenuGroup");
    }

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
