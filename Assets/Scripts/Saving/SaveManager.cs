using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveManager
{
    private static readonly string pathSuffix = "data.lls";

    /// <summary>
    /// The last data saved/loaded this session. Defaults to a fresh save.
    /// </summary>
    public static SaveData CachedData
    {
        get { return cachedData != null ? cachedData : LoadDataFromFile(); }
        private set { cachedData = value; }
    }
    private static SaveData cachedData;

    /// <summary>
    /// Saves provided data to a file.
    /// </summary>
    /// <param name="highestCompletedIndex">The build index of the last level the player played.</param>
    /// <param name="collectedBoltIndices">An array of build indices that the player has collected
    /// bolts from.</param>
    /// <param name="bindingOverrides">All of the player's overrides to control bindings.</param>
    public static void SaveDataToFile(
        int highestCompletedIndex,
        int[] collectedBoltIndices,
        BindingOverride[] bindingOverrides
    )
    {
        //Create a formatter to save the data with and prepare the path to save it to.
        BinaryFormatter formatter = new BinaryFormatter();
        string dataPath = Path.Combine(Application.persistentDataPath, pathSuffix);

        //Create some SaveData with the given parameters.
        SaveData dataToSave = new SaveData(highestCompletedIndex, collectedBoltIndices, bindingOverrides);

        //Open a FileStream and save the SaveData we just made to it.
        using (FileStream stream = new FileStream(dataPath, FileMode.Create))
        {
            formatter.Serialize(stream, dataToSave);
        }

        //Cache the newly saved data to ensure the cache is up to date.
        CachedData = dataToSave;

        //Saving has completed, dispatch an event saying so.
        EventDispatcher.Dispatch(new EventDefiner.SaveToFile(true));
    }
    /// <summary>
    /// Save completed level data.
    /// </summary>
    /// <param name="completedIndex">The index of the level that was just completed.</param>
    /// <param name="boltCollected">Whether a bolt was collected in the level or not.</param>
    public static void SaveDataToFile(int completedIndex, bool boltCollected = false)
    {
        //Saving has started and is not complete; dispatch an event saying so.
        EventDispatcher.Dispatch(new EventDefiner.SaveToFile(false));

        //Get the current values for HighestCompletedIndex and CollectedBoltIndices.
        int indexToSave = CachedData.LastCompletedIndex;
        int[] collectedIndicesToSave = CachedData.CollectedBoltIndices;

        //We only need to save if something changed. If nothing changed, just do nothing.
        if (completedIndex > indexToSave || boltCollected)
        {
            //Check if the completedIndex is higher than the cached one; if so, overwrite it
            indexToSave = completedIndex > indexToSave ? completedIndex : indexToSave;

            //If a bolt was collected, add it to the array of collected bolts.
            if (boltCollected)
            {
                int[] newIndex = { completedIndex };
                collectedIndicesToSave = collectedIndicesToSave.Union(newIndex).ToArray();
            }

            SaveDataToFile(indexToSave, collectedIndicesToSave, CachedData.BindingOverrides);
        }
        //Since there's nothing to save if we got here, the save is complete.
        else { EventDispatcher.Dispatch(new EventDefiner.SaveToFile(true)); }
    }
    /// <summary>
    /// Save binding related data.
    /// </summary>
    /// <param name="bindingOverrides">The overrides to add/remove from the saved array of overrides.</param>
    /// <param name="removePassedBindings">Whether to remove <paramref name="bindingOverrides"/> from the saved 
    /// array of overrides or not.</param>
    public static void SaveDataToFile(BindingOverride[] bindingOverrides, bool removePassedBindings = false)
    {
        //Saving has started and is not complete; dispatch an event saying so.
        EventDispatcher.Dispatch(new EventDefiner.SaveToFile(false));

        //Set up an array to put the updated array of overrides in, so we can save it.
        BindingOverride[] updatedOverrides = null;

        //Remove the bindings from the saved array or add them to the saved array.
        if (removePassedBindings)
        {
            updatedOverrides = CachedData.BindingOverrides.Except(bindingOverrides).ToArray();
        }
        else
        {
            updatedOverrides = CachedData.BindingOverrides.Union(bindingOverrides).ToArray();
        }

        SaveDataToFile(CachedData.LastCompletedIndex, CachedData.CollectedBoltIndices, updatedOverrides);
    }

    /// <summary>
    /// Loads saved data and returns it.
    /// </summary>
    /// <returns>The saved data to be loaded.</returns>
    public static SaveData LoadDataFromFile()
    {
        //Prepare the path to load from, and a variable to store the result in.
        string dataPath = Path.Combine(Application.persistentDataPath, pathSuffix);
        SaveData loadedData = null;

        //Before we try and load from the path, we need to check if it exists at all.
        if (!File.Exists(dataPath))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"No save data found at expected path \"{dataPath}\". Returning empty save.");
#endif
            loadedData = new SaveData();
        }
        else
        {
            //If it does, open a stream and deserialize the data in the file at dataPath.
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(dataPath, FileMode.Open))
            {
                loadedData = formatter.Deserialize(stream) as SaveData;
            }
        }

        //Cache the loaded data so it can be accessed easily.
        CachedData = loadedData;

        return loadedData;
    }
}
