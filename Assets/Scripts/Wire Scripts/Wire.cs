using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    public Transform start;
    public Transform end;
    public WireType type;
    [HideInInspector] public bool playerClose;
}