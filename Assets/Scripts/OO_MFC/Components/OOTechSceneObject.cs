// =============================================================================
// OO_MFC 역할 주석
// - 스크립트: OOTechSceneObject.cs
// - 역할: 오브젝트에 붙는 작은 역할표 컴포넌트입니다.
// - 감독 관점: 감독이 배우를 이름으로 직접 붙잡지 않고 역할 ID로 찾게 해주는 이름표입니다.
// - 유지보수 포인트: 새 기능을 넣기보다 역할 식별과 참조 보조처럼 작은 책임만 유지합니다.
// =============================================================================
using UnityEngine;

/// <summary>
/// 씬 오브젝트에 안정적인 역할 이름을 붙입니다.
/// 배우 의자에 붙은 이름표처럼, 감독 스크립트가 긴 참조 목록 없이 올바른 배우를 찾게 합니다.
/// </summary>
[DisallowMultipleComponent]
public class OOTechSceneObject : MonoBehaviour
{
    [SerializeField] private string _roleId;

    public string RoleId { get { return _roleId; } }

    /// <summary>
    /// 에디터 수리 도구나 초기 세팅 코드가 배우의 역할 이름표를 붙일 때 사용합니다.
    /// </summary>
    public void RequestSetRoleId(string roleId)
    {
        _roleId = roleId;
    }

    /// <summary>
    /// 이 배우에게 붙은 지정 타입 컴포넌트를 반환합니다.
    /// </summary>
    public T GetRoleComponent<T>() where T : Component
    {
        return GetComponent<T>();
    }
}
