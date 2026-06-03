using UnityEngine;

public class TipCollider : MonoBehaviour
{
    public int damage = 20;
    TentacleFABRIK tentacle;

    void Start()
    {
        tentacle = GetComponentInParent<TentacleFABRIK>();
        Debug.Log($"Tentacle found: {tentacle}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"{gameObject.name} hit {other.name}, isSlashing: {tentacle.IsSlashing()}, state tentacle: {tentacle.GetState()}");
        if (!tentacle.IsSlashing()) return;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Health>()?.TakeDamage(damage);
        }
    }
}