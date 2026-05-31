using UnityEngine;

public class Stamina : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float idleRegenRate = 20f;  // Lebih cepat saat diam
    [SerializeField] private float walkRegenRate = 10f;  // Lebih lambat saat jalan
    [SerializeField] private float regenDelay = 1.0f;

    private float currentStamina;
    private float nextRegenTime;
    
    // Properti untuk dikontrol oleh PlayerController
    public bool IsMoving { get; set; } 
    public System.Action OnStaminaChanged;

    void Awake()
    {
        // Isi data sebelum script lain (seperti UI) mengaksesnya
        currentStamina = maxStamina;
    }

    void Start()
    {
        // Opsional: Kamu bisa Invoke di sini untuk memastikan UI sinkron
        OnStaminaChanged?.Invoke();
    }

    void Update()
    {
        if (Time.time >= nextRegenTime && currentStamina < maxStamina)
        {
            // Tentukan rate berdasarkan status gerak
            float currentRate = IsMoving ? walkRegenRate : idleRegenRate;
            
            currentStamina += currentRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            
            OnStaminaChanged?.Invoke();
        }
    }

    public bool HasEnough(float amount) => currentStamina >= amount;

    public void Drain(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        nextRegenTime = Time.time + regenDelay;
        OnStaminaChanged?.Invoke();
    }

    public float GetPercentage() => currentStamina / maxStamina;
}