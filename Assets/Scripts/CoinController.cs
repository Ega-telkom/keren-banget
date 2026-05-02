using UnityEngine;

public class CoinController : MonoBehaviour
{
    Animator animator;
    bool isCollected = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger detected: " + other.name); // <-- tambahin
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;
            animator.SetTrigger("Collect");
            GameManager.instance.AddCoin();
        }
    }

    // Method ini dipanggil dari Animation Event
    public void DestroyCoin()
    {
        Destroy(gameObject);
    }
}