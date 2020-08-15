using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveOnClear : MonoBehaviour
{
    [SerializeField] private GameObject goldBolt = null;

#if UNITY_EDITOR
    [Header("Cached Save Data")]
    [SerializeField] private bool showCachedData = false;
    [Space(7.5f)]
    [SerializeField] private int lastCompletedIndex;
    [SerializeField] private int[] collectedBoltIndices;

    private void OnValidate()
    {
        if (showCachedData)
        {
            lastCompletedIndex = SaveManager.CachedData.LastCompletedIndex;
            collectedBoltIndices = SaveManager.CachedData.CollectedBoltIndices;
        }
    }
#endif

    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.LevelEnd>(OnLevelEnd);
    }
    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<EventDefiner.LevelEnd>(OnLevelEnd);
    }

    private void OnLevelEnd(EventDefiner.LevelEnd evt)
    {
        //If the level was cleared,
        if (evt.LevelSuccess)
        {
            //Attempt to save this level's completion to a file.
            //If we have a reference to the gold bolt, see if it was collected (i.e. is not active).
            //If not, default to false.
            SaveManager.SaveDataToFile
            (
                SceneManager.GetActiveScene().buildIndex,
                goldBolt ? !goldBolt.activeSelf : false
            );
        }
    }
}
