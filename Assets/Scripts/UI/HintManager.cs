using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public class HintManager : MonoBehaviour
{
    [SerializeField] private RectTransform hintPanel = null;
    [SerializeField] private TextMeshProUGUI hintText = null;

    [Tooltip("An action from the map you want to reference. Any action from the map will work." +
        "\n\nAs far as I know, getting a reference to an action map directly is impossible via the inspector.")]
    [SerializeField] private InputActionReference actionFromDesiredMap = null;
    private InputActionMap actionMap = null;
    private Dictionary<string, InputAction> actionDict = null;
    private string currentControlScheme = "";

    [Tooltip("How long it should take the panel to move in and out of place, in seconds.")]
    [SerializeField] [Range(0f, 2f)] private float panelMoveDuration = 0.5f;
    [Tooltip("How long the panel should stay on screen before moving away, in seconds.")]
    [SerializeField] [Range(0.1f, 10f)] private float panelDisplayDuration = 5f;

    [Tooltip("The message / hint that should be in the hint panel." +
        "\n\nType the name of an action with a | on either side to show the binding for that action.")]
    [SerializeField] [TextArea] private string hintMessage = "";

    private Vector2 originalPanelPos;
    private Vector2 offsetPanelPos;

    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.ControlSchemeChange>(OnControlSchemeChange);
    }
    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<EventDefiner.ControlSchemeChange>(OnControlSchemeChange);
    }
    private void OnControlSchemeChange(EventDefiner.ControlSchemeChange evt)
    {
        currentControlScheme = evt.ControlScheme;
        if (hintText.gameObject.activeInHierarchy)
        {
            hintText.text = FormatHintMessage(hintMessage);
        }
    }

    void Start()
    {
        actionMap = actionFromDesiredMap.action.actionMap;
        actionDict = new Dictionary<string, InputAction>();
        for (int i = 0; i < actionMap.actions.Count; i++)
        {
            actionDict.Add(actionMap.actions[i].name.ToLower(), actionMap.actions[i]);
        }

        //Make note of the original position, and set an offset position one panel's worth to the left.
        originalPanelPos = hintPanel.anchoredPosition;
        offsetPanelPos = new Vector2
        (
            hintPanel.anchoredPosition.x - hintPanel.rect.width,
            hintPanel.anchoredPosition.y
        );
        hintPanel.anchoredPosition = offsetPanelPos;

        //Set the hint message to what is typed in the inspector, and make the panel active if it isn't already.
        hintText.text = FormatHintMessage(hintMessage);
        hintPanel.gameObject.SetActive(true);

        //Make the panel slide onscreen, then slide offscreen.
        StartCoroutine(DisplayPanel(panelDisplayDuration, 0.5f));
    }

    private string FormatHintMessage(string message)
    {
        //Split the string using |, our template bar, for lack of a better term.
        string[] splitMessage = message.Split('|');

        //If the result has a length of 1, there are no bars.
        if (splitMessage.Length <= 1)
        {
            return message;
        }
        //If it's got an even length, it's missing a bar somewhere.
        else if (splitMessage.Length % 2 != 1)
        {
            Debug.LogWarning("HintManager: Hint message is missing a templating bar. Check the message " +
                    "and fix the bar mismatch.");
            return message;
        }

        //Now that we've checked for abnormalities, go through all the odd entries in splitMessage. These are the
        //things to be formatted. Check for what action they represent and replace them with their bindings.
        for (int i = 1; i < splitMessage.Length; i += 2)
        {
            string binding = GetBindingByName(splitMessage[i].ToLower());
            if (binding != "")
            {
                splitMessage[i] = binding;
            }
        }

        //We have the bindings, now format the bindings further.
        message = string.Concat(splitMessage);
        message = message.Replace(" | ", "/");

        return message;
    }

    private string GetBindingByName(string name)
    {
        //Check the dictionary to see if there's an action by the given name.
        if (actionDict.TryGetValue(name, out InputAction action))
        {
            //If so, return a display string of said action's binding, using the current control scheme as a mask.
            return action.GetBindingDisplayString
            (
                InputBinding.MaskByGroup(currentControlScheme),
                InputBinding.DisplayStringOptions.DontIncludeInteractions
            );
        }

        return "";
    }

    /// <summary>
    /// Move the panel onscreen, leave it onscreen for <paramref name="displayDuration"/> seconds, then move it 
    /// offscreen.
    /// </summary>
    /// <param name="displayDuration">How long to leave the panel onscreen before sliding away.</param>
    /// <param name="delay">How long to wait before actually moving the panel onscreen. Can circumvent
    /// <c>Time.unscaledDeltaTime</c> weirdness on scene startup.</param>
    /// <returns></returns>
    private IEnumerator DisplayPanel(float displayDuration, float delay = 0)
    {
        if (delay > 0) { yield return new WaitForSeconds(delay); }

        yield return StartCoroutine(MoveHintPanel(offsetPanelPos, originalPanelPos, panelMoveDuration));
        yield return new WaitForSeconds(displayDuration);
        StartCoroutine(MoveHintPanel(originalPanelPos, offsetPanelPos, panelMoveDuration));
    }

    /// <summary>
    /// Moves the panel from <paramref name="startPos"/> to <paramref name="endPos"/> smoothly, in 
    /// <paramref name="durationInSeconds"/> seconds.
    /// </summary>
    /// <param name="startPos">The starting position.</param>
    /// <param name="endPos">The ending position.</param>
    /// <param name="durationInSeconds">How long the move will take in seconds.</param>
    /// <returns></returns>
    private IEnumerator MoveHintPanel(Vector2 startPos, Vector2 endPos, float durationInSeconds)
    {
        float progress = 0;

        while (progress < 1)
        {
            progress += Time.unscaledDeltaTime / durationInSeconds;

            float newXPos = Mathf.SmoothStep(startPos.x, endPos.x, progress);
            hintPanel.anchoredPosition = new Vector2(newXPos, startPos.y);

            yield return null;
        }
    }
}
