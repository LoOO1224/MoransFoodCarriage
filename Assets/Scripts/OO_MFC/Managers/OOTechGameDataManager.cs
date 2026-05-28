using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OO_MFC에서 사용하는 Static Data Json을 로드하고 조회하는 매니저입니다.
/// Json 파일은 Resources/OO_MFC/Data 아래에 두며, 런타임 진행 데이터는 GameManager의 Model에서 따로 관리합니다.
/// </summary>
public class OOTechGameDataManager : MonoBehaviour
{
    public static OOTechGameDataManager Inst { get; private set; }

    // ==================== 데이터 Dictionary ====================
    private readonly Dictionary<string, OO_Narration> _narrationDic = new Dictionary<string, OO_Narration>();
    private readonly Dictionary<string, OO_Character> _characterDic = new Dictionary<string, OO_Character>();
    private readonly Dictionary<string, OO_Dialogue> _dialogueDic = new Dictionary<string, OO_Dialogue>();
    private readonly Dictionary<string, OO_Tutorial> _tutorialDic = new Dictionary<string, OO_Tutorial>();

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
        LoadTutorialData();

        Debug.Log("[OOTechGameDataManager] 모든 데이터 로드 완료");
    }

    private void LoadNarrationData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("OO_MFC/Data/OO_Narration");

        if (jsonFile == null)
        {
            Debug.LogWarning("[OOTechGameDataManager] OO_Narration.json 파일을 찾을 수 없습니다. 경로: Assets/Resources/OO_MFC/Data/OO_Narration.json");
            return;
        }

        OOTechNarrationJsonWrapper wrapper = JsonUtility.FromJson<OOTechNarrationJsonWrapper>(WrapJsonArray(jsonFile.text));

        _narrationDic.Clear();

        if (wrapper == null || wrapper.Items == null)
        {
            Debug.LogWarning("[OOTechGameDataManager] OO_Narration.json 데이터가 비어 있습니다.");
            return;
        }

        foreach (OOTechNarrationJsonData jsonData in wrapper.Items)
        {
            OO_Narration narrationData = CreateNarrationData(jsonData);

            if (narrationData == null || string.IsNullOrEmpty(narrationData.Id))
                continue;

            _narrationDic[narrationData.Id] = narrationData;
        }

        Debug.Log($"[OOTechGameDataManager] Narration 데이터 로드 완료: {_narrationDic.Count}개");
    }

    private void LoadCharacterData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("OO_MFC/Data/OO_Character");

        if (jsonFile == null)
        {
            Debug.LogWarning("[OOTechGameDataManager] OO_Character.json 파일을 찾을 수 없습니다.");
            return;
        }

        OOTechCharacterJsonWrapper wrapper = JsonUtility.FromJson<OOTechCharacterJsonWrapper>(WrapJsonArray(jsonFile.text));

        _characterDic.Clear();

        if (wrapper == null || wrapper.Items == null)
            return;

        foreach (OO_Character characterData in wrapper.Items)
        {
            if (characterData == null || string.IsNullOrEmpty(characterData.Id))
                continue;

            characterData.Name = NormalizeJsonText(characterData.Name);
            characterData.Description = NormalizeJsonText(characterData.Description);
            _characterDic[characterData.Id] = characterData;
        }

        Debug.Log($"[OOTechGameDataManager] Character 데이터 로드 완료: {_characterDic.Count}개");
    }

    private void LoadDialogueData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("OO_MFC/Data/OO_Dialogue");

        if (jsonFile == null)
        {
            Debug.LogWarning("[OOTechGameDataManager] OO_Dialogue.json 파일을 찾을 수 없습니다.");
            return;
        }

        OOTechDialogueJsonWrapper wrapper = JsonUtility.FromJson<OOTechDialogueJsonWrapper>(WrapJsonArray(jsonFile.text));

        _dialogueDic.Clear();

        if (wrapper == null || wrapper.Items == null)
        {
            Debug.LogWarning("[OOTechGameDataManager] OO_Dialogue.json 데이터가 비어 있습니다.");
            return;
        }

        foreach (OOTechDialogueJsonData jsonData in wrapper.Items)
        {
            OO_Dialogue dialogueData = CreateDialogueData(jsonData);

            if (dialogueData == null || string.IsNullOrEmpty(dialogueData.Id))
                continue;

            _dialogueDic[dialogueData.Id] = dialogueData;
        }

        Debug.Log($"[OOTechGameDataManager] Dialogue 데이터 로드 완료: {_dialogueDic.Count}개");
    }

    private void LoadTutorialData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("OO_MFC/Data/OO_Tutorial");

        if (jsonFile == null)
        {
            Debug.LogWarning("[OOTechGameDataManager] OO_Tutorial.json 파일을 찾을 수 없습니다.");
            return;
        }

        OOTechTutorialJsonWrapper wrapper = JsonUtility.FromJson<OOTechTutorialJsonWrapper>(WrapJsonArray(jsonFile.text));

        _tutorialDic.Clear();

        if (wrapper == null || wrapper.Items == null)
        {
            Debug.LogWarning("[OOTechGameDataManager] OO_Tutorial.json 데이터가 비어 있습니다.");
            return;
        }

        foreach (OOTechTutorialJsonData jsonData in wrapper.Items)
        {
            OO_Tutorial tutorialData = CreateTutorialData(jsonData);

            if (tutorialData == null || string.IsNullOrEmpty(tutorialData.Id))
                continue;

            _tutorialDic[tutorialData.Id] = tutorialData;
        }

        Debug.Log($"[OOTechGameDataManager] Tutorial 데이터 로드 완료: {_tutorialDic.Count}개");
    }

    // ==================== 데이터 생성 ====================

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

    private string ResolveDialogueSpeakerName(OOTechDialogueJsonData jsonData)
    {
        string speakerId = NormalizeJsonText(GetFirstNotEmpty(jsonData.SpeakerCharacterId, jsonData.CharacterId));
        string speakerName = NormalizeJsonText(GetFirstNotEmpty(jsonData.SpeakerName, jsonData.Name));

        if (!string.IsNullOrEmpty(speakerId) && _characterDic.TryGetValue(speakerId, out OO_Character speakerData))
            speakerName = NormalizeJsonText(speakerData.Name);

        if (string.IsNullOrEmpty(speakerName) && !string.IsNullOrEmpty(jsonData.Id) && _characterDic.TryGetValue(jsonData.Id, out OO_Character sameIdCharacterData))
            speakerName = NormalizeJsonText(sameIdCharacterData.Name);

        return speakerName;
    }

    // ==================== 변환 유틸 ====================

    private string WrapJsonArray(string jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText))
            return "{\"Items\":[]}";

        string trimmedJsonText = jsonText.Trim();

        if (trimmedJsonText.StartsWith("{"))
            return trimmedJsonText;

        return "{\"Items\":" + trimmedJsonText + "}";
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
    /// 튜토리얼 안내 데이터를 ID로 조회합니다.
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
