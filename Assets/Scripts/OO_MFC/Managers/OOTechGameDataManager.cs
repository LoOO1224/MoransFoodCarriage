using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OOTechGameDataManager
/// 게임에서 사용하는 Static Data Json을 로드하고 조회하는 매니저입니다.
/// 런타임 중 저장되는 인스턴스 데이터는 GameManager의 Model에서 관리하고,
/// 이 클래스는 변하지 않는 표 데이터만 담당합니다.
/// </summary>
public class OOTechGameDataManager : MonoBehaviour
{
    public static OOTechGameDataManager Inst { get; private set; }

    // ==================== 데이터 Dictionary ====================
    private readonly Dictionary<string, OO_Narration> _narrationDic = new Dictionary<string, OO_Narration>();
    private readonly Dictionary<string, OO_Character> _characterDic = new Dictionary<string, OO_Character>();

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
    /// 게임 시작 시 필요한 모든 Static Data를 로드합니다.
    /// </summary>
    public void LoadAllData()
    {
        Debug.Log("[OOTechGameDataManager] 모든 데이터 로드 시작");

        LoadNarrationData();
        LoadCharacterData();

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
            return;

        OOTechCharacterJsonWrapper wrapper = JsonUtility.FromJson<OOTechCharacterJsonWrapper>(WrapJsonArray(jsonFile.text));

        _characterDic.Clear();

        if (wrapper == null || wrapper.Items == null)
            return;

        foreach (OO_Character characterData in wrapper.Items)
        {
            if (characterData == null || string.IsNullOrEmpty(characterData.Id))
                continue;

            _characterDic[characterData.Id] = characterData;
        }

        Debug.Log($"[OOTechGameDataManager] Character 데이터 로드 완료: {_characterDic.Count}개");
    }

    private string WrapJsonArray(string jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText))
            return "{\"Items\":[]}";

        string trimmedJsonText = jsonText.Trim();

        if (trimmedJsonText.StartsWith("{"))
            return trimmedJsonText;

        return "{\"Items\":" + trimmedJsonText + "}";
    }

    private OO_Narration CreateNarrationData(OOTechNarrationJsonData jsonData)
    {
        if (jsonData == null)
            return null;

        OO_Narration narrationData = new OO_Narration
        {
            Id = jsonData.Id,
            Title = jsonData.Title,
            PartNumber = ParseInt(jsonData.PartNumber),
            NarrationTexts = CreateTextList(jsonData.NarrationTexts),
            BackgroundImagePaths = CreateBackgroundImagePathList(jsonData),
            BGMPath = jsonData.BGMPath,
            NextGroup = jsonData.NextGroup
        };

        return narrationData;
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
