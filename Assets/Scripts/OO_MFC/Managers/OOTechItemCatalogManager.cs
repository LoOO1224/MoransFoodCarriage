using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬/프리팹에 배치된 아이템 역할표를 모아 HUD와 요리 시스템에 전달하는 소품 창고 매니저입니다.
/// 데이터 이름은 GameDataManager에서 읽고, 사용자가 넣은 아이콘은 ItemDefinitionObject에서 읽습니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechItemCatalogManager : MonoBehaviour
{
    public static OOTechItemCatalogManager Inst { get; private set; }

    private readonly Dictionary<string, OOTechItemDefinitionObject> _itemDefinitionDic = new Dictionary<string, OOTechItemDefinitionObject>();

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
    /// 씬에 비활성으로 놓인 쌀/채소/채소죽 오브젝트까지 찾아 데이터 ID 사전으로 묶습니다.
    /// </summary>
    public void RequestCacheItemDefinitionObjects()
    {
        _itemDefinitionDic.Clear();
        OOTechItemDefinitionObject[] definitionArray = FindObjectsByType<OOTechItemDefinitionObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (OOTechItemDefinitionObject definitionObject in definitionArray)
        {
            RequestRegisterItemDefinition(definitionObject);
        }
    }

    /// <summary>
    /// 새 아이템 역할표를 카탈로그에 등록합니다.
    /// </summary>
    public void RequestRegisterItemDefinition(OOTechItemDefinitionObject definitionObject)
    {
        if (definitionObject == null || string.IsNullOrEmpty(definitionObject.ItemDataId))
            return;

        definitionObject.ResolveReferences();
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
        OOTechItemDefinitionObject definitionObject = GetItemDefinition(itemDataId);

        if (definitionObject != null)
        {
            Sprite definitionSprite = definitionObject.ResolveIconSprite();

            if (definitionSprite != null)
                return definitionSprite;
        }

        return LoadIconSpriteFromData(itemDataId);
    }

    private string GetDataDisplayName(string itemDataId)
    {
        if (string.IsNullOrEmpty(itemDataId) || OOTechGameDataManager.Inst == null)
            return string.Empty;

        OO_Ingredient ingredientData = OOTechGameDataManager.Inst.GetIngredientData(itemDataId);

        if (ingredientData != null && !string.IsNullOrEmpty(ingredientData.Name))
            return ingredientData.Name;

        OO_Cook cookData = OOTechGameDataManager.Inst.GetCookData(itemDataId);

        if (cookData != null && !string.IsNullOrEmpty(cookData.Name))
            return cookData.Name;

        return string.Empty;
    }

    private Sprite LoadIconSpriteFromData(string itemDataId)
    {
        if (string.IsNullOrEmpty(itemDataId) || OOTechGameDataManager.Inst == null)
            return null;

        string iconPath = string.Empty;
        OO_Ingredient ingredientData = OOTechGameDataManager.Inst.GetIngredientData(itemDataId);

        if (ingredientData != null)
            iconPath = ingredientData.IconPath;

        if (string.IsNullOrEmpty(iconPath))
        {
            OO_Cook cookData = OOTechGameDataManager.Inst.GetCookData(itemDataId);

            if (cookData != null)
                iconPath = cookData.IconPath;
        }

        if (string.IsNullOrEmpty(iconPath))
            return null;

        return Resources.Load<Sprite>(NormalizeResourcesPath(iconPath));
    }

    private string NormalizeResourcesPath(string iconPath)
    {
        string normalizedPath = iconPath.Replace("\\", "/");
        normalizedPath = normalizedPath.Replace("Assets/Resources/", string.Empty);

        int extensionIndex = normalizedPath.LastIndexOf('.');

        if (extensionIndex > 0)
            normalizedPath = normalizedPath.Substring(0, extensionIndex);

        return normalizedPath;
    }

    private string GetFallbackDisplayName(string itemDataId)
    {
        if (itemDataId == "Ing_Rice_01")
            return "쌀";

        if (itemDataId == "Ing_Veggie_01" || itemDataId == "Ing_Pumpkin_01")
            return "채소";

        if (itemDataId == "OO_Cook_1")
            return "채소죽";

        return string.IsNullOrEmpty(itemDataId) ? "알 수 없는 아이템" : itemDataId;
    }
}
