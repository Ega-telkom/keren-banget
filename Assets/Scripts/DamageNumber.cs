using UnityEngine;
using TMPro;
using System.Collections;

public class DamageNumber : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float floatSpeed = 1f;
    public float lifetime = 0.8f;
    public float fallSpeed = 1f;

    public void Init(int damage, Color color)
    {
        text.text = $"-{damage}";
        text.color = color;
        StartCoroutine(Animate());
    }
    
    IEnumerator Animate()
    {
        float elapsed = 0f;
        Color c = text.color;
        Vector3 startPos = transform.position;

        while (elapsed < lifetime)
        {
            float t = elapsed / lifetime;

            // Naik dulu lalu jatuh (parabola)
            float y = Mathf.Lerp(0f, 1f, 1f - (t * 2f - 1f) * (t * 2f - 1f));
            transform.position = startPos + Vector3.up * y;

            // Fade out di paruh kedua
            c.a = t < 0.5f ? 1f : 1f - ((t - 0.5f) / 0.5f);
            text.color = c;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}