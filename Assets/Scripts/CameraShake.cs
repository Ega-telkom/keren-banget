using UnityEngine;
using System.Collections;

using UnityEngine;

[System.Serializable]
public class ShakeData
{
    public string label;
    
    [Header("Movement")]
    public Vector3 posDirection = Vector3.up;
    public float magnitude = 0.3f;
    public float duration = 0.2f;

    [Header("Custom Easing")]
    // Sumbu X kurva adalah waktu (0 ke 1), Sumbu Y adalah intensitas (0 ke 1)
    public AnimationCurve intensityCurve = AnimationCurve.Linear(0, 1, 1, 0); 
    
    [Tooltip("Jika true, arah akan sedikit acak agar tidak kaku")]
    public bool useRandomness = true;
}

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    public Vector3 PositionOffset { get; private set; }

    private Coroutine currentShake;

    void Awake() => Instance = this;

    public void Shake(ShakeData data, Transform source = null)
    {
        if (currentShake != null) StopCoroutine(currentShake);
        currentShake = StartCoroutine(DoShake(data, source));
    }

    IEnumerator DoShake(ShakeData data, Transform source)
    {
        float elapsed = 0f;

        // Tentukan arah dasar
        Vector3 baseDir = data.posDirection;
        if (source != null)
        {
            // Balik arah X jika player menghadap kiri (lossyScale.x negatif)
            baseDir = source.TransformDirection(data.posDirection) * Mathf.Sign(source.lossyScale.x);
        }

        while (elapsed < data.duration)
        {
            float t = elapsed / data.duration;
            float intensity = data.intensityCurve.Evaluate(t);

            // Tambahkan sedikit variasi random jika diaktifkan
            Vector3 noise = data.useRandomness ? 
                new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0) : Vector3.zero;

            PositionOffset = (baseDir.normalized + noise) * (intensity * data.magnitude);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Smoothly reset ke nol (bukan snap instan)
        float resetElapsed = 0f;
        float resetDuration = 0.05f;
        Vector3 startPos = PositionOffset;

        while (resetElapsed < resetDuration)
        {
            resetElapsed += Time.deltaTime;
            PositionOffset = Vector3.Lerp(startPos, Vector3.zero, resetElapsed / resetDuration);
            yield return null;
        }

        PositionOffset = Vector3.zero;
        currentShake = null;
    }
}  