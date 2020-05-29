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
}
