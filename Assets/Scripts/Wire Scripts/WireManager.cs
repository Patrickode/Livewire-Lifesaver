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
    /// The speed of the current as it moves along the wire, roughly in units per frame.
    /// </summary>
    [Range(0f, 1f)] public float currentSpeed = 0.1f;

    private GameObject current;
    private int currentIndex;

    void Start()
    {
        current = GameObject.FindWithTag("Current");
        if (!current)
        {
            throw new MissingReferenceException("No current was found. Add a current prefab to the scene.");
        }

        currentIndex = 0;

        //The wires add themselves to the list as they're enabled, but order isn't guaranteed.
        //We need to sort the list so the wires are in order, according to their order variable.
        //The lambda function returns whether wire1 should go before wire2.
        wireList.Sort((wire1, wire2) => wire1.order.CompareTo(wire2.order));
    }

    void Update()
    {
        //First, move the current to towards its destination, the end of the wire it's on.
        current.transform.position = Vector3.MoveTowards
            (
                current.transform.position,
                wireList[currentIndex].end.position,
                currentSpeed * Time.deltaTime
            );

        //If the current is at the end of the current wire...
        if (current.transform.position.Equals(wireList[currentIndex].end.position))
        {
            currentIndex++;
            if (wireList[currentIndex - 1].type == WireType.Broken)
            {
                if (wireList[currentIndex - 1].playerClose)
                {
                    current.transform.position = wireList[currentIndex].start.position;
                }
                else
                {
                    //TODO: Initiate lose
                    currentIndex += 10;
                }
            }
        }
    }
}
