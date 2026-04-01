using UnityEngine;

public class CharaAnimator : MonoBehaviour
{
    [Header("Pause")]
    public UIController uiController;

    [Header("Refs")]
    public Rigidbody2D rb;
    public Animator animator;

    [Header("Détection sol")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    void Update()
    {
        bool curPause = uiController.Pause;
        if (!curPause)
        {
            bool isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );


            bool isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.1f && isGrounded;
            bool isJumping = !isGrounded;

            Debug.Log(isJumping);
            animator.SetBool("Course", isRunning);
            animator.SetBool("Jump", isJumping);
        }
    }
}