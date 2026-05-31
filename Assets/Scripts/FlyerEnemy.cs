using UnityEngine;

public class FlyerEnemy : EnemyBase
{
    [Header("Patrol")]
    public Transform patrolPointA;
    public Transform patrolPointB;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Hover")]
    public float hoverAmplitude = 0.3f;
    public float hoverFrequency = 2f;

    SpriteRenderer spriteRenderer;
    Transform currentPatrolTarget;
    float hoverOffset;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        currentPatrolTarget = patrolPointA;
        hoverOffset = Random.Range(0f, Mathf.PI * 2f); // biar tiap flyer tidak sync
    }

    protected override void HandlePatrol()
    {
        Vector2 dir = (currentPatrolTarget.position - transform.position).normalized;
        Vector2 hover = new Vector2(0, Mathf.Sin(Time.time * hoverFrequency + hoverOffset) * hoverAmplitude);
        transform.position += (Vector3)(dir * patrolSpeed + hover) * Time.deltaTime;
        Flip(dir.x);

        if (Vector2.Distance(transform.position, currentPatrolTarget.position) < 0.2f)
            currentPatrolTarget = currentPatrolTarget == patrolPointA ? patrolPointB : patrolPointA;
    }

    protected override void HandleChase()
    {
        if (player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        Vector2 hover = new Vector2(0, Mathf.Sin(Time.time * hoverFrequency + hoverOffset) * hoverAmplitude);
        transform.position += (Vector3)(dir * moveSpeed + hover) * Time.deltaTime;
        Flip(dir.x);
    }

    protected override void PerformAttack()
    {
        if (player == null || projectilePrefab == null) return;

        Vector2 dir = (player.position - firePoint.position).normalized;
        var proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        proj.GetComponent<Projectile>()?.Launch(dir, attackDamage);
    }

    void Flip(float dirX)
    {
        if (dirX > 0) spriteRenderer.flipX = false;
        else if (dirX < 0) spriteRenderer.flipX = true;
    }
}