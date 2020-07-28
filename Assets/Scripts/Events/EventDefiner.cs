using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines all the different kinds of events that are stored / dispatched by EventDispatcher.
/// </summary>
public class EventDefiner : MonoBehaviour
{
    public abstract class GenericEvent { }

    public class LevelEnd : GenericEvent
    {
        public bool LevelSuccess { get; private set; }

        public LevelEnd(bool levelSuccess)
        {
            LevelSuccess = levelSuccess;
        }
    }

    public class MenuExit : GenericEvent
    {
        public int DestinationSceneIndex { get; private set; }
        public float FadeOutDuration { get; private set; }

        public MenuExit(int destinationSceneIndex) : this(destinationSceneIndex, -1) { }
        public MenuExit(int destinationSceneIndex, float fadeOutDuration)
        {
            DestinationSceneIndex = destinationSceneIndex;
            FadeOutDuration = fadeOutDuration;
        }
    }

    public class MenuSwap : GenericEvent
    {
        public bool ShouldFlash { get; private set; }

        public MenuSwap(bool shouldFlash)
        {
            ShouldFlash = shouldFlash;
        }
    }

    public class MoveInput : GenericEvent
    {
        public Vector3 Direction { get; private set; }

        public MoveInput(Vector3 direction)
        {
            Direction = direction;
        }
    }

    public class JumpInput : GenericEvent
    {
        public bool PressedThisFrame { get; private set; }
        public bool IsHeld { get; private set; }

        public JumpInput(bool pressedThisFrame, bool isHeld)
        {
            PressedThisFrame = pressedThisFrame;
            IsHeld = isHeld;
        }
    }

    public class SwivelInput : GenericEvent
    {
        public float Magnitude { get; private set; }

        public SwivelInput(float magnitude)
        {
            Magnitude = magnitude;
        }
    }

    public class BoostInput : GenericEvent
    {
        public bool PressedThisFrame { get; private set; }
        public bool IsHeld { get; private set; }

        public BoostInput(bool pressedThisFrame, bool isHeld)
        {
            PressedThisFrame = pressedThisFrame;
            IsHeld = isHeld;
        }
    }

    public class PauseStateChange : GenericEvent
    {
        public bool Paused { get; private set; }

        public PauseStateChange(bool paused)
        {
            Paused = paused;
        }
    }

    public class ControlSchemeChange : GenericEvent
    {
        public string ControlScheme { get; private set; }

        public ControlSchemeChange(string controlScheme)
        {
            ControlScheme = controlScheme;
        }
    }
}
