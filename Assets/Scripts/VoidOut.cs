using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidOut : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventDispatcher.Dispatch(new EventDefiner.LevelEnd(false));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            EventDispatcher.Dispatch(new EventDefiner.LevelEnd(false));
        }
    }
}
