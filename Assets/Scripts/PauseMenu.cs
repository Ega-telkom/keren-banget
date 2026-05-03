using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseCanvas;
    public PlayerController playerController;
    public PlayerInput playerInput;
    public GameObject firstSelected;
    private bool isPaused = false;

    public void OnPause(InputValue value)
    {
        if (value.isPressed)
            TogglePause();
    }

    public void Resume()
    {
        if (isPaused)
            TogglePause();
    }
    
    public void OnUnpause(InputValue value)
    {
        if (value.isPressed)
            TogglePause();
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        pauseCanvas.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        playerController.enabled = !isPaused;

        if (isPaused)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
            playerInput.actions.FindActionMap("Player").Disable();
            playerInput.actions.FindActionMap("UI").Enable();
        }
        else
        {
            playerInput.actions.FindActionMap("UI").Disable();
            playerInput.actions.FindActionMap("Player").Enable();
        }
    }
}