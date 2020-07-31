using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltCollect : MonoBehaviour
{
    [SerializeField] GameObject collectParticleObj = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collected();
        }
    }

    private void Collected()
    {
        if (collectParticleObj) { Instantiate(collectParticleObj, transform.position, Quaternion.identity); }
        gameObject.SetActive(false);
    }
}
