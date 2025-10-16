using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    private bool isAttacking = false;
    private bool detectionActive = false;


    public void Attack(InputAction.CallbackContext callback)
    {
        if (callback.performed && !isAttacking)
        {
            Attack();
        }
    }

    public void Attack()
    {
        if (isAttacking) return;

        isAttacking = true;
        animator.SetTrigger("Attack");
    }

    // Animation Event: ACTIVAR detección
    public void ActivateAttackDetection()
    {
        detectionActive = true;
        Debug.Log("⚔️ Detección ACTIVADA");
    }

    // Animation Event: DESACTIVAR detección  
    public void DeactivateAttackDetection()
    {
        detectionActive = false;
        Debug.Log("⚔️ Detección DESACTIVADA");
    }

    // Animation Event: Fin del ataque
    public void OnAttackEnd()
    {
        isAttacking = false;
        Debug.Log("Ataque terminado");
    }

    void FixedUpdate()
    {
        if (detectionActive)
        {
            DetectEnemies();
        }
    }

    void DetectEnemies()
    {
        // Ajustar posición del attackPoint según dirección
        Vector2 detectionPos = attackPoint.position;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(detectionPos, attackRadius, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Attackable target = enemy.GetComponent<Attackable>();
                if (target != null && target.IsAlive())
                {
                    target.TakeDamage(attackDamage);
                    Debug.Log($"💥 Golpeado: {enemy.name} - Daño: {attackDamage}");
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = detectionActive ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}