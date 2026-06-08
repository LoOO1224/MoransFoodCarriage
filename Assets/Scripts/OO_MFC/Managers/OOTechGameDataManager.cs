// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechGameDataManager.cs
// - ??븷: ?щ윭 ?λ㈃?먯꽌 ?④퍡 ?곕뒗 怨듯넻 Manager?낅땲??
// - 媛먮룆 愿?? 媛?遺?쒖뿉 怨듯넻 李쎄뎄瑜??댁뼱 二쇰뒗 ?쒖옉 蹂몃??낅땲??
// - ?좎?蹂댁닔 ?ъ씤?? ?뱀젙 ?λ㈃???몃? ?곗텧??吏곸젒 泥섎━?섏? 留먭퀬, 怨듯넻 議고쉶/?깅줉/?붿껌 API留??좎??⑸땲??
// =============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OO_MFC?먯꽌 ?ъ슜?섎뒗 Static Data Json??濡쒕뱶?섍퀬 議고쉶?섎뒗 留ㅻ땲??낅땲??
/// Json ?뚯씪? Resources/OO_MFC/Data ?꾨옒???먮ŉ, ?고???吏꾪뻾 ?곗씠?곕뒗 GameManager??Model?먯꽌 ?곕줈 愿由ы빀?덈떎.
/// </summary>
public class OOTechGameDataManager : MonoBehaviour
{
    // ?쎈뒗 ?쒖꽌:
    // 1. Awake/RequestLoadAllData 怨꾩뿴: Resources/JsonOutput???덈뒗 JSON ?뚯씪???쎌뼱 Dictionary???깅줉?⑸땲??
    // 2. LoadOO_XXX 怨꾩뿴: 媛??묒? ?뚯씠釉붿뿉???섏삩 JSON???대떦 Data ?대옒?ㅻ줈 蹂?섑빀?덈떎.
    // 3. CreateXXXData 怨꾩뿴: 臾몄옄?대줈 ?ㅼ뼱??JSON 媛믪쓣 int, List<string> 媛숈? Unity ?먮즺?뺤쑝濡??뺣━?⑸땲??
    // 4. GetXXXData 怨꾩뿴: Controller? UI媛 ID濡?static data瑜??덉쟾?섍쾶 議고쉶?⑸땲??
    // 5. NormalizeJsonText/List 怨꾩뿴: ?묒? 鍮덉뭏, null, 援щ텇??臾몄옄?댁쓣 寃뚯엫?먯꽌 ?곌린 醫뗭? 媛믪쑝濡?諛붽퓠?덈떎.
    // ?좎?蹂댁닔 二쇱쓽:
    // - ???묒? ?뚯씪??異붽??섎㈃ Data ?대옒?? JsonData ?대옒?? Load 硫붿꽌?? Get 硫붿꽌?쒕? ?④퍡 異붽??⑸땲??
    // - ?뚮젅??以?諛붾뚮뒗 媛믪? ?ш린 ?ｌ? 留먭퀬 Model/GameManager 履쎌쑝濡?蹂대깄?덈떎.
    // - ?곗씠??ID媛 ?由щ㈃ ?붾㈃??鍮꾧굅???붿옄媛 ?섎せ ?섏삤誘濡? 寃쎄퀬 濡쒓렇瑜?吏?곗? 留먭퀬 ?먯씤??異붿쟻?⑸땲??

    public static OOTechGameDataManager Inst { get; private set; }

    // ==================== ?곗씠??Dictionary ====================
    private readonly Dictionary<string, OO_Narration> _narrationDic = new Dictionary<string, OO_Narration>();
    private readonly Dictionary<string, OO_Character> _characterDic = new Dictionary<string, OO_Character>();
    private readonly Dictionary<string, OO_Dialogue> _dialogueDic = new Dictionary<string, OO_Dialogue>();
    private readonly Dictionary<string, OO_DialogueGroup> _dialogueGroupDic = new Dictionary<string, OO_DialogueGroup>();
    private readonly Dictionary<string, OO_Choice> _choiceDic = new Dictionary<string, OO_Choice>();
    private readonly Dictionary<string, OO_Codex> _codexDic = new Dictionary<string, OO_Codex>();
    private readonly Dictionary<string, OO_Tutorial> _tutorialDic = new Dictionary<string, OO_Tutorial>();
    private readonly Dictionary<string, OO_Ingredient> _ingredientDic = new Dictionary<string, OO_Ingredient>();
    private readonly Dictionary<string, OO_Recipe> _recipeDic = new Dictionary<string, OO_Recipe>();
    private readonly Dictionary<string, OO_CookingTool> _cookingToolDic = new Dictionary<string, OO_CookingTool>();
    private readonly Dictionary<string, OO_CookingCueSheet> _cookingCueSheetDic = new Dictionary<string, OO_CookingCueSheet>();
    private readonly Dictionary<string, OO_Cook> _cookDic = new Dictionary<string, OO_Cook>();
    private readonly Dictionary<string, OO_Stage> _stageDic = new Dictionary<string, OO_Stage>();
    private readonly Dictionary<string, OO_StageQuest> _stageQuestDic = new Dictionary<string, OO_StageQuest>();
    private readonly Dictionary<string, OO_Stage2CueSheet> _stage2CueSheetDic = new Dictionary<string, OO_Stage2CueSheet>();
    private readonly Dictionary<string, OO_Stage3CueSheet> _stage3CueSheetDic = new Dictionary<string, OO_Stage3CueSheet>();
    private readonly Dictionary<string, OO_SpeechBubble> _speechBubbleDic = new Dictionary<string, OO_SpeechBubble>();
    private readonly Dictionary<string, OO_Stage4CueSheet> _stage4CueSheetDic = new Dictionary<string, OO_Stage4CueSheet>();
    private readonly Dictionary<string, OO_FinalCueSheet> _finalCueSheetDic = new Dictionary<string, OO_FinalCueSheet>();

    /// <summary>
    /// 以묐났 留ㅻ땲?瑜??뺣━?섍퀬 Static Data瑜?濡쒕뱶?⑸땲??
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

