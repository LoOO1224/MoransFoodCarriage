// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechTutorial2Controller.cs
// - ??븷: 濡쒕뱶留? ?붾뱶留? ?ㅽ뀒?댁? ?꾪솚 ?먮쫫???대떦?섎뒗 ?λ㈃ Controller?낅땲??
// - 媛먮룆 愿?? 湲??꾩쓽 ?λ㈃ ?꾪솚 ?먯떆?몃? ?ㅺ퀬 ?덈뒗 臾대?媛먮룆?낅땲??
// - ?좎?蹂댁닔 ?ъ씤?? 諛곌꼍/踰꾪듉/罹먮┃??諛곗튂???ㅻ툕?앺듃? View媛 留↔퀬, ???ㅽ겕由쏀듃???쒖꽌 吏?섎쭔 留≪븘???⑸땲??
// =============================================================================
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 1st_Road_to_Stage1??HUD ?뚭컻? 珥덈컲 ????먯떆?몃? ?대떦?⑸땲??
/// Road 媛먮룆? ?대룞留?留↔퀬, ??諛곗슦??UI 媛?대뱶? ?곗씠??????쒖꽌留?留≪뒿?덈떎.
/// </summary>
[DisallowMultipleComponent]
public class OOTechTutorial2Controller : MonoBehaviour
{
    private static bool _isRiceStarterRewardGiven;
    private static bool _isVegetableStarterRewardGiven;

    [Header("Run Rule")]
    [SerializeField] private string _firstRoadGroupName = "1st_Road_to_Stage1";

    [Header("UI Group")]
    [SerializeField] private string _dialogueGroupName = "DialogueGroup";
    [SerializeField] private string _tutorialGuideGroupName = "TutorialGuideGroup";

    [Header("Tutorial Data Id")]
    [SerializeField] private string[] _hudTutorialIdArray =
    {
        "narration_tutorial_04",
        "narration_tutorial_05",
        "narration_tutorial_06",
        "narration_tutorial_07",
        "narration_tutorial_08"
    };
    [SerializeField] private string _missionTutorialId = "narration_tutorial_09";

    [Header("Dialogue Data Id")]
    [SerializeField] private string _chunyangRiceDialogueId = "character_Chunyang_03";
    [SerializeField] private string _moranPumpkinDialogueId = "character_Moran_03";
    [SerializeField] private string _mrJaeikReadyDialogueId = "character_Mr.Jaeik_03";
    [SerializeField] private string _chunyangEastDialogueId = "character_Chunyang_04";
    [SerializeField] private string _roadMap1MrJaeikDialogueId = "character_Mr.Jaeik_04";
    [SerializeField] private string _roadMap1ChunyangDialogueId = "character_Chunyang_05";
    [SerializeField] private string _roadMap1MoranDialogueId = "character_Moran_04";
    [SerializeField] private string _roadMap1TutorialId = "narration_tutorial_10";

    [Header("Ingredient Data Id")]
    [SerializeField] private string _riceIngredientId = "Ing_Rice_01";
    [SerializeField] private string _vegetableIngredientId = "Ing_Veggie_01";
    [SerializeField] private int _riceIngredientCount = 12;
    [SerializeField] private int _vegetableIngredientCount = 12;

    private readonly OOTechRoadHUDButtonKind[] _hudButtonKindArray =
    {
        OOTechRoadHUDButtonKind.Inventory,
        OOTechRoadHUDButtonKind.Codex,
        OOTechRoadHUDButtonKind.Mission,
        OOTechRoadHUDButtonKind.Cooking,
        OOTechRoadHUDButtonKind.WorldMap
    };

    private readonly string[] _fallbackTitleArray =
    {
        "인벤토리",
        "도감",
        "임무",
        "요리하기",
        "월드맵"
    };

