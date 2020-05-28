using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines all the different kinds of events that are stored / dispatched by EventDispatcher.
/// </summary>
public class EventDefiner : MonoBehaviour
{
    public abstract class GenericEvent { }

    public class ExampleEvent : GenericEvent
    {
        public int SomeData { get; private set; }

        public ExampleEvent(int someData)
        {
            SomeData = someData;
        }
    }
}
