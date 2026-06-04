// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechItemCatalogManager.cs
// - 역할: 여러 장면에서 함께 쓰는 공통 Manager입니다.
// - 감독 관점: 각 부서에 공통 창구를 열어 주는 제작 본부입니다.
// - 유지보수 포인트: 특정 장면의 세부 연출을 직접 처리하지 말고, 공통 조회/등록/요청 API만 유지합니다.
// =============================================================================
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 씬/프리팹에 배치된 아이템 역할표를 모아 HUD와 요리 시스템에 전달하는 소품 창고 매니저입니다.
/// 데이터 이름은 GameDataManager에서 읽고, 사용자가 넣은 아이콘은 ItemDefinitionObject에서 읽습니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechItemCatalogManager : MonoBehaviour
{
    public static OOTechItemCatalogManager Inst { get; private set; }

    private readonly Dictionary<string, OOTechItemDefinitionObject> _itemDefinitionDic = new Dictionary<string, OOTechItemDefinitionObject>();
    private readonly Dictionary<string, Sprite> _loadedIconSpriteDic = new Dictionary<string, Sprite>();
    private readonly HashSet<string> _missingIconWarningSet = new HashSet<string>();

    /// <summary>
    /// 하나의 아이템 카탈로그 매니저만 유지하고 씬 안의 아이템 역할표를 캐싱합니다.
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
        RequestCacheItemDefinitionObjects();
    }

    /// <summary>
    /// 씬에 비활성으로 놓인 쌀/채소/야채죽 오브젝트까지 찾아 데이터 ID 사전으로 묶습니다.
    /// </summary>
    public void RequestCacheItemDefinitionObjects()
    {
        _itemDefinitionDic.Clear();
        OOTechItemDefinitionObject[] definitionArray = FindObjectsByType<OOTechItemDefinitionObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (OOTechItemDefinitionObject definitionObject in definitionArray)
        {
            RequestRegisterItemDefinition(definitionObject);
        }

#if UNITY_EDITOR
        RequestCacheItemDefinitionPrefabAssets();
#endif
    }

#if UNITY_EDITOR
    private void RequestCacheItemDefinitionPrefabAssets()
    {
        string[] prefabGuidArray = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/OO_MFC/Props/Items" });

        foreach (string prefabGuid in prefabGuidArray)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefabObject == null)
                continue;

            OOTechItemDefinitionObject definitionObject = prefabObject.GetComponentInChildren<OOTechItemDefinitionObject>(true);
            RequestRegisterItemDefinition(definitionObject);
        }
    }
