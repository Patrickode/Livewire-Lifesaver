using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltFloat : MonoBehaviour
{
    [Tooltip("Measured in unity units.")]
    [SerializeField] [Range(0f, 0.75f)] private float bobDistance = 0.1f;
    [Tooltip("Measured in degrees per second, because bobbing is done via sine wave.")]
    [SerializeField] [Range(0f, 5f)] private float bobSpeed = 1.5f;
    [Tooltip("Measured in degrees per second.")]
    [SerializeField] [Range(0f, 360f)] private float spinSpeed = 25f;

    private float bobAngle = 0f;
    private Vector3 originalPos;

    private void Start() { originalPos = transform.position; }

    private void Update()
    {
        bobAngle += bobSpeed * Time.deltaTime;

        transform.position = new Vector3
            (
                transform.position.x,
                originalPos.y + bobDistance * Mathf.Sin(bobAngle),
                transform.position.z
            );

        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime);

        while (bobAngle >= 360) { bobAngle -= 360; }
    }
}
