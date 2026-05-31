using UnityEngine;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseCanvas;
    public PlayerController playerController;
    public GameObject firstSelected;
    public InputReader inputReader;
    
    Rigidbody2D playerRb;
    Vector2 savedVelocity;

    void Awake()
    {
        playerRb = playerController.GetComponent<Rigidbody2D>();
    }

    public void Initialize(InputReader reader)
    {
        inputReader = reader;
        inputReader.OnPausePerformed += TogglePause;
        inputReader.OnCancelPerformed += CancelPause;
    }

    void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.OnPausePerformed -= TogglePause;
            inputReader.OnCancelPerformed -= CancelPause;
        }
    }
    
    void CancelPause()
    {
        if (Time.timeScale == 0f)
            TogglePause();
    }
    
    public void TogglePause()
    {
        bool isPaused = Time.timeScale == 0f;

        if (isPaused)
        {
            Time.timeScale = 1f;
            playerRb.linearVelocity = savedVelocity;
            pauseCanvas.SetActive(false);
            inputReader.SetGameplay();
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            savedVelocity = playerRb.linearVelocity;
            Time.timeScale = 0f;
            playerRb.linearVelocity = Vector2.zero;
            pauseCanvas.SetActive(true);
            inputReader.SetUI();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }   
    }
    
    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        GameManager.instance.LoadMainMenu();
    }
}