using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidOut : MonoBehaviour
{
    private bool canVoidOut = true;

    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.LevelEnd>(OnLevelEnd);
    }

    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<EventDefiner.LevelEnd>(OnLevelEnd);
    }

    private void OnLevelEnd(EventDefiner.LevelEnd _)
    {
        //This ensures that the player cannot void out while winning or losing, which would potentially
        //cause overlap issues
        canVoidOut = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canVoidOut && other.CompareTag("Player"))
        {
            EventDispatcher.Dispatch(new EventDefiner.LevelEnd(false));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (canVoidOut && collision.gameObject.CompareTag("Player"))
        {
            EventDispatcher.Dispatch(new EventDefiner.LevelEnd(false));
        }
    }
}
