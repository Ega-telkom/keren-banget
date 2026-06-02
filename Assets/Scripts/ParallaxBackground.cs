using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform cam;
    
    [Header("Parallax Axis Settings")]
    public bool parallaxX = true; // Centang jika ingin bergerak di sumbu X
    public bool parallaxY = false; // Hilangkan centang jika ingin sumbu Y

    [Header("Parallax Strength")]
    [Range(0f, 1f)] public float parallaxFactorX = 0.5f; // 0 = diam, 1 = ikut kamera
    [Range(0f, 1f)] public float parallaxFactorY = 0.5f;

    private Vector3 lastCamPos;

    void Start()
    {
        if (cam != null)
        {
            lastCamPos = cam.position;
        }
    }

    // Menggunakan FixedUpdate agar pergerakannya selaras dengan CameraFollow yang baru 
    // dan bebas dari jitter patah-patah saat player lari.
    void FixedUpdate()
    {
        if (cam == null) return;

        // 1. Hitung seberapa jauh kamera bergeser sejak frame lalu
        Vector3 delta = cam.position - lastCamPos;

        // 2. Hitung pergerakan parallax berdasarkan pilihan sumbu x dan y
        float moveX = parallaxX ? delta.x * parallaxFactorX : 0f;
        float moveY = parallaxY ? delta.y * parallaxFactorY : 0f;

        // 3. Terapkan posisi baru ke background
        transform.position += new Vector3(moveX, moveY, 0);

        // 4. Catat posisi kamera sekarang untuk kalkulasi frame berikutnya
        lastCamPos = cam.position;
    }
}