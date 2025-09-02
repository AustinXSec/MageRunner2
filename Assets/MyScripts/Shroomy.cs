using UnityEngine;

public class Shroomy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float jumpForce = 5f;

    [Header("Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public Transform frontCheck;
    public float frontCheckDistance = 0.5f;

    [Header("References")]
    public Transform player;
    public Animator animator;

    private Rigidbody2D rb;
    private bool isGrounded;
    private Vector3 originalScale;
    private Enemy enemyScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        enemyScript = GetComponent<Enemy>();
    }

    void Update()
    {
        // Stop AI movement if dead, but allow gravity to fall
        if (enemyScript != null && enemyScript.isDead)
            return;

        // Check if grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Follow player horizontally
        Vector2 direction = player.position - transform.position;
        rb.velocity = new Vector2(Mathf.Sign(direction.x) * moveSpeed, rb.velocity.y);

        // Flip sprite correctly
        if (direction.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

        // Obstacle detection and jump
        RaycastHit2D hit = Physics2D.Raycast(frontCheck.position, Vector2.right * Mathf.Sign(transform.localScale.x), frontCheckDistance, groundLayer);
        if (hit.collider != null && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Update animator
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalVelocity", rb.velocity.y);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (frontCheck != null)
            Gizmos.DrawLine(frontCheck.position, frontCheck.position + Vector3.right * frontCheckDistance * Mathf.Sign(transform.localScale.x));
    }
}
