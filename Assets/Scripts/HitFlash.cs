using UnityEngine;
using System.Collections;

public class HitFlash : MonoBehaviour
{
    SpriteRenderer sr;
    public float flashDuration = 0.1f;
    public Color flashColor = Color.red;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    public void Flash()
    {
        StartCoroutine(DoFlash());
    }

    IEnumerator DoFlash()
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        sr.color = Color.white;
    }
}