    private readonly string[] _fallbackDescriptionArray =
    {
        "?덈줈 ?살? ?щ즺? 臾쇨굔???뺤씤?⑸땲??",
        "?덈줈 ?뚭쾶 ???뺣낫? 湲곕줉???뺤씤?⑸땲??",
        "?꾩옱 ?댁빞 ???쇱쓣 ?뺤씤?⑸땲??",
        "?щ즺瑜??ъ슜???뚯떇??留뚮벊?덈떎. 泥?踰덉㎏ 湲몄쓣 吏?섎㈃ ?대┰?덈떎.",
        "?꾩껜 ?대룞 寃쎈줈? ?ㅼ쓬 紐⑹쟻吏瑜??뺤씤?⑸땲??"
    };

    private GameObject Group_Dialogue;
    private DialogueUI UI_Dialogue;
    private GameObject Group_TutorialGuide;
    private OOTechTutorialGuideUI UI_TutorialGuide;

    public bool IsTutorialRunning { get; private set; }

    /// <summary>
    /// 洹몃９??爰쇱쭏 ???⑥? ??ъ갹怨?媛?대뱶李쎌쓣 ?뺣━?⑸땲??
    /// ?댁쟾 怨듭뿰???먭? ?⑥븘 ?ㅼ쓬 Road ?ъ떆?묒쓣 留됱? ?딄쾶 ?섎뒗 ?덉쟾?μ튂?낅땲??
    /// </summary>
    private void OnDisable()
    {
        IsTutorialRunning = false;
        CloseDialogueGroup();
        CloseTutorialGuideGroup();
    }

    /// <summary>
    /// RoadMap1??泥섏쓬 ?꾩갑?덉쓣 ???ъ씡援? 異섏뼇, 紐⑤? ??붿? ?붾━ 以鍮??덈궡瑜??ъ깮?⑸땲??
    /// </summary>
    public IEnumerator PlayRoadMap1ArrivalRoutine()
    {
        IsTutorialRunning = true;

        yield return ShowDialogueDataAndWait(_roadMap1MrJaeikDialogueId);
        yield return ShowDialogueDataAndWait(_roadMap1ChunyangDialogueId);
        yield return ShowDialogueDataAndWait(_roadMap1MoranDialogueId);
        CloseDialogueGroup();
        yield return OpenRoadMap1TutorialGuideAndWait();

        IsTutorialRunning = false;
    }

    /// <summary>
    /// 1st_Road_to_Stage1??吏꾩엯?섏옄留덉옄 HUD ?뚭컻, 珥덈컲 ??? ?щ즺 吏湲? ?꾨Т ?덈궡瑜??쒖꽌?濡?吏꾪뻾?⑸땲??
    /// </summary>
    public IEnumerator PlayOpeningTutorialRoutine(OOTechRoadHUDController hudController, string currentGroupName)
    {
        if (hudController == null || currentGroupName != _firstRoadGroupName)
        {
            if (hudController != null)
                hudController.SetCookingUnlocked(true);

            yield break;
        }

        IsTutorialRunning = true;
        hudController.SetCookingUnlocked(false);
        hudController.SetInventoryNewBadgeActive(false);
        hudController.SetCodexNewBadgeActive(false);
        hudController.SetMissionNewBadgeActive(false);

        yield return PlayHUDGuideRoutine(hudController);
        yield return PlayOpeningDialogueRoutine(hudController);
        RequestEnsureStarterIngredients(hudController);
        yield return OpenMissionTutorialGuideAndWait();

        hudController.RequestSetCookingQuestActive();
        IsTutorialRunning = false;
    }

    /// <summary>
    /// 泥?Road ?붾━ ?쒗넗由ъ뼹???꾩슂??湲곕낯 ?щ즺瑜?鍮좎쭚?놁씠 蹂댁젙?⑸땲??
    /// 鍮뚮뱶?먯꽌 ???肄붾（?댁씠 以묎컙???딄꺼??RoadMap1???꾩갑??諛곗슦媛 議곕━ ?뚰뭹???껋뼱踰꾨━吏 ?딄쾶 ?섎뒗 ?덉쟾?μ튂?낅땲??
    /// </summary>
    public void RequestEnsureStarterIngredients(OOTechRoadHUDController hudController)
    {
        if (hudController != null)
            hudController.RequestRefreshInventoryView();

        Debug.Log("[OOTechTutorial2Controller] Starter ingredient top-up is disabled. Inventory count is preserved.");
    }

