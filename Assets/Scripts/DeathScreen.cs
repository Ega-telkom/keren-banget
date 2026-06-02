using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class DeathScreen : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject deathCanvasPanel; // Tarik objek 'DeathScreen' (Anak UI) ke sini
    public GameObject firstSelectedButton; // Tarik tombol Retry ke sini

    [Header("Buttons")]
    public Button retryButton;
    public Button quitButton;

    void Start()
    {
        // Otomatis sembunyikan panel saat level dimulai
        if (deathCanvasPanel != null)
            deathCanvasPanel.SetActive(false);

        // Pasang fungsi tombol secara otomatis via kode
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RestartLevel);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitToMainMenu);
        }
    }

    public void ShowDeathScreen()
    {
        Debug.Log("DeathScreen: Membuka layar kematian.");
        if (deathCanvasPanel != null)
        {
            deathCanvasPanel.SetActive(true);
            
            if (GameManager.instance != null && GameManager.instance.inputReader != null)
            {
                GameManager.instance.inputReader.SetUI();
            }

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        // Biarkan script masing-masing yang membersihkan dirinya melalui OnDisable
        // Kita tidak perlu memanggil ClearAllSubscribers() secara paksa di sini lagi.

        // Muat ulang level
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        if (GameManager.instance != null) GameManager.instance.LoadMainMenu();
        else SceneManager.LoadScene(0);
    }
}