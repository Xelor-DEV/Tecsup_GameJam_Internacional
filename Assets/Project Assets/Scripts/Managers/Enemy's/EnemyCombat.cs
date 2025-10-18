using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private EnemyAI enemyAI;

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private Entity entityTarget = Entity.Player;
    [SerializeField] private float maxAttackDuration = 3f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Attack Priority")]
    [SerializeField] private AttackPriority attackPriority;

    [Header("Animation Parameters")]
    [SerializeField] private string attackBoolName = "IsAttacking";

    // Estados de combate
    private bool isAttacking = false;
    private bool detectionActive = false;
    private bool hasAppliedDamage = false;
    private float lastAttackTime;
    private bool hasFirstAttack = false; // NUEVO: Para controlar el primer ataque

    private bool wasInterrupted = false;

    // Safety timer para evitar ataques infinitos
    private float attackStartTime;

    // NUEVA PROPIEDAD: Primer ataque no tiene cooldown
    public bool CanAttack => !hasFirstAttack || Time.time >= lastAttackTime + attackCooldown;
    public bool IsAttacking => isAttacking;
    public AttackPriority Priority => attackPriority;

    private Vector2 originalAttackPointPosition;

    [Header("Fast Attack Settings")]
    [SerializeField] private bool useImmediateFirstAttack = true;
    [SerializeField] private float immediateAttackWindow = 0.1f; // Ventana para ataque inmediato

    // Nueva propiedad
    public bool HasFirstAttack => hasFirstAttack;

    private void Awake()
    {
        if (attackPoint != null)
        {
            originalAttackPointPosition = attackPoint.localPosition;
        }

        if (enemyAI == null)
        {
            enemyAI = GetComponent<EnemyAI>();
        }

        // Inicializar prioridad si es nula
        if (attackPriority == null)
        {
            attackPriority = new AttackPriority();
        }
    }

    private void Update()
    {
        // Safety check: si el ataque dura demasiado, forzar finalización
        if (isAttacking && Time.time - attackStartTime > maxAttackDuration)
        {
            Debug.LogWarning($"⚠️ Ataque de enemigo excedió tiempo máximo. Forzando finalización.");
            ForceFinishAttack();
        }
    }

    public void StartAttack()
    {
        if (!CanAttack || isAttacking) return;

        isAttacking = true;
        wasInterrupted = false;
        detectionActive = false;
        hasAppliedDamage = false;
        lastAttackTime = Time.time;
        attackStartTime = Time.time;

        // ACTIVAR DETECCIÓN INMEDIATAMENTE para el primer frame
        detectionActive = true;
        hasAppliedDamage = false;

        if (!hasFirstAttack)
        {
            hasFirstAttack = true;
        }

        if (enemyAI != null)
        {
            enemyAI.SetMovementEnabled(false);
        }

        if (animator != null)
        {
            animator.SetBool(attackBoolName, true);
        }

        Debug.Log("🧌 Ataque de enemigo iniciado INMEDIATAMENTE");
    }

    public void StartImmediateAttack()
    {
        if (isAttacking) return;

        // Ignorar cooldown para ataque inmediato
        isAttacking = true;
        wasInterrupted = false;
        detectionActive = true; // Activar detección inmediatamente
        hasAppliedDamage = false;
        attackStartTime = Time.time;

        if (enemyAI != null)
        {
            enemyAI.SetMovementEnabled(false);
        }

        if (animator != null)
        {
            animator.SetBool(attackBoolName, true);
        }

        // Forzar primera detección inmediata
        DetectTargets();

        Debug.Log("⚡ Ataque INMEDIATO iniciado");
    }

    // Resto del código se mantiene igual...
    // Animation Event: Activar detección
    public void ActivateDetection()
    {
        if (wasInterrupted) return; // No activar si fue interrumpido

        detectionActive = true;
        Debug.Log("⚔️ Detección de enemigo ACTIVADA");
    }

    // Animation Event: Desactivar detección
    public void DeactivateDetection()
    {
        detectionActive = false;
        Debug.Log("🔒 Detección de enemigo DESACTIVADA");
    }

    // Animation Event: Finalizar ataque
    public void FinishAttack()
    {
        if (wasInterrupted) return; // No hacer nada si ya fue interrumpido

        isAttacking = false;
        detectionActive = false;
        hasAppliedDamage = false;

        if (animator != null)
        {
            animator.SetBool(attackBoolName, false);
        }

        if (enemyAI != null)
        {
            enemyAI.SetMovementEnabled(true);
        }

        Debug.Log("✅ Ataque de enemigo finalizado NORMALMENTE");
    }

    // NUEVO MÉTODO: Resetear el estado de primer ataque (opcional, si quieres resetear en ciertas situaciones)
    public void ResetFirstAttack()
    {
        hasFirstAttack = false;
    }

    private void FixedUpdate()
    {
        // Para el primer ataque, hacer detección inmediata
        if (isAttacking && !hasAppliedDamage && !wasInterrupted)
        {
            if (!hasFirstAttack || Time.time - attackStartTime < 0.1f) // Primeros 0.1 segundos
            {
                DetectTargets();
            }
        }

        // Detección normal durante el ataque
        if (detectionActive && !hasAppliedDamage && !wasInterrupted)
        {
            DetectTargets();
        }

        UpdateAttackPointPosition();
    }

    private void DetectTargets()
    {
        if (attackPoint == null) return;

        Vector2 detectionPos = attackPoint.position;
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(detectionPos, attackRadius, targetLayers);

        foreach (Collider2D target in hitTargets)
        {
            EntityIdentifier entity = target.GetComponent<EntityIdentifier>();
            if (entity != null && entity.Entity == entityTarget)
            {
                // Buscar HealthManager directamente en el objetivo y sus hijos
                HealthManager healthManager = target.GetComponent<HealthManager>();
                if (healthManager != null && healthManager.CurrentHealth > 0)
                {
                    // Buscar PlayerCombat en el objetivo y sus hijos para verificar prioridad
                    PlayerCombat playerCombat = target.GetComponentInChildren<PlayerCombat>();

                    // Aplicar sistema de prioridad solo si el jugador está atacando
                    if (playerCombat != null && playerCombat.IsAttacking)
                    {
                        bool attackerWins;
                        bool hasDecision = CombatPriorityManager.Instance.CheckAttackPriority(
                            this.attackPriority,
                            playerCombat.Priority,
                            out attackerWins
                        );

                        if (hasDecision && !attackerWins)
                        {
                            // El jugador gana la prioridad, no aplicamos daño
                            Debug.Log("🎯 Jugador gana prioridad - Enemigo no aplica daño");
                            continue;
                        }
                    }

                    // Aplicar daño al HealthManager del jugador
                    healthManager.TakeDamage(attackDamage);
                    hasAppliedDamage = true;

                    // Aplicar knockback al jugador si tiene el componente
                    PlayerKnockback playerKnockback = target.GetComponent<PlayerKnockback>();
                    if (playerKnockback != null)
                    {
                        playerKnockback.ApplyKnockback();
                    }

                    Debug.Log($"💥 Enemigo golpeó: {target.name} - Daño: {attackDamage}");
                    break;
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

    public bool IsTargetInAttackRange(Transform target)
    {
        if (target == null || attackPoint == null) return false;
        return Vector2.Distance(attackPoint.position, target.position) <= attackRadius;
    }

    // Resto de métodos existentes (InterruptAttack, ForceFinishAttack, etc.) se mantienen igual...
    public void InterruptAttack()
    {
        if (!isAttacking) return;

        // Si tiene HyperArmor, no se interrumpe
        if (attackPriority.hasHyperArmor)
        {
            Debug.Log("🛡️ Enemigo con HyperArmor - Ataque no interrumpido");
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
        if (enemyAI != null)
        {
            enemyAI.SetMovementEnabled(true);
        }

        isAttacking = false;
        Debug.Log("🚫 Ataque de enemigo INTERRUMPIDO por daño");
    }

    private void ForceFinishAttack()
    {
        isAttacking = false;
        detectionActive = false;
        hasAppliedDamage = false;

        if (animator != null)
        {
            animator.SetBool(attackBoolName, false);
        }

        if (enemyAI != null)
        {
            enemyAI.SetMovementEnabled(true);
        }

        Debug.Log("🛠️ Ataque forzado a finalizar (safety timer)");
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