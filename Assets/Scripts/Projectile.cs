using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public int speed = 8;
    public int lifetime = 3;

    int damage;
    Vector2 direction;

    public void Launch(Vector2 dir, int dmg)
    {
        direction = dir;
        damage = dmg;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
            
        var health = other.GetComponent<Health>();
        if (health == null) return;

        health.TakeDamage(damage);
        other.GetComponent<Knockback>()?.Apply(transform.position);
        Destroy(gameObject);
    }
}