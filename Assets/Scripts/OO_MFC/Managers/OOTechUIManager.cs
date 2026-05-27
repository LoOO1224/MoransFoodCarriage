using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// OOTechUIManager
/// PDF 스타일에 따라 UI 그룹들을 중앙에서 관리하는 매니저입니다.
/// </summary>
public class OOTechUIManager : MonoBehaviour
{
    public static OOTechUIManager Inst { get; private set; }

    [Header("Scene UI Groups - Inspector에서 그룹을 등록하세요")]
    [SerializeField] private UIGroupReference[] _initialGroups = new UIGroupReference[0];

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

        foreach (UIGroupReference group in _initialGroups)
        {
            if (group != null && group.Group != null)
            {
                RegisterUI(group.Name, group.Group);
            }
        }

        Debug.Log("[OOTechUIManager] 초기화 완료");
    }

    private void OnDestroy()
    {
        if (Inst == this)
            Inst = null;
    }

    // ==================== UI 등록 ====================
    public void RegisterUI(string uiName, GameObject uiObject)
    {
        if (string.IsNullOrEmpty(uiName) || uiObject == null) return;

        _createdUIDic[uiName] = uiObject;

        if (uiObject.activeSelf)
            _openedUIDic[uiName] = uiObject;

        Debug.Log($"[OOTechUIManager] UI 등록 완료: {uiName}");
    }

    // ==================== UI 제어 ====================
    public bool OpenUI(string uiName)
    {
        if (!_createdUIDic.TryGetValue(uiName, out GameObject ui) || ui == null)
        {
            Debug.LogWarning($"[OOTechUIManager] 등록되지 않은 UI: {uiName}");
            return false;
        }

        ui.SetActive(true);
        _openedUIDic[uiName] = ui;
        Debug.Log($"[OOTechUIManager] OpenUI 성공: {uiName}");
        return true;
    }

    public bool CloseUI(string uiName)
    {
        if (!_createdUIDic.TryGetValue(uiName, out GameObject ui) || ui == null)
        {
            Debug.LogWarning($"[OOTechUIManager] 등록되지 않은 UI: {uiName}");
            return false;
        }

        ui.SetActive(false);
        _openedUIDic.Remove(uiName);
        Debug.Log($"[OOTechUIManager] CloseUI 성공: {uiName}");
        return true;
    }

    public bool SwitchUI(string closingUIName, string openingUIName)
    {
        CloseUI(closingUIName);
        return OpenUI(openingUIName);
    }

    // ==================== 디버깅용 ====================
    public void PrintRegisteredUI()
    {
        Debug.Log("=== 등록된 UI 목록 ===");
        foreach (var pair in _createdUIDic)
        {
            Debug.Log($"등록된 UI: {pair.Key} (활성: {pair.Value.activeSelf})");
        }
    }

    // ==================== 편의 메서드 ====================
    public void ShowMainMenu() => OpenUI("MainMenuGroup");
    public void HideMainMenu() => CloseUI("MainMenuGroup");
}

// ==================== Inspector에서 사용되는 클래스 ====================
[System.Serializable]
public class UIGroupReference
{
    public string Name;
    public GameObject Group;
}