        LoadAllData();
    }

    /// <summary>
    /// 留ㅻ땲?媛 ?뚭눼?????꾩뿭 李몄“瑜?鍮꾩썎?덈떎.
    /// </summary>
    private void OnDestroy()
    {
        if (Inst == this)
            Inst = null;
    }

    // ==================== ?곗씠??濡쒕뱶 ====================

    /// <summary>
    /// 寃뚯엫 ?쒖옉???꾩슂??紐⑤뱺 Static Data瑜?濡쒕뱶?⑸땲??
    /// Character??Dialogue???붿옄 ?대쫫 蹂댁젙???곗씪 ???덉뼱 Dialogue蹂대떎 癒쇱? 濡쒕뱶?⑸땲??
    /// </summary>
    public void LoadAllData()
    {
        Debug.Log("[OOTechGameDataManager] 모든 데이터 로드 시작");

        LoadNarrationData();
        LoadCharacterData();
        LoadDialogueData();
        LoadDialogueGroupData();
        LoadChoiceData();
        LoadCodexData();
        LoadTutorialData();
        LoadIngredientData();
        LoadRecipeData();
        LoadCookingToolData();
        LoadCookingCueSheetData();
        LoadCookData();
        LoadStageData();
        LoadStageQuestData();
        LoadStage2CueSheetData();
        LoadStage3CueSheetData();
        LoadSpeechBubbleData();
        LoadStage4CueSheetData();
        LoadFinalCueSheetData();

        Debug.Log("[OOTechGameDataManager] 모든 데이터 로드 완료");
    }

    /// <summary>
    /// OO_Narration.json???쎌뼱 ?꾨·濡쒓렇/?섎젅?댁뀡 ?蹂몄쑝濡??깅줉?⑸땲??
    /// </summary>
    private void LoadNarrationData()
    {
        _narrationDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_Narration"))
        {
            OOTechNarrationJsonWrapper wrapper = JsonUtility.FromJson<OOTechNarrationJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechNarrationJsonData jsonData in wrapper.Items)
            {
                OO_Narration narrationData = CreateNarrationData(jsonData);

                if (narrationData == null || string.IsNullOrEmpty(narrationData.Id))
                    continue;

                _narrationDic[narrationData.Id] = narrationData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] Narration data loaded: {_narrationDic.Count}");
    }

    /// <summary>
    /// OO_Character.json???쎌뼱 ?붿옄 ?대쫫怨?罹먮┃???뺣낫瑜??깅줉?⑸땲??
    /// </summary>
    private void LoadCharacterData()
    {
        _characterDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_Character"))
        {
            OOTechCharacterJsonWrapper wrapper = JsonUtility.FromJson<OOTechCharacterJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OO_Character characterData in wrapper.Items)
            {
                if (characterData == null || string.IsNullOrEmpty(characterData.Id))
                    continue;

                characterData.Name = NormalizeJsonText(characterData.Name);
                characterData.Description = NormalizeJsonText(characterData.Description);
                _characterDic[characterData.Id] = characterData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] Character data loaded: {_characterDic.Count}");
    }

    /// <summary>
    /// OO_Dialogue.json???쎌뼱 罹먮┃????щ? ?깅줉?⑸땲??
    /// </summary>
    private void LoadDialogueData()
    {
        _dialogueDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_Dialogue"))
        {
            OOTechDialogueJsonWrapper wrapper = JsonUtility.FromJson<OOTechDialogueJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechDialogueJsonData jsonData in wrapper.Items)
            {
                OO_Dialogue dialogueData = CreateDialogueData(jsonData);

                if (dialogueData == null || string.IsNullOrEmpty(dialogueData.Id))
                    continue;

                _dialogueDic[dialogueData.Id] = dialogueData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] Dialogue data loaded: {_dialogueDic.Count}");
    }

    /// <summary>
    /// OO_DialogueGroup.json???쎌뼱 ?щ윭 ?몃Ъ???숈떆??留먰븯?????臾띠쓬???깅줉?⑸땲??
    /// </summary>
    private void LoadDialogueGroupData()
    {
        _dialogueGroupDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_DialogueGroup"))
        {
            OOTechDialogueGroupJsonWrapper wrapper = JsonUtility.FromJson<OOTechDialogueGroupJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechDialogueGroupJsonData jsonData in wrapper.Items)
            {
                OO_DialogueGroup dialogueGroupData = CreateDialogueGroupData(jsonData);

                if (dialogueGroupData == null || string.IsNullOrEmpty(dialogueGroupData.Id))
                    continue;

                _dialogueGroupDic[dialogueGroupData.Id] = dialogueGroupData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] DialogueGroup data loaded: {_dialogueGroupDic.Count}");
    }

    /// <summary>
    /// OO_Tutorial.json???쎌뼱 HUD/媛?대뱶 ?ㅻ챸 ?곗씠?곕? ?깅줉?⑸땲??
    /// </summary>
    /// <summary>
    /// OO_Choice.json???쎌뼱 ?좏깮吏媛 遺숈? ?곹샇?묒슜 ?먯떆?몃? ?깅줉?⑸땲??
    /// ?곹솕 鍮꾩쑀濡쒕뒗 ?쇰컲 ?蹂??놁뿉 "愿媛??좏깮 遺꾧린??瑜??곕줈 苑귥븘 ?먮뒗 ?④퀎?낅땲??
    /// </summary>
    private void LoadChoiceData()
    {
        _choiceDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_Choice"))
        {
            OOTechChoiceJsonWrapper wrapper = JsonUtility.FromJson<OOTechChoiceJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechChoiceJsonData jsonData in wrapper.Items)
            {
                OO_Choice choiceData = CreateChoiceData(jsonData);

                if (choiceData == null || string.IsNullOrEmpty(choiceData.Id))
                    continue;

                _choiceDic[choiceData.Id] = choiceData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] Choice data loaded: {_choiceDic.Count}");
    }

    /// <summary>
    /// OO_Codex.json???쎌뼱 ?꾧컧 ??ぉ???깅줉?⑸땲??
    /// ?곹솕濡?移섎㈃ ?섏쨷??愿媛앹씠 ?ㅼ떆 ?쇱퀜蹂??꾨줈洹몃옩遺곸쓽 ??ぉ?ㅼ쓣 誘몃━ ?뺣━?섎뒗 ?④퀎?낅땲??
    /// </summary>
    private void LoadCodexData()
    {
        _codexDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_Codex"))
        {
            OOTechCodexJsonWrapper wrapper = JsonUtility.FromJson<OOTechCodexJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OO_Codex codexData in wrapper.Items)
            {
                OO_Codex createdData = CreateCodexData(codexData);

                if (createdData == null || string.IsNullOrEmpty(createdData.Id))
                    continue;

                _codexDic[createdData.Id] = createdData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] Codex data loaded: {_codexDic.Count}");
    }

    private void LoadTutorialData()
    {
        _tutorialDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_Tutorial"))
        {
            OOTechTutorialJsonWrapper wrapper = JsonUtility.FromJson<OOTechTutorialJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechTutorialJsonData jsonData in wrapper.Items)
            {
                OO_Tutorial tutorialData = CreateTutorialData(jsonData);

                if (tutorialData == null || string.IsNullOrEmpty(tutorialData.Id))
                    continue;

                _tutorialDic[tutorialData.Id] = tutorialData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] Tutorial data loaded: {_tutorialDic.Count}");
    }

    /// <summary>
    /// OO_Ingredient.json???쎌뼱 ?, 梨꾩냼 媛숈? ?щ즺 ?곗씠?곕? ?깅줉?⑸땲??
    /// </summary>
    private void LoadIngredientData()
    {
        _ingredientDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_Ingredient"))
        {
            OOTechIngredientJsonWrapper wrapper = JsonUtility.FromJson<OOTechIngredientJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechIngredientJsonData jsonData in wrapper.Items)
            {
                OO_Ingredient ingredientData = CreateIngredientData(jsonData);

                if (ingredientData == null || string.IsNullOrEmpty(ingredientData.Id))
                    continue;

                _ingredientDic[ingredientData.Id] = ingredientData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] Ingredient data loaded: {_ingredientDic.Count}");
    }

    // ==================== ?곗씠???앹꽦 ====================

    /// <summary>
    /// OO_Recipe.json???쎌뼱 ?щ즺 議고빀怨?寃곌낵 ?뚯떇 洹쒖튃???깅줉?⑸땲??
    /// </summary>
    private void LoadRecipeData()
    {
        _recipeDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_Recipe"))
        {
            OOTechRecipeJsonWrapper wrapper = JsonUtility.FromJson<OOTechRecipeJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechRecipeJsonData jsonData in wrapper.Items)
            {
                OO_Recipe recipeData = CreateRecipeData(jsonData);

                if (recipeData == null || string.IsNullOrEmpty(recipeData.Id))
                    continue;

                _recipeDic[recipeData.Id] = recipeData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] Recipe data loaded: {_recipeDic.Count}");
    }

    /// <summary>
    /// OO_CookingTool.json???쎌뼱 媛留덉넡, ?꾨쭏 媛숈? 議곕━?꾧뎄 ??븷?쒕? ?깅줉?⑸땲??
    /// ?곹솕 鍮꾩쑀濡쒕뒗 ?뚰뭹???留뚮뱺 ?ㅼ젣 議곕━?꾧뎄留덈떎 "諛쏆쓣 ???덈뒗 ?щ즺" ?쒕? 遺숈씠???④퀎?낅땲??
    /// </summary>
    private void LoadCookingToolData()
    {
        _cookingToolDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_CookingTool"))
        {
            OOTechCookingToolJsonWrapper wrapper = JsonUtility.FromJson<OOTechCookingToolJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechCookingToolJsonData jsonData in wrapper.Items)
            {
                OO_CookingTool cookingToolData = CreateCookingToolData(jsonData);

                if (cookingToolData == null || string.IsNullOrEmpty(cookingToolData.Id))
                    continue;

                _cookingToolDic[cookingToolData.Id] = cookingToolData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] CookingTool data loaded: {_cookingToolDic.Count}");
    }

    /// <summary>
    /// OO_CookingCueSheet.json???쎌뼱 CookingGroup??怨듯넻 ?곗텧 ?먮? ?깅줉?⑸땲??
    /// 媛먮룆 鍮꾩쑀濡쒕뒗 遺???λ㈃??移대찓?? 議곕챸, ?덈궡 ?붿궡????대컢?쒕? ?쒖옉 蹂몃???苑귥븘 ?먮뒗 ?④퀎?낅땲??
    /// </summary>
    private void LoadCookingCueSheetData()
    {
        _cookingCueSheetDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_CookingCueSheet"))
        {
            OOTechCookingCueSheetJsonWrapper wrapper = JsonUtility.FromJson<OOTechCookingCueSheetJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechCookingCueSheetJsonData jsonData in wrapper.Items)
            {
                OO_CookingCueSheet cueSheetData = CreateCookingCueSheetData(jsonData);

                if (cueSheetData == null || string.IsNullOrEmpty(cueSheetData.Id))
                    continue;

                _cookingCueSheetDic[cueSheetData.Id] = cueSheetData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] CookingCueSheet data loaded: {_cookingCueSheetDic.Count}");
    }

    /// <summary>
    /// OO_Cook.json???쎌뼱 ?꾩꽦 ?뚯떇 ?곗씠?곕? ?깅줉?⑸땲??
    /// </summary>
    private void LoadCookData()
    {
        _cookDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_Cook"))
        {
            OOTechCookJsonWrapper wrapper = JsonUtility.FromJson<OOTechCookJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechCookJsonData jsonData in wrapper.Items)
            {
                OO_Cook cookData = CreateCookData(jsonData);

                if (cookData == null || string.IsNullOrEmpty(cookData.Id))
                    continue;

                _cookDic[cookData.Id] = cookData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] Cook data loaded: {_cookDic.Count}");
    }

    /// <summary>
    /// OO_Stage.json???쎌뼱 ?ㅽ뀒?댁? ?대쫫, ?ㅻ챸, ?대룞 ?뺣낫瑜??깅줉?⑸땲??
    /// </summary>
    private void LoadStageData()
    {
        _stageDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_Stage"))
        {
            OOTechStageJsonWrapper wrapper = JsonUtility.FromJson<OOTechStageJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechStageJsonData jsonData in wrapper.Items)
            {
                OO_Stage stageData = CreateStageData(jsonData);

                if (stageData == null || string.IsNullOrEmpty(stageData.Id))
                    continue;

                _stageDic[stageData.Id] = stageData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] Stage data loaded: {_stageDic.Count}");
    }

    /// <summary>
    /// OO_StageQuest.json???쎌뼱 StageGroup HUD???쒖떆???꾨Т ?곗씠?곕? ?깅줉?⑸땲??
    /// </summary>
    private void LoadStageQuestData()
    {
        _stageQuestDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_StageQuest"))
        {
            OOTechStageQuestJsonWrapper wrapper = JsonUtility.FromJson<OOTechStageQuestJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechStageQuestJsonData jsonData in wrapper.Items)
            {
                OO_StageQuest stageQuestData = CreateStageQuestData(jsonData);

                if (stageQuestData == null || string.IsNullOrEmpty(stageQuestData.Id))
                    continue;

                _stageQuestDic[stageQuestData.Id] = stageQuestData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] StageQuest data loaded: {_stageQuestDic.Count}");
    }

    /// <summary>
    /// OO_Stage2CueSheet.json???쎌뼱 Stage2Group???곗텧 ?먯떆?몃? ?깅줉?⑸땲??
    /// </summary>
    private void LoadStage2CueSheetData()
    {
        _stage2CueSheetDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_Stage2CueSheet"))
        {
            OOTechStage2CueSheetJsonWrapper wrapper = JsonUtility.FromJson<OOTechStage2CueSheetJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechStage2CueSheetJsonData jsonData in wrapper.Items)
            {
                OO_Stage2CueSheet cueSheetData = CreateStage2CueSheetData(jsonData);

                if (cueSheetData == null || string.IsNullOrEmpty(cueSheetData.Id))
                    continue;

                _stage2CueSheetDic[cueSheetData.Id] = cueSheetData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] Stage2CueSheet data loaded: {_stage2CueSheetDic.Count}");
    }

    /// <summary>
    /// OO_Stage3CueSheet.json???쎌뼱 Stage3Group/EncounterGroup???곗텧 ?먯떆?몃? ?깅줉?⑸땲??
    /// </summary>
    private void LoadStage3CueSheetData()
    {
        _stage3CueSheetDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_Stage3CueSheet"))
        {
            OOTechStage3CueSheetJsonWrapper wrapper = JsonUtility.FromJson<OOTechStage3CueSheetJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechStage3CueSheetJsonData jsonData in wrapper.Items)
            {
                OO_Stage3CueSheet cueSheetData = CreateStage3CueSheetData(jsonData);

                if (cueSheetData == null || string.IsNullOrEmpty(cueSheetData.Id))
                    continue;

                _stage3CueSheetDic[cueSheetData.Id] = cueSheetData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] Stage3CueSheet data loaded: {_stage3CueSheetDic.Count}");
    }

    /// <summary>
    /// OO_SpeechBubble.json???쎌뼱 Rabbit 媛숈? 諛곗슦??癒몃━ ??留먰뭾???蹂몄쑝濡??깅줉?⑸땲??
    /// 媛먮룆 鍮꾩쑀濡쒕뒗 ???????⑤꼸???꾨땲?? 諛곗슦媛 ?吏곸씠硫?以묒뼹嫄곕━??吏㏃? 履쎌?瑜?紐⑥쑝???④퀎?낅땲??
    /// </summary>
    private void LoadSpeechBubbleData()
    {
        _speechBubbleDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_SpeechBubble"))
        {
            OOTechSpeechBubbleJsonWrapper wrapper = JsonUtility.FromJson<OOTechSpeechBubbleJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechSpeechBubbleJsonData jsonData in wrapper.Items)
            {
                OO_SpeechBubble speechBubbleData = CreateSpeechBubbleData(jsonData);

                if (speechBubbleData == null || string.IsNullOrEmpty(speechBubbleData.Id))
                    continue;

                _speechBubbleDic[speechBubbleData.Id] = speechBubbleData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] SpeechBubble data loaded: {_speechBubbleDic.Count}");
    }

    /// <summary>
    /// OO_Stage4CueSheet.json???쎌뼱 Stage4 ?좊겮/嫄곕턿???먯떆?몃줈 ?깅줉?⑸땲??
    /// </summary>
    private void LoadStage4CueSheetData()
    {
        _stage4CueSheetDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_Stage4CueSheet"))
        {
            OOTechStage4CueSheetJsonWrapper wrapper = JsonUtility.FromJson<OOTechStage4CueSheetJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechStage4CueSheetJsonData jsonData in wrapper.Items)
            {
                OO_Stage4CueSheet cueSheetData = CreateStage4CueSheetData(jsonData);

                if (cueSheetData == null || string.IsNullOrEmpty(cueSheetData.Id))
                    continue;

                _stage4CueSheetDic[cueSheetData.Id] = cueSheetData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] Stage4CueSheet data loaded: {_stage4CueSheetDic.Count}");
    }

    /// <summary>
    /// OO_FinalCueSheet.json???쎌뼱 理쒖쥌 而룹떊/?붾뵫 ?먯떆?몃줈 ?깅줉?⑸땲??
    /// </summary>
    private void LoadFinalCueSheetData()
    {
        _finalCueSheetDic.Clear();

        foreach (TextAsset jsonFile in LoadDataTextAssetArray("OO_FinalCueSheet"))
        {
            OOTechFinalCueSheetJsonWrapper wrapper = JsonUtility.FromJson<OOTechFinalCueSheetJsonWrapper>(WrapJsonArray(jsonFile.text));

            if (wrapper == null || wrapper.Items == null)
                continue;

            foreach (OOTechFinalCueSheetJsonData jsonData in wrapper.Items)
            {
                OO_FinalCueSheet cueSheetData = CreateFinalCueSheetData(jsonData);

                if (cueSheetData == null || string.IsNullOrEmpty(cueSheetData.Id))
                    continue;

                _finalCueSheetDic[cueSheetData.Id] = cueSheetData;
            }
        }

        Debug.Log($"[OOTechGameDataManager] FinalCueSheet data loaded: {_finalCueSheetDic.Count}");
    }

    /// <summary>
    /// JSON ??以꾩쓣 OO_Narration 紐⑤뜽濡?蹂?섑빀?덈떎.
    /// </summary>
    private OO_Narration CreateNarrationData(OOTechNarrationJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        OO_Narration narrationData = new OO_Narration
        {
            Id = NormalizeJsonText(jsonData.Id),
            Title = NormalizeJsonText(jsonData.Title),
            PartNumber = ParseInt(jsonData.PartNumber),
            NarrationTexts = CreateTextList(jsonData.NarrationTexts),
            BackgroundImagePaths = CreateBackgroundImagePathList(jsonData),
            BGMPath = NormalizeJsonText(jsonData.BGMPath),
            NextGroup = NormalizeJsonText(jsonData.NextGroup)
        };

        return narrationData;
    }

    /// <summary>
    /// JSON ??以꾩쓣 OO_Dialogue 紐⑤뜽濡?蹂?섑븯怨??붿옄 ?대쫫??蹂댁젙?⑸땲??
    /// </summary>
    private OO_Dialogue CreateDialogueData(OOTechDialogueJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        OO_Dialogue dialogueData = new OO_Dialogue
        {
            Id = NormalizeJsonText(jsonData.Id),
            SpeakerName = ResolveDialogueSpeakerName(jsonData),
            Text = NormalizeJsonText(GetFirstNotEmpty(jsonData.Text, jsonData.Description)),
            NextDialogueId = NormalizeJsonText(jsonData.NextDialogueId),
            SelectionNameList = CreateStringList(jsonData.SelectionNameList),
            SelectionDialogueIdList = CreateStringList(jsonData.SelectionDialogueIdList),
            TexturePath = NormalizeJsonText(jsonData.TexturePath),
            VoicePath = NormalizeJsonText(jsonData.VoicePath)
        };

        return dialogueData;
    }

    /// <summary>
    /// JSON ??以꾩쓣 ?щ윭 ?붿옄 ???臾띠쓬 紐⑤뜽濡?蹂?섑빀?덈떎.
    /// </summary>
    private OO_DialogueGroup CreateDialogueGroupData(OOTechDialogueGroupJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        OO_DialogueGroup dialogueGroupData = new OO_DialogueGroup
        {
            Id = NormalizeJsonText(jsonData.Id),
            Name = NormalizeJsonText(jsonData.Name),
            Description = NormalizeJsonText(jsonData.Description),
            SpeakerCharacterIdList = CreateStringList(jsonData.SpeakerCharacterIdList),
            SpeakerNameList = CreateStringList(jsonData.SpeakerNameList),
            Text = NormalizeJsonText(GetFirstNotEmpty(jsonData.Text, jsonData.Description)),
            DialogueIdList = CreateStringList(jsonData.DialogueIdList),
            NextDialogueGroupId = NormalizeJsonText(jsonData.NextDialogueGroupId)
        };

        return dialogueGroupData;
    }

    /// <summary>
    /// JSON ??以꾩쓣 ?쒗넗由ъ뼹 媛?대뱶 紐⑤뜽濡?蹂?섑빀?덈떎.
    /// </summary>
    /// <summary>
    /// JSON ??以꾩쓣 OO_Choice 紐⑤뜽濡?諛붽퓠?덈떎.
    /// 媛먮룆 鍮꾩쑀濡쒕뒗 ?묒? ?먯떆????以꾩쓣 ?ㅼ젣 臾대??먯꽌 ?ㅽ뻾???좏깮 遺꾧린 移대뱶濡???린??怨쇱젙?낅땲??
    /// </summary>
    private OO_Choice CreateChoiceData(OOTechChoiceJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        string speakerId = NormalizeJsonText(jsonData.SpeakerId);

        OO_Choice choiceData = new OO_Choice
        {
            Id = NormalizeJsonText(jsonData.Id),
            SpeakerId = speakerId,
            SpeakerName = ResolveCharacterName(speakerId, NormalizeJsonText(jsonData.SpeakerName)),
            PromptText = NormalizeJsonText(GetFirstNotEmpty(jsonData.PromptText, jsonData.Description)),
            ChoiceMode = NormalizeJsonText(jsonData.ChoiceMode),
            OptionCount = ParseInt(jsonData.OptionCount),
            OptionKeyList = CreateStringList(jsonData.OptionKeyList),
            OptionTextList = CreateStringList(jsonData.OptionTextList),
            ResultTypeList = CreateStringList(jsonData.ResultTypeList),
            ResultValueList = CreateStringList(jsonData.ResultValueList),
            ResultCountList = CreateIntList(jsonData.ResultCountList),
            NextDialogueIdList = CreateStringList(jsonData.NextDialogueIdList),
            NextChoiceIdList = CreateStringList(jsonData.NextChoiceIdList),
            StageQuestIdList = CreateStringList(jsonData.StageQuestIdList),
            Memo = NormalizeJsonText(jsonData.Memo)
        };

        return choiceData;
    }

    /// <summary>
    /// JSON ?됱쓣 ?꾧컧 移대뱶 ?곗씠?곕줈 ?뺣━?⑸땲??
    /// ?곹솕濡?移섎㈃ 罹먮┃???뚯떇/吏???뚭컻 移대뱶???쒕ぉ, ?ㅻ챸, ?ъ쭊 寃쎈줈瑜????μ쓽 ?먯뭅?쒕줈 留뚮뱶???④퀎?낅땲??
    /// </summary>
    private OO_Codex CreateCodexData(OO_Codex jsonData)
    {
        if (jsonData == null)
            return null;

        OO_Codex codexData = new OO_Codex
        {
            Id = NormalizeJsonText(jsonData.Id),
            Category = NormalizeJsonText(jsonData.Category),
            Title = NormalizeJsonText(jsonData.Title),
            Description = NormalizeJsonText(jsonData.Description),
            ImagePath = NormalizeJsonText(jsonData.ImagePath),
            UnlockCondition = NormalizeJsonText(jsonData.UnlockCondition)
        };

        return codexData;
    }

    private OO_Tutorial CreateTutorialData(OOTechTutorialJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        string title = GetFirstNotEmpty(jsonData.Title, jsonData.Name);

        OO_Tutorial tutorialData = new OO_Tutorial
        {
            Id = NormalizeJsonText(jsonData.Id),
            Name = NormalizeJsonText(jsonData.Name),
            Title = NormalizeJsonText(title),
            Description = NormalizeJsonText(jsonData.Description),
            TargetStageId = NormalizeJsonText(jsonData.TargetStageId),
            TriggerCondition = NormalizeJsonText(jsonData.TriggerCondition),
            DialogueGroupId = NormalizeJsonText(jsonData.DialogueGroupId),
            SkillList = NormalizeJsonText(jsonData.SkillList),
            UseWeaponId = NormalizeJsonText(jsonData.UseWeaponId),
            BasicCostumeId = NormalizeJsonText(jsonData.BasicCostumeId)
        };

        return tutorialData;
    }

    /// <summary>
    /// JSON ??以꾩쓣 ?щ즺 紐⑤뜽濡?蹂?섑빀?덈떎.
    /// </summary>
    private OO_Ingredient CreateIngredientData(OOTechIngredientJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        OO_Ingredient ingredientData = new OO_Ingredient
        {
            Id = NormalizeJsonText(jsonData.Id),
            Name = NormalizeJsonText(jsonData.Name),
            Description = NormalizeJsonText(jsonData.Description),
            IconPath = NormalizeJsonText(jsonData.IconPath),
            Grade = NormalizeJsonText(jsonData.Grade),
            MaxStackCount = Mathf.Max(1, ParseInt(jsonData.MaxStackCount))
        };

        return ingredientData;
    }

    /// <summary>
    /// JSON ??以꾩쓣 ?덉떆??紐⑤뜽濡?蹂?섑빀?덈떎.
    /// </summary>
    private OO_Recipe CreateRecipeData(OOTechRecipeJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        OO_Recipe recipeData = new OO_Recipe
        {
            Id = NormalizeJsonText(jsonData.Id),
            Name = NormalizeJsonText(jsonData.Name),
            Description = NormalizeJsonText(jsonData.Description),
            ResultItemId = NormalizeJsonText(jsonData.ResultItemId),
            RequiredIngredients = CreateStringList(jsonData.RequiredIngredients),
            MaxDuplicateCount = Mathf.Max(1, ParseInt(jsonData.MaxDuplicateCount)),
            RequiredTool = NormalizeJsonText(jsonData.RequiredTool),
            RequiredIngredientIds = CreateStringList(GetFirstNotEmpty(jsonData.RequiredIngredientIds, jsonData.RequiredIngredients)),
            RequiredIngredientCounts = CreateIntList(jsonData.RequiredIngredientCounts),
            RequiredToolIds = CreateStringList(jsonData.RequiredToolIds),
            ResultCount = Mathf.Max(1, ParseInt(GetFirstNotEmpty(jsonData.ResultCount, "1"))),
            QuantityGuideText = NormalizeJsonText(jsonData.QuantityGuideText)
        };

        if (recipeData.RequiredIngredients == null || recipeData.RequiredIngredients.Count == 0)
            recipeData.RequiredIngredients = new List<string>(recipeData.RequiredIngredientIds);

        if (recipeData.RequiredIngredientCounts == null || recipeData.RequiredIngredientCounts.Count == 0)
        {
            recipeData.RequiredIngredientCounts = new List<int>();

            for (int index = 0; index < recipeData.RequiredIngredientIds.Count; index++)
                recipeData.RequiredIngredientCounts.Add(1);
        }

        return recipeData;
    }

    /// <summary>
    /// JSON ??以꾩쓣 議곕━?꾧뎄 ??븷?쒕줈 蹂?섑빀?덈떎.
    /// </summary>
    private OO_CookingTool CreateCookingToolData(OOTechCookingToolJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        OO_CookingTool cookingToolData = new OO_CookingTool
        {
            Id = NormalizeJsonText(jsonData.Id),
            Name = NormalizeJsonText(jsonData.Name),
            Description = NormalizeJsonText(jsonData.Description),
            AcceptedIngredientIds = CreateStringList(jsonData.AcceptedIngredientIds),
            DropAreaPadding = Mathf.Max(1f, ParseFloat(GetFirstNotEmpty(jsonData.DropAreaPadding, "1.18"))),
            GuideTutorialId = NormalizeJsonText(jsonData.GuideTutorialId),
            ToolRoleId = NormalizeJsonText(jsonData.ToolRoleId)
        };

        return cookingToolData;
    }

    /// <summary>
    /// JSON ??以꾩쓣 CookingGroup ?먯떆?몃줈 蹂?섑빀?덈떎.
    /// </summary>
    private OO_CookingCueSheet CreateCookingCueSheetData(OOTechCookingCueSheetJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        OO_CookingCueSheet cueSheetData = new OO_CookingCueSheet
        {
            Id = NormalizeJsonText(jsonData.Id),
            CauldronTutorialId = NormalizeJsonText(jsonData.CauldronTutorialId),
            CuttingboardTutorialId = NormalizeJsonText(jsonData.CuttingboardTutorialId),
            JulguTutorialId = NormalizeJsonText(jsonData.JulguTutorialId),
            CauldronToolId = NormalizeJsonText(jsonData.CauldronToolId),
            CuttingboardToolId = NormalizeJsonText(jsonData.CuttingboardToolId),
            JulguToolId = NormalizeJsonText(jsonData.JulguToolId),
            SortingOrder = ParseInt(GetFirstNotEmpty(jsonData.SortingOrder, "1260")),
            ReferenceResolutionWidth = ParseFloat(GetFirstNotEmpty(jsonData.ReferenceResolutionWidth, "1920")),
            ReferenceResolutionHeight = ParseFloat(GetFirstNotEmpty(jsonData.ReferenceResolutionHeight, "1080")),
            CameraPadding = ParseFloat(GetFirstNotEmpty(jsonData.CameraPadding, "1.04")),
            GuideArrowBlinkSpeed = ParseFloat(GetFirstNotEmpty(jsonData.GuideArrowBlinkSpeed, "6")),
            GuideArrowMinimumAlpha = ParseFloat(GetFirstNotEmpty(jsonData.GuideArrowMinimumAlpha, "0.25")),
            NewBadgeBlinkSpeed = ParseFloat(GetFirstNotEmpty(jsonData.NewBadgeBlinkSpeed, "7")),
            NewBadgeMinimumAlpha = ParseFloat(GetFirstNotEmpty(jsonData.NewBadgeMinimumAlpha, "0.25")),
            DragGhostIconWidth = ParseFloat(GetFirstNotEmpty(jsonData.DragGhostIconWidth, "88")),
            DragGhostIconHeight = ParseFloat(GetFirstNotEmpty(jsonData.DragGhostIconHeight, "88")),
            EmptyPotText = NormalizeJsonText(jsonData.EmptyPotText),
            DefaultStatusText = NormalizeJsonText(jsonData.DefaultStatusText)
        };

        return cueSheetData;
    }

    /// <summary>
    /// JSON ??以꾩쓣 Stage3 ?먯떆??紐⑤뜽濡?蹂?섑빀?덈떎.
    /// ?곌뎔 ?λ㈃?먯꽌 ?꾩슂????? ?좏깮吏, 蹂댁긽, ?곗텧 ?쒓컙????踰덉뿉 ?뺣━?⑸땲??
    /// </summary>
    private OO_Stage3CueSheet CreateStage3CueSheetData(OOTechStage3CueSheetJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        OO_Stage3CueSheet cueSheetData = new OO_Stage3CueSheet
        {
            Id = NormalizeJsonText(jsonData.Id),
            StageId = NormalizeJsonText(jsonData.StageId),
            RoadGroupId = NormalizeJsonText(jsonData.RoadGroupId),
            RoadMissionDataId = NormalizeJsonText(jsonData.RoadMissionDataId),
            RoadMissionFallbackText = NormalizeJsonText(jsonData.RoadMissionFallbackText),
            StageGroupId = NormalizeJsonText(jsonData.StageGroupId),
            EncounterGroupId = NormalizeJsonText(jsonData.EncounterGroupId),
            MFCRoleId = NormalizeJsonText(jsonData.MFCRoleId),
            SangunRoleId = NormalizeJsonText(jsonData.SangunRoleId),
            MoranRoleId = NormalizeJsonText(GetFirstNotEmpty(jsonData.MoranRoleId, "Moran")),
            MrJaeikRoleId = NormalizeJsonText(GetFirstNotEmpty(jsonData.MrJaeikRoleId, "Mr.Jaeik")),
            EntryPointAId = NormalizeJsonText(jsonData.EntryPointAId),
            SangunFirstDialogueId = NormalizeJsonText(jsonData.SangunFirstDialogueId),
            EncounterSangunDialogueId = NormalizeJsonText(jsonData.EncounterSangunDialogueId),
            EncounterMoranDialogueId = NormalizeJsonText(jsonData.EncounterMoranDialogueId),
            EncounterQuestDialogueId = NormalizeJsonText(jsonData.EncounterQuestDialogueId),
            ClearDialogueId = NormalizeJsonText(GetFirstNotEmpty(jsonData.ClearDialogueId, "character_Sangun_04")),
            StageQuestId = NormalizeJsonText(jsonData.StageQuestId),
            JulguToolId = NormalizeJsonText(jsonData.JulguToolId),
            JulguTutorialId = NormalizeJsonText(jsonData.JulguTutorialId),
            KoreanCakeItemId = NormalizeJsonText(jsonData.KoreanCakeItemId),
            HoneyIngredientId = NormalizeJsonText(jsonData.HoneyIngredientId),
            HoneyKoreanCakeItemId = NormalizeJsonText(jsonData.HoneyKoreanCakeItemId),
            CakeOnlyChoiceId = NormalizeJsonText(jsonData.CakeOnlyChoiceId),
            MakeHoneyCakeChoiceId = NormalizeJsonText(jsonData.MakeHoneyCakeChoiceId),
            GiveHoneyCakeChoiceId = NormalizeJsonText(jsonData.GiveHoneyCakeChoiceId),
            DeathRetryChoiceId = NormalizeJsonText(jsonData.DeathRetryChoiceId),
            ClearRewardItemIdList = CreateStringList(jsonData.ClearRewardItemIds),
            ClearRewardCountList = CreateIntList(jsonData.ClearRewardCounts),
            NextRoadGroupName = NormalizeJsonText(jsonData.NextRoadGroupName),
            SangunStartScaleRatio = ParseFloat(GetFirstNotEmpty(jsonData.SangunStartScaleRatio, "0.5")),
            SangunThreateningScaleRatio = ParseFloat(GetFirstNotEmpty(jsonData.SangunThreateningScaleRatio, "0.7")),
            SangunAppearSeconds = ParseFloat(GetFirstNotEmpty(jsonData.SangunAppearSeconds, "1.2")),
            ThreateningAnimationSpeed = ParseFloat(GetFirstNotEmpty(jsonData.ThreateningAnimationSpeed, "0.7")),
            AttackingAnimationSpeed = ParseFloat(GetFirstNotEmpty(jsonData.AttackingAnimationSpeed, "0.7")),
            EncounterWaitSeconds = ParseFloat(GetFirstNotEmpty(jsonData.EncounterWaitSeconds, "5")),
            DeathMessage = NormalizeJsonText(jsonData.DeathMessage)
        };

        return cueSheetData;
    }

    /// <summary>
    /// JSON ??以꾩쓣 留먰뭾???곗씠?곕줈 蹂?섑빀?덈떎.
    /// 留먰뭾?좎? DialoguePanel怨??ㅻⅤ寃?諛곗슦 癒몃━ ?꾩뿉????댄븨?섎?濡??쒖떆 ?쒓컙怨?諛섎났 ?щ????④퍡 ?뺣━?⑸땲??
    /// </summary>
    private OO_SpeechBubble CreateSpeechBubbleData(OOTechSpeechBubbleJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        return new OO_SpeechBubble
        {
            Id = NormalizeJsonText(jsonData.Id),
            Name = NormalizeJsonText(jsonData.Name),
            Description = NormalizeJsonText(jsonData.Description),
            SpeakerCharacterId = NormalizeJsonText(jsonData.SpeakerCharacterId),
            Text = NormalizeJsonText(GetFirstNotEmpty(jsonData.Text, jsonData.Description)),
            TypingSpeed = ParseFloat(GetFirstNotEmpty(jsonData.TypingSpeed, "1")),
            DisplaySeconds = ParseFloat(GetFirstNotEmpty(jsonData.DisplaySeconds, "2")),
            IsLoop = ParseBool(jsonData.IsLoop),
            IsWhisper = ParseBool(jsonData.IsWhisper)
        };
    }

    /// <summary>
    /// JSON ??以꾩쓣 Stage4 ?먯떆???곗씠?곕줈 蹂?섑빀?덈떎.
    /// Controller????移대뱶??ID留?蹂닿퀬 Turtle/Rabbit 諛곗슦?먭쾶 ?꾩슂????븷??留↔퉩?덈떎.
    /// </summary>
    private OO_Stage4CueSheet CreateStage4CueSheetData(OOTechStage4CueSheetJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        return new OO_Stage4CueSheet
        {
            Id = NormalizeJsonText(jsonData.Id),
            Name = NormalizeJsonText(jsonData.Name),
            Description = NormalizeJsonText(jsonData.Description),
            Stage4_1GroupName = NormalizeJsonText(GetFirstNotEmpty(jsonData.Stage4_1GroupName, "Stage4_1Group")),
            Stage4_2GroupName = NormalizeJsonText(GetFirstNotEmpty(jsonData.Stage4_2GroupName, "Stage4_2Group")),
            PreFinalGroupName = NormalizeJsonText(GetFirstNotEmpty(jsonData.PreFinalGroupName, "PreFinal_Narration")),
            TurtleRoleId = NormalizeJsonText(GetFirstNotEmpty(jsonData.TurtleRoleId, "Turtle")),
            RabbitRoleId = NormalizeJsonText(GetFirstNotEmpty(jsonData.RabbitRoleId, "Rabbit")),
            SleepingRabbitRoleId = NormalizeJsonText(GetFirstNotEmpty(jsonData.SleepingRabbitRoleId, "Rabbit_isSleeping")),
            MoranRoleId = NormalizeJsonText(GetFirstNotEmpty(jsonData.MoranRoleId, "Moran")),
            StumpRoleId = NormalizeJsonText(GetFirstNotEmpty(jsonData.StumpRoleId, "Stump")),
            Stump2RoleId = NormalizeJsonText(GetFirstNotEmpty(jsonData.Stump2RoleId, "Stump2")),
            StopPointARoleId = NormalizeJsonText(GetFirstNotEmpty(jsonData.StopPointARoleId, "StopPoint_A")),
            TurtleDialogueIdList = NormalizeJsonText(GetFirstNotEmpty(jsonData.TurtleDialogueIdList, "character_Turtle_01|character_Turtle_02")),
            TurtleClearDialogueIdList = NormalizeJsonText(GetFirstNotEmpty(jsonData.TurtleClearDialogueIdList, "character_Turtle_03|character_Turtle_04")),
            RabbitIntroDialogueId = NormalizeJsonText(GetFirstNotEmpty(jsonData.RabbitIntroDialogueId, "character_Rabbit_01")),
            RabbitStopDialogueId = NormalizeJsonText(GetFirstNotEmpty(jsonData.RabbitStopDialogueId, "character_Rabbit_02")),
            RabbitCarrotCakeDialogueIdList = NormalizeJsonText(GetFirstNotEmpty(jsonData.RabbitCarrotCakeDialogueIdList, "character_Rabbit_03|character_Rabbit_04")),
            RabbitSpeechBubbleIdList = NormalizeJsonText(GetFirstNotEmpty(jsonData.RabbitSpeechBubbleIdList, "character_Rabbit_01|character_Rabbit_02|character_Rabbit_03|character_Rabbit_04|character_Rabbit_05")),
            RabbitSleepingBubbleId = NormalizeJsonText(GetFirstNotEmpty(jsonData.RabbitSleepingBubbleId, "character_Rabbit_06")),
            StageQuestId = NormalizeJsonText(GetFirstNotEmpty(jsonData.StageQuestId, "Stage4__Quest_01")),
            CarrotIngredientId = NormalizeJsonText(GetFirstNotEmpty(jsonData.CarrotIngredientId, "Ing_Carrot_01")),
            KoreanCakeItemId = NormalizeJsonText(GetFirstNotEmpty(jsonData.KoreanCakeItemId, "OO_KoreanCake_1")),
            CarrotStarchItemId = NormalizeJsonText(GetFirstNotEmpty(jsonData.CarrotStarchItemId, "OO_CarrotStarch_1")),
            CarrotCakeItemId = NormalizeJsonText(GetFirstNotEmpty(jsonData.CarrotCakeItemId, "OO_CarrotCake_1")),
            Stage4BGMPath = NormalizeJsonText(GetFirstNotEmpty(jsonData.Stage4BGMPath, "Audio/BGM/Stage4_BGM")),
            NextTutorialNarrationId = NormalizeJsonText(GetFirstNotEmpty(jsonData.NextTutorialNarrationId, "narration_tutorial_15")),
            InteractionDistance = ParseFloat(GetFirstNotEmpty(jsonData.InteractionDistance, "95")),
            RabbitRunSpeed = ParseFloat(GetFirstNotEmpty(jsonData.RabbitRunSpeed, "480")),
            RabbitReachTimeoutSeconds = ParseFloat(GetFirstNotEmpty(jsonData.RabbitReachTimeoutSeconds, "3"))
        };
    }

    /// <summary>
    /// JSON ??以꾩쓣 Final 援ш컙 ?먯떆???곗씠?곕줈 蹂?섑빀?덈떎.
    /// YeonSanJa ?쒓린???곗씠?곌? 怨쇨굅 ?대쫫怨??욎뿬 ?덉뼱??Controller?먯꽌 fallback?쇰줈 ?덉쟾?섍쾶 泥섎━?⑸땲??
    /// </summary>
    private OO_FinalCueSheet CreateFinalCueSheetData(OOTechFinalCueSheetJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        return new OO_FinalCueSheet
        {
            Id = NormalizeJsonText(jsonData.Id),
            Name = NormalizeJsonText(jsonData.Name),
            Description = NormalizeJsonText(jsonData.Description),
            PreFinalGroupName = NormalizeJsonText(GetFirstNotEmpty(jsonData.PreFinalGroupName, "PreFinal_Narration")),
            FinalStageGroupName = NormalizeJsonText(GetFirstNotEmpty(jsonData.FinalStageGroupName, "FinalStageGroup")),
            EpilogueGroupName = NormalizeJsonText(GetFirstNotEmpty(jsonData.EpilogueGroupName, "EpilogueGroup")),
            EndingCreditGroupName = NormalizeJsonText(GetFirstNotEmpty(jsonData.EndingCreditGroupName, "EndingCreditGroup")),
            MainMenuGroupName = NormalizeJsonText(GetFirstNotEmpty(jsonData.MainMenuGroupName, "MainMenuGroup")),
            PreFinalNarrationId = NormalizeJsonText(GetFirstNotEmpty(jsonData.PreFinalNarrationId, "narration_prologue_08")),
            EpilogueNarrationId = NormalizeJsonText(GetFirstNotEmpty(jsonData.EpilogueNarrationId, "narration_Epilogue_01")),
            FinalOpeningDialogueId = NormalizeJsonText(GetFirstNotEmpty(jsonData.FinalOpeningDialogueId, "character_YeonSanJa_01")),
            FinalHappyDialogueIdList = NormalizeJsonText(GetFirstNotEmpty(jsonData.FinalHappyDialogueIdList, "character_YeonSanJa_02|character_YeonSanJa_03")),
            MoranRoleId = NormalizeJsonText(GetFirstNotEmpty(jsonData.MoranRoleId, "Moran")),
            YeonSanJaRoleId = NormalizeJsonText(GetFirstNotEmpty(jsonData.YeonSanJaRoleId, "YeonSanJa")),
            EndPointRoleId = NormalizeJsonText(GetFirstNotEmpty(jsonData.EndPointRoleId, "End_Point")),
            FinalStageBGMPath = NormalizeJsonText(GetFirstNotEmpty(jsonData.FinalStageBGMPath, "Audio/BGM/FinalStage_BGM")),
            MoranMoveSpeed = ParseFloat(GetFirstNotEmpty(jsonData.MoranMoveSpeed, "180")),
            MoranStuckFallbackSeconds = ParseFloat(GetFirstNotEmpty(jsonData.MoranStuckFallbackSeconds, "1.5")),
            MoranEndScale = ParseFloat(GetFirstNotEmpty(jsonData.MoranEndScale, "0.6")),
            CameraZoomSize = ParseFloat(GetFirstNotEmpty(jsonData.CameraZoomSize, "280")),
            YeonSanJaEatingSpeed = ParseFloat(GetFirstNotEmpty(jsonData.YeonSanJaEatingSpeed, "0.5"))
        };
    }

    /// <summary>
    /// JSON ??以꾩쓣 ?꾩꽦 ?뚯떇 紐⑤뜽濡?蹂?섑빀?덈떎.
    /// </summary>
    private OO_Cook CreateCookData(OOTechCookJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        OO_Cook cookData = new OO_Cook
        {
            Id = NormalizeJsonText(jsonData.Id),
            Name = NormalizeJsonText(jsonData.Name),
            Description = NormalizeJsonText(jsonData.Description),
            IconPath = NormalizeJsonText(jsonData.IconPath),
            Grade = NormalizeJsonText(jsonData.Grade),
            MaxStackCount = Mathf.Max(1, ParseInt(jsonData.MaxStackCount)),
            EffectDescription = NormalizeJsonText(jsonData.EffectDescription)
        };

        return cookData;
    }

    /// <summary>
    /// JSON ??以꾩쓣 ?ㅽ뀒?댁? 紐⑤뜽濡?蹂?섑빀?덈떎.
    /// </summary>
    private OO_Stage CreateStageData(OOTechStageJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        OO_Stage stageData = new OO_Stage
        {
            Id = NormalizeJsonText(jsonData.Id),
            Name = NormalizeJsonText(jsonData.Name),
            StageNumber = ParseInt(jsonData.StageNumber),
            Description = NormalizeJsonText(jsonData.Description),
            BackgroundImagePath = NormalizeJsonText(jsonData.BackgroundImagePath),
            BGMPath = NormalizeJsonText(jsonData.BGMPath),
            RequiredPreviousStageId = NormalizeJsonText(jsonData.RequiredPreviousStageId),
            RewardItemIds = CreateStringList(jsonData.RewardItemIds),
            StartDialogueGroupId = NormalizeJsonText(jsonData.StartDialogueGroupId),
            QuestTitle = NormalizeJsonText(jsonData.QuestTitle),
            QuestDescription = NormalizeJsonText(jsonData.QuestDescription),
            RequiredCookId = NormalizeJsonText(jsonData.RequiredCookId),
            NextRoadGroupId = NormalizeJsonText(jsonData.NextRoadGroupId)
        };

        return stageData;
    }

    /// <summary>
    /// JSON ??以꾩쓣 ?ㅽ뀒?댁? ?꾨Т 紐⑤뜽濡?蹂?섑빀?덈떎.
    /// </summary>
    private OO_StageQuest CreateStageQuestData(OOTechStageQuestJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        OO_StageQuest stageQuestData = new OO_StageQuest
        {
            Id = NormalizeJsonText(jsonData.Id),
            StageId = NormalizeJsonText(jsonData.StageId),
            Name = NormalizeJsonText(jsonData.Name),
            Description = NormalizeJsonText(jsonData.Description),
            ObjectiveType = NormalizeJsonText(jsonData.ObjectiveType),
            ObjectiveId = NormalizeJsonText(jsonData.ObjectiveId),
            RequiredCount = ParseInt(jsonData.RequiredCount),
            RewardItemIds = CreateStringList(jsonData.RewardItemIds),
            NextGroupId = NormalizeJsonText(jsonData.NextGroupId)
        };

        return stageQuestData;
    }

    /// <summary>
    /// JSON ??以꾩쓣 Stage2 ?먯떆??紐⑤뜽濡?蹂?섑빀?덈떎.
    /// 臾대?媛먮룆???묒? ??以꾩쓣 ?ㅼ젣 Stage2Controller媛 ?쎌쓣 ??移대뱶濡?諛붽퓠?덈떎.
    /// </summary>
    private OO_Stage2CueSheet CreateStage2CueSheetData(OOTechStage2CueSheetJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        OO_Stage2CueSheet cueSheetData = new OO_Stage2CueSheet
        {
            Id = NormalizeJsonText(jsonData.Id),
            StageId = NormalizeJsonText(jsonData.StageId),
            MoranRoleId = NormalizeJsonText(jsonData.MoranRoleId),
            MrJaeikRoleId = NormalizeJsonText(jsonData.MrJaeikRoleId),
            ChunyangRoleId = NormalizeJsonText(jsonData.ChunyangRoleId),
            GreedyDuckRoleId = NormalizeJsonText(jsonData.GreedyDuckRoleId),
            LeeMongRyongRoleId = NormalizeJsonText(jsonData.LeeMongRyongRoleId),
            BackgroundRoleId = NormalizeJsonText(jsonData.BackgroundRoleId),
            ArriveEffectRoleId = NormalizeJsonText(jsonData.ArriveEffectRoleId),
            EntryPointAId = NormalizeJsonText(jsonData.EntryPointAId),
            EntryPointBId = NormalizeJsonText(jsonData.EntryPointBId),
            EntryPointCId = NormalizeJsonText(jsonData.EntryPointCId),
            EntryPointDId = NormalizeJsonText(jsonData.EntryPointDId),
            EntryPointEId = NormalizeJsonText(jsonData.EntryPointEId),
            TempColliderRoleId = NormalizeJsonText(jsonData.TempColliderRoleId),
            RoadMissionDataId = NormalizeJsonText(jsonData.RoadMissionDataId),
            RoadMissionFallbackText = NormalizeJsonText(jsonData.RoadMissionFallbackText),
            StageQuestDataId = NormalizeJsonText(jsonData.StageQuestDataId),
            GreedyDuckFirstDialogueId = NormalizeJsonText(jsonData.GreedyDuckFirstDialogueId),
            GreedyDuckSecondDialogueId = NormalizeJsonText(jsonData.GreedyDuckSecondDialogueId),
            LeeMongRyongFirstDialogueId = NormalizeJsonText(jsonData.LeeMongRyongFirstDialogueId),
            LeeMongRyongSecondDialogueId = NormalizeJsonText(jsonData.LeeMongRyongSecondDialogueId),
            MoranQuestDialogueId = NormalizeJsonText(jsonData.MoranQuestDialogueId),
            GreedyDuckFinalDialogueId = NormalizeJsonText(jsonData.GreedyDuckFinalDialogueId),
            EndingDialogueIdList = CreateStringList(jsonData.EndingDialogueIdList),
            KimchiStewCookId = NormalizeJsonText(jsonData.KimchiStewCookId),
            HoneyIngredientId = NormalizeJsonText(jsonData.HoneyIngredientId),
            HoneyRewardCount = ParseInt(jsonData.HoneyRewardCount),
            StageClearRewardItemIdList = CreateStringList(jsonData.StageClearRewardItemIds),
            StageClearRewardCountList = CreateIntList(jsonData.StageClearRewardCounts),
            NextRoadGroupName = NormalizeJsonText(jsonData.NextRoadGroupName),
            PlaceholderCanvasName = NormalizeJsonText(jsonData.PlaceholderCanvasName),
            NextButtonName = NormalizeJsonText(jsonData.NextButtonName),
            EntryMoveSpeed = ParseFloat(jsonData.EntryMoveSpeed),
            GreedyDuckEscapeSpeed = ParseFloat(jsonData.GreedyDuckEscapeSpeed),
            GreedyDuckExitTimeoutSeconds = ParseFloat(jsonData.GreedyDuckExitTimeoutSeconds),
            GreedyDuckEscapeAnimationSpeed = ParseFloat(jsonData.GreedyDuckEscapeAnimationSpeed),
            ForcedDialogueSeconds = ParseFloat(jsonData.ForcedDialogueSeconds),
            FinalDuckDialogueSeconds = ParseFloat(jsonData.FinalDuckDialogueSeconds),
            ArriveEffectSeconds = ParseFloat(jsonData.ArriveEffectSeconds),
            ArriveEffectAnimationSpeed = ParseFloat(jsonData.ArriveEffectAnimationSpeed),
            CameraMoveSeconds = ParseFloat(jsonData.CameraMoveSeconds),
            CameraZoomSize = ParseFloat(jsonData.CameraZoomSize),
            GreedyDuckFallbackObjectName = NormalizeJsonText(jsonData.GreedyDuckFallbackObjectName),
            GreedyDuckInteractionDistance = ParseFloat(jsonData.GreedyDuckInteractionDistance),
            GreedyDuckVisibleSortingOrder = ParseInt(jsonData.GreedyDuckVisibleSortingOrder),
            GreedyDuckVisibilityCheckInterval = ParseFloat(jsonData.GreedyDuckVisibilityCheckInterval),
            InteractionKey = NormalizeJsonText(jsonData.InteractionKey)
        };

        return cueSheetData;
    }

    /// <summary>
    /// Dialogue ?곗씠?곗쓽 ?붿옄 ID瑜?Character ?곗씠?곗쓽 ?ㅼ젣 ?대쫫?쇰줈 諛붽퓠?덈떎.
    /// </summary>
    /// <summary>
    /// Character ID瑜??붾㈃???쒖떆???ㅼ젣 ?대쫫?쇰줈 諛붽퓠?덈떎.
    /// ?щ윭 ?곗씠????낆뿉??媛숈씠 ?곕뒗 諛곗슦 ?대쫫 罹먯뒪??蹂댁“ ?⑥닔?낅땲??
    /// </summary>
    private string ResolveCharacterName(string characterId, string fallbackName)
    {
        string normalizedCharacterId = NormalizeJsonText(characterId);
        string normalizedFallbackName = NormalizeJsonText(fallbackName);

        if (!string.IsNullOrEmpty(normalizedCharacterId) && _characterDic.TryGetValue(normalizedCharacterId, out OO_Character characterData))
            return NormalizeJsonText(characterData.Name);

        return normalizedFallbackName;
    }

    private string ResolveDialogueSpeakerName(OOTechDialogueJsonData jsonData)
    {
        string speakerId = NormalizeJsonText(GetFirstNotEmpty(jsonData.SpeakerCharacterId, jsonData.CharacterId));
        string speakerName = NormalizeJsonText(GetFirstNotEmpty(jsonData.SpeakerName, jsonData.Name));

        if (string.IsNullOrEmpty(speakerId))
            speakerId = InferSpeakerCharacterIdFromDialogueId(jsonData.Id);

        if (!string.IsNullOrEmpty(speakerId) && _characterDic.TryGetValue(speakerId, out OO_Character speakerData))
            speakerName = NormalizeJsonText(speakerData.Name);

        if (string.IsNullOrEmpty(speakerName) && !string.IsNullOrEmpty(jsonData.Id) && _characterDic.TryGetValue(jsonData.Id, out OO_Character sameIdCharacterData))
            speakerName = NormalizeJsonText(sameIdCharacterData.Name);

        return speakerName;
    }

    /// <summary>
    /// Dialogue ?됱뿉 ?붿옄 移몄씠 鍮꾩뼱 ?덉뼱??ID ?묐몢?щ줈 諛곗슦 ?대쫫?쒕? 異붾줎?⑸땲??
    /// ?? character_Moran_04??紐⑤? 罹먮┃???곗씠??character_Moran_02瑜??붿옄濡??ъ슜?⑸땲??
    /// </summary>
    private string InferSpeakerCharacterIdFromDialogueId(string dialogueId)
    {
        string normalizedId = NormalizeJsonText(dialogueId);

        if (string.IsNullOrEmpty(normalizedId))
            return string.Empty;

        if (normalizedId.StartsWith("character_Moran_"))
            return "character_Moran_02";

        if (normalizedId.StartsWith("character_Chunyang_"))
            return "character_Chunyang_05";

        if (normalizedId.StartsWith("character_Mr.Jaeik_"))
            return "character_Mr.Jaeik_04";

        if (normalizedId.StartsWith("character_narrator_"))
            return "character_narrator_01";

        return string.Empty;
    }

    // ==================== 蹂???좏떥 ====================

    /// <summary>
    /// JsonUtility媛 諛곗뿴???쎌쓣 ???덈룄濡?諛곗뿴 JSON??Items ?섑띁濡?媛먯뙃?덈떎.
    /// </summary>
    private string WrapJsonArray(string jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText))
            return "{\"Items\":[]}";

        string trimmedJsonText = jsonText.Trim();

        if (trimmedJsonText.StartsWith("{"))
            return trimmedJsonText;

        return "{\"Items\":" + trimmedJsonText + "}";
    }

    /// <summary>
    /// Resources/OO_MFC/Data? Resources/JsonOutput?먯꽌 ?곗씠??TextAsset??李얠뒿?덈떎.
    /// </summary>
    private List<TextAsset> LoadDataTextAssetArray(string dataName)
    {
        List<TextAsset> textAssetList = new List<TextAsset>();
        string[] resourcePathArray =
        {
            $"OO_MFC/Data/{dataName}",
            $"JsonOutput/{dataName}"
        };

        foreach (string resourcePath in resourcePathArray)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);

            if (jsonFile != null)
                textAssetList.Add(jsonFile);
        }

        if (textAssetList.Count == 0)
            Debug.LogWarning($"[OOTechGameDataManager] {dataName}.json 파일을 찾을 수 없습니다.");

        return textAssetList;
    }

    private int ParseInt(string value)
    {
        return int.TryParse(value, out int result) ? result : 0;
    }

    /// <summary>
    /// ?묒??먯꽌 ?ㅼ뼱???レ옄 臾몄옄?댁쓣 float濡?諛붽퓠?덈떎.
    /// ?띾룄? ?곗텧 ?쒓컙泥섎읆 ?뚯닔?먯씠 ?꾩슂???먯떆??媛믪쓣 ?쎌쓣 ???ъ슜?⑸땲??
    /// </summary>
    private float ParseFloat(string value)
    {
        string normalizedValue = NormalizeJsonText(value);

        if (string.IsNullOrEmpty(normalizedValue))
            return 0f;

        return float.TryParse(normalizedValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result) ? result : 0f;
    }

    /// <summary>
    /// Excel/JSON?먯꽌 ?ㅼ뼱??true, 1, yes, ??媛숈? 媛믪쓣 bool濡??뺣━?⑸땲??
    /// 留먰뭾??諛섎났 ?щ?泥섎읆 ?묒? ?곗텧 ?ㅼ쐞移섎? 肄붾뱶 ?섏젙 ?놁씠 耳쒓퀬 ?????ъ슜?⑸땲??
    /// </summary>
    private bool ParseBool(string value)
    {
        string normalizedValue = NormalizeJsonText(value).ToLowerInvariant();

        return normalizedValue == "true" ||
               normalizedValue == "1" ||
               normalizedValue == "yes" ||
               normalizedValue == "y" ||
               normalizedValue == "예";
    }

    private List<string> CreateTextList(string rawText)
    {
        List<string> textList = new List<string>();

        string normalizedText = NormalizeJsonText(rawText);

        if (!string.IsNullOrWhiteSpace(normalizedText))
            textList.Add(normalizedText);

        return textList;
    }

    private List<string> CreateBackgroundImagePathList(OOTechNarrationJsonData jsonData)
    {
        List<string> pathList = new List<string>();

        string singlePath = NormalizeJsonText(jsonData.BackgroundImagePath);
        if (!string.IsNullOrWhiteSpace(singlePath))
            pathList.Add(singlePath);

        string multiPath = NormalizeJsonText(jsonData.BackgroundImagePaths);
        if (!string.IsNullOrWhiteSpace(multiPath))
            pathList.Add(multiPath);

        return pathList;
    }

    private List<string> CreateStringList(string rawText)
    {
        List<string> stringList = new List<string>();
        string normalizedText = NormalizeJsonText(rawText);

        if (string.IsNullOrWhiteSpace(normalizedText))
            return stringList;

        string[] splitTextArray = normalizedText.Split(new[] { "|", ",", "<np>" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string splitText in splitTextArray)
        {
            string itemText = NormalizeJsonText(splitText);

            if (!string.IsNullOrWhiteSpace(itemText))
                stringList.Add(itemText);
        }

        return stringList;
    }

    private List<int> CreateIntList(string rawText)
    {
        List<int> intList = new List<int>();
        List<string> stringList = CreateStringList(rawText);

        foreach (string text in stringList)
            intList.Add(ParseInt(text));

        return intList;
    }

    private string GetFirstNotEmpty(params string[] valueArray)
    {
        if (valueArray == null)
            return string.Empty;

        foreach (string value in valueArray)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return string.Empty;
    }

    private string NormalizeJsonText(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return string.Empty;

        string normalizedText = rawText.Trim();

        if (normalizedText.Length >= 2 && normalizedText[0] == '"' && normalizedText[normalizedText.Length - 1] == '"')
            normalizedText = normalizedText.Substring(1, normalizedText.Length - 2);

        normalizedText = normalizedText.Replace("\\n", "\n");
        normalizedText = normalizedText.Replace("\\\"", "\"");

        return normalizedText.Trim();
    }

    // ==================== ?곗씠??議고쉶 ====================

    /// <summary>
    /// ?섎젅?댁뀡 ?곗씠?곕? ID濡?議고쉶?⑸땲??
    /// </summary>
    /// <summary>
    /// ID濡??섎젅?댁뀡 ?곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    public OO_Narration GetNarrationData(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[OOTechGameDataManager] 조회할 Narration ID가 비어 있습니다.");
            return null;
        }

        if (_narrationDic.TryGetValue(id, out OO_Narration data))
            return data;

        Debug.LogWarning($"[OOTechGameDataManager] Narration 데이터를 찾을 수 없음: {id}");
        return null;
    }

    /// <summary>
    /// 罹먮┃???곗씠?곕? ID濡?議고쉶?⑸땲??
    /// </summary>
    /// <summary>
    /// ID濡?罹먮┃???곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    public OO_Character GetCharacterData(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[OOTechGameDataManager] 조회할 Character ID가 비어 있습니다.");
            return null;
        }

        if (_characterDic.TryGetValue(id, out OO_Character data))
            return data;

        Debug.LogWarning($"[OOTechGameDataManager] Character 데이터를 찾을 수 없음: {id}");
        return null;
    }

    /// <summary>
    /// 罹먮┃??????곗씠?곕? ID濡?議고쉶?⑸땲??
    /// </summary>
    /// <summary>
    /// ID濡?????곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    public OO_Dialogue GetDialogueData(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[OOTechGameDataManager] 조회할 Dialogue ID가 비어 있습니다.");
            return null;
        }

        if (_dialogueDic.TryGetValue(id, out OO_Dialogue data))
            return data;

        Debug.LogWarning($"[OOTechGameDataManager] Dialogue 데이터를 찾을 수 없음: {id}");
        return null;
    }

    /// <summary>
    /// ID濡??숈떆 ???洹몃９ ?곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    public OO_DialogueGroup GetDialogueGroupData(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[OOTechGameDataManager] 조회할 DialogueGroup ID가 비어 있습니다.");
            return null;
        }

        if (_dialogueGroupDic.TryGetValue(id, out OO_DialogueGroup data))
            return data;

        Debug.LogWarning($"[OOTechGameDataManager] DialogueGroup 데이터를 찾을 수 없음: {id}");
        return null;
    }

    /// <summary>
    /// ?쒗넗由ъ뼹 ?덈궡 ?곗씠?곕? ID濡?議고쉶?⑸땲??
    /// </summary>
    /// <summary>
    /// ID濡??쒗넗由ъ뼹 ?곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    /// <summary>
    /// ID濡??좏깮吏 ?곗씠?곕? 議고쉶?⑸땲??
    /// Controller???좏깮吏 臾몄옣??吏곸젒 ?ㅺ퀬 ?덉? ?딄퀬 ??李쎄뎄留??듯빐 ?먯떆?몃? 爰쇰깄?덈떎.
    /// </summary>
    public OO_Choice GetChoiceData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _choiceDic.TryGetValue(id, out OO_Choice data) ? data : null;
    }

    /// <summary>
    /// ID濡??꾧컧 ??ぉ??議고쉶?⑸땲??
    /// </summary>
    public OO_Codex GetCodexData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _codexDic.TryGetValue(id, out OO_Codex data) ? data : null;
    }

    /// <summary>
    /// ?꾩옱 ?깅줉??紐⑤뱺 ?꾧컧 ??ぉ??由ъ뒪?몃줈 諛섑솚?⑸땲??
    /// Game View?먯꽌??CodexGroup???ㅽ겕濡?紐⑸줉?????곗씠?곕? ?ъ슜?⑸땲??
    /// </summary>
    public List<OO_Codex> GetCodexDataList()
    {
        List<OO_Codex> codexDataList = new List<OO_Codex>(_codexDic.Values);
        codexDataList.Sort((leftData, rightData) => string.Compare(leftData != null ? leftData.Id : string.Empty, rightData != null ? rightData.Id : string.Empty, StringComparison.Ordinal));
        return codexDataList;
    }

    public OO_Tutorial GetTutorialData(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[OOTechGameDataManager] 조회할 Tutorial ID가 비어 있습니다.");
            return null;
        }

        if (_tutorialDic.TryGetValue(id, out OO_Tutorial data))
            return data;

        Debug.LogWarning($"[OOTechGameDataManager] Tutorial 데이터를 찾을 수 없음: {id}");
        return null;
    }

    /// <summary>
    /// ID濡??щ즺 ?곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    public OO_Ingredient GetIngredientData(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[OOTechGameDataManager] 조회할 Ingredient ID가 비어 있습니다.");
            return null;
        }

        if (_ingredientDic.TryGetValue(id, out OO_Ingredient data))
            return data;

        Debug.LogWarning($"[OOTechGameDataManager] Ingredient 데이터를 찾을 수 없음: {id}");
        return null;
    }
    /// <summary>
    /// ID濡??덉떆???곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    /// <summary>
    /// ?щ즺 ?곗씠?곌? ?덈뒗吏留?議곗슜???뺤씤?⑸땲??
    /// ?곹솕濡?鍮꾩쑀?섎㈃ 李쎄퀬??? 諛곗슦媛 ?덈뒗吏 ?뺤씤留??섍퀬, ?녿떎怨?怨듭뿰???꾩껜??寃쎄퀬 諛⑹넚???섏? ?딅뒗 議고쉶?낅땲??
    /// </summary>
    public bool TryGetIngredientData(string id, out OO_Ingredient data)
    {
        data = null;

        if (string.IsNullOrEmpty(id))
            return false;

        return _ingredientDic.TryGetValue(id, out data);
    }

    public OO_Recipe GetRecipeData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _recipeDic.TryGetValue(id, out OO_Recipe data) ? data : null;
    }

    /// <summary>
    /// ID濡?議곕━?꾧뎄 ??븷???곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    public OO_CookingTool GetCookingToolData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _cookingToolDic.TryGetValue(id, out OO_CookingTool data) ? data : null;
    }

    /// <summary>
    /// 議곕━?꾧뎄 ??븷?쒓? ?덈뒗吏留?議곗슜???뺤씤?⑸땲??
    /// </summary>
    public bool TryGetCookingToolData(string id, out OO_CookingTool data)
    {
        data = null;

        if (string.IsNullOrEmpty(id))
            return false;

        return _cookingToolDic.TryGetValue(id, out data);
    }

    /// <summary>
    /// ID濡?CookingGroup ?먯떆???곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    public OO_CookingCueSheet GetCookingCueSheetData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _cookingCueSheetDic.TryGetValue(id, out OO_CookingCueSheet data) ? data : null;
    }

    /// <summary>
    /// ID濡??꾩꽦 ?뚯떇 ?곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    public OO_Cook GetCookData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _cookDic.TryGetValue(id, out OO_Cook data) ? data : null;
    }

    /// <summary>
    /// ID濡??ㅽ뀒?댁? ?곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    /// <summary>
    /// ?꾩꽦 ?뚯떇 ?곗씠?곌? ?덈뒗吏留?議곗슜???뺤씤?⑸땲??
    /// ?щ즺? ?꾩꽦 ?뚯떇??媛숈? ?몃깽?좊━ ?щ’??蹂댁뿬以???遺덊븘?뷀븳 寃쎄퀬 濡쒓렇瑜?以꾩씠湲??꾪븳 ?덉쟾 議고쉶?낅땲??
    /// </summary>
    public bool TryGetCookData(string id, out OO_Cook data)
    {
        data = null;

        if (string.IsNullOrEmpty(id))
            return false;

        return _cookDic.TryGetValue(id, out data);
    }

    public OO_Stage GetStageData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _stageDic.TryGetValue(id, out OO_Stage data) ? data : null;
    }

    /// <summary>
    /// ID濡??ㅽ뀒?댁? ?꾨Т ?곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    public OO_StageQuest GetStageQuestData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _stageQuestDic.TryGetValue(id, out OO_StageQuest data) ? data : null;
    }

    /// <summary>
    /// ID濡?Stage2 ?먯떆???곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    public OO_Stage2CueSheet GetStage2CueSheetData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _stage2CueSheetDic.TryGetValue(id, out OO_Stage2CueSheet data) ? data : null;
    }

    /// <summary>
    /// ID濡?Stage3 ?먯떆???곗씠?곕? 議고쉶?⑸땲??
    /// </summary>
    public OO_Stage3CueSheet GetStage3CueSheetData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _stage3CueSheetDic.TryGetValue(id, out OO_Stage3CueSheet data) ? data : null;
    }

    public OO_SpeechBubble GetSpeechBubbleData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _speechBubbleDic.TryGetValue(id, out OO_SpeechBubble data) ? data : null;
    }

    public OO_Stage4CueSheet GetStage4CueSheetData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _stage4CueSheetDic.TryGetValue(id, out OO_Stage4CueSheet data) ? data : null;
    }

    public OO_FinalCueSheet GetFinalCueSheetData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _finalCueSheetDic.TryGetValue(id, out OO_FinalCueSheet data) ? data : null;
    }

    /// <summary>
    /// ?꾩옱 ?깅줉??紐⑤뱺 ?덉떆?쇰? 諛섑솚?⑸땲??
    /// </summary>
    public List<OO_Recipe> GetRecipeDataList()
    {
        return new List<OO_Recipe>(_recipeDic.Values);
    }

    public List<OO_Character> GetCharacterDataList()
    {
        List<OO_Character> dataList = new List<OO_Character>(_characterDic.Values);
        dataList.Sort((leftData, rightData) => string.Compare(leftData != null ? leftData.Id : string.Empty, rightData != null ? rightData.Id : string.Empty, StringComparison.Ordinal));
        return dataList;
    }

    public List<OO_Cook> GetCookDataList()
    {
        List<OO_Cook> dataList = new List<OO_Cook>(_cookDic.Values);
        dataList.Sort((leftData, rightData) => string.Compare(leftData != null ? leftData.Id : string.Empty, rightData != null ? rightData.Id : string.Empty, StringComparison.Ordinal));
        return dataList;
    }

    public List<OO_Ingredient> GetIngredientDataList()
    {
        List<OO_Ingredient> dataList = new List<OO_Ingredient>(_ingredientDic.Values);
        dataList.Sort((leftData, rightData) => string.Compare(leftData != null ? leftData.Id : string.Empty, rightData != null ? rightData.Id : string.Empty, StringComparison.Ordinal));
        return dataList;
    }

    public List<OO_Narration> GetNarrationDataList()
    {
        List<OO_Narration> dataList = new List<OO_Narration>(_narrationDic.Values);
        dataList.Sort((leftData, rightData) => string.Compare(leftData != null ? leftData.Id : string.Empty, rightData != null ? rightData.Id : string.Empty, StringComparison.Ordinal));
        return dataList;
    }

    public List<OO_Stage> GetStageDataList()
    {
        List<OO_Stage> dataList = new List<OO_Stage>(_stageDic.Values);
        dataList.Sort((leftData, rightData) => string.Compare(leftData != null ? leftData.Id : string.Empty, rightData != null ? rightData.Id : string.Empty, StringComparison.Ordinal));
        return dataList;
    }

    /// <summary>
    /// ?ъ엯???щ즺 紐⑸줉怨??꾩쟾???쇱튂?섎뒗 ?덉떆?쇰? 李얠뒿?덈떎.
    /// </summary>
    public OO_Recipe RequestRecipeByIngredientList(List<string> ingredientIdList)
    {
        if (ingredientIdList == null || ingredientIdList.Count == 0)
            return null;

        foreach (OO_Recipe recipeData in _recipeDic.Values)
        {
            if (recipeData == null)
                continue;

            List<string> requiredIngredientList = recipeData.RequiredIngredientIds != null && recipeData.RequiredIngredientIds.Count > 0
                ? recipeData.RequiredIngredientIds
                : recipeData.RequiredIngredients;

            if (IsSameIngredientSet(requiredIngredientList, ingredientIdList))
                return recipeData;
        }

        return null;
    }

    private bool IsSameIngredientSet(List<string> requiredIngredientList, List<string> inputIngredientList)
    {
        if (requiredIngredientList == null || inputIngredientList == null)
            return false;

        if (requiredIngredientList.Count != inputIngredientList.Count)
            return false;

        Dictionary<string, int> countDic = new Dictionary<string, int>();

        foreach (string requiredIngredientId in requiredIngredientList)
        {
            string normalizedId = NormalizeJsonText(requiredIngredientId);

            if (string.IsNullOrEmpty(normalizedId))
                continue;

            if (!countDic.ContainsKey(normalizedId))
                countDic[normalizedId] = 0;

            countDic[normalizedId]++;
        }

        foreach (string inputIngredientId in inputIngredientList)
        {
            string normalizedId = NormalizeJsonText(inputIngredientId);

            if (string.IsNullOrEmpty(normalizedId) || !countDic.ContainsKey(normalizedId))
                return false;

            countDic[normalizedId]--;

            if (countDic[normalizedId] < 0)
                return false;
        }

        foreach (int count in countDic.Values)
        {
            if (count != 0)
                return false;
        }

        return true;
    }
}

