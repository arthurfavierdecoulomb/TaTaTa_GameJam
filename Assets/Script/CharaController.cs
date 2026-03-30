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
    int airJumpsLeft;
    bool isGrounded;
    JumpMode jumpMode = JumpMode.Normal;

    // ✅ Exposé pour que PlayerHealth puisse bloquer les inputs
    public bool IsInputLocked { get; set; } = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (IsInputLocked) return;

        inputX = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.Raycast(
            transform.position, Vector2.down, GroundCheckDistance, groundLayer
        );

        if (isGrounded)
        {
            airJumpsLeft = MaxAirJumps;
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
            if (coyoteTimeCounter > 0f)
            {
                PerformJump();
                coyoteTimeCounter = 0f;
            }
            else if (airJumpsLeft > 0)
            {
                PerformJump();
                airJumpsLeft--;
            }
        }
    }

    void FixedUpdate()
    {
        if (IsInputLocked) return;
        HandleMovement();
    }

    void HandleMovement()
    {
        float targetSpeed = inputX * MoveSpeed;
        float currentSpeed = rb.linearVelocity.x;
        float rate = (Mathf.Abs(targetSpeed) > 0.01f) ? Acceleration : Deceleration;
        float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
    }

    void PerformJump()
    {
        float force = (jumpMode == JumpMode.High) ? HighJumpForce : JumpForce;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        jumpBufferCounter = 0f;
    }

    
    public void FreezePhysics(bool freeze)
    {
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = freeze ? 0f : 1f;
    }

    public void Teleport(Vector3 position)
    {
        transform.position = position;
    }
}