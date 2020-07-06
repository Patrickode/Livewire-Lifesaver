using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [Tooltip("The object that contains every menu.")]
    [SerializeField] private GameObject menuObject = null;
    private GameObject currentMenu = null;
    private GameObject defaultMenu = null;

    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.PauseInput>(OnPauseInput);
    }
    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<EventDefiner.PauseInput>(OnPauseInput);
    }

    /// <summary>
    /// Goes through all of this object's children, and sets currentMenu equal to the first active menu.
    /// Also sets defaultMenu the first time through.
    /// This will only happen once.
    /// </summary>
    /// <param name="transformToCheck">The transform to look in. Defaults to this script's transform.</param>
    /// <returns>Whether currentMenu was assigned or not.</returns>
    private bool GetCurrentMenu(Transform transformToCheck = null)
    {
        //If transformToCheck is null, use this script's transform instead. Otherwise, just use transformToCheck.
        transformToCheck = transformToCheck ? transformToCheck : transform;

        //Only get the current menu if currentMenu is uninitialized / null.
        if (!currentMenu)
        {
            //For each child of this object,
            foreach (Transform child in transformToCheck)
            {
                //Check if its active. (We don't need to check its children, because they'll be inactive if this
                //is inactive.)
                if (child.gameObject.activeInHierarchy)
                {
                    //If it is, and it's a menu, we found the first active menu. Set currentMenu equal to it.
                    if (child.CompareTag("Menu"))
                    {
                        currentMenu = child.gameObject;
                        if (!defaultMenu) { defaultMenu = child.gameObject; }
                        return true;
                    }
                    //If it's not a menu, it's a container; check all of its children, and if we find
                    //a menu when we do, return true.
                    else
                    {
                        if (GetCurrentMenu(child)) { return true; }
                    }
                }
            }
        }

        return false;
    }

    private void Start() { GetCurrentMenu(); }

    public void LoadScene(int index)
    {
        EventDispatcher.Dispatch(new EventDefiner.MenuExit(index));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SwapMenu(GameObject destination)
    {
        //If we don't have a reference to any menu in currentMenu, get one.
        GetCurrentMenu();

        //If destination isn't null...
        if (destination)
        {
            EventDispatcher.Dispatch(new EventDefiner.MenuSwap());

            //Make destination active, and the current menu inactive. Destination is now the current menu.
            destination.SetActive(true);
            currentMenu.SetActive(false);
            currentMenu = destination;
        }
    }

    public void ResumeGame()
    {
        EventDispatcher.Dispatch(new EventDefiner.PauseMenuResumeClicked());
        SetMenuActive(false);
    }
    private void SetMenuActive(bool active)
    {
        if (menuObject) { menuObject.SetActive(active); }

        //If the menu was turned on, and currentMenu is not the default menu, switch to the default menu.
        if (active && currentMenu != defaultMenu)
        {
            defaultMenu.SetActive(true);
            currentMenu.SetActive(false);
            currentMenu = defaultMenu;
        }
    }
    private void OnPauseInput(EventDefiner.PauseInput evt) { SetMenuActive(evt.Paused); }
}