// ==================== Json Wrapper ====================

[Serializable]
public class OOTechNarrationJsonWrapper
{
    public OOTechNarrationJsonData[] Items;
}

[Serializable]
public class OOTechNarrationJsonData
{
    public string Id;
    public string Title;
    public string PartNumber;
    public string NarrationTexts;
    public string BackgroundImagePath;
    public string BackgroundImagePaths;
    public string BGMPath;
    public string NextGroup;
}

[Serializable]
public class OOTechCharacterJsonWrapper
{
    public OO_Character[] Items;
}

[Serializable]
public class OOTechDialogueJsonWrapper
{
    public OOTechDialogueJsonData[] Items;
}

[Serializable]
public class OOTechDialogueJsonData
{
    public string Id;
    public string Name;
    public string SpeakerName;
    public string CharacterId;
    public string SpeakerCharacterId;
    public string Description;
    public string Text;
    public string NextDialogueId;
    public string SelectionNameList;
    public string SelectionDialogueIdList;
    public string TexturePath;
    public string VoicePath;
}

[Serializable]
public class OOTechDialogueGroupJsonWrapper
{
    public OOTechDialogueGroupJsonData[] Items;
}

[Serializable]
public class OOTechDialogueGroupJsonData
{
    public string Id;
    public string Name;
    public string Description;
    public string SpeakerCharacterIdList;
    public string SpeakerNameList;
    public string Text;
    public string DialogueIdList;
    public string NextDialogueGroupId;
}

