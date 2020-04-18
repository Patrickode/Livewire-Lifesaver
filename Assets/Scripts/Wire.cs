using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    [HideInInspector] public Transform start;
    [HideInInspector] public Transform end;
    public WireType type;

    private void OnEnable()
    {
        start = transform.GetChild(0);
        end = transform.GetChild(transform.childCount - 1);

        WireManager.wireList.Add(this);
    }

    private void OnDisable()
    {
        WireManager.wireList.Remove(this);
    }
}
