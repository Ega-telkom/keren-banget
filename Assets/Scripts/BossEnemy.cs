using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    Transform player;
    public float moveSpeed = 2f;
    public float stopDistance = 5f; // berhenti kalau sudah dekat

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }
    
    void Update()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > stopDistance)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
        }
    }
}