using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("References")]
    public PlayerController playerController;

    [Header("UI")]
    public TextMeshProUGUI coinText;

    [Header("Level Transition Settings")]
    public Animator transitionAnimator; 
    public float transitionTime = 1f;

    private bool _isTransitioning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        Debug.Log("[LOG] LevelManager: Fungsi Start dipanggil di awal game.");
        InitializeLevelComponents();
    }

    void InitializeLevelComponents()
    {
        _isTransitioning = false; 
        
        var reader = GameManager.instance.inputReader;
        UpdateCoinUI();

        // 1. Cari PlayerController baru yang segar di scene lokal
        playerController = FindAnyObjectByType<PlayerController>();
        
        if (playerController != null) 
        {
            Debug.Log("[LOG] LevelManager: Sukses menemukan Player baru. MEMANGGIL INITIALIZE PLAYER.");
            playerController.Initialize(reader);
        }
        else
        {
            Debug.LogError("[LOG] LevelManager: PlayerController tidak ditemukan di scene ini!");
        }
        
        // 2. Cari PauseManager lokal dan suapi InputReader
        // PauseManager localPauseManager = FindAnyObjectByType<PauseManager>();
        // if (localPauseManager != null)
        // {
        //     localPauseManager.Initialize(reader);
        // }

        // Jalankan SetGameplay setelah Player dan UI benar-benar selesai di-initialize
        if (reader != null)
        {
            reader.SetGameplay();
            Debug.Log("[LOG] LevelManager: Input Gameplay dinyalakan setelah seluruh komponen siap.");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[LOG] LevelManager: Scene baru berhasil di-load: " + scene.name);
    
        // JIKA SCENE ADALAH BOOT ATAU MAINMENU, JANGAN INSIALISASI KOMPONEN GAMEPLAY!
        if (scene.name == "Boot" || scene.name == "MainMenu")
        {
            // Tetap cari text koin jika ada di main menu (opsional)
            try {
                if (coinText == null) coinText = GameObject.FindWithTag("CoinText")?.GetComponent<TextMeshProUGUI>();
                UpdateCoinUI();
            } catch { /* mengantisipasi jika tag belum dibuat */ }
        
            return; // LANGSUNG KELUAR, JANGAN LANJUTKAN KODE DI BAWAH
        }

        // Hanya jalankan ini jika masuk ke scene permainan (Level_1, Level_2, dst)
        InitializeLevelComponents();

        try {
            if (coinText == null) coinText = GameObject.FindWithTag("CoinText")?.GetComponent<TextMeshProUGUI>();
            UpdateCoinUI();
        } catch { }

        if (scene.name == "Level_1") 
        {
            Debug.Log("<color=white>[LevelManager]</color> Ini Level 1, membatalkan EndTransition otomatis.");
            if (GameManager.instance.inputReader != null) GameManager.instance.inputReader.SetGameplay();
            return; 
        }

        if (transitionAnimator != null)
        {
            Debug.Log("[LOG] LevelManager: Memicu animasi EndTransition (Membuka layar).");
            transitionAnimator.SetTrigger("EndTransition");
            StartCoroutine(RestoreGameplayAfterOpen());
        }
    }

    IEnumerator RestoreGameplayAfterOpen()
    {
        yield return new WaitForSeconds(transitionTime);
        Debug.Log("<color=cyan>[LevelManager]</color> Layar terbuka sempurna. Gameplay dimulai!");
        if (GameManager.instance.inputReader != null) GameManager.instance.inputReader.SetGameplay();
    }

    // ==========================================
    //      FUNGSI BARU: PINDAH LEVEL OTOMATIS
    // ==========================================
    public void LoadNextLevel()
    {
        Debug.Log("[LOG] LevelManager: Fungsi LoadNextLevel dipanggil.");
        if (_isTransitioning) 
        {
            Debug.LogWarning("[LOG] LevelManager: Panggilan LoadNextLevel diabaikan karena sedang dalam proses transisi!");
            return;
        }

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Cek apakah index scene berikutnya ada di dalam Build Settings
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("[LOG] LevelManager: Memulai Coroutine transisi menuju index ke-" + nextSceneIndex);
            StartCoroutine(LoadLevelCoroutine(nextSceneIndex));
        }
        else
        {
            Debug.LogWarning("[LOG] LevelManager: Tidak ada scene berikutnya di Build Settings. Kembali ke MainMenu.");
            if (GameManager.instance != null) GameManager.instance.LoadMainMenu();
        }
    }

    private IEnumerator LoadLevelCoroutine(int sceneIndex)
    {
        _isTransitioning = true;

        // 1. Matikan input player agar karakter tidak bisa bergerak saat transisi layar berjalan
        if (GameManager.instance.inputReader != null)
        {
            Debug.Log("[LOG] LevelManager: Mengunci gerakan player ke SETUI sebelum layar menutup.");
            GameManager.instance.inputReader.SetUI(); // Mengunci pergerakan gameplay
        }

        // 2. Mainkan animasi layar menutup (Fade-Out)
        if (transitionAnimator != null)
        {
            Debug.Log("[LOG] LevelManager: Memicu animasi StartTransition (Menutup layar).");
            transitionAnimator.SetTrigger("StartTransition");
        }

        // 3. Tunggu sampai animasi menutup selesai sepenuhnya
        Debug.Log("[LOG] LevelManager: Memicu animasi StartTransition (Menutup layar).");
        yield return new WaitForSeconds(transitionTime);
        
        string sceneName = NameFromIndex(sceneIndex);
        Debug.Log("[LOG] LevelManager: Durasi transisi selesai. Memerintahkan GameManager memuat scene: " + sceneName);

        // 4. Perintahkan GameManager untuk memuat scene baru (sekaligus mereset koin)
        if (GameManager.instance != null)
        {
            // Mengambil nama scene berdasarkan index untuk dimasukkan ke fungsi GameManager
            GameManager.instance.LoadScene(sceneName);
        }
        else
        {
            // Terobosan darurat jika GameManager tidak aktif
            SceneManager.LoadScene(sceneIndex);
        }
    }

    // Fungsi pembantu untuk mendapatkan nama Scene dari angka indeksnya
    private string NameFromIndex(int BuildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
        int slash = path.LastIndexOf('/');
        string name = path.Substring(slash + 1);
        int dot = name.LastIndexOf('.');
        return name.Substring(0, dot);
    }

    public void AddCoin()
    {
        GameManager.instance.AddCoin();
        UpdateCoinUI();
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = "" + GameManager.instance.coinCount;
    }
}