[Serializable]
public class OOTechChoiceJsonWrapper
{
    public OOTechChoiceJsonData[] Items;
}

[Serializable]
public class OOTechChoiceJsonData
{
    public string Id;
    public string SpeakerId;
    public string SpeakerName;
    public string Description;
    public string PromptText;
    public string ChoiceMode;
    public string OptionCount;
    public string OptionKeyList;
    public string OptionTextList;
    public string ResultTypeList;
    public string ResultValueList;
    public string ResultCountList;
    public string NextDialogueIdList;
    public string NextChoiceIdList;
    public string StageQuestIdList;
    public string Memo;
}

[Serializable]
public class OOTechCodexJsonWrapper
{
    public OO_Codex[] Items;
}

[Serializable]
public class OOTechTutorialJsonWrapper
{
    public OOTechTutorialJsonData[] Items;
}

[Serializable]
public class OOTechTutorialJsonData
{
    public string Id;
    public string Name;
    public string Title;
    public string Description;
    public string TargetStageId;
    public string TriggerCondition;
    public string DialogueGroupId;
    public string SkillList;
    public string UseWeaponId;
    public string BasicCostumeId;
}

[Serializable]
public class OOTechIngredientJsonWrapper
{
    public OOTechIngredientJsonData[] Items;
}

