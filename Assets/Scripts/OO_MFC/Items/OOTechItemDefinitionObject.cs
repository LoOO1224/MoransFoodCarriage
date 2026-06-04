// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechItemDefinitionObject.cs
// - 역할: 아이템 오브젝트가 어떤 데이터 ID와 아이콘을 갖는지 표시하는 역할표입니다.
// - 감독 관점: 쌀, 채소, 야채죽 같은 소품에 붙는 라벨입니다.
// - 유지보수 포인트: 이미지와 데이터 ID 연결만 맡기고, 인벤토리 수량 계산은 Model/Manager가 맡게 합니다.
// =============================================================================
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 아이템 하나의 데이터 ID와 아이콘 소품을 들고 있는 하이어라키용 역할표입니다.
/// Game View에서는 인벤토리 슬롯이 이 정보를 읽어 쌀, 채소, 야채죽 아이콘을 표시합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechItemDefinitionObject : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string _itemDataId;
    [SerializeField] private string _displayName;
    [SerializeField] private string _iconResourcePath;

    [Header("Icon")]
    [SerializeField] private Sprite Sprite_Icon;
    [SerializeField] private SpriteRenderer Renderer_Preview;
    [SerializeField] private Image Image_Preview;

    public string ItemDataId => _itemDataId;
    public string DisplayName => _displayName;
    public string IconResourcePath => _iconResourcePath;
    public Sprite IconSprite => Sprite_Icon;

    /// <summary>
    /// 사용자가 프리팹/하이어라키에서 직접 지정한 미리보기 컴포넌트를 자동으로 연결합니다.
    /// </summary>
    public void ResolveReferences()
    {
        if (Renderer_Preview == null)
            Renderer_Preview = GetComponentInChildren<SpriteRenderer>(true);

        if (Image_Preview == null)
            Image_Preview = GetComponentInChildren<Image>(true);
    }

    /// <summary>
    /// 에디터 보수 도구가 아이템 역할표를 처음 세팅할 때 사용합니다.
    /// </summary>
    public void RequestSetupDefinition(string itemDataId, string displayName, string iconResourcePath)
    {
        _itemDataId = itemDataId;
        _displayName = displayName;
        _iconResourcePath = iconResourcePath;
        ResolveReferences();
        RequestRefreshPreview();
    }

    /// <summary>
    /// 인스펙터에서 넣은 Sprite가 있으면 우선 사용하고, 없으면 Resources 경로에서 불러옵니다.
    /// </summary>
    public Sprite ResolveIconSprite()
    {
        if (Sprite_Icon != null)
            return Sprite_Icon;

        if (string.IsNullOrEmpty(_iconResourcePath))
            return null;

        string normalizedPath = NormalizeResourcesPath(_iconResourcePath);
        Sprite resourceSprite = Resources.Load<Sprite>(normalizedPath);

        if (resourceSprite != null)
            return resourceSprite;

#if UNITY_EDITOR
        return LoadEditorSpriteAtPath("Assets/" + normalizedPath);
#else
        return null;
#endif
    }

#if UNITY_EDITOR
    private Sprite LoadEditorSpriteAtPath(string assetPathWithoutExtension)
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

    /// <summary>
    /// 프리팹 미리보기 Renderer/Image에 현재 아이콘을 반영합니다.
    /// </summary>
    public void RequestRefreshPreview()
    {
        ResolveReferences();
        Sprite iconSprite = ResolveIconSprite();

        if (Renderer_Preview != null)
            Renderer_Preview.sprite = iconSprite;

        if (Image_Preview != null)
        {
            Image_Preview.sprite = iconSprite;
            Image_Preview.enabled = iconSprite != null;
        }
    }

    private string NormalizeResourcesPath(string iconResourcePath)
    {
        string normalizedPath = iconResourcePath.Replace("\\", "/");
        normalizedPath = normalizedPath.Replace("Assets/Resources/", string.Empty);

        int extensionIndex = normalizedPath.LastIndexOf('.');

        if (extensionIndex > 0)
            normalizedPath = normalizedPath.Substring(0, extensionIndex);

        return normalizedPath;
    }
}
