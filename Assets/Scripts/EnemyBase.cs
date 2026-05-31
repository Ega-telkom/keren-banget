using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 5f;
    public float attackRange = 3f;
    public LayerMask playerLayer;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float patrolSpeed = 1f;

    [Header("Combat")]
    public int attackDamage = 10;
    public float attackCooldown = 2f;
    
    [Header("Stun")]
    public float stunDuration = 1f;
    
    bool isStunned;
    float stunTimer;
    protected Transform player;
    protected Health health;
    protected float attackTimer;

    protected enum EnemyState { Patrol, Chase, Attack }
    protected EnemyState state = EnemyState.Patrol;

    public void Stun()
    {
        isStunned = true;
        stunTimer = stunDuration;
    }
    
    protected virtual void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath += OnDeath;
    }

    protected virtual void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
                isStunned = false;
            return; // skip semua logic saat stun
        }
        
        attackTimer -= Time.deltaTime;
        DetectPlayer();

        switch (state)
        {
            case EnemyState.Patrol: HandlePatrol(); break;
            case EnemyState.Chase:  HandleChase();  break;
            case EnemyState.Attack: HandleAttack(); break;
        }
    }

    void DetectPlayer()
    {
        var hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (hit != null)
        {
            player = hit.transform;
            float dist = Vector2.Distance(transform.position, player.position);
            state = dist <= attackRange ? EnemyState.Attack : EnemyState.Chase;
        }
        else
        {
            player = null;
            state = EnemyState.Patrol;
        }
    }

    protected abstract void HandlePatrol();
    protected abstract void HandleChase();

    protected virtual void HandleAttack()
    {
        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            PerformAttack();
        }
    }

    protected abstract void PerformAttack();

    protected virtual void OnDeath()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}