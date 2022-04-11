using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RequireBolts : MonoBehaviour
{
    [SerializeField] private Button lockedButton;
    [SerializeField] private int boltsRequired = 0;

    private void Start()
    {
        lockedButton.interactable = SaveManager.CachedData.CollectedBoltIndices.Length >= boltsRequired;
    }
}
