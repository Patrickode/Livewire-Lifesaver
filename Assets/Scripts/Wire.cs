using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    public Transform start;
    public Transform end;
    public GameObject gapTrigger;
    public WireType type;

    private void OnEnable()
    {
        //WireManager.wireList.Add(this);
    }

    private void OnDisable()
    {
        //WireManager.wireList.Remove(this);
    }
}