[Serializable]
public class OOTechIngredientJsonData
{
    public string Id;
    public string Name;
    public string Description;
    public string IconPath;
    public string Grade;
    public string MaxStackCount;
}

[Serializable]
public class OOTechRecipeJsonWrapper
{
    public OOTechRecipeJsonData[] Items;
}

[Serializable]
public class OOTechRecipeJsonData
{
    public string Id;
    public string Name;
    public string Description;
    public string ResultItemId;
    public string RequiredIngredients;
    public string MaxDuplicateCount;
    public string RequiredTool;
    public string RequiredIngredientIds;
    public string RequiredIngredientCounts;
    public string RequiredToolIds;
    public string ResultCount;
    public string QuantityGuideText;
}

[Serializable]
public class OOTechCookingToolJsonWrapper
{
    public OOTechCookingToolJsonData[] Items;
}

[Serializable]
public class OOTechCookingToolJsonData
{
    public string Id;
    public string Name;
    public string Description;
    public string AcceptedIngredientIds;
    public string DropAreaPadding;
    public string GuideTutorialId;
    public string ToolRoleId;
}

[Serializable]
public class OOTechCookingCueSheetJsonWrapper
{
    public OOTechCookingCueSheetJsonData[] Items;
}

[Serializable]
public class OOTechCookingCueSheetJsonData
{
    public string Id;
    public string CauldronTutorialId;
    public string CuttingboardTutorialId;
    public string JulguTutorialId;
    public string CauldronToolId;
    public string CuttingboardToolId;
    public string JulguToolId;
    public string SortingOrder;
    public string ReferenceResolutionWidth;
    public string ReferenceResolutionHeight;
    public string CameraPadding;
    public string GuideArrowBlinkSpeed;
    public string GuideArrowMinimumAlpha;
    public string NewBadgeBlinkSpeed;
    public string NewBadgeMinimumAlpha;
    public string DragGhostIconWidth;
    public string DragGhostIconHeight;
    public string EmptyPotText;
    public string DefaultStatusText;
}

