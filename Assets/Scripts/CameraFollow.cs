using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float smoothTime = 0.15f;

    // --- FITUR ANTI JITTER & STAY ---
    [Header("Anti Jitter Settings")]
    [Tooltip("Batas toleransi pixel terkecil. Jika jarak kamera & player di bawah angka ini, kamera langsung stay mengunci posisi.")]
    public float snapThreshold = 0.08f; 

    // --- FITUR DROP CAMERA (BARU) ---
    [Header("Dropper / Look Down Settings")]
    [Tooltip("Kecepatan jatuh player (Y) untuk mengaktifkan intip bawah. Harus minus, misal -5 atau -7.")]
    public float fallSpeedThreshold = -6f;
    [Tooltip("Berapa jauh kamera mengintip ke bawah player saat jatuh bebas. Harus minus, misal -3.")]
    public float lookDownOffset = -3f;
    [Tooltip("Seberapa cepat kamera beralih posisi dari normal ke intip bawah (dan sebaliknya).")]
    public float shiftSpeed = 4f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 logicalPosition;
    
    // Variabel internal untuk mendeteksi fisika player
    private Rigidbody2D targetRb;
    private float currentOffsetY;

    void Start()
    {
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody2D>();
            currentOffsetY = offset.y;

            logicalPosition = target.position + offset;
            logicalPosition.z = -10f; 
            transform.position = logicalPosition;
        }
    }

    void FixedUpdate() 
    {
        if (target == null) return;

        // Ambil Rigidbody2D player secara otomatis jika belum ter-cache
        if (targetRb == null)
        {
            targetRb = target.GetComponent<Rigidbody2D>();
        }

        // ================================================================
        // LOGIKA DYNAMIC LOOK-DOWN (DROPPER)
        // ================================================================
        float targetOffsetY = offset.y;

        // Cek apakah player punya Rigidbody2D dan sedang jatuh melebihi batas threshold
        if (targetRb != null && targetRb.linearVelocity.y < fallSpeedThreshold)
        {
            // Player sedang jatuh cepat! Geser target kamera ke bawah player
            targetOffsetY = offset.y + lookDownOffset;
        }

        // Haluskan pergeseran offset Y nya saja menggunakan Lerp
        currentOffsetY = Mathf.Lerp(currentOffsetY, targetOffsetY, Time.fixedDeltaTime * shiftSpeed);
        // ================================================================

        // 1. Hitung posisi tujuan ideal (menggunakan currentOffsetY yang dinamis)
        Vector3 dynamicOffset = new Vector3(offset.x, currentOffsetY, offset.z);
        Vector3 targetPos = target.position + dynamicOffset;
        targetPos.z = -10f; 

        // 2. Haluskan seluruh pergerakan menggunakan SmoothDamp
        logicalPosition = Vector3.SmoothDamp(logicalPosition, targetPos, ref velocity, smoothTime);

        // --- LOGIKA STAY / SNAP ANTI JITTER ---
        if (Vector3.Distance(logicalPosition, targetPos) < snapThreshold)
        {
            logicalPosition = targetPos;
            velocity = Vector3.zero; 
        }

        // 3. Gabungkan dengan Shake
        Vector3 shakeOffset = Vector3.zero;
        if (CameraShake.Instance != null)
        {
            shakeOffset = CameraShake.Instance.PositionOffset;
        }

        Vector3 finalPos = logicalPosition + shakeOffset;
        finalPos.z = -10f; 

        transform.position = finalPos;
    }
}