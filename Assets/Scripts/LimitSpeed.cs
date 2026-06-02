using UnityEngine;

public class LimitSpeed : MonoBehaviour
{
    public Rigidbody2D rb;
    public float maxSpeed;

    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}