using UnityEngine;
using System.Collections;

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

    [Header("Death Juice (Satisfying)")]
    [Tooltip("Masukkan Prefab Partikel EnemyBlood di sini")]
    public GameObject enemyBloodPrefab; 
    [Tooltip("Masukkan Prefab Partikel EnemyDied di sini")]
    public GameObject enemyDiedPrefab; 
    [Tooltip("Berapa lama musuh melayang terlempar sebelum akhirnya meledak hancur")]
    public float deathFlyDuration = 0.25f;
    [Tooltip("Masukkan Data Shake untuk guncangan kamera saat musuh mati meledak")]
    public ShakeData enemyDeathCameraShake; 
    
    bool isStunned;
    bool isDead; 
    float stunTimer;
    protected Transform player;
    protected Health health;
    protected float attackTimer;

    protected enum EnemyState { Patrol, Chase, Attack }
    protected EnemyState state = EnemyState.Patrol;

    public void Stun()
    {
        if (isDead) return; 
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
        if (isDead) return; 

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
                isStunned = false;
            return; 
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
        if (isDead) return;
        // Matikan fungsi update AI agar musuh diam tidak mengejar player saat proses mati
        isDead = true; 
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        gameObject.layer = 2; 

        var allChildren = GetComponentsInChildren<Transform>();
        foreach (var child in allChildren) child.gameObject.layer = 2;
        
        yield return new WaitForSeconds(deathFlyDuration);
        
        if (enemyDiedPrefab != null)
        {
            GameObject particle = Instantiate(enemyDiedPrefab, transform.position, Quaternion.identity);
            
            // Jika prefab menggunakan ParticleSystem, nyalakan secara instan
            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();

            Destroy(particle, 2f); 
        }

        // [FASE 3]: Guncangan kamera bergetar tepat di frame ledakan terjadi
        if (enemyDeathCameraShake != null && CameraShake.Instance != null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                float distance = Vector2.Distance(transform.position, playerObj.transform.position);
                float maxShakeDistance = 20f; 

                if (distance <= maxShakeDistance)
                {
                    CameraShake.Instance.Shake(enemyDeathCameraShake, transform);
                }
            }
            else
            {
                CameraShake.Instance.Shake(enemyDeathCameraShake, transform);
            }
        }

        // [FASE 4]: Hilangkan sisa visual sprite musuh dan hapus dari scene
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