#endif

    /// <summary>
    /// 새 아이템 역할표를 카탈로그에 등록합니다.
    /// </summary>
    public void RequestRegisterItemDefinition(OOTechItemDefinitionObject definitionObject)
    {
        if (definitionObject == null || string.IsNullOrEmpty(definitionObject.ItemDataId))
            return;

        definitionObject.ResolveReferences();
        if (_itemDefinitionDic.TryGetValue(definitionObject.ItemDataId, out OOTechItemDefinitionObject currentDefinition) &&
            currentDefinition != null &&
            currentDefinition.ResolveIconSprite() != null &&
            definitionObject.ResolveIconSprite() == null)
        {
            return;
        }

        _itemDefinitionDic[definitionObject.ItemDataId] = definitionObject;
    }

    /// <summary>
    /// 데이터 ID에 맞는 아이템 역할표를 반환합니다.
    /// </summary>
    public OOTechItemDefinitionObject GetItemDefinition(string itemDataId)
    {
        if (string.IsNullOrEmpty(itemDataId))
            return null;

        if (_itemDefinitionDic.Count == 0)
            RequestCacheItemDefinitionObjects();

        return _itemDefinitionDic.TryGetValue(itemDataId, out OOTechItemDefinitionObject definitionObject) ? definitionObject : null;
    }

    /// <summary>
    /// 인벤토리 슬롯에 표시할 이름을 데이터 우선으로 가져오고, 없으면 오브젝트 이름표를 사용합니다.
    /// </summary>
    public string GetItemDisplayName(string itemDataId)
    {
        string dataDisplayName = GetDataDisplayName(itemDataId);

        if (!string.IsNullOrEmpty(dataDisplayName))
            return dataDisplayName;

        OOTechItemDefinitionObject definitionObject = GetItemDefinition(itemDataId);

        if (definitionObject != null && !string.IsNullOrEmpty(definitionObject.DisplayName))
            return definitionObject.DisplayName;

        return GetFallbackDisplayName(itemDataId);
    }

    /// <summary>
    /// 인벤토리 슬롯에 표시할 아이콘을 오브젝트 Sprite 우선으로 가져오고, 없으면 데이터 IconPath를 사용합니다.
    /// </summary>
    public Sprite GetItemIconSprite(string itemDataId)
    {
        if (!string.IsNullOrEmpty(itemDataId) && _loadedIconSpriteDic.TryGetValue(itemDataId, out Sprite cachedSprite))
            return cachedSprite;

        OOTechItemDefinitionObject definitionObject = GetItemDefinition(itemDataId);

        if (definitionObject != null)
        {
            Sprite definitionSprite = definitionObject.ResolveIconSprite();

            if (definitionSprite != null)
            {
                _loadedIconSpriteDic[itemDataId] = definitionSprite;
                return definitionSprite;
            }
        }

        Sprite dataSprite = LoadIconSpriteFromData(itemDataId);

        if (dataSprite != null && !string.IsNullOrEmpty(itemDataId))
            _loadedIconSpriteDic[itemDataId] = dataSprite;

        return dataSprite;
    }

    /// <summary>
    /// ItemCatalogManager가 아직 Awake 되기 전이어도 아이콘을 불러오는 공통 입구입니다.
    /// Game View에서는 슬롯 배우가 먼저 올라와도, 제작부 창고(Resources)의 이미지를 직접 찾아 보여줍니다.
    /// </summary>
    public static Sprite RequestItemIconSprite(string itemDataId)
    {
        if (Inst != null)
            return Inst.GetItemIconSprite(itemDataId);

        return RequestLoadIconSpriteFromPath(RequestGetIconPathFromData(itemDataId));
    }

    private string GetDataDisplayName(string itemDataId)
    {
        if (string.IsNullOrEmpty(itemDataId) || OOTechGameDataManager.Inst == null)
            return string.Empty;

        OOTechGameDataManager.Inst.TryGetIngredientData(itemDataId, out OO_Ingredient ingredientData);

        if (ingredientData != null && !string.IsNullOrEmpty(ingredientData.Name))
            return ingredientData.Name;

        OOTechGameDataManager.Inst.TryGetCookData(itemDataId, out OO_Cook cookData);

        if (cookData != null && !string.IsNullOrEmpty(cookData.Name))
            return cookData.Name;

        return string.Empty;
    }

    private Sprite LoadIconSpriteFromData(string itemDataId)
    {
        if (string.IsNullOrEmpty(itemDataId))
            return null;

        string iconPath = RequestGetIconPathFromData(itemDataId);
        Sprite iconSprite = RequestLoadIconSpriteFromPath(iconPath);

        if (iconSprite != null)
            return iconSprite;

        RequestLogMissingIcon(itemDataId, iconPath);
        return null;
    }

    private void RequestLogMissingIcon(string itemDataId, string iconPath)
    {
        if (string.IsNullOrEmpty(itemDataId) || _missingIconWarningSet.Contains(itemDataId))
            return;

        _missingIconWarningSet.Add(itemDataId);
        Debug.LogWarning($"[OOTechItemCatalogManager] Item icon not found. ItemDataId: {itemDataId}, IconPath: {iconPath}");
    }

    private static string RequestGetIconPathFromData(string itemDataId)
    {
        if (string.IsNullOrEmpty(itemDataId) || OOTechGameDataManager.Inst == null)
            return string.Empty;

        OOTechGameDataManager.Inst.TryGetIngredientData(itemDataId, out OO_Ingredient ingredientData);

        if (ingredientData != null && !string.IsNullOrEmpty(ingredientData.IconPath))
            return ingredientData.IconPath;

        OOTechGameDataManager.Inst.TryGetCookData(itemDataId, out OO_Cook cookData);

        if (cookData != null && !string.IsNullOrEmpty(cookData.IconPath))
            return cookData.IconPath;

        return string.Empty;
    }

    private static Sprite RequestLoadIconSpriteFromPath(string iconPath)
    {
        if (string.IsNullOrEmpty(iconPath))
            return null;

        string normalizedPath = NormalizeResourcesPath(iconPath);
        Sprite resourceSprite = RequestLoadSpriteFromResources(normalizedPath);

        if (resourceSprite != null)
            return resourceSprite;

#if UNITY_EDITOR
        Sprite editorSprite = LoadEditorSpriteAtPath(iconPath);

        if (editorSprite != null)
            return editorSprite;

        editorSprite = LoadEditorSpriteAtPath("Assets/" + normalizedPath);

        if (editorSprite != null)
            return editorSprite;

        return LoadEditorSpriteAtPath("Assets/Resources/" + normalizedPath);
#else
        return null;
#endif
    }

    private static Sprite RequestLoadSpriteFromResources(string normalizedPath)
    {
        foreach (string candidatePath in CreateResourcesCandidatePathArray(normalizedPath))
        {
            Sprite resourceSprite = Resources.Load<Sprite>(candidatePath);

            if (resourceSprite != null)
                return resourceSprite;

            Texture2D resourceTexture = Resources.Load<Texture2D>(candidatePath);

            if (resourceTexture != null)
            {
                Rect spriteRect = new Rect(0f, 0f, resourceTexture.width, resourceTexture.height);
                return Sprite.Create(resourceTexture, spriteRect, new Vector2(0.5f, 0.5f), 100f);
            }
        }

        return null;
    }

    private static string[] CreateResourcesCandidatePathArray(string normalizedPath)
    {
        string path = normalizedPath.Replace("\\", "/");
        string fileName = path;
        int slashIndex = fileName.LastIndexOf('/');

        if (slashIndex >= 0 && slashIndex < fileName.Length - 1)
            fileName = fileName.Substring(slashIndex + 1);

        return new[]
        {
            path,
            RemovePathPrefix(path, "Assets/Resources/"),
            RemovePathPrefix(path, "Assets/"),
            RemovePathPrefix(path, "Resources/"),
            "Images/Food/" + fileName
        };
    }

