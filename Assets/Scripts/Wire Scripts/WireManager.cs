using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireManager : MonoBehaviour
{
    /// <summary>
    /// An array of all the wires in the level. These are in sequential order.
    /// </summary>
    Wire[] wireList;

    /// <summary>
    /// The speed of the current as it moves along the wire, roughly in units per frame.
    /// </summary>
    [Range(0f, 1f)] public float currentSpeed = 0.1f;

    private GameObject current;
    private int currentIndex;

    void Start()
    {
        current = GameObject.FindWithTag("Current");

        currentIndex = 0;
        wireList = FindObjectsOfType<Wire>();
        System.Array.Reverse(wireList);
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
