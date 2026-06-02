using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 20;
    public float knockbackOnEnemyDeath = 2.5f;

    // Referensi ke PlayerController untuk mengecek indeks kombo
    private PlayerController player;

    void Awake()
    {
        player = GetComponentInParent<PlayerController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyProjectile")) return;

        var enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy == null) return;

        var health = enemy.GetComponent<Health>();
        var knockback = enemy.GetComponent<Knockback>();

        // 1. Tentukan damage dasar berdasarkan kombo player
        int finalDamage = damage;
        float originalKnockbackForce = 5f; 

        if (player != null)
        {
            int currentCombo = player.GetComboIndex();
            if (knockback != null) originalKnockbackForce = knockback.knockbackForce;

            switch (currentCombo)
            {
                case 1:
                    finalDamage = damage; 
                    if (knockback != null) knockback.knockbackForce = originalKnockbackForce * 0.3f; 
                    break;
                case 2:
                    finalDamage = Mathf.RoundToInt(damage * 1.3f); 
                    if (knockback != null) knockback.knockbackForce = originalKnockbackForce * 0.3f; 
                    break;
                case 3:
                    finalDamage = Mathf.RoundToInt(damage * 2.0f); 
                    if (knockback != null) knockback.knockbackForce = originalKnockbackForce * 2.0f; 
                    break;
            }
        }

        // ==========================================================
        // LOGIKA BARU: HITUNG ARAH PENTALAN SECARA DINAMIS (X dan Y)
        // ==========================================================
        
        // Tentukan arah horizontal (X) dasar dari posisi pedang ke musuh
        float pushX = enemy.transform.position.x > transform.position.x ? 1f : -1f;
        float pushY = 0f; // Default: horizontal murni saat musuh masih hidup

        if (health != null && knockback != null)
        {
            // DETEKSI HIT TERAKHIR (MUSUH MAU MATI)
            if (health.currentHealth - finalDamage <= 0)
            {
                // Jika pukulan ini membunuh musuh, kalikan kekuatannya biar terpental jauh
                knockback.knockbackForce = originalKnockbackForce * knockbackOnEnemyDeath;
                
                // KUNCI: Berikan nilai Y ke atas hanya saat musuh mau mati!
                pushY = 0.7f; 
            }
        }

        Vector2 finalDir = new Vector2(pushX, pushY);
        
        // Spawn partikel darah luka (EnemyBlood)
        if (enemy.enemyBloodPrefab != null)
        {
            GameObject hitVisual = Instantiate(enemy.enemyBloodPrefab, enemy.transform.position, Quaternion.identity);
            Destroy(hitVisual, 2f); 
        }

        knockback?.Apply(finalDir);

        // 2. Baru kurangi darah musuh
        health?.TakeDamage(finalDamage);
        
        // 3. Berikan efek stun sejenak
        enemy.Stun();

        // 4. KEMBALIKAN nilai knockback asli agar musuh lain tidak ikut error kekuatannya
        if (knockback != null) knockback.knockbackForce = originalKnockbackForce;
    }
}