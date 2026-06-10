// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: BackButtonController.cs
// - 역할: UI 오브젝트 참조, 표시 갱신, 버튼 입력 연결을 담당합니다.
// - 유지보수: 씬 Hierarchy 이름으로 런타임 참조를 복구하는 코드가 많아 오브젝트 이름 변경에 주의합니다.
// =============================================================================
using UnityEngine;

public class BackButtonController : MonoBehaviour
{
    [Header("Back Button Settings")]
    [SerializeField] private string _defaultPreviousGroup = "MainMenuGroup";

    private string _previousGroupName;

    // ==================== Public Methods ====================

    /// <summary>
    ///  ׷  մϴ.
    /// CodexGroup  Awake Start ȣϿ մϴ.
    /// </summary>
    public void SetPreviousGroup(string previousGroupName)
    {
        _previousGroupName = previousGroupName;
        Debug.Log($"[BackButtonController]  ׷ : {previousGroupName}");
    }

    /// <summary>
    /// BackButton Ŭ  ȣǴ ޼
    /// UIManagerExtension  ׷ ȯ ûմϴ.
    /// </summary>
    public void OnBackButtonClicked()
    {
        string targetGroup = string.IsNullOrEmpty(_previousGroupName) ?
                            _defaultPreviousGroup : _previousGroupName;

        Debug.Log($"[BackButtonController] ڷΰ ư Ŭ  {targetGroup}");

        // UIManagerExtension  ̺Ʈ 
        UIManagerExtension.OnBackButtonClicked(targetGroup);
    }
}