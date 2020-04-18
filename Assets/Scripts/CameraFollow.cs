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
