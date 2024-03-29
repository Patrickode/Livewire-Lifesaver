﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput = null;

    private bool jumpHeld = false;
    private bool boostHeld = false;
    private bool levelTransitioning = false;

    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.LevelEnd>(OnLevelTransition);
        EventDispatcher.AddListener<EventDefiner.MenuExit>(OnLevelTransition);
    }
    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<EventDefiner.LevelEnd>(OnLevelTransition);
        EventDispatcher.RemoveListener<EventDefiner.MenuExit>(OnLevelTransition);
    }
    private void OnLevelTransition(EventDefiner.LevelEnd _) { levelTransitioning = true; }
    private void OnLevelTransition(EventDefiner.MenuExit _) { levelTransitioning = true; }

    private void Start()
    {
        EventDispatcher.Dispatch(new EventDefiner.ControlSchemeChange(playerInput.currentControlScheme));
    }

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

        //We only need to dispatch an event if the level isn't transitioning, though; the current will never
        //be alive when the level is ending.
        if (!levelTransitioning)
        {
            //Send info about whether boost input started this frame, and whether boost input is held this frame.
            EventDispatcher.Dispatch(new EventDefiner.BoostInput(context.started, boostHeld));
        }
    }

    public void GetPauseInput(InputAction.CallbackContext context)
    {
        //If the button was pressed, toggle PauseHandler's pause state.
        //Only follow through with the pause input if the level is still going.
        if (!levelTransitioning && context.performed)
        {
            EventDispatcher.Dispatch(new EventDefiner.PauseStateChange(!PauseHandler.Paused));
        }
    }

    public void OnControlSchemeChanged()
    {
        EventDispatcher.Dispatch(new EventDefiner.ControlSchemeChange(playerInput.currentControlScheme));
    }
}