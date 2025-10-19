using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrolling, Chasing, Attacking, Stunned }

    [Header("AI Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float lostTargetWaitTime = 1f;
    [SerializeField] private Transform patrolArea;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private EnemyVision vision;
    [SerializeField] private EnemyCombat combat;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [Header("Animation Parameters")]
    [SerializeField] private string movingBoolName = "IsMoving";

    private EnemyState currentState = EnemyState.Patrolling;
    private Vector2 patrolMinBounds;
    private Vector2 patrolMaxBounds;
    private Vector2 currentPatrolTarget;
    private bool isWaiting = false;
    private float stateTime = 0f;
    private bool wasMoving = false;

    public Rigidbody2D Rb => rb;
    public EnemyState CurrentState => currentState;

    private void Start()
    {
        CalculatePatrolBounds();
        SetRandomPatrolTarget();

        // Inicializar estado de animación
        UpdateMovementAnimation(false);
    }

    private void Update()
    {
        stateTime += Time.deltaTime;

        switch (currentState)
        {
            case EnemyState.Patrolling:
                UpdatePatrolState();
                break;
            case EnemyState.Chasing:
                UpdateChaseState();
                break;
            case EnemyState.Attacking:
                UpdateAttackState();
                break;
            case EnemyState.Stunned:
                // No hacer nada durante stun
                break;
        }

        UpdateFacingDirection();
        UpdateMovementState();
    }

    private void UpdatePatrolState()
    {
        // Moverse hacia el punto de patrulla
        if (!isWaiting)
        {
            Vector2 direction = (currentPatrolTarget - (Vector2)transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * patrolSpeed, direction.y * patrolSpeed);

            if (Vector2.Distance(transform.position, currentPatrolTarget) < 0.5f)
            {
                StartCoroutine(WaitAtPatrolPoint());
            }
        }

        // Transición: Detectar jugador
        if (vision.CanSeeTarget)
        {
            ChangeState(EnemyState.Chasing);
        }
    }

    private void UpdateChaseState()
    {
        if (vision.Target == null)
        {
            if (stateTime > lostTargetWaitTime)
            {
                ChangeState(EnemyState.Patrolling);
            }
            return;
        }

        // Verificar si el objetivo está en rango de ataque
        bool inAttackRange = combat.IsTargetInAttackRange(vision.Target);

        // Notificar al combat si el objetivo sale del rango
        if (!inAttackRange && combat.HasFirstAttack)
        {
            combat.NotifyTargetExitedRange();
        }

        // Si el objetivo está en rango de ataque, atacar
        if (inAttackRange)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }

        // Perseguir al objetivo
        Vector2 direction = (vision.Target.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, direction.y * chaseSpeed);
    }

    private void UpdateAttackState()
    {
        // Detener movimiento durante ataque
        rb.linearVelocity = Vector2.zero;

        // Solo intentar atacar si NO está actualmente atacando y PUEDE atacar
        if (!combat.IsAttacking && combat.CanAttack)
        {
            // DECISIÓN MEJORADA: Usar ataque inmediato solo si es el primer ataque del encuentro
            // y se permite el ataque inmediato, O si tiene HyperArmor
            bool shouldUseImmediateAttack = (!combat.HasFirstAttack && combat.AllowImmediateFirstAttack) ||
                                          combat.Priority.hasHyperArmor;

            if (shouldUseImmediateAttack)
            {
                combat.StartImmediateAttack();
            }
            else
            {
                combat.StartAttack();
            }
        }

        // Transiciones MEJORADAS
        if (vision.Target == null)
        {
            ChangeState(EnemyState.Patrolling);
        }
        else if (!combat.IsAttacking && !combat.CanAttack)
        {
            // Si no puede atacar (en cooldown) y no está atacando, volver a perseguir
            ChangeState(EnemyState.Chasing);
        }
        else if (!combat.IsTargetInAttackRange(vision.Target))
        {
            // Si el objetivo sale del rango de ataque, volver a perseguir
            ChangeState(EnemyState.Chasing);
        }
    }

    private void CheckTargetExitRange()
    {
        // Si el objetivo sale del rango de ataque, resetear el primer ataque para el próximo encuentro
        if (vision.Target != null && !combat.IsTargetInAttackRange(vision.Target) && combat.HasFirstAttack)
        {
            // Opcional: puedes agregar un pequeño delay antes de resetear
            StartCoroutine(ResetFirstAttackAfterDelay());
        }
    }

    private IEnumerator ResetFirstAttackAfterDelay()
    {
        yield return new WaitForSeconds(1f); // Esperar 1 segundo antes de resetear
        if (vision.Target != null && !combat.IsTargetInAttackRange(vision.Target))
        {
            combat.ResetFirstAttack();
            Debug.Log("🔄 Primer ataque reseteado - jugador salió del rango");
        }
    }

    private void UpdateMovementState()
    {
        // Calcular si se está moviendo (velocidad significativa)
        bool isMovingNow = rb.linearVelocity.magnitude > 0.1f;

        // Solo actualizar la animación si el estado cambió
        if (isMovingNow != wasMoving)
        {
            UpdateMovementAnimation(isMovingNow);
            wasMoving = isMovingNow;
        }
    }

    private void UpdateMovementAnimation(bool isMoving)
    {
        if (animator != null && !string.IsNullOrEmpty(movingBoolName))
        {
            animator.SetBool(movingBoolName, isMoving);
        }
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        rb.linearVelocity = Vector2.zero;

        // Asegurar que la animación se actualice inmediatamente al detenerse
        UpdateMovementAnimation(false);

        yield return new WaitForSeconds(patrolWaitTime);

        isWaiting = false;
        SetRandomPatrolTarget();

        // La animación se actualizará automáticamente en UpdateMovementState cuando empiece a moverse
    }

    private void SetRandomPatrolTarget()
    {
        currentPatrolTarget = new Vector2(
            Random.Range(patrolMinBounds.x, patrolMaxBounds.x),
            Random.Range(patrolMinBounds.y, patrolMaxBounds.y)
        );
    }

    private void CalculatePatrolBounds()
    {
        if (patrolArea != null)
        {
            patrolMinBounds = patrolArea.position - patrolArea.localScale / 2f;
            patrolMaxBounds = patrolArea.position + patrolArea.localScale / 2f;
        }
        else
        {
            patrolMinBounds = (Vector2)transform.position - Vector2.one * 3f;
            patrolMaxBounds = (Vector2)transform.position + Vector2.one * 3f;
        }
    }

    private void UpdateFacingDirection()
    {
        if (spriteRenderer == null) return;

        // Solo actualizar la dirección si se está moviendo
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            if (rb.linearVelocity.x > 0.1f)
                spriteRenderer.flipX = true;
            else if (rb.linearVelocity.x < -0.1f)
                spriteRenderer.flipX = false;
        }
    }

    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        // Salir del estado actual
        switch (currentState)
        {
            case EnemyState.Patrolling:
                StopAllCoroutines();
                isWaiting = false;
                break;
            case EnemyState.Chasing:
                // Notificar al combat cuando sale de chasing (objetivo perdido)
                if (newState == EnemyState.Patrolling)
                {
                    combat.NotifyTargetExitedRange();
                }
                break;
            case EnemyState.Attacking:
                // Si está saliendo de ataque y va a patrulla, resetear
                if (newState == EnemyState.Patrolling)
                {
                    combat.NotifyTargetExitedRange();
                }
                break;
        }

        // Entrar al nuevo estado
        currentState = newState;
        stateTime = 0f;

        switch (newState)
        {
            case EnemyState.Patrolling:
                SetRandomPatrolTarget();
                break;
            case EnemyState.Chasing:
                rb.linearVelocity = Vector2.zero;
                UpdateMovementAnimation(false);
                break;
            case EnemyState.Attacking:
                rb.linearVelocity = Vector2.zero;
                UpdateMovementAnimation(false);
                break;
            case EnemyState.Stunned:
                rb.linearVelocity = Vector2.zero;
                UpdateMovementAnimation(false);
                break;
        }

        Debug.Log($"🔄 Enemigo cambió de estado: {currentState} -> {newState}");
    }

    public void SetStunned(bool stunned)
    {
        if (stunned)
        {
            ChangeState(EnemyState.Stunned);
            rb.linearVelocity = Vector2.zero;
            UpdateMovementAnimation(false);
        }
        else
        {
            ChangeState(EnemyState.Patrolling);
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        if (enabled)
        {
            // Reactivar movimiento según el estado actual
            switch (currentState)
            {
                case EnemyState.Patrolling:
                    // Ya se reactivará automáticamente en UpdatePatrolState
                    break;
                case EnemyState.Chasing:
                    // Ya se reactivará automáticamente en UpdateChaseState
                    break;
            }
        }
        else
        {
            // Detener movimiento inmediatamente
            rb.linearVelocity = Vector2.zero;
            UpdateMovementAnimation(false);
        }
    }

    private void OnDrawGizmos()
    {
        // Dibujar área de patrulla
        if (patrolArea != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(patrolArea.position, patrolArea.localScale);
        }

        // Dibujar punto de patrulla actual
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(currentPatrolTarget, 0.3f);
    }
}