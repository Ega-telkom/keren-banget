using UnityEngine;

public class WalkerEnemy : EnemyBase
{
    [Header("Patrol")]
    public Transform patrolPointA;
    public Transform patrolPointB;
    public float waitTime = 1f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Jump")]
    public float jumpForce = 5f;
    public LayerMask wallMask;
    public float wallCheckDistance = 0.3f;

    bool isGrounded;
    public Transform groundCheck;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;
    
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Transform currentPatrolTarget;
    float waitTimer;
    float lastDirection = 1f;
    
    protected override void Update()
    {
        CheckGround();
        CheckAndJump();
        base.Update();
    }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        currentPatrolTarget = patrolPointA;
    }

    protected override void HandlePatrol()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        Vector2 dir = (currentPatrolTarget.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * patrolSpeed, rb.linearVelocity.y);
        Flip(dir.x);

        if (Vector2.Distance(transform.position, currentPatrolTarget.position) < 0.2f)
        {
            currentPatrolTarget = currentPatrolTarget == patrolPointA ? patrolPointB : patrolPointA;
            waitTimer = waitTime;
        }
    }

    protected override void HandleChase()
    {
        if (player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
        Flip(dir.x);
    }

    protected override void PerformAttack()
    {
        if (player == null || projectilePrefab == null) return;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        Vector2 dir = (player.position - firePoint.position).normalized;
        var proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        proj.GetComponent<Projectile>()?.Launch(dir, attackDamage);
    }
    
    void Flip(float dirX)
    {
        if (dirX > 0) { spriteRenderer.flipX = false; lastDirection = 1f; }
        else if (dirX < 0) { spriteRenderer.flipX = true; lastDirection = -1f; }
    }
    
    void CheckAndJump()
    {
        // Cek wall di depan
        Vector2 dir = new Vector2(lastDirection, 0);
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, dir, wallCheckDistance, wallMask);

        if (wallHit.collider != null && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckDistance, groundMask);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, new Vector2(lastDirection, 0) * wallCheckDistance);
    }
}