#if UNITY_EDITOR
    private static Sprite LoadEditorSpriteAtPath(string assetPathWithoutExtension)
    {
        if (string.IsNullOrEmpty(assetPathWithoutExtension))
            return null;

        string normalizedPath = assetPathWithoutExtension.Replace("\\", "/");
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(normalizedPath);

        if (sprite != null)
            return sprite;

        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(normalizedPath + ".png");

        if (sprite != null)
            return sprite;

        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(normalizedPath + ".jpg");

        if (sprite != null)
            return sprite;

        return AssetDatabase.LoadAssetAtPath<Sprite>(normalizedPath + ".jpeg");
    }
#endif

    private static string NormalizeResourcesPath(string iconPath)
    {
        string normalizedPath = iconPath.Replace("\\", "/");
        normalizedPath = normalizedPath.Replace("Assets/Resources/", string.Empty);

        int extensionIndex = normalizedPath.LastIndexOf('.');

        if (extensionIndex > 0)
            normalizedPath = normalizedPath.Substring(0, extensionIndex);

        return normalizedPath;
    }

    private static string RemovePathPrefix(string path, string prefix)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(prefix))
            return path;

        return path.StartsWith(prefix) ? path.Substring(prefix.Length) : path;
    }

    private string GetFallbackDisplayName(string itemDataId)
    {
        if (itemDataId == "Ing_Rice_01")
            return "쌀";

        if (itemDataId == "Ing_Veggie_01")
            return "채소";

        if (itemDataId == "Ing_Pumpkin_01")
            return "호박";

        if (itemDataId == "Ing_ChiliPepper_01")
            return "청양고추";

        if (itemDataId == "Ing_Fish_01")
            return "조기";

        if (itemDataId == "Ing_Kimch_01")
            return "김치";

        if (itemDataId == "Ing_Honey_01")
            return "꿀";

        if (itemDataId == "OO_PumpkinSoup_1")
            return "호박죽";

        if (itemDataId == "OO_VegetableSoup_1")
            return "야채죽";

        if (itemDataId == "OO_GrilledFishMeal_1")
            return "조기밥상";

        if (itemDataId == "OO_KimchiStew_1")
            return "김치찌개";

        return string.IsNullOrEmpty(itemDataId) ? "알 수 없는 아이템" : itemDataId;
    }
}
