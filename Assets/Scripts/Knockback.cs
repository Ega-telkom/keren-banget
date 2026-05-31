using UnityEngine;

public class Knockback : MonoBehaviour
{
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    Rigidbody2D rb;
    bool isKnockedBack;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public void Apply(Vector2 sourcePosition)
    {
        if (isKnockedBack) return;
        Vector2 dir = new Vector2(
            Mathf.Sign((transform.position - (Vector3)sourcePosition).x),
            0.5f
        ).normalized;
        StartCoroutine(DoKnockback(dir));
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