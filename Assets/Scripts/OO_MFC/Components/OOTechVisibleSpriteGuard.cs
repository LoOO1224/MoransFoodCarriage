// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechVisibleSpriteGuard.cs
// - 역할: 여러 그룹에서 재사용하는 씬 컴포넌트/검색 보조 기능입니다.
// - 유지보수: 역할 ID와 씬 오브젝트 이름 기반 검색을 보조하므로 공용 호출 범위를 확인합니다.
// =============================================================================
using UnityEngine;

/// <summary>
/// SpriteRenderer를 가진 배우가 Game View에서 보이도록 렌더러, 알파, 정렬 레이어, fallback Sprite를 정리합니다.
/// GreedyDuck처럼 게임 진행에 필수인 배우에게 붙이면, 렌더러가 꺼지거나 Sprite가 빠져도 최소한의 화면 표시를 복구합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechVisibleSpriteGuard : MonoBehaviour
{
    [Header("View References")]
    [SerializeField] private SpriteRenderer Renderer_Target;
    [SerializeField] private Sprite Sprite_Fallback;

    [Header("Render Rule")]
    [SerializeField] private string _sortingLayerName = "Characters";
    [SerializeField] private int _minimumSortingOrder = 70;
    [SerializeField] private bool _isForceAlphaOne = true;

    private bool _hasLoggedVisible;

    /// <summary>
    /// 배우가 무대에 올라오는 순간, 자기 SpriteRenderer를 찾아 바로 보이는 상태로 맞춥니다.
    /// </summary>
    private void Awake()
    {
        RequestEnsureVisible();
    }

    /// <summary>
    /// 꺼졌다 켜진 배우도 다시 조명과 의상 상태를 점검합니다.
    /// </summary>
    private void OnEnable()
    {
        _hasLoggedVisible = false;
        RequestEnsureVisible();
    }

    /// <summary>
    /// 에디터 수리 도구나 Controller가 fallback Sprite와 렌더링 규칙을 지정할 때 사용합니다.
    /// </summary>
    public void RequestSetup(SpriteRenderer targetRenderer, Sprite fallbackSprite, string sortingLayerName, int minimumSortingOrder)
    {
        Renderer_Target = targetRenderer;
        Sprite_Fallback = fallbackSprite;
        _sortingLayerName = sortingLayerName;
        _minimumSortingOrder = minimumSortingOrder;
        RequestEnsureVisible();
    }

    /// <summary>
    /// Game View에서 배우가 보이도록 SpriteRenderer 상태를 복구합니다.
    /// 감독 큐로 보면 "GreedyDuck 배우 조명 켜고, 무대 앞쪽으로 보이게 정렬"하는 호출입니다.
    /// </summary>
    public void RequestEnsureVisible()
    {
        ResolveRenderer();

        if (Renderer_Target == null)
        {
            Debug.LogWarning($"[OOTechVisibleSpriteGuard] Renderer missing: {name}", this);
            return;
        }

        Renderer_Target.gameObject.SetActive(true);
        Renderer_Target.enabled = true;

        if (Renderer_Target.sprite == null && Sprite_Fallback != null)
            Renderer_Target.sprite = Sprite_Fallback;

        if (!string.IsNullOrEmpty(_sortingLayerName))
            Renderer_Target.sortingLayerName = _sortingLayerName;

        Renderer_Target.sortingOrder = Mathf.Max(Renderer_Target.sortingOrder, _minimumSortingOrder);

        if (_isForceAlphaOne)
        {
            Color color = Renderer_Target.color;
            color.a = 1f;
            Renderer_Target.color = color;
        }

        LogVisibleStateOnce();
    }

    private void ResolveRenderer()
    {
        if (Renderer_Target != null)
            return;

        Renderer_Target = GetComponent<SpriteRenderer>();

        if (Renderer_Target == null)
            Renderer_Target = GetComponentInChildren<SpriteRenderer>(true);
    }

    private void LogVisibleStateOnce()
    {
        if (_hasLoggedVisible || Renderer_Target == null)
            return;

        _hasLoggedVisible = true;
        string spriteName = Renderer_Target.sprite != null ? Renderer_Target.sprite.name : "NULL";
        Debug.Log($"[OOTechVisibleSpriteGuard] {name} visible. Sprite={spriteName}, Layer={Renderer_Target.sortingLayerName}, Order={Renderer_Target.sortingOrder}", this);
    }
}
