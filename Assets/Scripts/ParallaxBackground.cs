using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform cam;
    public float parallaxFactor = 0.5f; // 0 = diam, 1 = ikut kamera
    float pixelsPerUnit = 16f;

    Vector3 lastCamPos;

    void Start() => lastCamPos = cam.position;

    void LateUpdate()
    {
        Vector3 delta = cam.position - lastCamPos;
        transform.position += new Vector3(delta.x * parallaxFactor, delta.y * parallaxFactor, 0);
        lastCamPos = cam.position;
    }
}