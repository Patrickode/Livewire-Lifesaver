using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwivel : MonoBehaviour
{
    [SerializeField]
    private float rotateSpeed = 1;

    private float currentSwivelAxis;

    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.SwivelInput>(GetSwivelInput);
    }
    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<EventDefiner.SwivelInput>(GetSwivelInput);
    }

    private void GetSwivelInput(EventDefiner.SwivelInput evt)
    {
        currentSwivelAxis = evt.Magnitude;
    }

    void Update()
    {
        if (!Mathf.Approximately(currentSwivelAxis, 0))
        {
            transform.Rotate(0, rotateSpeed * currentSwivelAxis * Time.deltaTime, 0);
        }
    }
}
