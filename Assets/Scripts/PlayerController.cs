using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // ================= SETTINGS =================

    [Header("Movement")] 
    public float jumpForce = 8f;
    public float walkSpeed = 4f;
    public float sprintSpeed = 12f; // Kecepatan saat lari
    public float sprintCostPerSecond = 16f; // Biaya stamina per detik

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("Feel")]
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    public float attackBufferTime = 0.1f;
    public float shootBufferTime = 0.1f;
    [SerializeField] float pixelsPerUnit = 16f;

    [Header("Dash")]
    public float dashCost = 20f; // Tambahkan cost dash
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;
    
    [Header("Attack")]
    public float attackCooldown = 0.5f;
    private float attack1Cost = 15f;
    float attackCooldownTimer;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float bulletSpeed = 10f;

    [Header("References")]
    public Transform visual;
    public AttackHitbox attackHitbox;
    public InputReader inputReader;
    
    [Header("Camera Shake")]
    public ShakeData attack1Shake;
    public ShakeData attack2Shake;
    public ShakeData attack3Shake;
    public ShakeData dashshake;
    public ShakeData shootshake;
    public ShakeData equipshake;
    
    // ================= STATE =================

    Rigidbody2D rb;
    Animator animator;
    Health health;
    private Stamina stamina;
    private InputAction sprintAction;
    private bool isSprinting;
    bool sprintInputPressed;
    private float lerpedSpeed;
    private float airMomentum;
    private float currentAirSpeed;

    Vector2 moveInput;
    float lastDirection = 1f;

    bool isGrounded;
    bool wasGrounded;
    bool isAttacking;
    bool isDashing;
    bool isShooting;

    float fallVelocity;
    float coyoteTimer;
    float jumpBufferTimer;
    float attackBufferTimer;
    float shootBufferTimer;
    float dashCooldownTimer;

    bool IsBusy() => isAttacking || isDashing || isShooting;

    // ================= LIFECYCLE =================

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        health = GetComponent<Health>();
        health.OnDeath += OnPlayerDeath;
        stamina = GetComponent<Stamina>();
    }

    void OnEnable() { if (inputReader != null) Subscribe(); }
    void OnDisable() { if (inputReader != null) Unsubscribe(); }

    public void Initialize(InputReader reader)
    {
        inputReader = reader;
        Subscribe();
    }

    void Subscribe()
    {
        inputReader.OnMove += HandleMove;
        inputReader.OnSprintPerformed += HandleSprint_Input;
        inputReader.OnSprintCanceled += HandleSprint_Canceled;
        inputReader.OnJumpPerformed += HandleJump_Input;
        inputReader.OnDashPerformed += HandleDash_Input;
        inputReader.OnAttackPerformed += HandleAttack_Input;
        inputReader.OnShootPerformed += HandleShoot_Input;
    }

    void Unsubscribe()
    {
        inputReader.OnMove -= HandleMove;
        inputReader.OnSprintPerformed -= HandleSprint_Input;
        inputReader.OnSprintCanceled -= HandleSprint_Canceled;
        inputReader.OnJumpPerformed -= HandleJump_Input;
        inputReader.OnDashPerformed -= HandleDash_Input;
        inputReader.OnAttackPerformed -= HandleAttack_Input;
        inputReader.OnShootPerformed -= HandleShoot_Input;
    }

    // ================= INPUT HANDLERS =================

    void HandleMove(Vector2 input)
    {
        moveInput = new Vector2(
            input.x > 0.1f ? 1 : (input.x < -0.1f ? -1 : 0),
            input.y > 0.1f ? 1 : (input.y < -0.1f ? -1 : 0)
        ).normalized;
    }
    
    void HandleSprint_Input() 
    {
        sprintInputPressed = true;
    }

    void HandleSprint_Canceled() 
    {
        sprintInputPressed = false;
    }

    void HandleJump_Input() => jumpBufferTimer = jumpBufferTime;

    void HandleDash_Input()
    {
        // Tambahkan syarat stamina.HasEnough
        if (dashCooldownTimer <= 0f && !IsBusy() && stamina.HasEnough(dashCost))
        {
            stamina.Drain(dashCost); // Drain stamina saat dash
            StartCoroutine(Dash());
        }
    }

    void HandleAttack_Input()
    {
        if (!IsBusy()) attackBufferTimer = attackBufferTime;
    }

    void HandleShoot_Input()
    {
        if (!IsBusy()) shootBufferTimer = shootBufferTime;
    }

    // ================= UPDATE =================
    void Update()
    {
        TrackFallVelocity();
        DetectLanding();
        wasGrounded = isGrounded;

        HandleSprint();
        UpdateStaminaStatus(); // Tambahkan ini
        CheckGround();
        UpdateTimers();
        HandleFacing();
        HandleJump();
        HandleAttack();
        HandleShoot();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (!IsBusy())
        {
            if (isGrounded)
            {
                // DI TANAH: Tentukan kecepatan berdasarkan status sprint
                currentAirSpeed = isSprinting ? sprintSpeed : walkSpeed;
            }
            else
            {
                // DI UDARA: Fades out kecepatan sprint menuju walkSpeed
                // 2f adalah kekuatan memudarnya, makin besar makin cepat balik ke walkSpeed
                if (currentAirSpeed > walkSpeed)
                {
                    currentAirSpeed -= 2f * Time.fixedDeltaTime; 
                }
                // Pastikan tidak lebih rendah dari walkSpeed
                currentAirSpeed = Mathf.Max(currentAirSpeed, walkSpeed);
            }

            // EKSEKUSI: Tetap Snap Tap arah (moveInput.x) tapi gunakan kecepatan yang memudar tadi
            rb.linearVelocity = new Vector2(moveInput.x * currentAirSpeed, rb.linearVelocity.y);
        }
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x * pixelsPerUnit) / pixelsPerUnit;
        pos.y = Mathf.Round(pos.y * pixelsPerUnit) / pixelsPerUnit;
        transform.position = pos;
    }

    // ================= CORE LOGIC =================
    
    void UpdateStaminaStatus()
    {
        if (stamina == null) return;

        // Player dianggap bergerak jika ada input horizontal 
        // dan tidak sedang dalam keadaan 'Busy' (Dashing/Attacking)
        stamina.IsMoving = Mathf.Abs(moveInput.x) > 0.01f;
    }

    void TrackFallVelocity()
    {
        if (rb.linearVelocity.y < 0)
            fallVelocity = rb.linearVelocity.y;
    }

    void DetectLanding()
    {
        if (!wasGrounded && isGrounded)
            HandleLanding();
    }
    
    void HandleSprint()
    {
        // Syarat Sprint: Tombol ditekan, sedang bergerak x, di tanah, dan tidak sibuk
        if (sprintInputPressed && Mathf.Abs(moveInput.x) > 0.01f && isGrounded && !IsBusy())
        {
            float costThisFrame = sprintCostPerSecond * Time.deltaTime;

            if (stamina.HasEnough(costThisFrame))
            {
                stamina.Drain(costThisFrame);
                isSprinting = true;
                return;
            }
        }
    
        isSprinting = false;
    }

    void HandleJump()
    {
        if (jumpBufferTimer > 0 && coyoteTimer > 0 && !isAttacking)
        {
            animator.SetTrigger("Jump");
            // Ambil kecepatan horizontal saat ini (bisa walkSpeed atau sprintSpeed)
            float currentHorizontalSpeed = isSprinting ? sprintSpeed : walkSpeed;
            airMomentum = moveInput.x * currentHorizontalSpeed;

            jumpBufferTimer = 0;
            coyoteTimer = 0;
        
            // Berikan dorongan vertikal + tetap pertahankan kecepatan horizontal saat itu
            rb.linearVelocity = new Vector2(airMomentum, jumpForce);
        }
    }

    void HandleAttack()
    {
        if (attackBufferTimer > 0 && isGrounded && !isAttacking && attackCooldownTimer <= 0f)
        {
            // Kita cek stamina di sini sebelum masuk ke TryAttack
            if (stamina.HasEnough(attack1Cost))
            {
                attackBufferTimer = 0;
                attackCooldownTimer = attackCooldown;
                StartAttack(); // Panggil langsung StartAttack
            }
            else 
            {
                // Jika stamina tidak cukup, kita bisa reset buffer agar tidak "ngelag"
                // atau biarkan saja agar player bisa attack tepat saat stamina cukup
                Debug.Log("Stamina tidak cukup untuk menyerang!");
            }
        }
    }

    void HandleShoot()
    {
        if (shootBufferTimer > 0 && isGrounded && !isShooting)
        {
            shootBufferTimer = 0;
            StartCoroutine(Shoot());
        }
    }

    void StartAttack()
    {
        stamina.Drain(attack1Cost);
        isAttacking = true;
        animator.SetTrigger("Attack");
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        CameraShake.Instance.Shake(attack1Shake, visual);
        StartCoroutine(Rumble(0.2f, 0.6f));
    }

    IEnumerator Shoot()
    {
        isShooting = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator.SetTrigger("Shoot");
        yield break;
    }

    IEnumerator Dash()
    {
        isDashing = true;
        isAttacking = false;
        attackHitbox.gameObject.SetActive(false);
        dashCooldownTimer = dashCooldown;

        CameraShake.Instance.Shake(dashshake, visual);
        animator.SetTrigger("Dash");
        rb.linearVelocity = new Vector2(lastDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    // ================= ANIMATION EVENTS =================

    public void EnableHitbox()
    {
        Vector3 pos = attackHitbox.transform.localPosition;
        pos.x = Mathf.Abs(pos.x) * lastDirection;
        attackHitbox.transform.localPosition = pos;
        attackHitbox.gameObject.SetActive(true);
    }

    public void AttackFinished()
    {
        isAttacking = false;
        attackHitbox.gameObject.SetActive(false);
    }

    public void SpawnBullet()
    {
        CameraShake.Instance.Shake(shootshake, visual);
        var bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(lastDirection * bulletSpeed, 0f);
    }

    public void ShootFinished() => isShooting = false;

    // ================= HELPERS =================

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundDistance, groundMask);
        if (isGrounded)
        {
            // Selama menyentuh tanah, timer selalu penuh
            coyoteTimer = coyoteTime;
        }
        else
        {
            // Saat mulai jatuh/meninggalkan platform, timer mulai berkurang
            coyoteTimer -= Time.deltaTime;
        }
    }

    void UpdateTimers()
    {
        jumpBufferTimer -= Time.deltaTime;
        attackBufferTimer -= Time.deltaTime;
        shootBufferTimer -= Time.deltaTime;
        coyoteTimer -= Time.deltaTime;
        dashCooldownTimer -= Time.deltaTime;
        attackCooldownTimer -= Time.deltaTime;
    }

    void HandleFacing()
    {
        if (isAttacking || isShooting) return; // lock arah saat aksi
        if (moveInput.x > 0) { visual.localScale = new Vector3(1, 1, 1); lastDirection = 1f; }
        else if (moveInput.x < 0) { visual.localScale = new Vector3(-1, 1, 1); lastDirection = -1f; }
    }

    void HandleLanding()
    {
        float impact = Mathf.Abs(fallVelocity);
        if (impact < 5f) return;
    }

    IEnumerator Rumble(float duration, float intensity)
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(intensity, intensity);
            yield return new WaitForSeconds(duration);
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
    }

    void OnPlayerDeath() => gameObject.SetActive(false);

    void UpdateAnimator()
    {
        if (animator == null) return;
    
        // Nilai ini akan menentukan apakah Blend Tree memutar Idle, Walk, atau Run
        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", horizontalSpeed);
    
        // Parameter lainnya tetap perlu dikirim
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("VelocityY", rb.linearVelocity.y);
    }

    // ================= CURVES =================

    public AnimationCurve landingImpact = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.1f, 0.6f),
        new Keyframe(0.25f, 0.25f),
        new Keyframe(1f, 0f)
    );
    
    public AnimationCurve shakeCurve = new AnimationCurve(
        new Keyframe(0f, 1f, 0f, -2f), // Start di 1, kemiringan awal turun
        new Keyframe(1f, 0f, 0f, 0f)   // End di 0
    );
}