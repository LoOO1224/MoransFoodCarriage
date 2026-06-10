// =============================================================================
// OO_MFC 코드 일관화 주석
// - 스크립트: OOTechSceneObject.cs
// - 역할: 여러 그룹에서 재사용하는 씬 컴포넌트/검색 보조 기능입니다.
// - 유지보수: 역할 ID와 씬 오브젝트 이름 기반 검색을 보조하므로 공용 호출 범위를 확인합니다.
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