[Serializable]
public class OOTechCookJsonWrapper
{
    public OOTechCookJsonData[] Items;
}

[Serializable]
public class OOTechCookJsonData
{
    public string Id;
    public string Name;
    public string Description;
    public string IconPath;
    public string Grade;
    public string MaxStackCount;
    public string EffectDescription;
}

[Serializable]
public class OOTechStageJsonWrapper
{
    public OOTechStageJsonData[] Items;
}

[Serializable]
public class OOTechStageJsonData
{
    public string Id;
    public string Name;
    public string StageNumber;
    public string Description;
    public string BackgroundImagePath;
    public string BGMPath;
    public string RequiredPreviousStageId;
    public string RewardItemIds;
    public string StartDialogueGroupId;
    public string QuestTitle;
    public string QuestDescription;
    public string RequiredCookId;
    public string NextRoadGroupId;
}

[Serializable]
public class OOTechStageQuestJsonWrapper
{
    public OOTechStageQuestJsonData[] Items;
}

[Serializable]
public class OOTechStageQuestJsonData
{
    public string Id;
    public string StageId;
    public string Name;
    public string Description;
    public string ObjectiveType;
    public string ObjectiveId;
    public string RequiredCount;
    public string RewardItemIds;
    public string NextGroupId;
}

[Serializable]
public class OOTechStage2CueSheetJsonWrapper
{
    public OOTechStage2CueSheetJsonData[] Items;
}

