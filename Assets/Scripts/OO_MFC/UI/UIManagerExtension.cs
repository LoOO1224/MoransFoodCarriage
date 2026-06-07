// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: UIManagerExtension.cs
// - ??븷: 踰꾪듉 ?대┃泥섎읆 ?щ윭 UI 洹몃９???뉖뒗 怨듭슜 ?몄텧??紐⑥븘 ?〓땲??
// - ?곹솕 鍮꾩쑀: 洹뱀옣 濡쒕퉬???덈궡 ?곗뒪?ъ엯?덈떎. 愿媛앹씠 "?쒖옉", "?꾧컧", "?뚯븘媛湲?瑜??꾨Ⅴ硫?//   ?대뼡 臾대???臾몄쓣 ?닿퀬 ?レ쓣吏留??덈궡?섍퀬, 媛?臾대????ㅼ젣 ?곌린???대떦 Controller媛 留≪뒿?덈떎.
// - ?좎?蹂댁닔 ?ъ씤: ??UI ?먮쫫???앷꺼???ш린?쒕뒗 怨듯넻 ?대룞 ?몄텧留??먭퀬, ?몃? ?곗텧? 洹몃９ 而댄룷?뚰듃濡?遺꾨━?⑸땲??
// =============================================================================
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// UIManagerExtension
/// 踰꾪듉 ?대깽?몄뿉???먯＜ ?곕뒗 UI ?대룞 ?먮쫫???뺣━???뺤쟻 ?대옒?ㅼ엯?덈떎.
/// UIManager媛 ?ㅼ젣 臾몄쓣 ?닿퀬 ?ル뒗 臾대? 愿由ъ옄?쇰㈃, ???대옒?ㅻ뒗 "?대뒓 臾몄쑝濡?媛덉?"留??뺥븯???덈궡?먯엯?덈떎.
/// </summary>
public static class UIManagerExtension
{
    /// <summary>
    /// ?쒖옉?섍린 踰꾪듉???꾨Ⅴ硫?硫붿씤 硫붾돱瑜??リ퀬 Prologue1Group???쎈땲??
    /// Game View?먯꽌??硫붿씤 硫붾돱媛 ?щ씪吏怨?泥??꾨·濡쒓렇 臾대?媛 耳쒖쭛?덈떎.
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
    /// 硫붿씤 硫붾돱???꾧컧 踰꾪듉???꾨Ⅴ硫?CodexGroup???쎈땲??
    /// ?꾩옱 ?꾧컧? 諛쒗몴 ???덈궡 紐⑤뱶?대?濡?紐⑸줉 ????덈궡 臾멸뎄瑜?蹂댁뿬以띾땲??
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
    /// 醫낅즺 踰꾪듉???꾨Ⅴ硫??좏뵆由ъ??댁뀡 醫낅즺瑜??붿껌?⑸땲??
    /// ?먮뵒?곗뿉?쒕뒗 醫낅즺 濡쒓렇留?蹂댁씠怨? 鍮뚮뱶??寃뚯엫?먯꽌???꾨줈洹몃옩???ロ옓?덈떎.
    /// </summary>
    public static void OnExitButtonClicked()
    {
        Debug.Log("[UIManagerExtension] Exit button clicked. Application quit requested.");
        Application.Quit();
    }

    /// <summary>
    /// 怨듭슜 ?뚯븘媛湲?踰꾪듉???꾨Ⅴ硫??댁쟾 洹몃９?쇰줈 ?뚯븘媛묐땲??
    /// Road/Stage?먯꽌 ?꾧컧?쇰줈 ?ㅼ뼱??寃쎌슦?먮뒗 HUD? ?낅젰 ?좉툑源뚯? 蹂듦뎄?⑸땲??
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
    /// Stage3 遺?뚯뿉???뚯븘?????댁쟾 湲곕줉??Stage2Group?쇰줈 ?⑥븘 ?덉쑝硫??곌뎔 ?좏깮吏媛 ?딄퉩?덈떎.
    /// Game View?먯꽌????轅?≪쓣 媛吏?諛곗슦媛 諛섎뱶??EncounterGroup 臾대?濡?蹂듦??섍쾶 ?섎뒗 ?덉쟾 ?먯엯?덈떎.
    /// </summary>
    private static string ResolveStage3EncounterBackTarget(string requestedGroupName)
    {
        if (requestedGroupName == "Stage4_2Group")
            return requestedGroupName;

        if (requestedGroupName == "EncounterGroup")
            return requestedGroupName;

        if (OOTechGameManager.Inst == null)
            return requestedGroupName;

        bool hasStage3QuestItem =
            OOTechGameManager.Inst.GetItemCount("OO_KoreanCake_1") > 0 ||
            OOTechGameManager.Inst.GetItemCount("OO_HoneyKoreanCake_1") > 0;

        if (!hasStage3QuestItem)
            return requestedGroupName;

        GameObject encounterGroupObject = RequestSceneObjectByName("EncounterGroup");

        if (encounterGroupObject == null)
            return requestedGroupName;

        Debug.LogWarning($"[UIManagerExtension] Stage3 cooking return corrected. {requestedGroupName} -> EncounterGroup");
        return "EncounterGroup";
    }

    /// <summary>
    /// Road/Stage?먯꽌 ?꾧컧?쇰줈 媛붾떎媛 ?뚯븘?ㅻ㈃ HUD? ?뚮젅?댁뼱 ?낅젰???ㅼ떆 耳?땲??
    /// ?곹솕濡?移섎㈃ ?꾧컧 濡쒕퉬?먯꽌 ?뚯븘???? 臾대? 議곕챸怨?諛곗슦 ?숈꽑???ㅼ떆 ?먮옒 ?먮줈 蹂듦뎄?섎뒗 ?④퀎?낅땲??
    /// </summary>
    private static void RestoreRoadHUDIfNeeded(string previousGroupName)
    {
        if (OOTechUIManager.Inst == null || string.IsNullOrEmpty(previousGroupName))
            return;

        GameObject previousGroupObject = OOTechUIManager.Inst.GetCreatedUI(previousGroupName);

        if (previousGroupObject == null)
            previousGroupObject = RequestSceneObjectByName(previousGroupName);

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
    /// UIManager ?깅줉 紐⑸줉???녿뜕 ??洹몃９???대쫫?쇰줈 李얠븘?듬땲??
    /// 鍮꾪솢??洹몃９源뚯? 李얠븘???섎?濡???猷⑦듃遺???먯떇?ㅼ쓣 吏곸젒 ?묒뒿?덈떎.
    /// </summary>
    private static GameObject RequestSceneObjectByName(string objectName)
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (!activeScene.IsValid())
            return null;

        foreach (GameObject rootObject in activeScene.GetRootGameObjects())
        {
            GameObject foundObject = RequestChildObjectByName(rootObject.transform, objectName);

            if (foundObject != null)
                return foundObject;
        }

        return null;
    }

    private static GameObject RequestChildObjectByName(Transform rootTransform, string objectName)
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

