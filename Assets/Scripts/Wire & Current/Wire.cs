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
    [Header("Editor Only: Toggle Corner Cap")]
    [SerializeField] private GameObject cornerCap = null;
    [SerializeField] private bool toggleCornerCap = false;
    private GameObject cachedCap = null;
    /// <summary>
    /// When getting CachedCap, check children of this object for a corner cap if one is not already cached.
    /// </summary>
    private GameObject CachedCap
    {
        get
        {
            if (!cachedCap)
            {
                foreach (Transform child in transform)
                {
                    if (child.gameObject.CompareTag("Corner Cap"))
                    {
                        cachedCap = child.gameObject;
                        break;
                    }
                }
            }
            return cachedCap;
        }
        set { cachedCap = value; }
    }

    //This is used to circumvent enigmatic, irrelevant warnings in the console about SendMessage,
    //which is clearly never used in this segment.
    //Gotten from https://forum.unity.com/threads/sendmessage-cannot-be-called-during-awake-checkconsistency-or-onvalidate-can-we-suppress.537265/#post-5560519
    void OnValidate() { UnityEditor.EditorApplication.delayCall += OnOnValidate; }
    private void OnOnValidate()
    {
        if (this == null) { return; }

        if (cornerCap && toggleCornerCap)
        {
            if (!CachedCap)
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

                CachedCap = addedCap;
            }
            else
            {
                //If we have a cap cached, destroy it and set CachedCap to null just to be safe.
                DestroyImmediate(CachedCap, false);
                CachedCap = null;
            }

            toggleCornerCap = false;
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