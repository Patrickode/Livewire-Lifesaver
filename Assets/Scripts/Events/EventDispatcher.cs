using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages event callback functions and event dispatching.
/// Adapted from MacSalad's Event Dispatcher: https://gist.github.com/ntratcliff/8d1b87a465e6bb6fe259fa97e5eddd43
/// </summary>
public static class EventDispatcher
{
    /// <summary>
    /// Delegate type for event callbacks
    /// </summary>
    /// <typeparam name="T">The event this delegate handles</typeparam>
    public delegate void EventDelegate<T>(T @event) where T : EventDefiner.GenericEvent;
    private delegate void EventDelegate(EventDefiner.GenericEvent @event);

    /// <summary>
    /// All event callbacks, stored by event name key
    /// </summary>
    //private static Dictionary<string, Action<UnityEngine.Object, object>> callbacks;
    private static Dictionary<Type, EventDelegate> delegates;

    /// <summary>
    /// Maps generic delegates to their typed delegate pair
    /// </summary>
    private static Dictionary<Delegate, EventDelegate> delegateLookup;

    static EventDispatcher()
    {
        delegates = new Dictionary<Type, EventDelegate>();
        delegateLookup = new Dictionary<Delegate, EventDelegate>();
    }

    /// <summary>
    /// Adds a listener for the provided event type
    /// </summary>
    public static void AddListener<T>(EventDelegate<T> callback)
        where T : EventDefiner.GenericEvent
    {
        if (delegateLookup.ContainsKey(callback))
        {
            Debug.LogWarning("[EventDispatcher] Attempt to add listener that's already been added.");
            return;
        }

        EventDelegate genericCallback = (e) => callback((T)e);
        delegateLookup[callback] = genericCallback;

        Type eventType = typeof(T);
        EventDelegate eventGenericDelegate;
        if (delegates.TryGetValue(eventType, out eventGenericDelegate)
            && eventGenericDelegate != null)
        {
            delegates[eventType] = eventGenericDelegate += genericCallback;
        }
        else
        {
            delegates[eventType] = genericCallback;
        }
    }

    /// <summary>
    /// Removes a callback for the provided event type
    /// </summary>
    public static void RemoveListener<T>(EventDelegate<T> callback)
        where T : EventDefiner.GenericEvent
    {
        EventDelegate eventCallback;
        if (delegateLookup.TryGetValue(callback, out eventCallback))
        {
            Type eventType = typeof(T);
            EventDelegate del;
            if (delegates.TryGetValue(eventType, out del))
            {
                del -= eventCallback;
                delegates[eventType] = del;
            }

            delegateLookup.Remove(callback);
        }
        else
        {
            Debug.LogErrorFormat("[EventDispatcher] Attempt to remove listener for event \"{0}\"" +
                " but event hasn't been registered with EventDispatcher!", typeof(T));
        }
    }

    /// <summary>
    /// Sends the provided event to all registered callbacks
    /// </summary>
    public static void Dispatch(EventDefiner.GenericEvent e)
    {
        EventDelegate del;
        if (delegates.TryGetValue(e.GetType(), out del)
            && del != null)
        {
            del(e);
        }
        else
        {
            Debug.LogWarningFormat("[EventDispatcher] Event \"{0}\" dispatched, " +
                "but no listeners registered!", e.GetType());
        }
    }

    /// <summary>
    /// Clears all registered event callbacks
    /// </summary>
    public static void RemoveAllListeners()
    {
        delegates.Clear();
        delegateLookup.Clear();
    }
}