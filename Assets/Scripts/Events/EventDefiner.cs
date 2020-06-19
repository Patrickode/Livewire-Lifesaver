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

        public MenuExit(int destinationSceneIndex)
        {
            DestinationSceneIndex = destinationSceneIndex;
        }
    }

    public class MenuSwap : GenericEvent
    {
        public MenuSwap() { }
    }

    public class CurrentBoost : GenericEvent
    {
        public bool Boosting { get; private set; }

        public CurrentBoost(bool boosting)
        {
            Boosting = boosting;
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
        public bool ButtonDown { get; private set; }

        public JumpInput(bool buttonDown)
        {
            ButtonDown = buttonDown;
        }
    }
}
