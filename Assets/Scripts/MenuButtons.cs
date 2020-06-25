using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    private GameObject currentMenu = null;

    private void Start()
    {
        //Get all children of the canvas, and make the first active menu the currentMenu.
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy && child.CompareTag("Menu"))
            {
                currentMenu = child.gameObject;
                break;
            }
        }
    }

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
        EventDispatcher.Dispatch(new EventDefiner.MenuSwap());

        //Make destination active, and the current menu inactive. Destination is now the current menu.
        destination.SetActive(true);
        currentMenu.SetActive(false);
        currentMenu = destination;
    }
}
