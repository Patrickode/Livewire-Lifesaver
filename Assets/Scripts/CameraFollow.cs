using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    private GameObject focalObj;
    private Vector3 camOffset;

    void Start()
    {
        //If a focal object hasn't been manually assigned
        if (!focalObj)
        {
            //Find the player and make them the focal object.
            focalObj = GameObject.FindWithTag("Player");

            //If focal object is still null, no player was found.
            if (!focalObj)
            {
                Debug.LogError("CameraFollow: No focal object was assigned and no player was found. " +
                    "Manually assign a focal object or add a player to the scene with tag \"Player.\"");
            }
        }

        camOffset = transform.position - focalObj.transform.position;
    }

    void Update()
    {
        transform.position = new Vector3
            (
                focalObj.transform.position.x + camOffset.x,
                focalObj.transform.position.y + camOffset.y,
                focalObj.transform.position.z + camOffset.z
            );

        transform.LookAt(focalObj.transform);
    }
}
