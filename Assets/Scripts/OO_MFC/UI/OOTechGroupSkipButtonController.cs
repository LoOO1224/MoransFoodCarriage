using UnityEngine;

/// <summary>
/// 특정 UI 그룹이 켜졌을 때 CommonSkipButton 프리팹을 우측 상단에 표시하는 보조 컨트롤러입니다.
/// Prologue2Group처럼 그룹마다 다음 목적지가 다른 경우, 인스펙터 값만 바꿔 재사용합니다.
/// </summary>
public class OOTechGroupSkipButtonController : MonoBehaviour
{
    [Header("Common Skip Button")]
    [SerializeField] private GameObject Prefab_CommonSkipButton;

    [Header("Group Names")]
    [SerializeField] private string _currentGroupName = "Prologue2Group";
    [SerializeField] private string _nextGroupName = "Senario1Group";

    [Header("View")]
    [SerializeField] private string _buttonText = "넘어가기";
    [SerializeField] private bool _isShowOnEnable = true;

    // ==================== 런타임 생성 객체 ====================
    private GameObject Object_CommonSkipButton;
    private NextButtonController Button_CommonSkip;

    private void OnEnable()
    {
        if (_isShowOnEnable)
            ShowButton();
    }

    private void OnDisable()
    {
        HideButton();
    }

    // ==================== 버튼 표시 ====================

    /// <summary>
    /// CommonSkipButton을 생성 또는 재사용해서 현재 그룹의 다음 이동 버튼으로 표시합니다.
    /// </summary>
    public void ShowButton()
    {
        CreateButtonIfNeeded();

        if (Object_CommonSkipButton == null)
            return;

        Object_CommonSkipButton.SetActive(true);

        if (Button_CommonSkip != null)
        {
            Button_CommonSkip.SetGroupName(_currentGroupName, _nextGroupName);
            Button_CommonSkip.SetButtonText(_buttonText);
        }
    }

    /// <summary>
    /// 현재 그룹이 꺼질 때 생성된 넘어가기 버튼을 함께 숨깁니다.
    /// </summary>
    public void HideButton()
    {
        if (Object_CommonSkipButton != null)
            Object_CommonSkipButton.SetActive(false);
    }

    private void CreateButtonIfNeeded()
    {
        if (Object_CommonSkipButton != null)
            return;

        if (Prefab_CommonSkipButton == null)
        {
            Debug.LogError("[OOTechGroupSkipButtonController] CommonSkipButton 프리팹이 연결되어 있지 않습니다.");
            return;
        }

        Object_CommonSkipButton = Instantiate(Prefab_CommonSkipButton, transform, false);
        Object_CommonSkipButton.name = "CommonSkipButton";
        Button_CommonSkip = Object_CommonSkipButton.GetComponent<NextButtonController>();
        Object_CommonSkipButton.SetActive(false);
    }
}
