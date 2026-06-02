using UnityEngine;

public class Knockback : MonoBehaviour
{
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    Rigidbody2D rb;
    public bool isKnockedBack;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    // SEKARANG MENERIMA ARAH (dir) LANGSUNG DARI LUAR
    public void Apply(Vector2 dir)
    {
        if (isKnockedBack) return;

        // Langsung jalankan coroutine menggunakan arah murni yang dikirim dari luar
        StartCoroutine(DoKnockback(dir.normalized));
    }

    System.Collections.IEnumerator DoKnockback(Vector2 dir)
    {
        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }
}