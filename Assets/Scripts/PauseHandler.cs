using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseHandler : MonoBehaviour
{
    /// <summary>
    /// Whether the game is paused or not.
    /// </summary>
    public static bool Paused { get; private set; } = false;
    [SerializeField] private GameObject pauseMenu = null;

    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.PauseStateChange>(OnPauseStateChanged);

        if (!pauseMenu)
        {
            Debug.LogError("PauseHandler: No pause menu reference assigned. You usually need a pause " +
                "menu to pause.");
        }
    }
    private void OnDestroy()
    {
        //Set pause state to false when destroyed, such as when a new scene is loaded.
        EventDispatcher.Dispatch(new EventDefiner.PauseStateChange(false));

        EventDispatcher.RemoveListener<EventDefiner.PauseStateChange>(OnPauseStateChanged);
    }

    private void OnPauseStateChanged(EventDefiner.PauseStateChange evt) { SetPauseState(evt.Paused); }

    private void SetPauseState(bool paused)
    {
        Paused = paused;
        //If paused, set time scale to 0. Otherwise, set time scale to 1.
        Time.timeScale = paused ? 0 : 1;
        //Enable the pause menu.
        if (pauseMenu) { pauseMenu.SetActive(paused); }
    }
}
