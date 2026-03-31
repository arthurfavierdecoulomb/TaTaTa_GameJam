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
    JumpMode jumpMode = JumpMode.Normal;

    public bool IsInputLocked { get; set; } = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        ResetJumps(); // assure 3 icônes visibles au spawn
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
                UpdateJumpUI(); // enlève une icône
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

    void UpdateJumpUI()
    {
        for (int i = 0; i < jumpIcons.Length; i++)
        {
            //ACTIVE / DESACTIVE complètement l'image
            jumpIcons[i].gameObject.SetActive(i < airJumpsLeft);
        }
    }

    // Recharge complète (respawn)
    public void ResetJumps()
    {
        airJumpsLeft = MaxAirJumps;

        // Force toutes les icônes visibles AVANT update (corrige ton bug des 2 icônes)
        for (int i = 0; i < jumpIcons.Length; i++)
        {
            jumpIcons[i].gameObject.SetActive(true);
        }

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