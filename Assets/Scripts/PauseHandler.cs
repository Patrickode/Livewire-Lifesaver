using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseHandler : MonoBehaviour
{
    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.PauseInput>(OnPauseInput);
        EventDispatcher.AddListener<EventDefiner.PauseMenuResumeClicked>(OnPauseInput);
    }
    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<EventDefiner.PauseInput>(OnPauseInput);
        EventDispatcher.RemoveListener<EventDefiner.PauseMenuResumeClicked>(OnPauseInput);
    }

    private void OnPauseInput(EventDefiner.PauseInput evt)
    {
        SetTimeScale(evt.Paused);
    }
    private void OnPauseInput(EventDefiner.PauseMenuResumeClicked evt)
    {
        SetTimeScale(false);
    }

    private void SetTimeScale(bool paused)
    {
        //If paused, set time scale to 0. Otherwise, set time scale to 1.
        Time.timeScale = paused ? 0 : 1;
    }
}
