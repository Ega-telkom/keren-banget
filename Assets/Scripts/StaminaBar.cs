using UnityEngine;
using UnityEngine.UI; // Wajib untuk Image

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private Image staminaFill; // Drag Image dengan type 'Filled' ke sini
    [SerializeField] private Stamina playerStamina; // Drag object Player ke sini

    void Start()
    {
        // Berlangganan ke event, jadi UI hanya update saat dipanggil
        playerStamina.OnStaminaChanged += UpdateBar;
        UpdateBar(); // Update awal
    }

    void UpdateBar()
    {
        staminaFill.fillAmount = playerStamina.GetPercentage();
    }
}