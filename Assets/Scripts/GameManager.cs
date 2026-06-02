using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Input")]
    public InputReader inputReader;
    public InputModeManager inputModeManager;

    [Header("Game Data")]
    public int coinCount = 0;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
            inputModeManager.SetUI();
        else
            inputModeManager.SetGameplay();
    }

    // ===== COINS =====
    public void AddCoin() => coinCount++;
    public void ResetCoins() => coinCount = 0;

    // ===== SCENE MANAGEMENT =====
    public void LoadScene(string sceneName)
    {
        ResetCoins();
        SceneManager.LoadScene(sceneName);
    }
    public void LoadMainMenu() => SceneManager.LoadScene("MainMenu");
    public void QuitGame() => Application.Quit();
}