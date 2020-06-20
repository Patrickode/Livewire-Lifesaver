using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private bool jumpHeld = false;

    public void GetMoveInput(InputAction.CallbackContext context)
    {
        //Get the direction of move input, then assign that direction to the X and Z of a vector3.
        Vector2 direction = context.ReadValue<Vector2>();
        EventDispatcher.Dispatch(new EventDefiner.MoveInput(new Vector3(direction.x, 0, direction.y)));
    }

    public void GetJumpInput(InputAction.CallbackContext context)
    {
        //If jump input was started this frame, jump is held.
        if (context.started)
        {
            jumpHeld = true;
        }
        //Once jump input is released, jump is no longer held.
        else if (context.performed)
        {
            jumpHeld = false;
        }

        //Note whether jump input started this frame, and whether jump input is held this frame.
        EventDispatcher.Dispatch(new EventDefiner.JumpInput(context.started, jumpHeld));
    }
}