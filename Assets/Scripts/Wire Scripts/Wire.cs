﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    public int order;
    public Transform start;
    public Transform end;
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