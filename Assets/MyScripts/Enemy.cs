using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public Animator animator;

    [Header("Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Collider2D[] groundColliders; // to ignore collisions

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
        animator.SetTrigger("Hurt");

        if (attacker != null)
        {
            Vector2 knockbackDirection = (transform.position - attacker.position).normalized;
            StartCoroutine(ApplyKnockback(knockbackDirection));
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator ApplyKnockback(Vector2 direction)
    {
        direction.y = 0f;
        direction.Normalize();

        float timer = 0f;
        while (timer < knockbackDuration)
        {
            rb.velocity = new Vector2(direction.x * knockbackForce, rb.velocity.y);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetBool("IsDead", true);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;       // stop horizontal movement
            rb.gravityScale = 1f;             // keep falling down
            rb.constraints = RigidbodyConstraints2D.None; // allow full movement
        }

        if (col != null)
        {
            // Make the enemy a trigger so it falls through everything
            col.isTrigger = true;
        }

        // Optionally disable AI scripts (like Shroomy.cs) so Update() stops
        this.enabled = false;
    }
}
