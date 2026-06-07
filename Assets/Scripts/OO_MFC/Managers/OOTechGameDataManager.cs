// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechGameDataManager.cs
// - 역할: 여러 장면에서 함께 쓰는 공통 Manager입니다.
// - 감독 관점: 각 부서에 공통 창구를 열어 주는 제작 본부입니다.
// - 유지보수 포인트: 특정 장면의 세부 연출을 직접 처리하지 말고, 공통 조회/등록/요청 API만 유지합니다.
// =============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OO_MFC에서 사용하는 Static Data Json을 로드하고 조회하는 매니저입니다.
/// Json 파일은 Resources/OO_MFC/Data 아래에 두며, 런타임 진행 데이터는 GameManager의 Model에서 따로 관리합니다.
/// </summary>
public class OOTechGameDataManager : MonoBehaviour
{
    // 읽는 순서:
    // 1. Awake/RequestLoadAllData 계열: Resources/JsonOutput에 있는 JSON 파일을 읽어 Dictionary에 등록합니다.
    // 2. LoadOO_XXX 계열: 각 엑셀 테이블에서 나온 JSON을 해당 Data 클래스로 변환합니다.
    // 3. CreateXXXData 계열: 문자열로 들어온 JSON 값을 int, List<string> 같은 Unity 자료형으로 정리합니다.
    // 4. GetXXXData 계열: Controller와 UI가 ID로 static data를 안전하게 조회합니다.
    // 5. NormalizeJsonText/List 계열: 엑셀 빈칸, null, 구분자 문자열을 게임에서 쓰기 좋은 값으로 바꿉니다.
    // 유지보수 주의:
    // - 새 엑셀 파일을 추가하면 Data 클래스, JsonData 클래스, Load 메서드, Get 메서드를 함께 추가합니다.
    // - 플레이 중 바뀌는 값은 여기 넣지 말고 Model/GameManager 쪽으로 보냅니다.
    // - 데이터 ID가 틀리면 화면이 비거나 화자가 잘못 나오므로, 경고 로그를 지우지 말고 원인을 추적합니다.

    public static OOTechGameDataManager Inst { get; private set; }

    // ==================== 데이터 Dictionary ====================
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

    /// <summary>
    /// 중복 매니저를 정리하고 Static Data를 로드합니다.
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
    /// 매니저가 파괴될 때 전역 참조를 비웁니다.
    /// </summary>
    private void OnDestroy()
    {
        if (Inst == this)
            Inst = null;
    }

    // ==================== 데이터 로드 ====================

    /// <summary>
    /// 게임 시작에 필요한 모든 Static Data를 로드합니다.
    /// Character는 Dialogue의 화자 이름 보정에 쓰일 수 있어 Dialogue보다 먼저 로드합니다.
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

