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

#if UNITY_EDITOR
    [Header("Auto-Add Corner Cap")]
    [SerializeField] private GameObject cornerCap = null;
    [SerializeField] private bool addCornerCap = false;

    //This is used to circumvent enigmatic, irrelevant warnings in the console about SendMessage,
    //which is clearly never used in this segment.
    //Gotten from https://forum.unity.com/threads/sendmessage-cannot-be-called-during-awake-checkconsistency-or-onvalidate-can-we-suppress.537265/#post-5560519
    void OnValidate() { UnityEditor.EditorApplication.delayCall += OnOnValidate; }
    private void OnOnValidate()
    {
        if (this == null) { return; }

        if (cornerCap && addCornerCap)
        {
            //Instantiate a corner cap and set it to the right size / position
            GameObject addedCap = Instantiate(cornerCap, gameObject.transform);
            addedCap.transform.localScale = new Vector3
            (
                addedCap.transform.localScale.x / transform.localScale.x,
                addedCap.transform.localScale.y / transform.localScale.y,
                addedCap.transform.localScale.z / transform.localScale.z
            );
            addedCap.transform.localPosition = new Vector3(0, 0, 0.5f);

            addCornerCap = false;
        }
    }
#endif

    private void OnEnable()
    {
        WireManager.AddWire(this);
    }

    private void OnDisable()
    {
        WireManager.RemoveWire(this);
    }
}