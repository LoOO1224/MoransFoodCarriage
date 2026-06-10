using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Reads road movement input from both Legacy Input Manager and the New Input System.
/// Road transition controllers ask this component whether the player is trying to
/// move right, instead of owning input backend details directly.
/// </summary>
[DisallowMultipleComponent]
public class OOTechRoadMoveInputReader : MonoBehaviour
{
    [SerializeField] private KeyCode _moveRightKey = KeyCode.D;
    [SerializeField] private KeyCode _alternateMoveRightKey = KeyCode.RightArrow;

    public bool IsMoveRightPressed()
    {
        return IsLegacyMoveRightPressed() || IsNewInputMoveRightPressed();
    }

    private bool IsLegacyMoveRightPressed()
    {
        try
        {
            return Input.GetKey(_moveRightKey) ||
                   Input.GetKey(_alternateMoveRightKey) ||
                   Input.GetKey(KeyCode.D) ||
                   Input.GetKey(KeyCode.RightArrow) ||
                   Input.GetAxisRaw("Horizontal") > 0.1f;
        }
        catch (System.InvalidOperationException)
        {
            return false;
        }
    }

    private bool IsNewInputMoveRightPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return false;

        return keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
#else
        return false;
#endif
    }
}