        Debug.Log("[OOTechGameDataManager] 모든 데이터 로드 완료");
    }

    /// <summary>
    /// OO_Narration.json을 읽어 프롤로그/나레이션 대본으로 등록합니다.
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

        Debug.Log($"[OOTechGameDataManager] Narration 데이터 로드 완료: {_narrationDic.Count}개");
    }

    /// <summary>
    /// OO_Character.json을 읽어 화자 이름과 캐릭터 정보를 등록합니다.
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

        Debug.Log($"[OOTechGameDataManager] Character 데이터 로드 완료: {_characterDic.Count}개");
    }

    /// <summary>
    /// OO_Dialogue.json을 읽어 캐릭터 대사를 등록합니다.
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

        Debug.Log($"[OOTechGameDataManager] Dialogue 데이터 로드 완료: {_dialogueDic.Count}개");
    }

    /// <summary>
    /// OO_DialogueGroup.json을 읽어 여러 인물이 동시에 말하는 대사 묶음을 등록합니다.
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

        Debug.Log($"[OOTechGameDataManager] DialogueGroup 데이터 로드 완료: {_dialogueGroupDic.Count}개");
    }

    /// <summary>
    /// OO_Tutorial.json을 읽어 HUD/가이드 설명 데이터를 등록합니다.
    /// </summary>
    /// <summary>
    /// OO_Choice.json을 읽어 선택지가 붙은 상호작용 큐시트를 등록합니다.
    /// 영화 비유로는 일반 대본 옆에 "관객 선택 분기표"를 따로 꽂아 두는 단계입니다.
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
    /// OO_Codex.json을 읽어 도감 항목을 등록합니다.
    /// 영화로 치면 나중에 관객이 다시 펼쳐볼 프로그램북의 항목들을 미리 정리하는 단계입니다.
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

        Debug.Log($"[OOTechGameDataManager] Tutorial 데이터 로드 완료: {_tutorialDic.Count}개");
    }

    /// <summary>
    /// OO_Ingredient.json을 읽어 쌀, 채소 같은 재료 데이터를 등록합니다.
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

        Debug.Log($"[OOTechGameDataManager] Ingredient 데이터 로드 완료: {_ingredientDic.Count}개");
    }

    // ==================== 데이터 생성 ====================

    /// <summary>
    /// OO_Recipe.json을 읽어 재료 조합과 결과 음식 규칙을 등록합니다.
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
    /// OO_CookingTool.json을 읽어 가마솥, 도마 같은 조리도구 역할표를 등록합니다.
    /// 영화 비유로는 소품팀이 만든 실제 조리도구마다 "받을 수 있는 재료" 표를 붙이는 단계입니다.
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
    /// OO_CookingCueSheet.json을 읽어 CookingGroup의 공통 연출 큐를 등록합니다.
    /// 감독 비유로는 부엌 장면의 카메라, 조명, 안내 화살표 타이밍표를 제작 본부에 꽂아 두는 단계입니다.
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
    /// OO_Cook.json을 읽어 완성 음식 데이터를 등록합니다.
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
    /// OO_Stage.json을 읽어 스테이지 이름, 설명, 이동 정보를 등록합니다.
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
    /// OO_StageQuest.json을 읽어 StageGroup HUD에 표시할 임무 데이터를 등록합니다.
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
    /// OO_Stage2CueSheet.json을 읽어 Stage2Group의 연출 큐시트를 등록합니다.
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
    /// OO_Stage3CueSheet.json을 읽어 Stage3Group/EncounterGroup의 연출 큐시트를 등록합니다.
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
    /// JSON 한 줄을 OO_Narration 모델로 변환합니다.
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
    /// JSON 한 줄을 OO_Dialogue 모델로 변환하고 화자 이름을 보정합니다.
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
    /// JSON 한 줄을 여러 화자 대사 묶음 모델로 변환합니다.
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
    /// JSON 한 줄을 튜토리얼 가이드 모델로 변환합니다.
    /// </summary>
    /// <summary>
    /// JSON 한 줄을 OO_Choice 모델로 바꿉니다.
    /// 감독 비유로는 엑셀 큐시트 한 줄을 실제 무대에서 실행할 선택 분기 카드로 옮기는 과정입니다.
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
    /// JSON 행을 도감 카드 데이터로 정리합니다.
    /// 영화로 치면 캐릭터/음식/지역 소개 카드의 제목, 설명, 사진 경로를 한 장의 큐카드로 만드는 단계입니다.
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
    /// JSON 한 줄을 재료 모델로 변환합니다.
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
    /// JSON 한 줄을 레시피 모델로 변환합니다.
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
    /// JSON 한 줄을 조리도구 역할표로 변환합니다.
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
    /// JSON 한 줄을 CookingGroup 큐시트로 변환합니다.
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
    /// JSON 한 줄을 Stage3 큐시트 모델로 변환합니다.
    /// 산군 장면에서 필요한 대사, 선택지, 보상, 연출 시간을 한 번에 정리합니다.
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
    /// JSON 한 줄을 완성 음식 모델로 변환합니다.
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
    /// JSON 한 줄을 스테이지 모델로 변환합니다.
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
    /// JSON 한 줄을 스테이지 임무 모델로 변환합니다.
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
    /// JSON 한 줄을 Stage2 큐시트 모델로 변환합니다.
    /// 무대감독용 엑셀 한 줄을 실제 Stage2Controller가 읽을 큐 카드로 바꿉니다.
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
    /// Dialogue 데이터의 화자 ID를 Character 데이터의 실제 이름으로 바꿉니다.
    /// </summary>
    /// <summary>
    /// Character ID를 화면에 표시할 실제 이름으로 바꿉니다.
    /// 여러 데이터 타입에서 같이 쓰는 배우 이름 캐스팅 보조 함수입니다.
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
    /// Dialogue 행에 화자 칸이 비어 있어도 ID 접두사로 배우 이름표를 추론합니다.
    /// 예: character_Moran_04는 모란 캐릭터 데이터 character_Moran_02를 화자로 사용합니다.
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

    // ==================== 변환 유틸 ====================

    /// <summary>
    /// JsonUtility가 배열을 읽을 수 있도록 배열 JSON을 Items 래퍼로 감쌉니다.
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
    /// Resources/OO_MFC/Data와 Resources/JsonOutput에서 데이터 TextAsset을 찾습니다.
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
    /// 엑셀에서 들어온 숫자 문자열을 float로 바꿉니다.
    /// 속도와 연출 시간처럼 소수점이 필요한 큐시트 값을 읽을 때 사용합니다.
    /// </summary>
    private float ParseFloat(string value)
    {
        string normalizedValue = NormalizeJsonText(value);

        if (string.IsNullOrEmpty(normalizedValue))
            return 0f;

        return float.TryParse(normalizedValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result) ? result : 0f;
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

    // ==================== 데이터 조회 ====================

    /// <summary>
    /// 나레이션 데이터를 ID로 조회합니다.
    /// </summary>
    /// <summary>
    /// ID로 나레이션 데이터를 조회합니다.
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
    /// 캐릭터 데이터를 ID로 조회합니다.
    /// </summary>
    /// <summary>
    /// ID로 캐릭터 데이터를 조회합니다.
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
    /// 캐릭터 대화 데이터를 ID로 조회합니다.
    /// </summary>
    /// <summary>
    /// ID로 대사 데이터를 조회합니다.
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
    /// ID로 동시 대사 그룹 데이터를 조회합니다.
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
    /// 튜토리얼 안내 데이터를 ID로 조회합니다.
    /// </summary>
    /// <summary>
    /// ID로 튜토리얼 데이터를 조회합니다.
    /// </summary>
    /// <summary>
    /// ID로 선택지 데이터를 조회합니다.
    /// Controller는 선택지 문장을 직접 들고 있지 않고 이 창구만 통해 큐시트를 꺼냅니다.
    /// </summary>
    public OO_Choice GetChoiceData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _choiceDic.TryGetValue(id, out OO_Choice data) ? data : null;
    }

    /// <summary>
    /// ID로 도감 항목을 조회합니다.
    /// </summary>
    public OO_Codex GetCodexData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _codexDic.TryGetValue(id, out OO_Codex data) ? data : null;
    }

    /// <summary>
    /// 현재 등록된 모든 도감 항목을 리스트로 반환합니다.
    /// Game View에서는 CodexGroup의 스크롤 목록이 이 데이터를 사용합니다.
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
    /// ID로 재료 데이터를 조회합니다.
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
    /// ID로 레시피 데이터를 조회합니다.
    /// </summary>
    /// <summary>
    /// 재료 데이터가 있는지만 조용히 확인합니다.
    /// 영화로 비유하면 창고에 쌀 배우가 있는지 확인만 하고, 없다고 공연장 전체에 경고 방송을 하지 않는 조회입니다.
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
    /// ID로 조리도구 역할표 데이터를 조회합니다.
    /// </summary>
    public OO_CookingTool GetCookingToolData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _cookingToolDic.TryGetValue(id, out OO_CookingTool data) ? data : null;
    }

    /// <summary>
    /// 조리도구 역할표가 있는지만 조용히 확인합니다.
    /// </summary>
    public bool TryGetCookingToolData(string id, out OO_CookingTool data)
    {
        data = null;

        if (string.IsNullOrEmpty(id))
            return false;

        return _cookingToolDic.TryGetValue(id, out data);
    }

    /// <summary>
    /// ID로 CookingGroup 큐시트 데이터를 조회합니다.
    /// </summary>
    public OO_CookingCueSheet GetCookingCueSheetData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _cookingCueSheetDic.TryGetValue(id, out OO_CookingCueSheet data) ? data : null;
    }

    /// <summary>
    /// ID로 완성 음식 데이터를 조회합니다.
    /// </summary>
    public OO_Cook GetCookData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _cookDic.TryGetValue(id, out OO_Cook data) ? data : null;
    }

    /// <summary>
    /// ID로 스테이지 데이터를 조회합니다.
    /// </summary>
    /// <summary>
    /// 완성 음식 데이터가 있는지만 조용히 확인합니다.
    /// 재료와 완성 음식을 같은 인벤토리 슬롯에 보여줄 때 불필요한 경고 로그를 줄이기 위한 안전 조회입니다.
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
    /// ID로 스테이지 임무 데이터를 조회합니다.
    /// </summary>
    public OO_StageQuest GetStageQuestData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _stageQuestDic.TryGetValue(id, out OO_StageQuest data) ? data : null;
    }

    /// <summary>
    /// ID로 Stage2 큐시트 데이터를 조회합니다.
    /// </summary>
    public OO_Stage2CueSheet GetStage2CueSheetData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _stage2CueSheetDic.TryGetValue(id, out OO_Stage2CueSheet data) ? data : null;
    }

    /// <summary>
    /// ID로 Stage3 큐시트 데이터를 조회합니다.
    /// </summary>
    public OO_Stage3CueSheet GetStage3CueSheetData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _stage3CueSheetDic.TryGetValue(id, out OO_Stage3CueSheet data) ? data : null;
    }

    /// <summary>
    /// 현재 등록된 모든 레시피를 반환합니다.
    /// </summary>
    public List<OO_Recipe> GetRecipeDataList()
    {
        return new List<OO_Recipe>(_recipeDic.Values);
    }

    /// <summary>
    /// 투입된 재료 목록과 완전히 일치하는 레시피를 찾습니다.
    /// </summary>
    public OO_Recipe FindRecipeByIngredientList(List<string> ingredientIdList)
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
