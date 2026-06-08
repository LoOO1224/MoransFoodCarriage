// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechDialogueSpeakerNameBackdrop.cs
// - 역할: UI 표시와 입력 연결을 담당하는 UI 컴포넌트입니다.
// - 감독 관점: 관객에게 보이는 패널과 버튼의 무대 동선을 담당합니다.
// - 유지보수 포인트: 사용자가 직접 편집할 UI는 하이어라키/프리팹에 두고, 코드에서 즉석 생성하지 않습니다.
// =============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// DialoguePanel의 SpeakerNameText 뒤에 흰색 반투명 이름표를 붙입니다.
/// 화자 이름은 검은색으로 바꿔 어떤 배경 위에서도 배우 이름이 잘 보이게 합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechDialogueSpeakerNameBackdrop : MonoBehaviour
{
    [SerializeField] private string _speakerNameObjectName = "SpeakerNameText";
    [SerializeField] private Color _backdropColor = new Color(1f, 0.96f, 0.86f, 0.78f);
    [SerializeField] private Color _narrationBackdropColor = new Color(1f, 0.93f, 0.70f, 0.88f);
    [SerializeField] private Color _characterBackdropColor = new Color(1f, 0.96f, 0.86f, 0.78f);
    [SerializeField] private Color _speakerNameColor = Color.black;
    [SerializeField] private Vector2 _padding = new Vector2(32f, 18f);

    private RectTransform Rect_SpeakerName;
    private TextMeshProUGUI Text_SpeakerName;
    private string _currentSpeakerName;

    /// <summary>
    /// 대화창이 처음 준비될 때 이름표 배경을 생성하거나 연결합니다.
    /// </summary>
    private void Awake()
    {
        PrepareBackdrop();
    }

    /// <summary>
    /// DialogueGroup이 다시 열릴 때 이름표 스타일과 위치를 다시 맞춥니다.
    /// </summary>
    private void OnEnable()
    {
        PrepareBackdrop();
        ApplyBackdropLayout();
    }

    /// <summary>
    /// SpeakerNameText가 움직여도 배경판이 같은 위치를 따라가게 합니다.
    /// </summary>
    private void LateUpdate()
    {
        if (Text_SpeakerName != null && _currentSpeakerName != Text_SpeakerName.text)
            _currentSpeakerName = Text_SpeakerName.text;

        ApplyBackdropLayout();
    }

    /// <summary>
    /// 현재 화자 이름에 맞춰 이름표 색을 바꿉니다.
    /// 영화로 치면 나레이션 마이크와 배우 대사 마이크에 서로 다른 조명을 주는 작업입니다.
    /// </summary>
    public void RequestApplySpeakerName(string speakerName)
    {
        _currentSpeakerName = speakerName ?? string.Empty;
        _backdropColor = ResolveBackdropColor(_currentSpeakerName);
        ApplyBackdropLayout();
    }

    /// <summary>
    /// SpeakerNameText를 찾고, 이전 작업에서 만들어진 중복 이름표 배경을 끕니다.
    /// 영화로 치면 배우 이름표를 두 장 겹쳐 붙이지 않도록 소품팀이 남은 종이를 걷어내는 과정입니다.
    /// </summary>
    private void PrepareBackdrop()
    {
        ApplyRequestedStyle();
        CacheSpeakerNameReference();
        DisableGeneratedBackdropObjects();
    }

    /// <summary>
    /// 이름표로 쓰는 SpeakerNameText 참조를 찾습니다.
    /// </summary>
    private void CacheSpeakerNameReference()
    {
        if (Rect_SpeakerName != null)
            return;

        GameObject speakerNameObject = OOTechSceneQuery.RequestChildObjectByName(transform, _speakerNameObjectName);
        Transform speakerNameTransform = speakerNameObject != null ? speakerNameObject.transform : null;

        if (speakerNameTransform == null)
            speakerNameTransform = GetComponentInChildren<RectTransform>(true);

        if (speakerNameTransform != null && speakerNameTransform.name == _speakerNameObjectName)
            Rect_SpeakerName = speakerNameTransform as RectTransform;

        if (Rect_SpeakerName != null)
            Text_SpeakerName = Rect_SpeakerName.GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// 화자 이름 색상만 안정적으로 적용합니다.
    /// 배경판은 씬에 배치된 기존 UI를 쓰고, 코드가 새 오브젝트를 만들지 않습니다.
    /// </summary>
    private void ApplyBackdropLayout()
    {
        if (Text_SpeakerName != null)
            Text_SpeakerName.color = _speakerNameColor;
    }

    /// <summary>
    /// 예전 런타임 생성 코드가 남긴 Image_SpeakerNameBackdrop을 끕니다.
    /// 사용자가 배치한 원래 SpeakerNameText 구조는 건드리지 않습니다.
    /// </summary>
    private void DisableGeneratedBackdropObjects()
    {
        Transform[] childTransformArray = GetComponentsInChildren<Transform>(true);

        foreach (Transform childTransform in childTransformArray)
        {
            if (childTransform == null || childTransform.name != "Image_SpeakerNameBackdrop")
                continue;

            childTransform.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 사용자가 요청한 이름표 스타일을 기본값으로 강제합니다.
    /// </summary>
    private void ApplyRequestedStyle()
    {
        _speakerNameColor = Color.black;
    }

    private Color ResolveBackdropColor(string speakerName)
    {
        if (IsNarrationSpeaker(speakerName))
            return _narrationBackdropColor;

        return _characterBackdropColor;
    }

    private bool IsNarrationSpeaker(string speakerName)
    {
        if (string.IsNullOrWhiteSpace(speakerName))
            return true;

        string normalizedName = speakerName.Trim().ToLowerInvariant();
        return normalizedName.Contains("나레이션")
            || normalizedName.Contains("narration")
            || normalizedName.Contains("narrator")
            || normalizedName.Contains("해설");
    }
}
