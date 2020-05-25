using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    public Transform start;
    public Transform end;
    public Transform trigger;
    public WireType type;
    [HideInInspector] public bool playerClose;

    private void OnEnable()
    {
        WireManager.AddWire(this);
    }

    private void OnDisable()
    {
        WireManager.RemoveWire(this);
    }
}