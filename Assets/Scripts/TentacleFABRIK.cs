using UnityEngine;

public class TentacleFABRIK : MonoBehaviour
{
    [Header("Setup")]
    public Transform root;
    public int segmentCount = 8;
    public float segmentLength = 0.5f;
    
    [Header("FABRIK")]
    public int iterations = 10;
    public float tolerance = 0.01f;

    [Header("Smoothing")]
    public float smoothSpeed = 8f;

    [Header("State")]
    public float slashSpeed = 20f;
    public float windupDistance = 2f;
    public float cooldownDuration = 1.5f;
    public float attackRange = 4f;
    
    [Header("Side")]
    public TentacleSide side;

    // Internal
    Vector2[] positions;
    Vector2[] smoothed;
    Transform[] segmentTransforms;
    Vector2 target;
    public LayerMask groundLayer;
    Transform player;
    public float curveAmount = 1.5f;
    Vector2 windupTarget;
    public float windupLiftHeight = 3f;

    public enum State { Idle, Windup, Slash, Cooldown }
    public enum TentacleSide { Left, Right }
    State state = State.Idle;
    float stateTimer;
    Vector2 slashTarget;

    // Tambah di TentacleFABRIK.cs

    public void TriggerAttack()
    {
        if (state != State.Idle) return;
        EnterWindup(player.position);
    }

    public bool IsIdle() => state == State.Idle;
    
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        segmentTransforms = new Transform[transform.childCount];
        positions = new Vector2[transform.childCount];
        smoothed = new Vector2[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            segmentTransforms[i] = transform.GetChild(i);
            positions[i] = segmentTransforms[i].position;
            smoothed[i] = positions[i];
        }

        segmentCount = segmentTransforms.Length;
    }

    void Update()
    {
        UpdateState();
        SolveChain();
        ApplySmoothing();
        ApplyCurve();
        ApplyToTransforms();
    }

    void UpdateState()
    {
        Vector2 playerPos = player.position;
        float dist = Vector2.Distance(root.position, playerPos);

        switch (state)
        {
            case State.Idle:
                float t = Time.time + GetInstanceID() * 0.5f;
                target = (Vector2)root.position
                         + new Vector2(Mathf.Sin(t * 1.2f) * 2f, -2f); // lebih ke samping, sedikit ke bawah
                break;

            case State.Windup:
                target = Vector2.MoveTowards(target, windupTarget, slashSpeed * 0.5f * Time.deltaTime);

                if (Vector2.Distance(target, windupTarget) < 0.2f)
                    stateTimer -= Time.deltaTime;

                if (stateTimer <= 0)
                    EnterSlash(playerPos);
                break;

            case State.Slash:
                target = Vector2.MoveTowards(target, slashTarget, slashSpeed * Time.deltaTime);

                if (Vector2.Distance(target, slashTarget) < 0.1f)
                    EnterCooldown();
                break;

            case State.Cooldown:
                stateTimer -= Time.deltaTime;
                target = Vector2.Lerp(target, (Vector2)root.position + Vector2.down * 2f, Time.deltaTime * 2f);

                if (stateTimer <= 0)
                    state = State.Idle;
                break;
        }

        Vector2 rayOrigin = new Vector2(target.x, target.y + 3f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 5f, groundLayer);
        if (hit.collider != null)
            target.y = Mathf.Max(target.y, hit.point.y + 0.2f);
    }

    void EnterWindup(Vector2 playerPos)
    {
        state = State.Windup;
        stateTimer = 0.8f;

        // Sedikit condong ke arah player
        Vector2 toPlayer = ((Vector2)playerPos - (Vector2)root.position).normalized;
        windupTarget = (Vector2)root.position + Vector2.up * windupLiftHeight + toPlayer * 0.5f;
    }

    void EnterSlash(Vector2 playerPos)
    {
        state = State.Slash;
        slashTarget = playerPos;
        target = positions[segmentCount - 1]; // mulai dari posisi tip sekarang
    }

    void EnterCooldown()
    {
        state = State.Cooldown;
        stateTimer = cooldownDuration;
    }

    void SolveChain()
    {
        positions[0] = root.position;

        for (int iter = 0; iter < iterations; iter++)
        {
            // Backward
            positions[segmentCount - 1] = target;
            for (int i = segmentCount - 2; i >= 0; i--)
            {
                Vector2 dir = (positions[i] - positions[i + 1]).normalized;
                positions[i] = positions[i + 1] + dir * segmentLength;
            }

            // Forward
            positions[0] = root.position;
            for (int i = 1; i < segmentCount; i++)
            {
                Vector2 dir = (positions[i] - positions[i - 1]).normalized;
                positions[i] = positions[i - 1] + dir * segmentLength;
            }

            if (Vector2.Distance(positions[segmentCount - 1], target) < tolerance)
                break;
        }
    }

    void ApplySmoothing()
    {
        for (int i = 0; i < segmentCount; i++)
        {
            float speed = state == State.Slash ? smoothSpeed * 3f : smoothSpeed;
            smoothed[i] = Vector2.Lerp(smoothed[i], positions[i], speed * Time.deltaTime);
        }
    }

    void ApplyToTransforms()
    {
        for (int i = 0; i < segmentCount; i++)
        {
            segmentTransforms[i].position = smoothed[i];

            // Rotate tiap segment menghadap segment berikutnya
            if (i < segmentCount - 1)
            {
                Vector2 dir = smoothed[i + 1] - smoothed[i];
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                segmentTransforms[i].rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
    
    void ApplyCurve()
    {
        if (state == State.Slash) return;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = (float)i / (segmentCount - 1);
            float curve;

            if (state == State.Windup)
            {
                // Melengkung ke arah player di bagian tengah, tip ngarah player
                Vector2 toPlayer = ((Vector2)player.position - (Vector2)root.position).normalized;
                float bend = Mathf.Sin(t * Mathf.PI) * curveAmount * 0.5f;
                positions[i] += new Vector2(-toPlayer.x * bend, 0);            }
            else
            {
                float curve2 = Mathf.Sin(t * Mathf.PI) * curveAmount * 0.3f; // dikecilkan
                positions[i] += new Vector2(0, curve2);
            }
        }
    }

    // Dipanggil dari TipCollider.cs
    public bool IsSlashing() => state == State.Slash;
    public State GetState() => state;
}