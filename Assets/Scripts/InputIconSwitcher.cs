using UnityEngine;
using UnityEngine.InputSystem;

public class InputIconSwitcher : MonoBehaviour
{
    public GameObject gamepadIcon;
    public GameObject keyboardIcon;

    private bool usingGamepad = false;

    void Update()
    {
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
            usingGamepad = true;
        if (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame)
            usingGamepad = false;

        gamepadIcon.SetActive(usingGamepad);
        keyboardIcon.SetActive(!usingGamepad);
    }
}