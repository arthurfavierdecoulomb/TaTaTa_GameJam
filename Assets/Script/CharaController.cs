using UnityEngine;

public enum JumpMode { Normal, High }

public class CharaController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float MoveSpeed = 8f;
    [SerializeField] float Acceleration = 15f;
    [SerializeField] float Deceleration = 20f;

    [Header("Jump")]
    [SerializeField] float JumpForce = 18f;
    [SerializeField] float HighJumpForce = 28f;
    [SerializeField] float FallMultiplier = 3f;
    [SerializeField] float LowJumpMultiplier = 5f;
    [SerializeField] float CoyoteTime = 0.15f;
    [SerializeField] float JumpBufferTime = 0.1f;
    [SerializeField] int MaxAirJumps = 1; 

    [Header("Ground Check")]
    [SerializeField] float GroundCheckDistance = 1.1f;
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb;
    float inputX;
    float coyoteTimeCounter;
    float jumpBufferCounter;
    int airJumpsLeft;       //Compteur de sauts aériens restants
    bool isGrounded;
    bool isDead;
    JumpMode jumpMode = JumpMode.Normal;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDead) return;

        inputX = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.Raycast(
            transform.position, Vector2.down, GroundCheckDistance, groundLayer
        );

        if (isGrounded)
        {
            airJumpsLeft = MaxAirJumps;      // Reset au sol
            coyoteTimeCounter = CoyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = JumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f)
        {
            // Saut normal (sol + coyote time)
            if (coyoteTimeCounter > 0f)
            {
                PerformJump();
                coyoteTimeCounter = 0f;
            }
            // Double saut (en l'air, si des sauts restent)
            else if (airJumpsLeft > 0)
            {
                PerformJump();
                airJumpsLeft--;  
            }
        }
    }

    void PerformJump()
    {
        float force = (jumpMode == JumpMode.High) ? HighJumpForce : JumpForce;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        jumpBufferCounter = 0f;
    }

    void FixedUpdate()
    {
        if (isDead) return;
        HandleMovement();
    }

    void HandleMovement()
    {
        float targetSpeed = inputX * MoveSpeed;
        float currentSpeed = rb.linearVelocity.x;

        // Choisit l'accélération ou la décélération selon la situation
        float rate = (Mathf.Abs(targetSpeed) > 0.01f) ? Acceleration : Deceleration;

        // Lisse la vitesse vers la cible
        float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
    }

    // ── Mort & Respawn ─────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("dead_zone") || other.gameObject.layer == LayerMask.NameToLayer("dead_zone"))
            Die();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("dead_zone") || other.gameObject.layer == LayerMask.NameToLayer("dead_zone"))
            Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        SpawnManager.Instance.Respawn(this);
    }

    public void Revive(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;
        isDead = false;
        GetComponent<PlayerHealth>()?.ResetHealth();
    }
}