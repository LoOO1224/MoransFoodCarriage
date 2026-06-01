using UnityEngine;

/// <summary>
/// Compatibility stub for a stale Unity compile entry.
/// The real instructor sample script remains disabled next to this file.
/// </summary>
public class DaniTechResourceManager : MonoBehaviour
{
    public static DaniTechResourceManager Inst { get; private set; }

    private void Awake()
    {
        if (Inst != null && Inst != this)
        {
            Destroy(gameObject);
            return;
        }

        Inst = this;
    }
}
