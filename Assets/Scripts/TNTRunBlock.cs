using System.Collections;
using UnityEngine;

public class TNTRunBlock : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float delayBeforeFall = 0.5f; // Waktu tunggu sebelum jatuh
    [SerializeField] private Color warningColor = Color.red; // Warna saat diinjak

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isActivated = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Cek apakah yang menginjak adalah Player dan belum diaktivasi
        if (collision.gameObject.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
            StartCoroutine(FallRoutine());
        }
    }

    private IEnumerator FallRoutine()
    {
        // 1. Beri efek visual peringatan (berubah warna/kedip)
        spriteRenderer.color = warningColor;

        // Kamu juga bisa menambahkan efek guncang (shake) kecil di sini jika mau
        yield return new WaitForSeconds(delayBeforeFall);

        // 2. Aktifkan physics agar balok jatuh bebas
        rb.bodyType = RigidbodyType2D.Dynamic;
        
        // Matikan collider agar balok tidak menabrak player lagi saat jatuh
        GetComponent<BoxCollider2D>().isTrigger = true; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 3. Jika menabrak Zona Penghancur Merah di bawah
        if (other.CompareTag("DestroyerZone"))
        {
            // Hancurkan objek balok ini dari memori
            Destroy(gameObject);
        }
    }
}