    /// <summary>
    /// RoadGroup??爰쇱?嫄곕굹 ?ㅼ떆 ?쒖옉????吏꾪뻾 以묒씤 ?쒗넗由ъ뼹 UI瑜??뺣━?⑸땲??
    /// </summary>
    public void StopTutorial(OOTechRoadHUDController hudController)
    {
        IsTutorialRunning = false;

        if (hudController != null)
            hudController.CloseHUDGuide();

        CloseDialogueGroup();
        CloseTutorialGuideGroup();
    }

    /// <summary>
    /// ?몃깽?좊━, ?꾧컧, ?꾨Т, ?붾━?섍린, ?붾뱶留?踰꾪듉???쇱そ遺??李⑤?濡??ъ빱?깊빀?덈떎.
    /// </summary>
    private IEnumerator PlayHUDGuideRoutine(OOTechRoadHUDController hudController)
    {
        int guideCount = Mathf.Min(_hudButtonKindArray.Length, _hudTutorialIdArray.Length);

        for (int index = 0; index < guideCount; index++)
        {
            bool isDone = false;
            OO_Tutorial tutorialData = GetTutorialData(_hudTutorialIdArray[index]);
            string title = GetGuideTitle(index, tutorialData);
            string description = GetGuideDescription(index, tutorialData);

            hudController.ShowHUDGuideStep(_hudButtonKindArray[index], title, description, delegate
            {
                isDone = true;
            });

            yield return new WaitUntil(() => isDone);
        }
    }

    /// <summary>
    /// HUD ?뚭컻 ??罹먮┃????щ? 蹂댁뿬二쇨퀬 ?/梨꾩냼 ?щ즺瑜??몃깽?좊━??吏湲됲빀?덈떎.
    /// </summary>
    private IEnumerator PlayOpeningDialogueRoutine(OOTechRoadHUDController hudController)
    {
        yield return ShowDialogueDataAndWait(_chunyangRiceDialogueId);
        RequestGiveIngredient(hudController, _riceIngredientId, _riceIngredientCount);

        yield return ShowDialogueDataAndWait(_moranPumpkinDialogueId);
        RequestGiveIngredient(hudController, _vegetableIngredientId, _vegetableIngredientCount);

        yield return ShowDialogueDataAndWait(_mrJaeikReadyDialogueId);
        yield return ShowDialogueDataAndWait(_chunyangEastDialogueId);
        CloseDialogueGroup();
    }

    /// <summary>
    /// ???蹂댁긽?쇰줈 ?ㅼ뼱???щ즺瑜?紐⑤뜽??異붽??섍퀬 HUD??NEW 諛곗?瑜??꾩썎?덈떎.
    /// </summary>
    private void RequestGiveIngredient(OOTechRoadHUDController hudController, string ingredientId, int count)
    {
        if (!RequestGiveStarterIngredientOnce(ingredientId, count))
            return;

        if (hudController != null)
        {
            hudController.RequestRefreshInventoryView();
            hudController.SetInventoryNewBadgeActive(true);
        }
    }

    /// <summary>
    /// 異섏뼇/紐⑤? ???蹂댁긽?쇰줈 諛쏅뒗 ?쒖옉 ?щ즺瑜???踰덈쭔 吏湲됲빀?덈떎.
    /// ?뚮젅?댁뼱媛 ?붾━???щ즺瑜??뚮퉬?섎㈃ 洹?媛먯냼?됱쓣 洹몃?濡??좎??섍퀬, ?ㅼ쓬 遺???낆옣 ???먮룞 蹂듦뎄?섏? ?딆뒿?덈떎.
    /// </summary>
    private bool RequestGiveStarterIngredientOnce(string ingredientId, int count)
    {
        if (OOTechGameManager.Inst == null || string.IsNullOrEmpty(ingredientId) || count <= 0)
            return false;

        if (ingredientId == _riceIngredientId)
        {
            if (_isRiceStarterRewardGiven)
                return false;

            _isRiceStarterRewardGiven = true;
        }
        else if (ingredientId == _vegetableIngredientId)
        {
            if (_isVegetableStarterRewardGiven)
                return false;

            _isVegetableStarterRewardGiven = true;
        }

        int addCount = Mathf.Max(1, count);
        OOTechGameManager.Inst.AddItem(ingredientId, addCount);
        Debug.Log($"[OOTechTutorial2Controller] Starter reward given once: {ingredientId} x{addCount}");
        return true;
    }

