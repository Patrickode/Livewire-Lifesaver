using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public GameObject menuContainer;
    public GameObject howToPlayContainer;
    private bool showMenu = true;

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ToggleMenu()
    {
        if (showMenu)
        {
            showMenu = false;
            menuContainer.SetActive(false);
            howToPlayContainer.SetActive(true);
        }
        else
        {
            showMenu = true;
            menuContainer.SetActive(true);
            howToPlayContainer.SetActive(false);
        }
    }
}
