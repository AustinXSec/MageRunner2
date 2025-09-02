using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlyingEye : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 0.5f;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("References")]
    public Transform player;
    private Rigidbody2D rb;
    private Collider2D col;
    public Animator animator;

    public bool isDead { get; private set; } = false;

    private Vector2 movement;
    private Vector3 originalScale;
    private bool isKnockedBack = false;

    void Start()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        originalScale = transform.localScale;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        Vector2 direction = (player.position - transform.position);
        float distance = direction.magnitude;

        // Move toward player if farther than stopping distance
        movement = (distance > stoppingDistance) ? direction.normalized : Vector2.zero;

        // Flip sprite to face player
        if (direction.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (!isKnockedBack)
            rb.velocity = movement * moveSpeed;
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
        isKnockedBack = true;

        float timer = 0f;
        while (timer < knockbackDuration)
        {
            rb.velocity = direction * knockbackForce;
            timer += Time.deltaTime;
            yield return null;
        }

        isKnockedBack = false;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        animator.SetBool("isDead", true);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 1f;       // enable gravity so it falls
            rb.constraints = RigidbodyConstraints2D.None; // allow full movement
        }

        if (col != null)
        {
            col.isTrigger = true;       // fall through all colliders
        }

        this.enabled = false;           // stop AI
    }
}
