using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseHandler : MonoBehaviour
{
    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.PauseInput>(OnPauseInput);
        EventDispatcher.AddListener<EventDefiner.PauseMenuResumeClicked>(OnPauseInput);

        EventDispatcher.AddListener<EventDefiner.LevelEnd>(OnLevelTransition);
        EventDispatcher.AddListener<EventDefiner.MenuExit>(OnLevelTransition);
    }
    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<EventDefiner.PauseInput>(OnPauseInput);
        EventDispatcher.RemoveListener<EventDefiner.PauseMenuResumeClicked>(OnPauseInput);

        EventDispatcher.RemoveListener<EventDefiner.LevelEnd>(OnLevelTransition);
        EventDispatcher.RemoveListener<EventDefiner.MenuExit>(OnLevelTransition);
    }

    private void OnPauseInput(EventDefiner.PauseInput evt) { SetPauseState(evt.Paused); }
    private void OnPauseInput(EventDefiner.PauseMenuResumeClicked evt) { SetPauseState(false); }

    private void OnLevelTransition(EventDefiner.LevelEnd _) { SetPauseState(false); }
    private void OnLevelTransition(EventDefiner.MenuExit _) { SetPauseState(false); }


    private void SetPauseState(bool paused)
    {
        //If paused, set time scale to 0. Otherwise, set time scale to 1.
        Time.timeScale = paused ? 0 : 1;
    }
}
