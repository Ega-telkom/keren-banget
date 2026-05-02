using UnityEngine;
using TMPro; // atau using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int coinCount = 0;
    public TextMeshProUGUI coinText; // atau public Text orbText;

    void Awake()
    {
        instance = this;
    }

    public void AddCoin()
    {
        coinCount++;
        UpdateUI();
    }

    void UpdateUI()
    {
        coinText.text = "" + coinCount;
    }
}