[Serializable]
public class OOTechStage2CueSheetJsonData
{
    public string Id;
    public string StageId;
    public string MoranRoleId;
    public string MrJaeikRoleId;
    public string ChunyangRoleId;
    public string GreedyDuckRoleId;
    public string LeeMongRyongRoleId;
    public string BackgroundRoleId;
    public string ArriveEffectRoleId;
    public string EntryPointAId;
    public string EntryPointBId;
    public string EntryPointCId;
    public string EntryPointDId;
    public string EntryPointEId;
    public string TempColliderRoleId;
    public string RoadMissionDataId;
    public string RoadMissionFallbackText;
    public string StageQuestDataId;
    public string GreedyDuckFirstDialogueId;
    public string GreedyDuckSecondDialogueId;
    public string LeeMongRyongFirstDialogueId;
    public string LeeMongRyongSecondDialogueId;
    public string MoranQuestDialogueId;
    public string GreedyDuckFinalDialogueId;
    public string EndingDialogueIdList;
    public string KimchiStewCookId;
    public string HoneyIngredientId;
    public string HoneyRewardCount;
    public string StageClearRewardItemIds;
    public string StageClearRewardCounts;
    public string NextRoadGroupName;
    public string PlaceholderCanvasName;
    public string NextButtonName;
    public string EntryMoveSpeed;
    public string GreedyDuckEscapeSpeed;
    public string GreedyDuckExitTimeoutSeconds;
    public string GreedyDuckEscapeAnimationSpeed;
    public string ForcedDialogueSeconds;
    public string FinalDuckDialogueSeconds;
    public string ArriveEffectSeconds;
    public string ArriveEffectAnimationSpeed;
    public string CameraMoveSeconds;
    public string CameraZoomSize;
    public string GreedyDuckFallbackObjectName;
    public string GreedyDuckInteractionDistance;
    public string GreedyDuckVisibleSortingOrder;
    public string GreedyDuckVisibilityCheckInterval;
    public string InteractionKey;
}

