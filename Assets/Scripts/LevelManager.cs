using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("References")]
    public PauseManager pauseManager;
    public PlayerController playerController;

    [Header("UI")]
    public TextMeshProUGUI coinText;

    void OnEnable()
    {
        // Sambungkan PauseManager ke InputReader dari GameManager
        pauseManager.inputReader = GameManager.instance.inputReader;
    }

    void Start()
    {
        GameManager.instance.inputReader.SetGameplay();
        UpdateCoinUI();
        var reader = GameManager.instance.inputReader;
        playerController.Initialize(reader);
        pauseManager.Initialize(reader);
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