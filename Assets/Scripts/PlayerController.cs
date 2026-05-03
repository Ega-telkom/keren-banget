using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float jumpForce = 10f;
    public float jumpForwardSpeed = 3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("Feel Settings")]
    float pixelsPerUnit = 16f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    public float attackBufferTime = 0.1f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;


    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;

    Vector2 moveInput;
    bool isGrounded;
    bool isAttacking;
    float fallVelocity;
    bool wasGrounded;

    float lastDirection = 1f;
    float coyoteTimer;
    float jumpBufferTimer;
    float attackBufferTimer;
    bool isDashing;
    float dashCooldownTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // ================= INPUT =================

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        // Retro Feeling
        moveInput = new Vector2(
            input.x > 0.1f ? 1 : (input.x < -0.1f ? -1 : 0),
            input.y > 0.1f ? 1 : (input.y < -0.1f ? -1 : 0)
        ).normalized;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
            jumpBufferTimer = jumpBufferTime;
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && dashCooldownTimer <= 0f && !isDashing && !isAttacking)
        {
            StartCoroutine(Dash());
        }
    }


    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
            attackBufferTimer = attackBufferTime;
    }

    // ================= UPDATE =================
    void Update()
    {
        // Store downward speed
        if (rb.linearVelocity.y < 0)
            fallVelocity = rb.linearVelocity.y;

        // Detect landing
        if (!wasGrounded && isGrounded)
        {
            HandleLanding();
        }

        wasGrounded = isGrounded;
        
        transform.position += (Vector3)moveInput * walkSpeed * Time.deltaTime;
        CheckGround();
        UpdateTimers();
        HandleFacing();
        HandleJump();
        HandleAttack();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (!isAttacking && !isDashing)
        {
            rb.linearVelocity = new Vector2(
                moveInput.x * walkSpeed,
                rb.linearVelocity.y
            );
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

    void HandleJump()
    {
        if (jumpBufferTimer > 0 && coyoteTimer > 0 && !isAttacking)
        {
            jumpBufferTimer = 0;
            coyoteTimer = 0;

            float horizontalVelocity = lastDirection * jumpForwardSpeed;

            rb.linearVelocity = new Vector2(horizontalVelocity, jumpForce);
        }
    }
    
    void HandleAttack()
    {
        if (attackBufferTimer > 0 && isGrounded && !isAttacking)
        {
            attackBufferTimer = 0;
            StartAttack();
        }
    }
    
    AnimationCurve landingImpact = new AnimationCurve(
        new Keyframe(0f, 1f),     // instant strong impact
        new Keyframe(0.1f, 0.6f), // quick drop
        new Keyframe(0.25f, 0.25f),
        new Keyframe(1f, 0f)      // fade out
    );
    
    void HandleLanding()
    {
        float impact = Mathf.Abs(fallVelocity);

        // Ignore tiny drops
        if (impact < 5f) return;

        // Scale shake based on impact
        float duration = Mathf.Clamp(impact * 0.01f, 0.01f, 0.01f);
        float magnitude = Mathf.Clamp(impact * 0.01f, 0.01f, 0.01f);

        CameraShake.instance?.Shake(duration, magnitude, landingImpact);
    }

    void StartAttack()
    {
        isAttacking = true;
        animator.SetTrigger("Attack");

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        CameraShake.instance?.Shake(0.1f, 0.8f);
        StartCoroutine(Rumble(0.2f, 0.6f));
    }

    public void AttackFinished()
    {
        isAttacking = false;
    }

    IEnumerator Dash()
    {
        isDashing = true;
        CameraShake.instance?.Shake(0.1f, 0.8f);
        dashCooldownTimer = dashCooldown;

        animator.SetTrigger("Dash"); // <-- INI trigger animator kamu

        float direction = lastDirection;

        rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
    }


    // ================= HELPERS =================

    void CheckGround()
    {
        if (Physics2D.OverlapCircle(groundCheck.position, groundDistance, groundMask))
        {
            isGrounded = true;
            coyoteTimer = coyoteTime;
        }
        else
        {
            isGrounded = false;
        }
    }

    void UpdateTimers()
    {
        jumpBufferTimer -= Time.deltaTime;
        attackBufferTimer -= Time.deltaTime;
        coyoteTimer -= Time.deltaTime;
        dashCooldownTimer -= Time.deltaTime;
    }

    void HandleFacing()
    {
        if (moveInput.x > 0)
        {
            spriteRenderer.flipX = false;
            lastDirection = 1f;
        }
        else if (moveInput.x < 0)
        {
            spriteRenderer.flipX = true;
            lastDirection = -1f;
        }
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


    void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("VelocityY", rb.linearVelocity.y);
    }
}