[Serializable]
public class OOTechStage3CueSheetJsonWrapper
{
    public OOTechStage3CueSheetJsonData[] Items;
}

[Serializable]
public class OOTechStage3CueSheetJsonData
{
    public string Id;
    public string StageId;
    public string RoadGroupId;
    public string RoadMissionDataId;
    public string RoadMissionFallbackText;
    public string StageGroupId;
    public string EncounterGroupId;
    public string MFCRoleId;
    public string SangunRoleId;
    public string MoranRoleId;
    public string MrJaeikRoleId;
    public string EntryPointAId;
    public string SangunFirstDialogueId;
    public string EncounterSangunDialogueId;
    public string EncounterMoranDialogueId;
    public string EncounterQuestDialogueId;
    public string ClearDialogueId;
    public string StageQuestId;
    public string JulguToolId;
    public string JulguTutorialId;
    public string KoreanCakeItemId;
    public string HoneyIngredientId;
    public string HoneyKoreanCakeItemId;
    public string CakeOnlyChoiceId;
    public string MakeHoneyCakeChoiceId;
    public string GiveHoneyCakeChoiceId;
    public string DeathRetryChoiceId;
    public string ClearRewardItemIds;
    public string ClearRewardCounts;
    public string NextRoadGroupName;
    public string SangunStartScaleRatio;
    public string SangunThreateningScaleRatio;
    public string SangunAppearSeconds;
    public string ThreateningAnimationSpeed;
    public string AttackingAnimationSpeed;
    public string EncounterWaitSeconds;
    public string DeathMessage;
}

[Serializable]
public class OOTechSpeechBubbleJsonWrapper
{
    public OOTechSpeechBubbleJsonData[] Items;
}

[Serializable]
public class OOTechSpeechBubbleJsonData
{
    public string Id;
    public string Name;
    public string Description;
    public string SpeakerCharacterId;
    public string Text;
    public string TypingSpeed;
    public string DisplaySeconds;
    public string IsLoop;
    public string IsWhisper;
}

[Serializable]
public class OOTechStage4CueSheetJsonWrapper
{
    public OOTechStage4CueSheetJsonData[] Items;
}

[Serializable]
public class OOTechStage4CueSheetJsonData
{
    public string Id;
    public string Name;
    public string Description;
    public string Stage4_1GroupName;
    public string Stage4_2GroupName;
    public string PreFinalGroupName;
    public string TurtleRoleId;
    public string RabbitRoleId;
    public string SleepingRabbitRoleId;
    public string MoranRoleId;
    public string StumpRoleId;
    public string Stump2RoleId;
    public string StopPointARoleId;
    public string TurtleDialogueIdList;
    public string TurtleClearDialogueIdList;
    public string RabbitIntroDialogueId;
    public string RabbitStopDialogueId;
    public string RabbitCarrotCakeDialogueIdList;
    public string RabbitSpeechBubbleIdList;
    public string RabbitSleepingBubbleId;
    public string StageQuestId;
    public string CarrotIngredientId;
    public string KoreanCakeItemId;
    public string CarrotStarchItemId;
    public string CarrotCakeItemId;
    public string Stage4BGMPath;
    public string NextTutorialNarrationId;
    public string InteractionDistance;
    public string RabbitRunSpeed;
    public string RabbitReachTimeoutSeconds;
}

[Serializable]
public class OOTechFinalCueSheetJsonWrapper
{
    public OOTechFinalCueSheetJsonData[] Items;
}

[Serializable]
public class OOTechFinalCueSheetJsonData
{
    public string Id;
    public string Name;
    public string Description;
    public string PreFinalGroupName;
    public string FinalStageGroupName;
    public string EpilogueGroupName;
    public string EndingCreditGroupName;
    public string MainMenuGroupName;
    public string PreFinalNarrationId;
    public string EpilogueNarrationId;
    public string FinalOpeningDialogueId;
    public string FinalHappyDialogueIdList;
    public string MoranRoleId;
    public string YeonSanJaRoleId;
    public string EndPointRoleId;
    public string FinalStageBGMPath;
    public string MoranMoveSpeed;
    public string MoranStuckFallbackSeconds;
    public string MoranEndScale;
    public string CameraZoomSize;
    public string YeonSanJaEatingSpeed;
}

