using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private bool jumpHeld = false;
    private bool boostHeld = false;

    /// <summary>
    /// Checks if an input is held using a callback context.
    /// </summary>
    /// <param name="resultContainer">The bool to store the result in, if there is a result.</param>
    /// <param name="contextToCheck">The context to use to check if the input is held.</param>
    private void CheckIfHeld(ref bool resultContainer, InputAction.CallbackContext contextToCheck)
    {
        //If input was started this frame, the input is held for now.
        if (contextToCheck.started)
        {
            resultContainer = true;
        }
        //Once input is released, input is no longer held.
        else if (contextToCheck.performed)
        {
            resultContainer = false;
        }
    }

    public void GetMoveInput(InputAction.CallbackContext context)
    {
        //Get the direction of move input, then assign that direction to the X and Z of a vector3.
        Vector2 direction = context.ReadValue<Vector2>();
        EventDispatcher.Dispatch(new EventDefiner.MoveInput(new Vector3(direction.x, 0, direction.y)));
    }

    public void GetJumpInput(InputAction.CallbackContext context)
    {
        //Check if jump is held, and store the result in jumpHeld.
        CheckIfHeld(ref jumpHeld, context);

        //Send info about whether jump input started this frame, and whether jump input is held this frame.
        EventDispatcher.Dispatch(new EventDefiner.JumpInput(context.started, jumpHeld));
    }

    public void GetSwivelInput(InputAction.CallbackContext context)
    {
        EventDispatcher.Dispatch(new EventDefiner.SwivelInput(context.ReadValue<float>()));
    }

    public void GetBoostInput(InputAction.CallbackContext context)
    {
        //Check if boost is held, and store the result in boostHeld.
        CheckIfHeld(ref boostHeld, context);

        //Send info about whether boost input started this frame, and whether boost input is held this frame.
        EventDispatcher.Dispatch(new EventDefiner.JumpInput(context.started, boostHeld));
    }
}