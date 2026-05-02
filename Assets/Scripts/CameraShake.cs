using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    [Header("Shake Settings")]
    public AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);

    void Awake()
    {
        instance = this;
    }

    public void Shake(float duration, float magnitude, AnimationCurve curve = null)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude, curve ?? shakeCurve));
    }

    System.Collections.IEnumerator ShakeCoroutine(float duration, float magnitude, AnimationCurve curve)
    {
        Vector3 originalPos = transform.localPosition;
        Vector3 originalRot = transform.localEulerAngles;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;

            float curveValue = curve.Evaluate(normalizedTime);
            float currentMagnitude = magnitude * curveValue;

            float x = Random.Range(-1f, 1f) * currentMagnitude;
            float y = Random.Range(-1f, 1f) * currentMagnitude;
            float rot = Random.Range(-1f, 1f) * currentMagnitude;

            transform.localPosition = new Vector3(
                originalPos.x + x,
                originalPos.y + y,
                originalPos.z
            );

            transform.localEulerAngles = new Vector3(
                originalRot.x,
                originalRot.y,
                originalRot.z + rot // (also fixed your rotation bug here 👀)
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        transform.localEulerAngles = originalRot;
    }
}
