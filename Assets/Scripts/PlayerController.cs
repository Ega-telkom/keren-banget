using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // ================= SETTINGS =================

    [Header("Movement (Hollow Knight Style)")]
    public float moveSpeed = 12f; // Kecepatan konstan, responsif, instan
    public float jumpForce = 12f; // Kekuatan lompatan

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
    public float dashCost = 20f; 
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
    public ShakeData damageShake;
    public ShakeData deathShake;
    
    [Header("Particle")]
    public ParticleSystem bloodParticlePrefab;
    
    [Header("UI Reference")]
    public DeathScreen deathScreenComponent;
    
    // ================= STATE =================

    Rigidbody2D rb;
    Animator animator;
    Health health;
    public bool IsDead { get; private set; } = false;
    private Stamina stamina;
    int comboIndex = 0; // Menyimpan status pukulan ke berapa (0, 1, 2)
    float comboResetTimer; // Timer untuk reset combo jika player telat pencet
    public float comboResetWindow = 0.8f; // Batas waktu toleransi antar pukulan

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

// Cari fungsi Awake() kamu, ubah menjadi seperti ini:
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        health = GetComponent<Health>();
        
        // DAFTARKAN EVENT DAMAGE DAN DEATH DI SINI
        health.OnDamageTaken += OnPlayerDamaged; // Pastikan script Health kamu punya event ini
        health.OnDeath += OnPlayerDeath;
        stamina = GetComponent<Stamina>();
    }

    void OnEnable() 
    { 
        // Jika inputReader belum diisi via Inspector, coba cari secara global dari GameManager
        if (inputReader == null && GameManager.instance != null)
        {
            inputReader = GameManager.instance.inputReader;
        }

        // Hanya lakukan subscribe jika inputReader sudah benar-benar tervalidasi
        if (inputReader != null) 
        {
            Subscribe(); 
        }
    }
    void OnDisable() { if (inputReader != null) Unsubscribe(); }

    public void Initialize(InputReader reader)
    {
        // Jika sebelumnya sudah punya inputReader, putuskan hubungan dulu agar tidak double-binding
        if (inputReader != null)
        {
            Unsubscribe();
        }

        inputReader = reader;
        
        // Daftarkan ulang ke InputReader yang segar dari LevelManager
        if (inputReader != null)
        {
            Subscribe();
            Debug.Log("<color=magenta>[PLAYER] Inisialisasi Berhasil! Sukses menjabat tangan InputReader dari LevelManager.</color>");
        }
    }

    void Subscribe()
    {
        inputReader.OnMove += HandleMove;
        inputReader.OnJumpPerformed += HandleJump_Input;
        inputReader.OnDashPerformed += HandleDash_Input;
        inputReader.OnAttackPerformed += HandleAttack_Input;
        inputReader.OnShootPerformed += HandleShoot_Input;
    }

    void Unsubscribe()
    {
        inputReader.OnMove -= HandleMove;
        inputReader.OnJumpPerformed -= HandleJump_Input;
        inputReader.OnDashPerformed -= HandleDash_Input;
        inputReader.OnAttackPerformed -= HandleAttack_Input;
        inputReader.OnShootPerformed -= HandleShoot_Input;
    }

    // ================= INPUT HANDLERS =================

    void HandleMove(Vector2 input)
    {
        Debug.Log($"[PLAYER CONTROL] Menerima input gerak dari keyboard/gamepad: {input}");
        // Sistem Snap/Digital: Begitu stik digeser melewati 0.1f, 
        // langsung DIKUNCI ke angka 1 atau -1 murni. 
        // Ini mengabaikan semua angka desimal halus dari analog.
        float targetX = input.x > 0.1f ? 1f : (input.x < -0.1f ? -1f : 0f);
        float targetY = input.y > 0.1f ? 1f : (input.y < -0.1f ? -1f : 0f);

        // Simpan sebagai nilai murni tanpa di-normalized secara vektor,
        // agar input serong (diagonal) tidak memotong kecepatan horizontal (X) kamu!
        moveInput = new Vector2(targetX, targetY);
    }

    void HandleJump_Input() => jumpBufferTimer = jumpBufferTime;

    void HandleDash_Input()
    {
        if (dashCooldownTimer <= 0f && !IsBusy() && stamina.HasEnough(dashCost))
        {
            stamina.Drain(dashCost); 
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

        UpdateStaminaStatus(); 
        CheckGround();
        UpdateTimers();
        HandleFacing();
        HandleJump();
        HandleAttack();
        HandleShoot();
        UpdateAnimator();
        // TAMBAHKAN INI: Reset combo jika player diam terlalu lama setelah memukul
        if (comboIndex > 0)
        {
            comboResetTimer -= Time.deltaTime;
            if (comboResetTimer <= 0f && !isAttacking)
            {
                comboIndex = 0; // Kembali ke pukulan pertama
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsBusy())
        {
            // GERAKAN HOLLOW KNIGHT: 100% Instan, Snappy, Tanpa Kuncian Kode
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
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

    void HandleJump()
    {
        if (jumpBufferTimer > 0 && coyoteTimer > 0 && !isAttacking)
        {
            animator.SetTrigger("Jump");

            jumpBufferTimer = 0;
            coyoteTimer = 0;
        
            // Dorongan vertikal instan, arah udara bisa langsung dikontrol di frame berikutnya
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, jumpForce);
        }
    }

    void HandleAttack()
    {
        // Masih mempertahankan fungsi buffer dan cek stamina bawaan kodemu
        if (attackBufferTimer > 0 && isGrounded && !isAttacking && attackCooldownTimer <= 0f)
        {
            if (stamina.HasEnough(attack1Cost))
            {
                attackBufferTimer = 0;
                attackCooldownTimer = attackCooldown;
                
                // SISTEM COMBO: Naikkan indeks setiap pencet, maksimal 3 pukulan
                comboIndex++;
                if (comboIndex > 3) comboIndex = 1;

                StartAttack(); 
            }
        }
    }

    void StartAttack()
    {
        stamina.Drain(attack1Cost);
        isAttacking = true;

        // Kirim data combo ke Animator
        animator.SetInteger("ComboCount", comboIndex);
        animator.SetTrigger("Attack"); // Trigger "Attack" yang ada di kodemu

        // Berikan waktu toleransi baru untuk pukulan berikutnya
        comboResetTimer = comboResetWindow;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // Pilih guncangan kamera berdasarkan pukulan ke berapa
        ShakeData currentShake = comboIndex == 1 ? attack1Shake : (comboIndex == 2 ? attack2Shake : attack3Shake);
        CameraShake.Instance.Shake(currentShake, visual);
        
        StartCoroutine(Rumble(0.2f, 0.6f));
    }

    void HandleShoot()
    {
        if (shootBufferTimer > 0 && isGrounded && !isShooting)
        {
            shootBufferTimer = 0;
            StartCoroutine(Shoot());
        }
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
    
    public int GetComboIndex() => comboIndex;

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundDistance, groundMask);
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
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
        if (isAttacking || isShooting) return; 
        if (moveInput.x > 0) { visual.localScale = new Vector3(1, 1, 1); lastDirection = 1f; }
        else if (moveInput.x < -0) { visual.localScale = new Vector3(-1, 1, 1); lastDirection = -1f; }
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
    
    void OnPlayerDamaged(float damageAmount)
    {
        Debug.Log("PlayerController: OnPlayerDamaged dipanggil. Damage: " + damageAmount);

        if (CameraShake.Instance != null && damageShake != null)
        {
            CameraShake.Instance.Shake(damageShake, visual);
        }
        
        if (animator != null) animator.SetTrigger("Hurt");
    }
    
// Tambahkan kata PUBLIC di depannya
    public void OnPlayerDeath()
    {
        // Pengaman: Jika player sudah mati, jangan eksekusi kode di bawahnya lagi
        if (IsDead) return; 

        Debug.Log("PlayerController: Player Mati.");
    
        IsDead = true; // <-- SET JADI TRUE SAAT MATI

        if (CameraShake.Instance != null && deathShake != null)
        {
            CameraShake.Instance.Shake(deathShake, visual);
        }

        if (inputReader != null) Unsubscribe();

        if (deathScreenComponent != null)
        {
            deathScreenComponent.ShowDeathScreen();
        }
        else
        {
            Debug.LogError("PlayerController ERROR: Slot deathScreenComponent di Inspector masih kosong!");
        }

        if (visual != null) visual.gameObject.SetActive(false);
    
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        if (rb != null) rb.simulated = false; 
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        float actualSpeed = Mathf.Abs(rb.linearVelocity.x);
        float visualSpeed = actualSpeed;

        // JIKA PLAYER MENEKAN TOMBOL ARAH
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(groundMask);
            filter.useLayerMask = true;

            RaycastHit2D[] hits = new RaycastHit2D[1];
            // Jika mendeteksi tembok tepat di arah input jalan player
            if (rb.Cast(new Vector2(moveInput.x, 0f), filter, hits, 0.05f) > 0)
            {
                visualSpeed = 0f; // Paksa nilai animasi jadi 0 (Idle)
            }
        }

        // LOGIKA ANIMASI AKHIR
        if (Mathf.Abs(moveInput.x) < 0.1f || visualSpeed < 0.1f)
        {
            animator.SetFloat("Speed", 0f); // Putar Idle
        }
        else
        {
            animator.SetFloat("Speed", moveSpeed); // Putar Run
        }

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
        new Keyframe(0f, 1f, 0f, -2f), 
        new Keyframe(1f, 0f, 0f, 0f)   
    );
}