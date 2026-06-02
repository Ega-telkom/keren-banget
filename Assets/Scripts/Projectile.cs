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
    
        var knockback = other.GetComponent<Knockback>();
        if (knockback != null)
        {
            // Hitung arah X saja, Y dipaksa 0 agar Player tidak membal ke atas
            float pushX = other.transform.position.x > transform.position.x ? 1f : -1f;
            Vector2 customDir = new Vector2(pushX, 0f); 
        
            knockback.Apply(customDir);
        }

        Destroy(gameObject);
    }
}