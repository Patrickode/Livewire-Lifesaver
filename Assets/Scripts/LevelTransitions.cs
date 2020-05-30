using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitions : MonoBehaviour
{
    [SerializeField] [Range(0.01f, 2f)] float panelAnimTime = 0.5f;

    [SerializeField] private RectTransform winPanel = null;
    [SerializeField] private RectTransform losePanel = null;

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
        StartCoroutine(ShowPanel(evt.LevelSuccess));
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

        //Now that the panel is on screen, prepare to move it offscreen again.
        yield return new WaitForSeconds(panelAnimTime);
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

        if (levelSuccess)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
