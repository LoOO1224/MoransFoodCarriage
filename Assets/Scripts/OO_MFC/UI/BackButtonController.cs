using UnityEngine;

public class BackButtonController : MonoBehaviour
{
    [Header("Back Button Settings")]
    [SerializeField] private string _defaultPreviousGroup = "MainMenuGroup";

    private string _previousGroupName;

    // ==================== Public Methods ====================

    /// <summary>
    /// 이전 그룹을 동적으로 설정합니다.
    /// CodexGroup 등에서 Awake나 Start에서 호출하여 사용합니다.
    /// </summary>
    public void SetPreviousGroup(string previousGroupName)
    {
        _previousGroupName = previousGroupName;
        Debug.Log($"[BackButtonController] 이전 그룹 설정됨: {previousGroupName}");
    }

    /// <summary>
    /// BackButton 클릭 시 호출되는 메서드
    /// UIManagerExtension을 통해 그룹 전환을 요청합니다.
    /// </summary>
    public void OnBackButtonClicked()
    {
        string targetGroup = string.IsNullOrEmpty(_previousGroupName) ?
                            _defaultPreviousGroup : _previousGroupName;

        Debug.Log($"[BackButtonController] 뒤로가기 버튼 클릭 → {targetGroup}");

        // UIManagerExtension을 통해 이벤트 전달
        UIManagerExtension.OnBackButtonClicked(targetGroup);
    }
}