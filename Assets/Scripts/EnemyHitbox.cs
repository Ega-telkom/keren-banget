using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public int damage = 10;
    public float damageCooldown = 1f;
    float damageTimer;

    void Update() => damageTimer -= Time.deltaTime;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (damageTimer > 0) return;

        damageTimer = damageCooldown;
        other.GetComponent<Health>()?.TakeDamage(damage);
    }
}