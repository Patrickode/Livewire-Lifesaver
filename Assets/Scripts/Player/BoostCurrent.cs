using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostCurrent : MonoBehaviour
{
    private bool levelEnding = false;

    private void Awake()
    {
        EventDispatcher.AddListener<EventDefiner.LevelEnd>(OnLevelEnd);
    }
    private void OnDestroy()
    {
        EventDispatcher.RemoveListener<EventDefiner.LevelEnd>(OnLevelEnd);
    }

    private void OnLevelEnd(EventDefiner.LevelEnd _)
    {
        levelEnding = true;
    }

    private void Update()
    {
        //We only need to fire these events if the level is still going.
        if (!levelEnding)
        {
            //When the player presses shift, dispatch a boost event that says we're boosting.
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                EventDispatcher.Dispatch(new EventDefiner.CurrentBoost(true));
            }
            //When the player releases shift, dispatch a boost event that says we're not boosting.
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                EventDispatcher.Dispatch(new EventDefiner.CurrentBoost(false));
            }
        }
    }
}
