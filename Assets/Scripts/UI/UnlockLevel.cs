using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

public class UnlockLevel : MonoBehaviour
{
    [SerializeField] private Button button = null;
    [Space(10)]
    [SerializeField] private int index = -1;

    void Start()
    {
        if (index > SaveManager.CachedData.LastCompletedIndex + 1)
        {
            button.interactable = false;
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Keyboard.current.digit1Key.isPressed && Keyboard.current.digit3Key.isPressed
            && Keyboard.current.numpad1Key.isPressed && Keyboard.current.numpad3Key.isPressed)
        {
            button.interactable = true;
        }
    }
#endif
}
