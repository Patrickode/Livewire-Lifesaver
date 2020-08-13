using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DataInitializer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI startButtonText = null;
    [Tooltip("A reference to any action in the desired action map." +
        "\n(You can't get an inspector reference to an action map, as far as I know.)")]
    [SerializeField] private InputActionReference actionInMap = null;
    private InputActionMap actionMap;

    void Start()
    {
        //Get the action map from the action reference supplied in the inspector.
        actionMap = actionInMap.action.actionMap;

        //Get the current cached data (if there is none, it will be loaded).
        SaveData loadedData = SaveManager.CachedData;

        //If the player has completed a level, make the startButton say "Continue" instead.
        if (loadedData.HighestCompletedIndex > 0)
        {
            startButtonText.text = "Continue";
        }

        //For each binding override saved, apply that override to the action map.
        foreach (BindingOverride @override in loadedData.BindingOverrides)
        {
            actionMap.actions[@override.ActionIndex].ApplyBindingOverride(@override.BindingIndex, @override.Path);
        }
    }
}