using UnityEngine;

/// <summary>
/// Owns the build-only road auto-move fallback.
/// Keep this as a separate component so forced road movement can be disabled
/// per road group without touching the main road transition controller.
/// </summary>
[DisallowMultipleComponent]
public class OOTechRoadAutoMoveFailSafe : MonoBehaviour
{
    [SerializeField] private bool _isEnabledInBuild = true;
    [SerializeField] private float _delaySeconds = 1.5f;

    private float _readySeconds;
    private bool _isLogged;
    private bool _isLockCleared;

    public bool IsLockCleared => _isLockCleared;

    public void Configure(bool isEnabledInBuild, float delaySeconds)
    {
        _isEnabledInBuild = isEnabledInBuild;
        _delaySeconds = Mathf.Max(0.1f, delaySeconds);
    }

    public bool RequestAutoMove(bool isMoveInputPressed, bool isChangingMap, bool isRoadTripComplete, GameObject roadActor)
    {
        if (!isActiveAndEnabled || !_isEnabledInBuild || Application.isEditor)
            return false;

        if (isMoveInputPressed || isChangingMap || isRoadTripComplete || roadActor == null)
        {
            ResetState();
            return false;
        }

        _readySeconds += Time.unscaledDeltaTime;

        if (_readySeconds < _delaySeconds)
            return false;

        if (!_isLogged)
        {
            Debug.LogWarning("[OOTechRoadAutoMoveFailSafe] Build auto-move fallback started because road input was not received.");
            _isLogged = true;
        }

        return true;
    }

    public void MarkLockCleared()
    {
        _isLockCleared = true;
    }

    public void ResetState()
    {
        _readySeconds = 0f;
        _isLogged = false;
        _isLockCleared = false;
    }
}
