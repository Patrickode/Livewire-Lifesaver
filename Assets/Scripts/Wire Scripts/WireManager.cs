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
    [Range(0f, 5f)] [SerializeField] private float currentSpeed = 1f;

    private GameObject current;
    private int currentIndex;


    private GameObject player;
    /// <summary>
    /// Whether the current is transitioning between wires right now.
    /// </summary>
    private bool currentTransitioning;
    /// <summary>
    /// How long a transition between wires should take in seconds.
    /// </summary>
    [Range(0f, 10f)] [SerializeField] private float transitionLength = 0.5f;
    private float transitionProgress = 0;

    void Start()
    {
        current = GameObject.FindWithTag("Current");
        if (!current) { Debug.LogError("No current was found. Add a current to the scene."); }

        player = GameObject.FindWithTag("Player");
        if (!current) { Debug.LogError("No player was found. Add a player to the scene."); }

        currentIndex = 0;

        //The wires add themselves to the list as they're enabled, but order isn't guaranteed.
        //We need to sort the list so the wires are in order, according to their order variable.
        //The lambda function returns whether wire1 should go before wire2.
        wireList.Sort((wire1, wire2) => wire1.order.CompareTo(wire2.order));
    }

    void Update()
    {
        //If the current hasn't been destroyed for whatever reason / isn't missing,
        if (current && wireList.Count > 0 && currentIndex < wireList.Count)
        {
            //If the current isn't transitioning between wires,
            if (!currentTransitioning)
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
                currentTransitioning = true;
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
        //Follow a path from the end of the last wire, to the player, to the next wire.
        //It's like it's arcing through the connection the player makes between the wires.
        current.transform.position = ThreePointLerp
            (
                wireList[currentIndex - 1].end.position,
                player.transform.position,
                wireList[currentIndex].start.position,
                percentTravelled
            );

        if (percentTravelled >= 1)
        {
            current.transform.position = wireList[currentIndex].start.position;
            transitionProgress = 0;
            currentTransitioning = false;
        }
    }

    /// <summary>
    /// Linearly interpolates a position on the lines mid - start or end - mid.
    /// </summary>
    /// <param name="t">The percentage of the way from start to end.</param>
    /// <returns>A point on one of the lines mid - start or end - mid, that is t percent along
    /// the line of start - end.</returns>
    private Vector3 ThreePointLerp(Vector3 start, Vector3 mid, Vector3 end, float t)
    {
        if (t <= 0.5f)
        {
            return Vector3.Lerp(start, mid, t * 2f);
        }
        else
        {
            return Vector3.Lerp(mid, end, t);
        }
    }
}
