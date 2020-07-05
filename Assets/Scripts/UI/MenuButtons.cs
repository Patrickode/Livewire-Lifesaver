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

    /// <summary>
    /// Goes through all of this object's children, and sets currentMenu equal to the first active menu.
    /// Also sets defaultMenu the first time through.
    /// This will only happen once.
    /// </summary>
    private void GetCurrentMenu()
    {
        //Only get the current menu if currentMenu is uninitialized / null.
        if (!currentMenu)
        {
            //Get all children of the canvas, and set currentMenu equal to the first active menu.
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeInHierarchy && child.CompareTag("Menu"))
                {
                    currentMenu = child.gameObject;
                    if (!defaultMenu) { defaultMenu = child.gameObject; }
                    return;
                }
            }
        }
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

    public void SetMenuActive(bool active)
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
}
