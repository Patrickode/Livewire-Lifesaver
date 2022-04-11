using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CurrentFlow : MonoBehaviour
{
    [SerializeField] private GameObject burstPrefab = null;
    [SerializeField] private GameObject boostEmitter = null;
    private GameObject player = null;

    [Tooltip("The speed of the current as it moves along the wire, roughly in units per second.")]
    [FormerlySerializedAs("normalSpeed")] [Range(0f, 5f)] [SerializeField] private float normalSpeed = 1f;
    [Tooltip("The speed of the current when boosting.")]
    [Range(0f, 10f)] [SerializeField] private float boostSpeed = 6f;
    [Tooltip("How long a transition between wires should take in seconds.")]
    [Range(0f, 1.5f)] [SerializeField] private float transitionLength = 0.5f;
    private float speed = 0;
    private bool boosting = false;

    /// <summary>
    /// The index the current is currently at in the list of wires.
    /// </summary>
    private int currentIndex;
    /// <summary>
    /// Whether the current is transitioning between wires right now.
    /// </summary>
    private bool currentTransitioning;
    private float transitionProgress = 0;

    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.BoostInput>(GetBoostInput);
    }
    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<EventDefiner.BoostInput>(GetBoostInput);
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (!player) { Debug.LogError("CurrentFlow: No player was found. Add a player to the scene."); }

        currentIndex = 0;
        speed = normalSpeed;

        WireManager.SortWires();
    }

    void Update()
    {
        //If we're boosting, move at boostSpeed and enable boost particles. If not, move at normal speed and
        //disable boost particles.
        speed = boosting ? boostSpeed : normalSpeed;
        boostEmitter.SetActive(boosting);

        //If there are wires to follow in the level and if we're not past the last wire,
        int numOfWires = WireManager.GetCount();
        if (numOfWires > 0 && currentIndex < numOfWires)
        {
            //If the current isn't transitioning between wires,
            if (!currentTransitioning)
            {
                //Move the current to towards its destination, the end of the wire it's on.
                Wire currentWire = WireManager.GetWire(currentIndex);
                transform.position = Vector3.MoveTowards
                    (
                        transform.position,
                        currentWire.end.position,
                        speed * Time.deltaTime
                    );
                //Face in the same direction as the wire we're on.
                transform.forward = currentWire.transform.forward;

                //If the current is at the end of the current wire...
                if (transform.position.Equals(currentWire.end.position))
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
            EventDispatcher.Dispatch(new EventDefiner.LevelEnd(true));
            CurrentBurst();
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
        Wire lastWire = WireManager.GetWire(currentIndex - 1);
        if (lastWire.type == WireType.BrokenEnd)
        {
            //The player needs to be close for the current to get to the next wire.
            if (lastWire.playerClose)
            {
                currentTransitioning = true;
            }
            //If they aren't, the current fizzles out. Initiate the losing sequence.
            else
            {
                EventDispatcher.Dispatch(new EventDefiner.LevelEnd(false));
                CurrentBurst();
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
        Wire lastWire = WireManager.GetWire(currentIndex - 1);
        Wire currentWire = WireManager.GetWire(currentIndex);
        transform.position = ThreePointLerp
            (
                lastWire.end.position,
                player.transform.position,
                currentWire.start.position,
                transitionProgress
            );

        //Rotate to face toward our current destination.
        if (transitionProgress < 0.5f)
        {
            transform.LookAt(player.transform);
        }
        else
        {
            transform.LookAt(currentWire.start.transform);
        }

        //Once the transition has completed, set the current's position as a failsafe, reset transitionProgress,
        //and not that the transition has ended.
        if (transitionProgress >= 1)
        {
            transform.position = currentWire.start.position;
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

    /// <summary>
    /// Make the current destroy itself and burst into particles.
    /// </summary>
    private void CurrentBurst()
    {
        if (burstPrefab)
        {
            Instantiate(burstPrefab, gameObject.transform.position, gameObject.transform.rotation);
        }
        else
        {
            Debug.LogWarning("CurrentFlow: Burst prefab was not assigned; cannot do current burst.");
        }

        Destroy(gameObject);
    }

    private void GetBoostInput(EventDefiner.BoostInput evt)
    {
        //If boost input was pressed, start boosting, and stop boosting once it's released.
        if (evt.PressedThisFrame)
        {
            boosting = true;
        }
        else if (!evt.IsHeld)
        {
            boosting = false;
        }
    }
}