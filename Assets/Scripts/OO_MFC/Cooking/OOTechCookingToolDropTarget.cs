// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechCookingToolDropTarget.cs
// - 역할: 요리 시스템의 입력, 조리도구, 레시피 판정을 담당하는 스크립트입니다.
// - 감독 관점: 부엌 장면에서 재료와 조리도구 배우가 어떤 순서로 만나는지 관리합니다.
// - 유지보수 포인트: 재료 규칙은 데이터와 DropTarget 역할표로 빼고, UI 배치는 CookingUIGroup에서 직접 수정합니다.
// =============================================================================
using UnityEngine;

/// <summary>
/// 조리도구 오브젝트가 어떤 재료를 받을 수 있는지 표시하는 역할표 컴포넌트입니다.
/// Game View에서는 플레이어가 재료를 드래그해서 올려놓을 수 있는 무대 위 소품 판정으로 쓰입니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechCookingToolDropTarget : MonoBehaviour
{
    [Header("Tool Data")]
    [SerializeField] private string _toolId = "Cauldron";
    [SerializeField] private string _displayName = "가마솥";
    [SerializeField] private string[] _acceptedIngredientIdArray = new string[0];
    [SerializeField] private float _dropAreaPadding = 1.18f;

    private SpriteRenderer Renderer_Tool;
    private Collider2D Collider_Tool;

    public string ToolId => _toolId;
    public string DisplayName => _displayName;
    public float DropAreaPadding => _dropAreaPadding;
    public bool HasAcceptedIngredientRule => _acceptedIngredientIdArray != null && _acceptedIngredientIdArray.Length > 0;

    /// <summary>
    /// 에디터 보정 도구나 컨트롤러가 조리도구의 역할표를 갱신할 때 사용합니다.
    /// </summary>
    public void RequestSetupTool(string toolId, string displayName, string[] acceptedIngredientIdArray, float dropAreaPadding)
    {
        _toolId = toolId;
        _displayName = displayName;
        _acceptedIngredientIdArray = acceptedIngredientIdArray != null ? acceptedIngredientIdArray : new string[0];
        _dropAreaPadding = Mathf.Max(1f, dropAreaPadding);
        ResolveReferences();
    }

    /// <summary>
    /// 조리도구에 이미 역할표가 있으면 보존하고, 비어 있을 때만 기본 역할표를 채웁니다.
    /// Game View에서는 감독이 인스펙터에서 추가한 고기/생선/채소 규칙이 batch 보정에 덮이지 않게 합니다.
    /// </summary>
    public void RequestSetupDefaultTool(string toolId, string displayName, string[] defaultAcceptedIngredientIdArray, float dropAreaPadding)
    {
        _toolId = toolId;
        _displayName = displayName;

        if (!HasAcceptedIngredientRule)
            _acceptedIngredientIdArray = defaultAcceptedIngredientIdArray != null ? defaultAcceptedIngredientIdArray : new string[0];

        _dropAreaPadding = Mathf.Max(1f, dropAreaPadding);
        ResolveReferences();
    }

    /// <summary>
    /// 재료 ID가 이 조리도구 역할표에 적힌 재료인지 확인합니다.
    /// </summary>
    public bool CanAcceptIngredient(string itemDataId)
    {
        if (string.IsNullOrEmpty(itemDataId) || _acceptedIngredientIdArray == null)
            return false;

        for (int index = 0; index < _acceptedIngredientIdArray.Length; index++)
        {
            if (_acceptedIngredientIdArray[index] == itemDataId)
                return true;
        }

        return false;
    }

    /// <summary>
    /// SpriteRenderer와 Collider를 찾아 실제 드롭 판정에 사용할 기준점을 준비합니다.
    /// </summary>
    public void ResolveReferences()
    {
        if (Renderer_Tool == null)
            Renderer_Tool = GetComponent<SpriteRenderer>();

        if (Renderer_Tool == null)
            Renderer_Tool = GetComponentInChildren<SpriteRenderer>(true);

        if (Collider_Tool == null)
            Collider_Tool = GetComponent<Collider2D>();

        if (Collider_Tool == null)
            Collider_Tool = GetComponentInChildren<Collider2D>(true);
    }

    /// <summary>
    /// 마우스 화면 좌표가 조리도구의 실제 스프라이트 또는 콜리더 위인지 확인합니다.
    /// </summary>
    public bool IsPointerInside(Vector2 screenPosition, Camera camera)
    {
        ResolveReferences();

        if (camera == null)
            return false;

        Vector3 worldPoint = GetWorldPointOnToolPlane(screenPosition, camera);

        if (Collider_Tool != null && Collider_Tool.OverlapPoint(worldPoint))
            return true;

        Bounds bounds = GetWorldBounds();
        bounds.Expand(bounds.size * Mathf.Max(0f, _dropAreaPadding - 1f));
        worldPoint.z = bounds.center.z;
        return bounds.Contains(worldPoint);
    }

    /// <summary>
    /// UI 드롭 영역과 화살표가 따라갈 월드 기준 Bounds를 반환합니다.
    /// </summary>
    public Bounds GetWorldBounds()
    {
        ResolveReferences();

        if (Collider_Tool != null)
            return Collider_Tool.bounds;

        if (Renderer_Tool != null)
            return Renderer_Tool.bounds;

        return new Bounds(transform.position, Vector3.one);
    }

    private Vector3 GetWorldPointOnToolPlane(Vector2 screenPosition, Camera camera)
    {
        float planeDistance = Mathf.Abs(camera.transform.position.z - transform.position.z);
        Vector3 worldPoint = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, planeDistance));
        worldPoint.z = transform.position.z;
        return worldPoint;
    }
}
