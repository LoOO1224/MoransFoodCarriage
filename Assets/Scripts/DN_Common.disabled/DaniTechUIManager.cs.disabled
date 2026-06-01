using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaniTechUIManager : MonoBehaviour
{
    [SerializeField] Canvas Canvas_BgRoot;
    [SerializeField] Canvas Canvas_MainRoot;
    [SerializeField] Canvas Canvas_ContentRoot;
    [SerializeField] Canvas Canvas_PopupRoot;
    [SerializeField] Canvas Canvas_VeryFrontRoot;

    public static DaniTechUIManager Instance { get; set; }

    // 얘는 생성과 제거에 관한 부분 -> Instancing과 가비지컬렉터와 연관이 있는 애
    private Dictionary<DaniTechUIType, DaniTechUIBase> _createdUIDic = new Dictionary<DaniTechUIType, DaniTechUIBase>();
    // 얘는 활성과 비활성에 관한 부분 -> SetActive
    private HashSet<DaniTechUIType> _openedUIDic = new HashSet<DaniTechUIType>();


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 매니저들이 탄생한 후에 UI매니저가 처음으로 게임이 실행될 때 필요한 UI들을 오픈해준다!
        this.ShowStartupUIOnGameStart();
    }

    public DaniTechUIBase OpenUI(DaniTechUIRootType uiRootType, DaniTechUIType uiType, bool isInitialHide = false)
    {
        // 딱히 요청이 있진 않고 오픈만 하면 되는 UI에서 사용

        var openedUI = GetCreatedUI(uiRootType, uiType);

        bool isSetActiveOnOpen = (isInitialHide == false); // 열었을 때 기본적으로 숨겨서 열 것인지 체크
        if (_openedUIDic.Contains(uiType) == false)
        {
            openedUI.gameObject.SetActive(isSetActiveOnOpen);
            _openedUIDic.Add(uiType);
        }

        return openedUI;
    }

    public void CloseUI(DaniTechUIRootType uiRootType, DaniTechUIType uiType)
    {
        if (_openedUIDic.Contains(uiType))
        {
            var openedUi = _createdUIDic[uiType];
            openedUi.gameObject.SetActive(false);
            _openedUIDic.Remove(uiType);
        }
    }

    private Transform GetRootTransform(DaniTechUIRootType uiRootType)
    {
        Transform root = null;
        switch (uiRootType) 
        {
            case DaniTechUIRootType.BackgroundUI:
                root = Canvas_BgRoot.transform;
                break;
            case DaniTechUIRootType.MainUI:
                root = Canvas_MainRoot.transform;
                break;
            case DaniTechUIRootType.ContentUI:
                root = Canvas_ContentRoot.transform;
                break;
            case DaniTechUIRootType.PopupUI:
                root = Canvas_PopupRoot.transform;
                break;
            case DaniTechUIRootType.VeryFrontUI:
                root = Canvas_VeryFrontRoot.transform;
                break;
        }
        return root;
    }

    private void CreateUI(DaniTechUIRootType uiRootType, DaniTechUIType uiType)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            string path = this.GetUIPath(uiRootType, uiType);
            GameObject loadedObj = (GameObject)Resources.Load(path);
            Transform root = GetRootTransform(uiRootType);
            GameObject gObj = Instantiate(loadedObj, root);
            if (gObj != null)
            {
                var uiBase = gObj.GetComponent<DaniTechUIBase>();
                _createdUIDic.Add(uiType, uiBase);
            }
        }
    }

    public DaniTechUIBase GetOpenedUI(DaniTechUIRootType uiRootType, DaniTechUIType uiType)
    {
        return GetCreatedUI(uiRootType, uiType);
    }

    private DaniTechUIBase GetCreatedUI(DaniTechUIRootType uiRootType, DaniTechUIType uiType)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            CreateUI(uiRootType, uiType);
        }
        return _createdUIDic[uiType];
    }


    public DaniTechUIBase OpenContentUI(DaniTechUIType uiType)
    {
        return OpenUI(DaniTechUIRootType.ContentUI, uiType);
    }

    public DaniTechUIBase OpenPopupUI(DaniTechUIType uiType)
    {
        return OpenUI(DaniTechUIRootType.PopupUI, uiType);
    }

    public void CloseContentUI(DaniTechUIType uiType)
    {
        CloseUI(DaniTechUIRootType.ContentUI, uiType);
    }

    public void ClosePopupUI(DaniTechUIType uiType)
    {
        CloseUI(DaniTechUIRootType.PopupUI, uiType);
    }

}
