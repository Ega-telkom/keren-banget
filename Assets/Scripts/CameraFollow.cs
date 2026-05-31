using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float smoothTime = 0.15f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 logicalPosition;

    void Start()
    {
        if (target != null)
        {
            // Set posisi awal, tapi PAKSA Z ke -10
            logicalPosition = target.position + offset;
            logicalPosition.z = -10f; 
            
            // Langsung pindahkan kamera ke sana agar tidak ada lompatan visual di awal
            transform.position = logicalPosition;
        }
    }

    void LateUpdate()
    {
        if (target == null || CameraShake.Instance == null) return;

        // 1. Hitung posisi tujuan
        Vector3 targetPos = target.position + offset;
        
        // --- KUNCI SUMBU Z DI SINI ---
        // Tidak peduli Player ada di Z -1, 0, atau 100, kamera tetap mau ke -10
        targetPos.z = -10f; 

        // 2. Haluskan pergerakan
        logicalPosition = Vector3.SmoothDamp(logicalPosition, targetPos, ref velocity, smoothTime);

        // 3. Gabungkan dengan Shake, tapi pastikan hasil akhirnya tetap Z = -10
        Vector3 finalPos = logicalPosition + CameraShake.Instance.PositionOffset;
        finalPos.z = -10f; 

        transform.position = finalPos;
    }
}