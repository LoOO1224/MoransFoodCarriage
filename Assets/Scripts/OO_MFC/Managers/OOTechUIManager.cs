using UnityEngine;
using System.Collections.Generic;

public class OOTechUIManager : MonoBehaviour
{
    public static OOTechUIManager Inst { get; private set; }

    // 생성된 UI 관리
    private Dictionary<string, GameObject> _createdUIDic = new Dictionary<string, GameObject>();
    // 현재 열려있는 UI 관리
    private Dictionary<string, GameObject> _openedUIDic = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Inst != null && Inst != this)
        {
            Destroy(gameObject);
            return;
        }
        Inst = this;
        DontDestroyOnLoad(gameObject);
    }

    // ==================== UI Open / Close ====================

    public void OpenUI(string uiName)
    {
        if (string.IsNullOrEmpty(uiName)) return;

        Debug.Log($"[OOTechUIManager] OpenUI: {uiName}");

        if (_openedUIDic.ContainsKey(uiName))
        {
            Debug.LogWarning($"이미 열려있는 UI입니다: {uiName}");
            return;
        }

        // TODO: 실제 UI 생성 로직 (나중에 ResourceManager와 연동)
        if (_createdUIDic.TryGetValue(uiName, out GameObject ui))
        {
            ui.SetActive(true);
            _openedUIDic.Add(uiName, ui);
        }
    }

    public void CloseUI(string uiName)
    {
        if (string.IsNullOrEmpty(uiName)) return;

        Debug.Log($"[OOTechUIManager] CloseUI: {uiName}");

        if (_openedUIDic.ContainsKey(uiName))
        {
            _openedUIDic[uiName].SetActive(false);
            _openedUIDic.Remove(uiName);
        }
    }

    // ==================== Group 관리용 편의 메서드 ====================

    public void ShowMainMenu()
    {
        OpenUI("MainMenuGroup");
    }

    public void HideMainMenu()
    {
        CloseUI("MainMenuGroup");
    }
}