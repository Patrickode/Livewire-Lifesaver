using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour
{
    private GameObject currentMenu = null;
    private GameObject defaultMenu = null;
    private GameObject selectAfterSwap = null;
    private bool shouldFlashOnSwap = true;

    private EventSystem eventSyst = null;
    private EventSystem EventSyst
    {
        get
        {
            if (!eventSyst)
            {
                eventSyst = EventSystem.current;

                if (!eventSyst)
                {
                    Debug.LogError("MenuButtons: No event system found. There should be " +
                        "an event system if buttons are needed.");
                }
            }

            return eventSyst;
        }
        set { eventSyst = value; }
    }

    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.PauseStateChange>(OnPauseStateChange);
    }
    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<EventDefiner.PauseStateChange>(OnPauseStateChange);
    }

    //---Main Methods---//

    private void Start()
    {
        GetCurrentMenu();
        defaultMenu = currentMenu;
    }

    private void OnPauseStateChange(EventDefiner.PauseStateChange evt)
    {
        if (!evt.Paused && currentMenu != defaultMenu) { SwapMenu(defaultMenu, false); }
    }

    /// <summary>
    /// Loads a scene by its index. If <paramref name="index"/> is negative, this method will load the scene
    /// with the index "currentSceneIndex - <paramref name="index"/>," or 
    /// "currentSceneIndex + |<paramref name="index"/>|."
    /// </summary>
    /// <param name="index">The index of the scene to load.</param>
    public void LoadScene(int index)
    {
        int indexToLoad = index >= 0 ? index : SceneManager.GetActiveScene().buildIndex - index;
        EventDispatcher.Dispatch(new EventDefiner.MenuExit(indexToLoad));
    }

    public void QuitGame() { Application.Quit(); }

    public void ResumeGame() { EventDispatcher.Dispatch(new EventDefiner.PauseStateChange(false)); }

    public void SwapMenu(GameObject destination)
    {
        SwapMenu(destination, shouldFlashOnSwap, selectAfterSwap);
        selectAfterSwap = null;
        shouldFlashOnSwap = true;
    }

    public void RestartScene()
    {
        EventDispatcher.Dispatch(new EventDefiner.MenuExit(SceneManager.GetActiveScene().buildIndex, 0.35f));
    }

    public void SelectAfterSwap(GameObject objToSelect) { selectAfterSwap = objToSelect; }
    public void DontFlashOnSwap() { shouldFlashOnSwap = false; }

    /// <summary>
    /// Swaps to another menu and sets the selection to the first selectable object in that menu.
    /// <para>Can optionally be set to not trigger a flash, or set to select a particular GameObject 
    /// after swapping.</para>
    /// </summary>
    /// <param name="destination">The menu to swap to.</param>
    /// <param name="shouldFlash">Whether a flash transition should happend during the swap.</param>
    /// <param name="objToSelect">The object to select after swapping. Defaults to the first selectable 
    /// child of <paramref name="destination"/>.</param>
    public void SwapMenu(GameObject destination, bool shouldFlash = true, GameObject objToSelect = null)
    {
        //If we don't have a reference to any menu in currentMenu, get one.
        //If currentMenu was assigned, assign defaultMenu as well.
        if (GetCurrentMenu()) { defaultMenu = currentMenu; }

        //If destination isn't null...
        if (destination)
        {
            EventDispatcher.Dispatch(new EventDefiner.MenuSwap(shouldFlash));

            //Make destination active, and the current menu inactive. Destination is now the current menu.
            destination.SetActive(true);
            currentMenu.SetActive(false);
            currentMenu = destination;

            //If objToSelect is not null, select it. If it is, get the first selectable in current menu and
            //select that.
            if (EventSyst)
            {
                EventSyst.SetSelectedGameObject(objToSelect ? objToSelect : GetFirstSelectable(currentMenu));
            }
        }
    }

    //---Helper Methods---//

    /// <summary>
    /// Goes through all of this object's children, and sets currentMenu equal to the first active menu.
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

    /// <summary>
    /// Goes through <paramref name="parent"/>'s children and returns the first selectable child object.
    /// Returns null if no such child exists.
    /// </summary>
    /// <param name="parent">The object to find a selectable child in.</param>
    /// <returns>The first selectable child of <paramref name="parent"/>. May be null.</returns>
    private GameObject GetFirstSelectable(GameObject parent) { return GetFirstSelectable(parent.transform); }
    /// <summary>
    /// Goes through <paramref name="parent"/>'s children and returns the first selectable child object.
    /// Returns null if no such child exists.
    /// </summary>
    /// <param name="parent">The transform of the object to find a selectable child in.</param>
    /// <returns>The first selectable child of <paramref name="parent"/>. May be null.</returns>
    private GameObject GetFirstSelectable(Transform parent)
    {
        //For each child in the supplied object,
        foreach (Transform child in parent)
        {
            //check if it's selectable, and return it if so.
            if (child.TryGetComponent(out Selectable _))
            {
                return child.gameObject;
            }
            //If it's not selectable, this child might still have a selectable child.
            else
            {
                if (child.childCount > 0)
                {
                    //Check if this child has any selectable children, and return the first one if it does.
                    GameObject result = GetFirstSelectable(child);
                    if (result) { return result; }
                }
            }
        }

        //If we made it this far, then no child was selectable.
        return null;
    }
}
