using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 공용 Next / Skip 버튼 컨트롤러입니다.
/// 현재 그룹을 닫고 다음 그룹을 여는 단순 전환만 담당하며,
/// 모든 UI 전환은 OOTechUIManager를 통해 처리합니다.
/// </summary>
public class NextButtonController : MonoBehaviour
{
    [Header("Group Names")]
    [SerializeField] private string _currentGroupName;
    [SerializeField] private string _nextGroupName;

    [Header("UI Reference")]
    [SerializeField] private Button Button_Next;

    [FormerlySerializedAs("_buttonText")]
    [SerializeField] private TextMeshProUGUI Text_Button;

    private void OnEnable()
    {
        BindButtonEvent();
    }

    private void OnDisable()
    {
        UnbindButtonEvent();
    }

    // ==================== 버튼 바인딩 ====================

    private void BindButtonEvent()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(OnNextButtonClicked);
        Button_Next.onClick.AddListener(OnNextButtonClicked);
    }

    private void UnbindButtonEvent()
    {
        if (Button_Next == null)
            return;

        Button_Next.onClick.RemoveListener(OnNextButtonClicked);
    }

    // ==================== Public 설정 ====================

    /// <summary>
    /// 프리팹 인스턴스를 코드에서 재사용할 때 현재 그룹과 다음 그룹 이름을 설정합니다.
    /// </summary>
    public void SetGroupName(string currentGroupName, string nextGroupName)
    {
        _currentGroupName = currentGroupName;
        _nextGroupName = nextGroupName;
    }

    /// <summary>
    /// 버튼 표시 텍스트를 변경합니다.
    /// </summary>
    public void SetButtonText(string buttonText)
    {
        if (Text_Button != null)
            Text_Button.text = buttonText;
    }

    // ==================== 버튼 이벤트 ====================

    /// <summary>
    /// 버튼 클릭 시 현재 그룹을 닫고 다음 그룹을 엽니다.
    /// DialogueGroup처럼 현재 그룹에 붙어 있는 보조 UI는 해당 그룹 컨트롤러의 OnDisable에서 정리합니다.
    /// </summary>
    public void OnNextButtonClicked()
    {
        Debug.Log($"[NextButtonController] 버튼 클릭 → {_nextGroupName} (현재: {_currentGroupName})");

        if (OOTechUIManager.Inst == null)
        {
            Debug.LogWarning("[NextButtonController] OOTechUIManager를 찾을 수 없습니다.");
            return;
        }

        if (!string.IsNullOrEmpty(_currentGroupName))
            OOTechUIManager.Inst.CloseUI(_currentGroupName);

        if (!string.IsNullOrEmpty(_nextGroupName))
            OOTechUIManager.Inst.OpenUI(_nextGroupName);
    }
}
