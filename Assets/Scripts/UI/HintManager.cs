using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    [SerializeField] private RectTransform hintPanel = null;
    [SerializeField] private TextMeshProUGUI hintText = null;

    [Tooltip("How long it should take the panel to move in and out of place, in seconds.")]
    [SerializeField] [Range(0f, 2f)] private float panelMoveDuration = 0.5f;
    [Tooltip("How long the panel should stay on screen before moving away, in seconds.")]
    [SerializeField] [Range(0.1f, 10f)] private float panelDisplayDuration = 5f;

    [Tooltip("The message / hint that should be in the hint panel." +
        "\n\nType the name of an action with a \"/\" on either side to show the binding for that action.")]
    [SerializeField] [TextArea] private string hintMessage = "";

    private Vector2 originalPanelPos;
    private Vector2 offsetPanelPos;

    void Start()
    {
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
        message = message.Replace("/Move/", "the WASD keys");
        message = message.Replace("/Swivel Camera/", "the Left / Right keys");
        return message;
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
