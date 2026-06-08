// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechDialogueLayout.cs
// - 역할: UI 표시와 입력 연결을 담당하는 UI 컴포넌트입니다.
// - 감독 관점: 관객에게 보이는 패널과 버튼의 무대 동선을 담당합니다.
// - 유지보수 포인트: 사용자가 직접 편집할 UI는 하이어라키/프리팹에 두고, 코드에서 즉석 생성하지 않습니다.
// =============================================================================
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// DialoguePanel의 배치를 담당하는 보조 컴포넌트입니다.
/// 기본 배치는 사용자가 씬에서 잡고, Road View처럼 HUD와 겹치는 장면만 중앙 무대로 살짝 올립니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechDialogueLayout : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private Vector2 _panelSizeDelta = new Vector2(-300f, 250f);
    [SerializeField] private Vector2 _panelAnchoredPosition = new Vector2(0f, 165f);
    [SerializeField] private Vector2 _speakerNameSize = new Vector2(470f, 68f);
    [SerializeField] private Vector2 _speakerNameAnchoredPosition = new Vector2(54f, 18f);
    [SerializeField] private Vector2 _nextButtonSize = new Vector2(270f, 78f);
    [SerializeField] private Vector2 _nextButtonAnchoredPosition = new Vector2(-46f, 32f);

    [Header("Road View Layout")]
    [SerializeField] private Vector2 _roadViewPanelSizeDelta = new Vector2(1320f, 360f);
    [SerializeField] private Vector2 _roadViewPanelAnchoredPosition = new Vector2(0f, 110f);

    private RectTransform Rect_Panel;
    private RectTransform Rect_SpeakerName;
    private RectTransform Rect_NextButton;

    private void Awake()
    {
        // 기본 대화창 배치는 씬에서 수동 연출합니다.
        // 이 컴포넌트는 Road View처럼 별도 요청이 들어올 때만 배치를 보정합니다.
    }

    private void OnEnable()
    {
        // DialogueGroup을 다시 열 때마다 사용자가 잡아 둔 위치를 덮어쓰지 않기 위해 비워 둡니다.
    }

    public void ApplyLayout()
    {
        // 수동 배치 모드: RectTransform 값은 감독이 씬에서 잡아 둔 값을 기준으로 삼습니다.
    }

    /// <summary>
    /// RoadMap에서는 HUD가 하단에 고정되어 있으므로 대화창을 화면 중앙 쪽으로 올립니다.
    /// Game View에서는 이어가기 버튼이 HUD에 가려지지 않게 보입니다.
    /// </summary>
    public void ApplyRoadViewLayout()
    {
        CacheComponentReferences();
        ApplyRoadViewPanelLayout();
        // 화자 이름과 배경판은 씬에서 직접 잡은 상대 위치를 유지합니다.
        // 텍스트만 따로 움직이면 Road View 대화에서 이름과 배경판이 어긋납니다.
        ApplyNextButtonLayout();
    }

    /// <summary>
    /// DialoguePanel, SpeakerNameText, NextButton의 RectTransform을 찾아 둡니다.
    /// </summary>
    private void CacheComponentReferences()
    {
        if (Rect_Panel == null)
            Rect_Panel = transform as RectTransform;

        if (Rect_SpeakerName == null)
        {
            GameObject speakerNameObject = OOTechSceneQuery.RequestChildObjectByName(transform, "SpeakerNameText");
            Transform speakerName = speakerNameObject != null ? speakerNameObject.transform : null;
            Rect_SpeakerName = speakerName as RectTransform;
        }

        if (Rect_NextButton == null)
        {
            Button nextButton = GetComponentInChildren<Button>(true);

            if (nextButton != null)
                Rect_NextButton = nextButton.transform as RectTransform;
        }
    }

    /// <summary>
    /// 기본 대화창 배치 값입니다. 현재는 수동 배치를 보호하기 위해 자동 호출하지 않습니다.
    /// </summary>
    private void ApplyPanelLayout()
    {
        if (Rect_Panel == null)
            return;

        Rect_Panel.anchorMin = new Vector2(0f, 0f);
        Rect_Panel.anchorMax = new Vector2(1f, 0f);
        Rect_Panel.pivot = new Vector2(0.5f, 0.5f);
        Rect_Panel.anchoredPosition = _panelAnchoredPosition;
        Rect_Panel.sizeDelta = _panelSizeDelta;
        Rect_Panel.localScale = Vector3.one;
    }

    /// <summary>
    /// Road View 전용으로 대화창 전체를 중앙 무대 쪽에 배치합니다.
    /// </summary>
    private void ApplyRoadViewPanelLayout()
    {
        if (Rect_Panel == null)
            return;

        Rect_Panel.anchorMin = new Vector2(0.5f, 0.5f);
        Rect_Panel.anchorMax = new Vector2(0.5f, 0.5f);
        Rect_Panel.pivot = new Vector2(0.5f, 0.5f);
        Rect_Panel.anchoredPosition = _roadViewPanelAnchoredPosition;
        Rect_Panel.sizeDelta = _roadViewPanelSizeDelta;
        Rect_Panel.localScale = Vector3.one;
    }

    /// <summary>
    /// 화자 이름표를 대화창 위쪽에 붙여 읽기 쉽게 만듭니다.
    /// </summary>
    private void ApplySpeakerNameLayout()
    {
        if (Rect_SpeakerName == null)
            return;

        Rect_SpeakerName.anchorMin = new Vector2(0f, 1f);
        Rect_SpeakerName.anchorMax = new Vector2(0f, 1f);
        Rect_SpeakerName.pivot = new Vector2(0f, 0f);
        Rect_SpeakerName.anchoredPosition = _speakerNameAnchoredPosition;
        Rect_SpeakerName.sizeDelta = _speakerNameSize;
        Rect_SpeakerName.localScale = Vector3.one;
    }

    /// <summary>
    /// 이어가기 버튼을 대화창 오른쪽 아래에 고정합니다.
    /// </summary>
    private void ApplyNextButtonLayout()
    {
        if (Rect_NextButton == null)
            return;

        Rect_NextButton.anchorMin = new Vector2(1f, 0f);
        Rect_NextButton.anchorMax = new Vector2(1f, 0f);
        Rect_NextButton.pivot = new Vector2(1f, 0f);
        Rect_NextButton.anchoredPosition = _nextButtonAnchoredPosition;
        Rect_NextButton.sizeDelta = _nextButtonSize;
        Rect_NextButton.localScale = Vector3.one;
    }
}
