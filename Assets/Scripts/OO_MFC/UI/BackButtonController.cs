// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: BackButtonController.cs
// - 역할: UI 표시와 입력 연결을 담당하는 UI 컴포넌트입니다.
// - 감독 관점: 관객에게 보이는 패널과 버튼의 무대 동선을 담당합니다.
// - 유지보수 포인트: 사용자가 직접 편집할 UI는 하이어라키/프리팹에 두고, 코드에서 즉석 생성하지 않습니다.
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