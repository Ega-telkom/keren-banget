using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    public event Action<float> OnHealthChanged; // 0-1 normalized
    public event Action OnDeath;
    public GameObject damageNumberPrefab;
    public Color damageNumberColor = Color.yellow;

    void Awake() => currentHealth = maxHealth;

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0)
            OnDeath?.Invoke();
        
        GetComponent<HitFlash>()?.Flash();
        GameObject go = Instantiate(damageNumberPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity); 
        go.GetComponent<DamageNumber>()?.Init(amount, damageNumberColor);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }
}