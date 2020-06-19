using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public void GetMoveInput(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        EventDispatcher.Dispatch(new EventDefiner.MoveInput(new Vector3(direction.x, 0, direction.y)));
    }

    public void GetJumpInput(InputAction.CallbackContext context)
    {
        EventDispatcher.Dispatch(new EventDefiner.JumpInput(context.ReadValueAsButton()));
    }
}
