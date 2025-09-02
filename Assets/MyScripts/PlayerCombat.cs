using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;

    [Header("Attack Settings")]
    public float attackRange = 1.0f;
    public LayerMask enemyLayers;
    public int attackDamage = 40;

    [Header("Attack Timing")]
    public float attackDuration = 0.25f;
    public int hitFrames = 5;

    private int currentAttack = 0;
    private bool isAttacking = false;
    private bool attackQueued = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (isAttacking)
                attackQueued = true;
            else
                StartCoroutine(PerformAttack());
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        attackQueued = false;

        // Combo cycle
        currentAttack++;
        if (currentAttack > 3) currentAttack = 1;

        animator.SetTrigger("Attack" + currentAttack);

        float interval = attackDuration / hitFrames;
        for (int i = 0; i < hitFrames; i++)
        {
            DealDamage();
            yield return new WaitForSeconds(interval);
        }

        isAttacking = false;

        if (attackQueued)
            StartCoroutine(PerformAttack());
    }

    void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayers);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            IDamageable damageable = enemyCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage, transform);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
