using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private Entity entityTarget = Entity.Enemy;

    [Header("Attack Priority")]
    [SerializeField] private AttackPriority attackPriority;

    [Header("Animation Parameters")]
    [SerializeField] private string attackBoolName = "IsAttacking";

    // Estados de combate
    private bool isAttacking = false;
    private bool detectionActive = false;
    private bool hasAppliedDamage = false;
    private bool attackInputReceived = false;
    private bool wasInterrupted = false; // NUEVO: Para controlar interrupciones

    private Vector2 originalAttackPointPosition;

    // PROPIEDADES PÚBLICAS - AÑADIDAS
    public AttackPriority Priority => attackPriority;
    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        if (attackPoint != null)
        {
            originalAttackPointPosition = attackPoint.localPosition;
        }

        // Asegurarse de tener referencia al PlayerMovement
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        // Inicializar prioridad si es nula
        if (attackPriority == null)
        {
            attackPriority = new AttackPriority();
        }
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        // SOLO en performed leemos como botón y guardamos el valor
        if (context.performed)
        {
            attackInputReceived = context.ReadValueAsButton();
        }
    }

    private void Update()
    {
        UpdateAttackPointPosition();

        // Verificación del input de ataque en Update
        if (attackInputReceived && !isAttacking && playerMovement != null && playerMovement.CanMove())
        {
            StartAttack();
            attackInputReceived = false; // Resetear después de usar
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        wasInterrupted = false; // NUEVO: Resetear estado de interrupción
        detectionActive = false;
        hasAppliedDamage = false;

        // Desactivar movimiento durante el ataque
        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(false);
        }

        // Activar booleano de animación
        if (animator != null)
        {
            animator.SetBool(attackBoolName, true);
        }

        Debug.Log("🗡️ Ataque iniciado");
    }

    // NUEVO MÉTODO: Interrumpir ataque cuando el jugador recibe daño
    public void InterruptAttack()
    {
        if (!isAttacking) return;

        // Si tiene HyperArmor, no se interrumpe
        if (attackPriority != null && attackPriority.hasHyperArmor)
        {
            Debug.Log("🛡️ Jugador con HyperArmor - Ataque no interrumpido");
            return;
        }

        wasInterrupted = true;
        detectionActive = false;
        hasAppliedDamage = false;

        // Desactivar booleano de animación
        if (animator != null)
        {
            animator.SetBool(attackBoolName, false);
        }

        // Reactivar movimiento
        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(true);
        }

        isAttacking = false;
        Debug.Log("🚫 Ataque de jugador INTERRUMPIDO");
    }

    // Animation Event: Activar detección (llamado desde Animation Clip)
    public void ActivateDetection()
    {
        if (wasInterrupted) return; // NUEVO: No activar si fue interrumpido
        detectionActive = true;
        Debug.Log("⚔️ Detección ACTIVADA");
    }

    // Animation Event: Desactivar detección (llamado desde Animation Clip)
    public void DeactivateDetection()
    {
        detectionActive = false;
        Debug.Log("🔒 Detección DESACTIVADA");
    }

    // Animation Event: Finalizar ataque (llamado desde Animation Clip)
    public void FinishAttack()
    {
        if (wasInterrupted) return; // NUEVO: No hacer nada si fue interrumpido

        isAttacking = false;
        detectionActive = false;
        hasAppliedDamage = false;

        // Desactivar booleano de animación
        if (animator != null)
        {
            animator.SetBool(attackBoolName, false);
        }

        // Reactivar movimiento después del ataque
        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(true);
        }

        Debug.Log("✅ Ataque finalizado");
    }

    private void FixedUpdate()
    {
        if (detectionActive && !hasAppliedDamage && !wasInterrupted) // NUEVO: Verificar interrupción
        {
            DetectEnemies();
        }
    }

    private void DetectEnemies()
    {
        if (attackPoint == null) return;

        Vector2 detectionPos = attackPoint.position;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(detectionPos, attackRadius, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EntityIdentifier entity = enemy.GetComponent<EntityIdentifier>();
            if (entity != null)
            {
                if (entity.Entity == entityTarget)
                {
                    // Buscar HealthManager en el enemigo
                    Attackable attackable = entity.GetComponent<Attackable>();
                    if (attackable != null && attackable.IsAlive())
                    {
                        // NUEVO: Aplicar sistema de prioridad contra enemigos
                        EnemyCombat enemyCombat = enemy.GetComponentInChildren<EnemyCombat>();
                        if (enemyCombat != null && enemyCombat.IsAttacking)
                        {
                            bool attackerWins;
                            bool hasDecision = CombatPriorityManager.Instance.CheckAttackPriority(
                                this.attackPriority,
                                enemyCombat.Priority,
                                out attackerWins
                            );

                            if (hasDecision && !attackerWins)
                            {
                                // El enemigo gana la prioridad, no aplicamos daño
                                Debug.Log("🎯 Enemigo gana prioridad - Jugador no aplica daño");
                                continue;
                            }
                        }

                        attackable.PlayerMovement = playerMovement;
                        // Aplicar daño al HealthManager del enemigo
                        attackable.TakeDamage(attackDamage);
                        hasAppliedDamage = true;
                        Debug.Log($"💥 Jugador golpeó: {enemy.name} - Daño: {attackDamage}");
                        break;
                    }
                }
            }
        }
    }

    private void UpdateAttackPointPosition()
    {
        if (attackPoint == null || spriteRenderer == null) return;

        float xPosition = spriteRenderer.flipX ?
            -originalAttackPointPosition.x : originalAttackPointPosition.x;

        attackPoint.localPosition = new Vector2(xPosition, originalAttackPointPosition.y);
    }

    private void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = detectionActive ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}