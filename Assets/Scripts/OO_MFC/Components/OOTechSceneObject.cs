using UnityEngine;

/// <summary>
/// Gives a scene object a stable role name.
/// Like a casting label on an actor's chair, the label lets a scene director
/// find the right object without storing a long list of inspector references.
/// </summary>
[DisallowMultipleComponent]
public class OOTechSceneObject : MonoBehaviour
{
    [SerializeField] private string _roleId;

    public string RoleId { get { return _roleId; } }

    public T GetRoleComponent<T>() where T : Component
    {
        return GetComponent<T>();
    }
}
