using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;  // Visible in Inspector for debugging

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("References")]
    public Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    public bool isDead { get; private set; } = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void TakeDamage(int damage, Transform attacker = null)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("Player Health: " + currentHealth); // shows health in console

        animator?.SetTrigger("Hurt");

        if (attacker != null)
        {
            Vector2 knockbackDirection = (transform.position - attacker.position).normalized;
            StartCoroutine(ApplyKnockback(knockbackDirection));
        }

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        float timer = 0f;
        while (timer < knockbackDuration)
        {
            rb.velocity = new Vector2(direction.x * knockbackForce, rb.velocity.y);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        animator?.SetTrigger("Death"); // play death animation

        rb.velocity = new Vector2(0, rb.velocity.y);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // allow falling

        HeroKnight hero = GetComponent<HeroKnight>();
        if (hero != null) hero.enabled = false;

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = false;

        StartCoroutine(FreezeWhenGrounded());
    }

    private IEnumerator FreezeWhenGrounded()
    {
        while (!IsGrounded())
        {
            yield return null;
        }

        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
}
