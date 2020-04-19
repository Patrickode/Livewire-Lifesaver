using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireManager : MonoBehaviour
{
    /// <summary>
    /// An array of all the wires in the level. These are in sequential order.
    /// </summary>
    public static List<Wire> wireList = new List<Wire>();

    /// <summary>
    /// The speed of the current as it moves along the wire, roughly in units per second.
    /// </summary>
    [Range(0f, 1f)] [SerializeField] private float currentSpeed = 0.5f;

    private GameObject current;
    private int currentIndex;
    private bool currentTransition;

    /// <summary>
    /// How long a transition between wires should take in seconds.
    /// </summary>
    [Range(0f, 10f)] [SerializeField] private float transitionLength = 0.5f;
    private float transitionProgress = 0;

    void Start()
    {
        current = GameObject.FindWithTag("Current");
        if (!current)
        {
            Debug.LogError("No current was found. Add a current to the scene.");
        }

        currentIndex = 0;

        //The wires add themselves to the list as they're enabled, but order isn't guaranteed.
        //We need to sort the list so the wires are in order, according to their order variable.
        //The lambda function returns whether wire1 should go before wire2.
        wireList.Sort((wire1, wire2) => wire1.order.CompareTo(wire2.order));
    }

    void Update()
    {
        //If the current hasn't been destroyed for whatever reason / isn't missing,
        if (current)
        {
            //If the current isn't transitioning between wires,
            if (!currentTransition)
            {
                //Move the current to towards its destination, the end of the wire it's on.
                current.transform.position = Vector3.MoveTowards
                    (
                        current.transform.position,
                        wireList[currentIndex].end.position,
                        currentSpeed * Time.deltaTime
                    );

                //If the current is at the end of the current wire...
                if (current.transform.position.Equals(wireList[currentIndex].end.position))
                {
                    //Go to the next wire and do all relevant logic.
                    GoToNextWire();
                }
            }
            //If the current IS transitioning between wires,
            else
            {
                //Move through the gap at a speed dictated by transitionSpeed.
                DoCurrentTransition();
            } 
        }
    }

    /// <summary>
    /// Updates currentIndex and currentTransition depending on whether the wire the current
    /// is on is broken or not.
    /// </summary>
    private void GoToNextWire()
    {
        currentIndex++;
        if (wireList[currentIndex - 1].type == WireType.Broken)
        {
            if (wireList[currentIndex - 1].playerClose)
            {
                //current.transform.position = wireList[currentIndex].start.position;
                currentTransition = true;
            }
            else
            {
                Destroy(current);
                //TODO: Initiate lose sequence
            }
        }
    }

    /// <summary>
    /// Moves the current between a wire gap in roughly transitionLength seconds.
    /// </summary>
    private void DoCurrentTransition()
    {
        transitionProgress++;

        //We want the transition to take roughly transitionLength seconds, so we 
        float percentTravelled = 1 / (60 * transitionLength) * transitionProgress;

        //Move it to the start of the next wire in roughly transitionLength seconds.
        current.transform.position = Vector3.Lerp
            (
                wireList[currentIndex - 1].end.position,
                wireList[currentIndex].start.position,
                percentTravelled
            );

        if (percentTravelled >= 1)
        {
            current.transform.position = wireList[currentIndex].start.position;
            transitionProgress = 0;
            currentTransition = false;
        }
    }
}