    /// <summary>
    /// DialogueGroup???닿퀬 ?곗씠??ID???대떦?섎뒗 ??щ? ??以??ъ깮?⑸땲??
    /// </summary>
    private IEnumerator ShowDialogueDataAndWait(string dialogueId)
    {
        if (TryOpenDialogueGroup() == false || UI_Dialogue == null)
            yield break;

        bool isDone = false;
        OO_Dialogue dialogueData = GetDialogueData(dialogueId);
        UI_Dialogue.ShowDialogue(dialogueData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
    }

    /// <summary>
    /// ?꾨Т ?덈궡 TutorialGuideGroup???닿퀬 ?뺤씤 ?낅젰源뚯? 湲곕떎由쎈땲??
    /// </summary>
    private IEnumerator OpenMissionTutorialGuideAndWait()
    {
        if (TryOpenTutorialGuideGroup() == false || UI_TutorialGuide == null)
            yield break;

        bool isDone = false;
        OO_Tutorial tutorialData = GetTutorialData(_missionTutorialId);

        if (tutorialData == null)
            tutorialData = CreateFallbackMissionTutorialData();

        UI_TutorialGuide.ShowGuide(tutorialData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
        CloseTutorialGuideGroup();
    }

    /// <summary>
    /// RoadMap1 ?꾩갑 ???붾━?섍린 踰꾪듉???뚮윭蹂대씪???덈궡瑜??ъ깮?⑸땲??
    /// </summary>
    private IEnumerator OpenRoadMap1TutorialGuideAndWait()
    {
        if (TryOpenTutorialGuideGroup() == false || UI_TutorialGuide == null)
            yield break;

        bool isDone = false;
        OO_Tutorial tutorialData = GetTutorialData(_roadMap1TutorialId);

        if (tutorialData == null)
            tutorialData = CreateFallbackRoadMap1TutorialData();

        UI_TutorialGuide.ShowGuide(tutorialData, delegate
        {
            isDone = true;
        });

        yield return new WaitUntil(() => isDone);
        CloseTutorialGuideGroup();
    }

    /// <summary>
    /// ?ъ뿉 ?볦씤 DialogueGroup??李얠븘 耳쒓퀬 DialogueUI 而댄룷?뚰듃瑜??뺣낫?⑸땲??
    /// </summary>
    private bool TryOpenDialogueGroup()
    {
        if (Group_Dialogue == null)
            Group_Dialogue = RequestSceneObjectByName(_dialogueGroupName);

        if (Group_Dialogue == null)
            return false;

        Group_Dialogue.SetActive(true);

        if (UI_Dialogue == null)
            UI_Dialogue = Group_Dialogue.GetComponentInChildren<DialogueUI>(true);

        if (UI_Dialogue != null)
        {
            UI_Dialogue.RequestRoadViewLayout();
            return true;
        }

        CloseDialogueGroup();
        return false;
    }

    /// <summary>
    /// ?ъ뿉 ?볦씤 TutorialGuideGroup??李얠븘 耳쒓퀬 媛?대뱶 UI 而댄룷?뚰듃瑜??뺣낫?⑸땲??
    /// </summary>
    private bool TryOpenTutorialGuideGroup()
    {
        if (Group_TutorialGuide == null)
            Group_TutorialGuide = RequestSceneObjectByName(_tutorialGuideGroupName);

        if (Group_TutorialGuide == null)
            return false;

        Group_TutorialGuide.SetActive(true);

        if (UI_TutorialGuide == null)
            UI_TutorialGuide = Group_TutorialGuide.GetComponentInChildren<OOTechTutorialGuideUI>(true);

        if (UI_TutorialGuide != null)
            return true;

        CloseTutorialGuideGroup();
        return false;
    }

    /// <summary>
    /// ???臾대?瑜??レ븘 ?ㅼ쓬 ?곗텧?대굹 ?뚮젅???낅젰??媛由ъ? ?딄쾶 ?⑸땲??
    /// </summary>
    private void CloseDialogueGroup()
    {
        if (Group_Dialogue != null)
            Group_Dialogue.SetActive(false);
    }

    /// <summary>
    /// ?쒗넗由ъ뼹 媛?대뱶 臾대?瑜??レ븘 ?ㅼ쓬 ?곗텧?대굹 ?뚮젅???낅젰??媛由ъ? ?딄쾶 ?⑸땲??
    /// </summary>
    private void CloseTutorialGuideGroup()
    {
        if (Group_TutorialGuide != null)
            Group_TutorialGuide.SetActive(false);
    }

    /// <summary>
    /// OO_Dialogue.json?먯꽌 ????곗씠?곕? 媛?몄삤怨? ?놁쑝硫??꾩떆 ??щ? 留뚮뱾??吏꾪뻾???딄린吏 ?딄쾶 ?⑸땲??
    /// </summary>
    private OO_Dialogue GetDialogueData(string dialogueId)
    {
        if (OOTechGameDataManager.Inst != null)
            return OOTechGameDataManager.Inst.GetDialogueData(dialogueId) ?? CreateFallbackDialogueData(dialogueId);

        return CreateFallbackDialogueData(dialogueId);
    }

    /// <summary>
    /// OO_Tutorial.json?먯꽌 媛?대뱶 ?곗씠?곕? 媛?몄샃?덈떎.
    /// </summary>
    private OO_Tutorial GetTutorialData(string tutorialId)
    {
        if (OOTechGameDataManager.Inst != null)
            return OOTechGameDataManager.Inst.GetTutorialData(tutorialId);

        return null;
    }

    /// <summary>
    /// ?곗씠?곌? 鍮꾩뼱 ?덉쓣 ?뚮룄 媛?HUD 踰꾪듉???대쫫???먯뿰?ㅻ읇寃?蹂댁씠?꾨줉 ?덈퉬 ?쒕ぉ???쒓났?⑸땲??
    /// </summary>
    private string GetGuideTitle(int index, OO_Tutorial tutorialData)
    {
        if (tutorialData != null && string.IsNullOrEmpty(tutorialData.Title) == false)
            return tutorialData.Title;

        if (index >= 0 && index < _fallbackTitleArray.Length)
            return _fallbackTitleArray[index];

        return "?덈궡";
    }

    /// <summary>
    /// ?곗씠?곌? 鍮꾩뼱 ?덉쓣 ?뚮룄 ?쒗넗由ъ뼹??硫덉텛吏 ?딅룄濡??덈퉬 ?ㅻ챸???쒓났?⑸땲??
    /// </summary>
    private string GetGuideDescription(int index, OO_Tutorial tutorialData)
    {
        string description = string.Empty;

        if (tutorialData != null && string.IsNullOrEmpty(tutorialData.Description) == false)
            description = tutorialData.Description;

        if (string.IsNullOrEmpty(description) && index >= 0 && index < _fallbackDescriptionArray.Length)
            description = _fallbackDescriptionArray[index];

        if (index >= 0 && index < _hudButtonKindArray.Length && _hudButtonKindArray[index] == OOTechRoadHUDButtonKind.Cooking)
            description = $"{description}\n\n?꾩쭅 ?좉꺼 ?덉뒿?덈떎. RoadMap1???꾩갑?섎㈃ ?붾━?섍린媛 ?닿툑?⑸땲??";

        return description;
    }

    /// <summary>
    /// ????곗씠???꾨씫 ???뚮젅?닿? 硫덉텛吏 ?딅룄濡?理쒖냼 ??щ? 留뚮벊?덈떎.
    /// </summary>
    private OO_Dialogue CreateFallbackDialogueData(string dialogueId)
    {
        switch (dialogueId)
        {
            case "character_Chunyang_03":
                return CreateDialogueData(dialogueId, "異섏뼇", "?닿? 媛?몄삩 ??대꽕.");
            case "character_Moran_03":
                return CreateDialogueData(dialogueId, "紐⑤?", "梨꾩냼??梨숆꺼 ?먯뿀?댁슂.");
            case "character_Mr.Jaeik_03":
                return CreateDialogueData(dialogueId, "재익군", "이제 길을 나서면 되겠군.");
            case "character_Chunyang_04":
                return CreateDialogueData(dialogueId, "異섏뼇", "癒쇱????숈そ?쇰줈 媛?쒖???");
            case "character_Mr.Jaeik_04":
                return CreateDialogueData(dialogueId, "재익군", "길이 이어지는군.");
            case "character_Chunyang_05":
                return CreateDialogueData(dialogueId, "異섏뼇", "?붾━瑜?以鍮꾪빐?쇨쿋??");
            case "character_Moran_04":
                return CreateDialogueData(dialogueId, "紐⑤?", "?곕쑜???뚯떇???꾩슂?댁슂.");
            default:
                return CreateDialogueData(dialogueId, "?섎젅?댁뀡", dialogueId);
        }
    }

    /// <summary>
    /// DialogueUI媛 ?붽뎄?섎뒗 理쒖냼 ?꾨뱶瑜?梨꾩썙 ?꾩떆 ????곗씠?곕? 留뚮벊?덈떎.
    /// </summary>
    private OO_Dialogue CreateDialogueData(string dialogueId, string speakerName, string text)
    {
        OO_Dialogue dialogueData = new OO_Dialogue();
        dialogueData.Id = dialogueId;
        dialogueData.SpeakerName = speakerName;
        dialogueData.Text = text;
        return dialogueData;
    }

    /// <summary>
    /// ?꾨Т ?덈궡 ?곗씠?곌? ?놁쓣 ??蹂댁뿬以??덈퉬 Tutorial ?곗씠?곕? 留뚮벊?덈떎.
    /// </summary>
    private OO_Tutorial CreateFallbackMissionTutorialData()
    {
        OO_Tutorial tutorialData = new OO_Tutorial();
        tutorialData.Id = _missionTutorialId;
        tutorialData.Title = "?꾨Т";
        tutorialData.Description = "諛곌퀬??紐⑤?怨??숇즺?ㅼ쓣 ?꾪빐 ?붾━?섏꽭??";
        return tutorialData;
    }

    /// <summary>
    /// RoadMap1 ?꾩갑 ?덈궡 ?곗씠?곌? ?놁쓣 ??蹂댁뿬以??덈퉬 Tutorial ?곗씠?곕? 留뚮벊?덈떎.
    /// </summary>
    private OO_Tutorial CreateFallbackRoadMap1TutorialData()
    {
        OO_Tutorial tutorialData = new OO_Tutorial();
        tutorialData.Id = _roadMap1TutorialId;
        tutorialData.Title = "?붾━?섍린";
        tutorialData.Description = "?붾━?섍린 踰꾪듉???뚮윭 遺?뚯쑝濡??대룞?섏꽭??";
        return tutorialData;
    }

    /// <summary>
    /// 鍮꾪솢?깊솕?????ㅻ툕?앺듃源뚯? ?ы븿???대쫫?쇰줈 臾대? ?ㅻ툕?앺듃瑜?李얠뒿?덈떎.
    /// </summary>
    private GameObject RequestSceneObjectByName(string objectName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjectArray = activeScene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjectArray)
        {
            if (rootObject.name == objectName)
                return rootObject;

            GameObject childObject = RequestChildObjectByName(rootObject.transform, objectName);

            if (childObject != null)
                return childObject;
        }

        return null;
    }

    /// <summary>
    /// ?먯떇 臾대? ?덉そ源뚯? ?ш??곸쑝濡??대젮媛 ?대쫫??媛숈? ?ㅻ툕?앺듃瑜?李얠뒿?덈떎.
    /// </summary>
    private GameObject RequestChildObjectByName(Transform rootTransform, string objectName)
    {
        foreach (Transform childTransform in rootTransform)
        {
            if (childTransform.name == objectName)
                return childTransform.gameObject;

            GameObject resultObject = RequestChildObjectByName(childTransform, objectName);

            if (resultObject != null)
                return resultObject;
        }

        return null;
    }
}

