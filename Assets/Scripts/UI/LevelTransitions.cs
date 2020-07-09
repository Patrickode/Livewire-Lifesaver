using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelTransitions : MonoBehaviour
{
    [SerializeField] [Range(0.01f, 2f)] private float panelAnimTime = 1f;
    [SerializeField] [Range(0.01f, 1f)] private float fadeInTime = 0.1f;
    [SerializeField] [Range(0.01f, 5f)] private float levelFadeOutTime = 2.5f;
    [SerializeField] [Range(0.01f, 5f)] private float menuFadeOutTime = 1.25f;
    [SerializeField] private RectTransform winPanel = null;
    [SerializeField] private RectTransform losePanel = null;
    [SerializeField] private Image fadePanel = null;

    private bool isFading = false;
    private bool showingPanel = false;

    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.LevelEnd>(OnLevelEnd);
        EventDispatcher.AddListener<EventDefiner.MenuExit>(OnMenuExit);
        EventDispatcher.AddListener<EventDefiner.MenuSwap>(OnMenuSwap);
    }

    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<EventDefiner.LevelEnd>(OnLevelEnd);
        EventDispatcher.RemoveListener<EventDefiner.MenuExit>(OnMenuExit);
        EventDispatcher.RemoveListener<EventDefiner.MenuSwap>(OnMenuSwap);
    }

    private void Start()
    {
        StartCoroutine(FadeBetween(Color.white, new Color(1, 1, 1, 0), fadeInTime));
    }

    private void OnLevelEnd(EventDefiner.LevelEnd evt)
    {
        StartCoroutine(ShowPanel(evt.LevelSuccess));
        StartCoroutine(FadeBetween(Color.clear, Color.black, levelFadeOutTime));
    }

    private void OnMenuExit(EventDefiner.MenuExit evt)
    {
        StartCoroutine(LoadSceneAfterFade(evt.DestinationSceneIndex));
    }
    private IEnumerator LoadSceneAfterFade(int sceneIndex)
    {
        yield return StartCoroutine(FadeBetween(Color.clear, Color.black, menuFadeOutTime));
        SceneManager.LoadScene(sceneIndex);
    }

    private void OnMenuSwap(EventDefiner.MenuSwap evt)
    {
        if (evt.ShouldFlash)
        {
            StartCoroutine(FadeBetween(Color.white, new Color(1, 1, 1, 0), fadeInTime));
        }
    }

    /// <summary>
    /// Moves the win or loss panel up as part of level transition.
    /// </summary>
    /// <param name="levelSuccess">Whether the player succeeded in finishing the level or not.</param>
    /// <returns></returns>
    private IEnumerator ShowPanel(bool levelSuccess)
    {
        //We don't want to show this level end panel twice, so exit early if we already are showing a panel.
        if (showingPanel) { yield break; }
        showingPanel = true;

        //Depending on whether the player succeeded, select the right panel to animate.
        RectTransform panelToShow = levelSuccess ? winPanel : losePanel;

        //Now get its position and make an offset vector with the screen height, so we can hide
        //the panel offscreen before enabling.
        Vector2 originalPos = panelToShow.anchoredPosition;
        Vector2 offsetPos = new Vector2(originalPos.x, originalPos.y - Screen.height);
        panelToShow.anchoredPosition = offsetPos;

        //Make the panel active and set up a progress variable for lerping
        panelToShow.gameObject.SetActive(true);
        float progress = 0f;

        //While we haven't reached the destination,
        while (progress < 1)
        {
            //Move progress forward a step, then interpolate between our points using it
            progress += Time.deltaTime / panelAnimTime;
            float newY = Mathf.SmoothStep(offsetPos.y, originalPos.y, progress);

            panelToShow.anchoredPosition = new Vector2(originalPos.x, newY);
            yield return null;
        }

        //Wait until the screen is done fading out to continue.
        yield return new WaitUntil(() => isFading == false);

        //We're done showing the panel, so prepare to move it offscreen, in the same direction it came in.
        offsetPos = new Vector2(originalPos.x, originalPos.y + Screen.height);
        progress = 0f;

        //While we haven't reached the destination,
        while (progress < 1)
        {
            //Move progress forward a step, then interpolate between our points using it
            progress += Time.deltaTime / panelAnimTime;
            float newY = Mathf.SmoothStep(originalPos.y, offsetPos.y, progress);

            panelToShow.anchoredPosition = new Vector2(originalPos.x, newY);
            yield return null;
        }

        //Finally, now that all the transitioning is done, load the appropriate scene.
        if (levelSuccess)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    /// <summary>
    /// Makes a panel in the transition canvas fade between two colors in a given amount of seconds.
    /// </summary>
    /// <param name="fromColor">The color to fade from.</param>
    /// <param name="toColor">The color to fade to.</param>
    /// <param name="fadeTime">The amount of time in seconds the fade will take.</param>
    /// <returns></returns>
    private IEnumerator FadeBetween(Color fromColor, Color toColor, float fadeTime)
    {
        if (isFading) { yield break; }
        isFading = true;

        //Ensure the fade panel is clear and ready to be faded in
        fadePanel.color = fromColor;
        fadePanel.gameObject.SetActive(true);

        //Wait a few frames to let unscaledDeltaTime even out; it doesn't work properly on scene startup otherwise
        for (int i = 0; i < 5; i++) { yield return new WaitForEndOfFrame(); }

        //Set up a progress variable and gradually lerp the fade panel's opacity
        float progress = 0f;
        while (progress < 1)
        {
            progress += Time.unscaledDeltaTime / fadeTime;
            fadePanel.color = Color.Lerp(fromColor, toColor, progress);
            yield return null;
        }

        //If we're fading to a fully transparent color, then disable the panel, because it won't be seen anyway
        if (Mathf.Approximately(toColor.a, 0)) { fadePanel.gameObject.SetActive(false); }

        isFading = false;
    }

    /// <summary>
    /// Calls FadeBetween() twice in direct sequence.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeInOut(Color fromColor1, Color toColor1, float fadeTime1,
        Color fromColor2, Color toColor2, float fadeTime2)
    {
        yield return StartCoroutine(FadeBetween(fromColor1, toColor1, fadeTime1));
        StartCoroutine(FadeBetween(fromColor2, toColor2, fadeTime2));
    }
}
