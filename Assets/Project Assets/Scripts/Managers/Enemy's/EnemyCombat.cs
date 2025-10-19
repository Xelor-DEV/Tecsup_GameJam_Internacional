using UnityEngine;
using DG.Tweening;

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

    [Header("Headbutt Animation Settings")]
    [SerializeField] private bool useHeadbuttAnimation = true;
    [SerializeField] private float headbuttDistance = 1f;
    [SerializeField] private float headbuttHeight = 0.5f;
    [SerializeField] private float headbuttDuration = 0.3f;
    [SerializeField] private AnimationCurve headbuttCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Transform headbuttTransform;
    [Range(0.1f, 1f)]
    [SerializeField] private float headbuttCurveCompletion = 0.5f; // Nuevo: controla cuánto de la curva se recorre

    // Estados de combate
    private bool isAttacking = false;
    private bool detectionActive = false;
    private bool hasAppliedDamage = false;
    private float lastAttackTime;
    private bool hasFirstAttack = false;

    private bool wasInterrupted = false;

    // Safety timer para evitar ataques infinitos
    private float attackStartTime;

    // Variables para la animación de cabezaso
    private Tween headbuttTween;
    private Vector3 originalHeadbuttPosition;

    // NUEVA PROPIEDAD: Primer ataque no tiene cooldown
    public bool CanAttack => !hasFirstAttack || Time.time >= lastAttackTime + attackCooldown;
    public bool IsAttacking => isAttacking;
    public AttackPriority Priority => attackPriority;

    private Vector2 originalAttackPointPosition;

    [Header("Fast Attack Settings")]
    [SerializeField] private bool useImmediateFirstAttack = true;
    [SerializeField] private float immediateAttackWindow = 0.1f;

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

        // Guardar posición original del transform del cabezaso
        if (headbuttTransform != null)
        {
            originalHeadbuttPosition = headbuttTransform.localPosition;
        }
        else
        {
            headbuttTransform = transform;
            originalHeadbuttPosition = Vector3.zero;
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

        // Iniciar animación de cabezaso si está activada
        if (useHeadbuttAnimation && headbuttTransform != null)
        {
            StartHeadbuttAnimation();
        }

        Debug.Log("🧌 Ataque de enemigo iniciado INMEDIATAMENTE");
    }

    public void StartImmediateAttack()
    {
        if (isAttacking) return;

        // Ignorar cooldown para ataque inmediato
        isAttacking = true;
        wasInterrupted = false;
        detectionActive = true;
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

        // Iniciar animación de cabezaso si está activada
        if (useHeadbuttAnimation && headbuttTransform != null)
        {
            StartHeadbuttAnimation();
        }

        // Forzar primera detección inmediata
        DetectTargets();

        Debug.Log("⚡ Ataque INMEDIATO iniciado");
    }

    private void StartHeadbuttAnimation()
    {
        // Cancelar animación anterior si existe
        if (headbuttTween != null && headbuttTween.IsActive())
        {
            headbuttTween.Kill();
        }

        // Determinar dirección horizontal basada en el flip del sprite
        Vector3 horizontalDirection = spriteRenderer.flipX ? Vector3.left : Vector3.right;

        // Calcular posición objetivo (mitad de la distancia + altura)
        Vector3 targetPosition = originalHeadbuttPosition +
                               (horizontalDirection * (headbuttDistance * 0.5f)) +
                               (Vector3.up * headbuttHeight);

        // Calcular dirección del movimiento para la rotación
        Vector3 moveDirection = (targetPosition - originalHeadbuttPosition).normalized;
        float targetZRotation = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;

        // Ajustar según la dirección del sprite
        if (spriteRenderer.flipX)
        {
            targetZRotation += 180f; // Ajuste para dirección izquierda
        }

        // Crear secuencia para animación completa
        Sequence headbuttSequence = DOTween.Sequence();

        // Animación de ida con trayectoria curva y rotación que sigue la dirección
        headbuttSequence.Append(headbuttTransform.DOLocalMove(targetPosition, headbuttDuration)
                       .SetEase(headbuttCurve))
                       .Join(headbuttTransform.DOLocalRotate(new Vector3(0, 0, targetZRotation), headbuttDuration)
                       .SetEase(headbuttCurve))
                       .OnComplete(() =>
                       {
                           // Solo regresar si no fue interrumpido
                           if (!wasInterrupted && isAttacking)
                           {
                               // Animación de regreso
                               Sequence returnSequence = DOTween.Sequence();
                               returnSequence.Append(headbuttTransform.DOLocalMove(originalHeadbuttPosition, headbuttDuration)
                                           .SetEase(headbuttCurve))
                                           .Join(headbuttTransform.DOLocalRotate(Vector3.zero, headbuttDuration)
                                           .SetEase(headbuttCurve));
                           }
                       });

        headbuttTween = headbuttSequence;
    }

    private void StopHeadbuttAnimation()
    {
        if (headbuttTween != null && headbuttTween.IsActive())
        {
            headbuttTween.Kill();
        }

        // Regresar inmediatamente a la posición y rotación original
        if (headbuttTransform != null)
        {
            headbuttTransform.localPosition = originalHeadbuttPosition;
            headbuttTransform.localEulerAngles = Vector3.zero; // Resetear rotación
        }
    }


    // Animation Event: Activar detección
    public void ActivateDetection()
    {
        if (wasInterrupted) return;

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
        if (wasInterrupted) return;

        isAttacking = false;
        detectionActive = false;
        hasAppliedDamage = false;

        // Detener animación de cabezaso
        if (useHeadbuttAnimation)
        {
            StopHeadbuttAnimation();
        }

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

    public void ResetFirstAttack()
    {
        hasFirstAttack = false;
    }

    private void FixedUpdate()
    {
        if (isAttacking && !hasAppliedDamage && !wasInterrupted)
        {
            if (!hasFirstAttack || Time.time - attackStartTime < 0.1f)
            {
                DetectTargets();
            }
        }

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
                HealthManager healthManager = target.GetComponent<HealthManager>();
                if (healthManager != null && healthManager.CurrentHealth > 0)
                {
                    PlayerCombat playerCombat = target.GetComponentInChildren<PlayerCombat>();

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
                            Debug.Log("🎯 Jugador gana prioridad - Enemigo no aplica daño");
                            continue;
                        }
                    }

                    healthManager.TakeDamage(attackDamage);
                    hasAppliedDamage = true;

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

    public void InterruptAttack()
    {
        if (!isAttacking) return;

        if (attackPriority.hasHyperArmor)
        {
            Debug.Log("🛡️ Enemigo con HyperArmor - Ataque no interrumpido");
            return;
        }

        wasInterrupted = true;
        detectionActive = false;
        hasAppliedDamage = false;

        // Detener animación de cabezaso
        if (useHeadbuttAnimation)
        {
            StopHeadbuttAnimation();
        }

        if (animator != null)
        {
            animator.SetBool(attackBoolName, false);
        }

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

        // Detener animación de cabezaso
        if (useHeadbuttAnimation)
        {
            StopHeadbuttAnimation();
        }

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

    private void OnDestroy()
    {
        // Limpiar tweens cuando se destruya el objeto
        if (headbuttTween != null && headbuttTween.IsActive())
        {
            headbuttTween.Kill();
        }
    }

    private void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = detectionActive ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        // Dibujar trayectoria del cabezaso en el gizmo
        if (useHeadbuttAnimation && headbuttTransform != null)
        {
            DrawHeadbuttTrajectory();
        }
    }

    private void DrawHeadbuttTrajectory()
    {
        if (spriteRenderer == null) return;

        // Determinar dirección basada en el flip del sprite
        Vector3 horizontalDirection = spriteRenderer.flipX ? Vector3.left : Vector3.right;

        // Calcular puntos de la curva (los mismos que en la animación)
        Vector3 targetPosition = originalHeadbuttPosition + (horizontalDirection * headbuttDistance);
        Vector3 controlPoint = originalHeadbuttPosition +
                              (horizontalDirection * (headbuttDistance * 0.5f)) +
                              (Vector3.up * headbuttHeight);

        // Convertir a posición mundial para el gizmo
        Vector3 worldStart = headbuttTransform.parent.TransformPoint(originalHeadbuttPosition);
        Vector3 worldControl = headbuttTransform.parent.TransformPoint(controlPoint);
        Vector3 worldEnd = headbuttTransform.parent.TransformPoint(targetPosition);

        // Dibujar curva Bezier aproximada
        Gizmos.color = Color.cyan;
        int segments = 20;
        Vector3 previousPoint = worldStart;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;

            // Fórmula de curva cuadrática Bezier
            Vector3 point = Mathf.Pow(1 - t, 2) * worldStart +
                           2 * (1 - t) * t * worldControl +
                           Mathf.Pow(t, 2) * worldEnd;

            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        // Dibujar puntos de control
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(worldStart, 0.1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(worldControl, 0.08f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(worldEnd, 0.1f);

        // Dibujar líneas de guía
        Gizmos.color = Color.gray;
        Gizmos.DrawLine(worldStart, worldControl);
        Gizmos.DrawLine(worldControl, worldEnd);
    }
}