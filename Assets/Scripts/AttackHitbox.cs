using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 20;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyProjectile")) return;

        var enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy == null) return;

        enemy.GetComponent<Health>()?.TakeDamage(damage);
        enemy.GetComponent<Knockback>()?.Apply(transform.position);
        enemy.Stun();
    }
}