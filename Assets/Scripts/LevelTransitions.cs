using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelTransitions : MonoBehaviour
{
    [SerializeField] [Range(0.01f, 2f)] float panelAnimTime = 1f;
    [SerializeField] [Range(0.01f, 1f)] float fadeInTime = 0.1f;
    [SerializeField] [Range(0.01f, 5f)] float fadeOutTime = 2.5f;

    [SerializeField] private RectTransform winPanel = null;
    [SerializeField] private RectTransform losePanel = null;
    [SerializeField] private Image fadePanel = null;

    private bool isFading = false;

    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.LevelEnd>(OnLevelEnd);
    }

    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<EventDefiner.LevelEnd>(OnLevelEnd);
    }

    private void Start()
    {
        StartCoroutine(FadeBetween(Color.white, new Color(1, 1, 1, 0), fadeInTime));
    }

    private void OnLevelEnd(EventDefiner.LevelEnd evt)
    {
        StartCoroutine(ShowPanel(evt.LevelSuccess));
        StartCoroutine(FadeBetween(Color.clear, Color.black, fadeOutTime));
    }

    private IEnumerator ShowPanel(bool levelSuccess)
    {
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

    private IEnumerator FadeBetween(Color fromColor, Color toColor, float fadeTime)
    {
        isFading = true;

        //Ensure the fade panel is clear and ready to be faded in
        fadePanel.color = fromColor;
        fadePanel.gameObject.SetActive(true);

        //Set up a progress variable and gradually lerp the fade panel's opacity
        float progress = 0f;
        while (progress < 1)
        {
            progress += Time.deltaTime / fadeTime;
            fadePanel.color = Color.Lerp(fromColor, toColor, progress);
            yield return null;
        }

        //If we're fading to a fully transparent color, then disable the panel, because it won't be seen anyway
        if (Mathf.Approximately(toColor.a, 0)) { fadePanel.gameObject.SetActive(false); }

        isFading = false;
    }
}
