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
    private readonly Dictionary<string, OO_Tutorial> _tutorialDic = new Dictionary<string, OO_Tutorial>();
    private readonly Dictionary<string, OO_Ingredient> _ingredientDic = new Dictionary<string, OO_Ingredient>();
    private readonly Dictionary<string, OO_Recipe> _recipeDic = new Dictionary<string, OO_Recipe>();
    private readonly Dictionary<string, OO_Cook> _cookDic = new Dictionary<string, OO_Cook>();
    private readonly Dictionary<string, OO_Stage> _stageDic = new Dictionary<string, OO_Stage>();
    private readonly Dictionary<string, OO_StageQuest> _stageQuestDic = new Dictionary<string, OO_StageQuest>();

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
        LoadTutorialData();
        LoadIngredientData();
        LoadRecipeData();
        LoadCookData();
        LoadStageData();
        LoadStageQuestData();

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
            RequiredTool = NormalizeJsonText(jsonData.RequiredTool)
        };

        return recipeData;
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
    /// Dialogue 데이터의 화자 ID를 Character 데이터의 실제 이름으로 바꿉니다.
    /// </summary>
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
    public OO_Recipe GetRecipeData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _recipeDic.TryGetValue(id, out OO_Recipe data) ? data : null;
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
            if (recipeData != null && IsSameIngredientSet(recipeData.RequiredIngredients, ingredientIdList))
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
