using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectPlayer : MonoBehaviour
{
    [SerializeField] private Wire parentWire = null;
    [SerializeField] private LineRenderer line = null;
    private Transform player = null;

    [SerializeField] [Range(0, 10)] private int lineJitterPoints = 0;
    [SerializeField] [Range(0, 0.5f)] private float minLineJitter = 0;
    [SerializeField] [Range(0, 0.5f)] private float maxLineJitter = 0;

    private Vector3[] linePos = null;
    private float linePointStep = 0;

    private void Start()
    {
        //First, make an array of vectors of size 2 (the start and end) plus lineJitterPoints.
        linePos = new Vector3[2 + lineJitterPoints];

        //If this wire is broken at its end, set the start of the line to the end of this wire.
        //Otherwise, set it to the start of this wire. If this script is present, this wire can't be "connected."
        //This ensures the line originates from the broken side of the wire.
        linePos[0] = parentWire.type == WireType.BrokenEnd
            ? parentWire.end.position : parentWire.start.position;

        //Since the end position follows the player, we don't need to set any other positions yet.
        //Establish where along the line of end - start each point should be for later, and set linePos equal 
        //to the base we established.
        linePointStep = 1f / (1f + lineJitterPoints);

        //Set the line's count and positions equal to the linePos.
        line.positionCount = linePos.Length;
        line.SetPositions(linePos);

        //Using world space in the editor offsets the entire wire prefab's transform in funky, enigmatic ways.
        //The solution is to turn it off until runtime.
        line.enabled = false;
        line.useWorldSpace = true;
    }

    private void Update()
    {
        //Make sure the line is only seen when the player is close.
        line.enabled = parentWire.playerClose;

        //If the player is close, and we have a reference to the player (we should, see on trigger enter),
        if (parentWire.playerClose && player)
        {
            //update the end position to be the player's position.
            linePos[line.positionCount - 1] = player.position;

            //Now go through all the points in the line between the start and end.
            for (int i = 0; i < lineJitterPoints; i++)
            {
                //First, get where the point would be if this were a straight line.
                Vector3 straightLinePoint = Vector3.Lerp
                (
                    line.GetPosition(0),
                    player.position,
                    linePointStep * (i + 1)
                );

                //Now get a random jitter direction and make sure it's not zero.
                Vector3 jitterModifier = Vector3.zero;
                while (jitterModifier == Vector3.zero)
                {
                    //Take the cross product of a random direction with the line direction to get a random vector 
                    //perpendicular to the line.
                    jitterModifier = Vector3.Cross(Random.onUnitSphere, player.position - line.GetPosition(0));
                }

                //Normalize the result, because two unit vectors crossed don't always yield a unit vector.
                jitterModifier.Normalize();

                //Now multiply that normalized jitter direction by a random scalar in the desired range.
                jitterModifier *= Random.Range(minLineJitter, maxLineJitter);

                //Finally, set this point to be the straight line point modified by the jitter vector.
                linePos[i + 1] = straightLinePoint + jitterModifier;
            }

            //Now that all the positions are set as they should be, assign them to the line.
            line.SetPositions(linePos);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            parentWire.playerClose = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            parentWire.playerClose = false;
        }
    }
}
