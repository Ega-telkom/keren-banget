using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseCanvas;
    public PlayerController playerController;
    public GameObject firstSelected;
    public InputReader inputReader;
    
    [Header("UI Buttons")]
    public Button resumeButton; 
    public Button quitButton;   
    
    private Rigidbody2D playerRb;
    private Vector2 savedVelocity;
    
    // Tambahkan fungsi Start() ini di dalam PauseManager.cs
    void Start()
    {
        // Mengambil InputReader langsung dari GameManager yang dibawa _GameSystem
        if (GameManager.instance != null && GameManager.instance.inputReader != null)
        {
            Initialize(GameManager.instance.inputReader);
            Debug.Log("<color=yellow>[PauseManager]</color> Otomatis terhubung dengan InputReader dari _GameSystem!");
        }
        else
        {
            Debug.LogError("<color=red>[PauseManager]</color> Gagal menemukan GameManager atau InputReader di scene ini!");
        }
    }   

    // Dipanggil otomatis oleh LevelManager saat scene baru dimuat
    public void Initialize(InputReader reader)
    {
        // Cabut event lama jika ada untuk mencegah double-binding
        if (inputReader != null)
        {
            inputReader.OnPausePerformed -= TogglePause;
            inputReader.OnCancelPerformed -= CancelPause;
        }

        inputReader = reader;
        
        // Daftarkan ke InputReader yang baru
        if (inputReader != null)
        {
            inputReader.OnPausePerformed += TogglePause;
            inputReader.OnCancelPerformed += CancelPause;
        }

        // Cari Player & Rigidbody lokal di scene saat ini
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }

        if (playerController != null)
            playerRb = playerController.GetComponent<Rigidbody2D>();

        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(TogglePause);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitToMenu);
        }
    }

    void OnDisable()
    {
        // Pengaman: Cabut event saat scene dihancurkan/di-restart agar tidak bocor ke scene berikutnya
        if (inputReader != null)
        {
            inputReader.OnPausePerformed -= TogglePause;
            inputReader.OnCancelPerformed -= CancelPause;
        }
    }

    void OnDestroy()
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
        if (pauseCanvas == null) return;

        // ====================================================================
        // PENGAMAN: Jika player terdeteksi mati, JANGAN izinkan pause menu terbuka!
        // ====================================================================
        if (playerController != null && playerController.IsDead)
        {
            Debug.LogWarning("[PauseManager] Pause ditolak karena Player sudah mati/DeathScreen aktif.");
            return; 
        }
        // ====================================================================

        bool isPaused = Time.timeScale == 0f;

        if (isPaused)
        {
            Time.timeScale = 1f;
            if (playerRb != null) playerRb.linearVelocity = savedVelocity;
            pauseCanvas.SetActive(false);
            if (inputReader != null) inputReader.SetGameplay();
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            if (playerRb != null) savedVelocity = playerRb.linearVelocity;
            Time.timeScale = 0f;
            if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
            pauseCanvas.SetActive(true);
            if (inputReader != null) inputReader.SetUI();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }   
    }
    
    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        if (GameManager.instance != null) GameManager.instance.LoadMainMenu();
    }
}