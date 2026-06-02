using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    // --- EVENTS ---
    public event Action<float> OnHealthChanged; 
    public event Action<float> OnDamageTaken;    
    public event Action OnDeath;

    [Header("Effects & Visuals")]
    public GameObject damageNumberPrefab;
    public Color damageNumberColor = Color.yellow;
    
    [Header("Particles (GameObject Prefabs)")]
    public GameObject bloodParticlePrefab; // SEKARANG BERUPA GAMEOBJECT
    public GameObject deathParticlePrefab; // SEKARANG BERUPA GAMEOBJECT

    void Awake() => currentHealth = maxHealth;

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        Debug.Log("Health: " + gameObject.name + " menerima damage " + amount + ". Sisa darah: " + currentHealth);
        
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        OnDamageTaken?.Invoke(amount); 

        GetComponent<HitFlash>()?.Flash();
        if (damageNumberPrefab != null)
        {
            GameObject go = Instantiate(damageNumberPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity); 
            go.GetComponent<DamageNumber>()?.Init(amount, damageNumberColor);
        }

        if (bloodParticlePrefab != null)
        {
            GameObject bloodGO = Instantiate(bloodParticlePrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = bloodGO.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();

            // SOLUSI: Ambil durasi burst, lalu tambahkan 1.5 detik ekstra untuk toleransi animasi Size over Lifetime
            float totalLifetime = (ps != null) ? (ps.main.duration + 1.5f) : 2f;
            Destroy(bloodGO, totalLifetime);
        }

        if (currentHealth <= 0)
        {
            Debug.Log("Health: Darah " + gameObject.name + " habis (0). Memicu proses kematian.");
            if (!CompareTag("Enemy")) 
            {
                HandleDeathVisuals(); 
            }
            
            OnDeath?.Invoke();
        }
    }

    private void HandleDeathVisuals()
    {
        // Pemicu partikel mati khusus Player
        if (deathParticlePrefab != null)
        {
            GameObject deathGO = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
            
            ParticleSystem ps = deathGO.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();

            float duration = ps != null ? ps.main.duration : 2f;
            Destroy(deathGO, duration);
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }
}