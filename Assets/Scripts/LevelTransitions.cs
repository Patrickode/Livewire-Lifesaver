using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitions : MonoBehaviour
{
    [SerializeField] private GameObject winPanel = null;
    [SerializeField] private GameObject losePanel = null;

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
        StartCoroutine(DoLevelTransition(evt.LevelSuccess));
    }

    private IEnumerator DoLevelTransition(bool levelSuccess)
    {
        if (levelSuccess)
        {
            winPanel.SetActive(true);
            yield return new WaitForSeconds(3);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            losePanel.SetActive(true);
            yield return new WaitForSeconds(3);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
