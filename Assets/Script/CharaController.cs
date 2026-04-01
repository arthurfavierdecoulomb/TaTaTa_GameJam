using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] int MaxAirJumps = 3;

    [Header("Jump UI")]
    [SerializeField] Image[] jumpIcons;

    [Header("Ground Check")]
    [SerializeField] float GroundCheckDistance = 1.1f;
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb;
    float inputX;
    float coyoteTimeCounter;
    float jumpBufferCounter;
    int airJumpsLeft;
    bool isGrounded;
    bool wasGrounded;
    bool hasJumped = false;
    JumpMode jumpMode = JumpMode.Normal;

    public bool IsInputLocked { get; set; } = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }

    void Start()
    {
        ResetJumps();
    }

    void Update()
    {
        if (IsInputLocked) return;

        inputX = Input.GetAxisRaw("Horizontal");

        wasGrounded = isGrounded;
        isGrounded = Physics2D.Raycast(
            transform.position, Vector2.down, GroundCheckDistance, groundLayer
        );

        // Atterrissage → remet juste hasJumped, PAS les double sauts
        if (!wasGrounded && isGrounded)
        {
            hasJumped = false;
        }

        if (isGrounded)
            coyoteTimeCounter = CoyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = JumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f)
        {
            // Saut depuis le sol
            if (coyoteTimeCounter > 0f && !hasJumped)
            {
                PerformJump();
                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
                hasJumped = true;
            }
            // Double saut en l'air
            else if (airJumpsLeft > 0 && hasJumped)
            {
                PerformJump();
                airJumpsLeft--;
                UpdateJumpUI();
                jumpBufferCounter = 0f;
            }
        }
    }

    void FixedUpdate()
    {
        
        if (IsInputLocked) return;
        HandleMovement();
        if (rb.rotation != 0f)
            rb.rotation = 0f;

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

    void UpdateJumpUI()
    {
        for (int i = 0; i < jumpIcons.Length; i++)
        {
            jumpIcons[i].gameObject.SetActive(i < airJumpsLeft);
        }
    }

    public void ResetJumps()
    {
        airJumpsLeft = MaxAirJumps;
        hasJumped = false;
        UpdateJumpUI();
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