// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: UIManagerExtension.cs
// - 역할: 버튼 클릭처럼 여러 UI 그룹을 잇는 공용 호출을 모아 둡니다.
// - 영화 비유: 극장 로비의 안내 데스크입니다. 관객이 "시작", "도감", "돌아가기"를 누르면
//   어떤 무대의 문을 열고 닫을지만 안내하고, 각 무대의 실제 연기는 해당 Controller가 맡습니다.
// - 유지보수 사인: 새 UI 흐름이 생겨도 여기서는 공통 이동 호출만 두고, 세부 연출은 그룹 컴포넌트로 분리합니다.
// =============================================================================
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// UIManagerExtension
/// 버튼 이벤트에서 자주 쓰는 UI 이동 흐름을 정리한 정적 클래스입니다.
/// UIManager가 실제 문을 열고 닫는 무대 관리자라면, 이 클래스는 "어느 문으로 갈지"만 정하는 안내판입니다.
/// </summary>
public static class UIManagerExtension
{
    /// <summary>
    /// 시작하기 버튼을 누르면 메인 메뉴를 닫고 Prologue1Group을 엽니다.
    /// Game View에서는 메인 메뉴가 사라지고 첫 프롤로그 무대가 켜집니다.
    /// </summary>
    public static void OnStartButtonClicked()
    {
        Debug.Log("[UIManagerExtension] Start button clicked. Opening Prologue1Group.");

        if (OOTechUIManager.Inst == null)
        {
            Debug.LogError("[UIManagerExtension] OOTechUIManager를 찾을 수 없습니다.");
            return;
        }

        OOTechUIManager.Inst.CloseUI("MainMenuGroup");
        OOTechUIManager.Inst.CloseUI("DialogueGroup");
        OOTechUIManager.Inst.OpenUI("Prologue1Group");
    }

    /// <summary>
    /// 메인 메뉴의 도감 버튼을 누르면 CodexGroup을 엽니다.
    /// 현재 도감은 발표 전 안내 모드이므로 목록 대신 안내 문구를 보여줍니다.
    /// </summary>
    public static void OnCodexButtonClicked()
    {
        Debug.Log("[UIManagerExtension] Codex button clicked. Opening CodexGroup.");

        if (OOTechUIManager.Inst == null)
        {
            Debug.LogError("[UIManagerExtension] OOTechUIManager를 찾을 수 없습니다.");
            return;
        }

        OOTechUIManager.Inst.CloseUI("MainMenuGroup");
        OOTechUIManager.Inst.OpenUI("CodexGroup");
    }

    /// <summary>
    /// 종료 버튼을 누르면 애플리케이션 종료를 요청합니다.
    /// 에디터에서는 종료 로그만 보이고, 빌드된 게임에서는 프로그램이 닫힙니다.
    /// </summary>
    public static void OnExitButtonClicked()
    {
        Debug.Log("[UIManagerExtension] Exit button clicked. Application quit requested.");
        Application.Quit();
    }

    /// <summary>
    /// 공용 돌아가기 버튼을 누르면 이전 그룹으로 돌아갑니다.
    /// Road/Stage에서 도감으로 들어온 경우에는 HUD와 입력 잠금까지 복구합니다.
    /// </summary>
    public static void OnBackButtonClicked(string previousGroupName = "MainMenuGroup")
    {
        string safePreviousGroupName = string.IsNullOrEmpty(previousGroupName) ? "MainMenuGroup" : previousGroupName;
        safePreviousGroupName = ResolveStage3EncounterBackTarget(safePreviousGroupName);
        Debug.Log($"[UIManagerExtension] Back button clicked. Returning to {safePreviousGroupName}.");

        if (OOTechUIManager.Inst == null)
        {
            Debug.LogError("[UIManagerExtension] OOTechUIManager를 찾을 수 없습니다.");
            return;
        }

        OOTechUIManager.Inst.CloseUI("CodexGroup");
        OOTechUIManager.Inst.CloseUI("CookingGroup");
        OOTechUIManager.Inst.CloseUI("WorldMapGroup");
        OOTechUIManager.Inst.OpenUI(safePreviousGroupName);
        RestoreRoadHUDIfNeeded(safePreviousGroupName);
    }

    /// <summary>
    /// Stage3 부엌에서 돌아올 때 이전 기록이 Stage2Group으로 남아 있으면 산군 선택지가 끊깁니다.
    /// Game View에서는 떡/꿀떡을 가진 배우가 반드시 EncounterGroup 무대로 복귀하게 하는 안전 큐입니다.
    /// </summary>
    private static string ResolveStage3EncounterBackTarget(string requestedGroupName)
    {
        if (requestedGroupName == "EncounterGroup")
            return requestedGroupName;

        if (OOTechGameManager.Inst == null)
            return requestedGroupName;

        bool hasStage3QuestItem =
            OOTechGameManager.Inst.GetItemCount("OO_KoreanCake_1") > 0 ||
            OOTechGameManager.Inst.GetItemCount("OO_HoneyKoreanCake_1") > 0;

        if (!hasStage3QuestItem)
            return requestedGroupName;

        GameObject encounterGroupObject = FindSceneObjectByName("EncounterGroup");

        if (encounterGroupObject == null)
            return requestedGroupName;

        Debug.LogWarning($"[UIManagerExtension] Stage3 cooking return corrected. {requestedGroupName} -> EncounterGroup");
        return "EncounterGroup";
    }

    /// <summary>
    /// Road/Stage에서 도감으로 갔다가 돌아오면 HUD와 플레이어 입력을 다시 켭니다.
    /// 영화로 치면 도감 로비에서 돌아온 뒤, 무대 조명과 배우 동선을 다시 원래 큐로 복구하는 단계입니다.
    /// </summary>
    private static void RestoreRoadHUDIfNeeded(string previousGroupName)
    {
        if (OOTechUIManager.Inst == null || string.IsNullOrEmpty(previousGroupName))
            return;

        GameObject previousGroupObject = OOTechUIManager.Inst.GetCreatedUI(previousGroupName);

        if (previousGroupObject == null)
            previousGroupObject = FindSceneObjectByName(previousGroupName);

        if (previousGroupObject == null)
            return;

        OOTechRoadHUDController hudController = previousGroupObject.GetComponent<OOTechRoadHUDController>();

        if (hudController == null)
            hudController = previousGroupObject.GetComponentInChildren<OOTechRoadHUDController>(true);

        if (hudController == null)
            return;

        Time.timeScale = 1f;
        hudController.RequestRestoreFromOverlayReturn();
        Debug.Log($"[UIManagerExtension] Road HUD restored after returning to {previousGroupName}.");
    }

    /// <summary>
    /// UIManager 등록 목록에 없던 씬 그룹도 이름으로 찾아옵니다.
    /// 비활성 그룹까지 찾아야 하므로 씬 루트부터 자식들을 직접 훑습니다.
    /// </summary>
    private static GameObject FindSceneObjectByName(string objectName)
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (!activeScene.IsValid())
            return null;

        foreach (GameObject rootObject in activeScene.GetRootGameObjects())
        {
            GameObject foundObject = FindChildByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private static GameObject FindChildByName(Transform rootTransform, string objectName)
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
