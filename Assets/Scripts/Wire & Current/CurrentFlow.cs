using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentFlow : MonoBehaviour
{
    /// <summary>
    /// The speed of the current as it moves along the wire, roughly in units per second.
    /// </summary>
    [Range(0f, 5f)] [SerializeField] private float currentSpeed = 1f;

    /// <summary>
    /// The index the current is currently at in the list of wires.
    /// </summary>
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
        player = GameObject.FindWithTag("Player");
        if (!player) { Debug.LogError("CurrentFlow: No player was found. Add a player to the scene."); }

        currentIndex = 0;

        WireManager.SortWires();
    }

    void Update()
    {
        //If there are wires to follow in the level and if we're not past the last wire,
        int numOfWires = WireManager.GetCount();
        if (numOfWires > 0 && currentIndex < numOfWires)
        {
            //If the current isn't transitioning between wires,
            if (!currentTransitioning)
            {
                //Move the current to towards its destination, the end of the wire it's on.
                transform.position = Vector3.MoveTowards
                    (
                        transform.position,
                        WireManager.GetWire(currentIndex).end.position,
                        currentSpeed * Time.deltaTime
                    );

                //If the current is at the end of the current wire...
                if (transform.position.Equals(WireManager.GetWire(currentIndex).end.position))
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
        //If there are wires in the level and we got this far, the wire's reached the end. Initiate 
        //the win sequence.
        else if (numOfWires > 0)
        {
            Destroy(gameObject);
            EventDispatcher.Dispatch(new EventDefiner.LevelEnd(true));
        }
    }

    /// <summary>
    /// Updates currentIndex and currentTransition depending on whether the wire the current
    /// is on is broken or not.
    /// </summary>
    private void GoToNextWire()
    {
        //Move on to the next wire, no matter what.
        currentIndex++;

        //If the wire the current was on just a second ago was broken...
        if (WireManager.GetWire(currentIndex - 1).type == WireType.BrokenEnd)
        {
            //The player needs to be close for the current to get to the next wire.
            if (WireManager.GetWire(currentIndex - 1).playerClose)
            {
                currentTransitioning = true;
            }
            //If they aren't, the current fizzles out. Initiate the losing sequence.
            else
            {
                Destroy(gameObject);
                EventDispatcher.Dispatch(new EventDefiner.LevelEnd(false));
            }
        }
    }

    /// <summary>
    /// Moves the current between a wire gap in roughly transitionLength seconds.
    /// </summary>
    private void DoCurrentTransition()
    {
        //Add one step to transition progress.
        //We want the transition to happen in transitionLength seconds, so one "step" is however many seconds have
        //passed since last frame over the desired length.
        transitionProgress += Time.deltaTime / transitionLength;

        //Follow a path from the end of the last wire, to the player, to the next wire.
        //It's like it's arcing through the connection the player makes between the wires.
        transform.position = ThreePointLerp
            (
                WireManager.GetWire(currentIndex - 1).end.position,
                player.transform.position,
                WireManager.GetWire(currentIndex).start.position,
                transitionProgress
            );

        //Once the transition has completed, set the current's position as a failsafe, reset transitionProgress,
        //and not that the transition has ended.
        if (transitionProgress >= 1)
        {
            transform.position = WireManager.GetWire(currentIndex).start.position;
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
            return Vector3.Lerp(mid, end, (t - 0.5f) * 2f);
        }
    }
}