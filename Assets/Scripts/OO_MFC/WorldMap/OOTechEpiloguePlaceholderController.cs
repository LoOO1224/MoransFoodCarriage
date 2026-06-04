// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechEpiloguePlaceholderController.cs
// - 역할: 로드맵, 월드맵, 스테이지 전환 흐름을 담당하는 장면 Controller입니다.
// - 감독 관점: 길 위의 장면 전환 큐시트를 들고 있는 무대감독입니다.
// - 유지보수 포인트: 배경/버튼/캐릭터 배치는 오브젝트와 View가 맡고, 이 스크립트는 순서 지휘만 맡아야 합니다.
// =============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// EpilogueGroup의 임시 엔딩 무대입니다.
/// 엔딩 크레딧 보기 버튼을 먼저 보여주고, 이후 메인 메뉴로 돌아가는 큐를 제공합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechEpiloguePlaceholderController : MonoBehaviour
{
    [SerializeField] private string _currentGroupName = "EpilogueGroup";
    [SerializeField] private string _mainMenuGroupName = "MainMenuGroup";
    [SerializeField] private int _sortingOrder = 950;

    private GameObject Root_Canvas;
    private GameObject Root_Credits;
    private Button Button_Credits;
    private Button Button_ReturnMainMenu;

    /// <summary>
    /// 에필로그 무대가 열리면 크레딧 패널은 숨기고 첫 버튼만 보여줍니다.
    /// </summary>
    private void OnEnable()
    {
        PrepareView();
        SetCreditsViewActive(false);
    }

    /// <summary>
    /// 씬에 배치된 Canvas_EpiloguePlaceholder에서 버튼과 크레딧 패널을 연결합니다.
    /// </summary>
    private void PrepareView()
    {
        if (Root_Canvas != null)
            return;

        Root_Canvas = FindChildByName(transform, "Canvas_EpiloguePlaceholder");

        if (Root_Canvas == null)
        {
            Debug.LogWarning("[OOTechEpiloguePlaceholderController] EpilogueGroup needs Canvas_EpiloguePlaceholder as a child object.");
            return;
        }

        Canvas canvas = Root_Canvas.GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = _sortingOrder;
        }

        GameObject creditsButtonObject = FindChildByName(Root_Canvas.transform, "Button_ShowEndingCredits");
        Button_Credits = creditsButtonObject != null ? creditsButtonObject.GetComponent<Button>() : null;

        if (Button_Credits != null)
        {
            Button_Credits.onClick.RemoveListener(OnCreditsButtonClicked);
            Button_Credits.onClick.AddListener(OnCreditsButtonClicked);
        }

        Root_Credits = FindChildByName(Root_Canvas.transform, "Panel_EndingCredits");
        GameObject returnButtonObject = Root_Credits != null ? FindChildByName(Root_Credits.transform, "Button_ReturnMainMenu") : null;
        Button_ReturnMainMenu = returnButtonObject != null ? returnButtonObject.GetComponent<Button>() : null;

        if (Button_ReturnMainMenu != null)
        {
            Button_ReturnMainMenu.onClick.RemoveListener(OnReturnMainMenuButtonClicked);
            Button_ReturnMainMenu.onClick.AddListener(OnReturnMainMenuButtonClicked);
        }
    }

    /// <summary>
    /// 엔딩 크레딧 보기 버튼을 누르면 크레딧 패널로 전환합니다.
    /// </summary>
    private void OnCreditsButtonClicked()
    {
        SetCreditsViewActive(true);
    }

    /// <summary>
    /// 크레딧 이후 메인 메뉴 무대로 돌아갑니다.
    /// </summary>
    private void OnReturnMainMenuButtonClicked()
    {
        GameObject currentGroupObject = FindSceneObjectByName(_currentGroupName);
        GameObject mainMenuGroupObject = FindSceneObjectByName(_mainMenuGroupName);

        if (OOTechUIManager.Inst != null)
        {
            if (currentGroupObject != null)
                OOTechUIManager.Inst.RegisterUI(_currentGroupName, currentGroupObject);

            if (mainMenuGroupObject != null)
                OOTechUIManager.Inst.RegisterUI(_mainMenuGroupName, mainMenuGroupObject);

            OOTechUIManager.Inst.CloseUI(_currentGroupName);

            if (OOTechUIManager.Inst.OpenUI(_mainMenuGroupName))
                return;
        }

        if (currentGroupObject != null)
            currentGroupObject.SetActive(false);

        if (mainMenuGroupObject != null)
            mainMenuGroupObject.SetActive(true);
    }

    /// <summary>
    /// 크레딧 패널과 첫 버튼의 표시 상태를 서로 반대로 맞춥니다.
    /// </summary>
    private void SetCreditsViewActive(bool isActive)
    {
        if (Root_Credits != null)
            Root_Credits.SetActive(isActive);

        if (Button_Credits != null)
            Button_Credits.gameObject.SetActive(!isActive);
    }

    private GameObject FindSceneObjectByName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

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
}
